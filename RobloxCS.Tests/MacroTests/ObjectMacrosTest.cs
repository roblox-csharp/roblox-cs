using RobloxCS.Luau;
using RobloxCS.Macros;

namespace RobloxCS.Tests.MacroTests;

public class ObjectMacrosTest : Base.Generation
{
    [Fact]
    public void Macros_ToString()
    {
        var ast = Generate("(123).ToString();");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<Variable>(statement);

        var expression = ((Variable)statement).Initializer;
        Assert.NotNull(expression);
        
        var call = (Call)expression;
        Assert.NotNull(call.ExpandedByMacro);
        Assert.Equal(MacroKind.ObjectMethod, call.ExpandedByMacro);
        Assert.IsType<IdentifierName>(call.Callee);

        var identifierName = (IdentifierName)call.Callee;
        Assert.Equal("tostring", identifierName.ToString());

        var argument = call.ArgumentList.Arguments.First().Expression;
        Assert.IsType<Parenthesized>(argument);

        var parenthesized = (Parenthesized)argument;
        Assert.IsType<Literal>(parenthesized.Expression);

        var literal = (Literal)parenthesized.Expression;
        Assert.Equal("123", literal.ValueText);
    }
}