namespace RobloxCS.Luau
{
    public sealed class ElementAccess : Expression
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
            Node index = Index;
            luau.WriteDescendantStatements(ref index);
            Index = (Expression)index;

            Expression.Render(luau);
            luau.Write('[');
            Index.Render(luau);
            luau.Write(']');
        }
    }
}