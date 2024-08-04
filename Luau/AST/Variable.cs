namespace RobloxCS.Luau
{
    public class Variable(Name name, bool isLocal, Expression? initializer = null, TypeRef? type = null) : Statement
    {
        public Name Name { get; } = name;
        public bool IsLocal { get; } = isLocal;
        public Expression? Initializer { get; } = initializer;
        public TypeRef? Type { get; } = type;

        public override void Render(LuauWriter luau)
        {
            luau.WriteVariable(Name, IsLocal, Initializer, Type);
        }
    }
}