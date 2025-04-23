using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static RobloxCS.Shared.Constants;

namespace RobloxCS.Shared;

public static class StandardUtility
{
    public class KeyValuePairEqualityComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
    {
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly IEqualityComparer<TValue> _valueComparer;

        public KeyValuePairEqualityComparer(
            IEqualityComparer<TKey> keyComparer = null!,
            IEqualityComparer<TValue> valueComparer = null!)
        {
            _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            _valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }

        public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
        {
            return _keyComparer.Equals(x.Key, y.Key) && _valueComparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(KeyValuePair<TKey, TValue> obj)
        {
            var hashKey = _keyComparer.GetHashCode(obj.Key!);
            var hashValue = _valueComparer.GetHashCode(obj.Value!);
            return hashKey ^ hashValue;
        }
    }
    
    public static Type GetRuntimeType(SemanticModel semanticModel, SyntaxNode node, ITypeSymbol typeSymbol)
    {
        var fullyQualifiedName = GetFullSymbolName(typeSymbol);
            
        Type? type;
        using (var memoryStream = new MemoryStream())
        {
            semanticModel.Compilation.Emit(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(memoryStream.ToArray());

            // get the type from the loaded assembly
            type = assembly.GetType(fullyQualifiedName);
        }

        type ??= Type.GetType(fullyQualifiedName);
        if (type == null)
            throw Logger.CodegenError(node, $"[GetRuntimeType()]: Unable to resolve type '{fullyQualifiedName}'.");

        return type;
    }
    
    public static string GetFullSymbolName(ISymbol symbol)
    {
        var containerName = symbol.ContainingNamespace != null || symbol.ContainingType != null
            ? GetFullSymbolName(symbol.ContainingNamespace ?? (ISymbol)symbol.ContainingType)
            : null;
        
        return (!string.IsNullOrEmpty(containerName) ? containerName + "." : "") + symbol.Name;
    }

    public static bool DoesTypeInheritFrom(ITypeSymbol? derived, string typeName)
    {
        if (derived == null)
            return false;
        
        return derived.BaseType != null
            ? derived.Name == typeName || derived.BaseType.Name == typeName || DoesTypeInheritFrom(derived.BaseType, typeName)
            : derived.Name == typeName;
    }
    
    public static bool DoesTypeInheritFrom(ITypeSymbol derived, ITypeSymbol baseType)
    {
        var current = derived;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;

            current = current.BaseType;
        }

        return false;
    }
    
    public static string GetDefaultValueForType(string typeName)
    {
        if (INTEGER_TYPES.Contains(typeName) || DECIMAL_TYPES.Contains(typeName))
            return "0";

        return typeName switch
        {
            "char" or "Char" or "string" or "String" => "\"\"",
            "bool" or "Boolean" => "false",
            _ => "nil"
        };
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

    public static List<T> FilterDuplicates<T>(IEnumerable<T> items, IEqualityComparer<T> comparer) where T : notnull
    {
        var seen = new Dictionary<T, bool>(comparer);
        return items.Where(item => seen.TryAdd(item, true)).ToList();
    }
        
    public static string GetMappedType(string csharpType)
    {
        if (csharpType.EndsWith("[]"))
        {
            var arrayType = csharpType[..^2];
            return $"{{ {GetMappedType(arrayType)} }}";
        }
        if (csharpType.EndsWith('?'))
        {
            var nonNullableType = csharpType[..^1];
            return $"{GetMappedType(nonNullableType)}?";
        }
        if (csharpType.StartsWith("Action<") || csharpType == "Action")
        {
            var typeArgs = ExtractTypeArguments(csharpType).ConvertAll(GetMappedType);
            return $"({string.Join(", ", typeArgs)}) -> nil";
        }
        if (csharpType.StartsWith("Func<"))
        {
            var typeArgs = ExtractTypeArguments(csharpType).ConvertAll(GetMappedType);
            var returnType = typeArgs.Last();
            typeArgs = typeArgs.SkipLast(1).ToList();
            
            return $"({string.Join(", ", typeArgs)}) -> {returnType}";
        }
        if (csharpType.StartsWith("Dictionary<"))
        {
            var typeArgs = ExtractTypeArguments(csharpType).ConvertAll(GetMappedType);
            var keyType = typeArgs.First();
            var valueType = typeArgs.Last();
            
            return $"{{ [{keyType}]: {valueType} }}";
        }

        if (csharpType.StartsWith("Roblox.Enum"))
            return GetMappedType(csharpType.Replace("Roblox.Enum", "Enum"));

        return csharpType switch
        {
            "Object" or "object" or "dynamic" or "Type" => "any",
            "void" or "Void" => "()",
            "null" => "nil",
            "char" or "Char" or "String" => "string",
            "Boolean" or "bool" => "boolean",
            "System.Index" or "Index" => "number",
            "Roblox.Buffer" or "Buffer" => "buffer",
            _ => INTEGER_TYPES.Contains(csharpType) || DECIMAL_TYPES.Contains(csharpType)
                ? "number"
                : csharpType
        };
    }

    public static string? GetBit32MethodName(string bitOp)
    {
        return bitOp switch
        {
            "&=" or "&" => "band",
            "|=" or "|" => "bor",
            "^=" or "^" => "bxor",
            ">>=" or ">>" => "rshift",
            ">>>=" or ">>>" => "arshift",
            "<<=" or "<<" => "lshift",
            "~" => "bnot",
            _ => null
        };
    }

    public static string GetMappedOperator(string op)
    {
        return op switch
        {
            "++" => "+=",
            "--" => "-=",
            "!" => "not ",
            "!=" => "~=",
            "&&" => "and",
            "||" => "or",
            _ => op
        };
    }
        
    public static bool IsFromSystemNamespace(ISymbol? typeSymbol)
    {
        if (typeSymbol is not { ContainingNamespace: not null })
            return false;
            
        return typeSymbol.ContainingNamespace.Name == "System" || IsFromSystemNamespace(typeSymbol.ContainingNamespace);
    }
        
    public static List<string> ExtractTypeArguments(string input)
    {
        var match = Regex.Match(input, "<(?<open>(?:[^<>]+|<(?<open>)|>(?<-open>))*)>");
        if (!match.Success)
            return [];

        var argumentsRaw = match.Groups[1].Value;
        var arguments = SplitGenericArguments(argumentsRaw);
        return arguments.Select(arg => arg.Trim()).ToList();
    }

    private static List<string> SplitGenericArguments(string input)
    {
        var args = new List<string>();
        var depth = 0;
        var lastSplit = 0;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            switch (c)
            {
                case '<':
                    depth++;
                    break;
                case '>':
                    depth--;
                    break;
                case ',' when depth == 0:
                    args.Add(input.Substring(lastSplit, i - lastSplit));
                    lastSplit = i + 1;
                    break;
            }
        }

        args.Add(input[lastSplit..]);
        return args;
    }
    
    public static bool IsGlobal(SyntaxNode node) =>
        node.Parent.IsKind(SyntaxKind.GlobalStatement) || node.Parent.IsKind(SyntaxKind.CompilationUnit);

    public static NameSyntax GetNameNode(List<string> pieces)
    {
        if (pieces.Count <= 1)
            return SyntaxFactory.IdentifierName(pieces.FirstOrDefault() ?? "");
        
        var left = GetNameNode(pieces.SkipLast(1).ToList());
        var right = SyntaxFactory.IdentifierName(pieces.Last());
        return SyntaxFactory.QualifiedName(left, right);
    }

    public static List<string> GetNamesFromNode(SyntaxNode? node, bool noGenerics = false)
    {
        if (node is BaseExpressionSyntax)
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
            return noGenerics ? names : addGenerics(names);
        }

        var childNodes = node.ChildNodes().ToList();
        var qualifiedNameNodes = (node is QualifiedNameSyntax qualifiedName
            ? [qualifiedName]
            : childNodes.OfType<QualifiedNameSyntax>()).ToList();
        var simpleNameNodes = (node is SimpleNameSyntax simpleName
            ? [simpleName]
            : childNodes.OfType<SimpleNameSyntax>()).ToList();
        
        if (simpleNameNodes.Count <= 1)
            foreach (var qualifiedNameNode in qualifiedNameNodes)
            {
                names.AddRange(GetNamesFromNode(qualifiedNameNode.Left).Select(name => name.Trim()));
                names.AddRange(GetNamesFromNode(qualifiedNameNode.Right).Select(name => name.Trim()));
            }

        if (qualifiedNameNodes.Count <= 1)
            names.AddRange(simpleNameNodes.Select(simpleNameNode => simpleNameNode.ToString().Trim()));
        
        return noGenerics ? names : addGenerics(names);
    }
}