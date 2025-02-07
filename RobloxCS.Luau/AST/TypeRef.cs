namespace RobloxCS.Luau;

public class TypeRef(string path, bool rawPath = false) : Name
{
    public string Path { get; protected init; } = rawPath ? path : AstUtility.CreateTypeRef(path)!.Path;

    public override void Render(LuauWriter luau) => luau.Write(Path);
    
    public override string ToString() => Path;
}