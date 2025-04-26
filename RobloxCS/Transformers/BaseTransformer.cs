using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Transformers;

public abstract class BaseTransformer(SyntaxTree tree, Prerequisites state, ConfigData config) : CSharpSyntaxRewriter
{
    protected readonly ConfigData _config = config;
    protected readonly SyntaxNode _root = tree.GetRoot();
    protected readonly Prerequisites _state = state;
    protected readonly SyntaxTree _tree = tree;

    public SyntaxTree TransformTree() => _tree.WithRootAndOptions(Visit(_root), _tree.Options);
    protected static string? TryGetName(SyntaxNode node) => StandardUtility.GetNamesFromNode(node).FirstOrDefault();
    protected static string GetName(SyntaxNode node) => StandardUtility.GetNamesFromNode(node).First();
    protected static bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax) => tokens.Any(token => token.IsKind(syntax));
}