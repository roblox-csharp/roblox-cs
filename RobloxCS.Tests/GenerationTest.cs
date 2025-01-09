using RobloxCS.Luau;

namespace RobloxCS.Tests;

public class GenerationTest
{
    [Fact]
    public void Generates_Namespaces()
    {
        var ast = Generate("namespace MyNamespace { enum Abc { A } }");
        Assert.NotEmpty(ast.Statements);
        
        var globalStatements = ast.Statements.Skip(1).ToList();
        Assert.IsType<Block>(globalStatements.First());
        
        var block = (Block)globalStatements.First();
        var statements = block.Statements;
        Assert.Equal(5, statements.Count);
        
        var firstStatement = statements[0];
        var secondStatement = statements[1];
        var thirdStatement = statements[2];
        var fourthStatement = statements[3];
        Assert.IsType<Variable>(firstStatement);
        Assert.IsType<ScopedBlock>(secondStatement);
        Assert.IsType<ExpressionStatement>(thirdStatement);
        Assert.IsType<TypeAlias>(fourthStatement);
        
        var initialDeclaration = (Variable)firstStatement;
        Assert.Null(initialDeclaration.Type);
        Assert.IsType<TableInitializer>(initialDeclaration.Initializer);
        Assert.Equal("MyNamespace", initialDeclaration.Name.ToString());
        
        var scopedBlock = (ScopedBlock)secondStatement;
        Assert.NotEmpty(scopedBlock.Statements);
        Assert.IsType<Block>(scopedBlock.Statements.First());
            
        var enumBlock = (Block)scopedBlock.Statements.First();
        Assert.Equal(3, enumBlock.Statements.Count);
        Assert.IsType<Variable>(enumBlock.Statements[0]);
        Assert.IsType<ExpressionStatement>(enumBlock.Statements[1]);
        Assert.IsType<Assignment>(((ExpressionStatement)enumBlock.Statements[1]).Expression);
        Assert.IsType<TypeAlias>(enumBlock.Statements[2]);
        
        var expressionStatement = (ExpressionStatement)thirdStatement;
        Assert.IsType<Call>(expressionStatement.Expression);

        var typeAlias = (TypeAlias)fourthStatement;
        Assert.Equal("MyNamespace", typeAlias.Name.ToString());
        Assert.IsType<TypeOfCall>(typeAlias.Type);
    }
    
    [Fact]
    public void Generates_Enums()
    {
        var ast = Generate("enum Abc { A, B, C = 5, D, E = 10, F }");
        Assert.NotEmpty(ast.Statements);
        
        var globalStatements = ast.Statements.Skip(1).ToList();
        Assert.IsType<Block>(globalStatements.First());
        
        var block = (Block)globalStatements.First();
        var statements = block.Statements;
        Assert.Equal(4, statements.Count);
        Assert.IsType<Variable>(statements[0]);
        
        var variable = (Variable)statements[0];
        Assert.Equal("Abc", variable.Name.ToString());
        Assert.IsType<TableInitializer>(variable.Initializer);

        var expectedTable = new Dictionary<string, string>()
        {
            { "A", "0" },
            { "B", "1" },
            { "C", "5" },
            { "D", "6" },
            { "E", "10" },
            { "F", "11" },
        };
        
        var table = (TableInitializer)variable.Initializer;
        Assert.Equal(expectedTable.Count, table.Entries.Count);
        Assert.True(table.TreatIdentifiersAsKeyNames);
        
        var index = 0;
        foreach (var (expectedKey, expectedValueText) in expectedTable)
        {
            var actualEntry = table.Entries.ElementAtOrDefault(index++);
            Assert.IsType<IdentifierName>(actualEntry.Key);
            Assert.IsType<Literal>(actualEntry.Value);
            
            var name = (IdentifierName)actualEntry.Key;
            Assert.Equal(expectedKey, name.ToString());
            
            var literal = (Literal)actualEntry.Value;
            Assert.Equal(expectedValueText, literal.ValueText);
        }
        
        Assert.IsType<ExpressionStatement>(statements[1]);
        
        var expressionStatement = (ExpressionStatement)statements[1];
        Assert.IsType<Call>(expressionStatement.Expression);
        
        Assert.IsType<TypeAlias>(statements[2]);
        var typeAlias = (TypeAlias)statements[2];
        Assert.Equal("Abc", typeAlias.Name.ToString());
        Assert.IsType<IndexCall>(typeAlias.Type);
        
        Assert.IsType<NoOp>(statements[3]);
    }
    
    [Theory]
    [InlineData("int getInt() => 69;")]
    [InlineData("""
                int getInt()
                {
                  return 69;
                }
                """)]
    public void Generates_LocalFunctions(string csharpSource)
    {
        var ast = Generate(csharpSource);
        Assert.NotEmpty(ast.Statements);
        
        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<Function>(statement);
        
        var function = (Function)statement;
        Assert.StartsWith("getInt", function.Name.ToString()); // change to Equal() when duplicate identifier handling is fixed
        Assert.NotNull(function.ReturnType);
        Assert.Equal("number", function.ReturnType.ToString());
        Assert.Empty(function.ParameterList.Parameters);
        Assert.NotNull(function.Body);
        Assert.NotEmpty(function.Body.Statements);

        var bodyStatement = function.Body.Statements.First();
        Assert.IsType<Return>(bodyStatement);
        
        var returnStatement = (Return)bodyStatement;
        Assert.IsType<Literal>(returnStatement.Expression);
        
        var literal = (Literal)returnStatement.Expression;
        Assert.Equal("69", literal.ValueText);
    }
    
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
        var tree = TranspilerUtility.ParseAndTransformTree(source, null);
        var compiler = TranspilerUtility.GetCompiler([tree], null);
        
        return TranspilerUtility.GetLuauAST(tree, compiler);
    }
}