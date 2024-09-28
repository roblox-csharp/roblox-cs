namespace RobloxCS.Luau
{
    public class TypeRef : Expression
    {
        public string Path { get; protected set; }
        public bool IsNullable { get; protected set; }

        public TypeRef(string path, bool rawPath = false)
        {
            Path = rawPath ? path : AstUtility.CreateTypeRef(path)!.Path;
        }

        public override void Render(LuauWriter luau)
        {
            luau.Write(Path);
        }
    }
}