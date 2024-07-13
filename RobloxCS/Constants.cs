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
    }
}
