namespace RobloxCS.Luau
{
    public class Block : Statement
    {
        public List<Statement> Statements { get; }

        public Block(List<Statement> statements)
        {
            Statements = statements;
        }

        public override void Render(LuauWriter luau)
        {
            foreach (var statement in Statements)
            {
                statement.Render(luau);
            }
        }
    }
}
