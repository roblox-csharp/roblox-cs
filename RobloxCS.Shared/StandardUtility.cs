using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static RobloxCS.Shared.Constants;

namespace RobloxCS.Shared;

public static class StandardUtility
{
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

    public static bool DoesTypeInheritFrom(ITypeSymbol? symbol, string typeName)
    {
        if (symbol == null)
            return false;
        
        return symbol.BaseType != null
            ? symbol.Name == typeName || symbol.BaseType.Name == typeName || DoesTypeInheritFrom(symbol.BaseType, typeName)
            : symbol.Name == typeName;
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

        return csharpType switch
        {
            "object" => "any",
            "void" or "null" => "nil",
            "char" or "Char" or "String" => "string",
            "double" or "float" => "number",
            "bool" or "Boolean" => "boolean",
            _ => INTEGER_TYPES.Contains(csharpType) ? "number" : csharpType
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