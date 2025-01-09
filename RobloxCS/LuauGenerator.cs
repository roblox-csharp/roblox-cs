using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Macros;
using RobloxCS.Shared;

namespace RobloxCS;

public sealed class LuauGenerator(SyntaxTree tree, CSharpCompilation compiler) : BaseGenerator(tree, compiler)
{
    private Macro _macro { get; } = new(compiler.GetSemanticModel(tree));

    public Luau.AST GetLuauAST() => Visit<Luau.AST>(_tree.GetRoot());

    private readonly HashSet<SyntaxKind> _hoistedSyntaxes =
    [
        SyntaxKind.NamespaceDeclaration,
        SyntaxKind.ClassDeclaration,
        SyntaxKind.InterfaceDeclaration,
        SyntaxKind.EnumDeclaration,
        SyntaxKind.LocalFunctionStatement
    ];

    public override Luau.AST VisitCompilationUnit(CompilationUnitSyntax node)
    {
        List<Luau.Statement> statements = [new Luau.SingleLineComment(Shared.Constants.HeaderComment + "\n\n")];

        void visitStatement(MemberDeclarationSyntax member)
        {
            var statement = Visit<Luau.Statement?>(member);
            if (statement == null)
                throw Logger.CompilerError($"Unhandled syntax node within {member.Kind()}:\n{member}");

            statements.Add(statement);
        }

        bool checkGlobalKind(MemberDeclarationSyntax memberDeclarationSyntax)
        {
            return _hoistedSyntaxes.Contains(
                memberDeclarationSyntax is GlobalStatementSyntax globalStatement
                    ? globalStatement.Statement.Kind()
                    : memberDeclarationSyntax.Kind()
            );
        }

        if (node.DescendantNodes().Any(descendant =>
                descendant.IsKind(SyntaxKind.EventDeclaration) || descendant.IsKind(SyntaxKind.EventFieldDeclaration)))
        {
            statements.Add(Luau.AstUtility.SignalImport());
            statements.Add(new Luau.NoOp()); // for the newline
        }
        

        var hoistedNodes = node.Members.Where(checkGlobalKind);
        var regularNodes = node.Members.Where(member => !checkGlobalKind(member));
        foreach (var member in hoistedNodes)
            visitStatement(member);
        foreach (var member in regularNodes)
            visitStatement(member);

        return new Luau.AST(statements);
    }

    public override Luau.Name? VisitPredefinedType(PredefinedTypeSyntax node) =>
        Luau.AstUtility.CreateTypeRef(node.Keyword.Text);

    public override Luau.ArrayType VisitArrayType(ArrayTypeSyntax node) =>
        new(Luau.AstUtility.CreateTypeRef(Visit<Luau.Name>(node.ElementType).ToString())!);

    public override Luau.OptionalType VisitNullableType(NullableTypeSyntax node) =>
        new(Luau.AstUtility.CreateTypeRef(Visit<Luau.Name>(node.ElementType).ToString())!);

    public override Luau.Statement VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (!IsStatic(node) || node.Parent is not ClassDeclarationSyntax || node.Initializer == null)
            return new Luau.NoOp();

        var classDeclaration = (ClassDeclarationSyntax)node.Parent!;
        var initializer = Visit<Luau.Expression>(node.Initializer);
        return new Luau.ExpressionStatement(
            new Luau.Assignment(
                new Luau.MemberAccess(
                    Luau.AstUtility.CreateSimpleName(classDeclaration),
                    Luau.AstUtility.CreateSimpleName(node)
                ),
                initializer
            )
        );
    }

    public override Luau.Statement VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (!IsStatic(node) || node.Parent is not ClassDeclarationSyntax)
            return new Luau.NoOp();

        var classDeclaration = (ClassDeclarationSyntax)node.Parent!;
        var staticFields = classDeclaration.Members
            .OfType<FieldDeclarationSyntax>()
            .Where(field => HasSyntax(field.Modifiers, SyntaxKind.StaticKeyword));

        List<Luau.Statement> statements = [];
        foreach (var declarator in node.Declaration.Variables)
        {
            if (declarator.Initializer == null) continue;

            var initializer = Visit<Luau.Expression>(declarator.Initializer);
            statements.Add(new Luau.ExpressionStatement(
                new Luau.Assignment(
                    new Luau.MemberAccess(
                        Luau.AstUtility.CreateSimpleName(classDeclaration),
                        Luau.AstUtility.CreateSimpleName(declarator)
                    ),
                    initializer
                )
            ));
        }

        return new Luau.Block(statements);
    }

    public override Luau.Function VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var classDeclaration = (ClassDeclarationSyntax)node.Parent!;
        var parameterList = Visit<Luau.ParameterList>(node.ParameterList);
        var body = Visit<Luau.Block?>(node.Body);
        var attributeLists = node.AttributeLists.Select(Visit<Luau.AttributeList>).ToList();

        return GenerateConstructor(classDeclaration, parameterList, body, attributeLists);
    }

    public override Luau.Function VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var name = Luau.AstUtility.CreateSimpleName(node);
        var className = Luau.AstUtility.CreateSimpleName(node.Parent!);
        var fullName = new Luau.QualifiedName(className, name, IsStatic(node) ? '.' : ':');
        var parameterList = Visit<Luau.ParameterList>(node.ParameterList);
        var returnType = Luau.AstUtility.CreateTypeRef(Visit<Luau.Name>(node.ReturnType).ToString());
        var body = node.ExpressionBody != null ?
            Visit<Luau.Block>(node.ExpressionBody)
            : Visit<Luau.Block?>(node.Body);

        var attributeLists = node.AttributeLists.Select(Visit<Luau.AttributeList>).ToList();
        return new Luau.Function(fullName, false, parameterList, returnType, body, attributeLists);
    }

    public override Luau.Block VisitArrowExpressionClause(ArrowExpressionClauseSyntax node) =>
        new Luau.Block([new Luau.Return(Visit<Luau.Expression>(node.Expression))]);

    public override Luau.IdentifierName VisitThisExpression(ThisExpressionSyntax node) =>
        new Luau.IdentifierName("self");

    // TODO: support initializers
    public override Luau.Call VisitArrayCreationExpression(ArrayCreationExpressionSyntax node) {
        var sizeExpression = node.Type.RankSpecifiers[0].Sizes[0];
        var translatedSize = Visit<Luau.Expression>(sizeExpression);

        return new Luau.Call(
            new Luau.MemberAccess(new Luau.IdentifierName("table"), new Luau.IdentifierName("create")),
            new Luau.ArgumentList([new Luau.Argument(translatedSize)])
        );
    }

    public override Luau.TableInitializer VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node) {
        var initializers = node.Initializer.Expressions.Select(Visit<Luau.Expression>);
        return new Luau.TableInitializer(initializers.ToList());
    }

    // long as hell lol
    public override Luau.Block VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var name = Luau.AstUtility.CreateSimpleName(node, registerIdentifier: true);
        var nonGenericName = Luau.AstUtility.GetNonGenericName(name);
        var members = node.Members.Select(Visit<Luau.Statement>).ToList();
        var explicitConstructor = node.Members.FirstOrDefault(member => member.IsKind(SyntaxKind.ConstructorDeclaration)) as ConstructorDeclarationSyntax;
        var constructor = explicitConstructor == null
            ? GenerateConstructor(node, new Luau.ParameterList([]))
            : Visit<Luau.Function>(explicitConstructor);

        // TODO: maybe move this to AstUtility, this shit is huge
        var typeRef = Luau.AstUtility.CreateTypeRef(name.ToString())!;
        var nameStringLiteral = Luau.AstUtility.String(nonGenericName.ToString());
        List<Luau.Statement> classMemberStatements = [
            new Luau.ExpressionStatement(
                new Luau.Assignment(
                    nonGenericName,
                    new Luau.Call(
                        new Luau.IdentifierName("setmetatable"),
                        Luau.AstUtility.CreateArgumentList([
                            new Luau.TableInitializer(),
                            new Luau.TableInitializer(
                                [new Luau.AnonymousFunction(
                                    new Luau.ParameterList([]),
                                    new Luau.TypeRef("string"),
                                    new Luau.Block([new Luau.Return(nameStringLiteral)])
                                )],
                                [new Luau.IdentifierName("__tostring")],
                                true
                            )
                        ])
                    )
                )
            ),
            new Luau.ExpressionStatement(
                new Luau.Assignment(
                    new Luau.MemberAccess(
                        nonGenericName,
                        new Luau.IdentifierName("__index")
                    ),
                    nonGenericName
                )
            ),
            new Luau.ExpressionStatement(
                new Luau.Assignment(
                    new Luau.MemberAccess(
                        nonGenericName,
                        new Luau.IdentifierName("__className")
                    ),
                    nameStringLiteral
                )
            ),
            new Luau.Function(
                new Luau.QualifiedName(nonGenericName, new Luau.IdentifierName("new")),
                false,
                constructor.ParameterList,
                typeRef,
                new Luau.Block([
                    new Luau.Variable(
                        new Luau.IdentifierName("self"),
                        true,
                        new Luau.TypeCast(
                            new Luau.Parenthesized(
                                new Luau.TypeCast(
                                    new Luau.Call(
                                        new Luau.IdentifierName("setmetatable"),
                                        Luau.AstUtility.CreateArgumentList([
                                            new Luau.TableInitializer(),
                                            nonGenericName
                                        ])
                                    ),
                                    Luau.AstUtility.AnyType()
                                )
                            ),
                            typeRef
                        )
                    ),
                    new Luau.Return(
                        new Luau.BinaryOperator(
                            new Luau.Call(
                                new Luau.MemberAccess(new Luau.IdentifierName("self"), nonGenericName, ':'),
                                Luau.AstUtility.CreateArgumentList(constructor.ParameterList.Parameters.ConvertAll<Luau.Expression>(parameter => parameter.Name))
                            ),
                            "or",
                            new Luau.IdentifierName("self")
                        )
                    )
                ])
            )
        ];

        if (IsGlobal(node))
            classMemberStatements.Insert(2, new Luau.ExpressionStatement(Luau.AstUtility.DefineGlobal(nonGenericName, nonGenericName)));

        if (explicitConstructor == null)
            classMemberStatements.Add(constructor);

        classMemberStatements.AddRange(members);

        List<Luau.Statement> statements = [
            new Luau.Variable(nonGenericName, true, null),
            new Luau.ScopedBlock(classMemberStatements),
            new Luau.TypeAlias(name, new Luau.TypeOfCall(nonGenericName))
        ];

        if (IsGlobal(node))
            return new Luau.Block(statements);

        var fullParentName = Luau.AstUtility.GetFullParentName(node);
        if (fullParentName != null)
            statements.Add(new Luau.ExpressionStatement(
                new Luau.Assignment(
                    new Luau.MemberAccess(
                        fullParentName,
                        nonGenericName
                    ),
                    nonGenericName
                )
            ));

        statements.Add(new Luau.NoOp()); // for the newline
        return new Luau.Block(statements);
    }

    public override Luau.Block VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        var enumKeys = new List<Luau.Expression>();
        var enumValues = new List<Luau.Expression>();
        var enumTypes = new List<Luau.TypeRef>();
        var index = 0;

        foreach (var member in node.Members)
        {
            var explicitValue = member.EqualsValue?.Value;
            var value = explicitValue?.ToString() ?? index.ToString();
            enumTypes.Add(new Luau.TypeRef(value));
            enumKeys.Add(Luau.AstUtility.CreateSimpleName(member, member.Identifier.Text));
            enumValues.Add(new Luau.Literal(value));

            index = (explicitValue != null ? int.Parse(explicitValue.ToString()) : index) + 1;
        }

        var name = Luau.AstUtility.CreateSimpleName(node, node.Identifier.Text, registerIdentifier: true);
        var enumType = new Luau.TypeOfCall(name);
        var finalType = new Luau.IndexCall(enumType, new Luau.KeyOfCall(enumType));
        List<Luau.Statement> statements = [
            new Luau.ExpressionStatement(
                new Luau.Assignment(
                    new Luau.IdentifierName(node.Identifier.Text),
                    new Luau.TableInitializer(enumValues, enumKeys)
                )
            )
        ];

        if (IsGlobal(node))
            statements.Add(new Luau.ExpressionStatement(Luau.AstUtility.DefineGlobal(name, name)));
        else
        {
            var fullParentName = Luau.AstUtility.GetFullParentName(node);
            if (fullParentName != null)
                statements.Add(new Luau.ExpressionStatement(
                    new Luau.Assignment(
                        new Luau.MemberAccess(
                            fullParentName,
                            name
                        ),
                        name
                    )
                ));

        }

        return new Luau.Block([
            new Luau.Variable(Luau.AstUtility.GetNonGenericName(name), true),
            new Luau.ScopedBlock(statements),
            new Luau.TypeAlias(name, finalType),
            new Luau.NoOp() // for the newline
        ]);
    }
    public override Luau.Block VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        var name = Luau.AstUtility.CreateSimpleName(node, registerIdentifier: true);
        var typeRef = Luau.AstUtility.CreateTypeRef(name.ToString());
        var members = new Luau.Block(node.Members.Select(Visit<Luau.Statement>).ToList());
        List<Luau.Statement> statements =
        [
            new Luau.Variable(Luau.AstUtility.GetNonGenericName(name), true, new Luau.TableInitializer(), typeRef)
        ];

        if (IsGlobal(node))
            statements.Add(new Luau.ExpressionStatement(Luau.AstUtility.DefineGlobal(name, name)));

        statements.AddRange(
            new Luau.ScopedBlock(members.Statements),
            new Luau.TypeAlias(name, new Luau.TypeOfCall(name)),
            new Luau.NoOp() // for the newline
        );
        
        return new Luau.Block(statements);
    }

    public override Luau.Repeat VisitDoStatement(DoStatementSyntax node)
    {
        var condition = Visit<Luau.Expression>(node.Condition);
        var body = Visit<Luau.Statement>(node.Statement);
        return new Luau.Repeat(new Luau.UnaryOperator("not ", condition), body);
    }

    public override Luau.While VisitWhileStatement(WhileStatementSyntax node)
    {
        var condition = Visit<Luau.Expression>(node.Condition);
        var body = Visit<Luau.Statement>(node.Statement);
        return new Luau.While(condition, body);
    }

    public override Luau.ExpressionalIf VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        var condition = Visit<Luau.Expression>(node.Condition);
        var body = Visit<Luau.Expression>(node.WhenTrue);
        var elseBranch = Visit<Luau.Expression>(node.WhenFalse);
        return new Luau.ExpressionalIf(condition, body, elseBranch);
    }

    public override Luau.If VisitIfStatement(IfStatementSyntax node)
    {
        var condition = Visit<Luau.Expression>(node.Condition);
        var body = Visit<Luau.Statement>(node.Statement);
        var elseBranch = Visit<Luau.Statement?>(node.Else?.Statement);
        return new Luau.If(condition, body, elseBranch);
    }

    public override Luau.Statement VisitForStatement(ForStatementSyntax node)
    {
        var initializer = Visit<Luau.VariableList?>(node.Declaration)?.Variables.FirstOrDefault();
        var condition = Visit<Luau.Expression?>(node.Condition) ?? Luau.AstUtility.True();
        var isNumericLoop = initializer is { Initializer: Luau.Literal literal } && int.TryParse(literal.ValueText, out _);
        var incrementByExpression = Visit<Luau.Expression?>(node.Incrementors.FirstOrDefault());
        var body = Visit<Luau.Statement>(node.Statement);
        if (isNumericLoop &&
            node.Condition is BinaryExpressionSyntax
            {
                OperatorToken: { Text: "<=" or "<" }
            } binaryOp &&
            incrementByExpression is Luau.BinaryOperator { Operator: "+=" or "-=" } incrementBinaryOp)
        {
            var minimum = initializer!.Initializer!;
            var maximum = ((Luau.BinaryOperator)condition).Right;
            if (binaryOp.OperatorToken.Text == "<")
                maximum = Luau.AstUtility.SubtractOne(maximum);

            return new Luau.NumericFor(initializer.Name, minimum, maximum, incrementBinaryOp.Operator == "-=" ? new Luau.Literal("-1") : null, body);
        }

        Luau.Statement? incrementBy = incrementByExpression != null ? new Luau.ExpressionStatement(incrementByExpression) : null;
        List<Luau.Statement> statements = [];

        if (initializer != null)
            statements.Add(initializer);

        var shouldIncrementIdentifier = Luau.AstUtility.CreateSimpleName<Luau.IdentifierName>(node, "_shouldIncrement", registerIdentifier: true);
        if (incrementBy != null)
            statements.Add(new Luau.Variable(shouldIncrementIdentifier, true, Luau.AstUtility.False()));

        List<Luau.Statement> whileStatements = [body];
        if (incrementBy != null)
        {
            if (incrementBy is Luau.ExpressionStatement { Expression: Luau.BinaryOperator binaryOperator } expressionStatement &&
                !binaryOperator.Operator.Contains('='))
            {
                incrementBy = new Luau.Variable(new Luau.IdentifierName("_"), true, expressionStatement.Expression);
            }
            whileStatements.Add(new Luau.If(shouldIncrementIdentifier, incrementBy, new Luau.ExpressionStatement(new Luau.Assignment(shouldIncrementIdentifier, Luau.AstUtility.True()))));
        }

        whileStatements.Add(new Luau.If(new Luau.UnaryOperator("not ", new Luau.Parenthesized(condition)), new Luau.Break()));
        statements.Add(new Luau.While(Luau.AstUtility.True(), new Luau.Block(whileStatements)));
        return new Luau.ScopedBlock(statements);
    }

    public override Luau.For VisitForEachStatement(ForEachStatementSyntax node)
    {
        List<Luau.IdentifierName> names = [Luau.AstUtility.CreateSimpleName<Luau.IdentifierName>(node)];
        var iterator = Visit<Luau.Expression>(node.Expression);
        var body = Visit<Luau.Statement>(node.Statement);
        return new Luau.For(names, iterator, body);
    }

    public override Luau.For VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
    {
        var variableList = Visit<Luau.Statement>(node.Variable);
        if (variableList is Luau.Variable variable)
            variableList = new Luau.VariableList([variable]);

        var names = ((Luau.VariableList)variableList).Variables.ConvertAll(v => v.Name);
        var iterator = Visit<Luau.Expression>(node.Expression);
        var body = Visit<Luau.Statement>(node.Statement);
        return new Luau.For(names, iterator, body);
    }

    public override Luau.Node? VisitDeclarationExpression(DeclarationExpressionSyntax node) =>
        Visit(node.Designation);

    public override Luau.Variable VisitSingleVariableDesignation(SingleVariableDesignationSyntax node) =>
        new Luau.Variable(Luau.AstUtility.CreateSimpleName<Luau.IdentifierName>(node), true);

    public override Luau.VariableList VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntax node)
    {
        var variableNodes = node.Variables.Select(Visit)
            .Where(variableNode => variableNode != null)
            .Select(variableNode => variableNode!)
            .SelectMany(variableNode =>
            {
                if (variableNode is Luau.VariableList variableList)
                    return variableList.Variables;
                
                return [(Luau.Variable)variableNode];
            })
            .ToList();

        return new Luau.VariableList(variableNodes);
    }

    public override Luau.Parenthesized VisitTypeOfExpression(TypeOfExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type;
        if (typeSymbol == null)
            throw Logger.CodegenError(node, "Unable to resolve type symbol of the type provided to typeof()");

        var type = StandardUtility.GetRuntimeType(_semanticModel, node, typeSymbol);
        return Luau.AstUtility.CreateTypeInfo(type);
    }

    public override Luau.Expression VisitExpressionElement(ExpressionElementSyntax node) =>
        Visit<Luau.Expression>(node.Expression);

    public override Luau.TableInitializer VisitCollectionExpression(CollectionExpressionSyntax node)
    {
        var elements = node.Elements.Select(Visit<Luau.Expression>).ToList();
        return new Luau.TableInitializer(elements);
    }

    public override Luau.Node? VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
    {
        // TODO: handle non-null node.Initializer (prob won't be supported)
        var classSymbol = (ITypeSymbol)_semanticModel.GetSymbolInfo(node).Symbol!.ContainingSymbol;
        var name = Luau.AstUtility.TypeNameFromSymbol(classSymbol);
        var nonGenericName = Luau.AstUtility.GetNonGenericName(name);
        var argumentList = Visit<Luau.ArgumentList>(node.ArgumentList);

        var callee = new Luau.QualifiedName(nonGenericName, new Luau.IdentifierName("new"));
        var expandedExpression = _macro.ObjectCreation(Visit, node);

        return expandedExpression ?? new Luau.Call(callee, argumentList);
    }

    public override Luau.Expression VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        // TODO: handle non-null node.Initializer (prob won't be supported)
        var name = Visit<Luau.Name>(node.Type);
        var nonGenericName = Luau.AstUtility.GetNonGenericName(name);
        var argumentList = Visit<Luau.ArgumentList>(node.ArgumentList);

        var callee = new Luau.QualifiedName(nonGenericName, new Luau.IdentifierName("new"));
        var expandedExpression = _macro.ObjectCreation(Visit, node);

        return expandedExpression ?? new Luau.Call(callee, argumentList);
    }

    public override Luau.Node VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var methodSymbolInfo = _semanticModel.GetSymbolInfo(node.Expression);
        if (methodSymbolInfo.Symbol == null &&
            methodSymbolInfo.CandidateSymbols.IsEmpty &&
            methodSymbolInfo.CandidateReason == CandidateReason.None &&
            node.Expression is IdentifierNameSyntax identifier &&
            identifier.Identifier.IsKind(SyntaxKind.IdentifierToken))
        {
            switch (identifier.Identifier.Text)
            {
                case "nameof":
                    return new Luau.Literal('"' + node.ArgumentList.Arguments.First().Expression.ToString() + '"');
            }
        }

        var callee = Visit<Luau.Expression>(node.Expression);
        if (callee is Luau.MemberAccess memberAccess)
            memberAccess.Operator = methodSymbolInfo.Symbol!.IsStatic ? '.' : ':';

        var argumentList = Visit<Luau.ArgumentList>(node.ArgumentList);
        List<MacroKind> returnCalleeMacroKinds =
        [
            MacroKind.NewInstance,
            MacroKind.IEnumerableMethod,
            MacroKind.ListMethod,
            MacroKind.DictionaryMethod,
            MacroKind.ObjectMethod
        ];

        // dumb ass hack bc null warning suppression doesn't work here for some reason
        if (callee.ExpandedByMacro != null && returnCalleeMacroKinds.Contains((MacroKind)callee.ExpandedByMacro))
            return callee;

        return new Luau.Call(callee, argumentList);
    }

    public override Luau.ArgumentList VisitArgumentList(ArgumentListSyntax node)
    {
        var arguments = node.Arguments.Select(Visit<Luau.Argument>).ToList();
        return new Luau.ArgumentList(arguments);
    }

    public override Luau.Argument VisitArgument(ArgumentSyntax node)
    {
        var expression = Visit<Luau.Expression>(node.Expression);
        return new Luau.Argument(expression);
    }

    public override Luau.Variable VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
    {
        var name = Visit<Luau.IdentifierName?>(node.NameEquals?.Name);
        if (name == null)
        {
            var expressionName = _semanticModel.GetSymbolInfo(node.Expression).Symbol!.Name;
            if (expressionName.Contains('.'))
                expressionName = expressionName.Split('.').Last();

            name = Luau.AstUtility.CreateSimpleName<Luau.IdentifierName>(node, expressionName);
        }

        var value = Visit<Luau.Expression?>(node.Expression);
        return new Luau.Variable(name, true, value);
    }

    public override Luau.Node VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        var expanded = _macro.Assignment(Visit, node);
        if (expanded != null)
            return expanded;
        
        var mappedOperator = StandardUtility.GetMappedOperator(node.OperatorToken.Text);
        var name = Visit<Luau.AssignmentTarget>(node.Left);
        var value = Visit<Luau.Expression>(node.Right);
        if (node.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return new Luau.Assignment(name, value);

        return new Luau.BinaryOperator(name, mappedOperator, value);
    }

    public override Luau.TableInitializer VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
    {
        List<Luau.Expression> values = [];
        List<Luau.Expression> keys = [];
        foreach (var member in node.Initializers)
        {
            var declaration = Visit<Luau.Variable>(member)!;
            var key = new Luau.Literal($"\"{declaration.Name}\"");
            var value = declaration.Initializer!;
            keys.Add(key);
            values.Add(value);
        }

        return new Luau.TableInitializer(values, keys);
    }

    public override Luau.Node VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var expression = Visit<Luau.Expression>(node.Expression);
        var name = Visit<Luau.SimpleName>(node.Name);
        var memberAccess = new Luau.MemberAccess(expression, name);

        var luauNode = Luau.AstUtility.DiscardVariableIfExpressionStatement(node, memberAccess, node.Parent);
        if (node.Parent is AssignmentExpressionSyntax assignment && assignment.Left == node)
            luauNode = Luau.AstUtility.QualifiedNameFromMemberAccess(memberAccess);

        var expandedExpression = _macro.MemberAccess(Visit, node);
        if (expandedExpression != null)
            luauNode = expandedExpression;

        return luauNode;
    }

    public override Luau.Node VisitImplicitElementAccess(ImplicitElementAccessSyntax node) =>
        Visit<Luau.Expression>(node.ArgumentList.Arguments.First().Expression);

    public override Luau.Node VisitElementAccessExpression(ElementAccessExpressionSyntax node)
    {
        var expression = Visit<Luau.Expression>(node.Expression);
        var index = Visit<Luau.Expression>(node.ArgumentList.Arguments.First().Expression);
        var elementAccess = new Luau.ElementAccess(expression, Luau.AstUtility.AddOne(index));
        return Luau.AstUtility.DiscardVariableIfExpressionStatement(node, elementAccess, node.Parent);
    }

    public override Luau.QualifiedName VisitQualifiedName(QualifiedNameSyntax node)
    {
        var left = Visit<Luau.Name>(node.Left);
        var right = Visit<Luau.IdentifierName>(node.Right);
        return new Luau.QualifiedName(left, right);
    }

    public override Luau.Name VisitIdentifierName(IdentifierNameSyntax node)
    {
        var classDeclaration = FindFirstAncestor<ClassDeclarationSyntax>(node);
        var isClassMember = classDeclaration != null &&
                            classDeclaration.Members.Any(member => member is not ConstructorDeclarationSyntax && TryGetName(member) == GetName(node)) &&
                            node.Parent is not MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax };

        var name = Luau.AstUtility.CreateSimpleName(node);
        return isClassMember ?
            new Luau.QualifiedName(new Luau.IdentifierName("self"), name)
            : name;
    }

    public override Luau.Expression VisitGenericName(GenericNameSyntax node)
    {
        var typeArguments = node.TypeArgumentList.Arguments.Select(typeArg => StandardUtility.GetMappedType(Visit<Luau.Name>(typeArg).ToString())).ToList();
        Luau.Expression? expandedExpression = _macro.GenericName(Visit, node);

        return expandedExpression ?? new Luau.GenericName(node.Identifier.Text, typeArguments);
    }

    public override Luau.Break VisitBreakStatement(BreakStatementSyntax node) => new();
    public override Luau.Continue VisitContinueStatement(ContinueStatementSyntax node) => new();
    public override Luau.Return VisitReturnStatement(ReturnStatementSyntax node) => new(Visit<Luau.Expression?>(node.Expression));

    public override Luau.Block VisitBlock(BlockSyntax node)
    {
        var statements = node.Statements.Select(Visit).OfType<Luau.Statement>().ToList();
        return node.Parent is BlockSyntax or GlobalStatementSyntax or null
            ? new Luau.ScopedBlock(statements)
            : new Luau.Block(statements);
    }

    public override Luau.Node VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        var expanded = _macro.BinaryExpression(Visit, node);
        if (expanded != null)
            return expanded;

        var left = Visit<Luau.Expression>(node.Left);
        var right = Visit<Luau.Expression>(node.Right);
        var mappedOperator = StandardUtility.GetMappedOperator(node.OperatorToken.Text);
        return new Luau.BinaryOperator(left, mappedOperator, right);
    }

    public override Luau.Node VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
    {
        var operand = Visit<Luau.Expression>(node.Operand);
        var operandType = _semanticModel.GetTypeInfo(node.Operand).Type!;
        if (node.OperatorToken.Text == "!")
            return new Luau.TypeCast(operand, Luau.AstUtility.CreateTypeRef(operandType.Name.Replace("?", ""))!);

        var mappedOperator = StandardUtility.GetMappedOperator(node.OperatorToken.Text);
        return new Luau.BinaryOperator(operand, mappedOperator, new Luau.Literal("1"));
    }

    public override Luau.Node? VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
    {
        var operatorText = node.OperatorToken.Text;
        if (operatorText == "^")
        {
            Logger.UnsupportedError(node, "'^' unary operator", useIs: true);
            return null;
        }

        var operand = Visit<Luau.Expression>(node.Operand);
        if (operatorText == "+")
            return operand;

        var mappedOperator = StandardUtility.GetMappedOperator(operatorText);
        var bit32MethodName = StandardUtility.GetBit32MethodName(mappedOperator);
        if (bit32MethodName != null)
            return Luau.AstUtility.Bit32Call(bit32MethodName, operand);

        return new Luau.UnaryOperator(mappedOperator, operand);
    }

    // TODO: create VisitCaseSwitchLabel, VisitCasePatternSwitchLabel, VisitDefaultSwitchLabel methods
    public override Luau.Block VisitSwitchStatement(SwitchStatementSyntax node)
    {
        var ifStatements = new List<Luau.Statement>();
        List<Luau.Statement>? defaultStatements = null;

        var nodeHasFallThrough = false;
        var createTempVariable = node.Expression is not IdentifierNameSyntax && node.Expression is not LiteralExpressionSyntax;
        var condition = Visit<Luau.Expression>(node.Expression);
        var comparand = createTempVariable ?
            Luau.AstUtility.CreateSimpleName(node.Expression, "_exp", registerIdentifier: true)
            : condition;

        var anyNodeHasFallThrough = node.Sections.Any(section => section.Labels.Count > 1);
        var fallthroughIdentifier = Luau.AstUtility.CreateSimpleName<Luau.IdentifierName>(node.Expression, "_fallthrough", registerIdentifier: anyNodeHasFallThrough);
        foreach (var section in node.Sections)
        {
            var fallThrough = section.Labels.Count > 1;

            foreach (var label in section.Labels) {
                var body = section.Labels.Last() == label ?
                    section.Statements.Select(Visit<Luau.Statement>).ToList()
                    : [];

                var hasFallThrough = fallThrough && label != section.Labels.Last();
                if (hasFallThrough) {
                    nodeHasFallThrough = true;
                    body.Insert(0, new Luau.ExpressionStatement(new Luau.Assignment(fallthroughIdentifier, Luau.AstUtility.True())));
                }

                switch (label) {
                    case CasePatternSwitchLabelSyntax patternLabel:
                    {
                        var binaryOp = HandlePattern(patternLabel.Pattern, comparand);
                        if (hasFallThrough)
                            binaryOp = new Luau.BinaryOperator(fallthroughIdentifier, "or", binaryOp);

                        ifStatements.Add(new Luau.If(binaryOp, new Luau.Block(body)));
                        break;
                    }
                    case CaseSwitchLabelSyntax caseLabel: {
                        var binaryOp = HandleCaseSwitchLabel(caseLabel, comparand);
                        if (hasFallThrough)
                            binaryOp = new Luau.BinaryOperator(fallthroughIdentifier, "or", binaryOp);

                        ifStatements.Add(new Luau.If(binaryOp, new Luau.Block(body)));
                        break;
                    }

                    case DefaultSwitchLabelSyntax:
                        defaultStatements = body;
                        break;
                }
            }
        }

        if (nodeHasFallThrough)
            ifStatements.Insert(0, new Luau.Variable(fallthroughIdentifier, true, Luau.AstUtility.False()));

        if (defaultStatements != null)
            ifStatements.Add(new Luau.ScopedBlock(defaultStatements));

        List<Luau.Statement> blockStatements = [
            new Luau.Repeat(Luau.AstUtility.True(), new Luau.Block(ifStatements))
        ];

        if (createTempVariable)
            blockStatements = blockStatements.Prepend(new Luau.Variable((Luau.IdentifierName)comparand, true, condition)).ToList();

        return new Luau.Block(blockStatements);
    }

    public override Luau.TypeAlias VisitDelegateDeclaration(DelegateDeclarationSyntax node) {
        var parameterTypes = new List<Luau.ParameterType>();

        foreach (var parameter in node.ParameterList.Parameters) {
            if (parameter.Type == null) continue;
            var pType = new Luau.ParameterType(
                parameter.Identifier.Text,
                new Luau.TypeRef(parameter.Type.ToString())
            );

            parameterTypes.Add(pType);
        }

        return new Luau.TypeAlias(
            new Luau.IdentifierName(node.Identifier.Text),
            new Luau.FunctionType(parameterTypes, new Luau.TypeRef(node.ReturnType.ToString()))
        );
    }

    public override Luau.Block VisitEventFieldDeclaration(EventFieldDeclarationSyntax node) {
        var classDeclaration = (ClassDeclarationSyntax)node.Parent!; // this should be temporary.
        var statements = node.Declaration.Variables.Select(variable => new Luau.ExpressionStatement(
            new Luau.Assignment(
                new Luau.MemberAccess(
                    new Luau.IdentifierName(classDeclaration.Identifier.Text),
                    new Luau.IdentifierName(variable.Identifier.Text) 
                ),
                new Luau.Call(
                    new Luau.QualifiedName(new Luau.IdentifierName("Signal"), new Luau.IdentifierName("new")),
                    new Luau.ArgumentList([])
                )
            )
        )).ToList<Luau.Statement>();

        return new Luau.Block(statements);
    }

    private Luau.Expression HandlePattern(PatternSyntax node, Luau.Expression comparand)
    {
        return node switch
        {
            RelationalPatternSyntax relationalPattern => HandleRelationalPattern(relationalPattern, comparand),
            BinaryPatternSyntax binaryPattern => HandleBinaryPattern(binaryPattern, comparand),
            UnaryPatternSyntax unaryPattern => HandleUnaryPattern(unaryPattern, comparand),
            ParenthesizedPatternSyntax parenthesizedPattern => HandleParenthesizedPattern(parenthesizedPattern, comparand),
            ConstantPatternSyntax constantPattern => HandleConstantPattern(constantPattern, comparand),
            TypePatternSyntax typePattern => HandleTypePattern(typePattern, comparand),
            _ => throw Logger.CompilerError($"Unhandled pattern type: {node.GetType().Name}")
        };
    }

    private Luau.Call HandleTypePattern(TypePatternSyntax node, Luau.Expression comparand)
    {
        var name = Visit<Luau.Name>(node.Type);
        var typeInfo = _semanticModel.GetTypeInfo(node.Type);
        if (typeInfo.Type is { ContainingNamespace: { Name: "System" } } && name is Luau.IdentifierName identifierName)
            name = new Luau.IdentifierName('"' + StandardUtility.GetMappedType(identifierName.Text) + '"');

        return Luau.AstUtility.CSCall("is", comparand, name);
    }

    private Luau.BinaryOperator HandleConstantPattern(ConstantPatternSyntax node, Luau.Expression comparand)
    {
        var operand = Visit<Luau.Expression>(node.Expression);
        return new Luau.BinaryOperator(comparand, "==", operand);
    }

    private Luau.Parenthesized HandleParenthesizedPattern(ParenthesizedPatternSyntax node, Luau.Expression comparand) =>
        new Luau.Parenthesized(HandlePattern(node.Pattern, comparand));

    private Luau.UnaryOperator HandleUnaryPattern(UnaryPatternSyntax node, Luau.Expression comparand)
    {
        var operand = HandlePattern(node.Pattern, comparand);
        return new Luau.UnaryOperator("not ", operand);
    }

    private Luau.BinaryOperator HandleBinaryPattern(BinaryPatternSyntax node, Luau.Expression comparand)
    {
        var left = HandlePattern(node.Left, comparand);
        var right = HandlePattern(node.Right, comparand);
        return new Luau.BinaryOperator(left, node.OperatorToken.Text, right);
    }

    private Luau.BinaryOperator HandleRelationalPattern(RelationalPatternSyntax node, Luau.Expression comparand)
    {
        var op = StandardUtility.GetMappedOperator(node.OperatorToken.Text);
        var operand = Visit<Luau.Expression>(node.Expression);
        return new Luau.BinaryOperator(comparand, op, operand);
    }

    private Luau.BinaryOperator HandleCaseSwitchLabel(CaseSwitchLabelSyntax caseLabel, Luau.Expression comparand)
    {
        var caseValue = Visit<Luau.Expression>(caseLabel.Value);
        return new Luau.BinaryOperator(comparand, "==", caseValue);
    }

    public override Luau.Parenthesized VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
    {
        var expression = Visit<Luau.Expression>(node.Expression);
        return new Luau.Parenthesized(expression);
    }

    public override Luau.AnonymousFunction VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Body).Type;
        var returnTypeName = typeSymbol != null
            ? Luau.AstUtility.TypeNameFromSymbol(typeSymbol)
            : null;

        var returnType = new Luau.TypeRef(returnTypeName?.ToString() ?? "nil");
        var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
        var body = node.ExpressionBody != null ?
            new Luau.Block([new Luau.ExpressionStatement(Visit<Luau.Expression>(node.ExpressionBody))])
            : Visit<Luau.Block?>(node.Block);

        return new Luau.AnonymousFunction(parameterList, returnType, body);
    }

    public override Luau.AnonymousFunction VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Body).Type;
        var returnTypeName = typeSymbol != null
            ? Luau.AstUtility.TypeNameFromSymbol(typeSymbol)
            : null;

        var returnType = new Luau.TypeRef(returnTypeName?.ToString() ?? "nil");
        var parameterList = new Luau.ParameterList([Visit<Luau.Parameter>(node.Parameter)]);
        var body = node.ExpressionBody != null
            ? new Luau.Block([new Luau.ExpressionStatement(Visit<Luau.Expression>(node.ExpressionBody))])
            : Visit<Luau.Block?>(node.Block);

        return new Luau.AnonymousFunction(parameterList, returnType, body);
    }

    public override Luau.AnonymousFunction VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Block).Type;
        var returnTypeName = typeSymbol != null
            ? Luau.AstUtility.TypeNameFromSymbol(typeSymbol)
            : null;

        var returnType = new Luau.TypeRef(returnTypeName?.ToString() ?? "nil");
        var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
        var body = node.ExpressionBody != null ?
            new Luau.Block([new Luau.ExpressionStatement(Visit<Luau.Expression>(node.ExpressionBody))])
            : Visit<Luau.Block?>(node.Block);

        return new Luau.AnonymousFunction(parameterList, returnType, body);
    }

    public override Luau.Function VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        var name = Luau.AstUtility.CreateSimpleName(node, registerIdentifier: true);
        var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
        var returnType = Luau.AstUtility.CreateTypeRef(node.ReturnType);
        var body = node.ExpressionBody != null ?
            Visit<Luau.Block>(node.ExpressionBody)
            : Visit<Luau.Block?>(node.Body);

        var attributeLists = node.AttributeLists.Select(Visit<Luau.AttributeList>).ToList();
        return new Luau.Function(name, true, parameterList, returnType, body, attributeLists);
    }

    public override Luau.Parameter VisitParameter(ParameterSyntax node)
    {
        var name = Luau.AstUtility.CreateSimpleName<Luau.IdentifierName>(node, registerIdentifier: true);
        var returnType = Luau.AstUtility.CreateTypeRef(Visit<Luau.Name>(node.Type).ToString());
        var initializer = Visit<Luau.Expression?>(node.Default);
        var isParams = HasSyntax(node.Modifiers, SyntaxKind.ParamsKeyword);

        return new Luau.Parameter(name, isParams, initializer, returnType);
    }

    public override Luau.Node? VisitAttribute(AttributeSyntax node)
    {
        switch (GetName(node))
        {
            case "Native":
                return new Luau.BuiltInAttribute(new Luau.IdentifierName("native"));
        }

        Logger.UnsupportedError(node, "User-defined attributes");
        return null;
    }

    public override Luau.AttributeList VisitAttributeList(AttributeListSyntax node) =>
        new(node.Attributes.Select(Visit).Where(luauNode => luauNode != null)!.ToList<Luau.Node>());

    public override Luau.Statement VisitGlobalStatement(GlobalStatementSyntax node)
    {
        var luauNode = Visit<Luau.Statement>(node.Statement);
        if (HasSyntax(node.Modifiers, SyntaxKind.PublicKeyword))
        {
            // TODO: add to exports
        }

        return luauNode;
    }

    public override Luau.ParameterList VisitParameterList(ParameterListSyntax node) =>
        new(node.Parameters.Select(Visit).OfType<Luau.Parameter>().ToList());

    public override Luau.Statement VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node) =>
        Visit<Luau.Statement>(node.Declaration);

    public override Luau.VariableList VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        var variables = node.Variables.Select(Visit).OfType<Luau.Variable>().ToList();

        return new Luau.VariableList(variables);
    }

    public override Luau.Variable VisitVariableDeclarator(VariableDeclaratorSyntax node)
    {
        var typeRef = Luau.AstUtility.CreateTypeRef(node.Parent switch
        {
            VariableDeclarationSyntax declaration => Visit<Luau.Name>(declaration.Type).ToString(),
            ParameterSyntax parameter => Visit<Luau.Name?>(parameter.Type)?.ToString(),
            _ => null
        } ?? "unknown");


        var initializer = Visit<Luau.Expression?>(node.Initializer);
        return new Luau.Variable(
            Luau.AstUtility.CreateSimpleName<Luau.IdentifierName>(node, registerIdentifier: true),
            true,
            initializer,
            typeRef
        );
    }

    public override Luau.Node? VisitEqualsValueClause(EqualsValueClauseSyntax node) =>
        Visit(node.Value);

    public override Luau.Node VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        var expressionNode = Visit<Luau.Node>(node.Expression);

        return expressionNode is Luau.Expression expression ?
            new Luau.ExpressionStatement(expression)
            : expressionNode;
    }

    public override Luau.InterpolatedString VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
    {
        var parts = node.Contents.Select(Visit<Luau.Expression>).ToList();
        return new Luau.InterpolatedString(parts);
    }

    public override Luau.Literal VisitInterpolatedStringText(InterpolatedStringTextSyntax node) =>
        new(node.TextToken.Text);

    public override Luau.Interpolation VisitInterpolation(InterpolationSyntax node) =>
        new(Visit<Luau.Expression>(node.Expression));

    public override Luau.Expression VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        var valueText = "";
        switch (node.Kind())
        {
            case SyntaxKind.StringLiteralExpression:
            case SyntaxKind.Utf8StringLiteralExpression:
            case SyntaxKind.CharacterLiteralExpression:
            {
                var fullText = node.Token.Text;
                var stringContents = node.Token.ValueText;
                if (fullText.StartsWith('@')) // verbatim strings
                    stringContents = Regex.Escape(stringContents);
                else if (fullText.StartsWith("\"\"\"")) // raw strings
                {
                    var lines = stringContents.Split("\r\n").ToList();
                    var newStringContents = new StringBuilder();
                    var index = 0;
                    foreach (var line in lines)
                    {
                        newStringContents.Append(Regex.Escape(line));
                        if (index++ != lines.Count - 1)
                            newStringContents.Append("\\n");
                    }

                    valueText = $"\"{newStringContents}\"";
                    break;
                }

                valueText = $"\"{stringContents}\"";
                break;
            }

            case SyntaxKind.NullLiteralExpression:
                valueText = "nil";
                break;

            case SyntaxKind.DefaultLiteralExpression:
                var typeSymbol = _semanticModel.GetTypeInfo(node).Type;
                if (typeSymbol == null) break;

                valueText = StandardUtility.GetDefaultValueForType(typeSymbol.Name);
                break;

            default:
                valueText = node.Token.ValueText;
                break;
        }

        return new Luau.Literal(valueText);
    }
}