namespace RobloxCS.Luau;

public class ArrayType : TypeRef
{
    public TypeRef ElementType { get; }

    public ArrayType(TypeRef elementType)
        : base("{ " + elementType.Path + " }", true)
    {
        ElementType = elementType;
        AddChild(ElementType);
    }
}