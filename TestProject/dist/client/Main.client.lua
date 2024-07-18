local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class
            
            function class.Main()
                local vec1 = namespace["$getMember"](namespace, "Vector4").new(3, 0, 6, 0)
                local vec2 = namespace["$getMember"](namespace, "Vector4").new(0, 2, 0, 9)
            end
            
            if namespace == nil then
                class.Main()
            else
                namespace["$onLoaded"](namespace, class.Main)
            end
            return class
        end)
        namespace:class("Vector4", function(namespace)
            local class = {}
            class.__index = class
            
            function class.new(x)
                local self = setmetatable({}, class)
                
                
                
                return self
            end
            
            return setmetatable({}, class)
        end)
    end)
end)