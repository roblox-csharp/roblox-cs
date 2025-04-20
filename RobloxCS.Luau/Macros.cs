using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Macros;

public enum MacroKind
{
    NewInstance,
    GetService,
    ListConstruction,
    DictionaryConstruction,
    IEnumerableType,
    DictionaryType,
    ObjectMethod,
    IEnumerableMethod,
    ListMethod,
    DictionaryMethod,
    ListProperty,
    BitOperation
}

public class MacroManager(SemanticModel semanticModel, TransformState transformState, OccupiedIdentifiersStack occupiedIdentifiersStack)
{
    public Node? Assignment(Func<SyntaxNode, Node?> visit, AssignmentExpressionSyntax assignment)
    {
        var mappedOperator = StandardUtility.GetMappedOperator(assignment.OperatorToken.Text);
        var bit32MethodName = StandardUtility.GetBit32MethodName(mappedOperator);
        if (bit32MethodName != null)
        {
            var target = (AssignmentTarget)visit(assignment.Left)!;
            var value = (Expression)visit(assignment.Right)!;
            var bit32Call = AstUtility.Bit32Call(bit32MethodName, target, value);
            bit32Call.MarkExpanded(MacroKind.BitOperation);

            return new Assignment(target, AstUtility.Bit32Call(bit32MethodName, target, value));
        }

        var leftSymbol = semanticModel.GetSymbolInfo(assignment.Left).Symbol;
        if (leftSymbol is IEventSymbol eventSymbol)
        {
            var symbolMetadata = SymbolMetadataManager.Get(eventSymbol);
            symbolMetadata.EventConnectionName ??=
                AstUtility.CreateSimpleName<IdentifierName>(assignment, "conn_" + eventSymbol.Name, true);

            var connectionName = symbolMetadata.EventConnectionName;
            switch (mappedOperator)
            {
                case "+=":
                {
                    var left = (Expression)visit(assignment.Left)!;
                    var right = (Expression)visit(assignment.Right)!;
                    return new Variable(
                        connectionName,
                        true,
                        new Call(
                            new MemberAccess(left, new IdentifierName("Connect"), ':'),
                            new ArgumentList([new Argument(right)])
                        )
                    );
                }
                case "-=":
                {
                    return new Call(
                        new MemberAccess(connectionName, new IdentifierName("Disconnect"), ':'),
                        new ArgumentList([])
                    );
                }
            }
        }

        return null;
    }

    public Expression? BinaryExpression(Func<SyntaxNode, Node?> visit, BinaryExpressionSyntax binaryExpression)
    {
        var mappedOperator = StandardUtility.GetMappedOperator(binaryExpression.OperatorToken.Text);
        var bit32MethodName = StandardUtility.GetBit32MethodName(mappedOperator);
        if (bit32MethodName != null)
        {
            var left = (Expression)visit(binaryExpression.Left)!;
            var right = (Expression)visit(binaryExpression.Right)!;
            var bit32Call = AstUtility.Bit32Call(bit32MethodName, left, right);
            bit32Call.MarkExpanded(MacroKind.BitOperation);

            return bit32Call;
        }

        return null;
    }

    /// <summary>
    /// Takes a C# generic name and expands the name into a macro'd type
    /// </summary>
    public Name? GenericName(Func<SyntaxNode, Node?> visit, GenericNameSyntax genericName)
    {
        var typeInfo = semanticModel.GetTypeInfo(genericName);
        if (StandardUtility.IsFromSystemNamespace(typeInfo.Type))
            switch (genericName.Identifier.Text)
            {
                // lord i am sorry for my sins
                // returning IdentifierName because when visiting GenericNameSyntax (C#) it expects that a Name (luau) is returned
                case "List":
                case "IEnumerable":
                {
                    var elementTypeName = visit(genericName.TypeArgumentList.Arguments.First())!;
                    var expanded =
                        new IdentifierName($"{{ {StandardUtility.GetMappedType(elementTypeName.ToString()!)} }}");
                    
                    expanded.MarkExpanded(MacroKind.IEnumerableType);
                    return expanded;
                }

                case "Dictionary":
                {
                    var keyTypeName = visit(genericName.TypeArgumentList.Arguments.First())!;
                    var valueTypeName = visit(genericName.TypeArgumentList.Arguments.Last())!;
                    var expanded =
                        new IdentifierName(
                            $"{{ [{StandardUtility.GetMappedType(keyTypeName.ToString()!)}]: {StandardUtility.GetMappedType(valueTypeName.ToString()!)} }}");
                    expanded.MarkExpanded(MacroKind.DictionaryType);

                    return expanded;
                }
            }

        return null;
    }

    /// <summary>Takes a C# member access and expands the macro into a Luau expression</summary>
    /// <returns>The expanded expression of the macro, or null if no macro was applied</returns>
    public Node? MemberAccess(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess)
    {
        var expressionType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
        {
            if (memberAccess is { Name.Identifier.Text: { } serviceName }
                && expressionType?.Name == "Services"
                && expressionType.ContainingNamespace.Name == "Roblox")
            {
                var getService = new MemberAccess(new IdentifierName("game"), new IdentifierName("GetService"), ':');
                var expanded = new Call(getService,
                    new ArgumentList([new Argument(new Literal('"' + serviceName + '"'))]));
                expanded.MarkExpanded(MacroKind.GetService);

                return expanded;
            }
        }
        {
            if (memberAccess is
                {
                    Parent: InvocationExpressionSyntax invocation,
                    Expression: IdentifierNameSyntax { Identifier.Text: "Instance" },
                    Name: GenericNameSyntax { Identifier.Text: "Create" } genericName
                } &&
                expressionType?.ContainingNamespace.Name == "Roblox")
            {
                var className = genericName.TypeArgumentList.Arguments.First().ToString();
                var instanceConstructor = new MemberAccess(new IdentifierName("Instance"), new IdentifierName("new"));

                List<Argument> arguments = [new(new Literal($"\"{className}\""))];
                var invocationArguments = (ArgumentList)visit(invocation.ArgumentList)!;
                arguments.AddRange(invocationArguments.Arguments);

                var expanded = new Call(instanceConstructor, new ArgumentList(arguments));
                expanded.MarkExpanded(MacroKind.NewInstance);

                return expanded;
            }
        }
        {
            if (memberAccess is { Parent: InvocationExpressionSyntax invocation })
            {
                {
                    if (StandardUtility.DoesTypeInheritFrom(expressionType, "Object")
                        && ObjectMethod(visit, memberAccess, invocation, out var expanded))
                    {
                        return expanded;
                    }
                }
                {
                    if (StandardUtility.DoesTypeInheritFrom(expressionType, "Instance")
                        && InstanceMethod(visit, memberAccess, invocation, out var expanded))
                    {
                        return expanded;
                    }
                }

                switch (expressionType?.Name)
                {
                    case "Dictionary":
                    {
                        if (DictionaryMethod(visit, memberAccess, invocation, out var expanded))
                            return expanded;

                        break;
                    }

                    case "List":
                    {
                        if (ListMethod(visit, memberAccess, invocation, out var expanded))
                            return expanded;

                        break;
                    }
                }
            }
            else
            {
                switch (expressionType?.Name)
                {
                    case "List":
                    {
                        if (ListProperty(visit, memberAccess, out var expanded))
                            return expanded;

                        break;
                    }
                }
            }
        }

        return null;
    }

    private bool ListProperty(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess, out Node? expanded)
    {
        expanded = null;
        var self = (Expression)visit(memberAccess.Expression)!;
        
        switch (memberAccess.Name.Identifier.Text)
        {
            case "Count":
                expanded = new UnaryOperator("#", self);
                break;
        }

        expanded?.MarkExpanded(MacroKind.ListProperty);
        return expanded != null;
    }

    /// <summary>Takes a C# object creation and expands the macro into a Luau expression</summary>
    /// <returns>The expanded expression of the macro, or null if no macro was applied</returns>
    public Expression? ObjectCreation(Func<SyntaxNode, Node?> visit,
        BaseObjectCreationExpressionSyntax baseObjectCreation)
    {
        // generic objects
        var symbol = baseObjectCreation is ObjectCreationExpressionSyntax objectCreation
            ? semanticModel.GetSymbolInfo(objectCreation.Type).Symbol
            : semanticModel.GetSymbolInfo(baseObjectCreation).Symbol?.ContainingSymbol;

        if (symbol is not INamedTypeSymbol { TypeParameters.Length: > 0 } namedTypeSymbol)
            return null;

        switch (namedTypeSymbol.Name)
        {
            case "List":
            {
                var expressions = baseObjectCreation.Initializer?.Expressions
                    .Select(expression => (Expression)visit(expression)!).ToList();
                var table = new TableInitializer(expressions ?? []);
                table.MarkExpanded(MacroKind.ListConstruction);

                return table;
            }

            case "Dictionary":
            {
                var values = new List<Expression>();
                var keys = new List<Expression>();

                if (baseObjectCreation.Initializer != null)
                    foreach (var expression in baseObjectCreation.Initializer.Expressions)
                        switch (expression)
                        {
                            case AssignmentExpressionSyntax assignmentExpression:
                            {
                                var key = (Expression)visit(assignmentExpression.Left)!;
                                var value = (Expression)visit(assignmentExpression.Right)!;

                                values.Add(value);
                                keys.Add(key);
                                break;
                            }

                            case InitializerExpressionSyntax initializerExpression:
                            {
                                var key = (Expression)visit(initializerExpression.Expressions[0])!;
                                var value = (Expression)visit(initializerExpression.Expressions[1])!;

                                values.Add(value);
                                keys.Add(key);
                                break;
                            }
                        }

                var table = new TableInitializer(values, keys);
                table.MarkExpanded(MacroKind.DictionaryConstruction);

                return table;
            }
        }

        return null;
    }
    
    /// <summary>Macros <see cref="Instance" /> methods</summary>
    private static bool InstanceMethod(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess,
        InvocationExpressionSyntax invocation, out Expression? expanded)
    {
        expanded = null;
        switch (memberAccess.Name.Identifier.Text)
        {
            case "FindFirstAncestorWhichIsA":
            case "FindFirstAncestorOfClass":
            case "FindFirstChildWhichIsA":
            case "FindFirstChildOfClass":
            case "IsA":
            {
                if (memberAccess.Name is not GenericNameSyntax genericName) break;
                var self = (Expression)visit(memberAccess.Expression)!;
                expanded = new Call(
                    new MemberAccess(self, new IdentifierName(memberAccess.Name.Identifier.Text), ':'),
                    new ArgumentList([new Argument(new Literal($"\"{genericName.TypeArgumentList.Arguments.First().ToString()}\""))])
                );
                break;
            }
        }

        expanded?.MarkExpanded(MacroKind.ObjectMethod);
        return expanded != null;
    }

    /// <summary>Macros <see cref="Object" /> methods</summary>
    private static bool ObjectMethod(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess,
        InvocationExpressionSyntax invocation, out Expression? expanded)
    {
        expanded = null;
        switch (memberAccess.Name.Identifier.Text)
        {
            case "ToString":
            {
                var self = (Expression)visit(memberAccess.Expression)!;
                expanded = new Call(new IdentifierName("tostring"), new ArgumentList([new Argument(self)]));
                break;
            }
            case "GetType":
                throw Logger.UnsupportedError(memberAccess.Name, "Object.GetType()", true, false);
        }

        expanded?.MarkExpanded(MacroKind.ObjectMethod);
        return expanded != null;
    }

    /// <summary>Macros <see cref="List" /> methods</summary>
    private bool ListMethod(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess,
        InvocationExpressionSyntax invocation, out Node? expanded)
    {
        expanded = null;
        var listExpression = (Expression)visit(memberAccess.Expression)!;
        Expression self;

        if (listExpression is not IdentifierName name)
        {
            self = occupiedIdentifiersStack.AddIdentifier("_exp");
            transformState.Prereq(new Variable((IdentifierName)self, true, listExpression));
        }
        else
            self = name;

        var filterFuncIdentifier = occupiedIdentifiersStack.AddIdentifier("_filterFunc");
        switch (memberAccess.Name.Identifier.Text)
        {
            case "Add":
            {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                arguments.Arguments.Insert(0, new Argument(self));

                expanded = AstUtility.TableCall("insert", arguments);
                break;
            }
            case "AsReadOnly":
            {
                expanded = AstUtility.TableCall("freeze", new ArgumentList([new Argument(self)]));
                break;
            }
            case "Contains":
            {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                arguments.Arguments.Insert(0, new Argument(self));

                expanded = new BinaryOperator(
                    AstUtility.TableCall("find", arguments),
                    "~=", AstUtility.Nil());
                break;
            }
            case "Clear":
            {
                expanded = AstUtility.TableCall("clear", new ArgumentList([new Argument(self)]));
                break;
            }
            case "Exists":
            {
                var FilterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");

                transformState.Prereq(new Block([
                    new Variable(filterFuncIdentifier, true, FilterFunc.Arguments.First()),
                    new Variable(expression, true, AstUtility.False()),
                    new For([key, value], self, new Block([
                        new If(new Call(
                            filterFuncIdentifier,
                            new ArgumentList([new Argument(value)])),
                            new Block([
                                new Assignment(expression, AstUtility.True()),
                                new Break()
                            ]))
                    ]))
                ]));
                expanded = expression;
                break;
            }
            case "Find":
            {
                var filterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");

                transformState.Prereq(new Block([
                    new Variable(expression, true, AstUtility.Nil()),
                    new Variable(filterFuncIdentifier, true, filterFunc.Arguments.First()),
                    new For([key, value], self, new Block([
                        new If(new Call(filterFuncIdentifier, new ArgumentList([new Argument(value)])), new Block([
                            new Assignment(expression, value),
                            new Break()
                        ]))
                    ]))
                ]));
                expanded = expression;
                break;
            }
            case "FindLast":
            {
                var filterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");

                transformState.Prereq(new Block([
                    new Variable(filterFuncIdentifier, true, filterFunc.Arguments.First()),
                    new Variable(expression, true),
                    new For([key, value], self, new Block([
                        new If(new Call(
                            filterFuncIdentifier,
                            new ArgumentList([new Argument(value)])),
                            new Block([
                                new Assignment(expression, value)
                            ]))
                    ]))
                ]));

                expanded = expression;
                break;
            }
            case "FindAll":
            {
                var filterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");

                transformState.Prereq(new Block([
                    new Variable(filterFuncIdentifier, true, filterFunc.Arguments.First()),
                    new Variable(expression, true, new TableInitializer()),
                    new For([key, value], self, new Block([
                        new If(new Call(
                            filterFuncIdentifier,
                            new ArgumentList([new Argument(value)])),
                            new Block([
                                new ExpressionStatement(AstUtility.TableCall("insert", new ArgumentList([new Argument(expression), new Argument(value)])))
                            ]))
                    ]))
                ]));
                expanded = expression;
                break;
            }
            case "AddRange":
            {
                var table = (ArgumentList)visit(invocation.ArgumentList)!;
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");

                expanded = new Block([
                    new For([key, value], table.Arguments.First(), new Block([
                        new ExpressionStatement(AstUtility.TableCall("insert", new ArgumentList([new Argument(self), new Argument(value)])))
                    ]))
                ]);
                break;
            }
            case "ForEach":
            {
                var args = (ArgumentList)visit(invocation.ArgumentList)!;
                var funcBody = (AnonymousFunction)args.Arguments.First().Expression!;
                var key = occupiedIdentifiersStack.AddIdentifier("_k");

                expanded =
                    new For([key, funcBody.ParameterList.Parameters.First().Name], self, funcBody.Body!);
                break;
            }
            case "ConvertAll":
            {
                var convertFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");
                var convertFuncIdentifier = occupiedIdentifiersStack.AddIdentifier("_convertFunc");

                transformState.Prereq(new Block([
                    new Variable(convertFuncIdentifier, true, convertFunc.Arguments.First()),
                    new Variable(expression, true,
                        AstUtility.TableCall("create",
                            new ArgumentList([new Argument(new UnaryOperator("#", self))]))),
                    new For([key, value], self, new Block([
                        new ExpressionStatement(AstUtility.TableCall("insert", new ArgumentList([
                            new Argument(expression),
                            new Argument(new Call(
                                convertFuncIdentifier,
                                new ArgumentList([new Argument(value)])))
                        ])))
                    ]))
                ]));
                expanded = expression;
                break;
            }
            case "FindIndex":
            {
                var filterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");

                if (invocation.ArgumentList.Arguments.Count == 1)
                {
                    transformState.Prereq(new Block([
                        new Variable(filterFuncIdentifier, true, filterFunc.Arguments.First()),
                        new Variable(expression, true),
                        new For([key, value], self, new Block([
                            new If(new Call(
                                filterFuncIdentifier,
                                new ArgumentList([new Argument(value)])),
                                new Block([
                                    new Assignment(expression, key),
                                    new Break()
                                ]))
                        ]))
                    ]));
                }
                else if (invocation.ArgumentList.Arguments.Count >= 2)
                {
                    Expression max = invocation.ArgumentList.Arguments.Count == 3
                        ? new BinaryOperator(filterFunc.Arguments.First(), "+", filterFunc.Arguments.ElementAt(1))
                        : new UnaryOperator("#", self);

                    var indexIdentifier = occupiedIdentifiersStack.AddIdentifier("_i");
                    transformState.Prereq(new Block([
                        new Variable(filterFuncIdentifier, true,
                            invocation.ArgumentList.Arguments.Count == 2
                                ? filterFunc.Arguments.ElementAt(1)
                                : filterFunc.Arguments.ElementAt(2)),
                        
                        new NumericFor(indexIdentifier, filterFunc.Arguments.First().Expression, max, null,
                            new Block([
                                new Variable(value, true, new ElementAccess(self, indexIdentifier)),
                                new If(new Call(
                                    filterFuncIdentifier, 
                                    new ArgumentList([new Argument(value)])),
                                    new Block([
                                        new Assignment(expression, key),
                                        new Break()
                                    ]))
                            ]))
                    ]));
                }

                expanded = expression;
                break;
            }
            case "FindIndexLast":
            {
                var filterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");

                if (invocation.ArgumentList.Arguments.Count == 1)
                {
                    transformState.Prereq(new Block([
                        new Variable(filterFuncIdentifier, true, filterFunc.Arguments.First()),
                        new Variable(expression, true),
                        new For([key, value], self, new Block([
                            new If(new Call(
                                filterFuncIdentifier,
                                new ArgumentList([new Argument(value)])),
                                new Block([
                                    new Assignment(expression, key)
                                ]))
                        ]))
                    ]));
                }
                else if (invocation.ArgumentList.Arguments.Count >= 2)
                {
                    Expression max = invocation.ArgumentList.Arguments.Count == 3
                        ? new BinaryOperator(filterFunc.Arguments.First(), "+", filterFunc.Arguments.ElementAt(1))
                        : new UnaryOperator("#", self);

                    var indexIdentifier = occupiedIdentifiersStack.AddIdentifier("_i");
                    transformState.Prereq(new Block([
                        new Variable(filterFuncIdentifier, true,
                            invocation.ArgumentList.Arguments.Count == 2
                                ? filterFunc.Arguments.ElementAt(1)
                                : filterFunc.Arguments.ElementAt(2)),
                        new NumericFor(indexIdentifier, filterFunc.Arguments.First().Expression, max, null,
                            new Block([
                                new Variable(value, true, new ElementAccess(self, indexIdentifier)),
                                new If(new Call(
                                    filterFuncIdentifier, 
                                    new ArgumentList([new Argument(value)])),
                                    new Block([
                                        new Assignment(expression, key)
                                    ]))
                            ]))
                    ]));
                }

                expanded = expression;
                break;
            }
            case "IndexOf":
            {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");
                var shouldCreateVariable =
                    invocation.ArgumentList.Arguments.First().Expression is not LiteralExpressionSyntax;

                List<Statement> block =
                [
                    new Variable(expression, true),
                    new For([key, value], self, new Block([
                        new If(
                            new BinaryOperator(
                                shouldCreateVariable
                                    ? new IdentifierName("_val")
                                    : arguments.Arguments.First().Expression, "==", value), new Block([
                                new Assignment(expression, key),
                                new Break()
                            ]))
                    ]))
                ];

                if (shouldCreateVariable)
                    block.Insert(0, new Variable(new IdentifierName("_val"), true, arguments.Arguments.First()));

                transformState.Prereq(new Block(block));

                expanded = expression;
                break;
            }
            case "IndexOfLast":
            {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");
                var shouldCreateVariable =
                    invocation.ArgumentList.Arguments.First().Expression is not LiteralExpressionSyntax;

                List<Statement> block =
                [
                    new Variable(expression, true),
                    new For([key, value], self, new Block([
                        new If(
                            new BinaryOperator(
                                shouldCreateVariable
                                    ? new IdentifierName("_val")
                                    : arguments.Arguments.First().Expression, "==", value), new Block([
                                new Assignment(expression, key)
                            ]))
                    ]))
                ];

                if (shouldCreateVariable)
                    block.Insert(0, new Variable(new IdentifierName("_val"), true, arguments.Arguments.First()));

                transformState.PrereqList(block);

                expanded = expression;
                break;
            }
            case "Insert":
            {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;

                expanded = AstUtility.TableCall("insert", new ArgumentList([new Argument(self), arguments.Arguments.First(), arguments.Arguments.Last()]));
                break;
            }
            case "Remove":
            {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;

                expanded = AstUtility.TableCall("remove",
                    new ArgumentList([
                        new Argument(self),
                        new Argument(AstUtility.TableCall("find",
                            new ArgumentList([
                                new Argument(self), arguments.Arguments.First()
                            ])))
                    ]));
                break;
            }
        }

        expanded?.MarkExpanded(MacroKind.ListMethod);
        return expanded != null;
    }

    /// <summary>Macros <see cref="Dictionary" /> methods</summary>
    private bool DictionaryMethod(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess,
        InvocationExpressionSyntax invocation, out Expression? expanded)
    {
        expanded = null;
        switch (memberAccess.Name.Identifier.Text)
        {
            case "Add":
            {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                var self = (Expression)visit(memberAccess.Expression)!;
                var key = arguments.Arguments.First().Expression;
                var value = arguments.Arguments.Last().Expression;

                var originalIdentifier = occupiedIdentifiersStack.AddIdentifier("_original");
                transformState.Prereq(new Assignment(new ElementAccess(self, key), value));
                expanded = originalIdentifier;
                break;
            }
            case "ContainsKey":
            {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                var self = (Expression)visit(memberAccess.Expression)!;
                var key = arguments.Arguments.First().Expression;

                expanded = new BinaryOperator(new ElementAccess(self, key), "~=", AstUtility.Nil());
                break;
            }
            case "Clear":
            {
                var self = (Expression)visit(memberAccess.Expression)!;

                expanded = new Call(new QualifiedName(new IdentifierName("table"), new IdentifierName("clear")),
                    new ArgumentList([new Argument(self)]));
                break;
            }
        }

        expanded?.MarkExpanded(MacroKind.DictionaryMethod);
        return expanded != null;
    }
}