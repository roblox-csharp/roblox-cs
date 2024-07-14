local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.class("Game", function(namespace)
    local class = {}
    class.__index = class
    
    function class.Main()
        warn("[TestProject/Client/Main.client.cs:7:9]:", "hello baby")
    end
    
    if namespace == nil then
        class.Main()
    else
        namespace["$onLoaded"](namespace, class.Main)
    end
    return class
end)