using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS.Luau
{
    /// <summary>Basically just defines utility methods for LuauGenerator</summary>
    public class BaseGenerator(SyntaxTree tree, CSharpCompilation compiler) : CSharpSyntaxVisitor<Node>
    {
        protected readonly SyntaxTree _tree = tree;
        protected readonly SemanticModel _semanticModel = compiler.GetSemanticModel(tree);

        protected TNode Visit<TNode>(SyntaxNode? node) where TNode : Node?
        {
            return (TNode)Visit(node)!;
        }

        protected string GetName(SyntaxNode node)
        {
            return Utility.GetNamesFromNode(node).First();
        }

        protected string? TryGetName(SyntaxNode? node)
        {
            return Utility.GetNamesFromNode(node).FirstOrDefault();
        }

        protected bool IsGlobal(SyntaxNode node)
        {
            return node.Parent.IsKind(SyntaxKind.GlobalStatement) || node.Parent.IsKind(SyntaxKind.CompilationUnit);
        }

        protected bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax)
        {
            return tokens.Any(token => token.IsKind(syntax));
        }
    }
}