using System.Linq.Expressions;

namespace RobloxCS.Luau
{
    public class MemberAccess : AssignmentTarget
    {
        public Expression Expression { get; }
        public char Operator { get; set; }
        public SimpleName Name { get; }

        public MemberAccess(Expression expression, SimpleName name, char @operator = '.')
        {
            Console.WriteLine(expression.ToString());
            Console.WriteLine(name.ToString());

            Expression = expression;
            Operator = @operator;
            Name = name;
            AddChildren([Expression, Name]);
        }

        public override void Render(LuauWriter luau)
        {
            Expression.Render(luau);
            luau.Write(Operator);
            Name.Render(luau);
        }
    }
}