local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

CS.namespace("TestGame", @native function(namespace: CS.Namespace)
    namespace:namespace("Client", @native function(namespace: CS.Namespace)
        namespace:class("Game", @native function(namespace: CS.Namespace)
            local class = CS.classDef("Game", namespace)
            
            function class.Main(): nil
                print("[TestProject/Client/Main.client.cs:9:13]:", "rah")
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