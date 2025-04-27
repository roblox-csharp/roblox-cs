namespace RobloxCS.Luau;

public class MemberAccess : AssignmentTarget
{
    public MemberAccess(Expression expression, SimpleName name, char @operator = '.')
    {
        Expression = expression;
        Operator = @operator;
        Name = name;
        AddChildren([Expression, Name]);
    }

    public Expression Expression { get; }
    public char Operator { get; }
    public SimpleName Name { get; }

    public override void Render(LuauWriter luau)
    {
        Expression.Render(luau);
        luau.Write(Operator);
        Name.Render(luau);
    }

    public MemberAccess WithOperator(char @operator) => new(Expression, Name, @operator);
}