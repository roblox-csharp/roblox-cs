namespace RobloxCS.Luau
{
    public class VariableList(List<Variable> variables) : Statement
    {
        public List<Variable> Variables { get; } = variables;

        public override void Render(LuauWriter luau)
        {
            foreach (var variable in Variables)
            {
                variable.Render(luau);
            }
        }
    }
}