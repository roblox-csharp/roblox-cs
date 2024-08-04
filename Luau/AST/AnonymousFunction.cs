namespace RobloxCS.Luau
{
    public class AnonymousFunction(ParameterList parameterList, Block? body) : Expression
    {
        public ParameterList ParameterList { get; } = parameterList;
        public Block? Body { get; } = body;

        public override void Render(LuauWriter luau)
        {
            luau.WriteFunction(null, false, ParameterList, null, Body);
        }
    }
}