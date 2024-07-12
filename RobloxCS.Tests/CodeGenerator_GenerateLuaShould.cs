namespace RobloxCS.Tests
{
    public class CodeGenerator_GenerateLuaShould
    {
        [Theory]
        [InlineData("object obj; obj?.Name;")]
        [InlineData("object a; a.b?.c;")]
        public void GenerateLua_SafeNavigation_GeneratesIfStatement(string source)
        {
            var cleanTree = TranspilerUtility.ParseTree(source);
            var transformedTree = TranspilerUtility.TransformTree(cleanTree);
            var compiler = TranspilerUtility.GetCompiler([transformedTree]);
            var generatedLua = TranspilerUtility.GenerateLua(transformedTree, compiler);
            var cleanedLua = TranspilerUtility.CleanUpLuaForTests(generatedLua, 4);
            switch (source)
            {
                case "obj?.Name;":
                    Assert.Equal("if obj == nil then nil else obj.Name", cleanedLua);
                    break;
                case "a.b?.c;":
                    Assert.Equal("if a.b == nil then nil else a.b.c", cleanedLua);
                    break;
            }
        }
    }
}