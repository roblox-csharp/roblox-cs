namespace RobloxCS.Luau
{
    public class BinaryOperator : Expression
    {
        public Expression Left { get; }
        public string Operator { get; }
        public Expression Right { get; }

        public BinaryOperator(Expression left, string @operator, Expression right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
            AddChildren([Left, Right]);
        }

        public override void Render(LuauWriter luau)
        {
            Left.Render(luau);
            luau.Write($" {Operator} ");
            Right.Render(luau);
        }

        public BinaryOperator WithLeft(Expression left) => new BinaryOperator(left, Operator, Right);
        public BinaryOperator WithRight(Expression right) => new BinaryOperator(Left, Operator, right);
    }
}