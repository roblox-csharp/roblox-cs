namespace RobloxCS.Luau
{
    public class BuiltInAttribute : BaseAttribute
    {
        public Name Name { get; }

        public BuiltInAttribute(Name name)
        {
            Name = name;
            AddChild(name);
        }

        public override void Render(LuauWriter luau)
        {
            luau.Write('@');
            Name.Render(luau);
        }
    }
}