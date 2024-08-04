namespace RobloxCS.Luau
{
    public class MemberAccess(Expression expression, IdentifierName name, char @operator = '.') : Expression
    {
        public Expression Expression { get; } = expression;
        public char Operator { get; set; } = @operator;
        public IdentifierName Name { get; } = name;

        public override void Render(LuauWriter luau)
        {
            Expression.Render(luau);
            luau.Write($"{Operator}");
            Name.Render(luau);
        }
    }
}