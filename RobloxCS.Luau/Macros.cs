using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;
using System.Xml.Linq;

namespace RobloxCS.Macros;

public enum MacroKind
{
    NewInstance,
    ListConstruction,
    DictionaryConstruction,
    IEnumerableType,
    DictionaryType,
    ObjectMethod,
    IEnumerableMethod,
    ListMethod,
    DictionaryMethod,
    BitOperation
}

public class Macro(SemanticModel semanticModel)
{
    private SemanticModel _semanticModel { get; } = semanticModel;

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

        var leftSymbol = _semanticModel.GetSymbolInfo(assignment.Left).Symbol;
        if (leftSymbol is IEventSymbol eventSymbol)
        {
            var symbolMetadata = SymbolMetadataManager.Get(eventSymbol);
            symbolMetadata.EventConnectionName ??= AstUtility.CreateSimpleName<IdentifierName>(assignment, "conn_" + eventSymbol.Name, registerIdentifier: true);
            
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
        var typeInfo = _semanticModel.GetTypeInfo(genericName);
        if (StandardUtility.IsFromSystemNamespace(typeInfo.Type))
        {
            switch (genericName.Identifier.Text)
            {
                // lord i am sorry for my sins
                // returning IdentifierName because when visiting GenericNameSyntax (C#) it expects that a Name (luau) is returned
                
                case "List":
                case "IEnumerable":
                {
                    var elementTypeName = visit(genericName.TypeArgumentList.Arguments.First())!;
                    var expanded = new IdentifierName($"{{ {StandardUtility.GetMappedType(elementTypeName.ToString()!)} }}");
                    expanded.MarkExpanded(MacroKind.IEnumerableType);
                    
                    return expanded;
                }
                
                case "Dictionary":
                {
                    var keyTypeName = visit(genericName.TypeArgumentList.Arguments.First())!;
                    var valueTypeName = visit(genericName.TypeArgumentList.Arguments.Last())!;
                    var expanded = new IdentifierName($"{{ [{StandardUtility.GetMappedType(keyTypeName.ToString()!)}]: {StandardUtility.GetMappedType(valueTypeName.ToString()!)} }}");
                    expanded.MarkExpanded(MacroKind.DictionaryType);
                    
                    return expanded;
                }
            }
        }

        return null;
    }
    
    /// <summary>Takes a C# member access and expands the macro into a Luau expression</summary>
    /// <returns>The expanded expression of the macro, or null if no macro was applied</returns>
    public Node? MemberAccess(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess)
    {
        var expressionType = _semanticModel.GetTypeInfo(memberAccess.Expression).Type;
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
                    if (StandardUtility.DoesTypeInheritFrom(expressionType, "Object") &&
                        ObjectMethod(visit, memberAccess, out var expanded))
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
        }

        return null;
    }

    /// <summary>Takes a C# object creation and expands the macro into a Luau expression</summary>
    /// <returns>The expanded expression of the macro, or null if no macro was applied</returns>
    public Expression? ObjectCreation(Func<SyntaxNode, Node?> visit, BaseObjectCreationExpressionSyntax baseObjectCreation) {
        // generic objects
        var symbol = baseObjectCreation is ObjectCreationExpressionSyntax objectCreation
            ? _semanticModel.GetSymbolInfo(objectCreation.Type).Symbol
            : _semanticModel.GetSymbolInfo(baseObjectCreation).Symbol?.ContainingSymbol;
        
        if (symbol is not INamedTypeSymbol { TypeParameters.Length: > 0 } namedTypeSymbol)
            return null;

        switch (namedTypeSymbol.Name) {
            case "List":
            {
                var expressions = baseObjectCreation.Initializer?.Expressions.Select(expression => (Expression)visit(expression)!).ToList();
                var table = new TableInitializer(expressions ?? []);
                table.MarkExpanded(MacroKind.ListConstruction);
                    
                return table;
            }
                
            case "Dictionary": {
                var values = new List<Expression>();
                var keys = new List<Expression>();

                if (baseObjectCreation.Initializer != null) {
                    foreach (var expression in baseObjectCreation.Initializer.Expressions)
                    {
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
                    }
                }

                var table = new TableInitializer(values, keys);
                table.MarkExpanded(MacroKind.DictionaryConstruction);

                return table;
            }
        }

        return null;
    }
    
    /// <summary>Macros <see cref="Object"/> methods</summary>
    private bool ObjectMethod(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess, out Expression? expanded)
    {
        expanded = null;
        switch (memberAccess.Name.Identifier.Text) {
            case "GetType":
                throw Logger.UnsupportedError(memberAccess.Name, "Object.GetType()", useIs: true, useYet: false);
        }
        
        expanded?.MarkExpanded(MacroKind.ObjectMethod);
        return expanded != null;
    }
    
    // TODO: Replace (function() end)() calls
    /// <summary>Macros <see cref="List"/> methods</summary>
    private static bool ListMethod(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess,
        InvocationExpressionSyntax invocation, out Node? expanded)
    {
        expanded = null;
        switch (memberAccess.Name.Identifier.Text) {
            case "Add": {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                var self = (Expression)visit(memberAccess.Expression)!;
                arguments.Arguments.Insert(0, new Argument(self));
                                
                expanded = new Call(new QualifiedName(new IdentifierName("table"), new IdentifierName("insert")), arguments);
                break;
            }
            case "Contains": {
                    var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                    var self = (Expression)visit(memberAccess.Expression)!;
                    arguments.Arguments.Insert(0, new Argument(self));

                    expanded = new BinaryOperator(new Call(new QualifiedName(new IdentifierName("table"), new IdentifierName("find")), arguments), "~=", AstUtility.Nil());
                    break;
                }
            case "Clear": {
                    var self = (Expression)visit(memberAccess.Expression)!;

                    expanded = new Call(new QualifiedName(new IdentifierName("table"), new IdentifierName("clear")), new ArgumentList([new Argument(self)]));
                    break;
                }
            case "Exists": {
                    var self = (Expression)visit(memberAccess.Expression)!;
                    var FilterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                    var key = new IdentifierName("_");
                    var value = new IdentifierName("_v");

                    expanded = new Call(new Parenthesized(new AnonymousFunction(new([]), body: new Block([
                            new Variable(new IdentifierName("_FilterFunc"), true, FilterFunc.Arguments.First()),
                            new For([key, value], self, new Block([
                                    new If(new Call(new IdentifierName("_FilterFunc"), new ArgumentList([new Argument(value)])), new Block([
                                            new Return(AstUtility.True())
                                        ])),
                                ])),
                            new Return(AstUtility.False())
                        ]))), new([]));
                    break;
                }
            case "Find": {
                    var self = (Expression)visit(memberAccess.Expression)!;
                    var filterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                    var key = new IdentifierName("_");
                    var value = new IdentifierName("_v");

                    expanded = new Call(new Parenthesized(new AnonymousFunction(new([]), body: new Block([
                            new Variable(new IdentifierName("_FilterFunc"), true, filterFunc.Arguments.First()),
                            new For([key, value], self, new Block([
                                    new If(new Call(new IdentifierName("_FilterFunc"), new ArgumentList([new Argument(value)])), new Block([
                                            new Return(value)
                                        ])),
                                ])),
                            new Return(AstUtility.Nil())
                        ]))), new([]));
                    break;
                }
            case "FindLast": {
                    var self = (Expression)visit(memberAccess.Expression)!;
                    var filterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                    var key = new IdentifierName("_");
                    var value = new IdentifierName("_v");

                    expanded = new Call(new Parenthesized(new AnonymousFunction(new([]), body: new Block([
                            new Variable(new IdentifierName("_FilterFunc"), true, filterFunc.Arguments.First()),
                            new Variable(new IdentifierName("_Return"), true),
                            new For([key, value], self, new Block([
                                    new If(new Call(new IdentifierName("_FilterFunc"), new ArgumentList([new Argument(value)])), new Block([
                                            new ExpressionStatement(new Assignment(new IdentifierName("_Return"), value))
                                        ]))
                                ])),
                            new Return(new IdentifierName("_Return"))
                        ]))), new([]));
                    break;
                }
            case "FindAll": {
                    var self = (Expression)visit(memberAccess.Expression)!;
                    var filterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                    var key = new IdentifierName("_");
                    var value = new IdentifierName("_v");

                    expanded = new Call(new Parenthesized(new AnonymousFunction(new([]), body: new Block([
                            new Variable(new IdentifierName("_FilterFunc"), true, filterFunc.Arguments.First()),
                            new Variable(new IdentifierName("_Filtered"), true, new TableInitializer()),
                            new For([key, value], self, new Block([
                                    new If(new Call(new IdentifierName("_FilterFunc"), new ArgumentList([new Argument(value)])), new Block([
                                            new ExpressionStatement(new Call(new MemberAccess(new IdentifierName("table"), new IdentifierName("insert")), new ArgumentList([new Argument(new IdentifierName("_Filtered")), new Argument(value)])))
                                        ])),
                                ])),
                            new Return(new IdentifierName("_Filtered"))
                        ]))), new([]));
                    break;
                }
            case "AddRange": {
                    var self = (Expression)visit(memberAccess.Expression)!;
                    var table = (ArgumentList)visit(invocation.ArgumentList)!;
                    var key = new IdentifierName("_");
                    var value = new IdentifierName("_v");

                    expanded = new Block([
                            new For([key, value], table.Arguments.First(), new Block([
                                    new ExpressionStatement(new Call(new MemberAccess(new IdentifierName("table"), new IdentifierName("insert")), new ArgumentList([new Argument(self), new Argument(value)])))
                                ])),
                        ]);
                    break;
                }
            case "ConvertAll": {
                    var self = (Expression)visit(memberAccess.Expression)!;
                    var convertFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                    var key = new IdentifierName("_");
                    var value = new IdentifierName("_v");

                    expanded = new Call(new Parenthesized(new AnonymousFunction(new([]), body: new Block([
                            new Variable(new IdentifierName("_ConvertFunc"), true, convertFunc.Arguments.First()),
                            new Variable(new IdentifierName("_Converted"), true, new Call(new MemberAccess(new IdentifierName("table"), new IdentifierName("insert")), new ArgumentList([new (new UnaryOperator("#", self))]))),
                            new For([key, value], self, new Block([
                                    new ExpressionStatement(new Call(new MemberAccess(new IdentifierName("table"), new IdentifierName("insert")), new ArgumentList([new Argument(new IdentifierName("_Converted")), new Argument(new Call(new IdentifierName("_ConvertFunc"), new([new(value)])))])))
                                ])),
                            new Return(new IdentifierName("_Converted"))
                        ]))), new([]));
                    break;
                }
            case "FindIndex": {
                    var self = (Expression)visit(memberAccess.Expression)!;
                    var filterFunc = (ArgumentList)visit(invocation.ArgumentList)!;
                    var key = new IdentifierName("_k");
                    var value = new IdentifierName("_v");

                    // TODO: Add the other 2 overloads
                    if (invocation.ArgumentList.Arguments.Count == 1)
                        expanded = new Call(new Parenthesized(new AnonymousFunction(new([]), body: new Block([
                                new Variable(new IdentifierName("_FilterFunc"), true, filterFunc.Arguments.First()),
                                new For([key, value], self, new Block([
                                        new If(new Call(new IdentifierName("_FilterFunc"), new ArgumentList([new Argument(value)])), new Block([
                                                new Return(key)
                                            ]))
                                    ])),
                                new Return(AstUtility.Nil())
                            ]))), new([]));
                    break;
                }
            case "IndexOf": {
                    var self = (Expression)visit(memberAccess.Expression)!;
                    var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                    var key = new IdentifierName("_k");
                    var value = new IdentifierName("_v");
                    var shouldCreateVariable = invocation.ArgumentList.Arguments.First().Expression is not LiteralExpressionSyntax;

                    List <Statement> block = [
                            new For([key, value], self, new Block([
                                    new If(new BinaryOperator(shouldCreateVariable ? new IdentifierName("_val") : arguments.Arguments.First().Expression, "==", value), new Block([
                                            new Return(key)
                                        ])),
                                ])),
                            new Return(AstUtility.Nil())
                        ];

                    if (shouldCreateVariable)
                        block.Insert(0, new Variable(new("_val"), true, arguments.Arguments.First()));

                    expanded = new Call(new Parenthesized(new AnonymousFunction(new([]), body: new Block(block))), new([]));
                    break;
                }
            case "Insert": {
                    var self = (Expression)visit(memberAccess.Expression)!;
                    var arguments = (ArgumentList)visit(invocation.ArgumentList)!;

                    expanded = new Call(new MemberAccess(new IdentifierName("table"), new IdentifierName("insert")), new ArgumentList([new(self), arguments.Arguments.First(), arguments.Arguments.Last()]));
                    break;
                }
            case "Remove": {
                    var self = (Expression)visit(memberAccess.Expression)!;
                    var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                    var key = new IdentifierName("_k");
                    var value = new IdentifierName("_v");
                    var shouldCreateVariable = invocation.ArgumentList.Arguments.First().Expression is not LiteralExpressionSyntax;

                    List<Statement> block = [
                            new For([key, value], self, new Block([
                                    new If(new BinaryOperator(shouldCreateVariable ? new IdentifierName("_val") : arguments.Arguments.First().Expression, "==", value), new Block([
                                            new ExpressionStatement(new Call(new MemberAccess(new IdentifierName("table"), new IdentifierName("remove")), new ArgumentList([new(self), new(key)]))),
                                            new Break()
                                        ])),
                                ])),
                            new Return(AstUtility.Nil())
                        ];

                    if (shouldCreateVariable)
                        block.Insert(0, new Variable(new("_val"), true, arguments.Arguments.First()));

                    expanded = new Block(block);
                    break;
                }
        }

        expanded?.MarkExpanded(MacroKind.ListMethod);
        return expanded != null;
    }

    /// <summary>Macros <see cref="Dictionary"/> methods</summary>
    private static bool DictionaryMethod(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess,
        InvocationExpressionSyntax invocation, out Expression? expanded)
    {
        expanded = null;
        switch (memberAccess.Name.Identifier.Text) {
            case "Add": {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                var self = (Expression)visit(memberAccess.Expression)!;
                var key = arguments.Arguments.First().Expression;
                var value = arguments.Arguments.Last().Expression;
                                
                expanded = new Assignment(new ElementAccess(self, key), value);
                break;
            }
            case "ContainsKey": {
                var arguments = (ArgumentList)visit(invocation.ArgumentList)!;
                var self = (Expression)visit(memberAccess.Expression)!;
                var key = arguments.Arguments.First().Expression;

                expanded = new BinaryOperator(new ElementAccess(self, key), "~=", AstUtility.Nil());
                break;
            }
            case "Clear": {
                    var self = (Expression)visit(memberAccess.Expression)!;

                    expanded = new Call(new QualifiedName(new IdentifierName("table"), new IdentifierName("clear")), new ArgumentList([new Argument(self)]));
                    break;
                }
        }

        expanded?.MarkExpanded(MacroKind.DictionaryMethod);
        return expanded != null;
    }
}