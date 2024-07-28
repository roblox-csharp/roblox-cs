local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

CS.namespace("TestGame", @native function(namespace: CS.Namespace)
    namespace:namespace("Client", @native function(namespace: CS.Namespace)
        namespace:class("Game", @native function(namespace: CS.Namespace)
            local class = CS.classDef("Game", namespace)
            
            @native
            function class.Main(): nil
                do
                    local i: number = 0
                    while i < 10 do
                        print("[TestProject/Client/Main.client.cs:12:17]:", i)
                        i += 1
                    end
                end
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