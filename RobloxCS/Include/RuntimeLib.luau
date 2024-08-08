--!native
--!strict

local CS = {}
CS.globalNamespace = {} :: { [string]: any }

function CS.defineGlobal<T>(name: string, value: T)
    CS.globalNamespace[name] = value
end

function CS.getGlobal<T>(name: string): T -- TODO: if getting namespace call getGlobal<Namespace>, if class then getGlobal<Class>, etc
    return CS.globalNamespace[name]
end

export type Class = {
    __name: string; -- prob temporary
} -- stub

function CS.is(object: any, class: Class | string): boolean
    if typeof(class) == "table" and type(class.__name) == "string" then
		return typeof(object) == "table" and type(object["className"]) == "string" and object["className"] == class.__name
	end

	-- metatable check
	if typeof(object) == "table" then
		object = getmetatable(object)

		while object ~= nil do
			if object == class then
				return true
			end

			local mt = getmetatable(object)
			object = if mt then mt.__index else nil
		end
	end

    
    return if typeof(class) == "string" then
        if typeof(object) == "Instance" then
            object:IsA(class)
        else
            typeof(object) == class
    else
        false
end

-- Exception class
export type Exception = {
    Message: string;
    Throw: () -> nil;
}
--[[
if message == nil then message = "An error occurred" end
self.Message = message

@native
function mt.__tostring(): string
    return `{self["$className"]}: {self.Message}`
end

@native
function self.Throw(withinTryBlock: boolean): nil
    error(if withinTryBlock then self else tostring(self))
    return
end
]]

type CatchBlock = {
    exceptionClass: string;
    block: (ex: Exception?, rethrow: () -> nil) -> nil
}

function CS.try(block: () -> nil, finallyBlock: () -> nil, catchBlocks: { CatchBlock }): nil
    local success: boolean, ex: Exception | string | nil = pcall(block)
    if not success then
        if typeof(ex) == "string" then
            ex = CS.getGlobal("Exception").new(ex, false)
        end
        for _, catchBlock in catchBlocks do
            if catchBlock.exceptionClass ~= nil and catchBlock.exceptionClass ~= (ex :: Exception)["$className"] then continue end
            catchBlock.block(ex :: Exception, (ex :: Exception).Throw)
        end
    end
    if finallyBlock ~= nil then
        finallyBlock()
    end
    return
end

return CS