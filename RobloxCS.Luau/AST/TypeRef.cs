namespace RobloxCS.Luau;

public class TypeRef(string path, bool rawPath = false) : Name
{
    public string Path { get; set; } = (rawPath ? path : AstUtility.CreateTypeRef(path)!.Path).Trim();

    public override void Render(LuauWriter luau) => luau.Write(Path);

    public override string ToString() => Path;
}