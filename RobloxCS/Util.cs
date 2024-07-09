using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        public static string GetMappedOperator(string op)
        {
            switch (op)
            {
                default: return op;
            }
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

        public static void PrintChildNodes(SyntaxNode node)
        {
            Logger.Info($"{node.Kind()} node children: {node.ChildNodes().Count()}");
            foreach (var child in node.ChildNodes())
            {
                Logger.Info(child.Kind().ToString() + ": " + child.GetText());
            }
        }

        public static void PrintChildTokens(SyntaxNode node)
        {
            Logger.Info($"{node.Kind()} token children: {node.ChildTokens().Count()}");
            foreach (var child in node.ChildTokens())
            {
                Logger.Info(child.Kind().ToString() + ": " + child.Text);
            }
        }
    }
}
