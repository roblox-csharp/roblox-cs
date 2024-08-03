namespace RobloxCS.Luau
{
    public class ParameterList(List<Parameter> parameters) : Statement
    {
        public List<Parameter> Parameters { get; } = parameters;

        public override void Render(LuauWriter luau)
        {
            luau.Write('(');
            foreach (var parameter in Parameters)
            {
                parameter.Render(luau);
                if (parameter != Parameters.Last())
                {
                    luau.Write(", ");
                }
            }
            luau.Write(')');
        }
    }
}