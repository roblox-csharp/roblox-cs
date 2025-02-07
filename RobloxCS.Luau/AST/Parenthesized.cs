namespace RobloxCS.Luau;

public class Parenthesized : Expression
{
    public Expression Expression { get; }

    public Parenthesized(Expression expression)
    {
        Expression = expression;
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write('(');
        Expression.Render(luau);
        luau.Write(')');
    }
}