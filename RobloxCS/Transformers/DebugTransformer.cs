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
            if (node.Expression is IdentifierNameSyntax || node.Expression is MemberAccessExpressionSyntax)
            {
                {
                    if (node.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        if (Utility.GetNamesFromNode(memberAccess.Expression).LastOrDefault() != "Globals")
                        {
                            goto DoNothing;
                        }
                    }
                }
                {
                    var name = GetName(node.Expression is MemberAccessExpressionSyntax memberAccess ? memberAccess.Name : node.Expression);
                    HashSet<string> concatMethodNames = ["error"];
                    HashSet<string> extraArgNames = ["print", "warn"];

                    var newNode = node;
                    if (concatMethodNames.Contains(name))
                    {
                        newNode = ConcatenateFileInfoToMessageArgument(node);
                    }
                    else if (extraArgNames.Contains(name))
                    {
                        newNode = PrependFileInfoArgument(node);
                    }

                    return base.VisitInvocationExpression(node.Expression is MemberAccessExpressionSyntax _memberAccess ? newNode.WithExpression(_memberAccess.Name) : newNode);
                }
            }

            DoNothing:
            return base.VisitInvocationExpression(node);
        }

        private InvocationExpressionSyntax ConcatenateFileInfoToMessageArgument(InvocationExpressionSyntax node)
        {
            var messageArgument = node.ArgumentList.Arguments.First().Expression;
            var fileInfoLiteral = GetFileInfoLiteral(node, addSpace: true);
            var emptyTrivia = SyntaxFactory.TriviaList();
            var plusToken = SyntaxFactory.Token(emptyTrivia, SyntaxKind.PlusToken, "+", "", emptyTrivia);
            var binaryExpression = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, fileInfoLiteral, plusToken, messageArgument);
            var newArgument = SyntaxFactory.Argument(binaryExpression);
            var newArguments = SeparatedSyntaxList.Create(new ReadOnlySpan<ArgumentSyntax>(ref newArgument));
            var newArgumentListNode = node.ArgumentList.WithArguments(newArguments);
            return node.WithArgumentList(newArgumentListNode);
        }

        private InvocationExpressionSyntax PrependFileInfoArgument(InvocationExpressionSyntax node)
        {
            var fileInfoLiteral = GetFileInfoLiteral(node, addSpace: false);
            var argument = SyntaxFactory.Argument(fileInfoLiteral);
            var newArguments = SeparatedSyntaxList.Create(new ReadOnlySpan<ArgumentSyntax>(new List<ArgumentSyntax> { argument }.Concat(node.ArgumentList.Arguments).ToArray())); // good lord why do they make that so convoluted
            var newArgumentListNode = node.ArgumentList.WithArguments(newArguments);
            return node.WithArgumentList(newArgumentListNode);
        }

        private LiteralExpressionSyntax GetFileInfoLiteral(SyntaxNode node, bool addSpace)
        {
            var fileLocation = FormatLocation(node);

            var infoText = $"[{fileLocation}]:" + (addSpace ? " " : "");
            var emptyTrivia = SyntaxFactory.TriviaList();
            var token = SyntaxFactory.Token(emptyTrivia, SyntaxKind.StringLiteralToken, infoText, infoText, emptyTrivia); // why is trivia required bruh
            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, token);
        }

        private string FormatLocation(SyntaxNode node)
        {
            var location = Utility.FormatLocation(node.GetLocation().GetLineSpan())
                .Replace(_config.SourceFolder + "/", "")
                .Replace("..", "./")
                .Replace("./", "");

            return location;
        }
    }
}