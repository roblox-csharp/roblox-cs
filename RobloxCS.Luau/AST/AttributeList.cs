namespace RobloxCS.Luau;

public class AttributeList : Statement
{
    public List<Node> Attributes { get; }
    public bool Inline { get; set; } = false;

    public AttributeList(List<Node> attributes)
    {
        Attributes = attributes;
        AddChildren(Attributes);
    }

    public override void Render(LuauWriter luau)
    {
        foreach (var attribute in Attributes)
        {
            if (attribute is BuiltInAttribute)
            {
                attribute.Render(luau);
            
                if (Inline) continue;
                luau.WriteLine();
            }
            else
            {
                // TODO: user-defined attribute stuff
            }
        }
    }
}