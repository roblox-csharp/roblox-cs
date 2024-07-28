local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

CS.namespace("TestGame", function(namespace: CS.Namespace)
    namespace:namespace("Client", function(namespace: CS.Namespace)
        namespace:class("Game", function(namespace: CS.Namespace)
            local class = CS.classDef("Game", namespace)
            
            function class.Main(): nil
                @native
                local function test(): nil
                    print("[TestProject/Client/Main.client.cs:12:17]:", "abc")
                    return nil :: any
                end
                test()
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