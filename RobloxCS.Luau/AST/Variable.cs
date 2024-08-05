namespace RobloxCS.Luau
{
    public sealed class Variable : Statement
    {
        public Name Name { get; }
        public bool IsLocal { get; }
        public Expression? Initializer;
        public TypeRef? Type { get; }

        public Variable(Name name, bool isLocal, Expression? initializer = null, TypeRef? type = null)
        {
            Name = name;
            IsLocal = isLocal;
            Initializer = initializer;
            Type = type;

            AddChild(Name);
            if (Initializer != null)
            {
                AddChild(Initializer);
            }
            if (Type != null)
            {
                AddChild(Type);
            }
        }

        public override void Render(LuauWriter luau)
        {
            luau.WriteVariable(Name, IsLocal, Initializer, Type);
        }
    }
}