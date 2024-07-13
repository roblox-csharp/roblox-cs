local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class
            
            function class.Main()
                local config = namespace["$getMember"](namespace, "Config").new()
                config.DoSomeCoolStuff = true
                config.AnAwesomeProgrammer = "CharSiewGuy"
                print(`the awesomest programmer: {config.AnAwesomeProgrammer}`)
            end
            
            if namespace == nil then
                class.Main()
            else
                namespace["$onLoaded"](namespace, class.Main)
            end
            return class
        end)
        namespace:class("Config", function(namespace)
            local class = {}
            class.__index = class
            
            function class.new()
                local self = setmetatable({}, class)
                
                self.CoolNumber = 69
                
                
                return self
            end
            
            return setmetatable({}, class)
        end)
    end)
end)