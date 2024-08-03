namespace RobloxCS.Luau
{
    public class Function(Name name, bool isLocal, ParameterList parameterList, TypeRef? returnType, Block? body) : Statement
    {
        public Name Name { get; } = name;
        public bool IsLocal { get; } = isLocal;
        public ParameterList ParameterList { get; } = parameterList;
        public TypeRef? ReturnType { get; } = returnType;
        public Block? Body { get; } = body;

        public override void Render(LuauWriter luau)
        {
            luau.WriteFunction(Name, IsLocal, ParameterList, ReturnType, Body);
        }
    }
}