namespace RobloxCS.Luau
{
    public class AttributeList(List<BaseAttribute> variables) : Statement
    {
        public List<BaseAttribute> Attributes { get; } = variables;
        public bool Inline { get; set; } = false;

        public override void Render(LuauWriter luau)
        {
            foreach (var attribute in Attributes)
            {
                attribute.Render(luau);
                if (!Inline)
                {
                    luau.WriteLine();
                }
            }
        }
    }
}