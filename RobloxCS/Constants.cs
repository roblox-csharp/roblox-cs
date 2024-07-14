namespace RobloxCS
{
    internal static class Constants
    {
        public static readonly HashSet<string> NO_FULL_QUALIFICATION_TYPES = new HashSet<string>
        {
            "System",
            "RobloxRuntime",
            "RobloxRuntime.Classes",
            "RobloxRuntime.PluginClasses",
            "RobloxRuntime.Globals"
        };

        public static readonly HashSet<string> IGNORED_BINARY_OPERATORS = new HashSet<string>
        {
            "as"
        };

        public static readonly Dictionary<List<string>, (string, string)> PER_TYPE_BINARY_OPERATOR_MAP = new Dictionary<List<string>, (string, string)>
        {
            { ["String", "string"], ("+", "..") }
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
