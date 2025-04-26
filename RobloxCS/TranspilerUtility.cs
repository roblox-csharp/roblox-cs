using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;
using RobloxCS.Transformers;

namespace RobloxCS;

using TransformMethod = Func<FileCompilation, SyntaxTree>;

public static class TranspilerUtility
{
    public static string GenerateLuau(FileCompilation file, CSharpCompilation compiler)
    {
        var luauAST = GetLuauAST(file, compiler);
        var luau = new LuauWriter();

        return luau.Render(luauAST);
    }

    public static AST GetLuauAST(FileCompilation file, CSharpCompilation compiler)
    {
        var generator = new LuauGenerator(file, compiler);

        return generator.GetLuauAST();
    }

    public static CSharpCompilation GetCompiler(List<SyntaxTree> trees, ConfigData config)
    {
        // config ??= ConfigReader.UnitTestingConfig;

        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);

        return CSharpCompilation.Create("test", //config.CSharpOptions.AssemblyName,
                                        trees,
                                        FileUtility.GetCompilationReferences(),
                                        compilationOptions);
    }

    public static FileCompilation ParseAndTransformTree(string source, RojoProject? rojoProject, ConfigData config)
    {
        // config ??= ConfigReader.UnitTestingConfig;

        var tree = ParseTree(source);
        var file = GetFileCompilation(tree, rojoProject, config);
        HashSet<TransformMethod> transformers = [BuiltInTransformers.Main()];

        TransformTree(file, transformers);
        return file;
    }

    public static FileCompilation GetFileCompilation(SyntaxTree tree, RojoProject? rojoProject, ConfigData config) =>
        new() { Tree = tree, RojoProject = rojoProject, Config = config };

    private static SyntaxTree TransformTree(FileCompilation file, HashSet<TransformMethod> transformMethods) =>

        // config ??= ConfigReader.UnitTestingConfig;
        transformMethods.Aggregate(file.Tree, (_, transform) => transform(file));

    private static SyntaxTree ParseTree(string source, string sourceFile = "TestFile.cs")
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