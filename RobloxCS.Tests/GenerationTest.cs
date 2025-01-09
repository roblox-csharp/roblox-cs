using Microsoft.CodeAnalysis.CSharp;
using RobloxCS.Luau;

namespace RobloxCS.Tests;

public class GenerationTest
{
    [Fact]
    public void Generates_MultipleVariableDeclarations()
    {
        const string source = """
                              int a = 1,
                                  b = 2,
                                  c = 3;
                              """;
        
        var ast = Generate(source);
        Assert.NotEmpty(ast.Statements);
        
        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);
        
        var variableList = (VariableList)statement;
        Assert.NotEmpty(variableList.Variables);
        Assert.Equal(3, variableList.Variables.Count);

        var index = 1;
        foreach (var variable in variableList.Variables)
            Assert.Equal((index++).ToString(), (variable.Initializer as Literal)?.ValueText);
    }
    [Theory]
    [InlineData("var a = 1;", null, "1")]
    [InlineData("int b = 2;", "number", "2")]
    [InlineData("string foo = \"bar\"", "string", "\"bar\"")]
    [InlineData("bool guh;", "boolean", null)]
    public void Generates_VariableDeclarations(string csharpSource, string? expectedLuauType, string? expectedValueText)
    {
        var ast = Generate(csharpSource);
        Assert.NotEmpty(ast.Statements);
        
        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);
        
        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal(expectedLuauType, variable.Type?.ToString());
        Assert.Equal(expectedValueText, (variable.Initializer as Literal)?.ValueText);
    }
    
    [Theory]
    [InlineData("object abc123;", "abc123")]
    [InlineData("object @bruh;", "bruh")]
    [InlineData("object abc;", "abc")]
    public void Generates_Identifiers(string csharpSource, string expectedLuauIdentifier)
    {
        var ast = Generate(csharpSource);
        Assert.NotEmpty(ast.Statements);
        
        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);
        
        // shit ass C# thinks they're declarations
        var variableList = (VariableList)statement;
        var identifier = variableList.Variables.First().Name;
        Assert.Equal(expectedLuauIdentifier, identifier.Text);
    }
    
    [Theory]
    [InlineData("69.420;", "69.42")]
    [InlineData("420;", "420")]
    [InlineData("\"abc\";", "\"abc\"")]
    [InlineData("'a';", "\"a\"")]
    [InlineData("true;", "true")]
    [InlineData("false;", "false")]
    [InlineData("null;", "nil")]
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