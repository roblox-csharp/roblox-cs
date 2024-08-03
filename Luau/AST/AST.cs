namespace RobloxCS.Luau
{
    public class AST(List<Statement> statements) : Node
    {
        public List<Statement> Statements { get; } = statements;

        public override void Render(LuauWriter luau)
        {
            foreach (var statement in Statements)
            {
                statement.Render(luau);
            }
        }
    }
}
