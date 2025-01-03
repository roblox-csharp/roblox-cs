namespace RobloxCS.Luau
{
    public class TypeOfCall(Expression expression) : TypeRef("")
    {
        public Expression Expression { get; } = expression;

        public override void Render(LuauWriter luau)
        {
            luau.Write("typeof(");
            Expression.Render(luau);
            luau.Write(")");
        }
    }
}