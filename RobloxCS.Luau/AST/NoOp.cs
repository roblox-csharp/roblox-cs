namespace RobloxCS.Luau;

/// <summary>Optionally renders a newline.</summary>
public sealed class NoOp(bool createNewline = true) : Statement
{
    public override void Render(LuauWriter luau)
    {
        if (!createNewline) return;

        luau.WriteLine();
    }
}