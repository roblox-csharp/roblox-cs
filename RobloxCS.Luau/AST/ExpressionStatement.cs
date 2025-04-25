namespace RobloxCS.Luau;

public class ExpressionStatement : Statement
{
    public Expression Expression { get; }

    public ExpressionStatement(Expression expression)
    {
        Expression = expression;
        AddChild(Expression);
    }

    public override void Render(LuauWriter luau)
    {
        Expression.Render(luau);

        if (Expression is NoOpExpression) return;

        luau.WriteLine();
    }
}