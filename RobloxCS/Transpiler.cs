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

        // this is prettyyy ass
        foreach (var (sourcePath, output) in TranspileSources(sourceFilePaths, rojoProject, config))
        {
            var outputPath = sourcePath.Replace(".cs", ".luau").Replace(sourceDirectory, outputDirectory);
            if (verbose)
                Console.WriteLine($"Transpiling '{Path.GetRelativePath(directoryPath, sourcePath)}' into '{Path.GetRelativePath(directoryPath, outputPath)}'...");
            
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            
            File.WriteAllText(outputPath, output);
        }
    }
    
    public static List<(string Path, string Output)> TranspileSources(IEnumerable<string> sourceFilePaths, RojoProject rojoProject, ConfigData config)
    {
        try
        {
            var files = sourceFilePaths
                .Select(path => (Path: path, Compilation: TranspilerUtility.ParseAndTransformTree(File.ReadAllText(path), rojoProject, config, path)))
                .ToList();
            
            var trees = files.ConvertAll(file => file.Compilation.Tree);
            var compiler = TranspilerUtility.GetCompiler(trees, config);
            var diagnostics = compiler.GetDiagnostics()
                .Where(diagnostic => !_ignoredDiagnostics.Contains(diagnostic.Id));
            
            foreach (var diagnostic in diagnostics)
                Logger.HandleDiagnostic(diagnostic);

            return files.ConvertAll(file => (file.Path, TranspilerUtility.GenerateLuau(file.Compilation, compiler)));
        }
        catch (CleanExitException)
        {
            return [];
        }
    }
}