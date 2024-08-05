namespace RobloxCS.Luau
{
    public class BuiltinAttribute : BaseAttribute
    {
        public Name Name { get; }

        public BuiltinAttribute(Name name)
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