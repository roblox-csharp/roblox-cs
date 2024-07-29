using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace RobloxCS
{
    internal sealed class MainTransformer : BaseTransformer
    {
        public MainTransformer(SyntaxTree tree, ConfigData config)
            : base(tree, config)
        {
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var newNode = node.Identifier.ValueText switch
            {
                "ToString" => node.WithIdentifier(CreateIdentifierToken("__tostring")),
                _ => node
            };
            return base.VisitMethodDeclaration(newNode);
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            var identifierText = node.Identifier.Text;
            if (!identifierText.Contains("@") || identifierText == "var")
            {
                return base.VisitIdentifierName(node);
            }

            var fixedIdentifierText = identifierText.Replace("@", "");
            var newToken = CreateIdentifierToken(fixedIdentifierText);
            return base.VisitIdentifierName(node.WithIdentifier(newToken));
        }

        private static SyntaxToken CreateIdentifierToken(string text, string? valueText = null, SyntaxTriviaList? trivia = null)
        {
            var triviaList = trivia ??= SyntaxFactory.TriviaList();
            return SyntaxFactory.VerbatimIdentifier(triviaList, text, valueText ?? text, triviaList);
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
            var whenNotNull = ProcessWhenNotNull(node.Expression, node.WhenNotNull);
            if (whenNotNull != null)
            {
                return base.VisitConditionalAccessExpression(node.WithWhenNotNull(whenNotNull));
            }
            return base.VisitConditionalAccessExpression(node);
        }

        private ExpressionSyntax? ProcessWhenNotNull(ExpressionSyntax expression, ExpressionSyntax whenNotNull)
        {
            if (whenNotNull == null)
            {
                return null;
            }

            switch (whenNotNull)
            {
                case MemberBindingExpressionSyntax memberBinding:
                    return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, memberBinding.Name);
                case InvocationExpressionSyntax invocation:
                    return invocation.WithExpression((invocation.Expression switch
                    {
                        MemberAccessExpressionSyntax memberAccess => SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression,
                            memberAccess.Name
                        ),
                        ConditionalAccessExpressionSyntax nestedConditional => ProcessWhenNotNull(nestedConditional.WhenNotNull, expression),
                        MemberBindingExpressionSyntax memberBinding => SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression,
                            memberBinding.Name
                        ),
                        _ => SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression,
                            SyntaxFactory.IdentifierName(invocation.Expression.ToString())
                        )
                    })!);
                case ConditionalAccessExpressionSyntax conditionalAccess:
                    return conditionalAccess
                        .WithExpression(ProcessWhenNotNull(expression, conditionalAccess.Expression) ?? conditionalAccess.Expression)
                        .WithWhenNotNull(ProcessWhenNotNull(expression, conditionalAccess.WhenNotNull) ?? conditionalAccess.WhenNotNull);
                default:
                    return null;
            };
        }
    }
}