namespace RobloxCS.Tests
{
    public class CodeGenerator_GenerateLuaShould
    {
        [Fact]
        public void GenerateLua_NamespaceDeclaration_GeneratesRuntimeCalls()
        {

            var cleanedLua = GetCleanLua("namespace Test { }");
            var expectedLua = "CS.namespace(\"Test\", function(namespace)\r\nend)";
            Assert.Equal(expectedLua.Trim(), cleanedLua);
        }

        [Fact]
        public void GenerateLua_NestedNamespaceDeclaration_GeneratesRuntimeCalls()
        {

            var cleanedLua = GetCleanLua("namespace Test.Nested { }");
            var lines = GetLines(cleanedLua);
            var expectedLines = new List<string>
            {
                "CS.namespace(\"Test\", function(namespace)",
                    "namespace:namespace(\"Nested\", function(namespace)",
                    "end)",
                "end)"
            };

            AssertEqualLines(lines, expectedLines);
        }

        [Fact]
        public void GenerateLua_ClassDeclaration_GeneratesRuntimeCalls()
        {
            var cleanedLua = GetCleanLua("namespace Test { class HelloWorld { } }");
            var lines = GetLines(cleanedLua);
            var expectedLines = new List<string>
            {
                "CS.namespace(\"Test\", function(namespace)",
                    "namespace:class(\"HelloWorld\", function(namespace)",
                        "local class = {}",
                        "class.__index = class",
                        "",
                        "function class.new()",
                            "local self = setmetatable({}, class)",
                            "",
                            "",
                            "",
                            "return self",
                        "end",
                        "",
                        "return setmetatable({}, class)",
                    "end)",
                "end)"
            };

            AssertEqualLines(lines, expectedLines);
        }

        private static void AssertEqualLines(List<string> lines, List<string> expectedLines)
        {
            foreach (var line in lines)
            {
                var expectedLine = expectedLines.ElementAt(lines.IndexOf(line));
                Assert.Equal(expectedLine, line);
            }
        }

        [Theory]
        [InlineData("Vector3")]
        [InlineData("NumberRange")]
        [InlineData("BrickColor")]
        [InlineData("Instance")]
        public void GenerateLua_RobloxType_DoesNotGenerateGetAssemblyTypeCall(string robloxType)
        {

            var cleanedLua = GetCleanLua($"using RobloxRuntime; using RobloxRuntime.Classes; {robloxType}.a;");
            Assert.Equal(robloxType + ".a", cleanedLua);
        }

        [Theory]
        [InlineData("Instance")]
        [InlineData("RobloxRuntime.Classes.Instance")]
        public void GenerateLua_InstanceCreate_Macros(string instanceClassPath)
        {
            var cleanedLua = GetCleanLua($"using RobloxRuntime.Classes; var part = {instanceClassPath}.Create<Part>()");
            Assert.Equal("local part = Instance.new(\"Part\")", cleanedLua);
        }

        [Fact]
        public void GenerateLua_InstanceIsA_Macros()
        {
            var cleanedLua = GetCleanLua("using RobloxRuntime.Classes; var part = Instance.Create<Part>(); part.IsA<Frame>();", 1);
            Assert.Equal("part:IsA(\"Frame\")", cleanedLua);
        }

        [Theory]
        [InlineData("Console.Write")]
        [InlineData("Console.WriteLine")]
        [InlineData("System.Console.Write")]
        [InlineData("System.Console.WriteLine")]
        public void GenerateLua_ConsoleMethods_Macro(string fullMethodPath)
        {
            var cleanedLua = GetCleanLua($"{fullMethodPath}(\"hello world\")");
            Assert.Equal("print(\"hello world\")", cleanedLua);
        }

        [Fact]
        public void GenerateLua_StaticClass_NoFullQualification()
        {
            var cleanedLua = GetCleanLua($"RobloxRuntime.Globals.game");
            Assert.Equal("game", cleanedLua);
        }

        [Theory]
        [InlineData("object obj; obj?.Name;")]
        [InlineData("object a; a.b?.c;")]
        public void GenerateLua_SafeNavigation_GeneratesIfStatement(string source)
        {
            var cleanedLua = GetCleanLua(source, 1);
            switch (source)
            {
                case "object obj; obj?.Name;":
                    Assert.Equal("if obj == nil then nil else obj.Name", cleanedLua);
                    break;
                case "object a; a.b?.c;":
                    Assert.Equal("if a.b == nil then nil else a.b.c", cleanedLua);
                    break;
            }
        }

        [Fact]
        public void GenerateLua_NullCoalescing_GeneratesIfStatement()
        {
            
            var cleanedLua = GetCleanLua("int? x; int? y; x ?? y", 2);
            Assert.Equal("if x == nil then y else x", cleanedLua);
        }

        [Fact]
        public void GenerateLua_TupleExpression_GeneratesTable()
        {
            var cleanedLua = GetCleanLua("var tuple = (1, 2, 3)");
            Assert.Equal("local tuple = {1, 2, 3}", cleanedLua);
        }

        [Fact]
        public void GenerateLua_TupleIndexing_GeneratesTableIndexing()
        {
            var cleanedLua = GetCleanLua("var tuple = (1, 2, 3);\ntuple.Item1;\ntuple.Item2;\ntuple.Item3;\n", 1);
            var lines = GetLines(cleanedLua);
            Assert.Equal("tuple[1]", lines[0]);
            Assert.Equal("tuple[2]", lines[1]);
            Assert.Equal("tuple[3]", lines[2]);
        }

        private List<string> GetLines(string cleanLua)
        {
            return cleanLua.Split('\n').Select(line => line.Trim()).ToList();
        }

        private string GetCleanLua(string source, int extraLines = 0)
        {
            var cleanTree = TranspilerUtility.ParseTree(source);
            var transformedTree = TranspilerUtility.TransformTree(cleanTree);
            var compiler = TranspilerUtility.GetCompiler([transformedTree]);
            var generatedLua = TranspilerUtility.GenerateLua(transformedTree, compiler);
            return TranspilerUtility.CleanUpLuaForTests(generatedLua, extraLines);
        }
    }
}