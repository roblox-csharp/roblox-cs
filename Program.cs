using Microsoft.CodeAnalysis.CSharp;
using RobloxCS;

var source = """
void increment(int n = 69)
{
    var x = 1;
    return x;
}
""";

var sourceAST = CSharpSyntaxTree.ParseText(source);
var compiler = CSharpCompilation.Create("RewriteTest", [sourceAST]);
var generator = new LuauGenerator(sourceAST, compiler);
var luauAST = generator.GetLuauAST();
var luau = new RobloxCS.Luau.LuauWriter();

Console.WriteLine(luau.Render(luauAST));