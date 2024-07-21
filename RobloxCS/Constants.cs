using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS
{
    internal static class Constants
    {
        public static readonly List<string> METAMETHODS = [
            "__tostring",
            "__add",
            "__sub",
            "__mul",
            "__div",
            "__idiv",
            "__mod",
            "__pow",
            "__unm",
            "__eq",
            "__le",
            "__lte",
            "__len",
            "__iter",
            "__call",
            "__concat",
            "__mode",
            "__index",
            "__newindex",
            "__metatable",
        ];

        public static readonly List<string> LUAU_KEYWORDS = [
            "local",
            "and",
            "or",
            "if",
            "else",
            "elseif",
            "then",
            "do",
            "end",
            "function",
            "for",
            "while",
            "in",
            "export",
            "type",
            "typeof"
        ];

        public static readonly List<SyntaxKind> MEMBER_PARENT_SYNTAXES = [
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.StructDeclaration
        ];

        public static readonly HashSet<string> NO_FULL_QUALIFICATION_TYPES = [
            "System",
            "Roblox",
            "Roblox.Globals",
            "Roblox.PluginClasses"
        ];

        public static readonly HashSet<string> IGNORED_BINARY_OPERATORS = [
            "as"
        ];

        public static readonly Dictionary<List<string>, (string, string)> PER_TYPE_BINARY_OPERATOR_MAP = new Dictionary<List<string>, (string, string)>
        {
            { ["String", "string"], ("+", "..") }
        };

        public static readonly HashSet<string> DECIMAL_TYPES = new HashSet<string>
        {
            "float",
            "double",
            "Single",
            "Double"
        };

        public static readonly HashSet<string> INTEGER_TYPES = new HashSet<string>
        {
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
        };
    }
}
