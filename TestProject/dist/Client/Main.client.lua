local CS = require(game:GetService("ReplicatedStorage")["rbxcs_include"]["RuntimeLib"])

for _, instance in game:GetService("CollectionService"):GetTagged("Lava") do
    if CS.is(instance, "BasePart") then
        local part = instance
        part.Touched:Connect(function(part)
            return if part.Parent == nil then nil else if Humanoid() == nil then nil else Humanoid():TakeDamage(100)
        end)
    end
end