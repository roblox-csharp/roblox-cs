local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

-- using ComponentRunner;
require(game:GetService("ReplicatedStorage")["C#"]["Components"])

-- using LavaComponent;
require(game:GetService("Players").LocalPlayer["PlayerScripts"]["C#"]["Components"]["Lava"])

CS.namespace("TestGame", function(namespace: CS.Namespace)
    namespace:namespace("Client", function(namespace: CS.Namespace)
        namespace:class("Game", function(namespace: CS.Namespace)
            local class = CS.classDef("Game", namespace)
            
            function class.Main(): nil
                CS.getAssemblyType("Components").ComponentRunner.AttachTag("Lava", function(instance)
                    return namespace["$getMember"](namespace, "LavaComponent").new(instance)
                end)
                local function test(): nil
                    print("[TestProject/Client/Main.client.cs:13:17]:", "abc")
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