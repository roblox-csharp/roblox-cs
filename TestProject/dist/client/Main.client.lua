local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class
            
            function class.Main()
                namespace["$getMember"](namespace, "ComponentRunner").Lava("Lava")
            end
            
            if namespace == nil then
                class.Main()
            else
                namespace["$onLoaded"](namespace, class.Main)
            end
            return class
        end)
        namespace:class("Lava", function(namespace)
            local class = {}
            class.__index = class
            
            function class.new()
                local self = setmetatable({}, class)
                
                function self.Start()
                    CS.getAssemblyType("Instance").Touched:Connect(function(hit)
                        local model = Modelhit:FindFirstAncestorOfClass("Model")
                        local humanoid = Humanoidif model == nil then nil else model:FindFirstChildOfClass("Humanoid")
                        if humanoid == nil then
                            return 
                        end
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
        namespace:class("ComponentRunner", function(namespace)
            local class = {}
            class.__index = class
            
            function class.AttachTag(tag)
                local instances = game:GetService("CollectionService"):GetTagged(tag)
                for _, instance in instances do
                    local component = CS.getAssemblyType("TComponent").new() as Instance not  1
                    
                    component.Instance = instance
                    CS.getAssemblyType("Run")(component)
                end
            end
            function class.Run(component)
                component:Start()
                local updateEvent = CS.getAssemblyType("GetUpdateEvent")(component)
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
                
                self.UpdateMethod = if game:GetService("RunService"):IsClient() then "RenderStepped" else "Heartbeat"
                function self.Start()
                end
                function self.Update(dt)
                end
                function self.Destroy()
                end
                
                return self
            end
            
            return setmetatable({}, class)
        end)
        namespace:class("GameComponent", function(namespace)
            local class = {}
            class.__index = class
            
            function class.new()
                local self = setmetatable({}, class)
                
                self.Instance = nil not  1
                
                
                return self
            end
            
            return setmetatable({}, class)
        end)
    end)
end)