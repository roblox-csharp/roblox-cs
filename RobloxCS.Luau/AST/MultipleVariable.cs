namespace RobloxCS.Luau;

public sealed class MultipleVariable : BaseVariable
{
    public MultipleVariable(HashSet<IdentifierName> names, bool isLocal, List<Expression> initializers, TypeRef? type = null)
        : base(isLocal, type)
    {
        Names = names;
        Initializers = initializers;

        AddChildren(Names);
        AddChildren(Initializers);
    }

    public HashSet<IdentifierName> Names { get; }
    public List<Expression> Initializers { get; }

    public override void Render(LuauWriter luau) => luau.WriteVariable(Names, IsLocal, Initializers, Type);
}