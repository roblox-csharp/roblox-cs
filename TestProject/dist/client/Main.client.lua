local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class

            function class.Main()
                local player = game:GetService("Players").LocalPlayer
                local character = if player.Character == nil then player.CharacterAdded:Wait() else player.Character
                local part = CS.getAssemblyType("Instance").new("Part", character)
                part.Anchored = true
                part.CanCollide = false
                part.Position = character.PrimaryPart.Position
                local runtime = game:GetService("RunService")
                local goalColor = CS.getAssemblyType("Color3").fromRGB(255, 0, 0)
                local alpha = 0.005
                runtime.RenderStepped:Connect(function(dt)
                    part.Color = part.Color:Lerp(goalColor, alpha)
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