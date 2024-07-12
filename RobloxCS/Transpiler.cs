using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

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
            var compilation = CompileASTs();
            WriteLuaOutput(compilation);
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
                var cleanTree = CSharpSyntaxTree.ParseText(fileContents);
                var compilationUnit = (CompilationUnitSyntax)cleanTree.GetRoot();
                var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));
                var newRoot = compilationUnit.AddUsings(usingDirective);
                var tree = cleanTree
                    .WithRootAndOptions(newRoot, cleanTree.Options)
                    .WithFilePath(sourceFile);

                var transformer = new Transformer(tree, _config);
                var transformedTree = tree.WithRootAndOptions(transformer.GetRoot(), cleanTree.Options);
                foreach (var diagnostic in transformedTree.GetDiagnostics())
                {
                    Logger.HandleDiagnostic(diagnostic);
                }

                _fileTrees.Add(transformedTree);
            }
        }
        

        private CSharpCompilation CompileASTs()
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
            var compilation = CSharpCompilation.Create(
                assemblyName: _config.CSharpOptions.AssemblyName,
                syntaxTrees: _fileTrees,
                references: GetCompilationReferences(),
                options: compilationOptions
            );

            foreach (var diagnostic in compilation.GetDiagnostics())
            {
                Logger.HandleDiagnostic(diagnostic);
            }

            return compilation;
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

            foreach (var coreLibReference in GetCoreLibReferences())
            {
                references.Add(coreLibReference);
            }
            return references;
        }

        private List<PortableExecutableReference> GetCoreLibReferences()
        {
            var coreLib = typeof(object).GetTypeInfo().Assembly.Location;
            var systemRuntime = Path.Combine(Path.GetDirectoryName(coreLib)!, "System.Runtime.dll");
            var systemConsole = Path.Combine(Path.GetDirectoryName(coreLib)!, "System.Console.dll");
            return new List<PortableExecutableReference>
            {
                MetadataReference.CreateFromFile(coreLib),
                MetadataReference.CreateFromFile(systemRuntime),
                MetadataReference.CreateFromFile(systemConsole)
            };
        }

        private void WriteLuaOutput(CSharpCompilation compilation)
        {
            var compiledFiles = new List<CompiledFile>();
            foreach (var tree in _fileTrees)
            {
                var codeGenerator = new CodeGenerator(tree, compilation, _config);
                var luaSource = codeGenerator.GenerateLua();
                var targetPath = tree.FilePath.Replace(_config.SourceFolder, _config.OutputFolder).Replace(".cs", ".lua");
                var compiledFile = new CompiledFile(targetPath, luaSource);
                compiledFiles.Add(compiledFile);
            }

            EnsureDirectoriesExist();
            FileManager.WriteCompiledFiles(_outDirectory, compiledFiles);
        }

        private void EnsureDirectoriesExist()
        {
            var subDirectories = Directory.GetDirectories(_sourceDirectory, "*", SearchOption.AllDirectories);
            foreach (string subDirectory in subDirectories)
            {
                Directory.CreateDirectory(subDirectory.Replace(_config.SourceFolder, _config.OutputFolder));
            }
        }
    }
}