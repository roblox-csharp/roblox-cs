using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Macros;

public enum MacroKind
{
    NewInstance,
    GetService,
    IEnumerableConstruction,
    DictionaryConstruction,
    IEnumerableType,
    DictionaryType,
    ObjectMethod,
    EnumerableMethod,
    ListMethod,
    DictionaryMethod,
    ListProperty,
    BitOperation,
    EventInvoke
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

        var leftSymbol = ModelExtensions.GetSymbolInfo(semanticModel, assignment.Left).Symbol;
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
        var typeInfo = ModelExtensions.GetTypeInfo(semanticModel, genericName);
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
        var expressionType = ModelExtensions.GetTypeInfo(semanticModel, memberAccess.Expression).Type;
        var expressionSymbol = ModelExtensions.GetSymbolInfo(semanticModel, memberAccess.Expression).Symbol;
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
            if (expressionSymbol is IEventSymbol eventSymbol
                && memberAccess is
                  {
                      Parent: InvocationExpressionSyntax invocation,
                      Name: IdentifierNameSyntax { Identifier.Text: "Invoke" } name
                  })
            {
                var expression = (Expression)visit(memberAccess.Expression)!;
                var expanded = new MemberAccess(expression, new IdentifierName("Fire"), ':');
                expanded.MarkExpanded(MacroKind.EventInvoke);
                
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
                        if (EnumerableMethod(visit, memberAccess, invocation, out var enumerableExpanded))
                            return enumerableExpanded;
                        if (DictionaryMethod(visit, memberAccess, invocation, out var expanded))
                            return expanded;

                        break;
                    }
                    case "Enumerable":
                    case "IEnumerable":
                    case "List":
                    {
                        if (EnumerableMethod(visit, memberAccess, invocation, out var enumerableExpanded))
                            return enumerableExpanded;
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
    
    private bool EnumerableMethod(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess,
        InvocationExpressionSyntax invocation, out Expression? expanded)
    {
        expanded = null;
        var arguments = invocation.ArgumentList.Arguments;
        var self = (Expression)visit(memberAccess.Expression)!;
        var one = new Literal("1");
        var zero = new Literal("0");

        switch (memberAccess.Name.Identifier.Text)
        {
            // TODO: error if index is out of bounds
            case "First":
            {
                expanded = new ElementAccess(self, one);
                break;
            }
            case "FirstOrDefault":
            {
                if (arguments.Count == 0)
                {
                    expanded = new ElementAccess(self, one);
                    break;
                }
                
                var foundIdentifier = occupiedIdentifiersStack.AddIdentifier("_found");
                var defaultIdentifier = occupiedIdentifiersStack.AddIdentifier("_default");
                var predicate = (Expression)visit(arguments.First().Expression)!;
                transformState.Prereq(new Variable(foundIdentifier, true));
                transformState.Prereq(new Variable(defaultIdentifier, true, predicate));

                occupiedIdentifiersStack.Push();
                var valueIdentifier = occupiedIdentifiersStack.AddIdentifier("v");
                transformState.Prereq(
                    new If(
                        new BinaryOperator(
                            new Call(new IdentifierName("typeof"), AstUtility.CreateArgumentList([defaultIdentifier])),
                            "==",
                            new Literal("\"function\"")),
                        new Block([
                            new For(
                                [AstUtility.DiscardName, valueIdentifier],
                                self,
                                new Block([
                                    new Block([
                                        new If(
                                            new UnaryOperator("not ", new Call(defaultIdentifier, AstUtility.CreateArgumentList([valueIdentifier]))),
                                            new Block([new Continue()]))
                                    ]),
                                    new Assignment(foundIdentifier, valueIdentifier),
                                    new Break()
                                ]))
                        ]),
                        new Block([
                            new Assignment(foundIdentifier, new BinaryOperator(
                                new ElementAccess(self, one),
                                "or",
                                defaultIdentifier))
                        ])));
                occupiedIdentifiersStack.Pop();
                expanded = foundIdentifier;
                
                break;
            }
            // TODO: error if index is out of bounds
            case "Last":
            {
                expanded = new ElementAccess(self, new UnaryOperator("#", self));
                break;
            }
            case "LastOrDefault":
            {
                if (arguments.Count == 0)
                {
                    expanded = new ElementAccess(self, new UnaryOperator("#", self));
                    break;
                }
                
                var foundIdentifier = occupiedIdentifiersStack.AddIdentifier("_found");
                var defaultIdentifier = occupiedIdentifiersStack.AddIdentifier("_default");
                var predicate = (Expression)visit(arguments.First().Expression)!;
                transformState.Prereq(new Variable(foundIdentifier, true));
                transformState.Prereq(new Variable(defaultIdentifier, true, predicate));

                occupiedIdentifiersStack.Push();
                var valueIdentifier = occupiedIdentifiersStack.AddIdentifier("v");
                transformState.Prereq(
                    new If(
                        new BinaryOperator(
                            new Call(new IdentifierName("typeof"), AstUtility.CreateArgumentList([defaultIdentifier])),
                            "==",
                            new Literal("\"function\"")),
                        new Block([
                            new For(
                                [AstUtility.DiscardName, valueIdentifier],
                                self,
                                new Block([
                                    new Block([
                                        new If(
                                            new UnaryOperator("not ", new Call(defaultIdentifier, AstUtility.CreateArgumentList([valueIdentifier]))),
                                            new Block([new Continue()]))
                                    ]),
                                    new Assignment(foundIdentifier, valueIdentifier)
                                ]))
                        ]),
                        new Block([
                            new Assignment(foundIdentifier, new BinaryOperator(
                                new ElementAccess(self, new UnaryOperator("#", self)),
                                "or",
                                defaultIdentifier))
                        ])));
                occupiedIdentifiersStack.Pop();
                expanded = foundIdentifier;
                
                break;
            }
            case "Count":
            {
                expanded = new UnaryOperator("#", self);
                break;
            }
            case "ElementAt": // TODO: error if index is out of bounds
            case "ElementAtOrDefault":
            {
                var index = (Expression)visit(arguments.First().Expression)!;
                expanded = new ElementAccess(self, AstUtility.AddOne(index));
                break;
            }
            case "Append":
            {
                var appendedIdentifier = occupiedIdentifiersStack.AddIdentifier("_appended");
                var element = (Expression)visit(arguments.First().Expression)!;
                
                transformState.Prereq(new Variable(appendedIdentifier, true, AstUtility.TableCall("clone", self)));
                transformState.Prereq(new ExpressionStatement(AstUtility.TableCall("insert", appendedIdentifier, element)));
                expanded = appendedIdentifier;
                
                break;
            }
            case "Prepend":
            {
                var prependedIdentifier = occupiedIdentifiersStack.AddIdentifier("_prepended");
                var element = (Expression)visit(arguments.First().Expression)!;
                
                transformState.Prereq(new Variable(prependedIdentifier, true, AstUtility.TableCall("clone", self)));
                transformState.Prereq(new ExpressionStatement(AstUtility.TableCall("insert", prependedIdentifier, one, element)));
                expanded = prependedIdentifier;
                
                break;
            }
            case "Distinct":
            {
                var distinctIdentifier = occupiedIdentifiersStack.AddIdentifier("_distinct");
                var seenIdentifier = occupiedIdentifiersStack.AddIdentifier("_seen");
                var valueIdentifier = occupiedIdentifiersStack.AddIdentifier("v");
                var seenValue = new ElementAccess(seenIdentifier, valueIdentifier);
                
                transformState.Prereq(new Variable(distinctIdentifier, true, TableInitializer.Empty));
                transformState.Prereq(new Variable(seenIdentifier, true, TableInitializer.Empty));
                transformState.Prereq(new For(
                    [AstUtility.DiscardName, valueIdentifier],
                    self,
                    new Block([
                        new If(
                            seenValue,
                            new Block([new Continue()])),
                        new Assignment(seenValue, AstUtility.True),
                        new ExpressionStatement(AstUtility.TableCall("insert", distinctIdentifier, valueIdentifier)),
                    ])));
                expanded = distinctIdentifier;
                
                break;
            }
            case "Concat":
            {
                var resultIdentifier = occupiedIdentifiersStack.AddIdentifier("_result");
                var valueIdentifier = occupiedIdentifiersStack.AddIdentifier("v");
                var other = (Expression)visit(arguments.First().Expression)!;
                
                transformState.Prereq(new Variable(resultIdentifier, true, AstUtility.TableCall("clone", self)));
                transformState.Prereq(new For(
                    [AstUtility.DiscardName, valueIdentifier],
                    other,
                    new Block([
                        new ExpressionStatement(AstUtility.TableCall("insert", resultIdentifier, valueIdentifier)),
                    ])));
                expanded = resultIdentifier;
                
                break;
            }
            case "Intersect":
            {
                var resultIdentifier = occupiedIdentifiersStack.AddIdentifier("_result");
                var firstIdentifier = occupiedIdentifiersStack.AddIdentifier("a");
                var secondIdentifier = occupiedIdentifiersStack.AddIdentifier("b");
                var other = (Expression)visit(arguments.First().Expression)!;
                
                transformState.Prereq(new Variable(resultIdentifier, true, TableInitializer.Empty));
                transformState.Prereq(new For(
                    [AstUtility.DiscardName, firstIdentifier],
                    self,
                    new Block([
                        new For(
                            [AstUtility.DiscardName, secondIdentifier],
                            other,
                            new Block([
                                new If(
                                    new BinaryOperator(firstIdentifier, "~=", secondIdentifier),
                                    new Block([new Continue()])),
                                new ExpressionStatement(AstUtility.TableCall("insert", resultIdentifier, firstIdentifier))
                            ]))
                    ])));
                
                expanded = resultIdentifier;
                
                break;
            }
            case "GetEnumerator":
            {
                var selfIdentifier = new IdentifierName("self");
                var indexIdentifier = new IdentifierName("_index");
                var currentIdentifier = new IdentifierName("Current");
                var gotValueIdentifier = new IdentifierName("gotValue");
                var indexField = new MemberAccess(selfIdentifier, indexIdentifier);
                var currentField = new MemberAccess(selfIdentifier, currentIdentifier);
                
                expanded = new TableInitializer(
                    [
                        AstUtility.Nil,
                        zero,
                        new AnonymousFunction(
                            new ParameterList([new Parameter(selfIdentifier)]),
                            new TypeRef("boolean"),
                            new Block([
                                new ExpressionStatement(new BinaryOperator(
                                    indexField,
                                    "+=",
                                    one)),
                                new Variable(
                                    gotValueIdentifier,
                                    true,
                                    new BinaryOperator(
                                        indexField,
                                        "<=",
                                        new UnaryOperator("#", self))),
                                new Assignment(currentField, new IfExpression(
                                    gotValueIdentifier,
                                    new ElementAccess(self, indexField),
                                    AstUtility.Nil,
                                    true)),
                                new Return(gotValueIdentifier)
                            ])),
                        new AnonymousFunction(
                            new ParameterList([new Parameter(selfIdentifier)]),
                            new TypeRef("()"),
                            new Block([
                                new Assignment(indexField, zero),
                                new Assignment(currentField, AstUtility.Nil),
                            ])),
                        new AnonymousFunction(
                            new ParameterList([new Parameter(selfIdentifier)]),
                            new TypeRef("()")) // no-op
                    ],
                    [
                        currentIdentifier,
                        indexIdentifier,
                        new IdentifierName("MoveNext"),
                        new IdentifierName("Reset"),
                        new IdentifierName("Dispose") // for API completeness
                    ]);
                
                break;
            }
            case "ToList":
            case "ToArray":
            case "ToDictionary":
            {
                expanded = self;
                break;
            }
        }
        
        expanded?.MarkExpanded(MacroKind.EnumerableMethod);
        return expanded != null;
    } 

    private static bool ListProperty(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess, out Node? expanded)
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
            case "IEnumerable":
            case "List":
            {
                var expressions = baseObjectCreation.Initializer?.Expressions
                    .Select(visit)
                    .OfType<Expression>()
                    .ToList();
                
                var table = new TableInitializer(expressions ?? []);
                table.MarkExpanded(MacroKind.IEnumerableConstruction);

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
    private bool ObjectMethod(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess,
        InvocationExpressionSyntax invocation, out Expression? expanded)
    {
        var expressionSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
        
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

        switch (memberAccess.Name.Identifier.Text)
        {
            case "Add":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                expanded = AstUtility.TableCall("insert", [self, ..arguments]);
                break;
            }
            case "AsReadOnly":
            {
                expanded = AstUtility.TableCall("freeze", self);
                break;
            }
            case "Contains":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                expanded = new BinaryOperator(
                    AstUtility.TableCall("find", [self, ..arguments]),
                    "~=", AstUtility.Nil);
                break;
            }
            case "Clear":
            {
                expanded = AstUtility.TableCall("clear", self);
                break;
            }
            case "Exists":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");
                var filterFuncIdentifier = occupiedIdentifiersStack.AddIdentifier("_filterFunc");

                transformState.Prereq(new Block([
                    new Variable(filterFuncIdentifier, true, arguments.First()),
                    new Variable(expression, true, AstUtility.False),
                    new For([key, value], self, new Block([
                        new If(new Call(
                            filterFuncIdentifier,
                            new ArgumentList([new Argument(value)])),
                            new Block([
                                new Assignment(expression, AstUtility.True),
                                new Break()
                            ]))
                    ]))
                ]));
                expanded = expression;
                break;
            }
            case "Find":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");
                var filterFuncIdentifier = occupiedIdentifiersStack.AddIdentifier("_filterFunc");

                transformState.Prereq(new Block([
                    new Variable(expression, true, AstUtility.Nil),
                    new Variable(filterFuncIdentifier, true, arguments.First()),
                    new For([key, value], self, new Block([
                        new If(new Call(
                            filterFuncIdentifier, 
                            new ArgumentList([new Argument(value)])),
                            new Block([
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
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");
                var filterFuncIdentifier = occupiedIdentifiersStack.AddIdentifier("_filterFunc");

                transformState.Prereq(new Block([
                    new Variable(filterFuncIdentifier, true, arguments.First()),
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
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");
                var filterFuncIdentifier = occupiedIdentifiersStack.AddIdentifier("_filterFunc");

                transformState.Prereq(new Block([
                    new Variable(filterFuncIdentifier, true, arguments.First()),
                    new Variable(expression, true, new TableInitializer()),
                    new For([key, value], self, new Block([
                        new If(new Call(
                            filterFuncIdentifier,
                            new ArgumentList([new Argument(value)])),
                            new Block([
                                new ExpressionStatement(AstUtility.TableCall("insert", expression, value))
                            ]))
                    ]))
                ]));
                expanded = expression;
                break;
            }
            case "AddRange":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");

                expanded = new Block([
                    new For([key, value], arguments.First(), new Block([
                        new ExpressionStatement(AstUtility.TableCall("insert", self, value))
                    ]))
                ]);
                break;
            }
            case "ForEach":
            {
                var args = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var funcBody = (AnonymousFunction)args.First();
                var key = occupiedIdentifiersStack.AddIdentifier("_k");

                expanded =
                    new For([key, funcBody.ParameterList.Parameters.First().Name], self, funcBody.Body!);
                break;
            }
            case "ConvertAll":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");
                var convertFuncIdentifier = occupiedIdentifiersStack.AddIdentifier("_convertFunc");

                transformState.Prereq(new Block([
                    new Variable(convertFuncIdentifier, true, arguments.First()),
                    new Variable(expression, true,
                        AstUtility.TableCall("create",
                            new ArgumentList([new Argument(new UnaryOperator("#", self))]))),
                    new For([key, value], self, new Block([
                        new ExpressionStatement(AstUtility.TableCall(
                            "insert",
                            expression,
                            new Call(
                                convertFuncIdentifier,
                                new ArgumentList([new Argument(value)]))))
                    ]))
                ]));
                expanded = expression;
                break;
            }
            case "FindIndex":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");
                var filterFuncIdentifier = occupiedIdentifiersStack.AddIdentifier("_filterFunc");

                if (invocation.ArgumentList.Arguments.Count == 1)
                {
                    transformState.Prereq(new Block([
                        new Variable(filterFuncIdentifier, true, arguments.First()),
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
                        ? new BinaryOperator(arguments.First(), "+", arguments.ElementAt(1))
                        : new UnaryOperator("#", self);

                    var indexIdentifier = occupiedIdentifiersStack.AddIdentifier("_i");
                    transformState.Prereq(new Block([
                        new Variable(filterFuncIdentifier, true,
                            invocation.ArgumentList.Arguments.Count == 2
                                ? arguments.ElementAt(1)
                                : arguments.ElementAt(2)),
                        
                        new NumericFor(indexIdentifier, arguments.First(), max, null,
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
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                var expression = occupiedIdentifiersStack.AddIdentifier("_newValue");
                var key = occupiedIdentifiersStack.AddIdentifier("_k");
                var value = occupiedIdentifiersStack.AddIdentifier("_v");
                var filterFuncIdentifier = occupiedIdentifiersStack.AddIdentifier("_filterFunc");
                
                if (invocation.ArgumentList.Arguments.Count == 1)
                {
                    transformState.Prereq(new Block([
                        new Variable(filterFuncIdentifier, true, arguments.First()),
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
                        ? new BinaryOperator(arguments.First(), "+", arguments.ElementAt(1))
                        : new UnaryOperator("#", self);

                    var indexIdentifier = occupiedIdentifiersStack.AddIdentifier("_i");
                    transformState.Prereq(new Block([
                        new Variable(filterFuncIdentifier, true,
                            invocation.ArgumentList.Arguments.Count == 2
                                ? arguments.ElementAt(1)
                                : arguments.ElementAt(2)),
                        new NumericFor(indexIdentifier, arguments.First(), max, null,
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
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments;
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
                                        : arguments.First(),
                                "==",
                                value),
                            new Block([
                                new Assignment(expression, key),
                                new Break()
                            ]))
                    ]))
                ];

                if (shouldCreateVariable)
                    block.Insert(0, new Variable(new IdentifierName("_val"), true, arguments.First()));

                transformState.Prereq(new Block(block));

                expanded = expression;
                break;
            }
            case "IndexOfLast":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments;
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
                                    : arguments.First(), "==", value), new Block([
                                new Assignment(expression, key)
                            ]))
                    ]))
                ];

                if (shouldCreateVariable)
                    block.Insert(0, new Variable(new IdentifierName("_val"), true, arguments.First()));

                transformState.PrereqList(block);

                expanded = expression;
                break;
            }
            case "Insert":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments;
                expanded = AstUtility.TableCall("insert", self, arguments.First(), arguments.Last());
                break;
            }
            case "Remove":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments;
                expanded = AstUtility.TableCall(
                    "remove",
                    self,
                    AstUtility.TableCall("find", self, arguments.First()));
                
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

                transformState.Prereq(new Assignment(new ElementAccess(self, key), value));
                expanded = new NoOpExpression();
                break;
            }
            case "ContainsKey":
            {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                var self = (Expression)visit(memberAccess.Expression)!;
                var key = arguments.Arguments.First().Expression;

                expanded = new Parenthesized(new BinaryOperator(new ElementAccess(self, key), "~=", AstUtility.Nil));
                break;
            }
            case "Clear":
            {
                var self = (Expression)visit(memberAccess.Expression)!;
                expanded = AstUtility.TableCall("clear", self);
                
                break;
            }
        }

        expanded?.MarkExpanded(MacroKind.DictionaryMethod);
        return expanded != null;
    }
    
    private IdentifierName AddIdentifierWithSelfName(ExpressionSyntax self, string identifier) =>
        occupiedIdentifiersStack.AddIdentifier(self is NameSyntax name
            ? name + identifier
            : identifier);
}