using Microsoft.CodeAnalysis.CSharp;
using RobloxCS.Luau;

namespace RobloxCS.Tests;

public class GenerationTest
{
    [Theory]
    [InlineData("69.420", "69.42")]
    [InlineData("420", "420")]
    [InlineData("\"abc\"", "\"abc\"")]
    [InlineData("'a'", "\"a\"")]
    [InlineData("true", "true")]
    [InlineData("false", "false")]
    [InlineData("null", "nil")]
    public void Generates_Literals(string csharpValueText, string luauValueText)
    {
        var ast = Generate(csharpValueText);
        Assert.NotEmpty(ast.Statements);
        
        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<ExpressionStatement>(statement);
        
        var expressionStatement = (ExpressionStatement)statement;
        Assert.IsType<Literal>(expressionStatement.Expression);
        
        var literal = (Literal)expressionStatement.Expression;
        Assert.Equal(luauValueText, literal.ValueText);
    }

    private static AST Generate(string source)
    {
        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var compiler = TranspilerUtility.GetCompiler([tree], null);
        
        return TranspilerUtility.GetLuauAST(tree, compiler);
    }
}