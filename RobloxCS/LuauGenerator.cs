using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RobloxCS.Luau;
using RobloxCS.Macros;
using RobloxCS.Shared;
using Expression = RobloxCS.Luau.Expression;

namespace RobloxCS;

internal enum LinqQueryClauseInfoKind : byte
{
    Select,
    Where,
    OrderBy,
    GroupBy,
    Continuation
}

internal class LinqQueryClauseInfo(LinqQueryClauseInfoKind kind, IdentifierName name)
{
    public LinqQueryClauseInfoKind Kind { get; } = kind;
    public IdentifierName Name { get; } = name;
}

public sealed class LuauGenerator(
    FileCompilation file,
    CSharpCompilation compiler)
    : BaseGenerator(file, compiler)
{
    private readonly HashSet<SyntaxKind> _hoistedSyntaxes =
    [
        SyntaxKind.NamespaceDeclaration,
        SyntaxKind.ClassDeclaration,
        SyntaxKind.InterfaceDeclaration,
        SyntaxKind.EnumDeclaration,
        SyntaxKind.LocalFunctionStatement
    ];

    private CSharpCompilation _compiler = compiler;

    private MacroManager _macro = null!; // hack

    public AST GetLuauAST() => Visit<AST>(_file.Tree.GetRoot());

    public override AST VisitCompilationUnit(CompilationUnitSyntax node)
    {
        _file.OccupiedIdentifiers.Push();
        List<Statement> result = [new SingleLineComment(Constants.HeaderComment + "\n\n")];

        var lastSyntaxTree = node.SyntaxTree;
        var i = 0;
        HashSet<SyntaxNode> alreadyHoisted = [];

        // TODO: handle nested types
        CompilationUnitSyntax loopMembersToHoist(CompilationUnitSyntax root)
        {
            while (true)
            {
                var members = root.Members
                                  .Where(m => _hoistedSyntaxes.Contains(m is GlobalStatementSyntax g
                                                                            ? g.Statement.Kind()
                                                                            : m.Kind()))
                                  .ToList();

                var nonHoisted = members.Where(m => !alreadyHoisted.Any(m.IsEquivalentTo))
                                        .ToList();

                var difference = members.Count - nonHoisted.Count;
                var member = nonHoisted.ElementAtOrDefault(i - difference);
                if (member == null) return root;

                i++;
                if (!TryHoistNode(root, member, out var newRoot)) continue;

                alreadyHoisted.Add(member);
                root = (CompilationUnitSyntax)newRoot;
                _compiler = _compiler.ReplaceSyntaxTree(lastSyntaxTree, root.SyntaxTree);
                _semanticModel = _compiler.GetSemanticModel(root.SyntaxTree);
                _macro = new MacroManager(_semanticModel, _file);
                lastSyntaxTree = root.SyntaxTree;
            }
        }

        void visitMember(MemberDeclarationSyntax member)
        {
            var (statement, prereqStatements) = _file.Prerequisites.Capture(() => Visit<Statement?>(member));
            if (statement == null)
                throw Logger.CompilerError($"Unhandled syntax node within {member.Kind()}", node);

            if (prereqStatements.Count > 0)
                result.AddRange(prereqStatements);

            result.Add(statement);
        }

        node = loopMembersToHoist(node);
        _compiler = _compiler.ReplaceSyntaxTree(lastSyntaxTree, node.SyntaxTree);
        _semanticModel = _compiler.GetSemanticModel(node.SyntaxTree);
        _macro = new MacroManager(_semanticModel, _file);

        if (node.DescendantNodes()
                .Any(descendant => descendant.IsKind(SyntaxKind.EventDeclaration)
                                || descendant.IsKind(SyntaxKind.EventFieldDeclaration)))
        {
            result.Add(AstUtility.SignalImport());
            result.Add(new NoOp()); // for the newline
        }

        foreach (var member in node.Members) visitMember(member);

        _file.OccupiedIdentifiers.Pop();

        return new AST(result);
    }

    public override TypeRef? VisitPredefinedType(PredefinedTypeSyntax node) => AstUtility.CreateTypeRef(node.Keyword.Text);

    public override ArrayType VisitArrayType(ArrayTypeSyntax node) => new(AstUtility.CreateTypeRef(Visit<Name>(node.ElementType).ToString())!);

    public override OptionalType VisitNullableType(NullableTypeSyntax node) => new(AstUtility.CreateTypeRef(Visit<Name>(node.ElementType).ToString())!);

    public override TableInitializer VisitTupleExpression(TupleExpressionSyntax node)
    {
        var expressions = node.Arguments.Select(argument => Visit<Expression>(argument.Expression)).ToList();
        return new TableInitializer(expressions, expressions.Select((_, index) => new IdentifierName($"Item{index + 1}")).ToList<Expression>());
    }

    public override IdentifierName VisitQueryExpression(QueryExpressionSyntax node)
    {
        List<Statement> statements = [];
        statements.AddRange(_file.Prerequisites.CaptureOnlyPrereqs(() => Visit(node.Body)));
        statements.AddRange(_file.Prerequisites.CaptureOnlyPrereqs(() => Visit(node.FromClause)));
        _file.Prerequisites.AddList(statements);

        return new IdentifierName(_file.OccupiedIdentifiers.GetDuplicateText("_result"));
    }

    public override NoOp VisitQueryBody(QueryBodySyntax node) => new(false);

    public override Node? VisitFromClause(FromClauseSyntax node)
    {
        var query = FindFirstAncestor<QueryExpressionSyntax>(node);

        return query == null
            ? null
            : HandleQuery(node.Expression, node.Identifier, query.Body);
    }

    public override Node? VisitGroupClause(GroupClauseSyntax node)
    {
        var query = FindFirstAncestor<QueryExpressionSyntax>(node);

        if (query == null) return null;

        var name = new IdentifierName(_file.OccupiedIdentifiers.GetDuplicateText("_groupby"));
        var paramText = FindFirstAncestor<QueryContinuationSyntax>(node) is { } continuation
            ? continuation.Identifier.Text
            : query.FromClause.Identifier.Text;

        _file.OccupiedIdentifiers.Push();
        var parameters = new ParameterList([new Parameter(_file.OccupiedIdentifiers.AddIdentifier(node, paramText))]);

        var byKey = Visit<Expression>(node.ByExpression);
        var byValue = Visit<Expression>(node.GroupExpression);
        var groupingInfo = new TableInitializer([byKey, byValue],
                                                [new IdentifierName("key"), new IdentifierName("value")]);

        var body = new Block([new Return(groupingInfo)]);
        _file.Prerequisites.Add(new Function(name,
                                             true,
                                             parameters,
                                             null,
                                             body));

        _file.OccupiedIdentifiers.Pop();

        return null;
    }

    public override Node? VisitWhereClause(WhereClauseSyntax node)
    {
        var query = FindFirstAncestor<QueryExpressionSyntax>(node);

        if (query == null) return null;

        var name = new IdentifierName(_file.OccupiedIdentifiers.GetDuplicateText("_where"));
        var paramText = FindFirstAncestor<QueryContinuationSyntax>(node) is { } continuation
            ? continuation.Identifier.Text
            : query.FromClause.Identifier.Text;

        _file.OccupiedIdentifiers.Push();
        var parameters = new ParameterList([new Parameter(_file.OccupiedIdentifiers.AddIdentifier(node, paramText))]);

        var condition = Visit<Expression>(node.Condition);
        var body = new Block([new Return(condition)]);
        _file.Prerequisites.Add(new Function(name,
                                             true,
                                             parameters,
                                             new TypeRef("boolean"),
                                             body));

        _file.OccupiedIdentifiers.Pop();

        return null;
    }

    public override Node? VisitSelectClause(SelectClauseSyntax node)
    {
        var query = FindFirstAncestor<QueryExpressionSyntax>(node);

        if (query == null) return null;

        var name = new IdentifierName(_file.OccupiedIdentifiers.GetDuplicateText("_select"));
        var paramText = FindFirstAncestor<QueryContinuationSyntax>(node) is { } continuation
            ? continuation.Identifier.Text
            : query.FromClause.Identifier.Text;

        _file.OccupiedIdentifiers.Push();
        var parameters = new ParameterList([new Parameter(_file.OccupiedIdentifiers.AddIdentifier(node, paramText))]);

        var expression = Visit<Expression>(node.Expression);
        var body = new Block([new Return(expression)]);
        _file.Prerequisites.Add(new Function(name,
                                             true,
                                             parameters,
                                             null,
                                             body));

        _file.OccupiedIdentifiers.Pop();

        return null;
    }

    public override Node? VisitOrderByClause(OrderByClauseSyntax node)
    {
        var query = FindFirstAncestor<QueryExpressionSyntax>(node);

        if (query == null) return null;

        var name = new IdentifierName(_file.OccupiedIdentifiers.GetDuplicateText("_orderby"));
        var paramText = FindFirstAncestor<QueryContinuationSyntax>(node) is { } continuation
            ? continuation.Identifier.Text
            : query.FromClause.Identifier.Text;

        _file.OccupiedIdentifiers.Push();
        var parameters = new ParameterList([new Parameter(_file.OccupiedIdentifiers.AddIdentifier(node, paramText))]);

        var expressions = node.Orderings.Select(ordering => Visit<Expression>(ordering.Expression)).ToList();
        var body = new Block([new Return(new TableInitializer(expressions))]);
        _file.OccupiedIdentifiers.Pop();

        _file.OccupiedIdentifiers.Push();
        var a = _file.OccupiedIdentifiers.AddIdentifier("a");
        var b = _file.OccupiedIdentifiers.AddIdentifier("b");
        var orderedA = _file.OccupiedIdentifiers.AddIdentifier("_orderedA");
        var orderedB = _file.OccupiedIdentifiers.AddIdentifier("_orderedB");
        _file.OccupiedIdentifiers.Pop();

        var i = 1;
        var orderClauses = node.Orderings.Select(ordering =>
        {
            var comparator = ordering.IsKind(SyntaxKind.DescendingOrdering) ? ">" : "<";
            var orderingA = new ElementAccess(orderedA, new Literal(i.ToString()));
            var orderingB = new ElementAccess(orderedB, new Literal(i.ToString()));
            i++;

            return new If(new BinaryOperator(orderingA, "~=", orderingB),
                          new Block([new Return(new BinaryOperator(orderingA, comparator, orderingB))]));
        });

        var comparatorName = _file.OccupiedIdentifiers.AddIdentifier(name.Text + "Comparator");
        _file.Prerequisites.Add(new Function(name,
                                             true,
                                             parameters,
                                             null,
                                             body));

        _file.Prerequisites.Add(new Function(comparatorName,
                                             true,
                                             new ParameterList([new Parameter(a), new Parameter(b)]),
                                             new TypeRef("boolean"),
                                             new Block([
                                                 new Variable(orderedA, true, new Call(name, AstUtility.CreateArgumentList([a]))),
                                                 new Variable(orderedB, true, new Call(name, AstUtility.CreateArgumentList([b]))),
                                                 ..orderClauses,
                                                 new Return(AstUtility.False)
                                             ])));

        return null;
    }

    public override Node? VisitQueryContinuation(QueryContinuationSyntax node)
    {
        var query = FindFirstAncestor<QueryExpressionSyntax>(node);

        return query == null
            ? null
            : HandleQuery(null, node.Identifier, node.Body);
    }

    public override Statement? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        var classDeclaration = FindFirstAncestor<ClassDeclarationSyntax>(node);
        if (!IsStatic(node) || classDeclaration == null) return null;

        // static props
        var initializer = GetFieldOrPropertyInitializer(classDeclaration, node.Type, node.Initializer);
        if (initializer == null) return null;

        return new Assignment(new MemberAccess(AstUtility.CreateSimpleName(classDeclaration, noGenerics: true),
                                               AstUtility.CreateSimpleName(node)),
                              initializer);
    }

    public override Statement? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        var classDeclaration = FindFirstAncestor<ClassDeclarationSyntax>(node);
        if (!IsStatic(node) || HasSyntax(node.Modifiers, SyntaxKind.ConstKeyword) || classDeclaration == null) return null;

        // static fields
        List<Statement> statements = [];
        foreach (var declarator in node.Declaration.Variables)
        {
            var initializer = GetFieldOrPropertyInitializer(classDeclaration, node.Declaration.Type, declarator.Initializer);
            if (initializer == null) continue;

            statements.Add(new Assignment(new MemberAccess(AstUtility.CreateSimpleName(classDeclaration, noGenerics: true),
                                                           AstUtility.CreateSimpleName(declarator)),
                                          initializer));
        }

        return new Block(statements);
    }

    public override Function VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var classDeclaration = FindFirstAncestor<ClassDeclarationSyntax>(node)!;
        var parameterList = Visit<ParameterList>(node.ParameterList);
        var body = node.ExpressionBody != null
            ? Visit<Block>(node.ExpressionBody)
            : Visit<Block?>(node.Body);

        var attributeLists = node.AttributeLists.Select(Visit<AttributeList>).ToList();
        return GenerateConstructor(classDeclaration, parameterList, body, attributeLists);
    }

    public override Function VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var classDeclaration = FindFirstAncestor<ClassDeclarationSyntax>(node)!;
        var symbol = _semanticModel.GetDeclaredSymbol(node)!;
        var name = DefineMethodName(symbol);
        var className = AstUtility.CreateSimpleName(classDeclaration, noGenerics: true);
        var fullName = new QualifiedName(className, name, IsStatic(node) ? '.' : ':');
        var parameterList = Visit<ParameterList>(node.ParameterList);

        var returnType = AstUtility.CreateTypeRef(Visit<Name>(node.ReturnType).ToString())!;
        var body = node.ExpressionBody != null
            ? Visit<Block>(node.ExpressionBody)
            : Visit<Block?>(node.Body);

        TryConvertGeneratorFunction(node.Body, returnType, body);
        var attributeLists = node.AttributeLists.Select(Visit<AttributeList>).ToList();

        return new Function(fullName,
                            false,
                            parameterList,
                            returnType,
                            body,
                            attributeLists);
    }

    public override Block VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
    {
        var (expression, prereqStatements) = _file.Prerequisites.Capture(() => Visit<Expression>(node.Expression));

        return new Block([..prereqStatements, new Return(expression)]);
    }

    public override IdentifierName VisitThisExpression(ThisExpressionSyntax node) => new("self");

    // TODO: support initializers
    public override Call VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
    {
        var sizeExpression = Visit<Expression>(node.Type.RankSpecifiers[0].Sizes[0]);

        return AstUtility.TableCall("create", AstUtility.CreateArgumentList([sizeExpression]));
    }

    public override TableInitializer VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
    {
        var initializers = node.Initializer.Expressions.Select(Visit<Expression>);

        return new TableInitializer(initializers.ToList());
    }

    public override NoOp VisitBaseList(BaseListSyntax node)
    {
        foreach (var baseType in node.Types)
        {
            var type = _semanticModel.GetTypeInfo(baseType.Type).Type;
            var disallowedName = Constants.DISALLOWED_BASE_TYPES.FirstOrDefault(name => StandardUtility.DoesTypeInheritFrom(type, name));
            if (disallowedName == null) continue;

            throw Logger.CodegenError(baseType, $"Types that have macros may not be inherited from ({disallowedName})");
        }

        // do nothing (for now)
        return new NoOp();
    }

    public override TypeAlias VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        var name = new IdentifierName(node.Identifier.Text);

        return new TypeAlias(name, new InterfaceType([]));
    }

    // long as hell lol
    public override Statement? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (node.AttributeLists.Any(list => list.Attributes.Any(attr =>
            {
                var symbol = _semanticModel.GetSymbolInfo(attr).Symbol;

                return symbol is { ContainingSymbol.Name: "AttributeUsageAttribute", ContainingNamespace.Name: "System" };
            })))
        {
            if (node.Members.Count > 0)
                throw Logger.CodegenError(node.Members.First(),
                                          "Attribute classes may not have members because only metadata attributes are supported");

            return null;
        }

        if (node.BaseList != null) Visit(node.BaseList); // TODO: do something with it

        var name = AstUtility.CreateSimpleName(node);
        var nonGenericName = AstUtility.GetNonGenericName(name);
        _file.OccupiedIdentifiers.AddIdentifier(nonGenericName.Text);
        _file.OccupiedIdentifiers.Push();

        var members = node.Members
                          .Select(Visit<Statement?>)
                          .OfType<Statement>()
                          .Where(s => s is not NoOp)
                          .ToList();

        var explicitConstructor = node.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
        var constructor = explicitConstructor == null
            ? GenerateConstructor(node, new ParameterList([]))
            : Visit<Function>(explicitConstructor);

        // TODO: maybe move this to AstUtility, this shit is huge
        var typeRef = AstUtility.CreateTypeRef(name.ToString())!;
        var nameStringLiteral = AstUtility.String(nonGenericName.ToString());
        List<Statement> classMemberStatements =
        [
            new Assignment(nonGenericName,
                           new Call(new IdentifierName("setmetatable"),
                                    AstUtility.CreateArgumentList([
                                        TableInitializer.Empty,
                                        new TableInitializer([
                                                                 new
                                                                     AnonymousFunction(new
                                                                                           ParameterList([]),
                                                                                       new
                                                                                           TypeRef("string"),
                                                                                       new Block([
                                                                                           new
                                                                                               Return(nameStringLiteral)
                                                                                       ]))
                                                             ],
                                                             [new IdentifierName("__tostring")])
                                    ]))),
            new Assignment(new MemberAccess(nonGenericName,
                                            new IdentifierName("__index")),
                           nonGenericName),
            new Assignment(new MemberAccess(nonGenericName,
                                            new IdentifierName("__className")),
                           nameStringLiteral),
            new Function(new QualifiedName(nonGenericName, new IdentifierName("new")),
                         false,
                         constructor.ParameterList,
                         typeRef,
                         new Block([
                             new Variable(new IdentifierName("self"),
                                          true,
                                          new TypeCast(new Parenthesized(new TypeCast(new
                                                                                          Call(new
                                                                                                   IdentifierName("setmetatable"),
                                                                                               AstUtility
                                                                                                   .CreateArgumentList([
                                                                                                       TableInitializer
                                                                                                           .Empty,
                                                                                                       nonGenericName
                                                                                                   ])),
                                                                                      AstUtility.AnyType)),
                                                       typeRef)),
                             new Return(new BinaryOperator(new Call(new MemberAccess(new IdentifierName("self"),
                                                                                     nonGenericName,
                                                                                     ':'),
                                                                    AstUtility.CreateArgumentList(constructor
                                                                                                  .ParameterList
                                                                                                  .Parameters
                                                                                                  .ConvertAll<
                                                                                                      Expression>(parameter => parameter
                                                                                                                      .Name))),
                                                           "or",
                                                           new IdentifierName("self")))
                         ]),
                         null,
                         name is GenericName genericName
                             ? genericName.TypeArguments.Select(a => new IdentifierName(a)).ToList()
                             : null)
        ];

        if (explicitConstructor == null) classMemberStatements.Add(constructor);

        classMemberStatements.AddRange(members);
        List<Statement> statements =
        [
            new Variable(nonGenericName, true),
            new ScopedBlock(classMemberStatements),
            AstUtility.DefineGlobalOrMember(node, nonGenericName),
            new TypeAlias(name, new TypeOfCall(nonGenericName))
        ];

        if (node.Parent is CompilationUnitSyntax) statements.Add(new NoOp()); // for the newline

        _file.OccupiedIdentifiers.Pop();
        return new Block(statements);
    }

    public override NoOp VisitEnumDeclaration(EnumDeclarationSyntax node) => new(false);

    public override Block VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        var name = _file.OccupiedIdentifiers.AddIdentifier(node.Name, node.Name.ToString().Split('.').Last());
        var members = new Block(node.Members.Select(Visit<Statement>).ToList());
        List<Statement> statements =
        [
            new Variable(name, true, TableInitializer.Empty), AstUtility.DefineGlobalOrMember(node, name), new TypeAlias(name, new TypeOfCall(name))
        ];

        if (members.Statements.Count > 0) statements.Insert(1, new ScopedBlock(members.Statements));

        if (node.Parent is CompilationUnitSyntax) statements.Add(new NoOp()); // for the newline

        return new Block(statements);
    }

    public override Repeat VisitDoStatement(DoStatementSyntax node)
    {
        var condition = Visit<Expression>(node.Condition);
        var body = Visit<Statement>(node.Statement);

        return new Repeat(new UnaryOperator("not ", condition), body);
    }

    public override While VisitWhileStatement(WhileStatementSyntax node)
    {
        var condition = Visit<Expression>(node.Condition);
        var body = Visit<Statement>(node.Statement);

        return new While(condition, body);
    }

    public override IfExpression VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        var condition = Visit<Expression>(node.Condition);
        var body = Visit<Expression>(node.WhenTrue);
        var elseBranch = Visit<Expression>(node.WhenFalse);

        return new IfExpression(condition, body, elseBranch);
    }

    public override Expression VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        var comparand = Visit<Expression>(node.Expression);
        var (whenNotNull, prereqs) = _file.Prerequisites.Capture(() => Visit<Expression>(node.WhenNotNull));
        var name = node.WhenNotNull.DescendantNodes().LastOrDefault(d => d is NameSyntax);
        var comparandTempNameText = name != null ? "_" + name : "_exp";
        var isWhenNotNullBranch = node.Ancestors().Any(a => a.IsKind(SyntaxKind.ConditionalAccessExpression));
        var comparandTempName = isWhenNotNullBranch
            ? new IdentifierName(_file.OccupiedIdentifiers.GetDuplicateText(comparandTempNameText))
            : PushToVariable(comparandTempNameText, comparand);

        var condition = new BinaryOperator(comparandTempName, "~=", AstUtility.Nil);
        List<Statement> ifBody = [new Assignment(comparandTempName, whenNotNull), ..prereqs];
        _file.Prerequisites.Add(new If(condition, new Block(ifBody)));

        return isWhenNotNullBranch ? comparand : comparandTempName;
    }

    public override If VisitIfStatement(IfStatementSyntax node)
    {
        var condition = Visit<Expression>(node.Condition);
        var body = Visit<Block>(node.Statement);
        var elseBranch = Visit<Block?>(node.Else?.Statement);

        return new If(condition, body, elseBranch);
    }

    public override Expression VisitIsPatternExpression(IsPatternExpressionSyntax node)
    {
        var expression = Visit<Expression>(node.Expression);

        return HandlePattern(node.Pattern, expression, node.Expression);
    }

    public override Statement VisitForStatement(ForStatementSyntax node)
    {
        var (initializer, initializerPrereqs) = _file.Prerequisites.Capture(() => Visit<VariableList?>(node.Declaration)?.Variables.FirstOrDefault());

        var condition = Visit<Expression?>(node.Condition) ?? AstUtility.True;
        var isNumericLoop = initializer is { Initializer: Literal literal } && int.TryParse(literal.ValueText, out _);
        var incrementByExpression = Visit<Expression?>(node.Incrementors.FirstOrDefault());
        var body = Visit<Statement>(node.Statement);

        if (isNumericLoop
         && node.Condition is BinaryExpressionSyntax { OperatorToken.Text: "<=" or "<" } binaryOp
         && incrementByExpression is BinaryOperator { Operator: "+=" or "-=" } incrementBinaryOp
         && initializerPrereqs.Count == 0)
        {
            var minimum = initializer!.Initializer!;
            var maximum = ((BinaryOperator)condition).Right;
            if (binaryOp.OperatorToken.Text == "<") maximum = AstUtility.SubtractOne(maximum);

            return new NumericFor(initializer.Name,
                                  minimum,
                                  maximum,
                                  incrementBinaryOp.Operator == "-=" ? new Literal("-1") : null,
                                  body);
        }

        Statement? incrementBy = incrementByExpression != null ? new ExpressionStatement(incrementByExpression) : null;
        var statements = initializerPrereqs;
        if (initializer != null) statements.Add(initializer);

        List<Statement> whileStatements = [];
        if (incrementBy != null)
        {
            var shouldIncrementIdentifier = _file.OccupiedIdentifiers.AddIdentifier("_shouldIncrement");
            statements.Add(new Variable(shouldIncrementIdentifier, true, AstUtility.False));
            if (incrementBy is ExpressionStatement { Expression: BinaryOperator binaryOperator } expressionStatement
             && !binaryOperator.Operator.Contains('='))
                incrementBy = new Variable(AstUtility.DiscardName, true, expressionStatement.Expression);

            whileStatements.Add(new If(shouldIncrementIdentifier,
                                       new Block([incrementBy]),
                                       new Block([new Assignment(shouldIncrementIdentifier, AstUtility.True)])));
        }

        whileStatements.Add(new If(new UnaryOperator("not ", new Parenthesized(condition)), new Block([new Break()])));
        whileStatements.Add(body);
        statements.Add(new While(AstUtility.True, new Block(whileStatements)));

        return new ScopedBlock(statements);
    }

    public override For VisitForEachStatement(ForEachStatementSyntax node)
    {
        var iterableSymbol = _semanticModel.GetTypeInfo(node.Expression).Type;
        var isList = StandardUtility.DoesTypeInheritFrom(iterableSymbol, "Array")
                  || StandardUtility.DoesTypeInheritFrom(iterableSymbol, "IEnumerable");

        _file.OccupiedIdentifiers.Push();
        List<IdentifierName> names = [_file.OccupiedIdentifiers.AddIdentifier(node.Identifier)];
        if (isList) names = names.Prepend(AstUtility.DiscardName).ToList();

        var iterable = Visit<Expression>(node.Expression);
        var body = Visit<Statement>(node.Statement);
        _file.OccupiedIdentifiers.Pop();

        return new For(names, iterable, body);
    }

    public override For VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
    {
        var iterableSymbol = _semanticModel.GetTypeInfo(node.Expression).Type;
        var isList = StandardUtility.DoesTypeInheritFrom(iterableSymbol, "Array")
                  || StandardUtility.DoesTypeInheritFrom(iterableSymbol, "IEnumerable");

        if (isList && iterableSymbol is INamedTypeSymbol { IsGenericType: true, TypeArguments: { Length: > 0 } typeArguments })
            iterableSymbol = typeArguments.First();

        _file.OccupiedIdentifiers.Push();
        var variableNode = Visit<Statement>(node.Variable);
        var names = variableNode switch
        {
            Variable variable => [variable.Name],
            MultipleVariable multipleVariable when iterableSymbol is { ContainingNamespace.Name: "Roblox", Name: "LuaTuple" } =>
                multipleVariable.Names.ToList(),
            _ => []
        };

        var iterator = Visit<Expression>(node.Expression);
        var body = Visit<Statement>(node.Statement);
        _file.OccupiedIdentifiers.Pop();

        return new For(names, iterator, body);
    }

    public override Node? VisitDeclarationExpression(DeclarationExpressionSyntax node) => Visit(node.Designation);

    public override Variable VisitSingleVariableDesignation(SingleVariableDesignationSyntax node) =>
        new(_file.OccupiedIdentifiers.AddIdentifier(node.Identifier), true);

    public override Variable VisitDiscardDesignation(DiscardDesignationSyntax node) => new(AstUtility.DiscardName, true);

    public override MultipleVariable VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntax node)
    {
        HashSet<IdentifierName> names = [];
        List<Expression> initializers = [];
        var variables = node.Variables
                            .Select(Visit)
                            .OfType<Node>()
                            .SelectMany(variable =>
                            {
                                if (variable is VariableList variableList) return variableList.Variables;

                                return [(Variable)variable];
                            })
                            .ToList();

        foreach (var variable in variables)
        {
            names.Add(variable.Name);
            initializers.Add(variable.Initializer ?? AstUtility.Nil);
        }

        return new MultipleVariable(names, true, initializers);
    }

    public override Parenthesized VisitTypeOfExpression(TypeOfExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type;

        if (typeSymbol == null) throw Logger.CodegenError(node, "Unable to resolve type symbol of the type provided to typeof()");

        var type = StandardUtility.GetRuntimeType(_semanticModel, node, typeSymbol);

        return new Parenthesized(AstUtility.CreateTypeInfo(type));
    }

    public override TypeCast VisitCastExpression(CastExpressionSyntax node)
    {
        var expression = Visit<Expression>(node.Expression);
        var typeName = Visit<Name>(node.Type);
        if (expression is TypeCast)
            expression = new Parenthesized(expression);

        return new TypeCast(expression, AstUtility.CreateTypeRef(typeName.ToString())!);
    }

    public override Expression VisitExpressionElement(ExpressionElementSyntax node) => Visit<Expression>(node.Expression);

    public override TableInitializer VisitCollectionExpression(CollectionExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node).ConvertedType;
        var elements = node.Elements.Select(Visit<Expression>).ToList();

        if (!StandardUtility.DoesTypeInheritFrom(typeSymbol, "HashSet"))
            return new TableInitializer(elements);

        var initializers = elements.ConvertAll(_ => AstUtility.Bool(true)).ToList<Expression>();
        return new TableInitializer(initializers, elements);
    }

    public override Expression VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
    {
        var baseSymbol = _semanticModel.GetSymbolInfo(node).Symbol;
        var classSymbol = baseSymbol?.ContainingSymbol ?? baseSymbol;

        if (classSymbol == null) throw Logger.CodegenError(node, "Unable to resolve class symbol for implicit object creation");

        var name = AstUtility.TypeNameFromSymbol(classSymbol);
        var nonGenericName = AstUtility.GetNonGenericName(name);
        var argumentList = Visit<ArgumentList>(node.ArgumentList);

        var callee = new QualifiedName(nonGenericName, new IdentifierName("new"));
        var expandedExpression = _macro.ObjectCreation(Visit, node);

        var creationExpression = expandedExpression ?? new Call(callee, argumentList);

        return HandleObjectCreationInitializer(node.Initializer, expandedExpression, creationExpression) ?? creationExpression;
    }

    public override Expression VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        var name = Visit<Name>(node.Type);
        var nonGenericName = AstUtility.GetNonGenericName(name);
        var argumentList = Visit<ArgumentList>(node.ArgumentList);

        var callee = new MemberAccess(nonGenericName, new IdentifierName("new"));
        var expandedExpression = _macro.ObjectCreation(Visit, node);

        var creationExpression = expandedExpression ?? new Call(callee, argumentList);

        return HandleObjectCreationInitializer(node.Initializer, expandedExpression, creationExpression) ?? creationExpression;
    }

    public override Node VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var methodSymbolInfo = _semanticModel.GetSymbolInfo(node.Expression);
        var methodSymbol = methodSymbolInfo.Symbol;
        var methodType = _semanticModel.GetTypeInfo(node.Expression).Type;
        if (methodSymbol == null
         && methodSymbolInfo.CandidateSymbols.IsEmpty
         && methodSymbolInfo.CandidateReason == CandidateReason.None
         && node.Expression is IdentifierNameSyntax identifier
         && identifier.Identifier.IsKind(SyntaxKind.IdentifierToken))
            switch (identifier.Identifier.Text)
            {
                case "nameof":
                    return AstUtility.String(node.ArgumentList.Arguments.First().Expression.ToString());
            }

        var callee = Visit<Expression>(node.Expression);
        if (methodSymbol != null)
        {
            var @operator = methodSymbol.IsStatic ? '.' : ':';
            callee = callee switch
            {
                MemberAccess memberAccess => memberAccess.WithOperator(@operator),
                QualifiedName qualifiedName => qualifiedName.WithOperator(@operator),
                _ => callee
            };
        }

        // TODO: PLEASE move this to AstUtility
        List<Statement> refStatements = [];
        var arguments = node.ArgumentList.Arguments.Select(arg =>
                            {
                                if (!arg.RefKindKeyword.IsKind(SyntaxKind.RefKeyword)
                                 && !arg.RefKindKeyword.IsKind(SyntaxKind.OutKeyword))
                                    return Visit<Argument>(arg);

                                Variable? variable = null;
                                if (arg.Expression.IsKind(SyntaxKind.DeclarationExpression))
                                {
                                    variable = Visit<Variable>((arg.Expression as DeclarationExpressionSyntax)!.Designation as SingleVariableDesignationSyntax);
                                    refStatements.Add(variable);
                                }

                                return new Argument(new AnonymousFunction(new ParameterList([new Parameter(AstUtility.Vararg)]),
                                                                          body: new Block([
                                                                              new Variable(new IdentifierName("_val"),
                                                                                           true,
                                                                                           AstUtility.Vararg),
                                                                              new If(new BinaryOperator(new Call(new IdentifierName("select"),
                                                                                                                 AstUtility.CreateArgumentList([
                                                                                                                     AstUtility.String("#"),
                                                                                                                     AstUtility.Vararg
                                                                                                                 ])),
                                                                                                        "~=",
                                                                                                        new Literal("0")),
                                                                                     new Block([
                                                                                         new Assignment(variable?.Name
                                                                                                     ?? Visit<IdentifierName>(arg
                                                                                                                                  .Expression),
                                                                                                        new IdentifierName("_val"))
                                                                                     ])),
                                                                              new Return(variable?.Name
                                                                                      ?? Visit<IdentifierName>(arg.Expression))
                                                                          ])));
                            })
                            .ToList();

        var argumentList = new ArgumentList(arguments);
        List<MacroKind> returnCalleeMacroKinds =
        [
            MacroKind.NewInstance, MacroKind.EnumerableMethod, MacroKind.ListMethod, MacroKind.HashSetMethod, MacroKind.DictionaryMethod, MacroKind.ObjectMethod
        ];

        // dumb ass hack bc null warning suppression doesn't work here for some reason
        if (callee.ExpandedByMacro != null && returnCalleeMacroKinds.Contains((MacroKind)callee.ExpandedByMacro))
        {
            var discard = methodSymbol is IMethodSymbol { ReturnType.Name: not "Void" }
                       && callee is not NoOpExpression
                       && node.Parent is ExpressionStatementSyntax;

            return discard
                ? AstUtility.DiscardVariable(node, callee)
                : callee;
        }

        if (refStatements.Count <= 0)
            return new Call(callee, argumentList);

        refStatements.Add(new ExpressionStatement(new Call(callee, argumentList)));
        return new Block(refStatements);
    }

    public override ArgumentList VisitArgumentList(ArgumentListSyntax node)
    {
        var arguments = node.Arguments.Select(Visit<Argument>).ToList();

        return new ArgumentList(arguments);
    }

    public override Argument VisitArgument(ArgumentSyntax node)
    {
        var expression = Visit<Expression>(node.Expression);

        return new Argument(expression);
    }

    public override Variable VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
    {
        var name = Visit<IdentifierName?>(node.NameEquals?.Name);
        if (name == null)
        {
            var expressionName = _semanticModel.GetSymbolInfo(node.Expression).Symbol!.Name;
            if (expressionName.Contains('.')) expressionName = expressionName.Split('.').Last();

            name = AstUtility.CreateSimpleName<IdentifierName>(node, expressionName);
        }

        var value = Visit<Expression?>(node.Expression);

        return new Variable(name, true, value);
    }

    public override Node VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        var expanded = _macro.Assignment(Visit, node);
        if (expanded != null) return expanded;

        var initializerType = _semanticModel.GetTypeInfo(node.Right).Type;
        var value = Visit<Expression>(node.Right);
        bool isCSharpTuple;

        if (node.Left is DeclarationExpressionSyntax declarationExpression
         && ((isCSharpTuple = initializerType is { ContainingNamespace.Name: "System", Name: "ValueTuple" })
          || initializerType is { ContainingNamespace.Name: "Roblox", Name: "LuaTuple" }))
        {
            var designation = Visit<BaseVariable>(declarationExpression.Designation);
            var names = designation switch
            {
                Variable variable => [variable.Name],
                MultipleVariable multipleVariable => multipleVariable.Names,
                _ => []
            };

            var finalValue = isCSharpTuple ? AstUtility.CSCall("unpackTuple", value) : value;
            return new MultipleVariable(names, designation.IsLocal, [finalValue], designation.Type);
        }

        var mappedOperator = StandardUtility.GetMappedOperator(node.OperatorToken.Text);
        var name = Visit<AssignmentTarget>(node.Left);
        var method = GetParameterList(node);
        Expression returningName = name;
        Statement returning = new ExpressionStatement(new BinaryOperator(name, mappedOperator, value));

        if (node.IsKind(SyntaxKind.SimpleAssignmentExpression)) returning = new Assignment(name, value);

        if (method != null)
        {
            var refKinds = GetRefKindParameters(method);
            if (refKinds.Contains(name.ToString()))
            {
                returning = new ExpressionStatement(new Call(name, AstUtility.CreateArgumentList([value])));
                returningName = new Call(name);
            }

            if (refKinds.Contains(value.ToString())) returning = new Assignment(name, new Call(value));
        }

        var isAlone = node.Parent is ExpressionStatementSyntax;
        if (!isAlone) _file.Prerequisites.Add(returning);

        return isAlone ? returning : returningName;
    }

    public override TableInitializer VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
    {
        List<Expression> values = [];
        List<Expression> keys = [];
        foreach (var member in node.Initializers)
        {
            var declaration = Visit<Variable>(member)!;
            var key = AstUtility.String(declaration.Name.ToString());
            var value = declaration.Initializer!;
            keys.Add(key);
            values.Add(value);
        }

        return new TableInitializer(values, keys);
    }

    public override Node VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var expression = Visit<Expression>(node.Expression);
        var originalName = Visit<Name>(node.Name);
        if (originalName is not SimpleName simpleName)
            throw Logger.CompilerError($"Member access name is not a simple name, instead it is '{originalName.GetType().Name}'", node.Name);

        var name = AstUtility.GetNonGenericName(simpleName);
        var memberAccess = new MemberAccess(expression, name);
        var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
        switch (symbol)
        {
            case IFieldSymbol { HasConstantValue: true } fieldSymbol:
                return AstUtility.CreateLuauConstant(fieldSymbol.ConstantValue);
            case IMethodSymbol methodSymbol when node.Parent is ArgumentSyntax or AssignmentExpressionSyntax { OperatorToken.Text: "+=" or "-=" }:
            {
                if (node.Parent is AssignmentExpressionSyntax assignment
                 && _semanticModel.GetSymbolInfo(assignment.Left).Symbol is not IEventSymbol)
                    break;

                return AstUtility.WrapNonStaticMethod(methodSymbol, memberAccess, _file.OccupiedIdentifiers);
            }
        }

        return _macro.MemberAccess(Visit, node) ?? memberAccess;
    }

    public override Node VisitImplicitElementAccess(ImplicitElementAccessSyntax node) => Visit<Expression>(node.ArgumentList.Arguments.First().Expression);

    public override Node VisitElementAccessExpression(ElementAccessExpressionSyntax node)
    {
        var expressionTypeSymbol = _semanticModel.GetTypeInfo(node.Expression).Type;
        var indexExpression = node.ArgumentList.Arguments.First().Expression;
        var indexTypeSymbol = _semanticModel.GetTypeInfo(indexExpression).Type;
        var expression = Visit<Expression>(node.Expression);
        var index = Visit<Expression>(indexExpression);
        index = indexTypeSymbol != null
             && expressionTypeSymbol != null
             && expressionTypeSymbol.Name != "Dictionary"
             && Shared.Constants.INTEGER_TYPES.Contains(indexTypeSymbol.Name)
            ? AstUtility.AddOne(index)
            : index;

        var elementAccess = new ElementAccess(expression, index);

        return AstUtility.DiscardVariableIfExpressionStatement(node, elementAccess, node.Parent);
    }

    public override QualifiedName VisitQualifiedName(QualifiedNameSyntax node)
    {
        var left = Visit<Name>(node.Left);
        var right = Visit<IdentifierName>(node.Right);

        return new QualifiedName(left, right);
    }

    public override Node VisitIdentifierName(IdentifierNameSyntax node)
    {
        var classDeclaration = FindFirstAncestor<ClassDeclarationSyntax>(node);
        var method = FindFirstAncestor<MethodDeclarationSyntax>(node);
        var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
        if (symbol is ILocalSymbol { HasConstantValue: true } localSymbol)
            return AstUtility.CreateLuauConstant(localSymbol.ConstantValue);

        var identifierText = _file.OccupiedIdentifiers.GetDuplicateText(node.Identifier.Text);
        if (symbol is IMethodSymbol methodSymbol
         && SymbolMetadataManager.Get(methodSymbol.ContainingType) is { MethodOverloads: not null }
         && GetMethodName(methodSymbol) is { } methodName)
            identifierText = methodName;

        var parent = node.Parent;
        node = node.WithIdentifier(SyntaxFactory.Identifier(identifierText));

        var name = AstUtility.CreateSimpleName(node);
        if (method != null && node.Parent is not AssignmentExpressionSyntax)
        {
            var refKinds = GetRefKindParameters(method.ParameterList);

            if (refKinds.Contains(node.Identifier.Text)) return new Call(name);
        }

        if (classDeclaration == null
         || symbol is not (IFieldSymbol or IPropertySymbol or IEventSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary })
         || symbol.ContainingType.Name != classDeclaration.Identifier.Text
         || IsAlreadyQualified(node, parent))
            return name;

        var qualifier = symbol.IsStatic
            ? AstUtility.CreateSimpleName(node, classDeclaration.Identifier.Text, noGenerics: true)
            : new IdentifierName("self");

        return new QualifiedName(qualifier, name, symbol is IMethodSymbol ? ':' : '.');
    }

    public override Expression VisitGenericName(GenericNameSyntax node)
    {
        var typeArguments = node.TypeArgumentList.Arguments
                                .Select(typeArg => StandardUtility.GetMappedType(Visit<Name>(typeArg).ToString()))
                                .ToList();

        Expression? expandedExpression = _macro.GenericName(node, typeArguments);
        return expandedExpression ?? new GenericName(node.Identifier.Text, typeArguments);
    }

    public override Literal VisitSizeOfExpression(SizeOfExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type;
        if (typeSymbol == null) throw Logger.CompilerError($"Failed to resolve type '{node.Type}' used in sizeof()", node);

        var type = StandardUtility.GetRuntimeType(_semanticModel, node.Type, typeSymbol);
        int size;
        try
        {
            size = Marshal.SizeOf(type);
        }
        catch (Exception e)
        {
            throw Logger.CompilerError($"Failed to resolve size of type '{node.Type}': {e.Message}", node);
        }

        return new Literal(size.ToString());
    }

    public override Break VisitBreakStatement(BreakStatementSyntax node) => new();
    public override Continue VisitContinueStatement(ContinueStatementSyntax node) => new();
    public override Return VisitReturnStatement(ReturnStatementSyntax node) => new(Visit<Expression?>(node.Expression));

    public override Block VisitBlock(BlockSyntax node)
    {
        _file.OccupiedIdentifiers.Push();
        var statements = node.Statements.Select(statement =>
                             {
                                 var (visitedStatement, prereqStatements) = _file.Prerequisites.Capture(() => Visit<Statement>(statement));

                                 if (prereqStatements.Count <= 0) return visitedStatement;

                                 var newStatements = prereqStatements.ToList();
                                 newStatements.Add(visitedStatement);

                                 return new Block(newStatements);
                             })
                             .ToList();

        _file.OccupiedIdentifiers.Pop();

        return node.Parent is BlockSyntax or GlobalStatementSyntax or null
            ? new ScopedBlock(statements)
            : new Block(statements);
    }

    public override Node VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        var expanded = MacroManager.BinaryExpression(Visit, node);

        if (expanded != null) return expanded;

        var leftType = _semanticModel.GetTypeInfo(node.Left).Type;
        var rightType = _semanticModel.GetTypeInfo(node.Right).Type;
        var mappedOperator = StandardUtility.GetMappedOperator(node.OperatorToken.Text);
        if ((leftType is { Name: "String" or "Char" } || rightType is { Name: "String" or "Char" }) && mappedOperator is "+=" or "+") mappedOperator = "..";

        var left = Visit<Expression>(node.Left);
        var right = Visit<Expression>(node.Right);

        return new BinaryOperator(left, mappedOperator, right);
    }

    public override Node VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
    {
        var operand = Visit<Expression>(node.Operand);
        var operandType = _semanticModel.GetTypeInfo(node.Operand).Type;
        if (node.OperatorToken.Text == "!")
        {
            var nonOptionalType = AstUtility.CreateTypeRef(operandType != null ? operandType.Name.Replace("?", "") : "any")!;

            return new TypeCast(operand, nonOptionalType);
        }

        var originalIdentifier = _file.OccupiedIdentifiers.AddIdentifier("_original");
        var mappedOperator = StandardUtility.GetMappedOperator(node.OperatorToken.Text);
        var increment = new BinaryOperator(operand, mappedOperator, new Literal("1"));
        var isAlone = node.Parent is ExpressionStatementSyntax or ForStatementSyntax;
        if (!isAlone) _file.Prerequisites.AddList([new Variable(originalIdentifier, true, operand), new ExpressionStatement(increment)]);

        return isAlone ? increment : originalIdentifier;
    }

    public override Node? VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
    {
        var operatorText = node.OperatorToken.Text;
        if (operatorText == "^")
        {
            Logger.UnsupportedError(node, "'^' unary operator", true);

            return null;
        }

        var operand = Visit<Expression>(node.Operand);

        if (operatorText == "+") return operand;

        // TODO: prefix increment/decrement
        var mappedOperator = StandardUtility.GetMappedOperator(operatorText);
        var bit32MethodName = StandardUtility.GetBit32MethodName(mappedOperator);

        if (bit32MethodName != null) return AstUtility.Bit32Call(bit32MethodName, operand);

        return new UnaryOperator(mappedOperator, operand);
    }

    public override Expression VisitSwitchExpression(SwitchExpressionSyntax node)
    {
        var createTempVariable = node.GoverningExpression is not IdentifierNameSyntax
                              && node.GoverningExpression is not LiteralExpressionSyntax;

        var condition = Visit<Expression>(node.GoverningExpression);
        var newValueIdentifier = new IdentifierName("_newValue");
        var comparand = createTempVariable
            ? _file.OccupiedIdentifiers.AddIdentifier("_exp")
            : condition;

        List<Statement> statements = [];
        List<Statement> prereqStatements = [new Variable(newValueIdentifier, true)];

        SwitchExpressionArmSyntax? discardPattern = null;
        foreach (var section in node.Arms)
        {
            if (section.Pattern is DiscardPatternSyntax)
            {
                discardPattern = section;

                continue;
            }

            var binaryOp = HandlePattern(section.Pattern, comparand, node.GoverningExpression);
            statements.Add(new If(binaryOp,
                                  new Block([new Assignment(newValueIdentifier, Visit<Expression>(section.Expression)), new Break()])));
        }

        if (createTempVariable) statements.Insert(0, new Variable((IdentifierName)comparand, true, condition));

        if (discardPattern != null) statements.Add(new Assignment(newValueIdentifier, Visit<Expression>(discardPattern.Expression)));

        prereqStatements.Add(new Repeat(AstUtility.True, new Block(statements)));
        _file.Prerequisites.AddList(prereqStatements);

        return newValueIdentifier;
    }

    // TODO: create VisitCaseSwitchLabel, VisitCasePatternSwitchLabel, VisitDefaultSwitchLabel methods
    public override Block VisitSwitchStatement(SwitchStatementSyntax node)
    {
        var ifStatements = new List<Statement>();
        List<Statement>? defaultStatements = null;

        var nodeHasFallThrough = false;
        var createTempVariable = node.Expression is not IdentifierNameSyntax && node.Expression is not LiteralExpressionSyntax;
        var condition = Visit<Expression>(node.Expression);
        var comparand = createTempVariable
            ? _file.OccupiedIdentifiers.AddIdentifier("_exp")
            : condition;

        var anyNodeHasFallThrough = node.Sections.Any(section => section.Labels.Count > 1);
        var fallthroughIdentifier = anyNodeHasFallThrough ? _file.OccupiedIdentifiers.AddIdentifier("_fallthrough") : null;
        foreach (var section in node.Sections)
        {
            var fallThrough = section.Labels.Count > 1;

            foreach (var label in section.Labels)
            {
                var body = section.Labels.Last() == label
                    ? section.Statements.Select(Visit<Statement>).ToList()
                    : [];

                var hasFallThrough = fallThrough && label != section.Labels.Last();
                if (hasFallThrough)
                {
                    nodeHasFallThrough = true;
                    body.Insert(0, new Assignment(fallthroughIdentifier!, AstUtility.True));
                }

                switch (label)
                {
                    case CasePatternSwitchLabelSyntax patternLabel:
                    {
                        var binaryOp = HandlePattern(patternLabel.Pattern, comparand, node.Expression);
                        if (nodeHasFallThrough) binaryOp = new BinaryOperator(fallthroughIdentifier!, "or", binaryOp);

                        ifStatements.Add(new If(binaryOp, new Block(body)));

                        break;
                    }
                    case CaseSwitchLabelSyntax caseLabel:
                    {
                        var binaryOp = HandleCaseSwitchLabel(caseLabel, comparand);
                        if (nodeHasFallThrough) binaryOp = new BinaryOperator(fallthroughIdentifier!, "or", binaryOp);

                        ifStatements.Add(new If(binaryOp, new Block(body)));

                        break;
                    }

                    case DefaultSwitchLabelSyntax:
                        defaultStatements = body;

                        break;
                }
            }
        }

        if (nodeHasFallThrough) ifStatements.Insert(0, new Variable(fallthroughIdentifier!, true, AstUtility.False));

        if (defaultStatements != null) ifStatements.Add(new ScopedBlock(defaultStatements));

        List<Statement> blockStatements = [new Repeat(AstUtility.True, new Block(ifStatements))];

        if (createTempVariable) blockStatements = blockStatements.Prepend(new Variable((IdentifierName)comparand, true, condition)).ToList();

        return new Block(blockStatements);
    }

    public override TypeAlias VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        var parameterTypes = new List<ParameterType>();

        foreach (var parameter in node.ParameterList.Parameters)
        {
            if (parameter.Type == null) continue;

            var pType = new ParameterType(parameter.Identifier.Text,
                                          new TypeRef(parameter.Type.ToString()));

            parameterTypes.Add(pType);
        }

        return new TypeAlias(new IdentifierName(node.Identifier.Text),
                             new FunctionType(parameterTypes, new TypeRef(node.ReturnType.ToString())));
    }

    public override Block? VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
        var classDeclaration = FindFirstAncestor<ClassDeclarationSyntax>(node);
        if (!IsStatic(node) || classDeclaration == null) return null;

        var statements = node.Declaration.Variables
                             .Select(variable => new Assignment(new MemberAccess(AstUtility.CreateSimpleName(classDeclaration, noGenerics: true),
                                                                                 AstUtility.CreateSimpleName(variable, noGenerics: true)),
                                                                AstUtility.NewSignal()))
                             .ToList<Statement>();

        return new Block(statements);
    }

    public override Parenthesized VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
    {
        var expression = Visit<Expression>(node.Expression);

        return new Parenthesized(expression);
    }

    public override AnonymousFunction VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Body).Type;
        var returnTypeName = typeSymbol != null
            ? AstUtility.TypeNameFromSymbol(typeSymbol)
            : null;

        var returnType = new TypeRef(returnTypeName?.ToString() ?? "()");
        var parameterList = Visit<ParameterList?>(node.ParameterList) ?? new ParameterList([]);
        Block? body;
        if (node.ExpressionBody != null)
        {
            var (expression, prereqStatements) = _file.Prerequisites.Capture(() => Visit<Expression>(node.ExpressionBody));

            body = new Block([..prereqStatements, new Return(expression)]);
        }
        else
        {
            body = Visit<Block?>(node.Block);
        }

        TryConvertGeneratorFunction(node.Block, returnType, body);

        return new AnonymousFunction(parameterList, returnType, body);
    }

    public override AnonymousFunction VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Body).Type;
        var returnTypeName = typeSymbol != null
            ? AstUtility.TypeNameFromSymbol(typeSymbol)
            : null;

        var returnType = new TypeRef(returnTypeName?.ToString() ?? "()");
        var parameterList = new ParameterList([Visit<Parameter>(node.Parameter)]);
        Block? body;
        if (node.ExpressionBody != null)
        {
            var (expression, prereqStatements) = _file.Prerequisites.Capture(() => Visit<Expression>(node.ExpressionBody));

            body = new Block([..prereqStatements, new Return(expression)]);
        }
        else
        {
            body = Visit<Block?>(node.Block);
        }

        TryConvertGeneratorFunction(node.Block, returnType, body);

        return new AnonymousFunction(parameterList, returnType, body);
    }

    public override AnonymousFunction VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Block).Type;
        var returnTypeName = typeSymbol != null
            ? AstUtility.TypeNameFromSymbol(typeSymbol)
            : null;

        var returnType = new TypeRef(returnTypeName?.ToString() ?? "()");
        var parameterList = Visit<ParameterList?>(node.ParameterList) ?? new ParameterList([]);
        Block? body;
        if (node.ExpressionBody != null)
        {
            var (expression, prereqStatements) = _file.Prerequisites.Capture(() => Visit<Expression>(node.ExpressionBody));

            body = new Block([..prereqStatements, new Return(expression)]);
        }
        else
        {
            body = Visit<Block?>(node.Body);
        }

        TryConvertGeneratorFunction(node.Block, returnType, body);
        return new AnonymousFunction(parameterList, returnType, body);
    }

    public override Function VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        var name = _file.OccupiedIdentifiers.AddIdentifier(node.Identifier);
        var parameterList = Visit<ParameterList?>(node.ParameterList) ?? new ParameterList([]);
        var typeParameters = node.TypeParameterList?.Parameters.Select(p => new IdentifierName(p.Identifier.Text)).ToList();
        var returnType = AstUtility.CreateTypeRef(node.ReturnType)!;
        var body = node.ExpressionBody != null
            ? Visit<Block>(node.ExpressionBody)
            : Visit<Block?>(node.Body);

        TryConvertGeneratorFunction(node.Body, returnType, body);
        var attributeLists = node.AttributeLists.Select(Visit<AttributeList>).ToList();

        return new Function(name,
                            true,
                            parameterList,
                            returnType,
                            body,
                            attributeLists,
                            typeParameters);
    }

    public override Statement VisitYieldStatement(YieldStatementSyntax node)
    {
        if (node.ReturnOrBreakKeyword.IsKind(SyntaxKind.BreakKeyword)) return new ExpressionStatement(new Call(new IdentifierName("_breakIteration")));

        var expression = Visit<Expression>(node.Expression!);
        return new Return(expression);
    }

    public override Parameter VisitParameter(ParameterSyntax node)
    {
        var name = _file.OccupiedIdentifiers.AddIdentifier(node.Identifier);
        TypeRef? type = null;
        if (node.Type != null) type = AstUtility.CreateTypeRef(Visit<Name>(node.Type).ToString());

        var initializer = Visit<Expression?>(node.Default);
        var isParams = HasSyntax(node.Modifiers, SyntaxKind.ParamsKeyword);
        if (type != null && node.Modifiers.Any(SyntaxKind.RefKeyword) || node.Modifiers.Any(SyntaxKind.OutKeyword))
            type = new FunctionType([new ParameterType(null, new OptionalType(type!))], type!);

        if (isParams && type is ArrayType arrayType) type = arrayType.ElementType;
        if (initializer != null && type != null) type = new OptionalType(type);

        return new Parameter(name, isParams, initializer, type);
    }

    public override Node? VisitAttribute(AttributeSyntax node) =>
        GetName(node) switch
        {
            "Native" => new BuiltInAttribute(new IdentifierName("native")),
            _ => null
        };

    public override AttributeList VisitAttributeList(AttributeListSyntax node) => new(node.Attributes.Select(Visit<Statement?>).OfType<Statement>().ToList());

    public override Statement VisitGlobalStatement(GlobalStatementSyntax node) => Visit<Statement>(node.Statement);

    public override ParameterList VisitParameterList(ParameterListSyntax node)
    {
        _file.OccupiedIdentifiers.Push();
        var parameterList = new ParameterList(node.Parameters.Select(Visit).OfType<Parameter>().ToList());
        _file.OccupiedIdentifiers.Pop();

        return parameterList;
    }

    public override Statement VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node) =>
        node.IsConst ? new NoOp(false) : Visit<Statement>(node.Declaration);

    public override VariableList VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        var variables = node.Variables.Select(Visit).OfType<Variable>().ToList();

        return new VariableList(variables);
    }

    public override Variable VisitVariableDeclarator(VariableDeclaratorSyntax node)
    {
        var typeRef = AstUtility.CreateTypeRef(node.Parent switch
        {
            VariableDeclarationSyntax declaration => Visit<Name>(declaration.Type).ToString(),
            ParameterSyntax parameter => Visit<Name?>(parameter.Type)?.ToString(),
            _ => null
        });

        var identifierName = _file.OccupiedIdentifiers.AddIdentifier(node.Identifier);
        var initializer = Visit<Expression?>(node.Initializer);

        return new Variable(identifierName,
                            true,
                            initializer,
                            typeRef);
    }

    public override Node? VisitEqualsValueClause(EqualsValueClauseSyntax node) => Visit(node.Value);

    public override Node VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        var expressionNode = Visit<Node>(node.Expression);
        return expressionNode is Expression expression
            ? new ExpressionStatement(expression)
            : expressionNode;
    }

    public override InterpolatedString VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
    {
        var parts = node.Contents.Select(Visit<Expression>).ToList();

        return new InterpolatedString(parts);
    }

    public override Literal VisitInterpolatedStringText(InterpolatedStringTextSyntax node) => new(node.TextToken.Text);

    public override Interpolation VisitInterpolation(InterpolationSyntax node) => new(Visit<Expression>(node.Expression));

    public override Expression VisitLiteralExpression(LiteralExpressionSyntax node)
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
                {
                    stringContents = Regex.Escape(stringContents);
                }

                else if (fullText.StartsWith("\"\"\"")) // raw strings
                {
                    var lines = stringContents.Split("\r\n").ToList();
                    var newStringContents = new StringBuilder();
                    var index = 0;
                    foreach (var line in lines)
                    {
                        newStringContents.Append(Regex.Escape(line));
                        if (index++ != lines.Count - 1) newStringContents.Append("\\n");
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

        return new Literal(valueText);
    }

    private IdentifierName? HandleObjectCreationInitializer(InitializerExpressionSyntax? initializer,
                                                            Expression? macroExpandedExpression,
                                                            Expression creationExpression)
    {
        if (initializer == null || macroExpandedExpression != null) return null;

        var binding = PushToVariable("_binding", creationExpression);
        foreach (var expression in initializer.Expressions)
            switch (expression)
            {
                case AssignmentExpressionSyntax assignment:
                {
                    var name = Visit<SimpleName>(assignment.Left);
                    var value = Visit<Expression>(assignment.Right);
                    _file.Prerequisites.Add(new Assignment(new QualifiedName(binding, name), value));

                    break;
                }
            }

        return binding;
    }

    private IdentifierName? HandleQuery(ExpressionSyntax? expression, SyntaxToken identifier, QueryBodySyntax body)
    {
        var lastResultIdentifier = new IdentifierName(_file.OccupiedIdentifiers.GetDuplicateText("_result"));
        var resultIdentifier = _file.OccupiedIdentifiers.AddIdentifier("_result");
        var originalIterable = expression == null ? lastResultIdentifier : Visit<Expression>(expression);

        HashSet<LinqQueryClauseInfo> clauseInfos = [];

        void addClauseInfo(SyntaxNode queryClauseSyntax)
        {
            var clauseKind = GetLinqQueryClauseKind(queryClauseSyntax);
            var identifierName = _file.OccupiedIdentifiers.AddIdentifier('_' + clauseKind.ToString().ToLower());
            clauseInfos.Add(new LinqQueryClauseInfo(clauseKind, identifierName));
            _file.Prerequisites.AddList(_file.Prerequisites.CaptureOnlyPrereqs(() => Visit(queryClauseSyntax)));
        }

        foreach (var clause in body.Clauses) addClauseInfo(clause);

        addClauseInfo(body.SelectOrGroup);
        var onlyGroupBy = clauseInfos.Count == 1 && clauseInfos.First().Kind == LinqQueryClauseInfoKind.GroupBy;
        var groupingsIdentifier = _file.OccupiedIdentifiers.AddIdentifier("_groupings");
        var keyMapIdentifier = _file.OccupiedIdentifiers.AddIdentifier("_keyMap");

        List<Statement> prereqStatements =
        [
            new Variable(onlyGroupBy
                             ? groupingsIdentifier
                             : resultIdentifier,
                         true,
                         onlyGroupBy
                             ? TableInitializer.Empty
                             : AstUtility.TableCall("clone", originalIterable))
        ];

        var firstClause = true;
        foreach (var clauseInfo in clauseInfos)
        {
            _file.OccupiedIdentifiers.Push();
            var variableName = _file.OccupiedIdentifiers.AddIdentifier(identifier);
            var indexName = _file.OccupiedIdentifiers.AddIdentifier("i");
            List<Statement> forStatementBody = [];
            Statement? nonForStatementPrereq = null;

            var call = new Call(clauseInfo.Name, AstUtility.CreateArgumentList([variableName]));
            switch (clauseInfo.Kind)
            {
                case LinqQueryClauseInfoKind.Select:
                    forStatementBody.Add(new Assignment(new ElementAccess(resultIdentifier, indexName), call));
                    _file.OccupiedIdentifiers.Pop();

                    break;
                case LinqQueryClauseInfoKind.Where:
                    forStatementBody.Add(new If(call,
                                                new Block([new Continue()])));

                    forStatementBody.Add(new Assignment(new ElementAccess(resultIdentifier, indexName),
                                                        AstUtility.Nil));

                    _file.OccupiedIdentifiers.Pop();

                    break;
                case LinqQueryClauseInfoKind.OrderBy:
                    _file.OccupiedIdentifiers.Pop();
                    var comparatorName = new IdentifierName(_file.OccupiedIdentifiers.GetDuplicateText(clauseInfo.Name.Text + "Comparator"));

                    nonForStatementPrereq = new ExpressionStatement(AstUtility.TableCall("sort", resultIdentifier, comparatorName));

                    break;
                case LinqQueryClauseInfoKind.GroupBy:
                    var byIdentifier = _file.OccupiedIdentifiers.AddIdentifier("by");
                    var byKey = new MemberAccess(byIdentifier, new IdentifierName("key"));
                    var byValue = new MemberAccess(byIdentifier, new IdentifierName("value"));
                    var keyMapAtKey = new ElementAccess(keyMapIdentifier, byKey);

                    forStatementBody.Add(new Variable(byIdentifier, true, call));
                    forStatementBody.Add(new If(new UnaryOperator("not ", keyMapAtKey),
                                                new Block([
                                                    new ExpressionStatement(AstUtility.TableCall("insert",
                                                                                                 groupingsIdentifier,
                                                                                                 new TableInitializer([byKey],
                                                                                                 [
                                                                                                     new
                                                                                                         IdentifierName("Key")
                                                                                                 ]))),
                                                    new Assignment(keyMapAtKey,
                                                                   new UnaryOperator("#", groupingsIdentifier))
                                                ])));

                    forStatementBody.Add(new ExpressionStatement(AstUtility.TableCall("insert",
                                                                                      new ElementAccess(groupingsIdentifier,
                                                                                                        keyMapAtKey),
                                                                                      byValue)));

                    _file.OccupiedIdentifiers.Pop();
                    if (!onlyGroupBy) prereqStatements.Add(new Variable(groupingsIdentifier, true, TableInitializer.Empty));
                    prereqStatements.Add(new Variable(keyMapIdentifier, true, TableInitializer.Empty));

                    break;

                case LinqQueryClauseInfoKind.Continuation:
                default:
                    _file.OccupiedIdentifiers.Pop();

                    break;
            }

            prereqStatements.Add(nonForStatementPrereq
                              ?? new For([indexName, variableName],
                                         firstClause ? originalIterable : resultIdentifier,
                                         new Block(forStatementBody)));

            if (clauseInfo.Kind == LinqQueryClauseInfoKind.GroupBy)
                prereqStatements.Add(onlyGroupBy
                                         ? new Variable(resultIdentifier, true, groupingsIdentifier)
                                         : new Assignment(resultIdentifier, groupingsIdentifier));

            firstClause = false;
        }

        _file.Prerequisites.AddList(prereqStatements);

        if (body.Continuation == null) return null;

        _file.Prerequisites.AddList(_file.Prerequisites.CaptureOnlyPrereqs(() => Visit(body.Continuation)));

        return resultIdentifier;
    }

    private void TryConvertGeneratorFunction(BlockSyntax? block, TypeRef returnType, Block? luauBlock)
    {
        var typeArguments = StandardUtility.ExtractTypeArguments(returnType.Path);
        var mappedTypeArguments = typeArguments.ConvertAll(StandardUtility.GetMappedType);
        var typeArgumentsText = string.Join(", ", mappedTypeArguments);
        var generateEnumerable = returnType.Path.StartsWith("IEnumerable<");
        var generatorFunctionReturn = CreateGeneratorFunctionReturn(block, generateEnumerable);
        if (generatorFunctionReturn == null || luauBlock == null) return;

        luauBlock.Statements.Clear();
        luauBlock.Statements.Add(generatorFunctionReturn);
        returnType.Path = generateEnumerable
            ? $"{{ {typeArgumentsText} }}"
            : $"CS.IEnumerator<{typeArgumentsText}>";
    }

    private Return? CreateGeneratorFunctionReturn(BlockSyntax? block, bool isEnumerable)
    {
        if (block == null) return null;

        var yields = CollectYields(block);
        if (yields.Count == 0) return null;

        var yieldStatements = block.Statements.OfType<YieldStatementSyntax>().ToList();
        var yieldReturns = yieldStatements
                           .TakeWhile(yield => !yield.ReturnOrBreakKeyword.IsKind(SyntaxKind.BreakKeyword))
                           .ToList();

        var isSimple = block.Statements.Count == yieldStatements.Count;
        if (isSimple)
        {
            var yieldedExpressions = yieldReturns.ConvertAll(yield => Visit<Expression>(yield.Expression!));
            var yieldedTable = new TableInitializer(yieldedExpressions);

            return new Return(isEnumerable ? yieldedTable : AstUtility.NewEnumerator(yieldedTable));
        }

        List<List<StatementSyntax>> enumerationBlocks = [];
        List<StatementSyntax> currentBlockStatements = [];
        foreach (var statement in block.Statements)
        {
            currentBlockStatements.Add(statement);

            if (statement is not YieldStatementSyntax) continue;

            enumerationBlocks.Add(currentBlockStatements.ToList());
            currentBlockStatements.Clear();
        }

        var enumerationFunctions = enumerationBlocks
                                   .ConvertAll(statements => new AnonymousFunction(new ParameterList([
                                                                                       new Parameter(new IdentifierName("_breakIteration"),
                                                                                                     false,
                                                                                                     null,
                                                                                                     FunctionType.NoOp)
                                                                                   ]),
                                                                                   null,
                                                                                   new Block(statements.ConvertAll(Visit<Statement>))))
                                   .OfType<Expression>()
                                   .ToList();

        var enumeratorConstruction = AstUtility.NewEnumerator(new AnonymousFunction(new ParameterList([]),
                                                                                    null,
                                                                                    new Block([new Return(new TableInitializer(enumerationFunctions))])));

        Expression returnExpression = isEnumerable
            ? new Call(new MemberAccess(enumeratorConstruction, new IdentifierName("_collect"), ':')) // collect into table
            : enumeratorConstruction;

        return new Return(returnExpression);
    }

    private static List<YieldStatementSyntax> CollectYields(BlockSyntax block) =>
        block.Statements
             .OfType<YieldStatementSyntax>()
             .ToList();

    private IdentifierName DefineMethodName(IMethodSymbol symbol)
    {
        var hasOtherOverloads = symbol.ContainingType
                                      .GetMembers(symbol.Name)
                                      .OfType<IMethodSymbol>()
                                      .Any(s => !SymbolEqualityComparer.Default.Equals(s, symbol));

        if (!hasOtherOverloads) return _file.OccupiedIdentifiers.AddIdentifier(symbol.Name);

        var symbolMetadata = SymbolMetadataManager.Get(symbol.ContainingType);
        symbolMetadata.MethodOverloads ??= [];

        Dictionary<IMethodSymbol, int> methodOverloads = [];
        if (!symbolMetadata.MethodOverloads.TryAdd(symbol.Name, methodOverloads)) methodOverloads = symbolMetadata.MethodOverloads[symbol.Name];

        var lastCount = methodOverloads.Values.Order().LastOrDefault(0);
        var count = lastCount + 1;
        methodOverloads.Add(symbol, count);

        return _file.OccupiedIdentifiers.AddIdentifier($"{symbol.Name}_impl{count}");
    }

    private static string? GetMethodName(IMethodSymbol symbol)
    {
        var hasOtherOverloads = symbol.ContainingType
                                      .GetMembers(symbol.Name)
                                      .OfType<IMethodSymbol>()
                                      .Any(s => !SymbolEqualityComparer.Default.Equals(s, symbol));

        var symbolMetadata = SymbolMetadataManager.Get(symbol.ContainingType);
        if (!hasOtherOverloads || symbolMetadata.MethodOverloads == null) return null;

        var methodOverloads = symbolMetadata.MethodOverloads[symbol.Name];
        var count = methodOverloads[symbol];
        return $"{symbol.Name}_impl{count}";
    }

    // extremely skidded
    private bool TryHoistNode(SyntaxNode root, SyntaxNode nodeToHoist, [NotNullWhen(true)] out SyntaxNode? newRoot)
    {
        newRoot = null;
        var originalNodeToHoist = nodeToHoist;
        if (nodeToHoist is GlobalStatementSyntax globalStatement) nodeToHoist = globalStatement.Statement;

        var shouldHoist = _hoistedSyntaxes.Contains(nodeToHoist.Kind());
        if (!shouldHoist) return false;

        var hoistTarget = GetHoistInsertionTarget(nodeToHoist);
        if (hoistTarget == null) return false; // nothing to do

        var originalParent = originalNodeToHoist.Parent;
        if (originalParent == null) return false;

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

                // stupid hack this code sucks so bad
                // if (newNode is ClassDeclarationSyntax c && declaration.Identifier.Text == c.Identifier.Text) return false;

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

        var scope = GetHoistScope(node);
        return scope?
               .ChildNodes()
               .FirstOrDefault(n => n.SpanStart >= location.SourceSpan.Start);
    }

    private static SyntaxNode? GetHoistScope(SyntaxNode node) =>
        node.Ancestors()
            .FirstOrDefault(n => n is BlockSyntax or CompilationUnitSyntax or NamespaceDeclarationSyntax or ClassDeclarationSyntax);

    private Location? FindHoistTarget(SyntaxNode node)
    {
        var scope = GetHoistScope(node);
        if (scope == null) return null;

        var calledMethods = node
                            .DescendantNodes()
                            .OfType<InvocationExpressionSyntax>()
                            .Select(inv =>
                            {
                                var symbol = _semanticModel.GetSymbolInfo(inv).Symbol;

                                return symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                            })
                            .Where(s => s != null && s != node && scope.Span.Contains(s.Span))
                            .Distinct()
                            .ToList();

        var referencedSymbols = node
                                .DescendantNodes()
                                .OfType<IdentifierNameSyntax>()
                                .Select(id => _semanticModel.GetSymbolInfo(id)
                                                            .Symbol?.DeclaringSyntaxReferences.FirstOrDefault()
                                                            ?.GetSyntax())
                                .Where(s => s != null && s != node && scope.Span.Contains(s.Span))
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

    private static HashSet<string?> GetRefKindParameters(ParameterListSyntax parameterList) =>
        parameterList.Parameters.Select(parameter =>
                     {
                         if (parameter.Modifiers.Any(SyntaxKind.RefKeyword)
                          || parameter.Modifiers.Any(SyntaxKind.OutKeyword))
                             return parameter.Identifier.Text.ToString();

                         return null;
                     })
                     .ToHashSet();

    private static ParameterListSyntax? GetParameterList(SyntaxNode node)
    {
        var ancestors = node.Ancestors();
        SyntaxNode? funcNode = null;
        foreach (var ancestorNode in ancestors)
            if (ancestorNode is MethodDeclarationSyntax or LocalFunctionStatementSyntax or AnonymousMethodExpressionSyntax)
            {
                funcNode = ancestorNode;

                break;
            }

        return funcNode switch
        {
            MethodDeclarationSyntax method => method.ParameterList,
            LocalFunctionStatementSyntax localFunction => localFunction.ParameterList,
            AnonymousMethodExpressionSyntax anonymousFunction => anonymousFunction.ParameterList,
            _ => null
        };
    }

    private Expression HandlePattern(PatternSyntax node, Expression comparand, ExpressionSyntax originalComparand) =>
        node switch
        {
            RelationalPatternSyntax relationalPattern => HandleRelationalPattern(relationalPattern, comparand),
            BinaryPatternSyntax binaryPattern => HandleBinaryPattern(binaryPattern, comparand, originalComparand),
            UnaryPatternSyntax unaryPattern => HandleUnaryPattern(unaryPattern, comparand, originalComparand),
            ParenthesizedPatternSyntax parenthesizedPattern => HandleParenthesizedPattern(parenthesizedPattern,
                                                                                          comparand,
                                                                                          originalComparand),
            ConstantPatternSyntax constantPattern => HandleConstantPattern(constantPattern, comparand),
            TypePatternSyntax typePattern => HandleTypePattern(typePattern, comparand, originalComparand),
            DeclarationPatternSyntax declarationPattern => HandleDeclarationPattern(declarationPattern,
                                                                                    comparand,
                                                                                    originalComparand),

            _ => throw Logger.CompilerError($"Unhandled pattern type: {node.GetType().Name}", node)
        };

    private Call HandleDeclarationPattern(DeclarationPatternSyntax node,
                                          Expression originalValue,
                                          ExpressionSyntax csharpOriginalValue)
    {
        var typeName = Visit<Name>(node.Type);
        var oldTypeName = typeName;
        var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type;
        if (typeSymbol is { ContainingNamespace.Name: "System" or "Roblox" } && typeName is IdentifierName identifierName)
            typeName = new IdentifierName('"' + StandardUtility.GetMappedType(identifierName.Text) + '"');

        var newName = node.Designation switch
        {
            SingleVariableDesignationSyntax singleDesignation =>
                _file.OccupiedIdentifiers.AddIdentifier(singleDesignation.Identifier)
        };

        _file.Prerequisites.Add(new Variable(newName, true, new TypeCast(originalValue, new TypeRef(oldTypeName.ToString()))));

        return PatternIsType(originalValue, csharpOriginalValue, typeName);
    }

    private Call HandleTypePattern(TypePatternSyntax node, Expression comparand, ExpressionSyntax originalComparand)
    {
        var name = Visit<Name>(node.Type);
        var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type; // TODO: some sort of runtime type check
        if (typeSymbol is { ContainingNamespace.Name: "System" or "Roblox" } && name is IdentifierName identifierName)
            name = new IdentifierName('"' + StandardUtility.GetMappedType(identifierName.Text) + '"');

        return PatternIsType(comparand, originalComparand, name);
    }

    private Call PatternIsType(Expression originalValue, ExpressionSyntax csharpOriginalValue, Name typeName)
    {
        var instanceSymbol = _semanticModel.Compilation.GetTypeByMetadataName("Roblox.Instance");
        var originalValueSymbol = _semanticModel.GetTypeInfo(csharpOriginalValue).Type;
        var isInstanceValue = originalValueSymbol != null
                           && instanceSymbol != null
                           && StandardUtility.DoesTypeInheritFrom(originalValueSymbol, instanceSymbol)
                           && typeName.ToString() != "\"Instance\"";

        return isInstanceValue
            ? new Call(new MemberAccess(originalValue, new IdentifierName("IsA"), ':'),
                       new ArgumentList([new Argument(typeName)]))
            : AstUtility.Is(originalValue, typeName); // TODO: some sort of runtime type check
    }

    private BinaryOperator HandleConstantPattern(ConstantPatternSyntax node, Expression comparand)
    {
        var operand = Visit<Expression>(node.Expression);

        return new BinaryOperator(comparand, "==", operand);
    }

    private Parenthesized HandleParenthesizedPattern(ParenthesizedPatternSyntax node,
                                                     Expression comparand,
                                                     ExpressionSyntax originalComparand) =>
        new(HandlePattern(node.Pattern, comparand, originalComparand));

    private UnaryOperator HandleUnaryPattern(UnaryPatternSyntax node, Expression comparand, ExpressionSyntax originalComparand)
    {
        var operand = HandlePattern(node.Pattern, comparand, originalComparand);

        return new UnaryOperator("not ", operand);
    }

    private BinaryOperator HandleBinaryPattern(BinaryPatternSyntax node, Expression comparand, ExpressionSyntax originalComparand)
    {
        var left = HandlePattern(node.Left, comparand, originalComparand);
        var right = HandlePattern(node.Right, comparand, originalComparand);

        return new BinaryOperator(left, node.OperatorToken.Text, right);
    }

    private BinaryOperator HandleRelationalPattern(RelationalPatternSyntax node, Expression comparand)
    {
        var op = StandardUtility.GetMappedOperator(node.OperatorToken.Text);
        var operand = Visit<Expression>(node.Expression);

        return new BinaryOperator(comparand, op, operand);
    }

    private BinaryOperator HandleCaseSwitchLabel(CaseSwitchLabelSyntax caseLabel, Expression comparand)
    {
        var caseValue = Visit<Expression>(caseLabel.Value);

        return new BinaryOperator(comparand, "==", caseValue);
    }

    private IdentifierName PushToVariable(string name, Expression initializer)
    {
        var identifier = _file.OccupiedIdentifiers.AddIdentifier(name);
        _file.Prerequisites.Add(new Variable(identifier, true, initializer));

        return identifier;
    }

    private bool IsAlreadyQualified(IdentifierNameSyntax node, SyntaxNode? parent)
    {
        {
            if (FindFirstAncestor<MemberAccessExpressionSyntax>(node) is { } memberAccess)
                return memberAccess.Name.ToString() == node.ToString();

            if (FindFirstAncestor<QualifiedNameSyntax>(node) is { } qualifiedName)
                return qualifiedName.Right.ToString() == node.ToString();
        }

        if (parent != null)
        {
            if (parent.FirstAncestorOrSelf<MemberAccessExpressionSyntax>() is { } memberAccess)
                return memberAccess.Name.ToString() == node.ToString();

            if (parent.FirstAncestorOrSelf<QualifiedNameSyntax>() is { } qualifiedName)
                return qualifiedName.Right.ToString() == node.ToString();
        }

        return false;
    }

    private static LinqQueryClauseInfoKind GetLinqQueryClauseKind(SyntaxNode clause) =>
        clause.Kind() switch
        {
            SyntaxKind.SelectClause => LinqQueryClauseInfoKind.Select,
            SyntaxKind.WhereClause => LinqQueryClauseInfoKind.Where,
            SyntaxKind.OrderByClause => LinqQueryClauseInfoKind.OrderBy,
            SyntaxKind.GroupClause => LinqQueryClauseInfoKind.GroupBy,
            SyntaxKind.QueryContinuation => LinqQueryClauseInfoKind.Continuation
        };
}