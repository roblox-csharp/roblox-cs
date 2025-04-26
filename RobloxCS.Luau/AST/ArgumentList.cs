namespace RobloxCS.Luau;

public class ArgumentList : Expression
{
    public static readonly ArgumentList Empty = new([]);

    public ArgumentList(List<Argument> arguments)
    {
        Arguments = arguments;
        AddChildren(Arguments);
    }

    public List<Argument> Arguments { get; }

    public override void Render(LuauWriter luau)
    {
        luau.Write('(');
        luau.WriteNodesCommaSeparated(Arguments);
        luau.Write(')');
    }
}