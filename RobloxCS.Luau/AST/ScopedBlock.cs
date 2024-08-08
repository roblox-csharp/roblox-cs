
namespace RobloxCS.Luau
{
    public class ScopedBlock : Block
    {
        public ScopedBlock(List<Statement> statements) : base(statements)
        {
        }

        public override void Render(LuauWriter luau)
        {
            luau.WriteLine("do");
            luau.PushIndent();
            base.Render(luau);
            luau.PopIndent();
            luau.WriteLine("end");
        }
    }
}
