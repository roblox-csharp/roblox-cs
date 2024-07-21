using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    internal sealed class MainTransformer : BaseTransformer
    {
        public MainTransformer(SyntaxTree tree, ConfigData config)
            : base(tree, config)
        {
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            var identifierText = node.Identifier.Text;
            if (!identifierText.Contains("@") || identifierText == "var")
            {
                return base.VisitIdentifierName(node);
            }

            var fixedIdentifierText = identifierText.Replace("@", "");
            var emptyTrivia = SyntaxFactory.TriviaList();
            var newToken = SyntaxFactory.VerbatimIdentifier(emptyTrivia, fixedIdentifierText, fixedIdentifierText, emptyTrivia);
            return base.VisitIdentifierName(node.WithIdentifier(newToken));
        }

        public override SyntaxNode? VisitArgument(ArgumentSyntax node)
        {
            if (node.Expression.IsKind(SyntaxKind.IdentifierName))
            {
                var newExpression = VisitIdentifierName((IdentifierNameSyntax)node.Expression);
                return base.VisitArgument(node.WithExpression((ExpressionSyntax)newExpression!));
            }
            return base.VisitArgument(node);
        }

        public override SyntaxNode? VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            ExpressionSyntax fullExpression = node.WhenNotNull;
            if (node.WhenNotNull is InvocationExpressionSyntax invocation)
            {
                var memberBinding = (MemberBindingExpressionSyntax)invocation.Expression;
                var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Expression, memberBinding.Name);
                var fullMemberAccess = memberAccess.WithName(invocation.Expression.ChildNodes().OfType<SimpleNameSyntax>().First());
                fullExpression = invocation.WithExpression(fullMemberAccess);
            }
            else if (node.WhenNotNull is MemberBindingExpressionSyntax memberBinding)
            {
                fullExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Expression, memberBinding.Name);
            }

            return base.VisitConditionalAccessExpression(node.WithWhenNotNull(fullExpression));
        }
    }
}