using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS;

public class Transpiler
{
    private const string _includeFolderName = "Include";
    private static readonly HashSet<string> _ignoredDiagnostics =
    [
        "CS5001"
    ];

    public static string Transpile(string source)
    {
        var tree = ParseSource(source);
        var compiler = TranspilerUtility.GetCompiler([tree]);
        foreach (var diagnostic in compiler.GetDiagnostics().Where(diagnostic => !_ignoredDiagnostics.Contains(diagnostic.Id)))
            Logger.HandleDiagnostic(diagnostic);
        
        return WriteLuaOutput(compiler);
    }

    private static SyntaxTree ParseSource(string source)
    {
        var tree = TranspilerUtility.ParseTree(source);
        HashSet<Func<SyntaxTree, ConfigData, SyntaxTree>> transformers = [BuiltInTransformers.Main()];
        
        return TranspilerUtility.TransformTree(tree, transformers);
    }
    
    private static string WriteLuaOutput(CSharpCompilation compiler)
    {
        var tree = compiler.SyntaxTrees.First();
        var generatedLua = TranspilerUtility.GenerateLua(tree, compiler);
        return generatedLua;
    }
}