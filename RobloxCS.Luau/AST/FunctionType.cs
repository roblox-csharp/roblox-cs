namespace RobloxCS.Luau;

public class FunctionType : TypeRef
{
    public static readonly FunctionType NoOp = new([], new TypeRef("()"));

    public FunctionType(List<ParameterType> parameterTypes, TypeRef returnType)
        : base("", true)
    {
        ParameterTypes = parameterTypes;
        ReturnType = returnType;
        Path = $"({string.Join(", ", parameterTypes.Select(type => type.Path))}) -> {returnType.Path}";
    }

    public List<ParameterType> ParameterTypes { get; }
    public TypeRef ReturnType { get; }
}