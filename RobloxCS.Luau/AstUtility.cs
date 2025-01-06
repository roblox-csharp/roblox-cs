using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static RobloxCS.Luau.Constants;

namespace RobloxCS.Luau
{
    public static class AstUtility
    {
        // TODO: make per-scope
        /// <summary>file path -> dictionary(identifier name, amount of times identifier is used)</summary>
        private static readonly Dictionary<string, Dictionary<string, uint>> _identifierDeclarations = [];

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
                new Literal("\"Name\""),
                new Literal("\"FullName\""),
                new Literal("\"Namespace\""),
                new Literal("\"AssemblyQualifiedName\""),
                new Literal("\"TypeInitializer\""),
                new Literal("\"ReflectedType\""),
                new Literal("\"IsAbstract\""),
                new Literal("\"IsAnsiClass\""),
                new Literal("\"IsArray\""),
                new Literal("\"IsSealed\""),
                new Literal("\"IsInterface\""),
                new Literal("\"IsGenericTypeParameter\""),
                new Literal("\"IsGenericTypeDefinition\""),
                new Literal("\"IsGenericType\""),
                new Literal("\"IsGenericMethodParameter\""),
                new Literal("\"IsConstructedGenericType\""),
                new Literal("\"IsImport\""),
                new Literal("\"IsClass\""),
                new Literal("\"IsCollectible\""),
                new Literal("\"IsByRef\""),
                new Literal("\"IsByRefLike\""),
                new Literal("\"IsAutoClass\""),
                new Literal("\"IsAutoLayout\""),
                new Literal("\"IsCOMObject\""),
                new Literal("\"IsContextful\""),
                new Literal("\"IsEnum\""),
                new Literal("\"IsExplicitLayout\""),
                new Literal("\"IsPointer\""),
                new Literal("\"IsFunctionPointer\""),
                new Literal("\"IsUnmanagedFunctionPointer\""),
                new Literal("\"IsLayoutSequential\""),
                new Literal("\"IsMarshalByRef\""),
                new Literal("\"IsNested\""),
                new Literal("\"IsNestedAssembly\""),
                new Literal("\"IsNestedFamily\""),
                new Literal("\"IsNestedFamANDAssem\""),
                new Literal("\"IsNestedFamORAssem\""),
                new Literal("\"IsNestedPrivate\""),
                new Literal("\"IsNestedPublic\""),
                new Literal("\"IsNotPublic\""),
                new Literal("\"IsPublic\""),
                new Literal("\"IsSZArray\""),
                new Literal("\"IsSecurityCritical\""),
                new Literal("\"IsSecuritySafeCritical\""),
                new Literal("\"IsSecurityTransparent\""),
                new Literal("\"IsSignatureType\""),
                new Literal("\"IsSpecialName\""),
                new Literal("\"IsTypeDefinition\""),
                new Literal("\"IsUnicodeClass\""),
                new Literal("\"IsValueType\""),
                new Literal("\"IsVariableBoundArray\""),
                new Literal("\"IsVisible\""),
                new Literal("\"UnderlyingSystemType\""),
                new Literal("\"BaseType\""),
                new Literal("\"DeclaringType\""),
                new Literal("\"ContainsGenericParameters\""),
                new Literal("\"GenericTypeArguments\""),
                new Literal("\"GUID\""),
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

            return new TableInitializer(values, keys);
        }

        /// <summary>
        /// Creates constructor info table for runtime type objects (via .GetType() and typeof())
        /// </summary>
        public static TableInitializer CreateConstructorInfo(ConstructorInfo type)
        {
            List<Expression> keys = [
                new Literal("Name"),
            ];
            List<Expression> values = [
                new Literal(type.Name),
            ];
            
            return new TableInitializer(values, keys);
        }

        /// <code>CS.defineGlobal(name, value)</code>
        public static Call DefineGlobal(Name name, Expression value) =>
            CSCall("defineGlobal", new Literal($"\"{name}\""), value);

        /// <code>CS.getGlobal(name)</code>
        public static Call GetGlobal(Name name) =>
            CSCall("getGlobal", new Literal($"\"{name}\""));

        /// <summary>
        /// Creates a call to a CS library method
        /// </summary>
        public static Call CSCall(string methodName, params Expression[] arguments) =>
            new Call(
                new MemberAccess(
                    new IdentifierName("CS"),
                    new IdentifierName(methodName)
                ),
                CreateArgumentList(arguments.ToList())
            );

        /// <summary>
        /// Creates a call to a bit32 library method
        /// </summary>
        public static Call Bit32Call(string methodName, params Expression[] arguments) =>
            new Call(
                new MemberAccess(
                    new IdentifierName("bit32"),
                    new IdentifierName(methodName)
                ),
                CreateArgumentList(arguments.ToList())
            );

        public static ArgumentList CreateArgumentList(List<Expression> arguments) =>
            new ArgumentList(arguments.ConvertAll(expression => new Argument(expression)));

        public static SimpleName TypeNameFromSymbol(ITypeSymbol symbol)
        {
            if (symbol is INamedTypeSymbol { TypeParameters.Length: > 0 } namedTypeSymbol)
            {
                var typeParameters = namedTypeSymbol.TypeParameters.Select(typeParameter => TypeNameFromSymbol(typeParameter).ToString()).ToList();
                return new GenericName(symbol.Name, typeParameters);
            }
            
            return new IdentifierName(symbol.Name);
        }
        
        /// <summary>
        /// Returns the full name of a C# node's parent.
        /// This method is meant for getting the absolute location of classes, enums, etc.
        /// For example a class under the namespace "Some.Namespace" would return a <see cref="MemberAccess"/> that transpiles to "Some.Namespace".
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
            return parentLocation == null ?
                (
                    node.Parent.SyntaxTree == node.SyntaxTree ?
                        parentName
                        : CSCall("getGlobal", new Literal($"\"{(parentName is GenericName genericName ? genericName.Text : parentName.ToString())}\""))
                )
                : new MemberAccess(parentLocation, parentName);
        }

        /// <code>
        /// if name == nil then
        ///     name = initializer
        /// end
        /// </code>
        public static If Initializer(Name name, Expression initializer) =>
            new(
                new BinaryOperator(name, "==", Nil()),
                new ExpressionStatement(new Assignment(name, initializer)),
                null
            );

        /// <summary>
        /// Takes a <see cref="MemberAccess"/> and converts it into a <see cref="QualifiedName"/>, given that <see cref="MemberAccess.Expression"/> inherits from <see cref="Name"/>
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
            new(CreateSimpleName<IdentifierName>(node, "_"), true, value);
        
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

        public static Name CreateName(string text)
        {
            Name expression = new IdentifierName(text);
            var pieces = text.Split('.');
            if (pieces.Length <= 0)
                return expression;

            return pieces
                .Skip(1)
                .Aggregate(expression, (current, piece) => new QualifiedName(current, new IdentifierName(piece)));
        }
        
        public static TNameNode CreateSimpleName<TNameNode>(SyntaxNode node, bool registerIdentifier = false, bool bypassReserved = false) 
            where TNameNode : SimpleName
        {
            return (TNameNode)CreateSimpleName(node, registerIdentifier, bypassReserved);
        }

        public static TNameNode CreateSimpleName<TNameNode>(SyntaxNode node, string name, bool registerIdentifier = false, bool bypassReserved = false) 
            where TNameNode : SimpleName
        {
            return (TNameNode)CreateSimpleName(node, name, registerIdentifier, bypassReserved);
        }
        
        public static SimpleName CreateSimpleName(SyntaxNode node, bool registerIdentifier = false,bool bypassReserved = false)
        {
            return CreateSimpleName(node, string.Join("", Utility.GetNamesFromNode(node)), registerIdentifier, bypassReserved);
        }

        public static SimpleName CreateSimpleName(SyntaxNode node, string name, bool registerIdentifier = false, bool bypassReserved = false)
        {
            if (RESERVED_IDENTIFIERS.Contains(name) && !bypassReserved)
                Logger.UnsupportedError(node, $"Using '{name}' as an identifier", useIs: true, useYet: false);

            var text = registerIdentifier ? FixIdentifierNameText(node, name, registerIdentifier) : name;
            return name.Contains('<') && name.Contains('>')
                    ? new GenericName(text.Split('<').First(), Utility.ExtractTypeArguments(text))
                    : new IdentifierName(text);
        }

        // TODO: per-scope duplicate handling
        public static string FixIdentifierNameText(SyntaxNode node, string name, bool registerIdentifier = false)
        {
            _identifierDeclarations.TryAdd(node.SyntaxTree.FilePath, []);

            var identifiersInFile = _identifierDeclarations[node.SyntaxTree.FilePath];
            identifiersInFile.TryAdd(name, 0);

            var useCount = identifiersInFile[name];
            if (registerIdentifier)
                identifiersInFile[name]++;
            
            if (useCount <= 0)
                return name;
            
            if (!name.EndsWith('_'))
                name += '_';
            
            name += useCount;
            return name;
        }

        public static TypeRef? CreateTypeRef(string? typePath)
        {
            switch (typePath)
            {
                case null:
                case "var":
                    return null;
            }

            var mappedType = Utility.GetMappedType(typePath);
            var arrayMatch = Regex.Match(mappedType, @"\{[a-zA-Z0-9]+\}");
            if (mappedType.EndsWith('?'))
                return new OptionalType(CreateTypeRef(mappedType.Replace("?", ""))!);
            else if (arrayMatch.Success)
                return new ArrayType(CreateTypeRef(arrayMatch.Value.Trim())!);

            return new TypeRef(mappedType, rawPath: true);
        }

        public static TypeRef? CreateTypeRef(TypeSyntax? type) => CreateTypeRef(type?.ToString());

        public static Literal Vararg() => new("...");

        public static Literal False() => new("false");

        public static Literal True() => new("true");

        public static Literal Nil() => new("nil");

        public static TypeRef AnyType() => new("any");
        
        
    }
}