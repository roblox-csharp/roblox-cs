package.path = "C:/Users/Riley Peel/Dev/c#/roblox-cs/RobloxRuntime/?.lua;" .. package.path
local CS = require("RuntimeLib")

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class

            function class.Main()
                Console.Print("hello world")
            end

            if namespace == nil then
                class.Main()
            else
                namespace["$onLoaded"](namespace, class.Main)
            end
            return class
        end)
        namespace:class("Abc", function(namespace)
            local class = {}
            class.__index = class

            function class.new(blah)
                local self = setmetatable({}, class)
                self._xyz = 5
                self._blah = blah
                return self
            end

            return setmetatable({}, class)
        end)
    end)
end)