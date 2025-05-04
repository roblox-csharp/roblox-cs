using System.Reflection;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;
using RobloxCS.Transformers;
using Path = System.IO.Path;

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
        var analyzer = new Analyzer(file, compiler);
        var analysisResult = analyzer.Analyze(file.Tree.GetRoot());
        var generator = new LuauGenerator(file, compiler, analysisResult);

        return generator.GetLuauAST();
    }

    public static CSharpCompilation GetCompiler(IEnumerable<SyntaxTree> trees, ConfigData config)
    {
        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
        
        return CSharpCompilation.Create("RobloxGame", // probably temporary until i set up msbuild (hell)
                                        trees,
                                        FileUtility.GetCompilationReferences(),
                                        compilationOptions);
    }

    // didn't change `source` to `path` cuz this is used in tests. this needs a big refactor
    public static FileCompilation ParseAndTransformTree(string source, RojoProject? rojoProject, ConfigData config, string path = "TestFile.cs")
    {
        var tree = ParseTree(source, path);
        var file = GetFileCompilation(tree, rojoProject, config);
        HashSet<TransformMethod> transformers = [BuiltInTransformers.Main()];

        TransformTree(file, transformers);
        return file;
    }

    private static FileCompilation GetFileCompilation(SyntaxTree tree, RojoProject? rojoProject, ConfigData config) =>
        new() { Tree = tree, RojoProject = rojoProject, Config = config };

    private static SyntaxTree TransformTree(FileCompilation file, HashSet<TransformMethod> transformMethods) =>
        // config ??= ConfigReader.UnitTestingConfig;
        transformMethods.Aggregate(file.Tree, (_, transform) => transform(file));

    private static SyntaxTree ParseTree(string source, string sourceFile)
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