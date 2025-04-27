namespace RobloxCS.Shared;

public abstract class BaseClassInfo
{
    public HashSet<string> MemberUses { get; } = [];
}

public sealed class TypeClassInfo : BaseClassInfo
{
    public AssemblyClassInfo AssemblyClassInfo { get; } = new();
}

public sealed class AssemblyClassInfo : BaseClassInfo
{
    // TODO: method info uses, etc.
}