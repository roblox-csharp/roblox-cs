local CS = require(game:GetService("ReplicatedStorage").rbxcs_include.RuntimeLib)

CS.namespace("TestGame", function(namespace)
    namespace:namespace("Client", function(namespace)
        namespace:class("Game", function(namespace)
            local class = CS.classDef(namespace)
            
            function class.Main()
                local square = namespace["$getMember"](namespace, "Square").new(5)
                print("[TestProject/Client/Main.client.cs:11:13]:", square:GetArea())
            end
            
            if namespace == nil then
                class.Main()
            else
                namespace["$onLoaded"](namespace, class.Main)
            end
            return class
        end)
        namespace:class("Square", function(namespace)
            local class = CS.classDef(namespace, "Rectangle")
            
            function class.new(size)
                local mt = {}
                local self = CS.classInstance(class, mt, namespace)
                
                self["$base"](size, size)
                
                
                return self
            end
            
            return class
        end)
        namespace:class("Rectangle", function(namespace)
            local class = CS.classDef(namespace)
            
            function class.new(height, width)
                local mt = {}
                local self = CS.classInstance(class, mt, namespace)
                
                self.Height = 0
                self.Width = 0
                
                self.Height = height
                self.Width = width
                
                function self.GetArea()
                    return self.Height * self.Width
                end
                
                return self
            end
            
            return class
        end)
    end)
end)