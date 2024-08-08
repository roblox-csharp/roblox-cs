namespace RobloxCS.Luau
{
    public class QualifiedName : Name
    {
        public Name Left { get; }
        public char Operator { get; }
        public IdentifierName Right { get; }

        public QualifiedName(Name left, IdentifierName right, char @operator = '.')
        {
            Left = left;
            Right = right;
            Operator = @operator;
            AddChildren([Left, Right]);
        }

        public override void Render(LuauWriter luau)
        {
            Left.Render(luau);
            luau.Write(Operator);
            Right.Render(luau);
        }

        public override string ToString()
        {
            return Left.ToString() + Operator + Right.ToString();
        }
    }
}