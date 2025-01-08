namespace RobloxCS.Luau;

public class Repeat : Statement
{
    public Expression UntilCondition { get; }
    public Statement Body { get; }

    public Repeat(Expression untilCondition, Statement body)
    {
        UntilCondition = untilCondition;
        Body = body;
        AddChildren([UntilCondition, Body]);
    }

    public override void Render(LuauWriter luau)
    {
        luau.WriteLine("repeat ");
        luau.PushIndent();
        Body.Render(luau);
        luau.PopIndent();
        luau.Write("until ");
        UntilCondition.Render(luau);
        luau.WriteLine(""); // is there a better way to add a new line?
    }
}