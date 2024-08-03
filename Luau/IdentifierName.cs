namespace RobloxCS.Luau
{
    public class IdentifierName(string name) : Name
    {
        public string Name { get; } = name;

        public override void Render(LuauWriter luau)
        {
            luau.Write(Name);
        }
    }
}