package.path = "C:/Users/Riley Peel/Dev/c#/roblox-cs/RobloxRuntime/?.lua;" .. package.path
local CS = require("RuntimeLib")

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class
            
            function class.Main()
                do
                    local i = 0
                    while true do
                        if not (i < 10) then break end
                        print(i)
                        i += 1
                    end
                end
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