﻿using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RobloxCS.Luau
{
    public class Parameter : Statement
    {
        public Name Name { get; }
        public Expression? Initializer { get; }
        public TypeRef? Type { get; }
        public bool IsVararg { get; }

        public Parameter(Name name, bool isVararg, Expression? initializer, TypeRef? type)
        {
            Name = name;
            Initializer = initializer;
            Type = type;
            IsVararg = isVararg;

            if (Type != null)
            {
                Type = FixType(Type);
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