namespace RobloxCS.Luau;

public abstract class Comment(string contents) : Node
{
    public string Contents { get; } = contents;
}