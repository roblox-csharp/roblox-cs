using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;

namespace RobloxCS.Macros;

public enum MacroKind
{
    NewInstance,
    ListConstruction,
    DictionaryConstruction,
    IEnumerableType,
    DictionaryType
}

public class Macro(SemanticModel semanticModel)
{
    private SemanticModel _semanticModel { get; } = semanticModel;
    
    public Name? GenericName(Func<SyntaxNode, Node?> visit, GenericNameSyntax genericName)
    {
        var typeInfo = _semanticModel.GetTypeInfo(genericName);
        if (Utility.IsFromSystemNamespace(typeInfo.Type))
        {
            switch (genericName.Identifier.Text)
            {
                // lord i am sorry for my sins
                // returning IdentifierName because when visiting GenericNameSyntax (C#) it expects a Name (luau)
                
                case "List":
                case "IEnumerable":
                {
                    var elementTypeName = (IdentifierName)visit(genericName.TypeArgumentList.Arguments.First())!;
                    var expanded = new IdentifierName($"{{ {Utility.GetMappedType(elementTypeName.Text)} }}");
                    expanded.MarkExpanded(MacroKind.IEnumerableType);
                    
                    return expanded;
                }
                
                case "Dictionary":
                {
                    var keyTypeName = (IdentifierName)visit(genericName.TypeArgumentList.Arguments.First())!;
                    var valueTypeName = (IdentifierName)visit(genericName.TypeArgumentList.Arguments.Last())!;
                    var expanded = new IdentifierName($"{{ [{Utility.GetMappedType(keyTypeName.Text)}]: {Utility.GetMappedType(valueTypeName.Text)} }}");
                    expanded.MarkExpanded(MacroKind.DictionaryType);
                    
                    return expanded;
                }
            }
        }

        return null;
    }
    
    /// <summary>Takes a C# member access and expands the macro into a Luau expression</summary>
    /// <returns>The expanded expression of the macro, or null if no macro was applied</returns>
    public Expression? MemberAccess(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess)
    {
        if (memberAccess is
            {
                Parent: InvocationExpressionSyntax invocation,
                Name: GenericNameSyntax { Identifier.Text: "Create" } genericName
            })
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

        return null;
    }

    /// <summary>Takes a C# object creation and expands the macro into a Luau expression</summary>
    /// <returns>The expanded expression of the macro, or null if no macro was applied</returns>
    public Expression? ObjectCreation(Func<SyntaxNode, Node?> visit, ObjectCreationExpressionSyntax objectCreation) {
        if (objectCreation.Type is GenericNameSyntax genericName) {
            switch (genericName.Identifier.Text) {
                case "List":
                {
                    var expressions = objectCreation.Initializer?.Expressions.Select(expression => (Expression)visit(expression)!).ToList();
                    var table = new TableInitializer(expressions ?? []);
                    table.MarkExpanded(MacroKind.ListConstruction);
                    
                    return table;
                }
                
                case "Dictionary": {
                        var values = new List<Expression>();
                        var keys = new List<Expression>();

                        if (objectCreation.Initializer != null) {
                            foreach (var expression in objectCreation.Initializer.Expressions) {
                                if (expression is AssignmentExpressionSyntax assignmentExpression) {
                                    var key = (Expression)visit(assignmentExpression.Left)!;
                                    var value = (Expression)visit(assignmentExpression.Right)!;

                                    values.Add(value);
                                    keys.Add(key);
                                } else if (expression is InitializerExpressionSyntax initializerExpression) {
                                    var key = (Expression)visit(initializerExpression.Expressions[0])!;
                                    var value = (Expression)visit(initializerExpression.Expressions[1])!;

                                    values.Add(value);
                                    keys.Add(key);
                                }
                            }
                        }

                        var table = new TableInitializer(values, keys);
                        table.MarkExpanded(MacroKind.DictionaryConstruction);

                        return table;
                }
            }
        }

        return null;
    }

    public Node? Invocation(Func<SyntaxNode, Node?> visit, InvocationExpressionSyntax node) {
        if (node.Expression.Kind().ToString() == "SimpleMemberAccessExpression") { // TODO: Replace this cursed method when possible
            var typeName = _semanticModel.GetTypeInfo(node.Expression).ToString();
            var memberAccess = (MemberAccessExpressionSyntax)node.Expression;
                if (typeName == "Dictionary") {
                    switch (memberAccess.Name.Identifier.Text) {
                        case "Add": {
                                var arguments = (ArgumentList)visit(node.ArgumentList)!;
                                arguments.Arguments.Insert(0, new Argument(new IdentifierName(node.Expression.ToString())));
                                return new Call(new QualifiedName(new IdentifierName("table"), new IdentifierName("insert")), arguments);
                            }
                    }
                } else if (typeName == "List") {
                    
                }
        }
        return null;
    }
}