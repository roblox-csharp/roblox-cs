using Microsoft.CodeAnalysis;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Transformers;

using TransformMethod = Func<SyntaxTree, TransformState, ConfigData, SyntaxTree>;

public static class BuiltInTransformers
{
    public static TransformMethod Main() => (tree, state, config) => new MainTransformer(tree, state, config).TransformTree();

    private static TransformMethod FailedToGetTransformer(string name) =>
        throw Logger.Error($"No built-in transformer \"{name}\" exists (roblox-cs.yml)");
}