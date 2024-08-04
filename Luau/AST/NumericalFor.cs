namespace RobloxCS.Luau
{
    public class NumericalFor(Variable? initializer, Expression? incrementBy, Expression? condition, Statement body) : Statement
    {
        public Variable? Initializer { get; } = initializer;
        public Expression Condition { get; } = condition ?? new Literal("true");
        public Statement? IncrementBy { get; private set; } = incrementBy != null ? new ExpressionStatement(incrementBy) : null;
        public Statement Body { get; } = body;

        public override void Render(LuauWriter luau)
        {
            luau.WriteLine("do");
            luau.PushIndent();
            Initializer?.Render(luau);

            var shouldIncrementIdentifier = new IdentifierName("_shouldIncrement");
            if (IncrementBy != null)
            {
                new Variable(shouldIncrementIdentifier, true, AstUtility.False()).Render(luau);
            }

            luau.WriteLine("while true do");
            luau.PushIndent();
            Body.Render(luau);
            if (IncrementBy != null)
            {
                if (IncrementBy is ExpressionStatement expressionStatement
                    && expressionStatement.Expression is BinaryOperator binaryOperator
                    && !binaryOperator.Operator.Contains('='))
                {
                    IncrementBy = new Variable(new IdentifierName("_"), true, expressionStatement.Expression);
                }
                new If(shouldIncrementIdentifier, IncrementBy, new ExpressionStatement(new Assignment(shouldIncrementIdentifier, AstUtility.True()))).Render(luau);
            }

            new If(new UnaryOperator("not ", new Parenthesized(Condition)), new Break()).Render(luau);

            luau.PopIndent();
            luau.WriteLine("end");

            luau.PopIndent();
            luau.WriteLine("end");
        }
    }
}