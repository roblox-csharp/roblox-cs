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
            new SingleLineComment("Compiled with roblox-cs v2.0.0").Render(luau);
            luau.WriteLine();
            luau.WriteLine();
            
            foreach (var statement in Statements)
                statement.Render(luau);
            
            luau.WriteReturn();
        }
    }
}