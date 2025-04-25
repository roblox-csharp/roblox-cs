namespace RobloxCS.Luau;

public class For : Statement
{
    public List<IdentifierName> Names { get; }
    public Expression Iterable { get; }
    public Statement Body { get; }

    public For(List<IdentifierName> names, Expression iterable, Statement body)
    {
        Names = names;
        Iterable = iterable;
        Body = body;
        AddChildren(Names);
        AddChild(Iterable);
        AddChild(Body);
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write("for ");
        luau.WriteNodesCommaSeparated(Names);

        luau.Write(" in ");
        Iterable.Render(luau);
        luau.WriteLine(" do");
        luau.PushIndent();

        Body.Render(luau);
        luau.PopIndent();
        luau.WriteLine("end");
    }
}