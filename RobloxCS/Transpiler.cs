using RobloxCS.Shared;

namespace RobloxCS;

/// <summary>
///     This class contains everything needed to transpile C# to Luau.
///     In the future this class will not be static and will take in C# source files as well as ConfigData.
/// </summary>
public static class Transpiler
{
    private static readonly HashSet<string> _ignoredDiagnostics =
    [
        "CS5001" // more than 2 entry points
    ];

    public static string TranspileDirectory(string directoryPath, ConfigData config)
    {
        var rojoProject = RojoReader.ReadFromDirectory(directoryPath, config.RojoProjectName);
        return "";
    }

    public static string TranspileSource(string source, RojoProject? rojoProject, ConfigData? config)
    {
        try
        {
            var file = TranspilerUtility.ParseAndTransformTree(source, rojoProject, config); // temporary
            var compiler = TranspilerUtility.GetCompiler([file.Tree], config);
            foreach (var diagnostic in compiler.GetDiagnostics()
                                               .Where(diagnostic => !_ignoredDiagnostics.Contains(diagnostic.Id)))
                Logger.HandleDiagnostic(diagnostic);

            return TranspilerUtility.GenerateLuau(file, compiler);
        }
        catch (CleanExitException)
        {
            return "";
        }
    }
}