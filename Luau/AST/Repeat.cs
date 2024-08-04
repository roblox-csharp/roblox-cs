namespace RobloxCS.Luau
{
    public class Repeat(Expression untilCondition, Statement body) : Statement
    {
        public Expression UntilCondition { get; } = untilCondition;
        public Statement Body { get; } = body;

        public override void Render(LuauWriter luau)
        {
            luau.WriteLine("repeat ");
            luau.PushIndent();
            Body.Render(luau);
            luau.PopIndent();
            luau.Write("until ");
            UntilCondition.Render(luau);
        }
    }
}