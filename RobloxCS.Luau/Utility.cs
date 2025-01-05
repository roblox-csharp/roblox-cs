using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;
using static RobloxCS.Luau.Constants;

namespace RobloxCS.Luau
{
    public static class Utility
    {
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
            if (csharpType.StartsWith("Action<") || csharpType == "Action")
            {
                var typeArgs = ExtractTypeArguments(csharpType).Select(GetMappedType);
                return $"({string.Join(", ", typeArgs)}) -> nil";
            }
            if (csharpType.StartsWith("Func<"))
            {
                var typeArgs = ExtractTypeArguments(csharpType).Select(GetMappedType);
                var returnType = typeArgs.Last();
                typeArgs = typeArgs.SkipLast(1).ToList();
                return $"({string.Join(", ", typeArgs)}) -> {returnType}";
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
                    if (INTEGER_TYPES.Contains(csharpType))
                    {
                        return "number";
                    }
                    return csharpType;
            }
        }

        public static string? GetBit32MethodName(string bitOp)
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
            }
            return null;
        }

        public static string GetMappedOperator(string op)
        {
            switch (op)
            {
                case "++":
                    return "+=";
                case "--":
                    return "-=";
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
        
        public static bool IsFromSystemNamespace(ISymbol? typeSymbol)
        {
            if (typeSymbol is not { ContainingNamespace: not null })
                return false;
            
            return typeSymbol.ContainingNamespace.Name == "System" || IsFromSystemNamespace(typeSymbol.ContainingNamespace);
        }
        
        public static List<string> ExtractTypeArguments(string input)
        {
            var match = Regex.Match(input, @"<([^>]+)>");
            if (!match.Success)
                return [];
            
            var arguments = match.Groups[1].Value.Split(',');
            return arguments.Select(arg => arg.Trim()).ToList();
        }

        public static List<string> GetNamesFromNode(SyntaxNode? node)
        {
            if (node is BaseExpressionSyntax baseExpression)
                return [""];

            List<string> names = [];
            if (node == null)
                return names;

            List<string> addGenerics(List<string> currentNames)
            {
                var typeParametersProperty = node.GetType().GetProperty("TypeParameterList");
                var typeParametersValue = typeParametersProperty?.GetValue(node);
                if (typeParametersProperty != null && typeParametersValue is TypeParameterListSyntax typeParameterList)
                    return currentNames.Append('<' + string.Join(", ", typeParameterList.Parameters.Select(p => GetNamesFromNode(p).First())) + '>').ToList();
                
                return currentNames;
            }
            
            var nameProperty = node.GetType().GetProperty("Name");
            var nameValue = nameProperty?.GetValue(node);
            if (nameProperty != null && nameValue is NameSyntax nameNode)
                return GetNamesFromNode(nameNode);

            var identifierProperty = node.GetType().GetProperty("Identifier");
            var identifierValue = identifierProperty?.GetValue(node);
            if (identifierProperty != null && identifierValue is SyntaxToken token)
            {
                names.Add(token.ValueText.Trim());
                return addGenerics(names);
            }

            var childNodes = node.ChildNodes().ToList();
            var qualifiedNameNodes = node is QualifiedNameSyntax qualifiedName
                ? [qualifiedName]
                : childNodes.OfType<QualifiedNameSyntax>();
            var simpleNameNodes = node is SimpleNameSyntax simpleName
                ? [simpleName]
                : childNodes.OfType<SimpleNameSyntax>();
            
            foreach (var qualifiedNameNode in qualifiedNameNodes)
            {
                names.AddRange(GetNamesFromNode(qualifiedNameNode.Left).Select(name => name.Trim()));
                names.AddRange(GetNamesFromNode(qualifiedNameNode.Right).Select(name => name.Trim()));
            }

            names.AddRange(simpleNameNodes.Select(simpleNameNode => simpleNameNode.ToString().Trim()));
            return addGenerics(names);
        }
    }
}