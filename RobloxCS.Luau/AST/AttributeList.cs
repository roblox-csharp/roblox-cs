namespace RobloxCS.Luau;

public class AttributeList : Statement
{
    public List<Statement> Attributes { get; }
    public bool Inline { get; set; } = false;

    public AttributeList(List<Statement> attributes)
    {
        Attributes = attributes;
        AddChildren(Attributes);
    }

    public override void Render(LuauWriter luau)
    {
        foreach (var attributeNode in Attributes)
        {
            if (attributeNode is BuiltInAttribute attribute)
            {
                attribute.Render(luau);

                if (Inline || attribute.Inline) continue;

                luau.WriteLine();
            }
            else
            {
                // TODO: user-defined attribute stuff
            }
        }
    }
}