namespace RobloxCS.Luau;

public class TypeCast : Expression
{
    public Expression Expression { get; }
    public TypeRef Type { get; }

    public TypeCast(Expression expression, TypeRef type)
    {
        Expression = expression;
        Type = type;
        AddChildren([Expression, Type]);
    }

    public override void Render(LuauWriter luau) => luau.WriteTypeCast(Expression, Type);
}