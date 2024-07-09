package.path = "C:/Users/Riley Peel/Dev/c#/roblox-cs/RobloxRuntime/?.lua;" .. package.path
local CS = require("RuntimeLib")

CS.getAssemblyType("RobloxRuntime")CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class

            function class.Main()
                local rect = namespace["$getMember"](namespace, "Rectangle").new(4, 3)
                CS.getAssemblyType("Console").Print(tostring(rect.Width))
            end

            if namespace == nil then
                class.Main()
            else
                namespace["$onLoaded"](namespace, class.Main)
            end
            return class
        end)
        namespace:class("Rectangle", function(namespace)
            local class = {}
            class.__index = class

            function class.new(width, height)
                local self = setmetatable({}, class)
                self.Width = width
                self.Height = height
                return self
            end

            return setmetatable({}, class)
        end)
    end)
end)