using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Transformers;

namespace RobloxCS;

using TransformMethod = Func<SyntaxTree, TransformState, ConfigData, SyntaxTree>;

public static class TranspilerUtility
{
    public static string GenerateLuau(SyntaxTree tree, CSharpCompilation compiler)
    {
        var luauAST = GetLuauAST(tree, compiler);
        var luau = new LuauWriter();

        return luau.Render(luauAST);
    }

    public static AST GetLuauAST(SyntaxTree tree, CSharpCompilation compiler)
    {
        var generator = new LuauGenerator(tree, compiler, new TransformState(), new OccupiedIdentifiersStack());

        return generator.GetLuauAST();
    }

    public static CSharpCompilation GetCompiler(List<SyntaxTree> trees, ConfigData? config)
    {
        // config ??= ConfigReader.UnitTestingConfig;

        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);

        return CSharpCompilation.Create(assemblyName: "test", //config.CSharpOptions.AssemblyName,
                                        syntaxTrees: trees,
                                        references: FileUtility.GetCompilationReferences(),
                                        options: compilationOptions);
    }

    public static SyntaxTree ParseAndTransformTree(string source, ConfigData? config)
    {
        var tree = ParseTree(source);
        HashSet<TransformMethod> transformers = [BuiltInTransformers.Main()];

        return TransformTree(tree, transformers, config);
    }

    public static SyntaxTree TransformTree(SyntaxTree cleanTree, HashSet<TransformMethod> transformMethods, ConfigData? config)
    {
        // config ??= ConfigReader.UnitTestingConfig;
        config ??= new ConfigData();

        var state = new TransformState();

        return transformMethods.Aggregate(cleanTree, (current, transform) => transform(current, state, config));
    }

    public static SyntaxTree ParseTree(string source, string sourceFile = "TestFile.cs")
    {
        var cleanTree = CSharpSyntaxTree.ParseText(source);
        var compilationUnit = (CompilationUnitSyntax)cleanTree.GetRoot();
        var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));
        var newRoot = compilationUnit.AddUsings(usingDirective);

        return cleanTree
               .WithRootAndOptions(newRoot, cleanTree.Options)
               .WithFilePath(sourceFile);
    }
}