namespace RobloxCS.Luau
{
    public class Return : Statement
    {
        public Expression? Expression { get; }

        public Return(Expression? expression = null)
        {
            Expression = expression;
            if (Expression != null) AddChild(Expression);
        }

        public override void Render(LuauWriter luau) => luau.WriteReturn(Expression);
    }
}