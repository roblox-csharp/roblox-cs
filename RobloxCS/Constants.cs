using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS
{
    internal static class Constants
    {
        public static readonly HashSet<string> UNSUPPORTED_BITWISE_TYPES =
        [
            "UInt128",
            "ulong",
            "long",
            "Int128"
        ];

        public static readonly HashSet<string> LENGTH_READABLE_TYPES =
        [
            "String",
            "string",
            "Array"
        ];

        public static readonly Dictionary<string, string> MAPPED_STRING_METHODS = new Dictionary<string, string>
        {
            { "Replace", "gsub" },
            { "Split", "split" },
            { "ToLower", "lower" },
            { "ToUpper", "upper" },
            { "Reverse", "reverse" }
        };

        public static readonly HashSet<SyntaxKind> MEMBER_PARENT_SYNTAXES =
        [
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.StructDeclaration
        ];

        public static readonly HashSet<string> NO_FULL_QUALIFICATION_TYPES =
        [
            "System",
            "Roblox",
            "Globals",
            "PluginClasses"
        ];

        public static readonly HashSet<string> IGNORED_BINARY_OPERATORS =
        [
            "as"
        ];

        public static readonly Dictionary<List<string>, (string, string)> PER_TYPE_BINARY_OPERATOR_MAP = new Dictionary<List<string>, (string, string)>
        {
            { ["String", "string"], ("+", "..") }
        };

        public static readonly HashSet<string> DECIMAL_TYPES =
        [
            "float",
            "double",
            "Single",
            "Double"
        ];

        public static readonly HashSet<string> INTEGER_TYPES =
        [
            "sbyte",
            "byte",
            "short",
            "ushort",
            "int",
            "uint",
            "long",
            "ulong",
            "SByte",
            "Byte",
            "Int16",
            "Int32",
            "Int64",
            "Int128",
            "UInt16",
            "UInt32",
            "UInt64",
            "UInt128",
        ];
    }
}