namespace RobloxCS.Luau
{
    public class TypeRef : Expression
    {
        public string Path { get; }
        public bool IsNullable { get; }

        public TypeRef(string path)
        {
            Path = Utility.GetMappedType(path);
            IsNullable = Path.EndsWith("?") || Path == "nil";
        }

        public override void Render(LuauWriter luau)
        {
            luau.Write(Path);
        }
    }
}