namespace RobloxCS.Luau;

public class ParameterList : Statement
{
    public static readonly ParameterList Empty = new([]);

    public ParameterList(List<Parameter> parameters)
    {
        Parameters = parameters;
        AddChildren(Parameters);
    }

    public List<Parameter> Parameters { get; }

    public override void Render(LuauWriter luau)
    {
        luau.Write('(');
        luau.WriteNodesCommaSeparated(Parameters);
        luau.Write(')');
    }
}