local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

-- Imports for "ComponentRunner"
require(game:GetService("ReplicatedStorage")["C#"]["Components"])

-- Imports for "LavaComponent"
require(game:GetService("Players").LocalPlayer["PlayerScripts"]["C#"]["Components"]["Lava"])

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class
            
            function class.Main()
                local x = 5
                print("[TestProject/Client/Main.client.cs:13:13]:", x)
                task.wait(2)
                print("[TestProject/Client/Main.client.cs:15:13]:", math.pow(x, 2))
                print("[TestProject/Client/Main.client.cs:16:13]:", game:GetService("Players"))
                CS.getAssemblyType("Components").ComponentRunner.AttachTag("Lava", function(instance)
                    return namespace["$getMember"](namespace, "LavaComponent").new(instance)
                end)
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