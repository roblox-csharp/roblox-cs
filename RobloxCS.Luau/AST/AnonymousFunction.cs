namespace RobloxCS.Luau
{
    public class AnonymousFunction : Expression
    {
        public ParameterList ParameterList { get; }
        public Block? Body { get; }

        public AnonymousFunction(ParameterList parameterList, Block? body = null)
        {
            ParameterList = parameterList;
            Body = body;
            AddChild(ParameterList);
            if (Body != null)
            { 
                AddChild(Body);
            }
        }

        public override void Render(LuauWriter luau)
        {
            luau.WriteFunction(null, false, ParameterList, null, Body, createNewline: false);
        }
    }
}