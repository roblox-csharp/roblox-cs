namespace RobloxCS.Luau
{
    public class If(Expression condition, Statement body, Statement? elseBranch) : Statement
    {
        public Expression Condition { get; } = condition;
        public Statement Body { get; } = body;
        public Statement? ElseBranch { get; } = elseBranch;

        public override void Render(LuauWriter luau)
        {
            luau.WriteIf(Condition, Body, ElseBranch);
        }
    }
}