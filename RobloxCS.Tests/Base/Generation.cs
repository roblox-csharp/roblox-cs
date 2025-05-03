using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Tests.Base;

public abstract class Generation
{
    protected static AST Generate(string source)
    {
        var config = ConfigReader.UnitTestingConfig;
        var file = TranspilerUtility.ParseAndTransformTree(source.Trim(), new RojoProject(), config);
        var compiler = TranspilerUtility.GetCompiler([file.Tree], config);

        return TranspilerUtility.GetLuauAST(file, compiler);
    }
}