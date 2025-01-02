using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS;

public class Transpiler
{
    private const string _includeFolderName = "Include";

    public static string Transpile(string source)
    {
        var tree = ParseSource(source);
        var compiler = CompileASTs(tree);
        return WriteLuaOutput(compiler);
    }

    private static SyntaxTree ParseSource(string source)
    {
        var tree = TranspilerUtility.ParseTree(source);
        HashSet<Func<SyntaxTree, ConfigData, SyntaxTree>> transformers = [BuiltInTransformers.Main()];

        var transformedTree = TranspilerUtility.TransformTree(tree, transformers);
        foreach (var diagnostic in transformedTree.GetDiagnostics())
        {
            Logger.HandleDiagnostic(diagnostic);
        }

        return transformedTree;
    }
    
    private static CSharpCompilation CompileASTs(SyntaxTree tree)
    {
        var compiler = TranspilerUtility.GetCompiler([tree]);
        foreach (var diagnostic in compiler.GetDiagnostics())
        {
            Logger.HandleDiagnostic(diagnostic);
        }

        return compiler;
    }
    
    private static string WriteLuaOutput(CSharpCompilation compiler)
    {
        var tree = compiler.SyntaxTrees.First();
        var generatedLua = TranspilerUtility.GenerateLua(tree, compiler);
        return generatedLua;
    }
}