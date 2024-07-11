local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class

            function class.Main()
                local player = game:GetService("Players").LocalPlayer
                local character = player.Character ?? player.CharacterAdded.Wait()
                local part = CS.getAssemblyType("Instance").new("Part", character)
                part.Anchored = true
                part.Position = CS.getAssemblyType("Vector3").new(0, 1, 0)
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