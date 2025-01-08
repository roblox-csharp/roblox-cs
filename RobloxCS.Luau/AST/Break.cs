namespace RobloxCS.Luau;

public class Break : Statement
{
    public override void Render(LuauWriter luau) => luau.WriteLine("break");
}