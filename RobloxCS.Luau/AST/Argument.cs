namespace RobloxCS.Luau;

public class Argument : Expression
{
    public Expression Expression { get; }

    public Argument(Expression expression)
    {
        Expression = expression;
        AddChild(Expression);
    }

    public override void Render(LuauWriter luau) => Expression.Render(luau);
}