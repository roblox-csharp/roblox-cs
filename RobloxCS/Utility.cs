using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    public static class Utility
    {
        public const string RuntimeAssemblyName = "Roblox";
        public const string LuaRuntimeModuleName = "RuntimeLib";

        public static string GetMappedType(string csharpType)
        {
            if (csharpType.EndsWith("[]"))
            {
                var arrayType = csharpType.Substring(0, csharpType.Length - 2);
                return $"{{ {GetMappedType(arrayType)} }}";
            }
            if (csharpType.EndsWith('?'))
            {
                var nonNullableType = csharpType.Substring(0, csharpType.Length - 1);
                return $"{GetMappedType(nonNullableType)}?";
            }

            switch (csharpType)
            {
                case "object":
                    return "any";

                case "void":
                case "null":
                    return "nil";

                case "char":
                case "Char":
                case "String":
                    return "string";
                case "double":
                case "float":
                    return "number";

                default:
                    if (Constants.INTEGER_TYPES.Contains(csharpType))
                    {
                        return "number";
                    }
                    return csharpType;
            }
        }



        public static string FormatLocation(FileLinePositionSpan lineSpan)
        {
            var filePath = lineSpan.Path;
            return $"{(filePath == "" ? "<anonymous>" : filePath)}:{lineSpan.StartLinePosition.Line + 1}:{lineSpan.StartLinePosition.Character + 1}";
        }

        public static ISymbol? FindMember(INamespaceSymbol namespaceSymbol, string memberName)
        {
            return namespaceSymbol.GetMembers().FirstOrDefault(member => member.Name == memberName);
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

        public static string GetBit32MethodName(string bitOp)
        {
            switch (bitOp)
            {
                case "&=":
                case "&":
                    return "band";
                case "|=":
                case "|":
                    return "bor";
                case "^=":
                case "^":
                    return "bxor";
                case ">>=":
                case ">>":
                    return "rshift";
                case ">>>=":
                case ">>>":
                    return "arshift";
                case "<<=":
                case "<<":
                    return "lshift";
                case "~":
                    return "bnot";
                default:
                    return bitOp;
            }
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

        public static string FixPathSep(string path)
        {
            return Path.TrimEndingDirectorySeparator(path).Replace("\\\\", "/").Replace('\\', '/').Replace("//", "/").Replace("./", "");
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
            var directoryName = Path.GetDirectoryName(GetAssemblyDirectory()); // pretend like this isn't here lol
            return directoryName == null ? null : FixPathSep(directoryName);
        }

        public static string GetAssemblyDirectory()
        {
            var location = FixPathSep(Assembly.GetExecutingAssembly().Location);
            var directoryName = Path.GetDirectoryName(location)!;
            return FixPathSep(directoryName);
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

        public static List<T> FilterDuplicates<T>(IEnumerable<T> items, IEqualityComparer<T> comparer)
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