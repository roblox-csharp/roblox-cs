using Microsoft.CodeAnalysis.CSharp;
using RobloxCS;
using RobloxCS.Luau;

var source = """
class A {}
""".Trim();

var references = FileUtility.GetCompilationReferences();
var sourceAST = CSharpSyntaxTree.ParseText(source);
var compiler = CSharpCompilation.Create(
    assemblyName: "RewriteTest",
    syntaxTrees: [sourceAST],
    references
);

// RobloxCS.Utility.PrettyPrint(sourceAST.GetRoot().ChildNodes().First().ChildNodes().First().ChildNodes().First().ChildNodes().Last().ChildNodes().First().ChildNodes().First().ChildNodes().First());
foreach (var diagnostic in compiler.GetDiagnostics())
{
    if (diagnostic.Id == "CS5001") continue;
    Logger.HandleDiagnostic(diagnostic);
}

var generator = new LuauGenerator(sourceAST, compiler);
var luauAST = generator.GetLuauAST();
var luau = new LuauWriter();

Console.WriteLine(luau.Render(luauAST));