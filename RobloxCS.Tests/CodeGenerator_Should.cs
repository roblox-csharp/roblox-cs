namespace RobloxCS.Tests
{
    public class CodeGenerator_Should
    {
        [Theory]
        [InlineData("var @abc = 1")]
        [InlineData("var @hello_brah = 1")]
        [InlineData("var @SIGMA = 1")]
        public void IdentifierWithAtSymbol_HasAtSymbolRemoved(string expression)
        {
            var cleanedLua = GetCleanLua(expression);
            Assert.Equal(expression.Replace("@", "").Replace("var", "local"), cleanedLua);
        }

        [Theory]
        [InlineData("ToNumber(\"69\")")]
        [InlineData("ToFloat(\"69\")")]
        [InlineData("ToDouble(\"69\")")]
        [InlineData("ToInt(\"69\")")]
        [InlineData("ToUInt(\"69\")")]
        [InlineData("ToShort(\"69\")")]
        [InlineData("ToUShort(\"69\")")]
        [InlineData("ToByte(\"69\")")]
        [InlineData("ToSByte(\"69\")")]
        public void NumberParsingMethods_MacroToToNumber(string methodCall)
        {
            var cleanedLua = GetCleanLua(methodCall);
            Assert.Equal("tonumber(\"69\")", cleanedLua);
        }

        [Fact]
        public void NamespaceDeclaration_GeneratesRuntimeCalls()
        {

            var cleanedLua = GetCleanLua("namespace Test { }");
            var expectedLua = "CS.namespace(\"Test\", function(namespace)\nend)";
            Assert.Equal(expectedLua, cleanedLua);
        }

        [Fact]
        public void NestedNamespaceDeclaration_GeneratesRuntimeCalls()
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
        public void ClassDeclaration_GeneratesRuntimeCalls()
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
                            "self.mt = {}",
                            "",
                            "",
                            "return setmetatable(self, self.mt)",
                        "end",
                        "",
                        "return setmetatable({}, class)",
                    "end)",
                "end)"
            };

            AssertEqualLines(lines, expectedLines);
        }

        [Theory]
        [InlineData("var x = 5;", "local x = 5")]
        [InlineData("char f = 'f'", "local f = \"f\"")]
        [InlineData("object a = 123", "local a = 123")]
        public void VariableDeclaration_GeneratesLocal(string input, string expected)
        {
            var cleanedLua = GetCleanLua(input);
            Assert.Equal(expected, cleanedLua);
        }

        [Theory]
        [InlineData("69", "69")]
        [InlineData("420.0f", "420")]
        [InlineData("'h'", "\"h\"")]
        [InlineData("\"abcefg\"", "\"abcefg\"")]
        [InlineData("true", "true")]
        [InlineData("false", "false")]
        [InlineData("null", "nil")]
        public void Literal_GeneratesLiteral(string input, string expected)
        {
            var cleanedLua = GetCleanLua(input);
            Assert.Equal(expected, cleanedLua);
        }

        [Fact]
        public void StringInterpolation_GeneratesInterpolation()
        {
            var cleanedLua = GetCleanLua("int count = 6; $\"count: {count}\"", 1);
            var expectedLua = "`count: {count}`";
            Assert.Equal(expectedLua, cleanedLua);
        }

        [Theory]
        [InlineData("Vector3")]
        [InlineData("NumberRange")]
        [InlineData("BrickColor")]
        [InlineData("Instance")]
        public void RobloxType_DoesNotGenerateGetAssemblyTypeCall(string robloxType)
        {

            var cleanedLua = GetCleanLua($"using {Utility.RuntimeAssemblyName}; {robloxType}.a;");
            Assert.Equal(robloxType + ".a", cleanedLua);
        }

        [Theory]
        [InlineData("Instance")]
        [InlineData($"{Utility.RuntimeAssemblyName}.Instance")]
        public void InstanceCreate_Macros(string instanceClassPath)
        {
            var cleanedLua = GetCleanLua($"using {Utility.RuntimeAssemblyName}; var part = {instanceClassPath}.Create<Part>()");
            Assert.Equal("local part = Instance.new(\"Part\")", cleanedLua);
        }

        [Fact]
        public void InstanceIsA_Macros()
        {
            var cleanedLua = GetCleanLua($"using {Utility.RuntimeAssemblyName}; var part = Instance.Create<Part>(); part.IsA<Frame>();", 1);
            Assert.Equal("part:IsA(\"Frame\")", cleanedLua);
        }

        [Theory]
        [InlineData("Console.Write")]
        [InlineData("Console.WriteLine")]
        [InlineData("System.Console.Write")]
        [InlineData("System.Console.WriteLine")]
        public void ConsoleMethods_Macro(string fullMethodPath)
        {
            var cleanedLua = GetCleanLua($"{fullMethodPath}(\"hello world\")");
            Assert.Equal("print(\"hello world\")", cleanedLua);
        }

        [Theory]
        [InlineData("game")]
        [InlineData("script")]
        [InlineData("os")]
        [InlineData("task")]
        public void StaticClass_NoFullQualification(string memberName)
        {
            var cleanedLua = GetCleanLua($"{Utility.RuntimeAssemblyName}.Globals.{memberName}");
            Assert.Equal(memberName, cleanedLua);
        }

        [Theory]
        [InlineData("object obj; obj?.Name;")]
        [InlineData("object a; a.b?.c;")]
        public void SafeNavigation_GeneratesIfStatement(string source)
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
        public void NullCoalescing_GeneratesIfStatement()
        {
            
            var cleanedLua = GetCleanLua("int? x; int? y; x ?? y", 2);
            Assert.Equal("if x == nil then y else x", cleanedLua);
        }

        [Fact]
        public void TupleExpression_GeneratesTable()
        {
            var cleanedLua = GetCleanLua("var tuple = (1, 2, 3)");
            Assert.Equal("local tuple = {1, 2, 3}", cleanedLua);
        }

        [Fact]
        public void TupleIndexing_GeneratesTableIndexing()
        {
            var cleanedLua = GetCleanLua("var tuple = (1, 2, 3);\ntuple.Item1;\ntuple.Item2;\ntuple.Item3;\n", 1);
            var lines = GetLines(cleanedLua);
            Assert.Equal("tuple[1]", lines[0]);
            Assert.Equal("tuple[2]", lines[1]);
            Assert.Equal("tuple[3]", lines[2]);
        }

        [Fact]
        public void CollectionInitializer_GeneratesTable()
        {
            var cleanedLua = GetCleanLua("int[] nums = [1, 2, 3]");
            Assert.Equal("local nums = {1, 2, 3}", cleanedLua);
        }

        [Theory]
        [InlineData("int[] nums = [1, 2, 3]; nums[0]", "nums[1]", 1)]
        [InlineData("int[] nums = [1, 2, 3]; int i = 4; nums[i]", "nums[i + 1]", 2)]
        public void CollectionIndexing_AddsOneToNumericalIndices(string input, string expected, int removeLines)
        {
            var cleanedLua = GetCleanLua(input, removeLines);
            Assert.Equal(expected, cleanedLua);
        }

        private static void AssertEqualLines(List<string> lines, List<string> expectedLines)
        {
            foreach (var line in lines)
            {
                var expectedLine = expectedLines.ElementAt(lines.IndexOf(line));
                Assert.Equal(expectedLine, line);
            }
        }

        private List<string> GetLines(string cleanLua)
        {
            return cleanLua.Split('\n').Select(line => line.Trim()).ToList();
        }

        private string GetCleanLua(string source, int extraLines = 0)
        {
            var cleanTree = TranspilerUtility.ParseTree(source);
            var transformedTree = TranspilerUtility.TransformTree(cleanTree, [TransformFactory.Main()]);
            var compiler = TranspilerUtility.GetCompiler([transformedTree]);
            var memberCollector = new MemberCollector([cleanTree]);
            var generatedLua = TranspilerUtility.GenerateLua(transformedTree, compiler, memberCollector.Collect());
            return TranspilerUtility.CleanUpLuaForTests(generatedLua, extraLines);
        }
    }
}