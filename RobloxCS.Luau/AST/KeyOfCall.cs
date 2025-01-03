namespace RobloxCS.Luau
{
    public sealed class KeyOfCall(TypeRef typeRef) : TypeRef(typeRef.Path)
    {
        public TypeRef TypeRef { get; } = typeRef;

        public override void Render(LuauWriter luau)
        {
            luau.Write("keyof<");
            TypeRef.Render(luau);
            luau.Write(">");
        }
    }
}