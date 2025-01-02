using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS.Transformers;

public abstract class BaseTransformer(SyntaxTree tree, ConfigData config) : CSharpSyntaxRewriter
{
    protected SyntaxNode _root = tree.GetRoot();
    protected readonly SyntaxTree _tree = tree;
    protected readonly ConfigData _config = config;

    public SyntaxTree TransformTree() =>
        _tree.WithRootAndOptions(Visit(_root), _tree.Options);
    protected string? TryGetName(SyntaxNode node) =>
        Luau.Utility.GetNamesFromNode(node).FirstOrDefault();
    protected string GetName(SyntaxNode node) =>
        Luau.Utility.GetNamesFromNode(node).First();

    protected SyntaxToken CreateIdentifierToken(string text, string? valueText = null, SyntaxTriviaList? trivia = null)
    {
        var triviaList = trivia ?? SyntaxFactory.TriviaList();
        return SyntaxFactory.VerbatimIdentifier(triviaList, text, valueText ?? text, triviaList);
    }
}