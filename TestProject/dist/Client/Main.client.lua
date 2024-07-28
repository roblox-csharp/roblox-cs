local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

CS.namespace("TestGame", function(namespace: CS.Namespace)
    namespace:namespace("Client", function(namespace: CS.Namespace)
        namespace:class("Game", function(namespace: CS.Namespace)
            local class = CS.classDef("Game", namespace)
            
            @native
            function class.Main(): nil
                print("[TestProject/Client/Main.client.cs:10:13]:", (1 + 2) * 4)
                return nil :: any
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