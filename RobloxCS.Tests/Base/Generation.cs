using RobloxCS.Luau;

namespace RobloxCS.Tests.Base;

public abstract class Generation
{
    protected static AST Generate(string source)
    {
        var tree = TranspilerUtility.ParseAndTransformTree(source, null);
        var compiler = TranspilerUtility.GetCompiler([tree], null);
        
        return TranspilerUtility.GetLuauAST(tree, compiler);
    }
}