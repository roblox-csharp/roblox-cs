using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RobloxCS.Macros;
using RobloxCS.Shared;

namespace RobloxCS;

internal enum LinqQueryClauseInfoKind : byte
{
    Select,
    Where,
    OrderBy,
    Group,
    Continuation
}

internal class LinqQueryClauseInfo(LinqQueryClauseInfoKind kind, Luau.IdentifierName name)
{
    public LinqQueryClauseInfoKind Kind { get; } = kind;
    public Luau.IdentifierName Name { get; } = name;
}

public sealed class LuauGenerator(
    SyntaxTree tree,
    CSharpCompilation compiler,
    Luau.TransformState transformState,
    OccupiedIdentifiersStack occupiedIdentifiersStack)
    : BaseGenerator(tree, compiler)
{
    private readonly HashSet<SyntaxKind> _hoistedSyntaxes =
    [
        SyntaxKind.NamespaceDeclaration,
        SyntaxKind.ClassDeclaration,
        SyntaxKind.InterfaceDeclaration,
        SyntaxKind.EnumDeclaration,
        SyntaxKind.LocalFunctionStatement
    ];

    private MacroManager _macro = null!; // hack
    private CSharpCompilation _compiler = compiler;

    public Luau.AST GetLuauAST() => Visit<Luau.AST>(_tree.GetRoot());

    public override Luau.AST VisitCompilationUnit(CompilationUnitSyntax node)
    {
        occupiedIdentifiersStack.Push();
        List<Luau.Statement> result = [new Luau.SingleLineComment(Shared.Constants.HeaderComment + "\n\n")];

        var lastSyntaxTree = node.SyntaxTree;
        var i = 0;
        HashSet<SyntaxNode> alreadyHoisted = [];

        CompilationUnitSyntax loopMembersToHoist(CompilationUnitSyntax root)
        {
            while (true)
            {
                var members = root.Members
                    .Where(m => _hoistedSyntaxes.Contains(m is GlobalStatementSyntax g ? g.Statement.Kind() : m.Kind()))
                    .ToList();
                var nonHoisted = members.Where(m => !alreadyHoisted.Any(m.IsEquivalentTo))
                    .ToList();

                var difference = members.Count - nonHoisted.Count;
                var member = nonHoisted.ElementAtOrDefault(i - difference);
                if (member == null) return root;
                i++;

                if (TryHoistNode(root, member, out var newRoot))
                {
                    alreadyHoisted.Add(member);
                    root = (CompilationUnitSyntax)newRoot;
                    _compiler = _compiler.ReplaceSyntaxTree(lastSyntaxTree, root.SyntaxTree);
                    _semanticModel = _compiler.GetSemanticModel(root.SyntaxTree);
                    _macro = new MacroManager(_semanticModel, transformState, occupiedIdentifiersStack);
                    lastSyntaxTree = root.SyntaxTree;
                }
            }
        }

        void visitMember(MemberDeclarationSyntax member)
        {
            var (statement, prereqStatements) = transformState.Capture(() => Visit<Luau.Statement?>(member));
            if (statement == null)
                throw Logger.CompilerError($"Unhandled syntax node within {member.Kind()}", node);

            if (prereqStatements.Count > 0)
                result.AddRange(prereqStatements);

            result.Add(statement);
        }

        node = loopMembersToHoist(node);
        _compiler = _compiler.ReplaceSyntaxTree(lastSyntaxTree, node.SyntaxTree);
        _semanticModel = _compiler.GetSemanticModel(node.SyntaxTree);
        _macro = new MacroManager(_semanticModel, transformState, occupiedIdentifiersStack);

        if (node.DescendantNodes().Any(descendant =>
                descendant.IsKind(SyntaxKind.EventDeclaration) || descendant.IsKind(SyntaxKind.EventFieldDeclaration)))
        {
            result.Add(Luau.AstUtility.SignalImport());
            result.Add(new Luau.NoOp()); // for the newline
        }

        foreach (var member in node.Members)
            visitMember(member);

        occupiedIdentifiersStack.Pop();
        return new Luau.AST(result);
    }

    public override Luau.TypeRef? VisitPredefinedType(PredefinedTypeSyntax node) =>
        Luau.AstUtility.CreateTypeRef(node.Keyword.Text);

    public override Luau.ArrayType VisitArrayType(ArrayTypeSyntax node) =>
        new(Luau.AstUtility.CreateTypeRef(Visit<Luau.Name>(node.ElementType).ToString())!);

    public override Luau.OptionalType VisitNullableType(NullableTypeSyntax node) =>
        new(Luau.AstUtility.CreateTypeRef(Visit<Luau.Name>(node.ElementType).ToString())!);

    public override Luau.IdentifierName VisitQueryExpression(QueryExpressionSyntax node)
    {
        var resultIdentifier = occupiedIdentifiersStack.AddIdentifier("_result");
        List<Luau.Statement> statements = [];
        statements.AddRange(transformState.CapturePrereqs(() => Visit(node.Body)));
        statements.AddRange(transformState.CapturePrereqs(() => Visit(node.FromClause)));
        transformState.PrereqList(statements);

        return resultIdentifier;
    }

    public override Luau.NoOp VisitQueryBody(QueryBodySyntax node) => new(false);

    public override Luau.NoOp VisitFromClause(FromClauseSyntax node)
    {
        var query = FindFirstAncestor<QueryExpressionSyntax>(node);
        return query == null
            ? new Luau.NoOp(false)
            : HandleQuery(node.Expression, node.Identifier, query.Body);
    }

    private Luau.NoOp HandleQuery(ExpressionSyntax expression, SyntaxToken identifier, QueryBodySyntax body)
    {
        var resultIdentifier = new Luau.IdentifierName(occupiedIdentifiersStack.GetDuplicateText("_result"));
        var iterable = Visit<Luau.Expression>(expression);

        HashSet<LinqQueryClauseInfo> clauseInfos = [];
        void addClauseInfo(SyntaxNode queryClauseSyntax)
        {
            var clauseKind = GetLinqQueryClauseKind(queryClauseSyntax.Kind());
            var identifierName = occupiedIdentifiersStack.Capture(() => occupiedIdentifiersStack.AddIdentifier('_' + clauseKind.ToString().ToLower())).First();
            clauseInfos.Add(new LinqQueryClauseInfo(clauseKind, identifierName));
            transformState.PrereqList(transformState.CapturePrereqs(() => Visit(queryClauseSyntax)));
        }

        foreach (var clause in body.Clauses)
            addClauseInfo(clause);

        addClauseInfo(body.SelectOrGroup);

        occupiedIdentifiersStack.Push();
        var resultValueIdentifier = occupiedIdentifiersStack.AddIdentifier("_resultValue");
        var variableName = occupiedIdentifiersStack.AddIdentifier(identifier);
        List<Luau.Statement> forStatementBody = [
            new Luau.Variable(resultValueIdentifier, true, variableName)
        ];

        var argumentList = new Luau.ArgumentList([new Luau.Argument(variableName)]);
        foreach (var clauseInfo in clauseInfos)
        {
            if (clauseInfo.Kind == LinqQueryClauseInfoKind.OrderBy) continue;

            var call = new Luau.Call(clauseInfo.Name, argumentList);
            switch (clauseInfo.Kind)
            {
                case LinqQueryClauseInfoKind.Select:
                    forStatementBody.Add(new Luau.Assignment(variableName, call));
                    break;
                case LinqQueryClauseInfoKind.Where:
                    forStatementBody.Add(new Luau.If(new Luau.UnaryOperator("not ", call), new Luau.Continue()));
                    break;

                case LinqQueryClauseInfoKind.Continuation:
                case LinqQueryClauseInfoKind.Group:
                case LinqQueryClauseInfoKind.OrderBy:
                default: break;
            }
        }

        forStatementBody.Add(new Luau.ExpressionStatement(Luau.AstUtility.TableCall("insert", resultIdentifier, resultValueIdentifier)));
        List<Luau.Statement> prereqStatements =
        [
            new Luau.Variable(resultIdentifier, true, Luau.TableInitializer.Empty),
            new Luau.For([Luau.AstUtility.DiscardName, variableName], iterable, new Luau.Block(forStatementBody))
        ];

        occupiedIdentifiersStack.Pop();
        transformState.PrereqList(prereqStatements);

        if (body.Continuation != null)
            transformState.PrereqList(transformState.CapturePrereqs(() => Visit(body.Continuation)));

        return new Luau.NoOp(false);
    }

    public override Luau.NoOp VisitGroupClause(GroupClauseSyntax node)
    {
        return new Luau.NoOp(false);
    }

    public override Luau.NoOp VisitWhereClause(WhereClauseSyntax node)
    {
        var query = FindFirstAncestor<QueryExpressionSyntax>(node);
        if (query == null)
            return new Luau.NoOp(false);

        var name = new Luau.IdentifierName(occupiedIdentifiersStack.GetDuplicateText("_where"));
        var parameters = new Luau.ParameterList([
            new Luau.Parameter(
                new Luau.IdentifierName(occupiedIdentifiersStack.GetDuplicateText(query.FromClause.Identifier.Text)))
        ]);

        var condition = Visit<Luau.Expression>(node.Condition);
        var body = new Luau.Block([new Luau.Return(condition)]);
        transformState.Prereq(new Luau.Function(name, true, parameters, new Luau.TypeRef("boolean"), body));

        return new Luau.NoOp(false);
    }

    public override Luau.NoOp VisitSelectClause(SelectClauseSyntax node)
    {
        var query = FindFirstAncestor<QueryExpressionSyntax>(node);
        if (query == null)
            return new Luau.NoOp(false);

        var name = new Luau.IdentifierName(occupiedIdentifiersStack.GetDuplicateText("_select"));
        var parameters = new Luau.ParameterList([
            new Luau.Parameter(
                new Luau.IdentifierName(occupiedIdentifiersStack.GetDuplicateText(query.FromClause.Identifier.Text)))
        ]);

        var expression = Visit<Luau.Expression>(node.Expression);
        var body = new Luau.Block([new Luau.Return(expression)]);
        transformState.Prereq(new Luau.Function(name, true, parameters, null, body));

        return new Luau.NoOp(false);
    }

    public override Luau.NoOp VisitOrderByClause(OrderByClauseSyntax node)
    {
        return new Luau.NoOp(false);
    }

    public override Luau.NoOp VisitQueryContinuation(QueryContinuationSyntax node)
    {
        var query = FindFirstAncestor<QueryExpressionSyntax>(node);
        return query == null
            ? new Luau.NoOp(false)
            : HandleQuery(query.FromClause.Expression, node.Identifier, node.Body);
    }

    public override Luau.Statement VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (!IsStatic(node) || node.Parent is not ClassDeclarationSyntax || node.Initializer == null)
            return new Luau.NoOp();

        var classDeclaration = (ClassDeclarationSyntax)node.Parent!;
        var initializer = Visit<Luau.Expression>(node.Initializer);
        return new Luau.Assignment(
            new Luau.MemberAccess(
                Luau.AstUtility.CreateSimpleName(classDeclaration, noGenerics: true),
                Luau.AstUtility.CreateSimpleName(node, noGenerics: true)
            ),
            initializer
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
            statements.Add(new Luau.Assignment(
                new Luau.MemberAccess(
                    Luau.AstUtility.CreateSimpleName(classDeclaration),
                    Luau.AstUtility.CreateSimpleName(declarator)
                ),
                initializer
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

    public override Luau.Block VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
    {
        var (expression, prereqStatements) = transformState.Capture(() => Visit<Luau.Expression>(node.Expression));
        return new Luau.Block([..prereqStatements, new Luau.Return(expression)]);
    }

    public override Luau.IdentifierName VisitThisExpression(ThisExpressionSyntax node) =>
        new("self");

    // TODO: support initializers
    public override Luau.Call VisitArrayCreationExpression(ArrayCreationExpressionSyntax node) {
        var sizeExpression = node.Type.RankSpecifiers[0].Sizes[0];
        var translatedSize = Visit<Luau.Expression>(sizeExpression);

        return Luau.AstUtility.TableCall("create", new Luau.ArgumentList([new Luau.Argument(translatedSize)]));
    }

    public override Luau.TableInitializer VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node) {
        var initializers = node.Initializer.Expressions.Select(Visit<Luau.Expression>);
        return new Luau.TableInitializer(initializers.ToList());
    }

    // long as hell lol
    public override Luau.Block VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var name = occupiedIdentifiersStack.AddIdentifier(node.Identifier);
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
            new Luau.Assignment(
                nonGenericName,
                new Luau.Call(
                    new Luau.IdentifierName("setmetatable"),
                    Luau.AstUtility.CreateArgumentList([
                        Luau.TableInitializer.Empty,
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
            ),
            new Luau.Assignment(
                new Luau.MemberAccess(
                    nonGenericName,
                    new Luau.IdentifierName("__index")
                ),
                nonGenericName
            ),
            new Luau.Assignment(
                new Luau.MemberAccess(
                    nonGenericName,
                    new Luau.IdentifierName("__className")
                ),
                nameStringLiteral
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
                                            Luau.TableInitializer.Empty,
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

        if (explicitConstructor == null)
            classMemberStatements.Add(constructor);

        classMemberStatements.AddRange(members);

        List<Luau.Statement> statements = [
            new Luau.Variable(nonGenericName, true, null),
            new Luau.ScopedBlock(classMemberStatements),
            Luau.AstUtility.DefineGlobalOrMember(node, nonGenericName),
            new Luau.TypeAlias(name, new Luau.TypeOfCall(nonGenericName))
        ];

        if (node.Parent is CompilationUnitSyntax)
            statements.Add(new Luau.NoOp()); // for the newline

        return new Luau.Block(statements);
    }

    public override Luau.Block VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        List<Luau.Expression> enumKeys = [];
        List<Luau.Expression> enumValues = [];
        // List<Luau.TypeRef> enumTypes = [];
        var index = 0;

        foreach (var member in node.Members)
        {
            var explicitValue = member.EqualsValue?.Value;
            var value = explicitValue?.ToString() ?? index.ToString();
            // enumTypes.Add(new Luau.TypeRef(value));
            enumKeys.Add(Luau.AstUtility.CreateSimpleName(member, member.Identifier.Text));
            enumValues.Add(new Luau.Literal(value));

            index = (explicitValue != null ? int.Parse(explicitValue.ToString()) : index) + 1;
        }

        var name = occupiedIdentifiersStack.AddIdentifier(node.Identifier);
        var enumType = new Luau.TypeOfCall(name);
        var finalType = new Luau.IndexCall(enumType, new Luau.KeyOfCall(enumType));
        List<Luau.Statement> statements =
        [
            new Luau.Variable(
                Luau.AstUtility.GetNonGenericName(name),
                true,
                new Luau.TableInitializer(enumValues, enumKeys, true)
            ),
            Luau.AstUtility.DefineGlobalOrMember(node, name),
            new Luau.TypeAlias(name, finalType)
        ];

        if (node.Parent is CompilationUnitSyntax)
            statements.Add(new Luau.NoOp()); // for the newline

        return new Luau.Block(statements);
    }
    public override Luau.Block VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        var name = occupiedIdentifiersStack.AddIdentifier(node.Name.ToString().Split('.').Last());
        var members = new Luau.Block(node.Members.Select(Visit<Luau.Statement>).ToList());
        List<Luau.Statement> statements =
        [
            new Luau.Variable(name, true, Luau.TableInitializer.Empty),
            Luau.AstUtility.DefineGlobalOrMember(node, name),
            new Luau.TypeAlias(name, new Luau.TypeOfCall(name))
        ];

        if (members.Statements.Count > 0)
            statements.Insert(1, new Luau.ScopedBlock(members.Statements));

        if (node.Parent is CompilationUnitSyntax)
            statements.Add(new Luau.NoOp()); // for the newline

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

    public override Luau.IfExpression VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        var condition = Visit<Luau.Expression>(node.Condition);
        var body = Visit<Luau.Expression>(node.WhenTrue);
        var elseBranch = Visit<Luau.Expression>(node.WhenFalse);

        return new Luau.IfExpression(condition, body, elseBranch);
    }

    public override Luau.IfExpression VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        var comparand = Visit<Luau.Expression>(node.Expression);
        var nil = Luau.AstUtility.Nil();
        var condition = new Luau.BinaryOperator(comparand, "==", nil);
        var elseBranch = Visit<Luau.Expression>(node.WhenNotNull);

        return new Luau.IfExpression(condition, nil, elseBranch);
    }

    public override Luau.If VisitIfStatement(IfStatementSyntax node)
    {
        var condition = Visit<Luau.Expression>(node.Condition);
        var body = Visit<Luau.Statement>(node.Statement);
        var elseBranch = Visit<Luau.Statement?>(node.Else?.Statement);

        return new Luau.If(condition, body, elseBranch);
    }

    public override Luau.Expression VisitIsPatternExpression(IsPatternExpressionSyntax node)
    {
        var expression = Visit<Luau.Expression>(node.Expression);
        return HandlePattern(node.Pattern, expression, node.Expression);
    }

    public override Luau.Statement VisitForStatement(ForStatementSyntax node)
    {
        var (initializer, initializerPrereqs) = transformState.Capture(() => Visit<Luau.VariableList?>(node.Declaration)?.Variables.FirstOrDefault());
        var condition = Visit<Luau.Expression?>(node.Condition) ?? Luau.AstUtility.True();
        var isNumericLoop = initializer is { Initializer: Luau.Literal literal } && int.TryParse(literal.ValueText, out _);
        var incrementByExpression = Visit<Luau.Expression?>(node.Incrementors.FirstOrDefault());
        var body = Visit<Luau.Statement>(node.Statement);

        if (isNumericLoop
            && node.Condition is BinaryExpressionSyntax
            {
                OperatorToken.Text: "<=" or "<"
            } binaryOp
            && incrementByExpression is Luau.BinaryOperator { Operator: "+=" or "-=" } incrementBinaryOp
            && initializerPrereqs.Count == 0)
        {
            var minimum = initializer!.Initializer!;
            var maximum = ((Luau.BinaryOperator)condition).Right;
            if (binaryOp.OperatorToken.Text == "<")
                maximum = Luau.AstUtility.SubtractOne(maximum);

            return new Luau.NumericFor(initializer.Name, minimum, maximum, incrementBinaryOp.Operator == "-=" ? new Luau.Literal("-1") : null, body);
        }

        Luau.Statement? incrementBy = incrementByExpression != null ? new Luau.ExpressionStatement(incrementByExpression) : null;
        var statements = initializerPrereqs;
        if (initializer != null)
            statements.Add(initializer);

        var shouldIncrementIdentifier = occupiedIdentifiersStack.AddIdentifier("_shouldIncrement");
        if (incrementBy != null)
            statements.Add(new Luau.Variable(shouldIncrementIdentifier, true, Luau.AstUtility.False()));

        List<Luau.Statement> whileStatements = [];
        if (incrementBy != null)
        {
            if (incrementBy is Luau.ExpressionStatement { Expression: Luau.BinaryOperator binaryOperator } expressionStatement &&
                !binaryOperator.Operator.Contains('='))
            {
                incrementBy = new Luau.Variable(new Luau.IdentifierName("_"), true, expressionStatement.Expression);
            }
            whileStatements.Add(new Luau.If(shouldIncrementIdentifier, incrementBy, new Luau.Assignment(shouldIncrementIdentifier, Luau.AstUtility.True())));
        }

        whileStatements.Add(new Luau.If(new Luau.UnaryOperator("not ", new Luau.Parenthesized(condition)), new Luau.Break()));
        whileStatements.Add(body);
        statements.Add(new Luau.While(Luau.AstUtility.True(), new Luau.Block(whileStatements)));
        return new Luau.ScopedBlock(statements);
    }

    public override Luau.For VisitForEachStatement(ForEachStatementSyntax node)
    {
        var iterableSymbol = _semanticModel.GetTypeInfo(node.Expression).Type;
        var isList = StandardUtility.DoesTypeInheritFrom(iterableSymbol, "Array")
                     || StandardUtility.DoesTypeInheritFrom(iterableSymbol, "List");

        List<Luau.IdentifierName> names = [Luau.AstUtility.CreateSimpleName<Luau.IdentifierName>(node)];
        if (isList)
            names = names.Prepend(Luau.AstUtility.DiscardName).ToList();

        var iterable = Visit<Luau.Expression>(node.Expression);
        var body = Visit<Luau.Statement>(node.Statement);
        return new Luau.For(names, iterable, body);
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
        new(occupiedIdentifiersStack.AddIdentifier(node.Identifier), true);

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
        return new Luau.Parenthesized(Luau.AstUtility.CreateTypeInfo(type));
    }

    public override Luau.TypeCast VisitCastExpression(CastExpressionSyntax node)
    {
        var expression = Visit<Luau.Expression>(node.Expression);
        var typeName = Visit<Luau.Name>(node.Type);
        return new Luau.TypeCast(expression, Luau.AstUtility.CreateTypeRef(typeName.ToString())!);
    }

    public override Luau.Expression VisitExpressionElement(ExpressionElementSyntax node) =>
        Visit<Luau.Expression>(node.Expression);

    public override Luau.TableInitializer VisitCollectionExpression(CollectionExpressionSyntax node)
    {
        var elements = node.Elements.Select(Visit<Luau.Expression>).ToList();
        return new Luau.TableInitializer(elements);
    }

    public override Luau.Expression VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
    {
        // TODO: handle non-null node.Initializer (prob won't be supported)
        var baseSymbol = _semanticModel.GetSymbolInfo(node).Symbol;
        var classSymbol = baseSymbol?.ContainingSymbol ?? baseSymbol;
        if (classSymbol == null)
            throw Logger.CodegenError(node, "Unable to resolve class symbol for implicit object creation");

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
        if (methodSymbolInfo.Symbol != null && callee is Luau.MemberAccess memberAccess)
            memberAccess.Operator = methodSymbolInfo.Symbol.IsStatic ? '.' : ':';

        List<Luau.Statement> statements = [];
        var arguments = node.ArgumentList.Arguments.Select((arg) => {
            if (!arg.RefKindKeyword.IsKind(SyntaxKind.RefKeyword) && !arg.RefKindKeyword.IsKind(SyntaxKind.OutKeyword))
                return Visit<Luau.Argument>(arg);

            Luau.Variable? variable = null;
            if (arg.Expression.IsKind(SyntaxKind.DeclarationExpression)) {
                variable = Visit<Luau.Variable>(((arg.Expression as DeclarationExpressionSyntax)!.Designation as SingleVariableDesignationSyntax));
                statements.Add(variable);
            }

            return new Luau.Argument(new Luau.AnonymousFunction(new Luau.ParameterList(new([new Luau.Parameter(new Luau.IdentifierName("..."))])), body: new Luau.Block([
                new Luau.Variable(new Luau.IdentifierName("_val"), true, new Luau.IdentifierName("...")),
                new Luau.If(
                    new Luau.BinaryOperator(new Luau.Call(
                        new Luau.IdentifierName("select"),
                        new ([
                            new (new Luau.Literal("\"#\"")),
                            new (new Luau.IdentifierName("..."))
                        ])
                    ), "~=", new Luau.Literal("0")),
                    new Luau.Assignment(variable?.Name ?? Visit<Luau.IdentifierName>(arg.Expression), new Luau.IdentifierName("_val"))
                ),
                new Luau.Return(variable?.Name ?? Visit<Luau.IdentifierName>(arg.Expression))
            ])));
        }).ToList();

        var argumentList = new Luau.ArgumentList(arguments);
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

        if (statements.Count > 0) {
            statements.Add(new Luau.ExpressionStatement(new Luau.Call(callee, argumentList)));
            return new Luau.Block(statements);
        }

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

    private HashSet<string?> GetRefKindParameters(ParameterListSyntax parameterList) {
        return parameterList.Parameters.Select((parameter) => {
            if (parameter.Modifiers.Any(SyntaxKind.RefKeyword) || parameter.Modifiers.Any(SyntaxKind.OutKeyword))
                return parameter.Identifier.Text.ToString();
            return null;
        }).ToHashSet();
    }

    private ParameterListSyntax? GetParameterList(SyntaxNode node) {
        var ancestors = node.Ancestors();
        SyntaxNode? funcNode = null;
        foreach (var ancestorNode in ancestors)
            if (ancestorNode is MethodDeclarationSyntax or LocalFunctionStatementSyntax or AnonymousMethodExpressionSyntax) {
                funcNode = ancestorNode;
                break;
            }

        return funcNode switch {
            MethodDeclarationSyntax method => method.ParameterList,
            LocalFunctionStatementSyntax localFunction => localFunction.ParameterList,
            AnonymousMethodExpressionSyntax anonymousFunction => anonymousFunction.ParameterList,
            _ => null
        };
    }

    public override Luau.Node VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        var expanded = _macro.Assignment(Visit, node);
        if (expanded != null)
            return expanded;

        var mappedOperator = StandardUtility.GetMappedOperator(node.OperatorToken.Text);
        var name = Visit<Luau.AssignmentTarget>(node.Left);
        var value = Visit<Luau.Expression>(node.Right);
        var method = GetParameterList(node);
        Luau.Expression returningName = name;
        Luau.Statement returning = new Luau.ExpressionStatement(new Luau.BinaryOperator(name, mappedOperator, value));

        if (node.IsKind(SyntaxKind.SimpleAssignmentExpression))
            returning = new Luau.Assignment(name, value);

        if (method != null)
        {
            var refKinds = GetRefKindParameters(method);
            if (refKinds.Contains(name.ToString()))
            {
                returning = new Luau.ExpressionStatement(new Luau.Call(name, new([new(value)])));
                returningName = new Luau.Call(name, new([]));
            }
            if (refKinds.Contains(value.ToString()))
                returning = new Luau.Assignment(name, new Luau.Call(value, new([])));
        }

        var isAlone = node.Parent is ExpressionStatementSyntax;
        if (!isAlone)
            transformState.Prereq(returning);

        return isAlone ? returning : returningName;
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
        var name = Luau.AstUtility.GetNonGenericName(Visit<Luau.SimpleName>(node.Name));
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
        var indexPlusOne = Luau.AstUtility.AddOne(index);
        var elementAccess = new Luau.ElementAccess(expression, indexPlusOne);
        return Luau.AstUtility.DiscardVariableIfExpressionStatement(node, elementAccess, node.Parent);
    }

    public override Luau.QualifiedName VisitQualifiedName(QualifiedNameSyntax node)
    {
        var left = Visit<Luau.Name>(node.Left);
        var right = Visit<Luau.IdentifierName>(node.Right);
        return new Luau.QualifiedName(left, right);
    }

    public override Luau.Node VisitIdentifierName(IdentifierNameSyntax node)
    {
        var method = FindFirstAncestor<MethodDeclarationSyntax>(node);
        node = node.WithIdentifier(
            SyntaxFactory.Identifier(occupiedIdentifiersStack.GetDuplicateText(node.Identifier.Text)));

        var name = Luau.AstUtility.CreateSimpleName(node);
        if (method != null && node.Parent is not AssignmentExpressionSyntax) {
            var refKinds = GetRefKindParameters(method.ParameterList);
            if (refKinds.Contains(node.Identifier.Text))
                return new Luau.Call(name, new([]));
        }

        var classDeclaration = FindFirstAncestor<ClassDeclarationSyntax>(node);
        var isClassMember = classDeclaration != null
                            && classDeclaration.Members.Any(member => member is not ConstructorDeclarationSyntax && TryGetName(member) == GetName(node))
                            && node.Parent is not MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax };

        // this is so cooked
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
    public override Luau.Return VisitReturnStatement(ReturnStatementSyntax node) =>new(Visit<Luau.Expression?>(node.Expression));

    public override Luau.Block VisitBlock(BlockSyntax node)
    {
        occupiedIdentifiersStack.Push();
        var statements = node.Statements.Select(statement => {
            var (visitedStatement, prereqStatements) = transformState.Capture(() => Visit<Luau.Statement>(statement));
            if (prereqStatements.Count <= 0)
                return visitedStatement;

            var newStatements = prereqStatements.ToList();
            newStatements.Add(visitedStatement);
            return new Luau.Block(newStatements);
        }).ToList();

        occupiedIdentifiersStack.Pop();
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
        var operandType = _semanticModel.GetTypeInfo(node.Operand).Type;
        if (node.OperatorToken.Text == "!")
        {
            var nonOptionalType = Luau.AstUtility.CreateTypeRef(operandType != null ? operandType.Name.Replace("?", "") : "any")!;
            return new Luau.TypeCast(operand, nonOptionalType);
        }


        var originalIdentifier = occupiedIdentifiersStack.AddIdentifier("_original");
        var mappedOperator = StandardUtility.GetMappedOperator(node.OperatorToken.Text);
        var increment = new Luau.BinaryOperator(operand, mappedOperator, new Luau.Literal("1"));
        var isAlone = node.Parent is ExpressionStatementSyntax or ForStatementSyntax;
        if (!isAlone)
            transformState.PrereqList([
                new Luau.Variable(originalIdentifier, true, operand),
                new Luau.ExpressionStatement(increment)
            ]);

        return isAlone ? increment : originalIdentifier;
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

        // TODO: prefix increment/decrement
        var mappedOperator = StandardUtility.GetMappedOperator(operatorText);
        var bit32MethodName = StandardUtility.GetBit32MethodName(mappedOperator);
        if (bit32MethodName != null)
            return Luau.AstUtility.Bit32Call(bit32MethodName, operand);

        return new Luau.UnaryOperator(mappedOperator, operand);
    }

    public override Luau.Expression VisitSwitchExpression(SwitchExpressionSyntax node) {
        var createTempVariable = node.GoverningExpression is not IdentifierNameSyntax && node.GoverningExpression is not LiteralExpressionSyntax;
        var condition = Visit<Luau.Expression>(node.GoverningExpression);
        var newValueIdentifier = new Luau.IdentifierName("_newValue");
        var comparand = createTempVariable ?
            occupiedIdentifiersStack.AddIdentifier("_exp")
            : condition;

        List<Luau.Statement> statements = [];
        List<Luau.Statement> prereqStatements = [
            new Luau.Variable(newValueIdentifier, true)
        ];

        SwitchExpressionArmSyntax? discardPattern = null;
        foreach (var section in node.Arms) {
            if (section.Pattern is DiscardPatternSyntax) {
                discardPattern = section;
                continue;
            }

            var binaryOp = HandlePattern(section.Pattern, comparand, node.GoverningExpression);
            statements.Add(new Luau.If(binaryOp, new Luau.Block([
                new Luau.Assignment(newValueIdentifier, Visit<Luau.Expression>(section.Expression)),
                new Luau.Break()
            ])));
        }

        if (createTempVariable)
            statements.Insert(0, new Luau.Variable((Luau.IdentifierName)comparand, true, condition));

        if (discardPattern != null)
            statements.Add(new Luau.Assignment(newValueIdentifier, Visit<Luau.Expression>(discardPattern.Expression)));

        prereqStatements.Add(new Luau.Repeat(Luau.AstUtility.True(), new Luau.Block(statements)));
        transformState.PrereqList(prereqStatements);

        return newValueIdentifier;
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
            occupiedIdentifiersStack.AddIdentifier("_exp")
            : condition;

        var anyNodeHasFallThrough = node.Sections.Any(section => section.Labels.Count > 1);
        var fallthroughIdentifier = anyNodeHasFallThrough ? occupiedIdentifiersStack.AddIdentifier("_fallthrough") : null;
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
                    body.Insert(0, new Luau.Assignment(fallthroughIdentifier!, Luau.AstUtility.True()));
                }

                switch (label) {
                    case CasePatternSwitchLabelSyntax patternLabel:
                    {
                        var binaryOp = HandlePattern(patternLabel.Pattern, comparand, node.Expression);
                        if (hasFallThrough)
                            binaryOp = new Luau.BinaryOperator(fallthroughIdentifier!, "or", binaryOp);

                        ifStatements.Add(new Luau.If(binaryOp, new Luau.Block(body)));
                        break;
                    }
                    case CaseSwitchLabelSyntax caseLabel: {
                        var binaryOp = HandleCaseSwitchLabel(caseLabel, comparand);
                        if (hasFallThrough)
                            binaryOp = new Luau.BinaryOperator(fallthroughIdentifier!, "or", binaryOp);

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
            ifStatements.Insert(0, new Luau.Variable(fallthroughIdentifier!, true, Luau.AstUtility.False()));

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
        var statements = node.Declaration.Variables.Select(variable =>
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
        ).ToList<Luau.Statement>();

        return new Luau.Block(statements);
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
        
        var returnType = new Luau.TypeRef(returnTypeName?.ToString() ?? "()");
        var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
        Luau.Block? body;
        if (node.ExpressionBody != null)
        {
            var (expression, prereqStatements) =
                transformState.Capture(() => Visit<Luau.Expression>(node.ExpressionBody));

            body = new Luau.Block([..prereqStatements, new Luau.Return(expression)]);
        }
        else
            body = Visit<Luau.Block?>(node.Block);

        return new Luau.AnonymousFunction(parameterList, returnType, body);
    }

    public override Luau.AnonymousFunction VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Body).Type;
        var returnTypeName = typeSymbol != null
            ? Luau.AstUtility.TypeNameFromSymbol(typeSymbol)
            : null;

        var returnType = new Luau.TypeRef(returnTypeName?.ToString() ?? "()");
        var parameterList = new Luau.ParameterList([Visit<Luau.Parameter>(node.Parameter)]);
        Luau.Block? body;
        if (node.ExpressionBody != null)
        {
            var (expression, prereqStatements) =
                transformState.Capture(() => Visit<Luau.Expression>(node.ExpressionBody));

            body = new Luau.Block([..prereqStatements, new Luau.Return(expression)]);
        }
        else
            body = Visit<Luau.Block?>(node.Block);

        return new Luau.AnonymousFunction(parameterList, returnType, body);
    }

    public override Luau.AnonymousFunction VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Block).Type;
        var returnTypeName = typeSymbol != null
            ? Luau.AstUtility.TypeNameFromSymbol(typeSymbol)
            : null;

        var returnType = new Luau.TypeRef(returnTypeName?.ToString() ?? "()");
        var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
        var body = node.ExpressionBody != null ?
            new Luau.Block([new Luau.ExpressionStatement(Visit<Luau.Expression>(node.ExpressionBody))])
            : Visit<Luau.Block?>(node.Block);

        return new Luau.AnonymousFunction(parameterList, returnType, body);
    }

    public override Luau.Function VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        var name = occupiedIdentifiersStack.AddIdentifier(node.Identifier);
        var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
        var typeParameters = node.TypeParameterList?.Parameters.Select(p => new Luau.IdentifierName(p.Identifier.Text)).ToList();
        var returnType = Luau.AstUtility.CreateTypeRef(node.ReturnType);
        var body = node.ExpressionBody != null
            ? Visit<Luau.Block>(node.ExpressionBody)
            : Visit<Luau.Block?>(node.Body);

        var attributeLists = node.AttributeLists.Select(Visit<Luau.AttributeList>).ToList();
        return new Luau.Function(name, true, parameterList, returnType, body, attributeLists, typeParameters);
    }

    public override Luau.Parameter VisitParameter(ParameterSyntax node)
    {
        var name = occupiedIdentifiersStack.AddIdentifier(node.Identifier);
        Luau.TypeRef? type = null;
        if (node.Type != null)
            type = Luau.AstUtility.CreateTypeRef(Visit<Luau.Name>(node.Type).ToString());

        var initializer = Visit<Luau.Expression?>(node.Default);
        var isParams = HasSyntax(node.Modifiers, SyntaxKind.ParamsKeyword);
        if (type != null && node.Modifiers.Any(SyntaxKind.RefKeyword) || node.Modifiers.Any(SyntaxKind.OutKeyword))
            type = new Luau.FunctionType([new Luau.ParameterType(null, new Luau.OptionalType(type!))], type!);

        if (isParams && type is Luau.ArrayType arrayType)
            type = arrayType.ElementType;

        if (initializer != null && type != null)
            type = new Luau.OptionalType(type);

        return new Luau.Parameter(name, isParams, initializer, type);
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
        new(node.Attributes.Select(Visit<Luau.Statement?>).Where(luauNode => luauNode != null)!.ToList<Luau.Statement>());

    public override Luau.Statement VisitGlobalStatement(GlobalStatementSyntax node)
    {
        var luauNode = Visit<Luau.Statement>(node.Statement);
        if (HasSyntax(node.Modifiers, SyntaxKind.PublicKeyword))
        {
            // TODO: add to exports
        }

        return luauNode;
    }

    public override Luau.ParameterList VisitParameterList(ParameterListSyntax node)
    {
        occupiedIdentifiersStack.Push();
        var parameterList = new Luau.ParameterList(node.Parameters.Select(Visit).OfType<Luau.Parameter>().ToList());
        occupiedIdentifiersStack.Pop();
        return parameterList;
    }

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
        });

        var identifierName = occupiedIdentifiersStack.AddIdentifier(node.Identifier);
        var initializer = Visit<Luau.Expression?>(node.Initializer);
        return new Luau.Variable(
            identifierName,
            true,
            initializer,
            typeRef
        );
    }

    public override Luau.Node? VisitEqualsValueClause(EqualsValueClauseSyntax node) => Visit(node.Value);

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

    // extremely skidded
    private bool TryHoistNode(SyntaxNode root, SyntaxNode nodeToHoist, [NotNullWhen(true)] out SyntaxNode? newRoot)
    {
        newRoot = null;
        var originalNodeToHoist = nodeToHoist;
        if (nodeToHoist is GlobalStatementSyntax globalStatement)
            nodeToHoist = globalStatement.Statement;

        var shouldHoist = _hoistedSyntaxes.Contains(nodeToHoist.Kind());
        if (!shouldHoist)
            return false;

        var hoistTarget = GetHoistInsertionTarget(nodeToHoist);
        if (hoistTarget == null)
            return false; // nothing to do

        var originalParent = originalNodeToHoist.Parent;
        if (originalParent == null)
            return false;

        var modifiedRoot = root
            .TrackNodes(originalNodeToHoist, originalParent, hoistTarget)
            .RemoveNode(originalNodeToHoist, SyntaxRemoveOptions.KeepNoTrivia)!;

        var updatedNodeToHoist = modifiedRoot.GetCurrentNode(originalNodeToHoist);
        if (updatedNodeToHoist == null)
        {
            newRoot = modifiedRoot;
            return true;
        }

        var updatedParent = modifiedRoot.GetCurrentNode(originalParent);
        if (updatedParent == null)
        {
            newRoot = modifiedRoot;
            return true;
        }

        var updatedTarget = modifiedRoot.GetCurrentNode(hoistTarget);
        if (updatedTarget == null)
        {
            newRoot = modifiedRoot;
            return true;
        }

        var container = updatedTarget.Parent;
        if (container == null)
        {
            newRoot = modifiedRoot;
            return true;
        }

        var newNode = updatedNodeToHoist.WithoutTrivia().NormalizeWhitespace();
        switch (container) // insert logic depends on the type of container
        {
            case BlockSyntax block:
            {
                var statements = block.Statements;
                if (statements.Count <= 1) return false;

                var targetIndex = statements.IndexOf((StatementSyntax)updatedTarget);
                var newStatements = statements.Insert(targetIndex, (StatementSyntax)newNode);
                var newBlock = block.WithStatements(newStatements);
                newRoot = modifiedRoot.ReplaceNode(block, newBlock);
                return true;
            }

            case ClassDeclarationSyntax declaration:
            {
                var members = declaration.Members;
                if (members.Count <= 1) return false;

                var targetIndex = members.IndexOf((MemberDeclarationSyntax)updatedTarget);
                var newMembers = members.Insert(targetIndex, (MemberDeclarationSyntax)newNode);
                var newClass = declaration.WithMembers(newMembers);
                newRoot = modifiedRoot.ReplaceNode(declaration, newClass);

                return true;
            }

            case NamespaceDeclarationSyntax declaration:
            {
                if (declaration.Members.Count <= 1) return false;
                var targetIndex = updatedTarget is MemberDeclarationSyntax t
                    ? declaration.Members.IndexOf(t)
                    : declaration.Usings.IndexOf((UsingDirectiveSyntax)updatedTarget);

                var newDeclaration = updatedNodeToHoist is MemberDeclarationSyntax n
                    ? declaration.WithMembers(declaration.Members
                        .Remove(n)
                        .Insert(targetIndex, (MemberDeclarationSyntax)newNode))
                    : declaration.WithUsings(declaration.Usings
                        .Remove((UsingDirectiveSyntax)updatedNodeToHoist)
                        .Insert(targetIndex, (UsingDirectiveSyntax)newNode));

                newRoot = modifiedRoot.ReplaceNode(declaration, newDeclaration);
                return true;
            }

            case CompilationUnitSyntax compilationUnit:
            {
                if (compilationUnit.Members.Count <= 1) return false;
                var targetIndex = updatedTarget is MemberDeclarationSyntax t
                    ? compilationUnit.Members.IndexOf(t)
                    : compilationUnit.Usings.IndexOf((UsingDirectiveSyntax)updatedTarget);

                // GARBAGE
                newRoot = updatedNodeToHoist is MemberDeclarationSyntax n
                    ? compilationUnit.WithMembers(compilationUnit.Members
                        .Remove(n)
                        .Insert(targetIndex, (MemberDeclarationSyntax)newNode))
                    : compilationUnit.WithUsings(compilationUnit.Usings
                        .Remove((UsingDirectiveSyntax)updatedNodeToHoist)
                        .Insert(targetIndex, (UsingDirectiveSyntax)newNode));

                return true;
            }

            default:
                newRoot = modifiedRoot;
                return true;
        }
    }

    private SyntaxNode? GetHoistInsertionTarget(SyntaxNode node)
    {
        var location = FindHoistTarget(node);
        if (location == null) return null;

        var scope = node.FirstAncestorOrSelf<SyntaxNode>(n =>
            n is BlockSyntax or ClassDeclarationSyntax or NamespaceDeclarationSyntax or CompilationUnitSyntax);

        return scope?
            .ChildNodes()
            .FirstOrDefault(n => n.SpanStart >= location.SourceSpan.Start);
    }

    private Location? FindHoistTarget(SyntaxNode node)
    {
        var scope = node.FirstAncestorOrSelf<SyntaxNode>(n =>
            n is BlockSyntax or ClassDeclarationSyntax or NamespaceDeclarationSyntax or CompilationUnitSyntax);
        if (scope == null) return null;

        var calledMethods = node
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(inv =>
            {
                var symbol = _semanticModel.GetSymbolInfo(inv).Symbol;
                return symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            })
            .Where(syntax => syntax != null && scope.Span.Contains(syntax.Span))
            .Distinct()
            .ToList();

        var referencedSymbols = node
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Select(id => _semanticModel.GetSymbolInfo(id).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax())
            .Where(s => s != null && scope.Span.Contains(s.Span))
            .Distinct()
            .ToList();

        var dependencies = calledMethods
            .Concat(referencedSymbols)
            .Where(dep => dep?.SpanStart < node.SpanStart)
            .OrderBy(d => d?.SpanStart)
            .ToList();

        if (dependencies.Count == 0)
            return Location.Create(node.SyntaxTree, new TextSpan(scope.SpanStart, 0));

        var lastDependency = dependencies.MaxBy(d => d?.Span.End)!;
        return Location.Create(node.SyntaxTree, new TextSpan(lastDependency.Span.End + 1, 0));
    }

    private Luau.Expression HandlePattern(PatternSyntax node, Luau.Expression comparand, ExpressionSyntax originalComparand)
    {
        return node switch
        {
            RelationalPatternSyntax relationalPattern => HandleRelationalPattern(relationalPattern, comparand),
            BinaryPatternSyntax binaryPattern => HandleBinaryPattern(binaryPattern, comparand, originalComparand),
            UnaryPatternSyntax unaryPattern => HandleUnaryPattern(unaryPattern, comparand, originalComparand),
            ParenthesizedPatternSyntax parenthesizedPattern => HandleParenthesizedPattern(parenthesizedPattern, comparand, originalComparand),
            ConstantPatternSyntax constantPattern => HandleConstantPattern(constantPattern, comparand),
            TypePatternSyntax typePattern => HandleTypePattern(typePattern, comparand, originalComparand),
            DeclarationPatternSyntax declarationPattern => HandleDeclarationPattern(declarationPattern, comparand, originalComparand),
            _ => throw Logger.CompilerError($"Unhandled pattern type: {node.GetType().Name}", node)
        };
    }

    private Luau.Call HandleDeclarationPattern(DeclarationPatternSyntax node, Luau.Expression originalValue, ExpressionSyntax csharpOriginalValue)
    {
        var typeName = Visit<Luau.Name>(node.Type);
        var oldTypeName = typeName;
        var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type;
        if (typeSymbol is { ContainingNamespace.Name: "System" or "Roblox" } && typeName is Luau.IdentifierName identifierName)
            typeName = new Luau.IdentifierName('"' + StandardUtility.GetMappedType(identifierName.Text) + '"');

        var newName = node.Designation switch
        {
            SingleVariableDesignationSyntax singleDesignation =>
                occupiedIdentifiersStack.AddIdentifier(singleDesignation.Identifier)
        };

        transformState.Prereq(new Luau.Variable(newName, true, new Luau.TypeCast(originalValue, new Luau.TypeRef(oldTypeName.ToString()))));
        return PatternIsType(originalValue, csharpOriginalValue, typeName);
    }

    private Luau.Call HandleTypePattern(TypePatternSyntax node, Luau.Expression comparand, ExpressionSyntax originalComparand)
    {
        var name = Visit<Luau.Name>(node.Type);
        var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type; // TODO: some sort of runtime type check
        if (typeSymbol is { ContainingNamespace.Name: "System" or "Roblox" } && name is Luau.IdentifierName identifierName)
            name = new Luau.IdentifierName('"' + StandardUtility.GetMappedType(identifierName.Text) + '"');
        
        return PatternIsType(comparand, originalComparand, name);
    }
    
    private Luau.Call PatternIsType(Luau.Expression originalValue, ExpressionSyntax csharpOriginalValue, Luau.Name typeName)
    {
        var instanceSymbol = _semanticModel.Compilation.GetTypeByMetadataName("Roblox.Instance");
        var originalValueSymbol = _semanticModel.GetTypeInfo(csharpOriginalValue).Type;
        var isInstanceValue = originalValueSymbol != null
                              && instanceSymbol != null
                              && StandardUtility.DoesTypeInheritFrom(originalValueSymbol, instanceSymbol)
                              && typeName.ToString() != "\"Instance\"";
        
        return isInstanceValue
            ? new Luau.Call(new Luau.MemberAccess(originalValue, new Luau.IdentifierName("IsA"), ':'), new Luau.ArgumentList([new Luau.Argument(typeName)]))
            : Luau.AstUtility.Is(originalValue, typeName); // TODO: some sort of runtime type check
    }

    private Luau.BinaryOperator HandleConstantPattern(ConstantPatternSyntax node, Luau.Expression comparand)
    {
        var operand = Visit<Luau.Expression>(node.Expression);
        return new Luau.BinaryOperator(comparand, "==", operand);
    }

    private Luau.Parenthesized HandleParenthesizedPattern(ParenthesizedPatternSyntax node, Luau.Expression comparand, ExpressionSyntax originalComparand) =>
        new(HandlePattern(node.Pattern, comparand, originalComparand));

    private Luau.UnaryOperator HandleUnaryPattern(UnaryPatternSyntax node, Luau.Expression comparand, ExpressionSyntax originalComparand)
    {
        var operand = HandlePattern(node.Pattern, comparand, originalComparand);
        return new Luau.UnaryOperator("not ", operand);
    }

    private Luau.BinaryOperator HandleBinaryPattern(BinaryPatternSyntax node, Luau.Expression comparand, ExpressionSyntax originalComparand)
    {
        var left = HandlePattern(node.Left, comparand, originalComparand);
        var right = HandlePattern(node.Right, comparand, originalComparand);
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

    private static LinqQueryClauseInfoKind GetLinqQueryClauseKind(SyntaxKind syntaxKind)
    {
        return syntaxKind switch
        {
            SyntaxKind.SelectClause => LinqQueryClauseInfoKind.Select,
            SyntaxKind.WhereClause => LinqQueryClauseInfoKind.Where,
            SyntaxKind.OrderByClause => LinqQueryClauseInfoKind.OrderBy,
            SyntaxKind.GroupClause => LinqQueryClauseInfoKind.Continuation,
            SyntaxKind.QueryContinuation => LinqQueryClauseInfoKind.Continuation,
        };
    }
}