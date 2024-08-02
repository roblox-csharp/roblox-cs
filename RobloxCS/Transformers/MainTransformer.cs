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

        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var usings = node.Usings;
            usings = usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Roblox")));
            usings = usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.Token(SyntaxKind.StaticKeyword), null, SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("Roblox"), SyntaxFactory.IdentifierName("Globals"))));
            return base.VisitCompilationUnit(node.WithUsings(usings));
        }

        public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            return VisitNamespaceDeclaration(SyntaxFactory.NamespaceDeclaration(node.AttributeLists, node.Modifiers, node.Name, node.Externs, node.Usings, node.Members));
        }

        public override SyntaxNode? VisitDoStatement(DoStatementSyntax node)
        {
            // invert condition
            return base.VisitDoStatement(node.WithCondition(SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, node.Condition)));
        }

        public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.OperatorToken.Text == "is")
            {
                var pattern = SyntaxFactory.TypePattern(SyntaxFactory.ParseTypeName(((IdentifierNameSyntax)node.Right).Identifier.Text));
                return SyntaxFactory.IsPatternExpression(node.Left, pattern);
            }
            return base.VisitBinaryExpression(node);
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var newNode = node.Identifier.ValueText switch
            {
                "ToString" => node.WithIdentifier(CreateIdentifierToken("__tostring")),
                "Equals" => node.WithIdentifier(CreateIdentifierToken("__eq")),
                _ => node
            };
            if (node != newNode && HasSyntax(newNode.Modifiers, SyntaxKind.OverrideKeyword))
            {
                var newModifiers = newNode.Modifiers.RemoveAt(newNode.Modifiers.Select(token => token.Kind()).ToList().IndexOf(SyntaxKind.OverrideKeyword));
                newNode = newNode.WithModifiers(newModifiers);
            }
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

        private static bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax)
        {
            return tokens.Any(token => token.IsKind(syntax));
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