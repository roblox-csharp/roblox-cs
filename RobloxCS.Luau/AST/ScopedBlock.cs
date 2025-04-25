namespace RobloxCS.Luau;

public class ScopedBlock(List<Statement> statements)
    : Block(statements)
{
    public override void Render(LuauWriter luau)
    {
        luau.WriteLine("do");
        luau.PushIndent();
        base.Render(luau);
        luau.PopIndent();
        luau.WriteLine("end");
    }
}