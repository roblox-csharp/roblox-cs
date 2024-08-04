namespace RobloxCS.Luau
{
    public class Call(Expression callee, List<Expression> arguments) : Expression
    {
        public Expression Callee { get; } = callee;
        public List<Expression> Arguments { get; } = arguments;

        public override void Render(LuauWriter luau)
        {
            Callee.Render(luau);
            luau.Write('(');
            foreach (var argument in Arguments)
            {
                argument.Render(luau);
                if (argument != Arguments.Last())
                {
                    luau.Write(", ");
                }
            }
            luau.Write(')');
        }
    }
}