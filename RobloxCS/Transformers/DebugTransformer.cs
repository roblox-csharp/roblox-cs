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
            if (node.Expression is MemberAccessExpressionSyntax memberAccess && TryGetName(memberAccess.Expression) != null)
            {
                var objectName = GetName(memberAccess.Expression);
                var name = GetName(memberAccess.Name);
                List<string> consoleMethodNames = ["Write", "WriteLine"];

                if (objectName.EndsWith("Console") && consoleMethodNames.Contains(name))
                {
                    return PrependFileInfoArgument(node);
                }
            }
            else if (node.Expression is IdentifierNameSyntax identifierName)
            {
                var name = GetName(identifierName);
                List<string> concatMethodNames = ["error"];
                List<string> extraArgNames = ["warn"];

                if (concatMethodNames.Contains(name))
                {
                    return ConcatenateFileInfoToMessageArgument(node);
                }
                else if (extraArgNames.Contains(name))
                {
                    return PrependFileInfoArgument(node);
                }
            }

            return base.VisitInvocationExpression(node);
        }

        private SyntaxNode? ConcatenateFileInfoToMessageArgument(InvocationExpressionSyntax node)
        {
            var messageArgument = node.ArgumentList.Arguments.First().Expression;
            var fileInfoLiteral = GetFileInfoLiteral(node, addSpace: true);
            var emptyTrivia = SyntaxFactory.TriviaList();
            var plusToken = SyntaxFactory.Token(emptyTrivia, SyntaxKind.PlusToken, "+", "", emptyTrivia);
            var binaryExpression = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, fileInfoLiteral, plusToken, messageArgument);
            var newArgument = SyntaxFactory.Argument(binaryExpression);
            var newArguments = SeparatedSyntaxList.Create(new ReadOnlySpan<ArgumentSyntax>(ref newArgument));
            var newArgumentListNode = node.ArgumentList.WithArguments(newArguments);
            return base.VisitInvocationExpression(node.WithArgumentList(newArgumentListNode));
        }

        private SyntaxNode? PrependFileInfoArgument(InvocationExpressionSyntax node)
        {
            var fileInfoLiteral = GetFileInfoLiteral(node, addSpace: false);
            var argument = SyntaxFactory.Argument(fileInfoLiteral);
            var newArguments = SeparatedSyntaxList.Create(new ReadOnlySpan<ArgumentSyntax>(new List<ArgumentSyntax> { argument }.Concat(node.ArgumentList.Arguments).ToArray())); // good lord why do they make that so convoluted
            var newArgumentListNode = node.ArgumentList.WithArguments(newArguments);
            return base.VisitInvocationExpression(node.WithArgumentList(newArgumentListNode));
        }

        private LiteralExpressionSyntax GetFileInfoLiteral(SyntaxNode node, bool addSpace)
        {
            var fileLocation = Utility.FormatLocation(node.GetLocation().GetLineSpan())
                .Replace(_config.SourceFolder + "/", "")
                .Replace("./", "");

            var infoText = $"[{fileLocation}]:" + (addSpace ? " " : "");
            var emptyTrivia = SyntaxFactory.TriviaList();
            var token = SyntaxFactory.Token(emptyTrivia, SyntaxKind.StringLiteralToken, infoText, infoText, emptyTrivia); // why is trivia required bruh
            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, token);
        }
    }
}