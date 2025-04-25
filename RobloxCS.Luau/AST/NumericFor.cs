namespace RobloxCS.Luau;

public class NumericFor : Statement
{
    public IdentifierName Name { get; }
    public Expression Minimum { get; }
    public Expression Maximum { get; }
    public Expression? IncrementBy { get; }
    public Statement Body { get; }

    public NumericFor(IdentifierName name, Expression minimum, Expression maximum, Expression? incrementBy, Statement body)
    {
        Name = name;
        Minimum = minimum;
        Maximum = maximum;
        IncrementBy = incrementBy;
        Body = body;

        AddChildren([Name, Minimum, Maximum]);
        if (IncrementBy != null) AddChild(IncrementBy);

        AddChild(Body);
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write("for ");
        Name.Render(luau);
        luau.Write(" = ");
        Minimum.Render(luau);
        luau.Write(", ");
        Maximum.Render(luau);
        if (IncrementBy != null)
        {
            luau.Write(", ");
            IncrementBy.Render(luau);
        }

        luau.WriteLine(" do");
        luau.PushIndent();

        Body.Render(luau);

        luau.PopIndent();
        luau.WriteLine("end");
    }
}