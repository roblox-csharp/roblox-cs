namespace RobloxCS.Luau;

/// <summary>
/// An interpolated section of an <see cref="InterpolatedString"/>
/// </summary>
public class Interpolation : Expression
{
    public Expression Expression { get; }

    public Interpolation(Expression expression)
    {
        Expression = expression;
        AddChild(Expression);
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write('{');
        Expression.Render(luau);
        luau.Write('}');
    }
}