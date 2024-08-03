namespace RobloxCS.Luau
{
    public class Assignment(Name name, Expression value) : Statement
    {
        public Name Name { get; } = name;
        public Expression Value { get; } = value;

        public override void Render(LuauWriter luau)
        {
            Name.Render(luau);
            luau.Write(" = ");
            Value.Render(luau);
        }
    }
}