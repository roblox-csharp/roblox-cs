using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static RobloxCS.Constants;

namespace RobloxCS
{
    public static class Utility
    {
        public const string RuntimeAssemblyName = "Roblox";

        public static string GetDefaultValueForType(string typeName)
        {
            if (INTEGER_TYPES.Contains(typeName) || DECIMAL_TYPES.Contains(typeName))
            {
                return "0";
            }

            switch (typeName)
            {
                case "char":
                case "Char":
                case "string":
                case "String":
                    return "\"\"";
                case "bool":
                case "Boolean":
                    return "false";
                default:
                    return "nil";
            }
        }

        public static ISymbol? FindMember(INamespaceSymbol namespaceSymbol, string memberName)
        {
            var member = namespaceSymbol.GetMembers().FirstOrDefault<ISymbol?>(member => member?.Name == memberName, null);
            if (member == null && namespaceSymbol.ContainingNamespace != null)
            {
                member = FindMember(namespaceSymbol.ContainingNamespace, memberName);
            }
            return member;
        }

        public static ISymbol? FindMemberDeep(INamedTypeSymbol namedTypeSymbol, string memberName)
        {
            var member = namedTypeSymbol.GetMembers().FirstOrDefault(member => member.Name == memberName);
            if (namedTypeSymbol.BaseType != null && member == null)
            {
                return FindMemberDeep(namedTypeSymbol.BaseType, memberName);
            }
            return member;
        }

        public static List<string> GetNamesFromNode(SyntaxNode? node)
        {
            if (node is BaseExpressionSyntax baseExpression)
            {
                return [""];
            }

            var names = new List<string>();
            if (node == null) return names;

            var identifierProperty = node.GetType().GetProperty("Identifier");
            var identifierValue = identifierProperty?.GetValue(node);
            if (identifierProperty != null && identifierValue != null && identifierValue is SyntaxToken)
            {
                names.Add(((SyntaxToken)identifierValue).ValueText.Trim());
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
                names.Add(identifierNameNode.Identifier.ValueText.Trim());
            }

            return names;
        }

        public static void PrettyPrint(object? obj)
        {
            if (obj == null)
            {
                Console.WriteLine("null");
                return;
            }

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = $"{type.Name}:\n";

            foreach (var property in properties)
            {
                var value = property.GetValue(obj, null);
                result += $"  {property.Name}: {value}\n";
            }

            Console.WriteLine(result);
        }

        public static string FixPathSep(string path)
        {
            path = Path.TrimEndingDirectorySeparator(path);
            return Regex.Replace(path.Replace("\\\\", "/").Replace('\\', '/').Replace("//", "/"), @"(?<!\.)\./", "");
        }

        public static string? GetRbxcsDirectory()
        {
            var directoryName = Path.GetDirectoryName(GetAssemblyDirectory()); // pretend like this isn't here lol
            return directoryName == null ? null : FixPathSep(directoryName);
        }

        public static string GetAssemblyDirectory()
        {
            var location = FixPathSep(Assembly.GetExecutingAssembly().Location);
            var directoryName = Path.GetDirectoryName(location)!;
            return FixPathSep(directoryName);
        }

        public static List<T> FilterDuplicates<T>(IEnumerable<T> items, IEqualityComparer<T> comparer) where T : notnull
        {
            var seen = new Dictionary<T, bool>(comparer);
            var result = new List<T>();
            foreach (var item in items)
            {
                if (!seen.ContainsKey(item))
                {
                    seen[item] = true;
                    result.Add(item);
                }
            }

            return result;
        }
    }
}