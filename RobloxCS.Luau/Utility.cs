﻿using Microsoft.CodeAnalysis;
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

        public static List<string> ExtractTypeArguments(string input)
        {
            var typeArguments = new List<string>();
            var regex = new Regex(@"<(?<args>[^<>]+)>");
            var match = regex.Match(input);
            if (match.Success)
            {
                var args = match.Groups["args"].Value;
                var argsArray = args.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var arg in argsArray)
                {
                    typeArguments.Add(arg.Trim());
                }
            }

            return typeArguments;
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
    }
}