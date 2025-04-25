namespace RobloxCS.Luau;

public class AST : Node
{
    public List<Statement> Statements { get; }

    public AST(List<Statement> statements)
    {
        Statements = statements;
        AddChildren(Statements);
    }

    public override void Render(LuauWriter luau)
    {
        foreach (var statement in Statements) statement.Render(luau);

        luau.WriteReturn();
    }
}