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

    /// <summary>Adds one to the expression</summary>
    public static Expression AddOne(Expression expression)
    {
        return expression is Literal literal && int.TryParse(literal.ValueText, out var value)
            ? new Literal((value + 1).ToString())
            : new BinaryOperator(expression, "+", new Literal("1"));
    }
        
    /// <summary>Subtracts one from the expression</summary>
    public static Expression SubtractOne(Expression expression)
    {
        return expression is Literal literal && int.TryParse(literal.ValueText, out var value)
            ? new Literal((value - 1).ToString())
            : new BinaryOperator(expression, "-", new Literal("1"));
    }

    /// <summary>
    /// Creates type info table for runtime type objects
    /// </summary>
    public static TableInitializer CreateTypeInfo(Type type)
    {
        List<Expression> keys = [
            new IdentifierName("Name"),
            new IdentifierName("FullName"),
            new IdentifierName("Namespace"),
            new IdentifierName("AssemblyQualifiedName"),
            new IdentifierName("TypeInitializer"),
            new IdentifierName("ReflectedType"),
            new IdentifierName("IsAbstract"),
            new IdentifierName("IsAnsiClass"),
            new IdentifierName("IsArray"),
            new IdentifierName("IsSealed"),
            new IdentifierName("IsInterface"),
            new IdentifierName("IsGenericTypeParameter"),
            new IdentifierName("IsGenericTypeDefinition"),
            new IdentifierName("IsGenericType"),
            new IdentifierName("IsGenericMethodParameter"),
            new IdentifierName("IsConstructedGenericType"),
            new IdentifierName("IsImport"),
            new IdentifierName("IsClass"),
            new IdentifierName("IsCollectible"),
            new IdentifierName("IsByRef"),
            new IdentifierName("IsByRefLike"),
            new IdentifierName("IsAutoClass"),
            new IdentifierName("IsAutoLayout"),
            new IdentifierName("IsCOMObject"),
            new IdentifierName("IsContextful"),
            new IdentifierName("IsEnum"),
            new IdentifierName("IsExplicitLayout"),
            new IdentifierName("IsPointer"),
            new IdentifierName("IsFunctionPointer"),
            new IdentifierName("IsUnmanagedFunctionPointer"),
            new IdentifierName("IsLayoutSequential"),
            new IdentifierName("IsMarshalByRef"),
            new IdentifierName("IsNested"),
            new IdentifierName("IsNestedAssembly"),
            new IdentifierName("IsNestedFamily"),
            new IdentifierName("IsNestedFamANDAssem"),
            new IdentifierName("IsNestedFamORAssem"),
            new IdentifierName("IsNestedPrivate"),
            new IdentifierName("IsNestedPublic"),
            new IdentifierName("IsNotPublic"),
            new IdentifierName("IsPublic"),
            new IdentifierName("IsSZArray"),
            new IdentifierName("IsSecurityCritical"),
            new IdentifierName("IsSecuritySafeCritical"),
            new IdentifierName("IsSecurityTransparent"),
            new IdentifierName("IsSignatureType"),
            new IdentifierName("IsSpecialName"),
            new IdentifierName("IsTypeDefinition"),
            new IdentifierName("IsUnicodeClass"),
            new IdentifierName("IsValueType"),
            new IdentifierName("IsVariableBoundArray"),
            new IdentifierName("IsVisible"),
            new IdentifierName("UnderlyingSystemType"),
            new IdentifierName("BaseType"),
            new IdentifierName("DeclaringType"),
            new IdentifierName("ContainsGenericParameters"),
            new IdentifierName("GenericTypeArguments"),
            new IdentifierName("GUID"),
        ];
            
        List<Expression> values = [
            new Literal($"\"{type.Name}\""),
            type.FullName != null ? new Literal($"\"{type.FullName}\"") : Nil(),
            type.Namespace != null ? new Literal($"\"{type.Namespace}\"") : Nil(),
            type.AssemblyQualifiedName != null ? new Literal($"\"{type.AssemblyQualifiedName}\"") : Nil(),
            type.TypeInitializer != null ? CreateConstructorInfo(type.TypeInitializer) : Nil(),
            type.ReflectedType != null ? CreateTypeInfo(type.ReflectedType) : Nil(),
            new Literal(type.IsAbstract.ToString().ToLower()),
            new Literal(type.IsAnsiClass.ToString().ToLower()),
            new Literal(type.IsArray.ToString().ToLower()),
            new Literal(type.IsSealed.ToString().ToLower()),
            new Literal(type.IsInterface.ToString().ToLower()),
            new Literal(type.IsGenericTypeParameter.ToString().ToLower()),
            new Literal(type.IsGenericTypeDefinition.ToString().ToLower()),
            new Literal(type.IsGenericType.ToString().ToLower()),
            new Literal(type.IsGenericMethodParameter.ToString().ToLower()),
            new Literal(type.IsConstructedGenericType.ToString().ToLower()),
            new Literal(type.IsImport.ToString().ToLower()),
            new Literal(type.IsClass.ToString().ToLower()),
            new Literal(type.IsCollectible.ToString().ToLower()),
            new Literal(type.IsByRef.ToString().ToLower()),
            new Literal(type.IsByRefLike.ToString().ToLower()),
            new Literal(type.IsAutoClass.ToString().ToLower()),
            new Literal(type.IsAutoLayout.ToString().ToLower()),
            new Literal(type.IsCOMObject.ToString().ToLower()),
            new Literal(type.IsContextful.ToString().ToLower()),
            new Literal(type.IsEnum.ToString().ToLower()),
            new Literal(type.IsExplicitLayout.ToString().ToLower()),
            new Literal(type.IsPointer.ToString().ToLower()),
            new Literal(type.IsFunctionPointer.ToString().ToLower()),
            new Literal(type.IsUnmanagedFunctionPointer.ToString().ToLower()),
            new Literal(type.IsLayoutSequential.ToString().ToLower()),
            new Literal(type.IsMarshalByRef.ToString().ToLower()),
            new Literal(type.IsNested.ToString().ToLower()),
            new Literal(type.IsNestedAssembly.ToString().ToLower()),
            new Literal(type.IsNestedFamily.ToString().ToLower()),
            new Literal(type.IsNestedFamANDAssem.ToString().ToLower()),
            new Literal(type.IsNestedFamORAssem.ToString().ToLower()),
            new Literal(type.IsNestedPrivate.ToString().ToLower()),
            new Literal(type.IsNestedPublic.ToString().ToLower()),
            new Literal(type.IsNotPublic.ToString().ToLower()),
            new Literal(type.IsPublic.ToString().ToLower()),
            new Literal(type.IsSZArray.ToString().ToLower()),
            new Literal(type.IsSecurityCritical.ToString().ToLower()),
            new Literal(type.IsSecuritySafeCritical.ToString().ToLower()),
            new Literal(type.IsSecurityTransparent.ToString().ToLower()),
            new Literal(type.IsSignatureType.ToString().ToLower()),
            new Literal(type.IsSpecialName.ToString().ToLower()),
            new Literal(type.IsTypeDefinition.ToString().ToLower()),
            new Literal(type.IsUnicodeClass.ToString().ToLower()),
            new Literal(type.IsValueType.ToString().ToLower()),
            new Literal(type.IsVariableBoundArray.ToString().ToLower()),
            new Literal(type.IsVisible.ToString().ToLower()),
            type != type.UnderlyingSystemType ? CreateTypeInfo(type.UnderlyingSystemType) : Nil(),
            type.BaseType != null ? CreateTypeInfo(type.BaseType) : Nil(),
            type.DeclaringType != null ? CreateTypeInfo(type.DeclaringType) : Nil(),
            new Literal(type.ContainsGenericParameters.ToString().ToLower()),
            new TableInitializer(type.GenericTypeArguments.Select(CreateTypeInfo).OfType<Expression>().ToList()),
            new Literal($"\"{type.GUID}\"")
        ];

        return new TableInitializer(values, keys, true);
    }

    /// <summary>
    /// Creates constructor info table for runtime type objects (via .GetType() and typeof())
    /// </summary>
    private static TableInitializer CreateConstructorInfo(ConstructorInfo type)
    {
        List<Expression> keys = [
            new IdentifierName("Name"),
        ];
        List<Expression> values = [
            new Literal(type.Name),
        ];
            
        return new TableInitializer(values, keys, true);
    }

    /// <summary>
    /// CS.defineGlobal(name, "name") or parentName.name = "name"
    /// </summary>
    public static Statement DefineGlobalOrMember(SyntaxNode node, SimpleName name)
    {
        if (StandardUtility.IsGlobal(node))
            return new ExpressionStatement(DefineGlobal(name, name));
        
        var fullParentName = GetFullParentName(node);
        if (fullParentName != null)
            return new Assignment(
                new MemberAccess(
                    fullParentName,
                    name
                ),
                name
            );

        return new NoOp();
    }

    /// <code>CS.defineGlobal(name, value)</code>
    public static Call DefineGlobal(Name name, Expression type) =>
        CSCall("defineGlobal", String(name.ToString()), type);

    /// <code>CS.getGlobal(name)</code>
    public static Call GetGlobal(Name name) =>
        CSCall("getGlobal", String(name.ToString()));
    
    /// <code>CS.is(value, type)</code>
    public static Call Is(Expression value, Expression type) =>
        CSCall("is", value, type);
    
    /// <summary>
    /// Creates a call to a table library method
    /// </summary>
    public static Call TableCall(string methodName, params Expression[] arguments) =>
        new(
            new MemberAccess(
                new IdentifierName("table"),
                new IdentifierName(methodName)
            ),
            CreateArgumentList(arguments.ToList())
        );

    /// <summary>
    /// Creates a call to a CS library method
    /// </summary>
    public static Call CSCall(string methodName, params Expression[] arguments) =>
        new(
            new MemberAccess(
                new IdentifierName("CS"),
                new IdentifierName(methodName)
            ),
            CreateArgumentList(arguments.ToList())
        );

    public static Variable SignalImport() =>
        new(
            new IdentifierName("Signal"),
            true,
            // temporary until RojoReader
            RequireCall(new Luau.QualifiedName(
                new IdentifierName("rbxcs_include"),
                new IdentifierName("GoodSignal")
            ))
        );
        
    public static Call RequireCall(Expression modulePath) =>
        new(
            new IdentifierName("require"),
            new ArgumentList([new Argument(modulePath)])
        );
        
    public static Call PrintCall(params List<Expression> args) =>
        new(
            new IdentifierName("print"),
            new ArgumentList(args.ConvertAll(value => new Argument(value)))
        );

    /// <summary>
    /// Creates a call to a bit32 library method
    /// </summary>
    public static Call Bit32Call(string methodName, params Expression[] arguments) =>
        new(
            new MemberAccess(
                new IdentifierName("bit32"),
                new IdentifierName(methodName)
            ),
            CreateArgumentList(arguments.ToList())
        );

    public static ArgumentList CreateArgumentList(List<Expression> arguments) =>
        new(arguments.ConvertAll(expression => new Argument(expression)));

    public static SimpleName TypeNameFromSymbol(ISymbol symbol)
    {
        if (symbol is not INamedTypeSymbol { TypeParameters.Length: > 0 } namedTypeSymbol)
            return new IdentifierName(symbol.Name);
            
        var typeParameters = namedTypeSymbol.TypeParameters.Select(typeParameter => TypeNameFromSymbol(typeParameter).ToString()).ToList();
        return new GenericName(symbol.Name, typeParameters);
    }
        
    /// <summary>
    /// Returns the full name of a C# node's parent.
    /// This method is meant for getting the absolute location of classes, enums, etc.
    /// For example a class under the namespace "Some.Namespace" would return a <see cref="RobloxCS.Luau.MemberAccess"/> that transpiles to "Some.Namespace".
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
            ? (
                node.Parent.SyntaxTree == node.SyntaxTree
                    ? parentName
                    : CSCall("getGlobal", new Literal($"\"{(parentName is GenericName genericName ? genericName.Text : parentName.ToString())}\""))
            )
            : new MemberAccess(parentLocation, parentName);
    }

    /// <code>
    /// if name == nil then
    ///     name = initializer
    /// end
    /// </code>
    public static If DefaultValueInitializer(Name name, Expression initializer) =>
        new(
            new BinaryOperator(name, "==", Nil()),
            new Assignment(name, initializer)
        );

    /// <summary>
    /// Takes a <see cref="RobloxCS.Luau.MemberAccess"/> and converts it into a <see cref="QualifiedName"/>, given that <see cref="RobloxCS.Luau.MemberAccess.Expression"/> inherits from <see cref="Name"/>
    /// </summary>
    public static QualifiedName QualifiedNameFromMemberAccess(MemberAccess memberAccess)
    {
        var left = memberAccess.Expression is MemberAccess leftMemberAccess ?
            QualifiedNameFromMemberAccess(leftMemberAccess)
            : (Name)memberAccess.Expression;

        return new QualifiedName(left, memberAccess.Name);
    }

    /// <summary>
    /// Creates a discard variable if <see cref="valueParent"/> is an <see cref="ExpressionStatementSyntax"/>
    /// </summary>
    public static Node DiscardVariableIfExpressionStatement(SyntaxNode node, Node value, SyntaxNode? valueParent) =>
        valueParent is ExpressionStatementSyntax ?
            DiscardVariable(node, (Expression)value)
            : value;

    /// <code>local _ = discardedValue</code>
    public static Variable DiscardVariable(SyntaxNode node, Expression value) =>
        new(DiscardName, true, value);
        
    public static GenericName? GetGenericName(Name name) =>
        name switch
        {
            GenericName baseName => baseName,
            QualifiedName { Right: GenericName rightName } => rightName,
            _ => null
        };
        
    /// <summary>
    /// Takes a Name and converts it into a non-generic Name
    /// </summary>
    public static Name GetNonGenericName(Name name) =>
        name switch
        {
            QualifiedName qualifiedName => GetNonGenericName(qualifiedName),
            SimpleName simpleName => GetNonGenericName(simpleName),
            _ => name
        };
        
    /// <summary>
    /// Takes a QualifiedName and converts it into a non-generic QualifiedName
    /// </summary>
    public static QualifiedName GetNonGenericName(QualifiedName qualifiedName)
    {
        if (qualifiedName.Right is IdentifierName)
            return qualifiedName;

        var right = GetNonGenericName(qualifiedName.Right);
        return new QualifiedName(qualifiedName.Left, right);
    }

    /// <summary>
    /// Takes a SimpleName (which GenericName extends from) and converts it into a standard IdentifierName
    /// </summary>
    public static IdentifierName GetNonGenericName(SimpleName simpleName)
    {
        if (simpleName is IdentifierName identifierName)
            return identifierName;
            
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
        if (pieces.Length <= 0)
            return name;

        return pieces
            .Skip(1)
            .Aggregate(name, (current, piece) => new QualifiedName(current, CreateSimpleName(node, piece)));
    }
        
    public static TNameNode CreateSimpleName<TNameNode>(SyntaxNode node, bool bypassReserved = false, bool noGenerics = false) 
        where TNameNode : SimpleName
    {
        return (TNameNode)CreateSimpleName(node, bypassReserved, noGenerics);
    }

    public static TNameNode CreateSimpleName<TNameNode>(SyntaxNode node, string name, bool bypassReserved = false, bool noGenerics = false) 
        where TNameNode : SimpleName
    {
        return (TNameNode)CreateSimpleName(node, name, bypassReserved, noGenerics);
    }
        
    public static SimpleName CreateSimpleName(SyntaxNode node, bool bypassReserved = false, bool noGenerics = false)
    {
        return CreateSimpleName(node, string.Join("", StandardUtility.GetNamesFromNode(node)), bypassReserved, noGenerics);
    }

    public static SimpleName CreateSimpleName(SyntaxNode node, string name, bool bypassReserved = false, bool noGenerics = false)
    {
        if (RESERVED_IDENTIFIERS.Contains(name) && !bypassReserved)
            throw Logger.UnsupportedError(node, $"Using '{name}' as an identifier", useIs: true, useYet: false);

        var text = name.Replace('@', '_');
        return name.Contains('<') && name.Contains('>') && !noGenerics
            ? new GenericName(text.Split('<').First(), StandardUtility.ExtractTypeArguments(text))
            : new IdentifierName(text);
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
        
        var mappedTypeMatch = Regex.Match(mappedTypePath, @"\{\s*\[([a-zA-Z0-9]+)\]:\s*(.*)\s*\}");
        if (mappedTypeMatch.Success)
        {
            var keyType = CreateTypeRef(mappedTypeMatch.Groups[1].Value)!;
            var valueType = CreateTypeRef(mappedTypeMatch.Groups[2].Value)!;
            return new MappedType(keyType, valueType);
        }
        
        var arrayMatch = Regex.Match(mappedTypePath, @"\{\s*(.*)\s*\}");
        if (arrayMatch.Success)
            return new ArrayType(CreateTypeRef(arrayMatch.Groups[1].Value.Trim())!);

        return new TypeRef(mappedTypePath, rawPath: true);
    }

    public static TypeRef? CreateTypeRef(TypeSyntax? type) => CreateTypeRef(type?.ToString());
        
    public static Literal String(string text) => new($"\"{text}\"");

    public static IdentifierName Vararg { get; } = new("...");

    public static Literal False() => new("false");

    public static Literal True() => new("true");

    public static Literal Nil() => new("nil");

    public static TypeRef AnyType { get; } = new("any");
}