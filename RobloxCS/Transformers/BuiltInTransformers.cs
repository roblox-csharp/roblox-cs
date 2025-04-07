using Microsoft.CodeAnalysis;
using RobloxCS.Shared;
using RobloxCS.Transformers;

namespace RobloxCS;

using TransformMethod = Func<SyntaxTree, ConfigData, SyntaxTree>;

public static class BuiltInTransformers
{
    public static TransformMethod Main() =>
        (tree, config) => new MainTransformer(tree, config).TransformTree();

    public static TransformMethod Get(string name)
    {
        return name.ToLower() switch
        {
            // "debug" => (tree, config) => new DebugTransformer(tree, config).TransformTree(),
            _ => FailedToGetTransformer(name)
        };
    }

    private static TransformMethod FailedToGetTransformer(string name) =>
        throw Logger.Error($"No built-in transformer \"{name}\" exists (roblox-cs.yml)");
}