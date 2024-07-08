using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics;

namespace RobloxCS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var inputDirectory = args.FirstOrDefault(".");

            const string sourceFolderName = "src"; // temporary
            const string outFolderName = "dist"; // temporary
            const string assemblyName = "RobloxCSProject"; // temporary
            const string entryPointName = "Game"; // temporary
            const string mainMethodName = "Main"; // temporary

            var sourceDirectory = inputDirectory + "/" + sourceFolderName;
            var outDirectory = inputDirectory + "/" + outFolderName;
            if (!Directory.Exists(sourceDirectory))
            {
                Logger.Error($"Source folder \"{sourceFolderName}\" does not exist!");
            }

            const string runtimeAssemblyName = "RobloxRuntime";
            var assemblyDirectory = Util.GetAssemblyDirectory();
            var tfm = Util.GetTargetFramework();
            var rbxcsDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(assemblyDirectory)))); // pretend like this isn't here lol
            var runtimeLibPath = Util.FixPathSep(string.Join('/', rbxcsDirectory, runtimeAssemblyName));
            var runtimeLibAssemblyPath = string.Join('/', runtimeLibPath, "bin",  "Release", tfm, runtimeAssemblyName + ".dll");
            var sourceFiles = FileManager.GetSourceFiles(sourceDirectory);
            var compiledFiles = new List<CompiledFile>();
            var fileTrees = new List<SyntaxTree>();
            foreach (var sourceFile in sourceFiles)
            {
                var fileContents = File.ReadAllText(sourceFile);
                var tree = CSharpSyntaxTree.ParseText(fileContents, null, sourceFile);
                foreach (var diagnostic in tree.GetDiagnostics())
                {
                    Logger.HandleDiagnostic(diagnostic);
                }
                fileTrees.Add(tree);
            }

            if (!File.Exists(runtimeLibAssemblyPath))
            {
                Logger.Error($"Failed to find {runtimeAssemblyName}.dll in {Util.FixPathSep(Path.GetDirectoryName(runtimeLibAssemblyPath))}");
            }

            var references = new List<PortableExecutableReference>()
            {
                MetadataReference.CreateFromFile(runtimeLibAssemblyPath)
            };

            foreach (var coreLibReference in Util.GetCoreLibReferences())
            {
                references.Add(coreLibReference);
            }

            var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
            var compiler = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: fileTrees,
                references,
                options: compilationOptions
            );
            foreach (var diagnostic in compiler.GetDiagnostics())
            {
                Logger.HandleDiagnostic(diagnostic);
            }

            foreach (var tree in fileTrees)
            {
                var codeGenerator = new CodeGenerator(tree.GetRoot(), entryPointName, mainMethodName);
                var luaSource = codeGenerator.GenerateLua();
                var targetPath = tree.FilePath.Replace(sourceFolderName, outFolderName).Replace(".cs", ".lua");
                var compiledFile = new CompiledFile(targetPath, luaSource);
                compiledFiles.Add(compiledFile);
            }

            FileManager.WriteCompiledFiles(outDirectory, compiledFiles);
        }
    }
}