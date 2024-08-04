namespace RobloxCS.Luau
{
    public class TableInitializer(List<Expression> values, List<Expression>? keys = null) : Expression
    {
        public List<Expression> Values { get; } = values;
        public List<Expression> Keys { get; } = keys ?? [];

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
                    luau.Write('[');
                    key.Render(luau);
                    luau.Write("] = ");
                }

                value.Render(luau);
                if (value != Values.Last())
                {
                    luau.Write(", ");
                }
                luau.WriteLine();
            }
            if (hasAnyKeys)
            { 
                luau.PopIndent();
            }
            luau.Write('}');
        }
    }
}
