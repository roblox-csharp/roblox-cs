namespace RobloxCS.Luau
{
    public class While(Expression condition, Statement body) : Statement
    {
        public Expression Condition { get; } = condition;
        public Statement Body { get; } = body;

        public override void Render(LuauWriter luau)
        {
            luau.Write("while ");
            Condition.Render(luau);
            luau.WriteLine(" do");
            luau.PushIndent();
            Body.Render(luau);
            luau.PopIndent();
            luau.WriteLine("end");
        }
    }
}