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
            luau.PushIndent();
            Body.Render(luau);

            var isElseIf = ElseBranch is If;
            if (ElseBranch != null)
            {
                luau.PopIndent();
                luau.Write("else" + (isElseIf ? "" : '\n'));
                if (!isElseIf)
                {
                    luau.PushIndent();
                }
                ElseBranch.Render(luau);
            }

            luau.PopIndent();
            if (!isElseIf)
            {
                luau.WriteLine("end");
            }
        }
    }
}