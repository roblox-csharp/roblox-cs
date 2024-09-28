namespace RobloxCS.Luau
{
    public sealed class TypeAlias : Statement
    {
        public IdentifierName Name { get; }
        public TypeRef Type { get; }

        public TypeAlias(IdentifierName name, TypeRef type)
        {
            Name = name;
            Type = type;

            AddChildren([Name, Type]);
        }

        public override void Render(LuauWriter luau)
        {
            luau.Write("type ");
            Name.Render(luau);
            luau.Write(" = ");
            Type.Render(luau);
            luau.WriteLine();
        }
    }
}