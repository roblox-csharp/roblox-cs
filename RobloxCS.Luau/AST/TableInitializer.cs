namespace RobloxCS.Luau;

public class TableInitializer : Expression
{
    public List<Expression> Values { get; }
    public List<Expression> Keys { get; }
    public List<KeyValuePair<Expression, Expression>> Entries { get; }
    public bool TreatIdentifiersAsKeyNames { get; }

    public TableInitializer(List<Expression>? values = null,
        List<Expression>? keys = null,
        bool treatIdentifiersAsKeyNames = false)
    {
        TreatIdentifiersAsKeyNames = treatIdentifiersAsKeyNames;
        Values = values ?? [];
        Keys = keys ?? [];

        Entries = [];
        for (var i = 0; i < Math.Max(Values.Count, Keys.Count); i++)
        {
            var key = Keys.ElementAtOrDefault(i);
            var value = Values.ElementAtOrDefault(i);
            if (key == null || value == null) continue;
            
            Entries.Add(KeyValuePair.Create(key, value));
        }
            
        AddChildren(Values);
        AddChildren(Keys);
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
            
        foreach (var value in Values)
        {
            var index = Values.IndexOf(value);
            var key = Keys.ElementAtOrDefault(index);
            if (key != null)
            {
                if (!TreatIdentifiersAsKeyNames || key is not IdentifierName)
                    luau.Write('[');
                
                key.Render(luau);
                if (!TreatIdentifiersAsKeyNames || key is not IdentifierName)
                    luau.Write(']');
                
                luau.Write(" = ");
            }

            value.Render(luau);
            if (value == Values.Last()) continue;
                
            luau.Write(',');
            if (hasAnyKeys && value is not AnonymousFunction)
                luau.WriteLine();
            else
                luau.Write(' ');
        }
            
        if (hasAnyKeys)
        { 
            luau.PopIndent();
            luau.WriteLine();
        }
            
        luau.Write('}');
    }
}