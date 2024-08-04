using Microsoft.CodeAnalysis.CSharp;
using RobloxCS;

var source = """
var ImCool = true;
var table = new
{
    ImCool
};

var table2 = new
{
    table.ImCool
};

var table3 = new
{
    ImTheCoolest = true
};
""";

var sourceAST = CSharpSyntaxTree.ParseText(source);
var compiler = CSharpCompilation.Create("RewriteTest", [sourceAST]);
var generator = new LuauGenerator(sourceAST, compiler);
var luauAST = generator.GetLuauAST();
var luau = new RobloxCS.Luau.LuauWriter();

Console.WriteLine(luau.Render(luauAST));