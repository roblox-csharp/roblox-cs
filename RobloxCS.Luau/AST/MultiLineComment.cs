namespace RobloxCS.Luau;

public class MultiLineComment(string contents)
    : Comment(contents)
{
    public override void Render(LuauWriter luau)
    {
        luau.WriteLine("--[[");
        luau.WriteLine(Contents);
        luau.WriteLine("]]");
    }
}