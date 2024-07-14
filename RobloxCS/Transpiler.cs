using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RobloxCS
{
    public sealed class Transpiler
    {
    
        private List<SyntaxTree> _fileTrees = new List<SyntaxTree>();
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
            var compiler = CompileASTs();
            WriteLuaOutput(compiler);
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
                var tree = TranspilerUtility.ParseTree(fileContents, sourceFile);
                var transformedTree = TranspilerUtility.DebugTransformTree(TranspilerUtility.TransformTree(tree, _config), _config);
                foreach (var diagnostic in transformedTree.GetDiagnostics())
                {
                    Logger.HandleDiagnostic(diagnostic);
                }

                _fileTrees.Add(transformedTree);
            }
        }
        
        private CSharpCompilation CompileASTs()
        {
            var compiler = TranspilerUtility.GetCompiler(_fileTrees, _config);
            foreach (var diagnostic in compiler.GetDiagnostics())
            {
                Logger.HandleDiagnostic(diagnostic);
            }

            return compiler;
        }

        private void WriteLuaOutput(CSharpCompilation compiler)
        {
            var compiledFiles = new List<CompiledFile>();
            foreach (var tree in _fileTrees)
            {
                var luaSource = TranspilerUtility.GenerateLua(tree, compiler, _config);
                var targetPath = tree.FilePath.Replace(_config.SourceFolder, _config.OutputFolder).Replace(".cs", ".lua");
                compiledFiles.Add(new CompiledFile(targetPath, luaSource));
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