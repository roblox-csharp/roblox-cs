using RobloxCS.Luau;

namespace RobloxCS.Tests;

public class RenderingTest
{
    [Fact]
    public void Renders_AST()
    {
        var statement = new ExpressionStatement(AstUtility.PrintCall(AstUtility.String("bruh")));
        var ast = new AST([statement]);
        var output = Render(ast);
        const string expectedOutput = """
                                      print("bruh")
                                      return nil

                                      """;

        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_Block()
    {
        var statements = Enumerable.Repeat(new ExpressionStatement(AstUtility.PrintCall(AstUtility.String("bruh"))), 5)
                                   .ToList<Statement>();

        var block = new Block(statements);
        var output = Render(block);
        const string expectedOutput = """
                                      print("bruh")
                                      print("bruh")
                                      print("bruh")
                                      print("bruh")
                                      print("bruh")

                                      """;

        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_ScopedBlock()
    {
        var statements = Enumerable.Repeat(new ExpressionStatement(AstUtility.PrintCall(AstUtility.String("bruh"))), 5)
                                   .ToList<Statement>();

        var block = new ScopedBlock(statements);
        var output = Render(block);
        const string expectedOutput = """
                                      do
                                        print("bruh")
                                        print("bruh")
                                        print("bruh")
                                        print("bruh")
                                        print("bruh")
                                      end

                                      """;

        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_MultiLineLineComment()
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
        var valueName = new IdentifierName("value");
        var iterable = new IdentifierName("abc");
        var body = new ExpressionStatement(AstUtility.PrintCall(valueName));
        var forStatement = new For([AstUtility.DiscardName, valueName], iterable, body);
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
        var forStatement = new NumericFor(name,
                                          minimum,
                                          maximum,
                                          increment,
                                          body);

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
    public void Renders_IfExpression()
    {
        var condition = new IdentifierName("runicIsCool");
        var body = new Literal("\"im tha best\"");
        var elseBranch = new Literal("\"im washed\"");
        var ifExpression = new IfExpression(condition, body, elseBranch, true);
        var output = Render(ifExpression);
        Assert.Equal("if runicIsCool then \"im tha best\" else \"im washed\"", output);
    }

    [Fact]
    public void Renders_If()
    {
        var identifier = new IdentifierName("balls");
        var condition1 = new BinaryOperator(identifier, "==", new Literal("69"));
        var condition2 = new BinaryOperator(identifier, "==", new Literal("420"));
        var body = new Block([new ExpressionStatement(AstUtility.PrintCall(new Literal("\"im tha best\"")))]);
        var elseBody = new Block([new ExpressionStatement(AstUtility.PrintCall(new Literal("\"uhhhh\"")))]);
        var elseifBody = new Block([new ExpressionStatement(AstUtility.PrintCall(new Literal("\"im washed\"")))]);
        var elseIfBranch = new Block([new If(condition2, elseifBody, elseBody)]);
        var ifStatement = new If(condition1, body, elseIfBranch);
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

        Assert.Equal(expectedOutput.Replace("\r", "").Trim(), output.Replace("\r", "").Trim());
    }

    [Fact]
    public void Renders_EmptyTableInitializer()
    {
        var tableInitializer = new TableInitializer();
        var output = Render(tableInitializer);

        Assert.Equal("{}", output);
    }

    [Fact]
    public void Renders_ArrayTableInitializer()
    {
        var tableInitializer = new TableInitializer([new Literal("69"), new Literal("420"), AstUtility.String("abc")]);
        var output = Render(tableInitializer);

        Assert.Equal("{69, 420, \"abc\"}", output);
    }

    [Fact]
    public void Renders_DictionaryTableInitializer()
    {
        var tableInitializer = new TableInitializer([new Literal("69"), new Literal("420"), AstUtility.String("abc")],
                                                    [new IdentifierName("foo"), new IdentifierName("bar"), AstUtility.String("baz")]);

        var output = Render(tableInitializer);
        const string expectedOutput = """
                                      {
                                        foo = 69,
                                        bar = 420,
                                        ["baz"] = "abc"
                                      }
                                      """;

        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_Calls()
    {
        var arguments = AstUtility.CreateArgumentList([new Literal("69"), new Literal("420"), AstUtility.String("abc")]);
        var call = new Call(new IdentifierName("bigMen"), arguments);
        var output = Render(call);

        Assert.Equal("bigMen(69, 420, \"abc\")", output);
    }

    [Fact]
    public void Renders_TypeOfCall()
    {
        var typeOfCall = new TypeOfCall(new IdentifierName("bigMen"));
        var output = Render(typeOfCall);

        Assert.Equal("typeof(bigMen)", output);
    }

    [Fact]
    public void Renders_IndexCall()
    {
        var indexCall = new IndexCall(new TypeRef("MyRecord"), new TypeRef("string"));
        var output = Render(indexCall);

        Assert.Equal("index<MyRecord, string>", output);
    }

    [Fact]
    public void Renders_KeyOfCall()
    {
        var keyOfCall = new KeyOfCall(new TypeRef("MyRecord"));
        var output = Render(keyOfCall);

        Assert.Equal("keyof<MyRecord>", output);
    }

    [Fact]
    public void Renders_ArgumentLists()
    {
        var arguments = AstUtility.CreateArgumentList([new Literal("69"), new Literal("420"), AstUtility.String("abc")]);
        var output = Render(arguments);

        Assert.Equal("(69, 420, \"abc\")", output);
    }

    [Fact]
    public void Renders_BuiltInAttributes()
    {
        var attribute = new AttributeList([new BuiltInAttribute(new IdentifierName("native"))]);
        var output = Render(attribute);

        Assert.Equal("@native\n", output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_MappedTypes()
    {
        var mappedType = new MappedType(new TypeRef("string"), new TypeRef("number"));
        var output = Render(mappedType);

        Assert.Equal("{ [string]: number }", output);
    }

    [Fact]
    public void Renders_InterfaceTypes()
    {
        var interfaceType = new InterfaceType([new FieldType("myField", new TypeRef("string"), true)],
                                              null,
                                              false);

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
        var functionType = new FunctionType([new ParameterType("myParam", new TypeRef("number"))],
                                            new TypeRef("boolean"));

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
    public void Renders_TypeCasts()
    {
        var value = new IdentifierName("myValue");
        var typeRef = new TypeRef("MyType");
        var typeCast = new TypeCast(value, typeRef);
        var output = Render(typeCast);

        Assert.Equal("myValue :: MyType", output);
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
        Assert.Equal("a[69] = 420\n", output);
    }

    [Fact]
    public void Renders_QualifiedName()
    {
        const string result = "Abc:myMethod";
        var name = new QualifiedName(new IdentifierName("Abc"), new IdentifierName("myMethod"), ':');
        var output = Render(name);

        Assert.Equal(result, output);
        Assert.Equal(result, name.ToString());
    }

    [Fact]
    public void Renders_GenericName()
    {
        const string result = "Abc<T, U>";
        var name = new GenericName("Abc", ["T", "U"]);
        var output = Render(name);

        Assert.Equal(result, output);
        Assert.Equal(result, name.ToString());
    }

    [Fact]
    public void Renders_IdentifierName()
    {
        const string text = "Abc";
        var name = new IdentifierName(text);
        var output = Render(name);

        Assert.Equal(text, output);
        Assert.Equal(text, name.ToString());
    }

    [Fact]
    public void Renders_Parenthesized()
    {
        var parenthesized = new Parenthesized(AstUtility.String("bruh"));
        var output = Render(parenthesized);

        Assert.Equal("(\"bruh\")", output);
    }

    [Fact]
    public void Renders_InterpolatedStrings()
    {
        var stringInterpolation = new InterpolatedString([new Literal("hello, "), new Interpolation(new IdentifierName("name")), new Literal("!"),]);

        var output = Render(stringInterpolation);
        Assert.Equal("`hello, {name}!`", output);
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
    public void Renders_VariableLists()
    {
        var identifier = new IdentifierName("abc");
        var value = new Literal("69");
        var typeRef = new TypeRef("number");
        var variables = Enumerable.Repeat(new Variable(identifier, true, value, typeRef), 5).ToList();
        var variableList = new VariableList(variables);
        var output = Render(variableList);
        const string expectedOutput = """
                                      local abc: number = 69
                                      local abc: number = 69
                                      local abc: number = 69
                                      local abc: number = 69
                                      local abc: number = 69

                                      """;

        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_ParametersWithDefault()
    {
        var identifier = new IdentifierName("myFunction");
        var parameterIdentifier = new IdentifierName("x");
        var parameterType = new OptionalType(new TypeRef("number"));
        var parameterDefault = new Literal("69");
        var body = new Block([]);
        var parameter = new Parameter(parameterIdentifier, false, parameterDefault, parameterType);
        var parameters = new ParameterList([parameter]);
        var function = new Function(identifier,
                                    true,
                                    parameters,
                                    null,
                                    body);

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
        var function = new Function(identifier,
                                    true,
                                    parameters,
                                    null,
                                    body);

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
        var function = new Function(identifier,
                                    true,
                                    parameters,
                                    null,
                                    body);

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
        const string expectedOutput = """
                                      function(): number
                                        return 69
                                      end
                                      """;

        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Renders_Functions(bool isLocal)
    {
        var identifier = new IdentifierName("myFunction");
        var body = new Block([new Return(new Literal("69"))]);
        var returnType = new TypeRef("number");
        var function = new Function(identifier,
                                    isLocal,
                                    new ParameterList([]),
                                    returnType,
                                    body);

        var output = Render(function);
        var expectedOutput = $"""
                              {(isLocal ? "local " : "")}function myFunction(): number
                                return 69
                              end

                              """;

        Assert.Equal(expectedOutput.Replace("\r", ""), output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_Continue()
    {
        var @continue = new Continue();
        var output = Render(@continue);

        Assert.Equal("continue\n", output.Replace("\r", ""));
    }

    [Fact]
    public void Renders_Break()
    {
        var @break = new Break();
        var output = Render(@break);

        Assert.Equal("break\n", output.Replace("\r", ""));
    }

    private static string Render(Node node)
    {
        var writer = new LuauWriter();
        node.Render(writer);

        return writer.ToString();
    }
}