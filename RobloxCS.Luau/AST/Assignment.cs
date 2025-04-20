namespace RobloxCS.Luau;

public sealed class Assignment : Statement
{
    public AssignmentTarget Target { get; }
    public Expression Value { get; }

    public Assignment(AssignmentTarget target, Expression value)
    {
        Target = target;
        Value = value;
        AddChildren([Target, Value]);
    }

    public override void Render(LuauWriter luau) => luau.WriteAssignment(Target, Value);
}