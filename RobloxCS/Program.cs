using Microsoft.CodeAnalysis.CSharp;
using RobloxCS;
using RobloxCS.Luau;

var source = """
var intType = typeof(int);
""".Trim();

var references = FileUtility.GetCompilationReferences();
var sourceAST = CSharpSyntaxTree.ParseText(source);
var compiler = CSharpCompilation.Create(
    assemblyName: "RewriteTest",
    syntaxTrees: [sourceAST],
    references
);

foreach (var diagnostic in compiler.GetDiagnostics())
{
    Logger.HandleDiagnostic(diagnostic);
}

var generator = new LuauGenerator(sourceAST, compiler);
var luauAST = generator.GetLuauAST();
var luau = new LuauWriter();

Console.WriteLine(luau.Render(luauAST));