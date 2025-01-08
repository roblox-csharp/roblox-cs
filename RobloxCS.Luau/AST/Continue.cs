namespace RobloxCS.Luau;

public class Continue : Statement
{
    public override void Render(LuauWriter luau) => luau.WriteLine("continue");
}