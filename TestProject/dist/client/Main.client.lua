package.path = "C:/Users/Riley Peel/Dev/C#/roblox-cs/RobloxRuntime/?.lua;" .. package.path
local CS = require("RuntimeLib")

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class
            
            function class.Main()
                local tuple = {1, 2, 3}
                print(tuple[1])
            end
            
            if namespace == nil then
                class.Main()
            else
                namespace["$onLoaded"](namespace, class.Main)
            end
            return class
        end)
    end)
end)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("TestBrah", function(namespace)
            local class = {}
            class.__index = class
            
            function class.HelloNiga()
                return 69
            end
            
            return class
        end)
    end)
end)