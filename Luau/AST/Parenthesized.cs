namespace RobloxCS.Luau
{
    public class Parenthesized(Expression expression) : Expression
    {
        public Expression Expression { get; } = expression;

        public override void Render(LuauWriter luau)
        {
            luau.Write('(');
            Expression.Render(luau);
            luau.Write(')');
        }
    }
}