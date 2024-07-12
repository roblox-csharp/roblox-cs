namespace RobloxCS.Tests
{
    public class CodeGenerator_GenerateLuaShould
    {
        [Fact]
        public void GenerateLua_NamespaceDeclaration_GeneratesRuntimeCalls()
        {

            var cleanedLua = GetCleanLua("namespace TestNamespace { }");
            var expectedLua = "CS.namespace(\"TestNamespace\", function(namespace)\r\nend)";
            Assert.Equal(expectedLua.Trim(), cleanedLua);
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

        [Fact]
        public void GenerateLua_InstanceCreate_Macros()
        {

            var cleanedLua = GetCleanLua("using RobloxRuntime.Classes; var part = Instance.Create<Part>()");
            Assert.Equal("local part = Instance.new(\"Part\")", cleanedLua);
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