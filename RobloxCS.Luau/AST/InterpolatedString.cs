namespace RobloxCS.Luau;

public class InterpolatedString : Expression
{
    public List<Expression> Parts { get; }

    public InterpolatedString(List<Expression> parts)
    {
        Parts = parts;
        AddChildren(Parts);
    }

    public override void Render(LuauWriter luau)
    {
        luau.Write('`');
        foreach (var part in Parts) part.Render(luau);

        luau.Write('`');
    }
}