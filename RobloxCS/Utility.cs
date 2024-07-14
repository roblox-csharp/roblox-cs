using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    public static class Utility
    {
        public const string RuntimeAssemblyName = "RobloxRuntime";

        public static string FormatLocation(FileLinePositionSpan lineSpan)
        {
            var filePath = lineSpan.Path;
            return $"{(filePath == "" ? "<anonymous>" : filePath)}:{lineSpan.StartLinePosition.Line + 1}:{lineSpan.StartLinePosition.Character + 1}";
        }

        public static List<string> GetNamesFromNode(SyntaxNode? node)
        {
            var names = new List<string>();
            if (node == null) return names;

            var identifierProperty = node.GetType().GetProperty("Identifier");
            var identifierValue = identifierProperty?.GetValue(node);
            if (identifierProperty != null && identifierValue != null && identifierValue is SyntaxToken)
            {
                names.Add(((SyntaxToken)identifierValue).Text.Trim());
                return names;
            }

            var childNodes = node.ChildNodes();
            var qualifiedNameNodes = node.IsKind(SyntaxKind.QualifiedName) ? [(QualifiedNameSyntax)node] : childNodes.OfType<QualifiedNameSyntax>();
            var identifierNameNodes = node.IsKind(SyntaxKind.IdentifierName) ? [(IdentifierNameSyntax)node] : childNodes.OfType<IdentifierNameSyntax>();
            foreach (var qualifiedNameNode in qualifiedNameNodes)
            {
                foreach (var name in GetNamesFromNode(qualifiedNameNode.Left))
                {
                    names.Add(name.Trim());
                }
                foreach (var name in GetNamesFromNode(qualifiedNameNode.Right))
                {
                    names.Add(name.Trim());
                }
            }

            foreach (var identifierNameNode in identifierNameNodes)
            {
                names.Add(identifierNameNode.Identifier.Text.Trim());
            }

            return names;
        }

        public static string GetMappedOperator(string op)
        {
            switch (op)
            {
                case "++":
                    return "+=";
                case "--":
                    return "--";
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