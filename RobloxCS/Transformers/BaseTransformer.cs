using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RobloxCS.Shared;

namespace RobloxCS.Transformers;

public abstract class BaseTransformer(SyntaxTree tree, ConfigData config) : CSharpSyntaxRewriter
{
    protected readonly SyntaxNode _root = tree.GetRoot();
    protected readonly SyntaxTree _tree = tree;
    protected readonly ConfigData _config = config;

    public SyntaxTree TransformTree() =>
        _tree.WithRootAndOptions(Visit(_root), _tree.Options);
    protected static string? TryGetName(SyntaxNode node) =>
        StandardUtility.GetNamesFromNode(node).FirstOrDefault();
    protected static string GetName(SyntaxNode node) =>
        StandardUtility.GetNamesFromNode(node).First();
    protected static bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax) =>
        tokens.Any(token => token.IsKind(syntax));

    protected static SyntaxToken CreateIdentifierToken(string text, string? valueText = null,
        SyntaxTriviaList? leadingTrivia = null, SyntaxTriviaList? trailingTrivia = null)
    {
        return SyntaxFactory.VerbatimIdentifier(
            leadingTrivia ?? SyntaxFactory.TriviaList(),
            text,
            valueText ?? text,
            trailingTrivia ?? SyntaxFactory.TriviaList()
        );
    }
}