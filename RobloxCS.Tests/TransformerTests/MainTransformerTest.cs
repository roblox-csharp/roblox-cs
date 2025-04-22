using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Transformers;

namespace RobloxCS.Tests.TransformerTests;

public class MainTransformerTest
{
    [Fact]
    public void Transforms_FileScopedNamespaces()
    {
        var compilationUnit = Transform("namespace Abc;");
        Assert.Single(compilationUnit.Members);
        Assert.IsType<NamespaceDeclarationSyntax>(compilationUnit.Members.First());
        
        var @namespace = (NamespaceDeclarationSyntax)compilationUnit.Members.First();
        Assert.Equal("Abc", @namespace.Name.ToString());
    }
    
    [Fact]
    public void AddsExtraUsings()
    {
        var compilationUnit = Transform("");
        Assert.Equal(4, compilationUnit.Usings.Count);

        var usingSystemCollectionsGeneric = compilationUnit.Usings[0];
        var usingSystemLinq = compilationUnit.Usings[1];
        var usingRoblox = compilationUnit.Usings[2];
        var usingRobloxGlobals = compilationUnit.Usings[3];
        Assert.Equal("System.Collections.Generic", usingSystemCollectionsGeneric.Name?.ToString());
        Assert.Equal("System.Linq", usingSystemLinq.Name?.ToString());
        Assert.Equal("Roblox", usingRoblox.Name?.ToString());
#pragma warning disable xUnit2002
        Assert.NotNull(usingRobloxGlobals.StaticKeyword);
#pragma warning restore xUnit2002
        Assert.Equal("Roblox.Globals", usingRobloxGlobals.Name?.ToString());
    }
    
    private static CompilationUnitSyntax Transform(string source)
    {
        var cleanTree = SyntaxFactory.ParseSyntaxTree(source);
        var transform = BuiltInTransformers.Main();
        var transformedTree = transform(cleanTree, new TransformState(), new ConfigData());
        return transformedTree.GetCompilationUnitRoot();
    }
}