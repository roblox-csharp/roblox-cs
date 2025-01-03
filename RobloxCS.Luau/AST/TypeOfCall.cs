namespace RobloxCS.Luau
{
    public sealed class TypeOfCall(Expression expression) : TypeRef("")
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