namespace RobloxCS.Luau
{
    public class TypeCast(Expression expression, TypeRef type) : Expression
    {
        public Expression Expression { get; } = expression;
        public TypeRef Type { get; } = type;

        public override void Render(LuauWriter luau)
        {
            Expression.Render(luau);
            luau.Write(" :: ");
            Type.Render(luau);
        }
    }
}