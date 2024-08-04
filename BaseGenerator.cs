using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS
{
    public class BaseGenerator : CSharpSyntaxVisitor<Luau.Node>
    {
        protected TNode Visit<TNode>(SyntaxNode? node) where TNode : Luau.Node?
        {
            return (TNode)Visit(node)!;
        }

        protected bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax)
        {
            return tokens.Any(token => token.IsKind(syntax));
        }

        protected Luau.Node? TryVisit(SyntaxNode? node)
        {
            if (node == null) return null;
            return Visit(node);
        }

        protected string GetName(SyntaxNode node)
        {
            return Utility.GetNamesFromNode(node).First();
        }

        protected string? TryGetName(SyntaxNode? node)
        {
            return Utility.GetNamesFromNode(node).FirstOrDefault();
        }
    }
}
