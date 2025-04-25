namespace RobloxCS.Luau;

public abstract class BaseVariable : Statement
{
    protected BaseVariable(bool isLocal, TypeRef? type)
    {
        IsLocal = isLocal;
        Type = type;
        if (Type != null) AddChild(Type);
    }

    public bool IsLocal { get; }
    public TypeRef? Type { get; }
}