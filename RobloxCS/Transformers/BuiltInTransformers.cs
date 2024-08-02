using Microsoft.CodeAnalysis;

namespace RobloxCS
{
    using TransformMethod = Func<SyntaxTree, ConfigData, SyntaxTree>;

    public static partial class BuiltInTransformers
    {
        public static TransformMethod Main()
        {
            return (tree, config) => new MainTransformer(tree, config).TransformTree();
        }

        public static TransformMethod Get(string name)
        {
            return name.ToLower() switch
            {
                "debug" => (tree, config) => new DebugTransformer(tree, config).TransformTree(),
                _ => FailedToGetTransformer(name)
            };
        }

        private static TransformMethod FailedToGetTransformer(string name)
        {
            Logger.Error($"No built-in transformer \"{name}\" exists (roblox-cs.yml)");
            return null!; // hack
        }
    }
}
