namespace RobloxCS.Luau
{
    public class If(Expression condition, Statement body, Statement? elseBranch) : Statement
    {
        public Expression Condition { get; } = condition;
        public Statement Body { get; } = body;
        public Statement? ElseBranch { get; } = elseBranch;

        public override void Render(LuauWriter luau)
        {
            luau.Write("if ");
            Condition.Render(luau);
            luau.WriteLine(" then");
            luau.WritePossibleBlock(Body);
            luau.WriteLine();
            if (ElseBranch != null)
            {
                luau.WriteLine("else");
                luau.WritePossibleBlock(ElseBranch);
            }
            luau.WriteLine("end");
        }
    }
}