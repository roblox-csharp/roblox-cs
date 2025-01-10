namespace RobloxCS.Luau;

public class UnaryOperator : Expression
{
    public string Operator { get; }
    public Expression Operand { get; }

    public UnaryOperator(string @operator, Expression operand)
    {
        Operator = @operator;
        Operand = operand;
        AddChild(Operand);
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write(Operator);
        Operand.Render(luau);
    }
}