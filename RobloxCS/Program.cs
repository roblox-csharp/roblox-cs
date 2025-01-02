using Microsoft.CodeAnalysis.CSharp;
using RobloxCS;
using RobloxCS.Luau;

var source = """
int a = 1
switch (5) {
    case 4:
        a = 4
    case 5:
        a = 5
        break;
    default:
        a = 10
        break;
}
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