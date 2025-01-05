namespace RobloxCS.Luau
{
    public class For : Statement
    {
        public List<IdentifierName> Names { get; }
        public Expression Iterable { get; }
        public Statement Body { get; }

        public For(List<IdentifierName> initializers, Expression iterable, Statement body)
        {
            Names = initializers;
            Iterable = iterable;
            Body = body;
            AddChildren(Names);
            AddChild(Iterable);
            AddChild(Body);
        }

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
            Iterable.Render(luau);
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