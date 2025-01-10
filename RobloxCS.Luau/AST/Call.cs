namespace RobloxCS.Luau
{
    public sealed class Call : Expression
    {
        public Expression Callee { get; }
        public ArgumentList ArgumentList { get; }

        public Call(Expression callee, ArgumentList argumentList)
        {
            Callee = callee;
            ArgumentList = argumentList;
            AddChildren([Callee, ArgumentList]);
        }

        public override void Render(LuauWriter luau)
        {
            Callee.Render(luau);
            luau.Write('(');
            luau.WriteNodesCommaSeparated(ArgumentList.Arguments);
            luau.Write(')');
        }
    }
}