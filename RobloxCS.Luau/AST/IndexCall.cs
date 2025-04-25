namespace RobloxCS.Luau;

public class IndexCall(TypeRef typeRef, TypeRef key)
    : TypeRef(typeRef.Path)
{
    public TypeRef TypeRef { get; } = typeRef;
    public TypeRef Key { get; } = key;

    public override void Render(LuauWriter luau)
    {
        luau.Write("index<");
        TypeRef.Render(luau);
        luau.Write(", ");
        Key.Render(luau);
        luau.Write(">");
    }
}