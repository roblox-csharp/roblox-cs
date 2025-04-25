namespace RobloxCS.Luau;

public sealed class Variable : BaseVariable
{
    public Variable(IdentifierName name, bool isLocal, Expression? initializer = null, TypeRef? type = null)
        : base(isLocal, type)
    {
        Name = name;
        Initializer = initializer;

        AddChild(Name);
        if (Initializer != null) AddChild(Initializer);
    }

    public IdentifierName Name { get; }
    public Expression? Initializer { get; }

    public override void Render(LuauWriter luau) => luau.WriteVariable([Name], IsLocal, Initializer != null ? [Initializer] : [], Type);
}