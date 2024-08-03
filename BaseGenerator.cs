using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    public class BaseGenerator : CSharpSyntaxVisitor<Luau.Node>
    {
        protected Luau.IdentifierName CreateIdentifierName(SyntaxNode node)
        {
            return CreateIdentifierName(GetName(node));
        }

        protected Luau.IdentifierName CreateIdentifierName(string name)
        {
            return new Luau.IdentifierName(name);
        }

        protected Luau.TypeRef? CreateTypeRef(TypeSyntax? type)
        {
            if (type == null) return null;
            if (type.ToString() == "var") return null;
            return new(Utility.GetMappedType(type.ToString()));
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
