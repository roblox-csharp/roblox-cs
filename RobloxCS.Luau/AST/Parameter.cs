using System.Text.RegularExpressions;

namespace RobloxCS.Luau
{
    public class Parameter : Statement
    {

        public IdentifierName Name { get; }
        public Expression? Initializer { get; }
        public TypeRef? Type { get; }
        public bool IsVararg { get; }

        public Parameter(IdentifierName name, bool isVararg = false, Expression? initializer = null, TypeRef? type = null)
        {
            Name = name;
            Initializer = initializer;
            Type = type;
            IsVararg = isVararg;

            AddChild(Name);
            if (Initializer != null)
            {
                AddChild(Initializer);
            }
            if (Type != null)
            {
                Type = FixType(Type);
                AddChild(Type);
            }
        }

        public override void Render(LuauWriter luau)
        {
            if (IsVararg)
            {
                luau.Write("...");
            }
            else
            {
                Name.Render(luau);
            }
            if (Type != null)
            {
                luau.Write(": ");
                Type.Render(luau);
            }
        }

        private TypeRef FixType(TypeRef type) {
            var arrayTypeMatch = Regex.Match(type.Path, @"\{ (\w+) \}");
            if (arrayTypeMatch.Success && IsVararg)
            {
                var elementType = arrayTypeMatch.Groups[1].Value;
                return FixType(new TypeRef(elementType));
            }

            if (Initializer != null || type.IsNullable)
            {
                return new TypeRef(type.Path + "?");
            }

            return type;
        }
    }
}