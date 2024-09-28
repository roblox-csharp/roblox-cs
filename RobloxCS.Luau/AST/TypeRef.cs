namespace RobloxCS.Luau
{
    public class TypeRef : Expression
    {
        public string Path { get; }
        public bool IsNullable { get; protected set; }

        public TypeRef(string path)
        {
            Path = Utility.GetMappedType(path);
        }

        public override void Render(LuauWriter luau)
        {
            luau.Write(Path);
        }
    }
}