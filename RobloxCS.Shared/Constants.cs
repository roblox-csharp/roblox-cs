using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS.Shared;

public static class Constants
{
    public const string IncludeFolderName = "Include";

    public static readonly HashSet<string> UNSUPPORTED_BITWISE_TYPES = ["UInt128", "ulong", "long", "Int128"];

    public static readonly HashSet<string> LENGTH_READABLE_TYPES = ["String", "string", "Array"];

    public static readonly HashSet<SyntaxKind> MEMBER_PARENT_SYNTAXES =
    [
        SyntaxKind.NamespaceDeclaration, SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.StructDeclaration
    ];

    public static readonly HashSet<string> GLOBAL_LIBRARIES =
    [
        "task",
        "math",
        "table",
        "os",
        "buffer",
        "coroutine",
        "utf8",
        "debug"
    ];

    public static readonly HashSet<string> METAMETHODS =
    [
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
        "__metatable"
    ];

    public static readonly HashSet<string> LUAU_KEYWORDS =
    [
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

    public static readonly HashSet<string> RESERVED_IDENTIFIERS = ["CS", "next", ..LUAU_KEYWORDS];

    public static readonly HashSet<string> DECIMAL_TYPES = ["float", "double", "Single", "Double"];

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
        "UInt128"
    ];
}