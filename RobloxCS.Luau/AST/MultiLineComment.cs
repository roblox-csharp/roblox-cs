namespace RobloxCS.Luau;

public class MultiLineComment(string contents) : Comment(contents)
{
    public override void Render(LuauWriter luau)
    {
        luau.Write("--[[");
        luau.Write(Contents);
        luau.Write("]]");
    }
}