local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.class("Game", function(namespace)
    local class = {}
    class.__index = class
    
    function class.Main()
        print("[TestProject/client/Main.client.cs:5:9]:", "hello" .. " " .. "world")
    end
    
    if namespace == nil then
        class.Main()
    else
        namespace["$onLoaded"](namespace, class.Main)
    end
    return class
end)