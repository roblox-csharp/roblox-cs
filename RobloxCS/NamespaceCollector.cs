using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    internal sealed class NamespaceCollector : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly SyntaxNode _root;
        private readonly Dictionary<string, string> _namespaceMap = new Dictionary<string, string>();

        public NamespaceCollector(CSharpCompilation compiler, SyntaxTree tree)
        {
            _semanticModel = compiler.GetSemanticModel(tree);
            _root = tree.GetRoot();
        }

        public Dictionary<string, string> GetNamespaceMap()
        {
            Visit(_root);
            return _namespaceMap;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
            {
                _namespaceMap[symbol.Name] = symbol.ContainingNamespace.ToDisplayString();
            }
            base.VisitClassDeclaration(node);
        }
    }
}
