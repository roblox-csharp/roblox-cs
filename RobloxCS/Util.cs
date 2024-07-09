using Microsoft.CodeAnalysis;
using System.Reflection;

namespace RobloxCS
{
    internal static class Util
    {
        public const string RuntimeAssemblyName = "RobloxRuntime";

        public static string FormatLocation(FileLinePositionSpan lineSpan)
        {
            var filePath = lineSpan.Path;
            return $"- {(filePath == "" ? "<anonymous>" : filePath)}:{lineSpan.StartLinePosition.Line + 1}:{lineSpan.StartLinePosition.Character + 1}";
        }

        public static string? FixPathSep(string? path)
        {
            return path?.Replace('\\', '/');
        }

        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public static List<PortableExecutableReference> GetCoreLibReferences()
        {
            var coreLib = typeof(object).GetTypeInfo().Assembly.Location;
            var systemRuntime = Path.Combine(Path.GetDirectoryName(coreLib)!, "System.Runtime.dll");
            return new List<PortableExecutableReference>
            {
                MetadataReference.CreateFromFile(coreLib),
                MetadataReference.CreateFromFile(systemRuntime)
            };
        }

        public static string? GetRbxcsDirectory()
        {
            return FixPathSep(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Util.GetAssemblyDirectory()))))); // pretend like this isn't here lol
        }

        public static string? GetAssemblyDirectory()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var uri = new UriBuilder(location);
            var path = Uri.UnescapeDataString(uri.Path);
            return FixPathSep(Path.GetDirectoryName(path));
        }

        public static string GetTargetFramework()
        {
            var assemblyDirectory = GetAssemblyDirectory();
            if (assemblyDirectory == null)
            {
                Logger.Error("Failed to find RobloxCS assembly directory!");
            }

            return assemblyDirectory!.Split('/').Last();
        }
    }
}
