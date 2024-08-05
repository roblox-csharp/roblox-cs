namespace RobloxCS.Luau
{
    public class TypeCast : Expression
    {
        public Expression Expression { get; }
        public TypeRef Type { get; }

        public TypeCast(Expression expression, TypeRef type)
        {
            Expression = expression;
            Type = type;
        }

        public override void Render(LuauWriter luau)
        {
            Expression.Render(luau);
            luau.Write(" :: ");
            Type.Render(luau);
        }
    }
}