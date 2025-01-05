using RobloxCS;

namespace RobloxCS.Tests;

public class RenderingTest
{
    private const string _tab = "  ";
    
    [Fact]
    public void Renders_MappedTypes()
    {
        var mappedType = new Luau.MappedType(new Luau.TypeRef("string"), new Luau.TypeRef("number"));
        var output = Render(mappedType);
        Assert.Equal("{ [string]: number; }", output);
    }
    
    [Fact]
    public void Renders_InterfaceTypes()
    {
        var interfaceType = new Luau.InterfaceType(
            [new Luau.FieldType("myField", new Luau.TypeRef("string"), true)],
            null,
            false
        );
        
        var output = Render(interfaceType);
        const string expectedOutput = """
                                      {
                                        read myField: string;
                                      }
                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }
    
    [Fact]
    public void Renders_OptionalTypes()
    {
        var optionalType = new Luau.OptionalType(new Luau.TypeRef("boolean"));
        var output = Render(optionalType);
        Assert.Equal("boolean?", output);
    }

    [Fact]
    public void Renders_FunctionTypes()
    {
        var functionType = new Luau.FunctionType(
            [new Luau.ParameterType("myParam", new Luau.TypeRef("number"))],
            new Luau.TypeRef("boolean")
        );
        var output = Render(functionType);
        Assert.Equal("(myParam: number) -> boolean", output);
    }

    [Fact]
    public void Renders_ArrayTypes()
    {
        var arrayType = new Luau.ArrayType(new Luau.TypeRef("string"));
        var output = Render(arrayType);
        Assert.Equal("{ string }", output);
    }
    
    [Fact]
    public void Renders_TypeAliases()
    {
        var name = new Luau.IdentifierName("MyType");
        var value = new Luau.TypeRef("string");
        var typeAlias = new Luau.TypeAlias(name, value);
        var output = Render(typeAlias);
        Assert.Equal("type MyType = string\n", output);
    }
    
    [Fact]
    public void Renders_UnaryOperators()
    {
        var operand = new Luau.IdentifierName("isActive");
        var unaryOp = new Luau.UnaryOperator("not ", operand);
        var output = Render(unaryOp);
        Assert.Equal("not isActive", output);
    }
    
    [Fact]
    public void Renders_BinaryOperators()
    {
        var left = new Luau.Literal("69");
        var right = new Luau.Literal("420");
        var binaryOp = new Luau.BinaryOperator(left, "+", right);
        var output = Render(binaryOp);
        Assert.Equal("69 + 420", output);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Renders_Variables(bool isLocal)
    {
        var identifier = new Luau.IdentifierName("abc");
        var value = new Luau.Literal("69");
        var typeRef = new Luau.TypeRef("number");
        var variable = new Luau.Variable(identifier, isLocal, value, typeRef);
        var output = Render(variable);
        Assert.Equal($"{(isLocal ? "local " : "")}abc: number = 69\n", output);
    }
    
    [Fact]
    public void Renders_ParametersWithDefault()
    {
        var identifier = new Luau.IdentifierName("myFunction");
        var parameterIdentifier = new Luau.IdentifierName("x");
        var parameterType = new Luau.TypeRef("number");
        var parameterDefault = new Luau.Literal("69");
        var body = new Luau.Block([]);
        var parameter = new Luau.Parameter(parameterIdentifier, false, parameterDefault, parameterType);
        var parameters = new Luau.ParameterList([parameter]);
        var function = new Luau.Function(identifier, true, parameters, null, body);
        var output = Render(function);
        const string expectedOutput = """
                                      local function myFunction(x: number?)
                                        if x == nil then
                                          x = 69
                                        end
                                      end

                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_StandardParameters()
    {
        var identifier = new Luau.IdentifierName("myFunction");
        var parameterIdentifier = new Luau.IdentifierName("x");
        var parameterType = new Luau.TypeRef("number");
        var body = new Luau.Block([]);
        var parameter = new Luau.Parameter(parameterIdentifier, false, null, parameterType);
        var parameters = new Luau.ParameterList([parameter]);
        var function = new Luau.Function(identifier, true, parameters, null, body);
        var output = Render(function);
        const string expectedOutput = """
                                      local function myFunction(x: number)
                                      end

                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }
    
    [Fact]
    public void Renders_VarargParameters()
    {
        var identifier = new Luau.IdentifierName("myFunction");
        var parameterIdentifier = new Luau.IdentifierName("args");
        var parameterType = new Luau.TypeRef("number");
        var body = new Luau.Block([]);
        var parameter = new Luau.Parameter(parameterIdentifier, true, null, parameterType);
        var parameters = new Luau.ParameterList([parameter]);
        var function = new Luau.Function(identifier, true, parameters, null, body);
        var output = Render(function);
        const string expectedOutput = """
                                      local function myFunction(...: number)
                                        local args: { number } = {...}
                                      end

                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Renders_Functions(bool isLocal)
    {
        var identifier = new Luau.IdentifierName("myFunction");
        var body = new Luau.Block([new Luau.Return(new Luau.Literal("69"))]);
        var returnType = new Luau.TypeRef("number");
        var function = new Luau.Function(identifier, isLocal, new Luau.ParameterList([]), returnType, body);
        var output = Render(function);
        var expectedOutput = $"""
                              {(isLocal ? "local " : "")}function myFunction(): number
                                return 69
                              end
                              
                              """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    private static string Render(Luau.Node node)
    {
        var writer = new Luau.LuauWriter();
        node.Render(writer);
        return writer.ToString();
    }
}