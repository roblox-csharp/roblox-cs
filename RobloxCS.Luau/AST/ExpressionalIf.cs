namespace RobloxCS.Luau
{
    public class ExpressionalIf : Expression
    {
        public Expression Condition { get; }
        public Expression Body { get; }
        public Expression? ElseBranch { get; }

        public ExpressionalIf(Expression condition, Expression body, Expression? elseBranch = null)
        {
            Condition = condition;
            Body = body;
            ElseBranch = elseBranch;

            AddChild(Condition);
            AddChild(Body);
            if (ElseBranch != null)
            {
                AddChild(ElseBranch);
            }
        }

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