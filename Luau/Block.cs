namespace RobloxCS.Luau
{
    public class Block(List<Statement> statements) : Statement
    {
        public List<Statement> Statements { get; } = statements;

        public override void Render(LuauWriter luau)
        {
            luau.PushIndent();
            foreach (var statement in Statements)
            {
                statement.Render(luau);
            }
            luau.PopIndent();
        }
    }
}
