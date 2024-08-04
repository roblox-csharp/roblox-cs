namespace RobloxCS.Luau
{
    public class QualifiedName : Name
    {
        public Name Left { get; }
        public IdentifierName Right { get; }

        public QualifiedName(Name left, IdentifierName right)
        {
            Left = left;
            Right = right;
            AddChildren([Left, Right]);
        }

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