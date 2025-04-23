namespace RobloxCS.Luau;

/// <summary>Only meant for use with macros.</summary>
public sealed class NoOpExpression : Expression
{
    public override void Render(LuauWriter luau)
    {
    }
}