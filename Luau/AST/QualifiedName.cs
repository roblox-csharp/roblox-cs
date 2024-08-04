namespace RobloxCS.Luau
{
    public class QualifiedName(Name left, IdentifierName right) : Name
    {
        public Name Left { get; } = left;
        public IdentifierName Right { get; } = right;

        public override void Render(LuauWriter luau)
        {
            Left.Render(luau);
            luau.Write('.');
            Right.Render(luau);
        }

        public override string ToString()
        {
            return $"{Left}.{Right}";
        }
    }
}