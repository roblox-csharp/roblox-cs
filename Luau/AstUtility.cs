namespace RobloxCS
{
    public static class AstUtility
    {
        public static Luau.If Initializer(Luau.Name name, Luau.Expression initializer)
        {
            return new Luau.If(
                new Luau.BinaryOperator(name, "==", Nil()),
                new Luau.ExpressionStatement(new Luau.Assignment(name, initializer)),
                null
            );
        }

        public static Luau.Call Bit32Call(string methodName, params Luau.Expression[] arguments)
        {
            return new Luau.Call(
                new Luau.MemberAccess(
                    new Luau.IdentifierName("bit32"),
                    new Luau.IdentifierName(methodName)
                ),
                arguments.ToList()
            );
        }

        public static Luau.QualifiedName QualifiedNameFromMemberAccess(Luau.MemberAccess memberAccess)
        {
            Luau.Name left;
            if (memberAccess.Expression is Luau.MemberAccess leftMemberAccess)
            {
                left = QualifiedNameFromMemberAccess(leftMemberAccess);
            }
            else
            {
                left = (Luau.Name)memberAccess.Expression;
            }
            return new Luau.QualifiedName(left, memberAccess.Name);
        }

        public static Luau.Literal Vararg()
        {
            return new Luau.Literal("...");
        }

        public static Luau.Literal False()
        {
            return new Luau.Literal("false");
        }

        public static Luau.Literal True()
        {
            return new Luau.Literal("true");
        }

        public static Luau.Literal Nil()
        {
            return new Luau.Literal("nil");
        }
    }
}
