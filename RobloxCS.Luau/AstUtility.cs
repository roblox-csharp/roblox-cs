using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Shared;
using static RobloxCS.Shared.Constants;

namespace RobloxCS.Luau;

public static class AstUtility
{
    public static readonly IdentifierName DiscardName = new("_");

    public static IdentifierName Vararg { get; } = new("...");

    public static Literal False { get; } = new("false");

    public static Literal True { get; } = new("true");

    public static Literal Nil { get; } = new("nil");

    public static TypeRef AnyType { get; } = new("any");

    /// <summary>Adds one to the expression</summary>
    public static Expression AddOne(Expression expression) =>
        expression is Literal literal && int.TryParse(literal.ValueText, out var value)
            ? new Literal((value + 1).ToString())
            : new BinaryOperator(expression, "+", new Literal("1"));

    /// <summary>Subtracts one from the expression</summary>
    public static Expression SubtractOne(Expression expression) =>
        expression is Literal literal && int.TryParse(literal.ValueText, out var value)
            ? new Literal((value - 1).ToString())
            : new BinaryOperator(expression, "-", new Literal("1"));

    /// <summary> Creates type info table for runtime type objects</summary>
    public static TableInitializer CreateTypeInfo(Type type, AnalysisResult analysisResult, bool noAttributes = false)
    {
        var memberUses = analysisResult.TypeClassInfo.MemberUses
                                .Where(name => !noAttributes || name != "CustomAttributes")
                                .ToList();
        
        var keys = memberUses.Select(name => new IdentifierName(name)).ToList<Expression>();
        var values = memberUses.Select(name => GetTypeInfoMember(type, analysisResult, name)).ToList();
        return new TableInitializer(values, keys);
    }

    private static Expression GetTypeInfoMember(Type type, AnalysisResult analysisResult, string name) =>
        GetMemberInfoMember(type, analysisResult, name)
        ?? name switch
        {
            "AssemblyQualifiedName" => type.AssemblyQualifiedName != null ? String(type.AssemblyQualifiedName) : Nil,
            "Namespace" => type.Namespace != null ? String(type.Namespace) : Nil,
            "FullName" => type.FullName != null ? String(type.FullName) : Nil,
            "Attributes" => Number((int)type.Attributes),
            "GenericParameterAttributes" => Number((int)type.GenericParameterAttributes),
            "Assembly" => CreateAssemblyInfo(type.Assembly, analysisResult),
            "ContainsGenericParameters" => Bool(type.ContainsGenericParameters),
            "HasElementType" => Bool(type.HasElementType),
            "IsAbstract" => Bool(type.IsAbstract),
            "IsArray" => Bool(type.IsArray),
            "IsAnsiClass" => Bool(type.IsAnsiClass),
            "IsAutoClass" => Bool(type.IsAutoClass),
            "IsAutoLayout" => Bool(type.IsAutoLayout),
            "IsByRef" => Bool(type.IsByRef),
            "IsByRefLike" => Bool(type.IsByRefLike),
            "IsClass" => Bool(type.IsClass),
            "IsContextful" => Bool(type.IsContextful),
            "IsConstructedGenericType" => Bool(type.IsConstructedGenericType),
            "IsCOMObject" => Bool(type.IsCOMObject),
            "IsEnum" => Bool(type.IsEnum),
            "IsExplicitLayout" => Bool(type.IsExplicitLayout),
            "IsFunctionPointer" => Bool(type.IsFunctionPointer),
            "IsGenericParameter" => Bool(type.IsGenericParameter),
            "IsGenericType" => Bool(type.IsGenericType),
            "IsGenericMethodParameter" => Bool(type.IsGenericMethodParameter),
            "IsGenericTypeDefinition" => Bool(type.IsGenericTypeDefinition),
            "IsGenericTypeParameter" => Bool(type.IsGenericTypeParameter),
            "IsImport" => Bool(type.IsImport),
            "IsInterface" => Bool(type.IsInterface),
            "IsLayoutSequential" => Bool(type.IsLayoutSequential),
            "IsMarshalByRef" => Bool(type.IsMarshalByRef),
            "IsNestedAssembly" => Bool(type.IsNestedAssembly),
            "IsNested" => Bool(type.IsNested),
            "IsNestedFamily" => Bool(type.IsNestedFamily),
            "IsNestedPrivate" => Bool(type.IsNestedPrivate),
            "IsNestedPublic" => Bool(type.IsNestedPublic),
            "IsNotPublic" => Bool(type.IsNotPublic),
            "IsNestedFamORAssem" => Bool(type.IsNestedFamORAssem),
            "IsNestedFamANDAssem" => Bool(type.IsNestedFamANDAssem),
            "IsPointer" => Bool(type.IsPointer),
            "IsPrimitive" => Bool(type.IsPrimitive),
            "IsPublic" => Bool(type.IsPublic),
            "IsSealed" => Bool(type.IsSealed),
            "IsSecurityCritical" => Bool(type.IsSecurityCritical),
            "IsSecurityTransparent" => Bool(type.IsSecurityTransparent),
            "IsSignatureType" => Bool(type.IsSignatureType),
            "IsSpecialName" => Bool(type.IsSpecialName),
            "IsSecuritySafeCritical" => Bool(type.IsSecuritySafeCritical),
            "IsSZArray" => Bool(type.IsSZArray),
            "IsTypeDefinition" => Bool(type.IsTypeDefinition),
            "IsUnicodeClass" => Bool(type.IsUnicodeClass),
            "IsUnmanagedFunctionPointer" => Bool(type.IsUnmanagedFunctionPointer),
            "IsVisible" => Bool(type.IsVisible),
            "IsValueType" => Bool(type.IsValueType),
            "IsVariableBoundArray" => Bool(type.IsVariableBoundArray),

            "GetProperties" => new AnonymousFunction(new ParameterList([new Parameter(new IdentifierName("self"))]),
                null,
                new Block([
                    new Return(new TableInitializer(type.GetProperties()
                        .Select(p => CreatePropertyInfo(p, analysisResult))
                        .ToList<Expression>()))
                ])),
            "GetArrayRank" => new AnonymousFunction(ParameterList.Empty,
                new TypeRef("number"),
                new Block([new Return(Number(type.GetArrayRank()))])),

            _ => throw Logger.CompilerError($"Member '{name}' is not yet supported on the Type class")
        };

    /// <summary>Creates assembly info table for runtime type objects</summary>
    private static TableInitializer CreateAssemblyInfo(Assembly assembly, AnalysisResult analysisResult)
    {
        var keys = analysisResult.AssemblyClassInfo.MemberUses.Select(name => new IdentifierName(name)).ToList<Expression>();
        var values = analysisResult.AssemblyClassInfo.MemberUses.Select(name => GetAssemblyInfoMember(assembly, analysisResult, name)).ToList();

        return new TableInitializer(values, keys);
    }

    private static Expression GetAssemblyInfoMember(Assembly assembly, AnalysisResult analysisResult, string name) =>
        name switch
        {
            "ImageRuntimeVersion" => String(assembly.ImageRuntimeVersion),
            "HostContext" => Number(assembly.HostContext),
            "Location" => String(assembly.Location),
            "FullName" => assembly.FullName != null ? String(assembly.FullName) : Nil,
            "SecurityRuleSet" => Number((byte)assembly.SecurityRuleSet),
            "ReflectionOnly" => Bool(assembly.ReflectionOnly),
            "IsCollectible" => Bool(assembly.IsCollectible),
            "IsDynamic" => Bool(assembly.IsDynamic),
            "IsFullyTrusted" => Bool(assembly.IsFullyTrusted),
            "GetManifestResourceNames" => new AnonymousFunction(ParameterList.Empty,
                                                                new TypeRef("string"),
                                                                new Block([
                                                                    new Return(new TableInitializer(assembly.GetManifestResourceNames()
                                                                                                            .Select(String)
                                                                                                            .ToList<Expression>()))
                                                                ])),

            _ => throw Logger.CompilerError($"Member '{name}' is not yet supported on the Assembly class")
        };

    /// <summary>Creates property info table for runtime type objects</summary>
    private static TableInitializer CreatePropertyInfo(PropertyInfo property, AnalysisResult analysisResult)
    {
        var keys = analysisResult.PropertyClassInfo.MemberUses.Select(name => new IdentifierName(name)).ToList<Expression>();
        var values = analysisResult.PropertyClassInfo.MemberUses.Select(name => GetPropertyInfoMember(property, analysisResult, name)).ToList();

        return new TableInitializer(values, keys);
    }

    private static Expression GetPropertyInfoMember(PropertyInfo property, AnalysisResult analysisResult, string name) =>
        GetMemberInfoMember(property, analysisResult, name)
     ?? name switch
        {
            "Attributes" => Number((int)property.Attributes),

            _ => throw Logger.CompilerError($"Member '{name}' is not yet supported on the PropertyInfo class")
        };

    /// <summary>Creates member info table for runtime type objects</summary>
    private static TableInitializer CreateMemberInfo(MemberInfo member, AnalysisResult analysisResult)
    {
        var keys = analysisResult.MemberClassInfo.MemberUses.Select(name => new IdentifierName(name)).ToList<Expression>();
        var values = analysisResult.MemberClassInfo.MemberUses.Select(name => GetMemberInfoMember(member, analysisResult, name))
                                   .OfType<Expression>()
                                   .ToList();

        return new TableInitializer(values, keys);
    }

    private static Expression? GetMemberInfoMember(MemberInfo member, AnalysisResult analysisResult, string name) =>
        name switch
        {
            "Name" => String(member.Name),
            "MemberType" => new Literal(member.MemberType.ToString()),
            "MetadataToken" => new Literal(member.MetadataToken.ToString()),
            "CustomAttributes" => new TableInitializer(member.CustomAttributes.Select(attr => CreateCustomAttributeData(attr, analysisResult)).ToList<Expression>()),
            "IsAbstract" => Bool(member.IsCollectible),

            _ => null
        };

    private static TableInitializer CreateCustomAttributeData(CustomAttributeData data, AnalysisResult analysisResult)
    {
        var memberUses = analysisResult.CustomAttributeDataClassInfo.MemberUses;
        var keys = memberUses.Select(name => new IdentifierName(name)).ToList<Expression>();
        var values = memberUses.Select(name => GetCustomAttributeDataMember(data, analysisResult, name)).ToList();

        return new TableInitializer(values, keys);
    }

    private static Expression GetCustomAttributeDataMember(CustomAttributeData data,
                                                           AnalysisResult analysisResult,
                                                           string name) =>
        name switch
        {
            "AttributeType" => CreateTypeInfo(data.AttributeType, analysisResult, noAttributes: true),
            
            _ => throw Logger.CompilerError($"Member '{name}' is not yet supported on the CustomAttributeData class")
        };

    public static Expression CreateLuauConstant(object? value) =>
        value switch
        {
            null => Nil,
            bool b => b ? True : False,
            string or char => new Literal('"' + value.ToString() + '"'),
            _ => string.IsNullOrEmpty(value.ToString()) ? Nil : new Literal(value.ToString()!)
        };

    /// <summary><code>CS.defineGlobal(name, "name")</code> or <code>parentName.name = "name"</code></summary>
    public static Statement DefineGlobalOrMember(SyntaxNode node, SimpleName name)
    {
        if (StandardUtility.IsGlobal(node))
            return new ExpressionStatement(DefineGlobal(name, name));

        var fullParentName = GetFullParentName(node);
        if (fullParentName != null)
            return new Assignment(new MemberAccess(fullParentName, name),
                                  name);

        return new NoOp();
    }

    /// <code>CS.defineGlobal(name, value)</code>
    public static Call DefineGlobal(Name name, Expression type) => CSCall("defineGlobal", String(name.ToString()), type);

    /// <code>CS.getGlobal(name)</code>
    public static Call GetGlobal(Name name) => CSCall("getGlobal", String(name.ToString()));

    /// <code>CS.is(value, type)</code>
    public static Call Is(Expression value, Expression type) => CSCall("is", value, type);

    /// <summary>
    ///     Creates a call to a table library method
    /// </summary>
    public static Call TableCall(string methodName, params Expression[] arguments) =>
        new(new MemberAccess(new IdentifierName("table"),
                             new IdentifierName(methodName)),
            CreateArgumentList(arguments.ToList()));

    /// <summary>
    ///     Creates a call to a CS library method
    /// </summary>
    public static Call CSCall(string methodName, params Expression[] arguments) =>
        new(new MemberAccess(new IdentifierName("CS"),
                             new IdentifierName(methodName)),
            CreateArgumentList(arguments.ToList()));

    public static Call NewEnumerator(Expression items) =>
        new(new MemberAccess(new MemberAccess(new IdentifierName("CS"),
                                              new IdentifierName("Enumerator")),
                             new IdentifierName("new")),
            CreateArgumentList([items]));

    public static Call NewSignal() =>
        new(new QualifiedName(new IdentifierName("Signal"),
                              new IdentifierName("new")));

    public static Variable SignalImport() =>
        new(new IdentifierName("Signal"),
            true,

            // temporary until RojoReader
            RequireCall(new QualifiedName(new IdentifierName("rbxcs_include"),
                                          new IdentifierName("GoodSignal"))));

    public static Call RequireCall(Expression modulePath) =>
        new(new IdentifierName("require"),
            CreateArgumentList([modulePath]));

    public static Call PrintCall(params List<Expression> args) =>
        new(new IdentifierName("print"),
            CreateArgumentList(args));

    /// <summary>
    ///     Creates a call to a bit32 library method
    /// </summary>
    public static Call Bit32Call(string methodName, params Expression[] arguments) =>
        new(new MemberAccess(new IdentifierName("bit32"),
                             new IdentifierName(methodName)),
            CreateArgumentList(arguments.ToList()));

    public static ArgumentList CreateArgumentList(List<Expression> arguments) => new(arguments.ConvertAll(expression => new Argument(expression)));

    public static AnonymousFunction? TryWrapNonStaticMethod(IMethodSymbol methodSymbol, Expression expression, OccupiedIdentifiersStack occupiedIdentifiers) =>
        expression switch
        {
            MemberAccess memberAccess => WrapNonStaticMethod(methodSymbol, memberAccess, occupiedIdentifiers),
            QualifiedName qualifiedName => WrapNonStaticMethod(methodSymbol, qualifiedName, occupiedIdentifiers),
            _ => null
        };

    public static AnonymousFunction WrapNonStaticMethod(IMethodSymbol methodSymbol, MemberAccess memberAccess, OccupiedIdentifiersStack occupiedIdentifiers)
    {
        if (!methodSymbol.IsStatic)
            memberAccess = memberAccess.WithOperator(':');

        return CreateMethodWrapper(methodSymbol, memberAccess, occupiedIdentifiers);
    }

    public static AnonymousFunction WrapNonStaticMethod(IMethodSymbol methodSymbol, QualifiedName qualifiedName, OccupiedIdentifiersStack occupiedIdentifiers)
    {
        if (!methodSymbol.IsStatic)
            qualifiedName = qualifiedName.WithOperator(':');

        return CreateMethodWrapper(methodSymbol, qualifiedName, occupiedIdentifiers);
    }

    private static AnonymousFunction CreateMethodWrapper(IMethodSymbol methodSymbol, Expression callee, OccupiedIdentifiersStack occupiedIdentifiers)
    {
        var parameters = methodSymbol.Parameters.Select(p => ParameterFromSymbol(p, occupiedIdentifiers)).ToList();
        var returnType = CreateTypeRef(methodSymbol.ReturnType.Name);
        var typeParameters = methodSymbol.TypeParameters.Select(p => new IdentifierName(p.Name)).ToList();
        var arguments = parameters.ConvertAll<Expression>(p => p.Name);

        return new AnonymousFunction(new ParameterList(parameters),
                                     returnType,
                                     new Block([new Return(new Call(callee, CreateArgumentList(arguments)))]),
                                     null,
                                     typeParameters);
    }

    public static Parameter ParameterFromSymbol(IParameterSymbol symbol, OccupiedIdentifiersStack occupiedIdentifiers)
    {
        var defaultValue = symbol.HasExplicitDefaultValue ? CreateLuauConstant(symbol.ExplicitDefaultValue) : null;
        var name = occupiedIdentifiers.AddIdentifier(symbol.Name);
        var type = CreateTypeRef(symbol.Type.Name);

        return new Parameter(name, false, defaultValue, type);
    }

    public static SimpleName TypeNameFromSymbol(ISymbol symbol)
    {
        if (symbol is not INamedTypeSymbol { TypeParameters.Length: > 0 } namedTypeSymbol)
            return new IdentifierName(symbol.Name);

        var typeParameters = namedTypeSymbol.TypeParameters
                                            .Select(typeParameter => TypeNameFromSymbol(typeParameter).ToString())
                                            .ToList();

        return new GenericName(symbol.Name, typeParameters);
    }

    /// <summary>
    ///     Returns the full name of a C# node's parent.
    ///     This method is meant for getting the absolute location of classes, enums, etc.
    ///     For example a class under the namespace "Some.Namespace" would return a <see cref="RobloxCS.Luau.MemberAccess" />
    ///     that transpiles to "Some.Namespace".
    /// </summary>
    public static Expression? GetFullParentName(SyntaxNode node)
    {
        switch (node.Parent)
        {
            case null:
            case CompilationUnitSyntax:
                return null;
        }

        var parentName = CreateSimpleName(node.Parent);
        var parentLocation = GetFullParentName(node.Parent);

        return parentLocation == null
            ? node.Parent.SyntaxTree == node.SyntaxTree
                ? parentName
                : GetGlobal(parentName)
            : new MemberAccess(parentLocation, parentName);
    }

    /// <code>
    /// if name == nil then
    ///     name = initializer
    /// end
    /// </code>
    public static If DefaultValueInitializer(Name name, Expression initializer) =>
        new(new BinaryOperator(name, "==", Nil),
            new Block([new Assignment(name, initializer)]));

    /// <summary>
    ///     Takes a <see cref="RobloxCS.Luau.MemberAccess" /> and converts it into a <see cref="QualifiedName" />, given that
    ///     <see cref="RobloxCS.Luau.MemberAccess.Expression" /> inherits from <see cref="Name" />
    /// </summary>
    public static QualifiedName QualifiedNameFromMemberAccess(MemberAccess memberAccess)
    {
        var left = memberAccess.Expression is MemberAccess leftMemberAccess
            ? QualifiedNameFromMemberAccess(leftMemberAccess)
            : (Name)memberAccess.Expression;

        return new QualifiedName(left, memberAccess.Name);
    }

    /// <summary>
    ///     Creates a discard variable if <see cref="valueParent" /> is an <see cref="ExpressionStatementSyntax" />
    /// </summary>
    public static Node DiscardVariableIfExpressionStatement(SyntaxNode node, Node value, SyntaxNode? valueParent) =>
        valueParent?.FirstAncestorOrSelf<ExpressionStatementSyntax>() != null
            ? DiscardVariable(node, (Expression)value)
            : value;

    /// <code>local _ = discardedValue</code>
    public static Variable DiscardVariable(SyntaxNode node, Expression value) => new(DiscardName, true, value);

    public static GenericName? GetGenericName(Name name) =>
        name switch
        {
            GenericName baseName => baseName,
            QualifiedName { Right: GenericName rightName } => rightName,
            _ => null
        };

    /// <summary>
    ///     Takes a Name and converts it into a non-generic Name
    /// </summary>
    public static Name GetNonGenericName(Name name) =>
        name switch
        {
            QualifiedName qualifiedName => GetNonGenericName(qualifiedName),
            SimpleName simpleName => GetNonGenericName(simpleName),
            _ => name
        };

    /// <summary>
    ///     Takes a QualifiedName and converts it into a non-generic QualifiedName
    /// </summary>
    public static QualifiedName GetNonGenericName(QualifiedName qualifiedName)
    {
        if (qualifiedName.Right is IdentifierName) return qualifiedName;

        var right = GetNonGenericName(qualifiedName.Right);

        return new QualifiedName(qualifiedName.Left, right);
    }

    /// <summary>
    ///     Takes a SimpleName (which GenericName extends from) and converts it into a standard IdentifierName
    /// </summary>
    public static IdentifierName GetNonGenericName(SimpleName simpleName)
    {
        if (simpleName is IdentifierName identifierName) return identifierName;

        return new IdentifierName(simpleName is GenericName genericName
                                      ? genericName.Text
                                      : simpleName.ToString());
    }

    public static Name CreateName(SyntaxNode node, bool bypassReserved = false) =>
        CreateName(node, string.Join("", StandardUtility.GetNamesFromNode(node)), bypassReserved);

    public static Name CreateName(SyntaxNode node, string text, bool bypassReserved = false)
    {
        Name name = CreateSimpleName(node, text, bypassReserved);
        var pieces = text.Split('.');

        if (pieces.Length <= 0) return name;

        return pieces
               .Skip(1)
               .Aggregate(name, (current, piece) => new QualifiedName(current, CreateSimpleName(node, piece)));
    }

    public static TNameNode CreateSimpleName<TNameNode>(SyntaxNode node, bool bypassReserved = false, bool noGenerics = false)
        where TNameNode : SimpleName =>
        (TNameNode)CreateSimpleName(node, bypassReserved, noGenerics);

    public static TNameNode CreateSimpleName<TNameNode>(SyntaxNode node,
                                                        string name,
                                                        bool bypassReserved = false,
                                                        bool noGenerics = false)
        where TNameNode : SimpleName =>
        (TNameNode)CreateSimpleName(node, name, bypassReserved, noGenerics);

    public static SimpleName CreateSimpleName(SyntaxNode node, bool bypassReserved = false, bool noGenerics = false) =>
        CreateSimpleName(node,
                         string.Join("", StandardUtility.GetNamesFromNode(node, noGenerics)),
                         bypassReserved,
                         noGenerics);

    public static SimpleName CreateSimpleName(SyntaxNode node, string name, bool bypassReserved = false, bool noGenerics = false)
    {
        if (!bypassReserved && CheckReservedName(node, name)) return null!;

        var text = name.Replace("@", "");

        return !noGenerics && name.Contains('<') && name.Contains('>')
            ? new GenericName(text.Split('<').First(), StandardUtility.ExtractTypeArguments(text))
            : new IdentifierName(text);
    }

    public static bool CheckReservedName(SyntaxNode node, string name) => CheckReservedName(node.GetFirstToken(), name);

    public static bool CheckReservedName(SyntaxToken token, string name)
    {
        var reserved = RESERVED_IDENTIFIERS.Contains(name);

        if (reserved) throw Logger.UnsupportedError(token, $"Using '{name}' as an identifier", true, false);

        return reserved;
    }

    public static TypeRef? CreateTypeRef(string? typePath)
    {
        switch (typePath)
        {
            case null:
            case "var":
                return null;
        }

        var mappedTypePath = StandardUtility.GetMappedType(typePath);
        if (mappedTypePath.EndsWith('?'))
            return new OptionalType(CreateTypeRef(mappedTypePath.TrimEnd('?'))!);

        var functionMatch = Regex.Match(mappedTypePath, @"^\(\s*(.*?)\s*\)\s*->\s*(.+)$");
        if (functionMatch.Success)
        {
            var argsRaw = functionMatch.Groups[1].Value;
            var returnTypeRaw = functionMatch.Groups[2].Value.Trim();
            var args = ParseFunctionArgs(argsRaw);
            var returnType = CreateTypeRef(returnTypeRaw)!;

            return new FunctionType(args, returnType);
        }

        var mappedTypeMatch = Regex.Match(mappedTypePath, @"\{\s*\[([a-zA-Z0-9]+)\]:\s*(.*)\s*\}");
        if (mappedTypeMatch.Success)
            return TryParseMappedType(mappedTypePath);

        var arrayMatch = Regex.Match(mappedTypePath, @"\{\s*(.*)\s*\}");
        if (arrayMatch.Success)
            return new ArrayType(CreateTypeRef(arrayMatch.Groups[1].Value.Trim())!);

        return new TypeRef(mappedTypePath, true);
    }

    private static MappedType? TryParseMappedType(string input)
    {
        input = input.Trim();
        if (!input.StartsWith('{') || !input.EndsWith('}'))
            return null;

        input = input.Substring(1, input.Length - 2).Trim();
        if (!input.StartsWith('['))
            return null;

        var index = 0;
        var bracketLevel = 0;
        var colonIndex = -1;
        for (; index < input.Length; index++)
        {
            var c = input[index];
            if (c == '[')
            {
                bracketLevel++;
            }
            else if (c == ']')
            {
                bracketLevel--;
            }
            else if (c == ':' && bracketLevel == 0)
            {
                colonIndex = index;
                break;
            }
        }

        if (colonIndex == -1)
            return null;

        var keyPart = input[..colonIndex].Trim(); // includes [ ... ]
        var valuePart = input[(colonIndex + 1)..].Trim();
        if (!keyPart.StartsWith('[') || !keyPart.EndsWith(']'))
            return null;

        var keyContent = keyPart.Substring(1, keyPart.Length - 2).Trim(); // remove [ and ]
        var keyType = CreateTypeRef(keyContent);
        var valueType = CreateTypeRef(valuePart);
        if (keyType != null && valueType != null)
            return new MappedType(keyType, valueType);

        return null;
    }

    private static List<ParameterType> ParseFunctionArgs(string input)
    {
        var args = new List<ParameterType>();

        if (string.IsNullOrWhiteSpace(input)) return args;

        var depth = 0;
        var lastSplit = 0;
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            switch (c)
            {
                case '<':
                case '(':
                    depth++;

                    break;
                case '>':
                case ')':
                    depth--;

                    break;
                case ',' when depth == 0:
                    args.Add(ParseSingleArg(input.Substring(lastSplit, i - lastSplit).Trim()));
                    lastSplit = i + 1;

                    break;
            }
        }

        // Add the final arg
        args.Add(ParseSingleArg(input.Substring(lastSplit).Trim()));

        return args;
    }

    private static ParameterType ParseSingleArg(string raw)
    {
        var parts = raw.Split(':', 2);
        if (parts.Length == 2)
        {
            var name = parts[0].Trim();
            var type = CreateTypeRef(parts[1].Trim())!;

            return new ParameterType(name, type);
        }
        else
        {
            var type = CreateTypeRef(raw)!;

            return new ParameterType(null, type);
        }
    }

    public static TypeRef? CreateTypeRef(TypeSyntax? type) => CreateTypeRef(type?.ToString());

    public static Literal String(string text) => new($"\"{text}\"");
    public static Literal Bool(bool value) => value ? True : False;

    private static Literal Number<T>(T number)
        where T : INumber<T> =>
        new(number.ToString() ?? "0");
}