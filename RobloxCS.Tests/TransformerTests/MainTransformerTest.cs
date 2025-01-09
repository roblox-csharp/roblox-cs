using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS.Tests.TransformerTests;

public class MainTransformerTest
{
    [Fact]
    public void AddsRobloxImports()
    {
        var transformedTree = Transform("");
        var compilationUnit = transformedTree.GetCompilationUnitRoot();
        Assert.Equal(2, compilationUnit.Usings.Count);

        var usingRoblox = compilationUnit.Usings.First();
        var usingRobloxGlobals = compilationUnit.Usings.Last();
        Assert.Equal("Roblox", usingRoblox.Name?.ToString());
#pragma warning disable xUnit2002
        Assert.NotNull(usingRobloxGlobals.StaticKeyword);
#pragma warning restore xUnit2002
        Assert.Equal("Roblox.Globals", usingRobloxGlobals.Name?.ToString());
    }
    
    private static SyntaxTree Transform(string source)
    {
        var cleanTree = SyntaxFactory.ParseSyntaxTree(source);
        var transform = BuiltInTransformers.Main();
        var transformedTree = transform(cleanTree, new ConfigData());
        return transformedTree;
    }
}