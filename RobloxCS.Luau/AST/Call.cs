namespace RobloxCS.Luau;

public sealed class Call : Expression
{
    public Call(Expression callee, ArgumentList? argumentList = null)
    {
        // monkey patch for https://github.com/roblox-csharp/roblox-cs/issues/44
        if (callee is Name name) callee = AstUtility.GetNonGenericName(name);

        Callee = callee;
        ArgumentList = argumentList ?? ArgumentList.Empty;
        AddChildren([Callee, ArgumentList]);
    }

    public Expression Callee { get; }
    public ArgumentList ArgumentList { get; }

    public override void Render(LuauWriter luau)
    {
        Callee.Render(luau);
        luau.Write('(');
        luau.WriteNodesCommaSeparated(ArgumentList.Arguments);
        luau.Write(')');
    }
}