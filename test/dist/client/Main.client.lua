package.path = "C:/Users/Riley Peel/Dev/c#/roblox-cs/RobloxRuntime/?.lua;" .. package.path
local CS = require("RuntimeLib")

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class

            function class.Main()
                local rect = namespace["$getMember"](namespace, "Rectangle").new(4, 3)
                CS.getAssemblyType("Console").WriteLine(CS.getAssemblyType("Math").Pow(4, 2))
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

                function self.Area()
                    return self.Width * self.Height
                end

                return self
            end

            return setmetatable({}, class)
        end)
    end)
end)