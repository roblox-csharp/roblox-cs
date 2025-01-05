namespace RobloxCS.Luau
{
    public sealed class Assignment : Expression
    {
        public AssignmentTarget Target { get; }
        public Expression Value;

        public Assignment(AssignmentTarget target, Expression value)
        {
            Target = target;
            Value = value;
            AddChildren([Target, Value]);
        }

        public override void Render(LuauWriter luau)
        {
            Node value = Value;
            luau.WriteDescendantStatements(ref value);
            Value = (Expression)value;
            luau.WriteAssignment(Target, Value);
        }
    }
}