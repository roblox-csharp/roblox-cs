namespace RobloxCS
{
    public static class LuauUtility
    {
        public static Luau.If Initializer(Luau.Name name, Luau.Expression initializer)
        {
            return new Luau.If(
                new Luau.BinaryExpression(name, "==", Nil()),
                new Luau.Assignment(name, initializer),
                null
            );
        }

        public static Luau.Literal Nil()
        {
            return new Luau.Literal("nil");
        }
    }
}
