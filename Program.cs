using Microsoft.CodeAnalysis.CSharp;
using RobloxCS;

var source = """
Func<int, int> action = delegate (int b)
{
    var x = 1 + b;
    return x;
};
""";

var sourceAST = CSharpSyntaxTree.ParseText(source);
var compiler = CSharpCompilation.Create("RewriteTest", [sourceAST]);
var generator = new LuauGenerator(sourceAST, compiler);
var luauAST = generator.GetLuauAST();
var luau = new RobloxCS.Luau.LuauWriter();

Console.WriteLine(luau.Render(luauAST));