using Microsoft.CodeAnalysis.CSharp;
using RobloxCS;

var source = """
foreach (var x in abc)
{

}

foreach (var (a, b) in xyz)
{

}
""";

var sourceAST = CSharpSyntaxTree.ParseText(source);
var compiler = CSharpCompilation.Create("RewriteTest", [sourceAST]);
var generator = new LuauGenerator(sourceAST, compiler);
var luauAST = generator.GetLuauAST();
var luau = new RobloxCS.Luau.LuauWriter();

Console.WriteLine(luau.Render(luauAST));