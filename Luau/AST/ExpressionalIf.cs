namespace RobloxCS.Luau
{
    public class ExpressionalIf(Expression condition, Expression body, Expression? elseBranch = null) : Expression
    {
        public Expression Condition { get; } = condition;
        public Expression Body { get; } = body;
        public Expression? ElseBranch { get; } = elseBranch;

        public override void Render(LuauWriter luau)
        {
            luau.Write("if ");
            Condition.Render(luau);
            luau.WriteLine(" then");
            luau.PushIndent();
            new ExpressionStatement(Body).Render(luau);

            var isElseIf = ElseBranch is ExpressionalIf;
            if (ElseBranch != null)
            {
                luau.PopIndent();
                luau.Write("else" + (isElseIf ? "" : '\n'));
                if (!isElseIf)
                {
                    luau.PushIndent();
                }
                new ExpressionStatement(ElseBranch).Render(luau);
                if (!isElseIf)
                {
                    luau.PopIndent();
                }
            }
        }
    }
}