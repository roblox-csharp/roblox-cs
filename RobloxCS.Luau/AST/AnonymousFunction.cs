namespace RobloxCS.Luau;

public class AnonymousFunction : Expression
{
    public ParameterList ParameterList { get; }
    public Block? Body { get; }
    public TypeRef? ReturnType { get; }
    public List<AttributeList> AttributeLists { get; }
    public List<IdentifierName>? TypeParameters { get; }

    public AnonymousFunction(ParameterList parameterList,
                             TypeRef? returnType = null,
                             Block? body = null,
                             List<AttributeList>? attributeLists = null,
                             List<IdentifierName>? typeParameters = null)
    {
        ParameterList = parameterList;
        Body = body;
        TypeParameters = typeParameters;
        Body = body;
        ReturnType = returnType;
        AttributeLists = attributeLists ?? [];
        AddChild(ParameterList);
        if (ReturnType != null) AddChild(ReturnType);
        if (Body != null) AddChild(Body);

        AddChildren(AttributeLists);
        if (typeParameters != null) AddChildren(typeParameters);
    }

    public override void Render(LuauWriter luau) =>
        luau.WriteFunction(null,
                           false,
                           ParameterList,
                           ReturnType,
                           Body,
                           AttributeLists,
                           inlineAttributes: true,
                           createNewline: false);
}