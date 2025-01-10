namespace RobloxCS.Luau;

public class Literal(string valueText) : Expression
{
    public string ValueText { get; } = valueText;

    public override void Render(LuauWriter luau) => luau.Write(ValueText);
}