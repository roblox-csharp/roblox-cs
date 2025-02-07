namespace RobloxCS.Luau;

public class AnonymousFunction : Expression
{
    public ParameterList ParameterList { get; }
    public Block? Body { get; }
    public TypeRef? ReturnType { get; }
    public List<AttributeList> AttributeLists { get; }

    public AnonymousFunction(ParameterList parameterList, TypeRef? returnType = null,
        Block? body = null,
        List<AttributeList>? attributeLists = null)
    {
        ParameterList = parameterList;
        Body = body;
        Body = body;
        ReturnType = returnType;
        AttributeLists = attributeLists ?? []; AddChild(ParameterList);
        AddChild(ParameterList);
        if (ReturnType != null)
        { 
            AddChild(ReturnType);
        }
        if (Body != null)
        {
            AddChild(Body);
        }
        AddChildren(AttributeLists);
    }

    public override void Render(LuauWriter luau) =>
        luau.WriteFunction(null, false, ParameterList, ReturnType, Body, AttributeLists,
            inlineAttributes: true, createNewline: false);
}