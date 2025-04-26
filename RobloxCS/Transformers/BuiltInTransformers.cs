using Microsoft.CodeAnalysis;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Transformers;

using TransformMethod = Func<SyntaxTree, Prerequisites, ConfigData, SyntaxTree>;

public static class BuiltInTransformers
{
    public static TransformMethod Main() => (tree, prerequisites, config) => new MainTransformer(tree, prerequisites, config).TransformTree();

    private static TransformMethod FailedToGetTransformer(string name) => throw Logger.Error($"No built-in transformer \"{name}\" exists (roblox-cs.yml)");
}