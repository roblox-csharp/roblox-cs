namespace RobloxCS.Luau;

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
            Names.First().Render(luau);
        else
            luau.Write("_binding");
        
        luau.Write(" in ");
        Iterable.Render(luau);
        luau.WriteLine(" do");
        luau.PushIndent();

        if (!singleValueIteration)
        {
            var index = 0;
            foreach (var name in Names)
            {
                var indexLiteral = new Literal((++index).ToString());
                luau.WriteVariable(name, true, new ElementAccess(new IdentifierName("_binding"), indexLiteral));
            }
        }
        
        Body.Render(luau);
        luau.PopIndent();
        luau.WriteLine("end");
    }
}