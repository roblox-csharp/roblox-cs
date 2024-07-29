using Roblox;

foreach (var instance in Services.CollectionService.GetTagged("Lava")) {
    if (instance is BasePart part)
    {
        part.Touched.Connect(part =>
            part.Parent?
                .FindFirstChildOfClass<Humanoid>()?
                .TakeDamage(100)
        );
    }
}