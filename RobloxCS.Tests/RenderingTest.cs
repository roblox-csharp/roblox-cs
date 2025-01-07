using RobloxCS.Luau;

namespace RobloxCS.Tests;

public class RenderingTest
{
    [Fact]
    public void MultiLineLineComment()
    {
        var comment = new MultiLineComment(string.Join('\n', Enumerable.Repeat("roblox-cs is the best!", 5)));
        var output = Render(comment);
        const string expectedOutput = """
                                      --[[
                                      roblox-cs is the best!
                                      roblox-cs is the best!
                                      roblox-cs is the best!
                                      roblox-cs is the best!
                                      roblox-cs is the best!
                                      ]]
                                      
                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }
    
    [Fact]
    public void Renders_SingleLineComment()
    {
        var comment = new SingleLineComment("roblox-cs is the best!");
        var output = Render(comment);
        const string expectedOutput = "-- roblox-cs is the best!";
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }
    
    [Fact]
    public void Renders_IterativeFor()
    {
        var name = new IdentifierName("value");
        var iterable = new IdentifierName("abc");
        var body = new ExpressionStatement(AstUtility.PrintCall(name));
        var forStatement = new For([name], iterable, body);
        var output = Render(forStatement);
        const string expectedOutput = """
                                      for _, value in abc do
                                        print(value)
                                      end

                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }
    
    [Fact]
    public void Renders_NumericFor()
    {
        var name = new IdentifierName("i");
        var minimum = new Literal("420");
        var maximum = new Literal("69");
        var increment = new Literal("-1");
        var body = new ExpressionStatement(AstUtility.PrintCall(new Literal("\"balls\"")));
        var forStatement = new NumericFor(name, minimum, maximum, increment, body);
        var output = Render(forStatement);
        const string expectedOutput = """
                                      for i = 420, 69, -1 do
                                        print("balls")
                                      end

                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }
    
    [Fact]
    public void Renders_Repeat()
    {
        var condition = new IdentifierName("balls");
        var body = new ExpressionStatement(AstUtility.PrintCall(new Literal("\"rah\"")));
        var repeatStatement = new Repeat(condition, body);
        var output = Render(repeatStatement);
        const string expectedOutput = """
                                      repeat 
                                        print("rah")
                                      until balls

                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }
    
    [Fact]
    public void Renders_While()
    {
        var condition = new IdentifierName("balls");
        var body = new ExpressionStatement(AstUtility.PrintCall(new Literal("\"rah\"")));
        var whileStatement = new While(condition, body);
        var output = Render(whileStatement);
        const string expectedOutput = """
                                      while balls do
                                        print("rah")
                                      end

                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }
    
    [Fact]
    public void Renders_ExpressionalIf()
    {
        var condition = new IdentifierName("runicIsCool");
        var body = new Literal("\"im tha best\"");
        var elseBranch = new Literal("\"im washed\"");
        var ifExpression = new ExpressionalIf(condition, body, elseBranch, true);
        var output = Render(ifExpression);
        Assert.Equal("if runicIsCool then \"im tha best\" else \"im washed\"", output);
    }
    
    [Fact]
    public void Renders_If()
    {
        var identifier = new IdentifierName("balls");
        var condition1 = new BinaryOperator(identifier, "==", new Literal("69"));
        var condition2 = new BinaryOperator(identifier, "==", new Literal("420"));
        var body = new ExpressionStatement(AstUtility.PrintCall(new Literal("\"im tha best\"")));
        var elseBody = new ExpressionStatement(AstUtility.PrintCall(new Literal("\"uhhhh\"")));
        var elseifBody = new ExpressionStatement(AstUtility.PrintCall(new Literal("\"im washed\"")));
        var elseifBranch = new If(condition2, elseifBody, elseBody);
        var ifStatement = new If(condition1, body, elseifBranch);
        var output = Render(ifStatement);
        const string expectedOutput = """
                                      if balls == 69 then
                                        print("im tha best")
                                      elseif balls == 420 then
                                        print("im washed")
                                      else
                                        print("uhhhh")
                                      end
                                      
                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_MappedTypes()
    {
        var mappedType = new MappedType(new TypeRef("string"), new TypeRef("number"));
        var output = Render(mappedType);
        Assert.Equal("{ [string]: number; }", output);
    }
    
    [Fact]
    public void Renders_InterfaceTypes()
    {
        var interfaceType = new InterfaceType(
            [new FieldType("myField", new TypeRef("string"), true)],
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
        var optionalType = new OptionalType(new TypeRef("boolean"));
        var output = Render(optionalType);
        Assert.Equal("boolean?", output);
    }

    [Fact]
    public void Renders_FunctionTypes()
    {
        var functionType = new FunctionType(
            [new ParameterType("myParam", new TypeRef("number"))],
            new TypeRef("boolean")
        );
        var output = Render(functionType);
        Assert.Equal("(myParam: number) -> boolean", output);
    }

    [Fact]
    public void Renders_ArrayTypes()
    {
        var arrayType = new ArrayType(new TypeRef("string"));
        var output = Render(arrayType);
        Assert.Equal("{ string }", output);
    }
    
    [Fact]
    public void Renders_TypeAliases()
    {
        var name = new IdentifierName("MyType");
        var value = new TypeRef("string");
        var typeAlias = new TypeAlias(name, value);
        var output = Render(typeAlias);
        Assert.Equal("type MyType = string\n", output);
    }
    
    [Fact]
    public void Renders_ElementAccess()
    {
        var elementAccess = new ElementAccess(new IdentifierName("a"), new Literal("123"));
        var output = Render(elementAccess);
        Assert.Equal("a[123]", output);
    }
    
    [Fact]
    public void Renders_MemberAccess()
    {
        var memberAccess = new MemberAccess(new IdentifierName("a"), new IdentifierName("b"));
        var output = Render(memberAccess);
        Assert.Equal("a.b", output);
    }
    
    [Fact]
    public void Renders_UnaryOperators()
    {
        var operand = new IdentifierName("isActive");
        var unaryOp = new UnaryOperator("not ", operand);
        var output = Render(unaryOp);
        Assert.Equal("not isActive", output);
    }
    
    [Fact]
    public void Renders_BinaryOperators()
    {
        var left = new Literal("69");
        var right = new Literal("420");
        var binaryOp = new BinaryOperator(left, "+", right);
        var output = Render(binaryOp);
        Assert.Equal("69 + 420", output);
    }
    
    [Fact]
    public void Renders_Assignment()
    {
        var target = new ElementAccess(new IdentifierName("a"), new Literal("69"));
        var value = new Literal("420");
        var assignment = new Assignment(target, value);
        var output = Render(assignment);
        Assert.Equal("a[69] = 420", output);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Renders_Variables(bool isLocal)
    {
        var identifier = new IdentifierName("abc");
        var value = new Literal("69");
        var typeRef = new TypeRef("number");
        var variable = new Variable(identifier, isLocal, value, typeRef);
        var output = Render(variable);
        Assert.Equal($"{(isLocal ? "local " : "")}abc: number = 69\n", output);
    }
    
    [Fact]
    public void Renders_ParametersWithDefault()
    {
        var identifier = new IdentifierName("myFunction");
        var parameterIdentifier = new IdentifierName("x");
        var parameterType = new TypeRef("number");
        var parameterDefault = new Literal("69");
        var body = new Block([]);
        var parameter = new Parameter(parameterIdentifier, false, parameterDefault, parameterType);
        var parameters = new ParameterList([parameter]);
        var function = new Function(identifier, true, parameters, null, body);
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
        var identifier = new IdentifierName("myFunction");
        var parameterIdentifier = new IdentifierName("x");
        var parameterType = new TypeRef("number");
        var body = new Block([]);
        var parameter = new Parameter(parameterIdentifier, false, null, parameterType);
        var parameters = new ParameterList([parameter]);
        var function = new Function(identifier, true, parameters, null, body);
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
        var identifier = new IdentifierName("myFunction");
        var parameterIdentifier = new IdentifierName("args");
        var parameterType = new TypeRef("number");
        var body = new Block([]);
        var parameter = new Parameter(parameterIdentifier, true, null, parameterType);
        var parameters = new ParameterList([parameter]);
        var function = new Function(identifier, true, parameters, null, body);
        var output = Render(function);
        const string expectedOutput = """
                                      local function myFunction(...: number)
                                        local args: { number } = {...}
                                      end

                                      """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_AnonymousFunctions()
    {
        var body = new Block([new Return(new Literal("69"))]);
        var function = new AnonymousFunction(new ParameterList([]), new TypeRef("number"), body);
        var output = Render(function);
        var expectedOutput = $"""
                              function(): number
                                return 69
                              end

                              """;
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Renders_Functions(bool isLocal)
    {
        var identifier = new IdentifierName("myFunction");
        var body = new Block([new Return(new Literal("69"))]);
        var returnType = new TypeRef("number");
        var function = new Function(identifier, isLocal, new ParameterList([]), returnType, body);
        var output = Render(function);
        var expectedOutput = $"""
                              {(isLocal ? "local " : "")}function myFunction(): number
                                return 69
                              end
                              
                              """;
        
        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    private static string Render(Node node)
    {
        var writer = new LuauWriter();
        node.Render(writer);
        return writer.ToString();
    }
}