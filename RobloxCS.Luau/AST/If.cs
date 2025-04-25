namespace RobloxCS.Luau;

public class If : Statement
{
    public Expression Condition { get; }
    public Block Body { get; }
    public Block? ElseBranch { get; }

    public If(Expression condition, Block body, Block? elseBranch = null)
    {
        Condition = condition;
        Body = body;
        ElseBranch = elseBranch;

        AddChildren([Condition, Body]);
        if (ElseBranch != null) AddChild(ElseBranch);
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write("if ");
        Condition.Render(luau);
        luau.Write(" then");

        var compact = ElseBranch == null
                   && Body.Statements.Count == 1
                   && Body.Statements.First() is Return { Expression: null or Literal { ValueText: "nil" } } or Break or Continue;

        luau.Write(compact ? ' ' : '\n');
        if (!compact) luau.PushIndent();

        (compact ? Body.Statements.First() : Body).Render(luau);
        if (compact)
        {
            luau.Remove(1);
            luau.Write(' ');
        }

        var isElseIf = ElseBranch is { Statements.Count: 1 } && ElseBranch.Statements.First() is If;
        if (ElseBranch != null)
        {
            luau.PopIndent();
            luau.Write("else" + (isElseIf ? "" : '\n'));
            if (!isElseIf) luau.PushIndent();

            ElseBranch.Render(luau);
        }

        if (!compact) luau.PopIndent();

        if (isElseIf) return;

        luau.WriteLine("end");
    }
}