using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS
{
    public sealed class Transpiler
    {
    
        private readonly List<SyntaxTree> _fileTrees = new List<SyntaxTree>();
        private readonly ConfigData _config;
        private readonly string _sourceDirectory;
        private readonly string _outDirectory;

        public Transpiler(string inputDirectory)
        {
            _config = ConfigReader.Read(inputDirectory);
            _sourceDirectory = inputDirectory + "/" + _config.SourceFolder;
            _outDirectory = inputDirectory + "/" + _config.OutputFolder;
        }

        public void Transpile()
        {
            ParseSource();
            CompileASTs();
            WriteLuaOutput();
        }

        private void ParseSource()
        {
            if (!Directory.Exists(_sourceDirectory))
            {
                Logger.Error($"Source folder \"{_config.SourceFolder}\" does not exist!");
            }

            var sourceFiles = FileManager.GetSourceFiles(_sourceDirectory);
            foreach (var sourceFile in sourceFiles)
            {
                var fileContents = File.ReadAllText(sourceFile);
                var tree = CSharpSyntaxTree.ParseText(fileContents, null, sourceFile);
                foreach (var diagnostic in tree.GetDiagnostics())
                {
                    Logger.HandleDiagnostic(diagnostic);
                }
                _fileTrees.Add(tree);
            }
        }
        

        private void CompileASTs()
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
            var compiler = CSharpCompilation.Create(
                assemblyName: _config.CSharpOptions.AssemblyName,
                syntaxTrees: _fileTrees,
                references: GetCompilationReferences(),
                options: compilationOptions
            );

            foreach (var diagnostic in compiler.GetDiagnostics())
            {
                Logger.HandleDiagnostic(diagnostic);
            }
        }

        private List<PortableExecutableReference> GetCompilationReferences()
        {
            var rbxcsDirectory = Util.GetRbxcsDirectory();
            var tfm = Util.GetTargetFramework();
            var runtimeLibPath = Util.FixPathSep(string.Join('/', rbxcsDirectory, Util.RuntimeAssemblyName));
            var runtimeLibAssemblyPath = string.Join('/', runtimeLibPath, "bin", "Release", tfm, Util.RuntimeAssemblyName + ".dll");
            if (!File.Exists(runtimeLibAssemblyPath))
            {
                Logger.Error($"Failed to find {Util.RuntimeAssemblyName}.dll in {Util.FixPathSep(Path.GetDirectoryName(runtimeLibAssemblyPath))}");
            }

            var references = new List<PortableExecutableReference>()
            {
                MetadataReference.CreateFromFile(runtimeLibAssemblyPath)
            };

            foreach (var coreLibReference in Util.GetCoreLibReferences())
            {
                references.Add(coreLibReference);
            }
            return references;
        }

        private void WriteLuaOutput()
        {
            var compiledFiles = new List<CompiledFile>();
            foreach (var tree in _fileTrees)
            {
                var codeGenerator = new CodeGenerator(tree.GetRoot(), _config);
                var luaSource = codeGenerator.GenerateLua();
                var targetPath = tree.FilePath.Replace(_config.SourceFolder, _config.OutputFolder).Replace(".cs", ".lua");
                var compiledFile = new CompiledFile(targetPath, luaSource);
                compiledFiles.Add(compiledFile);
            }

            FileManager.WriteCompiledFiles(_outDirectory, compiledFiles);
        }
    }
}