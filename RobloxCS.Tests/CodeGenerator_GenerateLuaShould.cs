namespace RobloxCS.Tests
{
    public class CodeGenerator_GenerateLuaShould
    {
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

        private string GetCleanLua(string source, int? extraLines)
        {
            var cleanTree = TranspilerUtility.ParseTree(source);
            var transformedTree = TranspilerUtility.TransformTree(cleanTree);
            var compiler = TranspilerUtility.GetCompiler([transformedTree]);
            var generatedLua = TranspilerUtility.GenerateLua(transformedTree, compiler);
            return TranspilerUtility.CleanUpLuaForTests(generatedLua, extraLines);
        }
    }
}