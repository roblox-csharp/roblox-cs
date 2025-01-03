using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS.Transformers;

public sealed class MainTransformer(SyntaxTree tree, ConfigData config) : BaseTransformer(tree, config)
{
    // Add `using Roblox` and `using static Roblox.Globals` to top of file
    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        var usings = node.Usings;
        usings = usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Roblox")));
        usings = usings.Add(SyntaxFactory.UsingDirective(
            SyntaxFactory.Token(SyntaxKind.StaticKeyword),
            null,
            SyntaxFactory.QualifiedName(
                SyntaxFactory.IdentifierName("Roblox"),
                SyntaxFactory.IdentifierName("Globals")
            )
        ));
        
        return base.VisitCompilationUnit(node.WithUsings(usings));
    }
    
    // Turn file-scoped namespaces into regular namespaces (to reduce code duplication)
    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) =>
        VisitNamespaceDeclaration(SyntaxFactory.NamespaceDeclaration(node.AttributeLists, node.Modifiers, node.Name, node.Externs, node.Usings, node.Members));
    
    public override SyntaxNode? VisitGlobalStatement(GlobalStatementSyntax node) => base.VisitGlobalStatement(node);
    
    // Return an IsPatternExpression if the binary operator is `is`
    public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        if (node.OperatorToken.Text != "is")
            return base.VisitBinaryExpression(node);
        
        var pattern = SyntaxFactory.TypePattern(SyntaxFactory.ParseTypeName(((IdentifierNameSyntax)node.Right).Identifier.Text));
        return SyntaxFactory.IsPatternExpression(node.Left, pattern);
    }
    
    // Transform `@` out of identifier names
    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        var identifierText = node.Identifier.Text;
        if (!identifierText.Contains('@') || identifierText == "var")
            return base.VisitIdentifierName(node);

        var fixedIdentifierText = identifierText.Replace("@", "");
        var newToken = CreateIdentifierToken(fixedIdentifierText);
        return base.VisitIdentifierName(node.WithIdentifier(newToken));
    }
    
    // Fix conditional accesses so that they return the AST you expect them to
    public override SyntaxNode? VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        var whenNotNull = ProcessWhenNotNull(node.Expression, node.WhenNotNull);
        var newNode = whenNotNull != null ? node.WithWhenNotNull(whenNotNull) : node;
        return base.VisitConditionalAccessExpression(newNode);
    }
    
    private static bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax) =>
        tokens.Any(token => token.IsKind(syntax));

    private static ExpressionSyntax? ProcessWhenNotNull(ExpressionSyntax expression, ExpressionSyntax? whenNotNull)
    {
        if (whenNotNull == null)
            return null;

        return whenNotNull switch
        {
            MemberBindingExpressionSyntax memberBinding => SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, expression, memberBinding.Name
            ),
            
            // dumb hacky nested switch
            InvocationExpressionSyntax invocation => invocation.WithExpression((invocation.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, expression, memberAccess.Name
                ),
                
                ConditionalAccessExpressionSyntax nestedConditional => ProcessWhenNotNull(
                    nestedConditional.WhenNotNull, expression
                ),
                
                MemberBindingExpressionSyntax memberBinding => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, expression, memberBinding.Name
               ),
                
                _ => SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression,
                    SyntaxFactory.IdentifierName(invocation.Expression.ToString())
                )
            })!),
            
            ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess
                .WithExpression(ProcessWhenNotNull(expression, conditionalAccess.Expression) ??
                                conditionalAccess.Expression)
                .WithWhenNotNull(ProcessWhenNotNull(expression, conditionalAccess.WhenNotNull) ??
                                 conditionalAccess.WhenNotNull),
            
            _ => null
        };
    }
}