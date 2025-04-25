namespace RobloxCS.Luau;

public class Function : Statement
{
    public Name Name { get; }
    public bool IsLocal { get; }
    public ParameterList ParameterList { get; }
    public Block? Body { get; }
    public TypeRef? ReturnType { get; }
    public List<AttributeList> AttributeLists { get; }
    public List<IdentifierName>? TypeParameters { get; }

    public Function(Name name,
                    bool isLocal,
                    ParameterList parameterList,
                    TypeRef? returnType = null,
                    Block? body = null,
                    List<AttributeList>? attributeLists = null,
                    List<IdentifierName>? typeParameters = null)
    {
        Name = name;
        IsLocal = isLocal;
        ParameterList = parameterList;
        Body = body;
        ReturnType = returnType;
        AttributeLists = attributeLists ?? [];
        TypeParameters = typeParameters;

        AddChild(Name);
        AddChild(ParameterList);
        if (ReturnType != null) AddChild(ReturnType);
        if (Body != null) AddChild(Body);

        AddChildren(AttributeLists);
        if (typeParameters != null) AddChildren(typeParameters);
    }

    public override void Render(LuauWriter luau) =>
        luau.WriteFunction(Name,
                           IsLocal,
                           ParameterList,
                           ReturnType,
                           Body,
                           AttributeLists,
                           TypeParameters);
}