namespace RobloxCS.Luau
{
    /// <summary>Simply renders a newline.</summary>
    public sealed class NoOp : Statement
    {
        public override void Render(LuauWriter luau) => luau.WriteLine();
    }
}