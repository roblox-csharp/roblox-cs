namespace RobloxCS.Luau
{
    public class MemberAccess : Expression
    {
        public Expression Expression { get; }
        public char Operator { get; set; }
        public IdentifierName Name { get; }

        public MemberAccess(Expression expression, IdentifierName name, char @operator = '.')
        {
            Expression = expression;
            Operator = @operator;
            Name = name;
            AddChildren([Expression, Name]);
        }

        public override void Render(LuauWriter luau)
        {
            Expression.Render(luau);
            luau.Write($"{Operator}");
            Name.Render(luau);
        }
    }
}