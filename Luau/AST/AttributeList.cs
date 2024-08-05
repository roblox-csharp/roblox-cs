namespace RobloxCS.Luau
{
    public class AttributeList : Statement
    {
        public List<BaseAttribute> Attributes { get; }
        public bool Inline { get; set; } = false;

        public AttributeList(List<BaseAttribute> attributes)
        {
            Attributes = attributes;
        }

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