using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;

namespace RobloxCS;

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
        var generator = new LuauGenerator(tree, compiler, new TransformState());
        return generator.GetLuauAST();
    }
    
    public static CSharpCompilation GetCompiler(List<SyntaxTree> trees, ConfigData? config)
    {
        // config ??= ConfigReader.UnitTestingConfig;
        
        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
        return CSharpCompilation.Create(
           assemblyName: "test",//config.CSharpOptions.AssemblyName,
           syntaxTrees: trees,
           references: FileUtility.GetCompilationReferences(),
           options: compilationOptions
       );
    }
    
    public static SyntaxTree ParseAndTransformTree(string source, ConfigData? config)
    {
        var tree = ParseTree(source);
        HashSet<Func<SyntaxTree, ConfigData, SyntaxTree>> transformers = [BuiltInTransformers.Main()];
        
        return TransformTree(tree, transformers);
    }
    
    public static SyntaxTree TransformTree(SyntaxTree cleanTree, HashSet<Func<SyntaxTree, ConfigData, SyntaxTree>> transformMethods, ConfigData? config = null)
    {
        // config ??= ConfigReader.UnitTestingConfig;
        config ??= new ConfigData();

        return transformMethods.Aggregate(cleanTree, (current, transform) => transform(current, config));
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