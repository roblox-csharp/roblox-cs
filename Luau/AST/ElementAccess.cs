namespace RobloxCS.Luau
{
    public class ElementAccess(Expression expression, Expression index) : Expression
    {
        public Expression Expression { get; } = expression;
        public Expression Index { get; } = index;

        public override void Render(LuauWriter luau)
        {
            Expression.Render(luau);
            luau.Write('[');
            Index.Render(luau);
            luau.Write(']');
        }
    }
}