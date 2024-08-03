namespace RobloxCS.Luau
{
    public class Parameter(Name name, Expression? initializer, TypeRef? type) : Statement
    {
        public Name Name { get; } = name;
        public Expression? Initializer { get; } = initializer;
        public TypeRef? Type { get; } = type == null ? null : ((initializer != null || type.IsNullable) ? new TypeRef(type.Path + "?") : type);

        public override void Render(LuauWriter luau)
        {
            Name.Render(luau);
            if (Type != null)
            {
                luau.Write(": ");
                Type.Render(luau);
            }
        }
    }
}