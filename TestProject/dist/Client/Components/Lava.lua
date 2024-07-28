local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

CS.namespace("TestGame", function(namespace: CS.Namespace)
    namespace:namespace("Client", function(namespace: CS.Namespace)
        namespace:class("LavaComponent", function(namespace: CS.Namespace)
            local class = CS.classDef("LavaComponent", namespace, "Components.GameComponent")
            
            function class.new(instance: Part)
                local mt = {}
                local self = CS.classInstance(class, mt, namespace)
                
                self["$base"](instance)
                
                function self.Start(): nil
                    self.Instance.Touched:Connect(function(hit)
                        local model = hit:FindFirstAncestorOfClass("Model")
                        local humanoid = if model == nil then nil else model:FindFirstChildOfClass("Humanoid")
                        if humanoid == nil then return  end
                        humanoid:TakeDamage(10)
                    end)
                    return nil :: any
                end
                function self.Update(dt: number): nil
                    return nil :: any
                end
                function self.Destroy(): nil
                    return nil :: any
                end
                
                return self
            end
            
            return class
        end)
    end)
end)

return {}