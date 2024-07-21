local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("Components", function(namespace)
    namespace:class("ComponentRunner", function(namespace)
        local class = {}
        class.__index = class
        
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
        local class = {}
        class.__index = class
        
        function class.new()
            local self = setmetatable({}, class)
            self.mt = {}
            
            self.UpdateMethod = if game:GetService("RunService"):IsClient() then "RenderStepped" else "Heartbeat"
            
            return setmetatable(self, self.mt)
        end
        
        return setmetatable({}, class)
    end)
    namespace:class("GameComponent", function(namespace)
        local class = {}
        class.__index = class
        
        function class.new(instance)
            local self = setmetatable({}, class)
            self.mt = {}
            
            self.Instance = instance
            
            
            return setmetatable(self, self.mt)
        end
        
        return setmetatable({}, class)
    end)
end)

return {}