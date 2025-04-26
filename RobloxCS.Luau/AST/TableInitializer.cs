using RobloxCS.Shared;

namespace RobloxCS.Luau;

public class TableInitializer : Expression
{
    public static readonly TableInitializer Empty = new();

    public TableInitializer(List<Expression>? values = null,
                            List<Expression>? keys = null)
    {
        Values = values ?? [];
        Keys = keys ?? [];

        KeyValuePairs = [];
        for (var i = 0; i < Math.Max(Values.Count, Keys.Count); i++)
        {
            var key = Keys.ElementAtOrDefault(i);
            var value = Values.ElementAtOrDefault(i);

            if (key == null || value == null) continue;

            KeyValuePairs.Add(KeyValuePair.Create(key, value));
        }

        AddChildren(Values);
        AddChildren(Keys);
    }

    public List<Expression> Values { get; }
    public List<Expression> Keys { get; }
    public List<KeyValuePair<Expression, Expression>> KeyValuePairs { get; }

    public static TableInitializer Union(TableInitializer a, TableInitializer b)
    {
        var kvpComparer = new StandardUtility.KeyValuePairEqualityComparer<Expression, Expression>();
        var pairs = a.KeyValuePairs.Union(b.KeyValuePairs, kvpComparer).ToDictionary();

        return new TableInitializer(pairs.Values.ToList(), pairs.Keys.ToList());
    }

    public override void Render(LuauWriter luau)
    {
        var hasAnyKeys = Keys.Count > 0;

        luau.Write('{');
        if (hasAnyKeys)
        {
            luau.WriteLine();
            luau.PushIndent();
        }

        for (var i = 0; i < Values.Count; i++)
        {
            var value = Values[i];
            var key = Keys.ElementAtOrDefault(i);
            if (key != null)
            {
                if (key is not IdentifierName) luau.Write('[');

                key.Render(luau);
                if (key is not IdentifierName) luau.Write(']');

                luau.Write(" = ");
            }

            value.Render(luau);

            if (i == Values.Count - 1) continue;

            luau.Write(',');
            luau.Write(hasAnyKeys ? '\n' : ' ');
        }

        if (hasAnyKeys)
        {
            luau.PopIndent();
            luau.WriteLine();
        }

        luau.Write('}');
    }
}