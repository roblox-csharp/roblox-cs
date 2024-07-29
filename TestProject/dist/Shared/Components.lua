local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

CS.namespace("Components", @native function(namespace: CS.Namespace)
    namespace:class("ComponentRunner", @native function(namespace: CS.Namespace)
        local class = CS.classDef("ComponentRunner", namespace)
        
        function class.AttachTag(tag: string, attachComponent: Func<Instance, TComponent>): nil
            local attached = false
            local instances = game:GetService("CollectionService"):GetTagged(tag)
            game:GetService("CollectionService").TagAdded:Connect(function(tag)
                if attached then
                    return 
                end
                local instance = game:GetService("CollectionService"):GetTagged(tag)[1]
                class.Run(attachComponent(instance))
                attached = true
            end)
            for _, instance in instances do
                if attached then
                    continueend
                class.Run(attachComponent(instance))
                attached = true
            end
            return nil :: any
        end
        function class.Run(component: GameComponent): nil
            component:Start()
            local updateEvent = class.GetUpdateEvent(component)
            updateEvent:Connect(component.Update)
            return nil :: any
        end
        function class.GetUpdateEvent(component: GameComponent): ScriptSignal<double>
            if component.UpdateMethod == "RenderStepped" then
                return game:GetService("RunService").RenderStepped
            elseif component.UpdateMethod == "Heartbeat" then
                return game:GetService("RunService").Heartbeat
            else
                return game:GetService("RunService").Heartbeat
            end
            return nil :: any
        end
        
        return class
    end)
    namespace:class("GameComponent", @native function(namespace: CS.Namespace)
        local class = CS.classDef("GameComponent", namespace)
        
        function class.new()
            local mt = {}
            local self = CS.classInstance(class, mt, namespace)
            
            self.UpdateMethod = if game:GetService("RunService"):IsClient() then "RenderStepped" else "Heartbeat"
            
            return self
        end
        
        return class
    end)
    namespace:class("GameComponent", @native function(namespace: CS.Namespace)
        local class = CS.classDef("GameComponent", namespace, "Components.GameComponent")
        
        function class.new(instance: TInstance)
            local mt = {}
            local self = CS.classInstance(class, mt, namespace)
            
            
            self.Instance = instance
            
            return self
        end
        
        return class
    end)
end)

return {}