namespace RobloxCS.Luau
{
    public class BuiltinAttribute(Name name) : BaseAttribute
    {
        public Name Name { get; } = name;

        public override void Render(LuauWriter luau)
        {
            luau.Write('@');
            Name.Render(luau);
        }
    }
}