namespace RobloxCS.Luau;

public class ParameterType : TypeRef
{
    public string? Name { get; }
    public TypeRef Type { get; }

    public ParameterType(string? name, TypeRef type)
        : base(name != null ? $"{name}: {type.Path}" : type.Path, true)
    {
        Name = name;
        Type = type;
    }
}