--!strict
--!native

local CS = {}
local assemblyGlobal = {}

local function chainIndex(location: table, ...: table): () -> any
    local names = {...}
    return function(t, k)
        for _, name in names do
            local tbl = location[name]
            local v = tbl[k]
            if v ~= nil then
                return v
            end
        end
    end
end

local function createArithmeticOperators(self, mt, fieldName): table
    -- TODO: bitwise ops (if necessary) (or possible)
    local function getNumericValue(value: table | number): number
        return if typeof(value) == "table" and value.__isEnumMember then value[fieldName] else value
    end

    function mt:__add(other)
        return self[fieldName] + getNumericValue(other)
    end
    function mt:__sub(other)
        return self[fieldName] - getNumericValue(other)
    end
    function mt:__mul(other)
        return self[fieldName] * getNumericValue(other)
    end
    function mt:__div(other)
        return self[fieldName] / getNumericValue(other)
    end
    function mt:__idiv(other)
        return self[fieldName] // getNumericValue(other)
    end
    function mt:__mod(other)
        return self[fieldName] % getNumericValue(other)
    end
    function mt:__pow(other)
        return self[fieldName] ^ getNumericValue(other)
    end
    function mt:__unm()
        return -self[fieldName]
    end
    return mt
end

export type Class = table;
export type Namespace = {
    name: string;
    parent: Namespace?;
    members: { Namespace | Class };
    class: (self: Namespace, name: string, create: (self: Namespace) -> Class) -> nil;
}

local CSNamespace = {} do
    @native
    function CSNamespace.new(name, parent)
        local self = {}
        self.name = name
        self.parent = parent
        self.members = {}
        self["$loadCallbacks"] = {}
        if self.parent ~= nil then
            self = setmetatable(self, { __index = self.parent })
        end
        return setmetatable(self, CSNamespace)
    end

    @native
    function CSNamespace:__index(index)
        return self.members[index] or CSNamespace[index]
    end

    @native
    function CSNamespace:__newindex(index, value)
        self.members[index] = value
    end

    @native
    function CSNamespace:__tostring(index)
        return self.name
    end

    CSNamespace["$getMember"] = @native function(self, name)
        return self.members[name]
    end

    CSNamespace["$onLoaded"] = @native function(self, callback)
        table.insert(self["$loadCallbacks"], callback)
    end

    @native
    function CSNamespace:class(name, create)
        CS.class(name, create, self)
    end

    @native
    function CSNamespace:namespace(name, registerMembers)
        CS.namespace(name, registerMembers, self.members, self)
    end
end

@native
function CS.classInstance(class: Class, mt: table, namespace: Namespace?)
    local instance = {}
    instance["$className"] = class.__name

    @native
    local function getSuperclass()
        if class.__superclass == nil then return end
        if class.__superclass:match(".") == nil then
            return assemblyGlobal[class.__superclass]
        end

        local pieces = class.__superclass:split(".");
        local result = assemblyGlobal
        for _, piece in pieces do
            result = result[piece] or result
        end
        return result
    end

    @native
    function mt.__tostring()
        return class.__name
    end

    instance["$base"] = @native function(...)
        if instance["$superclass"] ~= nil then return end
        local Superclass = getSuperclass()
        local superclassInstance = Superclass.new(...)
        instance["$superclass"] = superclassInstance
        mt.__index = superclassInstance
    end

    return setmetatable(instance, mt)
end

@native
function CS.classDef(name: string, namespace: Namespace?, superclass: string?, ...: string)
    local mt = {}
    mt.__index = chainIndex(if namespace ~= nil then namespace else assemblyGlobal, ...)

    @native
    function mt.__tostring()
        return name
    end

    local class = {}
    class.__name = name
    class.__superclass = superclass
    return setmetatable(class, mt)
end

@native
function CS.class(name: string, create: (namespace: Namespace?) -> table, namespace: Namespace?)
    local location = if namespace ~= nil then namespace.members else assemblyGlobal
    local class = create(namespace)
    location[name] = class
end

@native
function CS.namespace(name: string, registerMembers: () -> nil, location: table?): Namespace
    local parent = location
    if location == nil then
        location = assemblyGlobal
    end

    local namespaceDefinition = location[name] or CSNamespace.new(name, parent)
    registerMembers(namespaceDefinition)
    location[name] = namespaceDefinition

    for _, callback in namespaceDefinition["$loadCallbacks"] do
        callback()
    end

    return namespaceDefinition
end

@native
function CS.enum(name: string, definition: table, location: table): table
    if location == nil then
        location = assemblyGlobal
    end
    definition.__name = name

    @native
    function definition:__index(index: string | number): table
        if index == "__name" then return name end
        local member = {
            name = index,
            value = definition[index],
            __isEnumMember = true
        }

        return setmetatable(member, createArithmeticOperators(member, {
            __eq = @native function(self, other)
                return typeof(other) == "table" and other.__isEnumMember and self.value == other.value
            end,
            __tostring = @native function(self)
                return self.name
            end
        }, "value"))
    end

    @native
    function definition:__eq(other: table): boolean
        return self.__name == other.__name
    end

    @native
    function definition:__tostring(): string
        return self.__name
    end

    location[name] = location[name] or table.freeze(setmetatable({}, definition))
    return location[name]
end

@native
function CS.is(object: any, class: Class | string): boolean
    if typeof(class) == "table" and type(class.__name) == "string" then
		return typeof(object) == "table" and type(object["className"]) == "string" and object["className"] == class.__name
	end

	-- metatable check
	if typeof(object) == "table" then
		obj = getmetatable(obj)
		while object ~= nil do
			if object == class then
				return true
			end
			local mt = getmetatable(object)
			if mt then
				object = mt.__index
			else
				object = nil
			end
		end
	end

    if typeof(class) == "string" then
        return if typeof(object) == "Instance" then object:IsA(class) else typeof(object) == class
    end

    return false
end

@native
function CS.getAssemblyType(name)
    local env
    if getfenv == nil then
        env = _ENV
    else
        env = getfenv()
    end
    return assemblyGlobal[name] or env[name]
end

CS.class("Exception", @native function()
    local class = CS.classDef("Exception")

    @native
    function class.new(message: string?): Exception
        local mt = {}
        local self = CS.classInstance(class, mt) :: Exception

        if message == nil then message = "An error occurred" end
        self.Message = message

        @native
        function mt.__tostring(): string
            return `{self["$className"]}: {self.Message}`
        end

        @native
        function self.Throw(withinTryBlock: boolean): nil
            error(if withinTryBlock then self else tostring(self))
            return nil
        end

        return self
    end

    return class
end)

export type Exception = {
    Message: string;
    Throw: () -> nil;
}

type CatchBlock = {
    exceptionClass: string;
    block: (ex: Exception?, rethrow: () -> nil) -> nil
}

@native
function CS.try(block: () -> nil, finallyBlock: () -> nil, catchBlocks: { CatchBlock })
    local success: boolean, ex: Exception | string | nil = pcall(block)
    if not success then
        if typeof(ex) == "string" then
            ex = CS.getAssemblyType("Exception").new(ex, false)
        end
        for _, catchBlock in catchBlocks do
            if catchBlock.exceptionClass ~= nil and catchBlock.exceptionClass ~= ex["$className"] then continue end
            catchBlock.block(ex :: Exception, (ex :: Exception).Throw)
        end
    end
    if finallyBlock ~= nil then
        finallyBlock()
    end
end

return CS