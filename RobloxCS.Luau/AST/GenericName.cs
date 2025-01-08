namespace RobloxCS.Luau;

public class GenericName(string text, List<string> typeArguments) : SimpleName
{
    public string Text { get; } = text;
    public List<string> TypeArguments { get; } = typeArguments;

    public override void Render(LuauWriter luau) => luau.Write(ToString());

    public override string ToString() => Text + '<' + string.Join(", ", TypeArguments) + '>';
}