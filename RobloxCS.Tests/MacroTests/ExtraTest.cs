using RobloxCS.Luau;

namespace RobloxCS.Tests.MacroTests;

public class ExtraTest : Base.Generation
{
    [Fact]
    public void CallMacros_Discard_IfUnused()
    {
        var ast = Generate("HashSet<int> set = []; set.Add(69);");
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(2).ToList();
        Assert.IsType<Variable>(statements[2]);
        
        var variable = (Variable)statements[2];
        Assert.Equal("_", variable.Name.ToString());
    }
    
    [Fact]
    public void CallMacros_DoNotDiscard_IfUsed()
    {
        var ast = Generate("HashSet<int> set = []; var abc = set.Add(69);");
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(2).ToList();
        Assert.IsType<VariableList>(statements[2]);
        
        var variableList = (VariableList)statements[2];
        Assert.Equal("abc", variableList.Variables.First().Name.ToString());
    }
}