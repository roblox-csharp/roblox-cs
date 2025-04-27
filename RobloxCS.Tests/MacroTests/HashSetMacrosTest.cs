using RobloxCS.Luau;
using RobloxCS.Macros;

namespace RobloxCS.Tests.MacroTests;

public class HashSetMacrosTest : Base.Generation
{
    [Fact]
    public void Macros_Add()
    {
        var ast = Generate("HashSet<int> set = []; set.Add(69);");
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(2).ToList();
        Assert.IsType<Variable>(statements[0]);
        Assert.IsType<Assignment>(statements[1]);
        Assert.IsType<Variable>(statements[2]);
        
        var variable = (Variable)statements[0];
        Assert.Equal("_wasAdded", variable.Name.Text);
        Assert.IsType<BinaryOperator>(variable.Initializer);
        
        var binaryOperator = (BinaryOperator)variable.Initializer;
        Assert.Equal("==", binaryOperator.Operator);
        Assert.IsType<ElementAccess>(binaryOperator.Left);
        Assert.IsType<Literal>(binaryOperator.Right);
        
        var assignment = (Assignment)statements[1];
        Assert.IsType<ElementAccess>(assignment.Target);
        Assert.IsType<Literal>(assignment.Value);
        
        var assignmentValue = (Literal)assignment.Value;
        Assert.Equal("true", assignmentValue.ValueText);
        
        var elementAccess = (ElementAccess)assignment.Target;
        Assert.IsType<IdentifierName>(elementAccess.Expression);
        Assert.IsType<Literal>(elementAccess.Index);
        Assert.Equal("set", elementAccess.Expression.ToString());
        
        var index = (Literal)elementAccess.Index;
        Assert.Equal("69", index.ValueText);

        var expression = ((Variable)statements[2]).Initializer;
        Assert.NotNull(expression);
        Assert.NotNull(expression.ExpandedByMacro);
        Assert.Equal(MacroKind.HashSetMethod, expression.ExpandedByMacro);
        Assert.IsType<IdentifierName>(expression);
        Assert.Equal("_wasAdded", expression.ToString());
    }
    
    [Fact]
    public void Macros_Remove()
    {
        var ast = Generate("HashSet<int> set = []; set.Remove(69);");
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(2).ToList();
        Assert.IsType<Variable>(statements[0]);
        Assert.IsType<Assignment>(statements[1]);
        Assert.IsType<Variable>(statements[2]);

        
        var variable = (Variable)statements[0];
        Assert.Equal("_wasRemoved", variable.Name.Text);
        Assert.IsType<BinaryOperator>(variable.Initializer);
        
        var binaryOperator = (BinaryOperator)variable.Initializer;
        Assert.Equal("~=", binaryOperator.Operator);
        Assert.IsType<ElementAccess>(binaryOperator.Left);
        Assert.IsType<Literal>(binaryOperator.Right);
        
        var assignment = (Assignment)statements[1];
        Assert.IsType<ElementAccess>(assignment.Target);
        Assert.IsType<TypeCast>(assignment.Value);
        
        var typeCast = (TypeCast)assignment.Value;
        Assert.IsType<Literal>(typeCast.Expression);
        
        var assignmentValue = (Literal)typeCast.Expression;
        Assert.Equal("nil", assignmentValue.ValueText);
        
        var elementAccess = (ElementAccess)assignment.Target;
        Assert.IsType<IdentifierName>(elementAccess.Expression);
        Assert.IsType<Literal>(elementAccess.Index);
        Assert.Equal("set", elementAccess.Expression.ToString());
        
        var index = (Literal)elementAccess.Index;
        Assert.Equal("69", index.ValueText);

        var expression = ((Variable)statements[2]).Initializer;
        Assert.NotNull(expression);
        Assert.NotNull(expression.ExpandedByMacro);
        Assert.Equal(MacroKind.HashSetMethod, expression.ExpandedByMacro);
        Assert.IsType<IdentifierName>(expression);
        Assert.Equal("_wasRemoved", expression.ToString());
    }
    
    [Fact]
    public void Macros_Contains()
    {
        var ast = Generate("HashSet<int> set = []; set.Contains(69);");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<Variable>(statement);

        var expression = ((Variable)statement).Initializer;
        Assert.IsType<ElementAccess>(expression);

        var elementAccess = (ElementAccess)expression;
        Assert.NotNull(elementAccess.ExpandedByMacro);
        Assert.Equal(MacroKind.HashSetMethod, elementAccess.ExpandedByMacro);
        Assert.IsType<IdentifierName>(elementAccess.Expression);
        Assert.IsType<Literal>(elementAccess.Index);
        Assert.Equal("set", elementAccess.Expression.ToString());

        var index = (Literal)elementAccess.Index;
        Assert.Equal("69", index.ValueText);
    }
    
    [Fact]
    public void Macros_Clear()
    {
        var ast = Generate("HashSet<int> set = []; set.Clear();");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<ExpressionStatement>(statement);

        var expression = ((ExpressionStatement)statement).Expression;
        Assert.IsType<Call>(expression);

        var call = (Call)expression;
        Assert.NotNull(call.ExpandedByMacro);
        Assert.Equal(MacroKind.HashSetMethod, call.ExpandedByMacro);
        Assert.Single(call.ArgumentList.Arguments);
        Assert.IsType<MemberAccess>(call.Callee);

        var selfArgument = call.ArgumentList.Arguments.First().Expression;
        Assert.IsType<IdentifierName>(selfArgument);
        Assert.Equal("set", selfArgument.ToString());

        var memberAccess = (MemberAccess)call.Callee;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        Assert.Equal("table", memberAccess.Expression.ToString());
        Assert.Equal("clear", memberAccess.Name.ToString());
    }
}