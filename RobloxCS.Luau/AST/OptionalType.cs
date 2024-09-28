﻿namespace RobloxCS.Luau
{
    public class OptionalType : TypeRef
    {
        public TypeRef NonNullableType { get; }

        public OptionalType(TypeRef nonNullableType)
            : base(nonNullableType.Path + "?")
        {
            NonNullableType = nonNullableType;
        }
    }
}