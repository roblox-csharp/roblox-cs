namespace RobloxCS.Luau;

public class While : Statement
{
    public Expression Condition { get; }
    public Statement Body { get; }

    public While(Expression condition, Statement body)
    {
        Condition = condition;
        Body = body;
        AddChildren([Condition, Body]);
    }

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