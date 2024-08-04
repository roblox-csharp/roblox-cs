namespace RobloxCS.Luau
{
    public class BinaryOperator(Expression left, string @operator, Expression right) : Expression
    {
        public Expression Left { get; } = left;
        public string Operator { get; } = @operator;
        public Expression Right { get; } = right;

        public override void Render(LuauWriter luau)
        {
            Left.Render(luau);
            luau.Write($" {Operator} ");
            Right.Render(luau);
        }
    }
}