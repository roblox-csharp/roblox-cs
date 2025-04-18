namespace RobloxCS.Luau;

public class BuiltInAttribute : Statement
{
    public Name Name { get; }
    public bool Inline { get; }

    public BuiltInAttribute(Name name, bool inline = false)
    {
        Name = name;
        Inline = inline;
        AddChild(name);
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write('@');
        Name.Render(luau);
    }
}