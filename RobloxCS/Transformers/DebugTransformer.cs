using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    public sealed class DebugTransformer : BaseTransformer
    {
        public DebugTransformer(SyntaxTree tree, ConfigData config)
            : base(tree, config)
        {
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression is MemberAccessExpressionSyntax)
            {
                var memberAccess = (MemberAccessExpressionSyntax)node.Expression;
                var objectName = GetName(memberAccess.Expression);
                var name = GetName(memberAccess.Name);

                switch (name)
                {
                    case "Write":
                    case "WriteLine":
                        {
                            if (!objectName.EndsWith("Console")) break;

                            var fileLocation = Utility.FormatLocation(node.GetLocation().GetLineSpan())
                                .Replace(_config.SourceFolder + "/", "")
                                .Replace("./", "");

                            var fileInfo = $"[{fileLocation}]:";
                            var literalToken = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.StringLiteralToken, fileInfo, fileInfo, SyntaxFactory.TriviaList()); // why is trivia required bruh
                            var literal = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, literalToken);
                            var argument = SyntaxFactory.Argument(literal);
                            var newArguments = SeparatedSyntaxList.Create(new ReadOnlySpan<ArgumentSyntax>(new List<ArgumentSyntax> { argument }.Concat(node.ArgumentList.Arguments).ToArray())); // good lord why do they make that so convoluted
                            var newArgumentListNode = node.ArgumentList.WithArguments(newArguments);
                            return base.VisitInvocationExpression(node.WithArgumentList(newArgumentListNode));
                        }
                }
            }

            return base.VisitInvocationExpression(node);
        }
    }
}