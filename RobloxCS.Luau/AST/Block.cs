namespace RobloxCS.Luau;

public class Block : Statement
{
    public Block(List<Statement> statements)
    {
        Statements = statements;
        AddChildren(Statements);
    }

    public List<Statement> Statements { get; }

    public override void Render(LuauWriter luau) => luau.WriteNodes(Statements);
}