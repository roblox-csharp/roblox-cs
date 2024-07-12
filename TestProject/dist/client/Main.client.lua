local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("Test", function(namespace)
    namespace:class("HelloWorld", function(namespace)
        local class = {}
        class.__index = class
        
        function class.new()
            local self = setmetatable({}, class)
            
            
            
            return self
        end
        function class.Main()
        end
        
        return setmetatable({}, class)
    end)
end)