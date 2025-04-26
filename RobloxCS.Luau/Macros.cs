using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Macros;

public enum MacroKind : byte
{
    NewInstance,
    GetService,
    ListConstruction,
    DictionaryConstruction,
    HashSetConstruction,
    IEnumerableType,
    ISetType,
    ObjectMethod,
    EnumerableMethod,
    ListMethod,
    HashSetMethod,
    DictionaryMethod,
    ListProperty,
    BitOperation,
    EventInvoke
}

public class MacroManager(
    SemanticModel semanticModel,
    FileCompilation file)
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
        var rightSymbol = semanticModel.GetSymbolInfo(assignment.Right).Symbol;
        if (leftSymbol is not IEventSymbol eventSymbol) return null;
        if (rightSymbol is not IMethodSymbol methodSymbol) return null;

        var symbolMetadata = SymbolMetadataManager.Get(eventSymbol);
        symbolMetadata.EventConnectionName ??= AstUtility.CreateSimpleName<IdentifierName>(assignment, "conn_" + eventSymbol.Name, true);

        var connectionName = symbolMetadata.EventConnectionName;
        switch (mappedOperator)
        {
            case "+=":
                var left = (Expression)visit(assignment.Left)!;
                var right = (Expression)visit(assignment.Right)!;
                var callback = !eventSymbol.IsStatic && right is Luau.MemberAccess or QualifiedName
                    ? AstUtility.TryWrapNonStaticMethod(methodSymbol, right, file.OccupiedIdentifiers)!
                    : right;

                return new Variable(connectionName,
                                    true,
                                    new Call(new MemberAccess(left, new IdentifierName("Connect"), ':'),
                                             AstUtility.CreateArgumentList([callback])));
            case "-=":
                return new Call(new MemberAccess(connectionName, new IdentifierName("Disconnect"), ':'),
                                ArgumentList.Empty);
        }

        return null;
    }

    public static Expression? BinaryExpression(Func<SyntaxNode, Node?> visit, BinaryExpressionSyntax binaryExpression)
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
    ///     Takes a C# generic name and expands the name into a macro'd type
    /// </summary>
    public Name? GenericName(GenericNameSyntax genericName, List<string> typeArguments)
    {
        var typeSymbol = semanticModel.GetTypeInfo(genericName).Type;
        if (StandardUtility.IsFromSystemNamespace(typeSymbol))
            switch (typeSymbol?.Name)
            {
                // lord i am sorry for my sins
                // returning IdentifierName because when visiting GenericNameSyntax (C#) it expects that a Name (luau) is returned
                case "List":
                case "IEnumerable":
                {
                    var expanded = new IdentifierName($"{{ {typeArguments.First()} }}");
                    expanded.MarkExpanded(MacroKind.IEnumerableType);

                    return expanded;
                }

                case "HashSet":
                case "ISet":
                {
                    var expanded = new IdentifierName($"{{ [{typeArguments.First()}]: boolean }}");
                    expanded.MarkExpanded(MacroKind.ISetType);

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
             && memberAccess is { Parent: InvocationExpressionSyntax invocation, Name: IdentifierNameSyntax { Identifier.Text: "Invoke" } name })
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
                }
             && expressionType?.ContainingNamespace.Name == "Roblox")
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
                        return expanded;
                }

                {
                    if (StandardUtility.DoesTypeInheritFrom(expressionType, "Instance")
                     && InstanceMethod(visit, memberAccess, invocation, out var expanded))
                        return expanded;
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

                    case "HashSet":
                    {
                        if (HashSetMethod(visit, memberAccess, invocation, out var expanded))
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
                        if (ListProperty(visit, memberAccess, out var expanded)) return expanded;

                        break;
                    }
                }
            }
        }

        return null;
    }

    private bool EnumerableMethod(Func<SyntaxNode, Node?> visit,
                                  MemberAccessExpressionSyntax memberAccess,
                                  InvocationExpressionSyntax invocation,
                                  out Expression? expanded)
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

                var foundIdentifier = file.OccupiedIdentifiers.AddIdentifier("_found");
                var defaultIdentifier = file.OccupiedIdentifiers.AddIdentifier("_default");
                var predicate = (Expression)visit(arguments.First().Expression)!;
                file.Prerequisites.Add(new Variable(foundIdentifier, true));
                file.Prerequisites.Add(new Variable(defaultIdentifier, true, predicate));

                file.OccupiedIdentifiers.Push();
                var valueIdentifier = file.OccupiedIdentifiers.AddIdentifier("v");
                file.Prerequisites.Add(new If(new BinaryOperator(new Call(new IdentifierName("typeof"),
                                                                          AstUtility.CreateArgumentList([defaultIdentifier])),
                                                                 "==",
                                                                 new Literal("\"function\"")),
                                              new Block([
                                                  new For([AstUtility.DiscardName, valueIdentifier],
                                                          self,
                                                          new Block([
                                                              new Block([
                                                                  new If(new UnaryOperator("not ",
                                                                                           new
                                                                                               Call(defaultIdentifier,
                                                                                                    AstUtility
                                                                                                        .CreateArgumentList([
                                                                                                            valueIdentifier
                                                                                                        ]))),
                                                                         new Block([new Continue()]))
                                                              ]),
                                                              new Assignment(foundIdentifier, valueIdentifier),
                                                              new Break()
                                                          ]))
                                              ]),
                                              new Block([
                                                  new Assignment(foundIdentifier,
                                                                 new BinaryOperator(new ElementAccess(self, one),
                                                                                    "or",
                                                                                    defaultIdentifier))
                                              ])));

                file.OccupiedIdentifiers.Pop();
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

                var foundIdentifier = file.OccupiedIdentifiers.AddIdentifier("_found");
                var defaultIdentifier = file.OccupiedIdentifiers.AddIdentifier("_default");
                var predicate = (Expression)visit(arguments.First().Expression)!;
                file.Prerequisites.Add(new Variable(foundIdentifier, true));
                file.Prerequisites.Add(new Variable(defaultIdentifier, true, predicate));

                file.OccupiedIdentifiers.Push();
                var valueIdentifier = file.OccupiedIdentifiers.AddIdentifier("v");
                file.Prerequisites.Add(new If(new BinaryOperator(new Call(new IdentifierName("typeof"),
                                                                          AstUtility.CreateArgumentList([defaultIdentifier])),
                                                                 "==",
                                                                 new Literal("\"function\"")),
                                              new Block([
                                                  new For([AstUtility.DiscardName, valueIdentifier],
                                                          self,
                                                          new Block([
                                                              new Block([
                                                                  new If(new UnaryOperator("not ",
                                                                                           new
                                                                                               Call(defaultIdentifier,
                                                                                                    AstUtility
                                                                                                        .CreateArgumentList([
                                                                                                            valueIdentifier
                                                                                                        ]))),
                                                                         new Block([new Continue()]))
                                                              ]),
                                                              new Assignment(foundIdentifier, valueIdentifier)
                                                          ]))
                                              ]),
                                              new Block([
                                                  new Assignment(foundIdentifier,
                                                                 new BinaryOperator(new ElementAccess(self,
                                                                                                      new
                                                                                                          UnaryOperator("#",
                                                                                                                        self)),
                                                                                    "or",
                                                                                    defaultIdentifier))
                                              ])));

                file.OccupiedIdentifiers.Pop();
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
                var appendedIdentifier = file.OccupiedIdentifiers.AddIdentifier("_appended");
                var element = (Expression)visit(arguments.First().Expression)!;

                file.Prerequisites.Add(new Variable(appendedIdentifier, true, AstUtility.TableCall("clone", self)));
                file.Prerequisites.Add(new ExpressionStatement(AstUtility.TableCall("insert", appendedIdentifier, element)));
                expanded = appendedIdentifier;

                break;
            }
            case "Prepend":
            {
                var prependedIdentifier = file.OccupiedIdentifiers.AddIdentifier("_prepended");
                var element = (Expression)visit(arguments.First().Expression)!;

                file.Prerequisites.Add(new Variable(prependedIdentifier, true, AstUtility.TableCall("clone", self)));
                file.Prerequisites.Add(new ExpressionStatement(AstUtility.TableCall("insert", prependedIdentifier, one, element)));
                expanded = prependedIdentifier;

                break;
            }
            case "Distinct":
            {
                var distinctIdentifier = file.OccupiedIdentifiers.AddIdentifier("_distinct");
                var seenIdentifier = file.OccupiedIdentifiers.AddIdentifier("_seen");
                var valueIdentifier = file.OccupiedIdentifiers.AddIdentifier("v");
                var seenValue = new ElementAccess(seenIdentifier, valueIdentifier);

                file.Prerequisites.Add(new Variable(distinctIdentifier, true, TableInitializer.Empty));
                file.Prerequisites.Add(new Variable(seenIdentifier, true, TableInitializer.Empty));
                file.Prerequisites.Add(new For([AstUtility.DiscardName, valueIdentifier],
                                               self,
                                               new Block([
                                                   new If(seenValue,
                                                          new Block([new Continue()])),
                                                   new Assignment(seenValue, AstUtility.True),
                                                   new ExpressionStatement(AstUtility.TableCall("insert",
                                                                                                distinctIdentifier,
                                                                                                valueIdentifier))
                                               ])));

                expanded = distinctIdentifier;

                break;
            }
            case "Concat":
            {
                var resultIdentifier = file.OccupiedIdentifiers.AddIdentifier("_result");
                var valueIdentifier = file.OccupiedIdentifiers.AddIdentifier("v");
                var other = (Expression)visit(arguments.First().Expression)!;

                file.Prerequisites.Add(new Variable(resultIdentifier, true, AstUtility.TableCall("clone", self)));
                file.Prerequisites.Add(new For([AstUtility.DiscardName, valueIdentifier],
                                               other,
                                               new Block([
                                                   new ExpressionStatement(AstUtility.TableCall("insert",
                                                                                                resultIdentifier,
                                                                                                valueIdentifier))
                                               ])));

                expanded = resultIdentifier;

                break;
            }
            case "Intersect":
            {
                var resultIdentifier = file.OccupiedIdentifiers.AddIdentifier("_result");
                var firstIdentifier = file.OccupiedIdentifiers.AddIdentifier("a");
                var secondIdentifier = file.OccupiedIdentifiers.AddIdentifier("b");
                var other = (Expression)visit(arguments.First().Expression)!;

                file.Prerequisites.Add(new Variable(resultIdentifier, true, TableInitializer.Empty));
                file.Prerequisites.Add(new For([AstUtility.DiscardName, firstIdentifier],
                                               self,
                                               new Block([
                                                   new For([AstUtility.DiscardName, secondIdentifier],
                                                           other,
                                                           new Block([
                                                               new If(new BinaryOperator(firstIdentifier,
                                                                                         "~=",
                                                                                         secondIdentifier),
                                                                      new Block([new Continue()])),
                                                               new ExpressionStatement(AstUtility
                                                                                           .TableCall("insert",
                                                                                                      resultIdentifier,
                                                                                                      firstIdentifier))
                                                           ]))
                                               ])));

                expanded = resultIdentifier;

                break;
            }
            case "GetEnumerator":
            {
                expanded = AstUtility.NewEnumerator(self);

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

        if (symbol is not INamedTypeSymbol { TypeParameters.Length: > 0 } namedTypeSymbol) return null;

        switch (namedTypeSymbol.Name)
        {
            case "List":
            {
                Expression finalExpression = new TableInitializer();
                if (baseObjectCreation.Initializer != null)
                {
                    finalExpression = new TableInitializer(baseObjectCreation.Initializer.Expressions
                                                                             .Select(visit)
                                                                             .OfType<Expression>()
                                                                             .ToList());
                }
                else if (baseObjectCreation.ArgumentList is { Arguments.Count: > 0 })
                {
                    var expression = baseObjectCreation.ArgumentList.Arguments.First().Expression;
                    var expressionType = semanticModel.GetTypeInfo(expression).Type;

                    if (expressionType != null && Constants.INTEGER_TYPES.Contains(expressionType.Name))
                        throw Logger.UnsupportedError(expression, "Fixed list capacities", useYet: false);

                    finalExpression = (Expression)visit(expression)!;
                }

                finalExpression.MarkExpanded(MacroKind.ListConstruction);

                return finalExpression;
            }
            case "HashSet":
            {
                Expression finalExpression = new TableInitializer();
                if (baseObjectCreation.Initializer != null)
                {
                    var elements = baseObjectCreation.Initializer.Expressions
                                                     .Select(visit)
                                                     .OfType<Expression>()
                                                     .ToList();

                    var initializers = elements.ConvertAll(_ => AstUtility.Bool(true)).ToList<Expression>();
                    finalExpression = new TableInitializer(initializers, elements);
                }
                else if (baseObjectCreation.ArgumentList is { Arguments.Count: > 0 })
                {
                    throw Logger.UnsupportedError(baseObjectCreation.ArgumentList, "Hash set constructor arguments");

                    var expression = baseObjectCreation.ArgumentList.Arguments.First().Expression;
                    var expressionType = semanticModel.GetTypeInfo(expression).Type;
                    if (expressionType != null && Constants.INTEGER_TYPES.Contains(expressionType.Name))
                        throw Logger.UnsupportedError(expression, "Fixed hash set capacities", useYet: false);

                    finalExpression = (Expression)visit(expression)!;
                }

                finalExpression.MarkExpanded(MacroKind.HashSetConstruction);

                return finalExpression;
            }
            case "Dictionary":
            {
                Expression finalExpression = new TableInitializer();
                if (baseObjectCreation.Initializer != null)
                {
                    List<Expression> values = [];
                    List<Expression> keys = [];

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

                            default:
                                throw Logger.CompilerError($"Unsupported dictionary initializer expression '{expression.Kind()}'",
                                                           expression);
                        }

                    finalExpression = new TableInitializer(values, keys);
                }
                else if (baseObjectCreation.ArgumentList is { Arguments.Count: > 0 })
                {
                    var arguments = baseObjectCreation.ArgumentList.Arguments.Select(arg => arg.Expression).ToList();
                    if (arguments.Any(arg =>
                        {
                            var type = semanticModel.GetTypeInfo(arg).Type;

                            return type is { Name: "EqualityComparer" or "IEqualityComparer" };
                        }))
                        throw Logger.UnsupportedError(baseObjectCreation.ArgumentList, "Equality comparers");

                    throw Logger.UnsupportedError(baseObjectCreation.ArgumentList, "Dictionary constructor arguments");
                }

                finalExpression.MarkExpanded(MacroKind.DictionaryConstruction);

                return finalExpression;
            }
        }

        return null;
    }

    /// <summary>Macros <see cref="Instance" /> methods</summary>
    private static bool InstanceMethod(Func<SyntaxNode, Node?> visit,
                                       MemberAccessExpressionSyntax memberAccess,
                                       InvocationExpressionSyntax invocation,
                                       out Expression? expanded)
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
                expanded = new Call(new MemberAccess(self, new IdentifierName(memberAccess.Name.Identifier.Text), ':'),
                                    new ArgumentList([
                                        new Argument(new
                                                         Literal($"\"{genericName.TypeArgumentList.Arguments.First().ToString()}\""))
                                    ]));

                break;
            }
        }

        expanded?.MarkExpanded(MacroKind.ObjectMethod);

        return expanded != null;
    }

    /// <summary>Macros <see cref="Object" /> methods</summary>
    private bool ObjectMethod(Func<SyntaxNode, Node?> visit,
                              MemberAccessExpressionSyntax memberAccess,
                              InvocationExpressionSyntax invocation,
                              out Expression? expanded)
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

    /// <summary>Macros <see cref="HashSet" /> methods</summary>
    private bool HashSetMethod(Func<SyntaxNode, Node?> visit,
                               MemberAccessExpressionSyntax memberAccess,
                               InvocationExpressionSyntax invocation,
                               out Node? expanded)
    {
        expanded = null;
        var listExpression = (Expression)visit(memberAccess.Expression)!;
        Expression self;

        if (listExpression is not IdentifierName name)
        {
            self = file.OccupiedIdentifiers.AddIdentifier("_exp");
            file.Prerequisites.Add(new Variable((IdentifierName)self, true, listExpression));
        }
        else
        {
            self = name;
        }

        switch (memberAccess.Name.Identifier.Text)
        {
            case "Add":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var element = arguments.First();
                var wasAddedIdentifier = file.OccupiedIdentifiers.AddIdentifier("_wasAdded");
                var selfAtElement = new ElementAccess(self, element);

                file.Prerequisites.Add(new Variable(wasAddedIdentifier, true, new BinaryOperator(selfAtElement, "==", AstUtility.Nil)));
                file.Prerequisites.Add(new Assignment(selfAtElement, AstUtility.True));
                expanded = wasAddedIdentifier;

                break;
            }
            case "Remove":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var element = arguments.First();
                var wasRemovedIdentifier = file.OccupiedIdentifiers.AddIdentifier("_wasRemoved");
                var selfAtElement = new ElementAccess(self, element);

                file.Prerequisites.Add(new Variable(wasRemovedIdentifier, true, new BinaryOperator(selfAtElement, "~=", AstUtility.Nil)));
                file.Prerequisites.Add(new Assignment(selfAtElement, new TypeCast(AstUtility.Nil, AstUtility.AnyType)));
                expanded = wasRemovedIdentifier;

                break;
            }
            case "Clear":
            {
                expanded = AstUtility.TableCall("clear", self);
                break;
            }
            case "Contains":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var element = arguments.First();
                expanded = new ElementAccess(self, element);

                break;
            }
        }

        expanded?.MarkExpanded(MacroKind.HashSetMethod);
        return expanded != null;
    }

    /// <summary>Macros <see cref="List" /> methods</summary>
    private bool ListMethod(Func<SyntaxNode, Node?> visit,
                            MemberAccessExpressionSyntax memberAccess,
                            InvocationExpressionSyntax invocation,
                            out Node? expanded)
    {
        expanded = null;
        var listExpression = (Expression)visit(memberAccess.Expression)!;
        Expression self;

        if (listExpression is not IdentifierName name)
        {
            self = file.OccupiedIdentifiers.AddIdentifier("_exp");
            file.Prerequisites.Add(new Variable((IdentifierName)self, true, listExpression));
        }
        else
        {
            self = name;
        }

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
                expanded = new BinaryOperator(AstUtility.TableCall("find", [self, ..arguments]),
                                              "~=",
                                              AstUtility.Nil);

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
                var expression = file.OccupiedIdentifiers.AddIdentifier("_newValue");
                var key = file.OccupiedIdentifiers.AddIdentifier("_k");
                var value = file.OccupiedIdentifiers.AddIdentifier("_v");
                var filterFuncIdentifier = file.OccupiedIdentifiers.AddIdentifier("_filterFunc");

                file.Prerequisites.Add(new Block([
                    new Variable(filterFuncIdentifier, true, arguments.First()),
                    new Variable(expression, true, AstUtility.False),
                    new For([key, value],
                            self,
                            new Block([
                                new If(new Call(filterFuncIdentifier,
                                                new ArgumentList([new Argument(value)])),
                                       new Block([new Assignment(expression, AstUtility.True), new Break()]))
                            ]))
                ]));

                expanded = expression;

                break;
            }
            case "Find":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var expression = file.OccupiedIdentifiers.AddIdentifier("_newValue");
                var key = file.OccupiedIdentifiers.AddIdentifier("_k");
                var value = file.OccupiedIdentifiers.AddIdentifier("_v");
                var filterFuncIdentifier = file.OccupiedIdentifiers.AddIdentifier("_filterFunc");

                file.Prerequisites.Add(new Block([
                    new Variable(expression, true, AstUtility.Nil),
                    new Variable(filterFuncIdentifier, true, arguments.First()),
                    new For([key, value],
                            self,
                            new Block([
                                new If(new Call(filterFuncIdentifier,
                                                new ArgumentList([new Argument(value)])),
                                       new Block([new Assignment(expression, value), new Break()]))
                            ]))
                ]));

                expanded = expression;

                break;
            }
            case "FindLast":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var expression = file.OccupiedIdentifiers.AddIdentifier("_newValue");
                var key = file.OccupiedIdentifiers.AddIdentifier("_k");
                var value = file.OccupiedIdentifiers.AddIdentifier("_v");
                var filterFuncIdentifier = file.OccupiedIdentifiers.AddIdentifier("_filterFunc");

                file.Prerequisites.Add(new Block([
                    new Variable(filterFuncIdentifier, true, arguments.First()),
                    new Variable(expression, true),
                    new For([key, value],
                            self,
                            new Block([
                                new If(new Call(filterFuncIdentifier,
                                                new ArgumentList([new Argument(value)])),
                                       new Block([new Assignment(expression, value)]))
                            ]))
                ]));

                expanded = expression;

                break;
            }
            case "FindAll":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var expression = file.OccupiedIdentifiers.AddIdentifier("_newValue");
                var key = file.OccupiedIdentifiers.AddIdentifier("_k");
                var value = file.OccupiedIdentifiers.AddIdentifier("_v");
                var filterFuncIdentifier = file.OccupiedIdentifiers.AddIdentifier("_filterFunc");

                file.Prerequisites.Add(new Block([
                    new Variable(filterFuncIdentifier, true, arguments.First()),
                    new Variable(expression, true, new TableInitializer()),
                    new For([key, value],
                            self,
                            new Block([
                                new If(new Call(filterFuncIdentifier,
                                                new ArgumentList([new Argument(value)])),
                                       new Block([
                                           new ExpressionStatement(AstUtility
                                                                       .TableCall("insert",
                                                                                  expression,
                                                                                  value))
                                       ]))
                            ]))
                ]));

                expanded = expression;

                break;
            }
            case "AddRange":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var key = file.OccupiedIdentifiers.AddIdentifier("_k");
                var value = file.OccupiedIdentifiers.AddIdentifier("_v");

                file.Prerequisites.Add(new For([key, value],
                                               arguments.First(),
                                               new Block([new ExpressionStatement(AstUtility.TableCall("insert", self, value))])));

                expanded = new NoOpExpression();

                break;
            }
            case "ForEach":
            {
                var args = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var callbackIdentifier = file.OccupiedIdentifiers.AddIdentifier("_callback");
                var valueName = file.OccupiedIdentifiers.AddIdentifier("_v");

                file.Prerequisites.Add(new Variable(callbackIdentifier, true, args.First()));
                file.Prerequisites.Add(new For([AstUtility.DiscardName, valueName],
                                               self,
                                               new Block([
                                                   new ExpressionStatement(new Call(callbackIdentifier,
                                                                                    AstUtility.CreateArgumentList([valueName])))
                                               ])));

                expanded = new NoOpExpression();

                break;
            }
            case "ConvertAll":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression);
                var expression = file.OccupiedIdentifiers.AddIdentifier("_newValue");
                var key = file.OccupiedIdentifiers.AddIdentifier("_k");
                var value = file.OccupiedIdentifiers.AddIdentifier("_v");
                var convertFuncIdentifier = file.OccupiedIdentifiers.AddIdentifier("_convertFunc");

                file.Prerequisites.Add(new Block([
                    new Variable(convertFuncIdentifier, true, arguments.First()),
                    new Variable(expression,
                                 true,
                                 AstUtility.TableCall("create",
                                                      new ArgumentList([
                                                          new Argument(new UnaryOperator("#",
                                                                                         self))
                                                      ]))),
                    new For([key, value],
                            self,
                            new Block([
                                new ExpressionStatement(AstUtility.TableCall("insert",
                                                                             expression,
                                                                             new
                                                                                 Call(convertFuncIdentifier,
                                                                                      new
                                                                                          ArgumentList([
                                                                                              new
                                                                                                  Argument(value)
                                                                                          ]))))
                            ]))
                ]));

                expanded = expression;

                break;
            }
            case "FindIndex":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                var expression = file.OccupiedIdentifiers.AddIdentifier("_newValue");
                var key = file.OccupiedIdentifiers.AddIdentifier("_k");
                var value = file.OccupiedIdentifiers.AddIdentifier("_v");
                var filterFuncIdentifier = file.OccupiedIdentifiers.AddIdentifier("_filterFunc");

                if (invocation.ArgumentList.Arguments.Count == 1)
                {
                    file.Prerequisites.Add(new Block([
                        new Variable(filterFuncIdentifier, true, arguments.First()),
                        new Variable(expression, true),
                        new For([key, value],
                                self,
                                new Block([
                                    new If(new Call(filterFuncIdentifier,
                                                    new ArgumentList([new Argument(value)])),
                                           new Block([new Assignment(expression, key), new Break()]))
                                ]))
                    ]));
                }
                else if (invocation.ArgumentList.Arguments.Count >= 2)
                {
                    Expression max = invocation.ArgumentList.Arguments.Count == 3
                        ? new BinaryOperator(arguments.First(), "+", arguments.ElementAt(1))
                        : new UnaryOperator("#", self);

                    var indexIdentifier = file.OccupiedIdentifiers.AddIdentifier("_i");
                    file.Prerequisites.Add(new Block([
                        new Variable(filterFuncIdentifier,
                                     true,
                                     invocation.ArgumentList.Arguments.Count == 2
                                         ? arguments.ElementAt(1)
                                         : arguments.ElementAt(2)),
                        new NumericFor(indexIdentifier,
                                       arguments.First(),
                                       max,
                                       null,
                                       new Block([
                                           new Variable(value,
                                                        true,
                                                        new ElementAccess(self,
                                                                          indexIdentifier)),
                                           new If(new Call(filterFuncIdentifier,
                                                           new ArgumentList([new Argument(value)])),
                                                  new Block([new Assignment(expression, key), new Break()]))
                                       ]))
                    ]));
                }

                expanded = expression;

                break;
            }
            case "FindLastIndex":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                var expression = file.OccupiedIdentifiers.AddIdentifier("_newValue");
                var key = file.OccupiedIdentifiers.AddIdentifier("_k");
                var value = file.OccupiedIdentifiers.AddIdentifier("_v");
                var filterFuncIdentifier = file.OccupiedIdentifiers.AddIdentifier("_filterFunc");

                if (invocation.ArgumentList.Arguments.Count == 1)
                {
                    file.Prerequisites.Add(new Block([
                        new Variable(filterFuncIdentifier, true, arguments.First()),
                        new Variable(expression, true),
                        new For([key, value],
                                self,
                                new Block([
                                    new If(new Call(filterFuncIdentifier,
                                                    new ArgumentList([new Argument(value)])),
                                           new Block([new Assignment(expression, key)]))
                                ]))
                    ]));
                }
                else if (invocation.ArgumentList.Arguments.Count >= 2)
                {
                    Expression max = invocation.ArgumentList.Arguments.Count == 3
                        ? new BinaryOperator(arguments.First(), "+", arguments.ElementAt(1))
                        : new UnaryOperator("#", self);

                    var indexIdentifier = file.OccupiedIdentifiers.AddIdentifier("_i");
                    file.Prerequisites.Add(new Block([
                        new Variable(filterFuncIdentifier,
                                     true,
                                     invocation.ArgumentList.Arguments.Count == 2
                                         ? arguments.ElementAt(1)
                                         : arguments.ElementAt(2)),
                        new NumericFor(indexIdentifier,
                                       arguments.First(),
                                       max,
                                       null,
                                       new Block([
                                           new Variable(value,
                                                        true,
                                                        new ElementAccess(self,
                                                                          indexIdentifier)),
                                           new If(new Call(filterFuncIdentifier,
                                                           new ArgumentList([new Argument(value)])),
                                                  new Block([new Assignment(expression, key)]))
                                       ]))
                    ]));
                }

                expanded = expression;

                break;
            }
            case "IndexOf":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                var init = arguments.ElementAtOrDefault(1);

                if (arguments.Count == 3)
                    throw Logger.UnsupportedError(invocation.ArgumentList.Arguments.Last(),
                                                  "IndexOf() count parameter",
                                                  useYet: false);

                expanded = AstUtility.SubtractOne(new Parenthesized(new BinaryOperator(AstUtility.TableCall("find",
                                                                                                            self,
                                                                                                            arguments.First(),
                                                                                                            init != null
                                                                                                                ? AstUtility
                                                                                                                    .AddOne(init)
                                                                                                                : AstUtility.Nil),
                                                                                       "or",
                                                                                       new Literal("0"))));

                break;
            }
            case "LastIndexOf":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                var expressionName = file.OccupiedIdentifiers.AddIdentifier("_newValue");
                var indexName = file.OccupiedIdentifiers.AddIdentifier("_i");
                var shouldCreateVariable = invocation.ArgumentList.Arguments.First().Expression is not LiteralExpressionSyntax;

                var init = arguments.ElementAtOrDefault(1);
                var value = shouldCreateVariable
                    ? new IdentifierName("_val")
                    : arguments.First();

                var negativeOne = new UnaryOperator("-", new Literal("1"));
                List<Statement> block =
                [
                    new Variable(expressionName, true),
                    new NumericFor(indexName,
                                   init ?? new UnaryOperator("#", self),
                                   new Literal("1"),
                                   negativeOne,
                                   new Block([
                                       new If(new BinaryOperator(new ElementAccess(self, indexName), "==", value),
                                              new Block([new Return(AstUtility.SubtractOne(indexName))])),
                                       new Return(negativeOne)
                                   ]))
                ];

                if (shouldCreateVariable) block.Insert(0, new Variable(new IdentifierName("_val"), true, arguments.First()));

                file.Prerequisites.AddList(block);

                expanded = expressionName;

                break;
            }
            case "Insert":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                ;
                expanded = AstUtility.TableCall("insert", self, AstUtility.AddOne(arguments.First()), arguments.Last());

                break;
            }
            case "Remove":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                ;
                expanded = AstUtility.TableCall("remove",
                                                self,
                                                AstUtility.TableCall("find", self, arguments.First()));

                break;
            }
            case "RemoveAt":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                ;
                expanded = AstUtility.TableCall("remove",
                                                self,
                                                AstUtility.AddOne(arguments.First()));

                break;
            }
        }

        expanded?.MarkExpanded(MacroKind.ListMethod);

        return expanded != null;
    }

    /// <summary>Macros <see cref="Dictionary" /> methods</summary>
    private bool DictionaryMethod(Func<SyntaxNode, Node?> visit,
                                  MemberAccessExpressionSyntax memberAccess,
                                  InvocationExpressionSyntax invocation,
                                  out Expression? expanded)
    {
        expanded = null;

        var self = (Expression)visit(memberAccess.Expression)!;
        switch (memberAccess.Name.Identifier.Text)
        {
            case "Add":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                ;
                var key = arguments.First();
                var value = arguments.Last();

                file.Prerequisites.Add(new Assignment(new ElementAccess(self, key), value));
                expanded = new NoOpExpression();

                break;
            }
            case "ContainsKey":
            {
                var arguments = ((ArgumentList)visit(invocation.ArgumentList)!).Arguments.Select(arg => arg.Expression).ToList();
                ;
                var key = arguments.First();

                expanded = new Parenthesized(new BinaryOperator(new ElementAccess(self, key), "~=", AstUtility.Nil));

                break;
            }
            case "Clear":
            {
                expanded = AstUtility.TableCall("clear", self);

                break;
            }
        }

        expanded?.MarkExpanded(MacroKind.DictionaryMethod);

        return expanded != null;
    }

    private IdentifierName AddIdentifierWithSelfName(ExpressionSyntax self, string identifier) =>
        file.OccupiedIdentifiers.AddIdentifier(self is NameSyntax name
                                                   ? name + identifier
                                                   : identifier);
}