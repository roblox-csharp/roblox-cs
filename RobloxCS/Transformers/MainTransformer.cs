using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Transformers;

public sealed class MainTransformer(SyntaxTree tree, TransformState state, ConfigData config)
    : BaseTransformer(tree, state, config)
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

    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        if (node.Name is not QualifiedNameSyntax qualifiedName)
            return base.VisitNamespaceDeclaration(node);
        
        var pieces = StandardUtility.GetNamesFromNode(qualifiedName);
        var firstName = pieces.First();
        var newFullName = StandardUtility.GetNameNode(pieces.Skip(1).ToList());
        var childNamespace = node.WithName(newFullName);
            
        node = node
            .WithName(SyntaxFactory.IdentifierName(firstName))
            .WithExterns([])
            .WithUsings([])
            .WithMembers([childNamespace]);

        return base.VisitNamespaceDeclaration(node);
    }

    // Return an IsPatternExpression if the binary operator is `is`
    public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        if (node.OperatorToken.Text != "is")
            return base.VisitBinaryExpression(node);
        
        var pattern = SyntaxFactory.TypePattern(SyntaxFactory.ParseTypeName(((IdentifierNameSyntax)node.Right).Identifier.Text));
        return SyntaxFactory.IsPatternExpression(node.Left, pattern);
    }
    
    // Fix conditional accesses so that they return the AST you expect them to
    public override SyntaxNode? VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        var whenNotNull = ProcessWhenNotNull(node.Expression, node.WhenNotNull);
        var newNode = whenNotNull != null ? node.WithWhenNotNull(whenNotNull) : node;
        return base.VisitConditionalAccessExpression(newNode);
    }

    public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node) =>
        base.VisitForEachStatement(SyntaxFactory.ForEachStatement(node.ForEachKeyword, node.OpenParenToken, node.Type, node.Identifier, node.InKeyword, node.Expression, node.CloseParenToken, Blockify(node.Statement)));

    public override SyntaxNode? VisitForStatement(ForStatementSyntax node) =>
        base.VisitForStatement(SyntaxFactory.ForStatement(node.Declaration, node.Initializers, node.Condition, node.Incrementors, Blockify(node.Statement)));

    public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node) =>
        base.VisitWhileStatement(SyntaxFactory.WhileStatement(node.WhileKeyword, node.OpenParenToken, node.Condition, node.CloseParenToken, Blockify(node.Statement)));
    
    public override SyntaxNode? VisitIfStatement(IfStatementSyntax node) =>
        base.VisitIfStatement(SyntaxFactory.IfStatement(node.Condition, Blockify(node.Statement), node.Else));
    
    public override SyntaxNode? VisitElseClause(ElseClauseSyntax node) =>
        base.VisitElseClause(SyntaxFactory.ElseClause(node.ElseKeyword, Blockify(node.Statement)));
    
    private static StatementSyntax Blockify(StatementSyntax statement)
    {
        return statement is BlockSyntax
            ? statement
            : SyntaxFactory.Block(SyntaxList.Create([statement]));
    }
        
    private ExpressionSyntax? ProcessWhenNotNull(ExpressionSyntax expression, ExpressionSyntax? whenNotNull)
    {
        if (whenNotNull == null)
            return null;
        
        return whenNotNull switch
        {
            MemberAccessExpressionSyntax memberAccess => SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, expression, memberAccess.Name
            ),
                
            MemberBindingExpressionSyntax memberBinding => SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, expression, memberBinding.Name
            ),
            
            ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess
                .WithExpression(ProcessWhenNotNull(expression, conditionalAccess.Expression) ?? conditionalAccess.Expression)
                .WithWhenNotNull(ProcessWhenNotNull(expression, conditionalAccess.WhenNotNull) ?? conditionalAccess.WhenNotNull),
            
            // dumb nested switch
            InvocationExpressionSyntax invocation => invocation.WithExpression((invocation.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, expression, memberAccess.Name
                ),
                
                MemberBindingExpressionSyntax memberBinding => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, expression, memberBinding.Name
                ),
                
                ConditionalAccessExpressionSyntax nestedConditional => ProcessWhenNotNull(
                    nestedConditional.WhenNotNull, expression
                ),
                
                _ => SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression,
                    (Visit(invocation.Expression) as SimpleNameSyntax)!
                )
            })!),
            
            _ => null
        };
    }
}