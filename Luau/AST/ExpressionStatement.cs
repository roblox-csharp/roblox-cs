﻿namespace RobloxCS.Luau
{
    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; }

        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
            AddChildren([Expression]);
        }

        public override void Render(LuauWriter luau)
        {
            Expression.Render(luau);
            luau.WriteLine();
        }
    }
}
