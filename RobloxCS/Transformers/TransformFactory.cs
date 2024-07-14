using Microsoft.CodeAnalysis;

namespace RobloxCS
{
    using TransformMethod = Func<SyntaxTree, ConfigData, SyntaxTree>;

    public static partial class TransformFactory
    {
        public static TransformMethod Main()
        {
            return (tree, config) => new MainTransformer(tree, config).TransformTree();
        }

        public static TransformMethod Debug()
        {
            return (tree, config) => new DebugTransformer(tree, config).TransformTree();
        }
    }
}
