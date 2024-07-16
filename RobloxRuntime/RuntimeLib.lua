local CS = {}
local assemblyGlobal = {}

local fempty = function() end

local split = string.split
if split == nil then
	split = function(inputString, separator)
		if sep == nil then
			sep = "%s"
		end
		local t = {}
		for str in string.gmatch(inputString, "([^"..sep.."]+)") do
			table.insert(t, str)
		end
		return t
	end
end

do
	local Math = {} do
		Math.PI = math.pi
		Math.Pow = math.pow
	end

	assemblyGlobal.Math = math;
end

local CSNamespace = {} do
	CSNamespace.__index = CSNamespace

	function CSNamespace.new(name, parent)
		local self = {}
		self.name = name
		self.parent = parent
		self.members = {}
		self["$loadCallbacks"] = {}
		if self.parent ~= nil then
			self = setmetatable(self, self.parent)
		end
		return setmetatable(self, CSNamespace)
	end

	function CSNamespace:__index(index)
		return self.members[index] or CSNamespace[index]
	end

	CSNamespace["$getMember"] = function(self, name)
		return self.members[name]
	end

	CSNamespace["$onLoaded"] = function(self, callback)
		table.insert(self["$loadCallbacks"], callback)
	end

	function CSNamespace:class(name, create)
		CS.class(name, create, self)
	end

	function CSNamespace:namespace(name, registerMembers)
		CS.namespace(name, registerMembers, self.members, self)
	end
end

function CS.class(name, create, namespace)
	local location
	if namespace ~= nil then
		location = namespace.members
	else
		location = assemblyGlobal
	end

	local class = create(namespace)
	location[name] = class
end

function CS.namespace(name, registerMembers, location, parent)
	if location == nil then
		location = assemblyGlobal
	end

	local namespaceDefinition = assemblyGlobal[name] or CSNamespace.new(name, parent)
	registerMembers(namespaceDefinition)
	for _, callback in pairs(namespaceDefinition["$loadCallbacks"]) do
		callback()
	end

	location[name] = namespaceDefinition
	return namespaceDefinition
end


function CS.getAssemblyType(name)
	local env
	if getfenv == nil then
		env = _ENV
	else
		env = getfenv()
	end
	return assemblyGlobal[name] or env[name]
end

return CS