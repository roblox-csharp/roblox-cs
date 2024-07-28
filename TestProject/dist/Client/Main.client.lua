local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

CS.namespace("TestGame", function(namespace: CS.Namespace)
    namespace:namespace("Client", function(namespace: CS.Namespace)
        namespace:class("Game", function(namespace: CS.Namespace)
            local class = CS.classDef("Game", namespace)
            
            @native
            function class.Main(): nil
                class.DoShit()
                return nil :: any
            end
            @native
            function class.DoShit(): any
                local name = "billy"
                repeat
                    local _fallthrough = false
                    if (name == "joanna") then
                        _fallthrough = true
                    end
                    if _fallthrough or (name == "milly") then
                        _fallthrough = true
                    end
                    if _fallthrough or (name == "mary" and name.Length == 4) then
                        local msg = "wtf"
                        print("[TestProject/Client/Main.client.cs:23:21]:", msg)
                        break
                    end
                    if (name == "bob") then
                        print("[TestProject/Client/Main.client.cs:26:21]:", "yay!")
                        break
                    end
                    local unknownName: string = name
                    print("[TestProject/Client/Main.client.cs:29:21]:", `who is {unknownName}?!`)
                until true
                return nil :: any
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