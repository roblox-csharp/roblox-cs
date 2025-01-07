namespace RobloxCS.Luau
{
    public class Block : Statement
    {
        public List<Statement> Statements { get; }

        public Block(List<Statement> statements)
        {
            Statements = statements;
            AddChildren(Statements);
        }

        public override void Render(LuauWriter luau) => luau.WriteNodes(Statements);
    }
}
