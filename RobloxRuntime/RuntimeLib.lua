local CS = {}
local assemblyGlobal = {}

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

local function insertRuntimeEnv(f)
	local env
	if getfenv == nil then
		env = _ENV
	else
		env = getfenv(f)
	end
	env.Console = {} do
		function env.Console.Print(...)
			print(...)
		end
		function env.Console.Warn(message)
			warn(message)
		end
		function env.Console.Error(message, level)
			error(message, level)
		end
	end
	return f
end

local CSNamespace = {} do
	CSNamespace.__index = CSNamespace

	function CSNamespace.new(name)
		local self = {}
		self.name = name
		self.members = {}
		self["$loadCallbacks"] = {}
		return setmetatable(self, CSNamespace)
	end

	function CSNamespace:__index(index)
		return self.members[index] or CSNamespace[index]
	end

	CSNamespace["$onLoaded"] = function(self, callback)
		table.insert(self["$loadCallbacks"], callback)
	end

	function CSNamespace:class(name, create)
		local class = insertRuntimeEnv(create)(self)
		self.members[name] = class
	end
	
	function CSNamespace:namespace(name, registerMembers)
		CS.namespace(name, registerMembers, self.members)
	end
end

function CS.namespace(name, registerMembers, location)
	if location == nil then
		location = assemblyGlobal
	end

	local namespaceDefinition = CSNamespace.new(name)
	insertRuntimeEnv(registerMembers)(namespaceDefinition)
	for _, callback in pairs(namespaceDefinition["$loadCallbacks"]) do
		callback()
	end

	location[name] = namespaceDefinition
	return namespaceDefinition
end

function CS.getAssemblyType(name)
	return assemblyGlobal[name]
end

return CS