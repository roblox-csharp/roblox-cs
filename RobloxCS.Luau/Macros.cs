using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;

namespace RobloxCS.Macros;

public enum MacroKind
{
    NewInstance
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
}