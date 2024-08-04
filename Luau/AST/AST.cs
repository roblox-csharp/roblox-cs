namespace RobloxCS.Luau
{
    public class AST(List<Statement> statements) : Node
    {
        public List<Statement> Statements { get; } = statements;

        public override void Render(LuauWriter luau)
        {
            luau.WriteLine("-- Compiled with roblox-cs v2.0.0");
            luau.WriteLine();
            foreach (var statement in Statements)
            {
                statement.Render(luau);
            }
        }
    }
}
