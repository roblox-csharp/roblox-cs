﻿using Microsoft.CodeAnalysis.CSharp;
using RobloxCS;

var source = """
var x = (1 + 2) / 2;
var name = nameof(x);
""";

var sourceAST = CSharpSyntaxTree.ParseText(source);
var compiler = CSharpCompilation.Create("RewriteTest", [sourceAST]);
var generator = new LuauGenerator(sourceAST, compiler);
var luauAST = generator.GetLuauAST();
var luau = new RobloxCS.Luau.LuauWriter();

Console.WriteLine(luau.Render(luauAST));