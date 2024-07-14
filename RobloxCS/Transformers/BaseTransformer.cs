using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS
{
    public abstract class BaseTransformer : CSharpSyntaxRewriter
    {
        protected SyntaxNode _root;
        protected readonly SyntaxTree _tree;
        protected readonly ConfigData _config;

        public BaseTransformer(SyntaxTree tree, ConfigData config)
        {
            _root = tree.GetRoot();
            _tree = tree;
            _config = config;
        }

        public SyntaxNode TransformRoot()
        {
            return Visit(_root);
        }

        protected string GetName(SyntaxNode node)
        {
            return Utility.GetNamesFromNode(node).First();
        }
    }
}
