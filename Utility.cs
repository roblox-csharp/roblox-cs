using System.Reflection;
using System.Text.RegularExpressions;
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
            if (csharpType.StartsWith("Dictionary<") || csharpType.StartsWith("IDictionary<"))
            {
                var typeArgs = ExtractTypeArguments(csharpType);
                var keyType = typeArgs[0];
                var valueType = typeArgs[1];
                return $"{{ [{GetMappedType(keyType)}]: {GetMappedType(valueType)} }}";
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

        public static List<string> ExtractTypeArguments(string input)
        {
            var typeArguments = new List<string>();
            var regex = new Regex(@"<(?<args>[^<>]+)>");
            var match = regex.Match(input);
            if (match.Success)
            {
                // Get the matched group containing the type arguments
                var args = match.Groups["args"].Value;

                // Split the arguments by comma and trim whitespace
                var argsArray = args.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var arg in argsArray)
                {
                    typeArguments.Add(arg.Trim());
                }
            }

            return typeArguments;
        }

        public static string FormatLocation(FileLinePositionSpan lineSpan)
        {
            return $"{(lineSpan.Path == "" ? "<anonymous>" : lineSpan.Path)}:{lineSpan.StartLinePosition.Line + 1}:{lineSpan.StartLinePosition.Character + 1}";
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