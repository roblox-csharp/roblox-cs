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

        public static Expression AddOne(Expression expression)
        {
            return expression is Literal literal && int.TryParse(literal.ValueText, out var value)
                ? new Literal((value + 1).ToString())
                : new BinaryOperator(expression, "+", new Literal("1"));
        }
        
        public static Expression SubtractOne(Expression expression)
        {
            return expression is Literal literal && int.TryParse(literal.ValueText, out var value)
                ? new Literal((value - 1).ToString())
                : new BinaryOperator(expression, "-", new Literal("1"));
        }

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

        public static Call DefineGlobal(Name name, Expression value) =>
            CSCall("defineGlobal", new Literal($"\"{name}\""), value);

        public static Call GetGlobal(Name name) =>
            CSCall("getGlobal", new Literal($"\"{name}\""));

        public static Call CSCall(string methodName, params Expression[] arguments) =>
            new Call(
                new MemberAccess(
                    new IdentifierName("CS"),
                    new IdentifierName(methodName)
                ),
                CreateArgumentList(arguments.ToList())
            );

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

        public static If Initializer(Name name, Expression initializer) =>
            new If(
                new BinaryOperator(name, "==", Nil()),
                new ExpressionStatement(new Assignment(name, initializer)),
                null
            );

        public static QualifiedName QualifiedNameFromMemberAccess(MemberAccess memberAccess)
        {
            var left = memberAccess.Expression is MemberAccess leftMemberAccess ?
                QualifiedNameFromMemberAccess(leftMemberAccess)
                : (Name)memberAccess.Expression;

            return new QualifiedName(left, memberAccess.Name);
        }

        public static Node DiscardVariableIfExpressionStatement(SyntaxNode node, Node value, SyntaxNode? valueParent) =>
            valueParent is ExpressionStatementSyntax ?
                DiscardVariable(node, (Expression)value)
                : value;

        public static Variable DiscardVariable(SyntaxNode node, Expression value) =>
            // shit hack
            new(CreateSimpleName<IdentifierName>(node, "_"), true, value);

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

        public static SimpleName CreateSimpleName(SyntaxNode node, bool registerIdentifier = false,
            bool bypassReserved = false)
        {
            foreach (var name in Utility.GetNamesFromNode(node))
                Console.WriteLine(name);
            
            return CreateSimpleName(node, string.Join("", Utility.GetNamesFromNode(node)), registerIdentifier, bypassReserved);
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

        public static SimpleName CreateSimpleName(SyntaxNode node, string name, bool registerIdentifier = false, bool bypassReserved = false)
        {
            if (RESERVED_IDENTIFIERS.Contains(name) && !bypassReserved)
                Logger.UnsupportedError(node, $"Using '{name}' as an identifier", useIs: true, useYet: false);

            var text = registerIdentifier ? FixIdentifierNameText(node, name, registerIdentifier) : name;
            return name.Contains('<') && name.Contains('>')
                    ? new Luau.GenericName(text.Split('<').First(), ExtractTypeArguments(text))
                    : new Luau.IdentifierName(text);
        }

        // TODO: reference the correct identifier names
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

        public static TypeRef? CreateTypeRef(string typePath)
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
        
        static List<string> ExtractTypeArguments(string input)
        {
            var match = Regex.Match(input, @"<([^>]+)>");
            if (!match.Success)
                return [];
            
            var arguments = match.Groups[1].Value.Split(',');
            return arguments.Select(arg => arg.Trim()).ToList();
        }
    }
}