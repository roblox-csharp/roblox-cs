local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("LavaComponent", function(namespace)
            local class = CS.classDef(namespace, "Components.GameComponent")
            
            function class.new(instance)
                local mt = {}
                local self = CS.classInstance(class, mt, namespace)
                
                self["$base"](instance)
                
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
            
            return class
        end)
    end)
end)

return {}