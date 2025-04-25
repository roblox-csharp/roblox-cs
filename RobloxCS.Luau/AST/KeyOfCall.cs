namespace RobloxCS.Luau;

public sealed class KeyOfCall : TypeRef
{
    public TypeRef TypeRef { get; }

    public KeyOfCall(TypeRef typeRef)
        : base(typeRef.Path)
    {
        TypeRef = typeRef;
        AddChild(TypeRef);
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write("keyof<");
        TypeRef.Render(luau);
        luau.Write(">");
    }
}