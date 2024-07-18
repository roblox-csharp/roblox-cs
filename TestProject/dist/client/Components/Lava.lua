local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("LavaComponent", function(namespace)
            local class = {}
            class.__index = class
            
            function class.new(instance)
                local self = setmetatable({}, class)
                
                self.Instance = instance
                
                function self.Start()
                    self.Instance.Touched:Connect(function(hit)
                        local model = hit:FindFirstAncestorOfClass("Model")
                        local humanoid = if model == nil then nil else model:FindFirstChildOfClass("Humanoid")
                        if humanoid == nil then return end
                        humanoid:TakeDamage(10)
                    end)
                end
                function self.Update(dt)
                end
                function self.Destroy()
                end
                
                return self
            end
            
            return setmetatable({}, class)
        end)
    end)
end)

return {}