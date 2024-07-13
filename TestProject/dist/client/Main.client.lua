local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class
            
            function class.Main()
                namespace["$getMember"](namespace, "ComponentRunner").AttachTag("Lava", function(instance)
                    return namespace["$getMember"](namespace, "LavaComponent").new(instance)end)
            end
            
            if namespace == nil then
                class.Main()
            else
                namespace["$onLoaded"](namespace, class.Main)
            end
            return class
        end)
        namespace:class("LavaComponent", function(namespace)
            local class = {}
            class.__index = class
            
            function class.new(instance)
                local self = setmetatable({}, class)
                
                print("lava component created")
                
                function self.Start()
                    print("lava component started")
                    CS.getAssemblyType("Instance").Touched:Connect(function(hit)
                        local model = hit:FindFirstAncestorOfClass("Model")
                        local humanoid = if model == nil then nil else model:FindFirstChildOfClass("Humanoid")
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
            
            function class.AttachTag(tag, attachComponent)
                local attached = false
                local instances = game:GetService("CollectionService"):GetTagged(tag)
                game:GetService("CollectionService").TagAdded:Connect(function(tag)
                    local instance = game:GetService("CollectionService"):GetTagged(tag)[0]
                    class.Run(attachComponent(instance))
                    attached = true
                end)
                for _, instance in instances do
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
                
                self.UpdateMethod = if game:GetService("RunService"):IsClient() then "RenderStepped" else "Heartbeat"
                
                return self
            end
            
            return setmetatable({}, class)
        end)
        namespace:class("GameComponent", function(namespace)
            local class = {}
            class.__index = class
            
            function class.new(instance)
                local self = setmetatable({}, class)
                
                class.Instance = instance
                
                
                return self
            end
            
            return setmetatable({}, class)
        end)
    end)
end)