local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class
            
            function class.Main()
                local s = utf8.char(16, 2, 24, 86)
                local n = tonumber("0xE16D")
                print("[TestProject/Client/Main.client.cs:14:13]:", s)
                print("[TestProject/Client/Main.client.cs:15:13]:", n)
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