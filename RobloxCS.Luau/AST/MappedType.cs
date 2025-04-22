namespace RobloxCS.Luau;

public class MappedType : TypeRef
{
    public TypeRef KeyType { get; }
    public TypeRef ValueType { get; }

    public MappedType(TypeRef keyType, TypeRef valueType)
        : base($"{{ [{keyType.Path}]: " + valueType.Path + " }", true)
    {
        KeyType = keyType;
        ValueType = valueType;
        AddChildren([KeyType, ValueType]);
    }
}