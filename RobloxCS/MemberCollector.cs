using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    public sealed class MemberCollectionResult
    {
        public Dictionary<NamespaceDeclarationSyntax, List<MemberDeclarationSyntax>> Namespaces { get; set; } = [];
    }

    public sealed class MemberCollector : CSharpSyntaxWalker
    {
        private readonly Dictionary<NamespaceDeclarationSyntax, List<MemberDeclarationSyntax>> _namespaces = [];
        private readonly List<SyntaxTree> _trees;

        public MemberCollector(List<SyntaxTree> trees)
        {
            _trees = trees;
        }

        public MemberCollectionResult Collect()
        {
            foreach (var tree in _trees)
            {
                Visit(tree.GetRoot());
            }

            return new MemberCollectionResult()
            {
                Namespaces = _namespaces
            };
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var members = _namespaces.ContainsKey(node) ? _namespaces[node] : new List<MemberDeclarationSyntax>();
            foreach (var member in node.Members)
            {
                members.Add(member);
            }
            _namespaces[node] = members;
        }
    }
}
