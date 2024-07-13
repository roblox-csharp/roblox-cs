using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace RobloxCS
{
    public static class TranspilerUtility
    {
        public static string CleanUpLuaForTests(string luaSource, int? extraLines)
        {
            var lines = luaSource.Split('\n').ToList();
            lines.RemoveRange(0, 3 + (extraLines ?? 0));
            return string.Join('\n', lines);
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

        public static SyntaxTree TransformTree(SyntaxTree cleanTree, ConfigData? config = null)
        {
            config ??= ConfigReader.UnitTestingConfig;
            var transformer = new MainTransformer(cleanTree, config);
            return cleanTree.WithRootAndOptions(transformer.GetRoot(), cleanTree.Options);
        }

        public static SyntaxTree ParseTree(string source, string sourceFile = "TransformerTestFile.cs")
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
                Logger.Error($"Failed to find {Utility.RuntimeAssemblyName}.dll in {Utility.FixPathSep(Path.GetDirectoryName(runtimeLibAssemblyPath))}");
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
