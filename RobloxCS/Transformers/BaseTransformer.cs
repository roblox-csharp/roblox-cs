using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Transformers;

public abstract class BaseTransformer(FileCompilation file) : CSharpSyntaxRewriter
{
    protected readonly FileCompilation _file = file;

    public SyntaxTree TransformTree() => _file.Tree = _file.Tree.WithRootAndOptions(Visit(_file.Tree.GetRoot()), _file.Tree.Options);
    protected static string? TryGetName(SyntaxNode node) => StandardUtility.GetNamesFromNode(node).FirstOrDefault();
    protected static string GetName(SyntaxNode node) => StandardUtility.GetNamesFromNode(node).First();
    protected static bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax) => tokens.Any(token => token.IsKind(syntax));
}