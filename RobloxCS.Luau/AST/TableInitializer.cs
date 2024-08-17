namespace RobloxCS.Luau
{
    public class TableInitializer : Expression
    {
        public List<Expression> Values { get; }
        public List<Expression> Keys { get; }

        public TableInitializer(List<Expression>? values = null, List<Expression>? keys = null)
        {
            Values = values ?? [];
            Keys = keys ?? [];
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
                    if (key is not IdentifierName)
                    {
                        luau.Write('[');
                    }
                    key.Render(luau);
                    if (key is not IdentifierName)
                    {
                        luau.Write(']');
                    }
                    luau.Write(" = ");
                }

                value.Render(luau);
                if (value != Values.Last())
                {
                    luau.Write(',');
                    if (hasAnyKeys && value is not AnonymousFunction)
                    {
                        luau.WriteLine();
                    }
                    else
                    {
                        luau.Write(' ');
                    }
                }
            }
            if (hasAnyKeys)
            { 
                luau.PopIndent();
                luau.WriteLine();
            }
            luau.Write('}');
        }
    }
}
