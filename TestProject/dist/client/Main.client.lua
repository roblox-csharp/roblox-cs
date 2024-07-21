local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = {}
            class.__index = class
            
            function class.Main()
                local vector = namespace["$getMember"](namespace, "Vector4").new()
                local x = bit32.band(5, 2)
                print("[TestProject/Client/Main.client.cs:12:13]:", vector)
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
            
            function class.new()
                local self = setmetatable({}, class)
                self.mt = {}
                
                self.X = 0
                self.Y = 0
                self.Z = 0
                self.W = 0
                
                function self.mt.__tostring()
                    return `{self.X}, {self.Y}, {self.Z}, {self.W}`
                end
                
                return setmetatable(self, self.mt)
            end
            
            return setmetatable({}, class)
        end)
    end)
end)