using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Reflection;

namespace RobloxCS
{
    public static class Utility
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
                case "!":
                    return "not ";
                case "!=":
                    return "~=";
                case "&&":
                    return "and";
                case "||":
                    return "or";
                default:
                    return op;
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
            return FixPathSep(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Utility.GetAssemblyDirectory()))))); // pretend like this isn't here lol
        }

        public static string? GetAssemblyDirectory()
        {
            var location = FixPathSep(Assembly.GetExecutingAssembly().Location);
            return FixPathSep(Path.GetDirectoryName(location));
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