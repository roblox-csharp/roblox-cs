namespace RobloxCS.Luau;

public abstract class Comment(string contents) : Statement
{
    public string Contents { get; } = contents;
}