using RobloxCS.Shared;
using Path = System.IO.Path;

namespace RobloxCS;

/// <summary>This class contains everything needed to transpile C# to Luau.</summary>
public static class Transpiler
{
    private static readonly HashSet<string> _ignoredDiagnostics = [];

    public static void Transpile(string directoryPath, ConfigData config, bool verbose)
    {
        var rojoProject = RojoReader.ReadFromDirectory(directoryPath, config.RojoProjectName);
        if (rojoProject == null)
            throw Logger.Error("Rojo project name 'UNIT_TESTING' is reserved for internal use.");

        var sourceDirectory = Path.Join(directoryPath, config.SourceFolder);
        var outputDirectory = Path.Join(directoryPath, config.OutputFolder);
        var sourceFilePaths = Directory.GetFiles(sourceDirectory,
                                                        "*.cs",
                                                        SearchOption.AllDirectories);

        foreach (var sourcePath in sourceFilePaths)
        {
            var outputPath = sourcePath.Replace(".cs", ".luau").Replace(sourceDirectory, outputDirectory);
            if (verbose)
                Console.WriteLine($"Transpiling '{Path.GetRelativePath(directoryPath, sourcePath)}' into '{Path.GetRelativePath(directoryPath, outputPath)}'...");
            
            // TODO: collect source files & create one single CSharpCompilation instead of one per file
            var csharpSource = File.ReadAllText(sourcePath);
            var transpiledLuau = TranspileSource(csharpSource, rojoProject, config);
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            
            File.WriteAllText(outputPath, transpiledLuau);
        }
    }

    public static string TranspileSource(string source, RojoProject rojoProject, ConfigData config)
    {
        try
        {
            var file = TranspilerUtility.ParseAndTransformTree(source, rojoProject, config);
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