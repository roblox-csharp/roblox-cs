local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

CS.namespace("TestGame", @native function(namespace: CS.Namespace)
    namespace:namespace("Client", @native function(namespace: CS.Namespace)
        CS.enum("Shape", {
            Circle = 1, 
            Quadrilateral = 2, 
            Triangle = 3
        }, namespace)
        namespace:class("Game", @native function(namespace: CS.Namespace)
            local class = CS.classDef("Game", namespace)
            
            function class.Main(): nil
                print("[TestProject/Client/Main.client.cs:14:13]:", namespace["$getMember"](namespace, "Shape").Quadrilateral + 5)
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