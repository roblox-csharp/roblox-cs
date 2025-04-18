namespace RobloxCS.Luau;

public sealed class ElementAccess : AssignmentTarget
{
    public Expression Expression { get; }
    public Expression Index { get; set; }

    public ElementAccess(Expression expression, Expression index)
    {
        Expression = expression;
        Index = index;
        AddChildren([Expression, Index]);
    }

    public override void Render(LuauWriter luau)
    {
        Expression.Render(luau);
        luau.Write('[');
        Index.Render(luau);
        luau.Write(']');
    }
}