namespace RobloxCS.Luau;

public class ArgumentList : Expression
{
    public List<Argument> Arguments { get; set; }

    public ArgumentList(List<Argument> arguments)
    {
        Arguments = arguments;
        AddChildren(Arguments);
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write('(');
        luau.WriteNodesCommaSeparated(Arguments);
        luau.Write(')');
    }
}