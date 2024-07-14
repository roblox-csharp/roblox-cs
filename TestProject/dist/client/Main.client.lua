local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.class("Game", function(namespace)
    local class = {}
    class.__index = class
    
    function class.Main()
        local nums = {1, 2, 3}
        local i = 4
        print("[TestProject/Client/Main.client.cs:9:9]:", nums[i + 1])
    end
    
    if namespace == nil then
        class.Main()
    else
        namespace["$onLoaded"](namespace, class.Main)
    end
    return class
end)