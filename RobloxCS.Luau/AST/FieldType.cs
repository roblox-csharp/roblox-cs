namespace RobloxCS.Luau;

public class FieldType : TypeRef
{
    public string Name { get; }
    public TypeRef ValueType { get; }
    public bool IsReadOnly { get; }

    public FieldType(string name, TypeRef valueType, bool isReadOnly)
        : base($"{(isReadOnly ? "read " : "")}{name}: {valueType.Path};", true)
    {
        Name = name;
        ValueType = valueType;
        IsReadOnly = isReadOnly;
        AddChild(ValueType);
    }
}