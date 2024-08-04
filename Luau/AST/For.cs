namespace RobloxCS.Luau
{
    public class For(List<Name> initializers, Expression iterator, Statement body) : Statement
    {
        public List<Name> Names { get; } = initializers;
        public Expression Iterator { get; } = iterator;
        public Statement Body { get; } = body;

        public override void Render(LuauWriter luau)
        {
            var singleValueIteration = Names.Count == 1;
            luau.Write("for _, ");
            if (singleValueIteration)
            {
                Names.First().Render(luau);
            }
            else
            {
                luau.Write("_binding");
            }
            luau.Write(" in ");
            Iterator.Render(luau);
            luau.WriteLine(" do");
            luau.PushIndent();

            if (!singleValueIteration)
            {
                foreach (var name in Names)
                {
                    var index = new Literal((Names.IndexOf(name) + 1).ToString());
                    new Variable(name, true, new ElementAccess(new IdentifierName("_binding"), index)).Render(luau);
                }
            }
            Body.Render(luau);

            luau.PopIndent();
            luau.WriteLine("end");
        }
    }
}