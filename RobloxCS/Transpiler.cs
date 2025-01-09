using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RobloxCS.Shared;

namespace RobloxCS;

/// <summary>
/// This class contains everything needed to transpile C# to Luau.
/// In the future this class will not be static and will take in C# source files as well as ConfigData.
/// </summary>
public static class Transpiler
{
    
    
    private static readonly HashSet<string> _ignoredDiagnostics =
    [
        "CS5001" // more than 2 entry points
    ];

    public static string Transpile(string source)
    {
        try
        {
            var tree = ParseSource(source);
            var compiler = TranspilerUtility.GetCompiler([tree], null); // temporary null config!!!1
            foreach (var diagnostic in compiler.GetDiagnostics()
                         .Where(diagnostic => !_ignoredDiagnostics.Contains(diagnostic.Id)))
                Logger.HandleDiagnostic(diagnostic);

            return TranspilerUtility.GenerateLuau(tree, compiler);
        }
        catch (CleanExitException)
        {
            return "";
        }
    }

    private static SyntaxTree ParseSource(string source)
    {
        var tree = TranspilerUtility.ParseTree(source);
        HashSet<Func<SyntaxTree, ConfigData, SyntaxTree>> transformers = [BuiltInTransformers.Main()];
        
        return TranspilerUtility.TransformTree(tree, transformers);
    }
}