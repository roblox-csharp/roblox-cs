namespace RobloxCS.Luau;

public class Parameter : Statement
{
    public IdentifierName Name { get; }
    public Expression? Initializer { get; }
    public TypeRef? Type { get; }
    public bool IsVararg { get; }

    // parameter initializers are not true to luau, but we have not used prerequisite statements anywhere yet. so when we do, this will likely be phased out.
    public Parameter(IdentifierName name, bool isVararg = false, Expression? initializer = null, TypeRef? type = null)
    {
        Name = name;
        Initializer = initializer;
        Type = type;
        IsVararg = isVararg;

        AddChild(Name);
        if (Initializer != null) AddChild(Initializer);
        if (Type != null) AddChild(FixType(Type));
    }

    public override void Render(LuauWriter luau)
    {
        if (IsVararg)
            luau.Write("...");
        else
            Name.Render(luau);

        if (Type == null) return;

        luau.Write(": ");
        Type.Render(luau);
    }

    private TypeRef FixType(TypeRef type)
    {
        while (true)
        {
            if (type is ArrayType arrayType && IsVararg)
            {
                type = arrayType.ElementType;

                continue;
            }

            var isOptional = type is OptionalType;
            if (Initializer != null || isOptional)
            {
                return isOptional ? type : new OptionalType(type);
            }

            return type;
        }
    }
}