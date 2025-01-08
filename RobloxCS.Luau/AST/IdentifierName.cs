namespace RobloxCS.Luau;

public class IdentifierName(string text) : SimpleName
{
    public string Text { get; } = text;

    public override void Render(LuauWriter luau) => luau.Write(Text);

    public override string ToString() => Text;
}