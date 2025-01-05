using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;

namespace RobloxCS.Macros;

public enum MacroKind
{
    NewInstance,
    ListConstruction,
    DictionaryConstruction
}

public static class Macro
{
    /// <summary>Takes a C# member access and expands the macro into a Luau expression</summary>
    /// <returns>The expanded expression of the macro, or null if no macro was applied</returns>
    public static Expression? MemberAccess(Func<SyntaxNode, Node?> visit, MemberAccessExpressionSyntax memberAccess)
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

    // TODO: Emit types when constructing Lists and Dictionaries?
    public static Expression? ObjectCreation(Func<SyntaxNode, Node?> visit, ObjectCreationExpressionSyntax objectCreation) {
        if (objectCreation.Type is GenericNameSyntax genericName) {
            switch (genericName.Identifier.Text) {
                case "List": {
                        var table = new Luau.TableInitializer(objectCreation.Initializer.Expressions.ToList().ConvertAll((expression) => (Expression)visit(expression)!)!);
                        table.MarkExpanded(MacroKind.ListConstruction);
                        return table;
                    }
                case "Dictionary": {
                        var values = new List<Expression>();
                        var keys = new List<Expression>();

                        if (objectCreation.Initializer != null) {
                            foreach (var expression in objectCreation.Initializer.Expressions) {
                                if (expression is AssignmentExpressionSyntax assignmentExpression) {
                                    var key = new IdentifierName(assignmentExpression.Left.ToString()); // Visiting didn't seem to work here
                                    var value = (Expression)visit(assignmentExpression.Right)!;

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
}