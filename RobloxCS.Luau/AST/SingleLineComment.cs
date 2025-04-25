namespace RobloxCS.Luau;

public class SingleLineComment(string contents)
    : Comment(contents)
{
    public override void Render(LuauWriter luau)
    {
        luau.Write("-- ");
        luau.Write(Contents);
    }
}