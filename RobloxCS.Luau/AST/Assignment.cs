namespace RobloxCS.Luau
{
    public sealed class Assignment : Expression
    {
        public Expression Expression { get; }
        public Expression Value;

        public Assignment(Expression name, Expression value)
        {
            Expression = name;
            Value = value;
            AddChildren([Expression, Value]);
        }

        public override void Render(LuauWriter luau)
        {
            Node value = Value;
            luau.WriteDescendantStatements(ref value);
            Value = (Expression)value;
            luau.WriteAssignment(Expression, Value);
        }
    }
}