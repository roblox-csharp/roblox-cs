using Microsoft.CodeAnalysis.CSharp;
using RobloxCS;
using RobloxCS.Luau;

var source = """
var i = 2;
var abc = i++;
""";

var sourceAST = CSharpSyntaxTree.ParseText(source);
var compiler = CSharpCompilation.Create("RewriteTest", [sourceAST]);
var generator = new LuauGenerator(sourceAST, compiler);
var luauAST = generator.GetLuauAST();
var luau = new LuauWriter();

Console.WriteLine(luau.Render(luauAST));