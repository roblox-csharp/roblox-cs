local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("Components", function(namespace)
    namespace:class("ComponentRunner", function(namespace)
        local class = CS.classDef(namespace)
        
        function class.AttachTag(tag, attachComponent)
            local attached = false
            local instances = game:GetService("CollectionService"):GetTagged(tag)
            game:GetService("CollectionService").TagAdded:Connect(function(tag)
                if attached then return end
                local instance = game:GetService("CollectionService"):GetTagged(tag)[1]
                class.Run(attachComponent(instance))
                attached = true
            end)
            for _, instance in instances do
                if attached then continue end
                class.Run(attachComponent(instance))
                attached = true
            end
        end
        function class.Run(component)
            component:Start()
            local updateEvent = class.GetUpdateEvent(component)
            updateEvent:Connect(component.Update)
        end
        function class.GetUpdateEvent(component)
            if component.UpdateMethod == "RenderStepped" then
                return game:GetService("RunService").RenderStepped
            elseif component.UpdateMethod == "Heartbeat" then
                return game:GetService("RunService").Heartbeat
            else
                return game:GetService("RunService").Heartbeat
            end
        end
        
        return class
    end)
    namespace:class("GameComponent", function(namespace)
        local class = CS.classDef(namespace)
        
        function class.new()
            local mt = {}
            local self = CS.classInstance(class, mt, namespace)
            
            self.UpdateMethod = if game:GetService("RunService"):IsClient() then "RenderStepped" else "Heartbeat"
            
            return self
        end
        
        return class
    end)
    namespace:class("GameComponent", function(namespace)
        local class = CS.classDef(namespace, "GameComponent")
        
        function class.new(instance)
            local mt = {}
            local self = CS.classInstance(class, mt, namespace)
            
            self.Instance = instance
            
            
            return self
        end
        
        return class
    end)
end)

return {}