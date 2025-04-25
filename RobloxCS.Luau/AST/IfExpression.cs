namespace RobloxCS.Luau;

public class IfExpression : Expression
{
    public Expression Condition { get; }
    public Expression Body { get; }
    public Expression? ElseBranch { get; }
    public bool IsCompact { get; }

    public IfExpression(Expression condition, Expression body, Expression? elseBranch = null, bool isCompact = false)
    {
        Condition = condition;
        Body = body;
        ElseBranch = elseBranch;
        IsCompact = isCompact;

        AddChild(Condition);
        AddChild(Body);
        if (ElseBranch != null) AddChild(ElseBranch);
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write("if ");
        Condition.Render(luau);
        luau.Write(" then");
        luau.Write(IsCompact ? ' ' : '\n');
        if (!IsCompact) luau.PushIndent();

        Node body = IsCompact ? Body : new ExpressionStatement(Body); // for the newline
        body.Render(luau);

        var isElseIf = ElseBranch is IfExpression;

        if (ElseBranch == null) return;

        if (IsCompact)
            luau.Write(' ');
        else
            luau.PopIndent();

        luau.Write("else");
        if (IsCompact && !isElseIf) luau.Write(' ');
        if (!isElseIf && !IsCompact)
        {
            luau.WriteLine();
            luau.PushIndent();
        }

        Node elseBranch = IsCompact ? ElseBranch : new ExpressionStatement(ElseBranch);
        elseBranch.Render(luau);
        if (elseBranch is Statement) luau.Remove(1);

        if (isElseIf || IsCompact) return;

        luau.PopIndent();
    }
}