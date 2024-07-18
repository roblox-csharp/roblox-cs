local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class
            
            function class.Main()
                CS.getAssemblyType("GameComponent")local x
            end
            
            if namespace == nil then
                class.Main()
            else
                namespace["$onLoaded"](namespace, class.Main)
            end
            return class
        end)
    end)
end)