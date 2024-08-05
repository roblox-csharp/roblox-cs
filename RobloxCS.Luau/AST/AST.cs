namespace RobloxCS.Luau
{
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
            luau.WriteLine("-- Compiled with roblox-cs v2.0.0");
            luau.WriteLine();
            foreach (var statement in Statements)
            {
                statement.Render(luau);
            }
            // TODO: return all public members?
            luau.WriteReturn();
        }
    }
}
