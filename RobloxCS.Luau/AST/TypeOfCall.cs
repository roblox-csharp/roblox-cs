namespace RobloxCS.Luau;

public sealed class TypeOfCall : TypeRef
{
    public TypeOfCall(Expression expression)
        : base("")
    {
        Expression = expression;
        AddChild(Expression);
    }

    public Expression Expression { get; }

    public override void Render(LuauWriter luau)
    {
        luau.Write("typeof(");
        Expression.Render(luau);
        luau.Write(")");
    }
}