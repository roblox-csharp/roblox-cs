namespace RobloxCS.Luau
{
    public class Return(Expression? expression) : Statement
    {
        public Expression? Expression { get; } = expression;

        public override void Render(LuauWriter luau)
        {
            luau.WriteReturn(Expression);
        }
    }
}