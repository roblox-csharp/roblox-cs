local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

for _, instance in game:GetService("CollectionService"):GetTagged("Lava") do
    print("[TestProject/Server/BasicLava.server.cs:5:5]:", CS.is(instance, "BasePart"))
    if CS.is(instance, "BasePart") then
        local part = instance
        part.Touched:Connect(function(part)
            return if part.Parent == nil then nil else if part.Parent:FindFirstChildOfClass("Humanoid") == nil then nil else part.Parent:FindFirstChildOfClass("Humanoid"):TakeDamage(100)
        end)
    end
end