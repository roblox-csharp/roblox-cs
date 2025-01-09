using System.Linq.Expressions;
using RobloxCS.Luau;

namespace RobloxCS.Tests;

public class GenerationTest
{
    // TODO: uncomment when complex expressions are macro'd
    // [Fact]
    // public void Generates_ExpressionalIncrement()
    // {
    //     var ast = Generate("var x = a++;");
    //     Assert.NotEmpty(ast.Statements);
    //     
    //     var statements = ast.Statements.Skip(1).ToList();
    //     Assert.Equal(3, statements.Count);
    //     
    //     var firstStatement = statements[0];
    //     var secondStatement = statements[1];
    //     var thirdStatement = statements[2];
    //     Assert.IsType<Variable>(firstStatement);
    //     Assert.IsType<ExpressionStatement>(secondStatement);
    //     Assert.IsType<VariableList>(thirdStatement);
    //     
    //     var tempVariable = (Variable)firstStatement;
    //     Assert.Equal("_original", tempVariable.Name.ToString());
    //     Assert.IsType<IdentifierName>(tempVariable.Initializer);
    //     
    //     var tempInitializer = (IdentifierName)tempVariable.Initializer;
    //     Assert.Equal("a", tempInitializer.ToString());
    //     
    //     var expressionStatement = (ExpressionStatement)secondStatement;
    //     Assert.IsType<BinaryOperator>(expressionStatement.Expression);
    //     
    //     var binaryOperator = (BinaryOperator)expressionStatement.Expression;
    //     Assert.IsType<IdentifierName>(binaryOperator.Left);
    //     Assert.IsType<Literal>(binaryOperator.Right);
    //     
    //     var left = (IdentifierName)binaryOperator.Left;
    //     var right = (Literal)binaryOperator.Right;
    //     Assert.Equal("a", left.ToString());
    //     Assert.Equal("1", right.ValueText);
    //     
    //     var variableList = (VariableList)thirdStatement;
    //     Assert.Single(variableList.Variables);
    //     
    //     var variable = variableList.Variables.First();
    //     Assert.Equal("x", variable.Name.ToString());
    //     Assert.IsType<IdentifierName>(variable.Initializer);
    //     
    //     var initializer = (IdentifierName)variable.Initializer;
    //     Assert.Equal("_original", initializer.ToString());
    // }
    
    [Fact]
    public void Generates_Increment()
    {
        var ast = Generate("a++;");
        Assert.NotEmpty(ast.Statements);
        
        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<ExpressionStatement>(statement);
        
        var expressionStatement = (ExpressionStatement)statement;
        Assert.IsType<BinaryOperator>(expressionStatement.Expression);
        
        var binaryOperator = (BinaryOperator)expressionStatement.Expression;
        Assert.IsType<IdentifierName>(binaryOperator.Left);
        Assert.IsType<Literal>(binaryOperator.Right);
        Assert.Equal("+=", binaryOperator.Operator);
        
        var left = (IdentifierName)binaryOperator.Left;
        var right = (Literal)binaryOperator.Right;
        Assert.Equal("a", left.ToString());
        Assert.Equal("1", right.ValueText);
    }
    
    [Fact]
    public void Generates_MemberAssignment()
    {
        const string name = "ParentNamespace";
        const string otherName = "ChildNamespace";
        var ast = Generate($"namespace {name}.{otherName};");
        Assert.NotEmpty(ast.Statements);
        
        var globalStatements = ast.Statements.Skip(1).ToList();
        Assert.IsType<Block>(globalStatements.First());
        
        var block = (Block)globalStatements.First();
        var statements = block.Statements;
        Assert.Equal(5, statements.Count);
        
        var secondStatement = statements.Skip(1).First();
        Assert.IsType<ScopedBlock>(secondStatement);
        
        var nestedBlock = (ScopedBlock)secondStatement;
        Assert.Single(nestedBlock.Statements);
        
        var nestedStatement = nestedBlock.Statements.First();
        Assert.IsType<Block>(nestedStatement);
        
        var doubleNestedBlock = (Block)nestedStatement;
        Assert.Equal(4, doubleNestedBlock.Statements.Count); // 3
        
        var doubleNestedStatement = doubleNestedBlock.Statements.Skip(1).First();
        Assert.IsType<ScopedBlock>(doubleNestedStatement);
            
        var memberAssignment = doubleNestedBlock.Statements.SkipLast(1).Last();
        Assert.IsType<ExpressionStatement>(memberAssignment);
        
        var expressionStatement = (ExpressionStatement)memberAssignment;
        Assert.IsType<Assignment>(expressionStatement.Expression);
        
        var assignment = (Assignment)expressionStatement.Expression;
        Assert.IsType<MemberAccess>(assignment.Target);
        Assert.IsType<IdentifierName>(assignment.Value);

        var memberAccess = (MemberAccess)assignment.Target;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        
        var left = (IdentifierName)memberAccess.Expression;
        var right = (IdentifierName)memberAccess.Name;
        Assert.Equal(name, left.ToString());
        Assert.Equal(otherName, right.ToString());
        
        var value = (IdentifierName)assignment.Value;
        Assert.Equal(otherName, value.ToString());
    }
    
    [Fact]
    public void Generates_GlobalAssignment()
    {
        const string name = "Xyz";
        var ast = Generate($"namespace {name};");
        Assert.NotEmpty(ast.Statements);
        
        var globalStatements = ast.Statements.Skip(1).ToList();
        Assert.IsType<Block>(globalStatements.First());
        
        var block = (Block)globalStatements.First();
        var statements = block.Statements;
        Assert.Equal(5, statements.Count); // 4
        
        var globalAssignment = statements.SkipLast(2).Last();
        Assert.IsType<ExpressionStatement>(globalAssignment);
        
        var expressionStatement = (ExpressionStatement)globalAssignment;
        Assert.IsType<Call>(expressionStatement.Expression);
        
        var call = (Call)expressionStatement.Expression;
        Assert.IsType<MemberAccess>(call.Callee);
        
        var memberAccess = (MemberAccess)call.Callee;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        
        var left = (IdentifierName)memberAccess.Expression;
        var right = (IdentifierName)memberAccess.Name;
        Assert.Equal("CS", left.ToString());
        Assert.Equal("defineGlobal", right.ToString());
        Assert.NotEmpty(call.ArgumentList.Arguments);
        Assert.IsType<Literal>(call.ArgumentList.Arguments.First().Expression);
        Assert.IsType<IdentifierName>(call.ArgumentList.Arguments.Last().Expression);
        
        var nameLiteral = (Literal)call.ArgumentList.Arguments.First().Expression;
        var value = (IdentifierName)call.ArgumentList.Arguments.Last().Expression;
        Assert.Equal($"\"{name}\"", nameLiteral.ValueText);
        Assert.Equal(name, value.ToString());
    }
    
    [Fact]
    public void Generates_Namespaces()
    {
        const string name = "MyNamespace";
        var ast = Generate($"namespace {name} {{ enum Abc {{ A }} }}");
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
        Assert.Equal(name, initialDeclaration.Name.ToString());
        
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
        Assert.Equal(name, typeAlias.Name.ToString());
        Assert.IsType<TypeOfCall>(typeAlias.Type);
    }
    
    [Fact]
    public void Generates_Enums()
    {
        const string name = "Abc";
        var ast = Generate($"enum {name} {{ A, B, C = 5, D, E = 10, F }}");
        Assert.NotEmpty(ast.Statements);
        
        var globalStatements = ast.Statements.Skip(1).ToList();
        Assert.IsType<Block>(globalStatements.First());
        
        var block = (Block)globalStatements.First();
        var statements = block.Statements;
        Assert.Equal(4, statements.Count);
        Assert.IsType<Variable>(statements[0]);
        
        var variable = (Variable)statements[0];
        Assert.Equal(name, variable.Name.ToString());
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
            
            var keyName = (IdentifierName)actualEntry.Key;
            Assert.Equal(expectedKey, keyName.ToString());
            
            var literal = (Literal)actualEntry.Value;
            Assert.Equal(expectedValueText, literal.ValueText);
        }
        
        Assert.IsType<ExpressionStatement>(statements[1]);
        
        var expressionStatement = (ExpressionStatement)statements[1];
        Assert.IsType<Call>(expressionStatement.Expression);
        
        Assert.IsType<TypeAlias>(statements[2]);
        var typeAlias = (TypeAlias)statements[2];
        Assert.Equal(name, typeAlias.Name.ToString());
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