using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    public static class TranspilerUtility
    {
        public static string CleanUpLuaForTests(string luaSource, int? extraLines)
        {
            var debugExtraLines = Utility.IsDebug() ? 1 : 0;
            var lines = luaSource.Split('\n').ToList();
            lines.RemoveRange(0, 2 + (extraLines ?? 0) + debugExtraLines);
            return string.Join('\n', lines)
                .Replace("\r", "");
        }

        public static string GenerateLua(SyntaxTree tree, CSharpCompilation compiler, ConfigData? config = null)
        {
            config ??= ConfigReader.UnitTestingConfig;
            var codeGenerator = new CodeGenerator(tree, compiler, config);
            return codeGenerator.GenerateLua();
        }

        public static CSharpCompilation GetCompiler(List<SyntaxTree> trees, ConfigData? config = null)
        {
            config ??= ConfigReader.UnitTestingConfig;
            var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
            var compiler = CSharpCompilation.Create(
                assemblyName: config.CSharpOptions.AssemblyName,
                syntaxTrees: trees,
                references: GetCompilationReferences(),
                options: compilationOptions
            );

            return compiler;
        }

        public static SyntaxTree TransformTree(SyntaxTree cleanTree, List<Func<SyntaxTree, ConfigData, SyntaxTree>> transformMethods, ConfigData? config = null)
        {
            config ??= ConfigReader.UnitTestingConfig;

            var tree = cleanTree;
            foreach (var transform in transformMethods)
            {
                tree = transform(tree, config);
            }
            return tree;
        }

        public static SyntaxTree ParseTree(string source, string sourceFile = "TestFile.cs")
        {
            var cleanTree = CSharpSyntaxTree.ParseText(source);
            var compilationUnit = (CompilationUnitSyntax)cleanTree.GetRoot();
            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));
            var newRoot = compilationUnit.AddUsings(usingDirective);
            return cleanTree
                .WithRootAndOptions(newRoot, cleanTree.Options)
                .WithFilePath(sourceFile);
        }

        private static List<PortableExecutableReference> GetCompilationReferences()
        {
            var rbxcsDirectory = Utility.GetRbxcsDirectory();
            var tfm = Utility.GetTargetFramework();
            var runtimeLibPath = Utility.FixPathSep(string.Join('/', rbxcsDirectory, Utility.RuntimeAssemblyName));
            var runtimeLibAssemblyPath = string.Join('/', runtimeLibPath, "bin", "Release", tfm, Utility.RuntimeAssemblyName + ".dll");
            if (!File.Exists(runtimeLibAssemblyPath))
            {
                var directoryName = Path.GetDirectoryName(runtimeLibAssemblyPath);
                Logger.Error($"Failed to find {Utility.RuntimeAssemblyName}.dll in {(directoryName == null ? "(could not find assembly directory)" : Utility.FixPathSep(directoryName))}");
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

        private static List<PortableExecutableReference> GetCoreLibReferences()
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
    }
}
