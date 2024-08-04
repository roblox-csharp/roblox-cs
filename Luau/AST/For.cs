namespace RobloxCS.Luau
{
    public class For(Variable initializer, Node incrementBy, Expression condition, Statement body) : Statement
    {
        public Variable Initializer { get; } = initializer;
        public Expression Condition { get; } = condition;
        public Node IncrementBy { get; } = incrementBy;
        public Statement Body { get; } = body;

        public override void Render(LuauWriter luau)
        {
            Initializer.Render(luau);
            luau.Write("while ");
            Condition.Render(luau);
            luau.WriteLine(" do");
            luau.PushIndent();
            Body.Render(luau);
            IncrementBy.Render(luau);
            luau.PopIndent();
            luau.WriteLine();
            luau.WriteLine("end");
        }
    }
}