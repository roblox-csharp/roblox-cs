namespace RobloxCS.Luau
{
    public class UnaryExpression(string @operator, Expression operand) : Expression
    {
        public string Operator { get; } = @operator;
        public Expression Operand { get; } = operand;

        public override void Render(LuauWriter luau)
        {
            luau.Write(Operator);
            Operand.Render(luau);
        }
    }
}