using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    public sealed class Transformer : CSharpSyntaxWalker
    {
        private SyntaxNode _root;
        private readonly SyntaxTree _tree;
        private readonly ConfigData _config;

        public Transformer(SyntaxTree tree, ConfigData config)
        {
            _root = tree.GetRoot();
            _tree = tree;
            _config = config;
        }

        public SyntaxNode GetRoot()
        {
            Visit(_root);
            return _root;
        }

        public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
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

            var newNode = node.WithWhenNotNull(fullExpression);
           _root = _root.ReplaceNode(node, newNode);
        }
    }
}