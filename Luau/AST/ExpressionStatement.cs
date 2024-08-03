namespace RobloxCS.Luau
{
    public class ExpressionStatement(Expression expression) : Statement
    {
        public Expression Expression { get; } = expression;

        public override void Render(LuauWriter luau)
        {
            Expression.Render(luau);
            luau.WriteLine();
        }
    }
}
