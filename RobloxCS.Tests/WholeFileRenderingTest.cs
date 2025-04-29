using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Tests;

public class WholeFileRenderingTest
{
    [Fact]
    public void Renders_BasicFunctionsAndVariables()
    {
        const string source = """
                              var n = 42;
                              var doubled = DoSomethingElse(n);
                              var mainDoubled = DoSomething();
                              
                              int DoSomething() => DoSomethingElse(69);
                              int DoSomethingElse(int x)
                              {
                                print("x:", x);
                                return x * 2;
                              }
                              """;
        
        const string expectedOutput = """
                                      local function DoSomethingElse(x: number): number
                                        print("x:", x)
                                        return x * 2
                                      end
                                      local function DoSomething(): number
                                        return DoSomethingElse(69)
                                      end
                                      local n = 42
                                      local doubled = DoSomethingElse(n)
                                      local mainDoubled = DoSomething()
                                      return nil
                                      """;

        var output = Emit(source);
        Assert.Equal(expectedOutput.Replace("\r", ""), string.Join('\n', output.Replace("\r", "").Split('\n').Skip(1)).Trim());
    }
    
    private static string Emit(string source)
    {
      var config = new ConfigData(); // ConfigReader.UnitTestingConfig;
      var file = TranspilerUtility.ParseAndTransformTree(source.Trim(), new RojoProject(), config);
      var compiler = TranspilerUtility.GetCompiler([file.Tree], config);
      var ast = TranspilerUtility.GetLuauAST(file, compiler);
      var writer = new LuauWriter();
      ast.Render(writer);

      return writer.ToString();
    }
}