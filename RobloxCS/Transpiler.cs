using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS;

/// <summary>
/// This class contains everything needed to transpile C# to Luau.
/// In the future this class will not be static and will take in C# source files as well as ConfigData.
/// </summary>
public static class Transpiler
{
    private const string _includeFolderName = "Include";
    private static readonly HashSet<string> _ignoredDiagnostics =
    [
        "CS5001" // more than 2 entry points
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