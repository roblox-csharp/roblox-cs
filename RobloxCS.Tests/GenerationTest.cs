using RobloxCS.Luau;
using RobloxCS.Macros;
using RobloxCS.Tests.Base;

namespace RobloxCS.Tests;

public class GenerationTest : Generation
{
    [Theory]
    [InlineData("var _ = sizeof(byte);", "1")]
    [InlineData("var _ = sizeof(short);", "2")]
    [InlineData("var _ = sizeof(ushort);", "2")]
    [InlineData("var _ = sizeof(int);", "4")]
    [InlineData("var _ = sizeof(uint);", "4")]
    public void Generates_SizeOf(string sizeofCall, string luauValueText)
    {
        var ast = Generate(sizeofCall);
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var initializer = variableList.Variables.First().Initializer;
        Assert.IsType<Literal>(initializer);

        var literal = (Literal)initializer;
        Assert.Equal(luauValueText, literal.ValueText);
    }

    [Fact]
    public void Generates_MethodOverloads()
    {
        const string source = """
                              var abc = new Abc();
                              abc.Method("abc");
                              abc.Method(true);
                              abc.Method(69);

                              class Abc
                              {
                                  public void Method(string a)
                                  {
                                  }
                              
                                  public void Method(bool a)
                                  {
                                  }
                              
                                  public void Method(int a)
                                  {
                                  }
                              }
                              """;

        var ast = Generate(source);
        var statements = ast.Statements.Skip(1).ToList();
        Assert.NotEmpty(statements);
        Assert.IsType<Block>(statements.First());

        var classBlock = (Block)statements.First();
        Assert.Equal(5, classBlock.Statements.Count);

        var secondClassStatement = classBlock.Statements[1];
        Assert.IsType<ScopedBlock>(secondClassStatement);

        var classScope = (ScopedBlock)secondClassStatement;
        var methodDeclarations = classScope.Statements.TakeLast(3).ToList();
        var firstStatement = methodDeclarations[0];
        var secondStatement = methodDeclarations[1];
        var thirdStatement = methodDeclarations[2];
        Assert.IsType<Function>(firstStatement);
        Assert.IsType<Function>(secondStatement);
        Assert.IsType<Function>(thirdStatement);

        var firstMethod = (Function)firstStatement;
        var secondMethod = (Function)secondStatement;
        var thirdMethod = (Function)thirdStatement;
        Assert.Equal("Abc:Method_impl1", firstMethod.Name.ToString());
        Assert.Equal("Abc:Method_impl2", secondMethod.Name.ToString());
        Assert.Equal("Abc:Method_impl3", thirdMethod.Name.ToString());

        var expressions = statements
                          .Skip(2)
                          .OfType<ExpressionStatement>()
                          .Select(e => e.Expression)
                          .ToList();

        expressions.ForEach(e => Assert.IsType<Call>(e));
        var stringCall = (Call)expressions[0];
        var boolCall = (Call)expressions[1];
        var intCall = (Call)expressions[2];
        Assert.IsType<MemberAccess>(stringCall.Callee);
        Assert.IsType<MemberAccess>(boolCall.Callee);
        Assert.IsType<MemberAccess>(intCall.Callee);

        var first = (MemberAccess)stringCall.Callee;
        var second = (MemberAccess)boolCall.Callee;
        var third = (MemberAccess)intCall.Callee;
        Assert.IsType<IdentifierName>(first.Name);
        Assert.IsType<IdentifierName>(second.Name);
        Assert.IsType<IdentifierName>(third.Name);
        Assert.Equal("Method_impl1", first.Name.ToString());
        Assert.Equal("Method_impl2", second.Name.ToString());
        Assert.Equal("Method_impl3", third.Name.ToString());
    }

    [Fact]
    public void Generates_LuaTupleDestructuring_InForEach()
    {
        var ast = Generate("foreach (var (i, v) in pairs<int>([]) { }");
        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<For>(statement);

        var forStatement = (For)statement;
        Assert.Equal(2, forStatement.Names.Count);

        var firstName = forStatement.Names[0];
        var secondName = forStatement.Names[1];
        Assert.Equal("i", firstName.ToString());
        Assert.Equal("v", secondName.ToString());
    }

    [Fact]
    public void Generates_LuaTupleDestructuring()
    {
        var ast = Generate("var (success, result) = pcall(() => { })");
        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<MultipleVariable>(statement);

        var variable = (MultipleVariable)statement;
        Assert.Single(variable.Initializers);
        Assert.IsType<Call>(variable.Initializers.First());
        Assert.Equal(2, variable.Names.Count);

        var enumerator = variable.Names.GetEnumerator();
        enumerator.MoveNext();
        var firstName = enumerator.Current;
        enumerator.MoveNext();
        var secondName = enumerator.Current;

        Assert.Equal("success", firstName.ToString());
        Assert.Equal("result", secondName.ToString());
    }

    [Fact]
    public void Generates_TupleDestructuring()
    {
        var ast = Generate("var (a, b, c) = (1, 2, 3)");
        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<MultipleVariable>(statement);

        var variable = (MultipleVariable)statement;
        Assert.Single(variable.Initializers);
        Assert.IsType<Call>(variable.Initializers.First());
        Assert.Equal(3, variable.Names.Count);

        var unpackCall = (Call)variable.Initializers.First();
        Assert.IsType<MemberAccess>(unpackCall.Callee);

        var memberAccess = (MemberAccess)unpackCall.Callee;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        Assert.Equal("CS", memberAccess.Expression.ToString());
        Assert.Equal("unpackTuple", memberAccess.Name.ToString());

        Assert.Single(unpackCall.ArgumentList.Arguments);
        Assert.IsType<TableInitializer>(unpackCall.ArgumentList.Arguments.First().Expression);

        var enumerator = variable.Names.GetEnumerator();
        enumerator.MoveNext();
        var firstName = enumerator.Current;
        enumerator.MoveNext();
        var secondName = enumerator.Current;
        enumerator.MoveNext();
        var thirdName = enumerator.Current;

        Assert.Equal("a", firstName.ToString());
        Assert.Equal("b", secondName.ToString());
        Assert.Equal("c", thirdName.ToString());
    }

    [Fact]
    public void Generates_Tuples()
    {
        var ast = Generate("(1, 2, 3)");
        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<ExpressionStatement>(statement);

        var expression = ((ExpressionStatement)statement).Expression;
        Assert.IsType<TableInitializer>(expression);

        var table = (TableInitializer)expression;
        var firstPair = table.KeyValuePairs[0];
        var secondPair = table.KeyValuePairs[1];
        var thirdPair = table.KeyValuePairs[2];
        Assert.IsType<IdentifierName>(firstPair.Key);
        Assert.IsType<IdentifierName>(secondPair.Key);
        Assert.IsType<IdentifierName>(thirdPair.Key);
        Assert.IsType<Literal>(firstPair.Value);
        Assert.IsType<Literal>(secondPair.Value);
        Assert.IsType<Literal>(thirdPair.Value);
        Assert.Equal("Item1", firstPair.Key.ToString());
        Assert.Equal("Item2", secondPair.Key.ToString());
        Assert.Equal("Item3", thirdPair.Key.ToString());

        var firstLiteral = (Literal)firstPair.Value;
        var secondLiteral = (Literal)secondPair.Value;
        var thirdLiteral = (Literal)thirdPair.Value;
        Assert.Equal("1", firstLiteral.ValueText);
        Assert.Equal("2", secondLiteral.ValueText);
        Assert.Equal("3", thirdLiteral.ValueText);
    }

    [Fact]
    public void Generates_ComplexGeneratorFunction()
    {
        var ast = Generate("""
                           IEnumerator<int> GetEnumerator()
                           {
                             print("a");
                             yield return 1;
                             print("b");
                             yield break;
                             print("c");
                             yield return 3;
                           }
                           """);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<Function>(statement);

        var function = (Function)statement;
        Assert.Empty(function.ParameterList.Parameters);
        Assert.True(function.IsLocal);
        Assert.NotNull(function.ReturnType);
        Assert.Equal("CS.IEnumerator<number>", function.ReturnType.Path);
        Assert.NotNull(function.Body);
        Assert.Single(function.Body.Statements);

        var bodyStatement = function.Body.Statements.First();
        Assert.IsType<Return>(bodyStatement);

        var returnStatement = (Return)bodyStatement;
        Assert.IsType<Call>(returnStatement.Expression);

        var call = (Call)returnStatement.Expression;
        Assert.Single(call.ArgumentList.Arguments);
        Assert.IsType<MemberAccess>(call.Callee);

        var enumeratorConstructor = (MemberAccess)call.Callee;
        Assert.IsType<MemberAccess>(enumeratorConstructor.Expression);
        Assert.IsType<IdentifierName>(enumeratorConstructor.Name);
        Assert.Equal("new", enumeratorConstructor.Name.ToString());

        var csDotEnumerator = (MemberAccess)enumeratorConstructor.Expression;
        Assert.IsType<IdentifierName>(csDotEnumerator.Expression);
        Assert.IsType<IdentifierName>(csDotEnumerator.Name);
        Assert.Equal("CS", csDotEnumerator.Expression.ToString());
        Assert.Equal("Enumerator", csDotEnumerator.Name.ToString());

        var argumentExpression = call.ArgumentList.Arguments.First().Expression;
        Assert.IsType<AnonymousFunction>(argumentExpression);

        var initializerFunction = (AnonymousFunction)argumentExpression;
        Assert.NotNull(initializerFunction.Body);
        Assert.Single(initializerFunction.Body.Statements);

        var initializerStatement = initializerFunction.Body.Statements.First();
        Assert.IsType<Return>(initializerStatement);

        var initializerReturn = (Return)initializerStatement;
        Assert.IsType<TableInitializer>(initializerReturn.Expression);

        var tableInitializer = (TableInitializer)initializerReturn.Expression;
        Assert.Equal(3, tableInitializer.Values.Count);

        var firstValue = tableInitializer.Values[0];
        var secondValue = tableInitializer.Values[1];
        var thirdValue = tableInitializer.Values[2];
        Assert.IsType<AnonymousFunction>(firstValue);
        Assert.IsType<AnonymousFunction>(secondValue);
        Assert.IsType<AnonymousFunction>(thirdValue);

        var firstFunction = (AnonymousFunction)firstValue;
        var secondFunction = (AnonymousFunction)secondValue;
        var thirdFunction = (AnonymousFunction)thirdValue;
        Assert.NotNull(firstFunction.Body);
        Assert.NotNull(secondFunction.Body);
        Assert.NotNull(thirdFunction.Body);
        Assert.Equal(2, firstFunction.Body.Statements.Count);
        Assert.Equal(2, secondFunction.Body.Statements.Count);
        Assert.Equal(2, thirdFunction.Body.Statements.Count);

        var firstFirstStatement = firstFunction.Body.Statements.First();
        var firstLastStatement = firstFunction.Body.Statements.Last();
        Assert.IsType<ExpressionStatement>(firstFirstStatement);
        Assert.IsType<Return>(firstLastStatement);

        var firstExpression = ((ExpressionStatement)firstFirstStatement).Expression;
        Assert.IsType<Call>(firstExpression);

        var firstReturn = (Return)firstLastStatement;
        Assert.IsType<Literal>(firstReturn.Expression);

        var firstReturnValue = (Literal)firstReturn.Expression;
        Assert.Equal("1", firstReturnValue.ValueText);

        var secondFirstStatement = secondFunction.Body.Statements.First();
        var secondLastStatement = secondFunction.Body.Statements.Last();
        Assert.IsType<ExpressionStatement>(secondFirstStatement);
        Assert.IsType<ExpressionStatement>(secondLastStatement);

        var secondExpression = ((ExpressionStatement)secondFirstStatement).Expression;
        var breakExpression = ((ExpressionStatement)secondLastStatement).Expression;
        Assert.IsType<Call>(secondExpression);
        Assert.IsType<Call>(breakExpression);

        var breakCall = (Call)breakExpression;
        Assert.IsType<IdentifierName>(breakCall.Callee);
        Assert.Equal("_breakIteration", breakCall.Callee.ToString());

        var thirdFirstStatement = thirdFunction.Body.Statements.First();
        var thirdLastStatement = thirdFunction.Body.Statements.Last();
        Assert.IsType<ExpressionStatement>(thirdFirstStatement);
        Assert.IsType<Return>(thirdLastStatement);

        var thirdExpression = ((ExpressionStatement)thirdFirstStatement).Expression;
        Assert.IsType<Call>(thirdExpression);

        var thirdReturn = (Return)thirdLastStatement;
        Assert.IsType<Literal>(thirdReturn.Expression);

        var thirdReturnValue = (Literal)thirdReturn.Expression;
        Assert.Equal("3", thirdReturnValue.ValueText);
    }

    [Fact]
    public void Generates_SimpleGeneratorFunction_WithBreak()
    {
        var ast = Generate("""
                           IEnumerator<int> GetEnumerator()
                           {
                             yield return 1;
                             yield break;
                             yield return 3;
                           }
                           """);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<Function>(statement);

        var function = (Function)statement;
        Assert.Empty(function.ParameterList.Parameters);
        Assert.True(function.IsLocal);
        Assert.NotNull(function.ReturnType);
        Assert.Equal("CS.IEnumerator<number>", function.ReturnType.Path);
        Assert.NotNull(function.Body);
        Assert.Single(function.Body.Statements);

        var bodyStatement = function.Body.Statements.First();
        Assert.IsType<Return>(bodyStatement);

        var returnStatement = (Return)bodyStatement;
        Assert.IsType<Call>(returnStatement.Expression);

        var call = (Call)returnStatement.Expression;
        Assert.Single(call.ArgumentList.Arguments);
        Assert.IsType<MemberAccess>(call.Callee);

        var enumeratorConstructor = (MemberAccess)call.Callee;
        Assert.IsType<MemberAccess>(enumeratorConstructor.Expression);
        Assert.IsType<IdentifierName>(enumeratorConstructor.Name);
        Assert.Equal("new", enumeratorConstructor.Name.ToString());

        var csDotEnumerator = (MemberAccess)enumeratorConstructor.Expression;
        Assert.IsType<IdentifierName>(csDotEnumerator.Expression);
        Assert.IsType<IdentifierName>(csDotEnumerator.Name);
        Assert.Equal("CS", csDotEnumerator.Expression.ToString());
        Assert.Equal("Enumerator", csDotEnumerator.Name.ToString());

        var argumentExpression = call.ArgumentList.Arguments.First().Expression;
        Assert.IsType<TableInitializer>(argumentExpression);

        var tableInitializer = (TableInitializer)argumentExpression;
        Assert.Single(tableInitializer.Values);

        var value = tableInitializer.Values.First();
        Assert.IsType<Literal>(value);

        var literal = (Literal)value;
        Assert.Equal("1", literal.ValueText);
    }

    [Fact]
    public void Generates_SimpleGeneratorFunction()
    {
        var ast = Generate("""
                           IEnumerator<int> GetEnumerator()
                           {
                             yield return 1;
                             yield return 2;
                             yield return 3;
                           }
                           """);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<Function>(statement);

        var function = (Function)statement;
        Assert.Empty(function.ParameterList.Parameters);
        Assert.True(function.IsLocal);
        Assert.NotNull(function.ReturnType);
        Assert.Equal("CS.IEnumerator<number>", function.ReturnType.Path);
        Assert.NotNull(function.Body);
        Assert.Single(function.Body.Statements);

        var bodyStatement = function.Body.Statements.First();
        Assert.IsType<Return>(bodyStatement);

        var returnStatement = (Return)bodyStatement;
        Assert.IsType<Call>(returnStatement.Expression);

        var call = (Call)returnStatement.Expression;
        Assert.Single(call.ArgumentList.Arguments);
        Assert.IsType<MemberAccess>(call.Callee);

        var enumeratorConstructor = (MemberAccess)call.Callee;
        Assert.IsType<MemberAccess>(enumeratorConstructor.Expression);
        Assert.IsType<IdentifierName>(enumeratorConstructor.Name);
        Assert.Equal("new", enumeratorConstructor.Name.ToString());

        var csDotEnumerator = (MemberAccess)enumeratorConstructor.Expression;
        Assert.IsType<IdentifierName>(csDotEnumerator.Expression);
        Assert.IsType<IdentifierName>(csDotEnumerator.Name);
        Assert.Equal("CS", csDotEnumerator.Expression.ToString());
        Assert.Equal("Enumerator", csDotEnumerator.Name.ToString());

        var argumentExpression = call.ArgumentList.Arguments.First().Expression;
        Assert.IsType<TableInitializer>(argumentExpression);

        var tableInitializer = (TableInitializer)argumentExpression;
        Assert.Equal(3, tableInitializer.Values.Count);

        var value = tableInitializer.Values.First();
        Assert.IsType<Literal>(value);

        var literal = (Literal)value;
        Assert.Equal("1", literal.ValueText);
    }

    [Fact]
    public void Hoists_LocalFunctions()
    {
        var ast = Generate("var a = 1; abc(); void abc() => print(a);");
        var statements = ast.Statements.Skip(1).ToList();
        Assert.Equal(3, statements.Count);

        var variableStatement = statements[0];
        var functionStatement = statements[1];
        var callStatement = statements[2];
        Assert.IsType<VariableList>(variableStatement);
        Assert.IsType<Function>(functionStatement);
        Assert.IsType<ExpressionStatement>(callStatement);

        var expressionStatement = (ExpressionStatement)callStatement;
        Assert.IsType<Call>(expressionStatement.Expression);
    }

    [Fact]
    public void Generates_ObjectCreation_WithInitializer()
    {
        var ast = Generate("class Abc { public required int A { get; set; } } var abc = new Abc() { A = 69 };");
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(2).ToList();
        Assert.Equal(3, statements.Count);

        var bindingVariableStatement = statements[0];
        Assert.IsType<Variable>(bindingVariableStatement);

        var bindingVariable = (Variable)bindingVariableStatement;
        Assert.Equal("_binding", bindingVariable.Name.ToString());
        Assert.IsType<Call>(bindingVariable.Initializer);

        var constructorCall = (Call)bindingVariable.Initializer;
        Assert.Empty(constructorCall.ArgumentList.Arguments);
        Assert.IsType<MemberAccess>(constructorCall.Callee);

        var constructor = (MemberAccess)constructorCall.Callee;
        Assert.IsType<IdentifierName>(constructor.Expression);
        Assert.IsType<IdentifierName>(constructor.Name);
        Assert.Equal("Abc", constructor.Expression.ToString());
        Assert.Equal("new", constructor.Name.ToString());

        var aFieldAssignment = statements[1];
        Assert.IsType<Assignment>(aFieldAssignment);

        var assignment = (Assignment)aFieldAssignment;
        Assert.IsType<QualifiedName>(assignment.Target);
        Assert.IsType<Literal>(assignment.Value);
        Assert.Equal("_binding.A", assignment.Target.ToString());

        var value = (Literal)assignment.Value;
        Assert.Equal("69", value.ValueText);

        var finalVariableStatement = statements[2];
        Assert.IsType<VariableList>(finalVariableStatement);

        var finalVariableList = (VariableList)finalVariableStatement;
        var finalVariable = finalVariableList.Variables.First();
        Assert.Equal("abc", finalVariable.Name.ToString());
        Assert.IsType<IdentifierName>(finalVariable.Initializer);
        Assert.Equal("_binding", finalVariable.Initializer.ToString());
    }

    [Fact]
    public void Generates_ObjectCreation()
    {
        var ast = Generate("class Abc<T>; var abc = new Abc<int>();");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("abc", variable.Name.ToString());
        Assert.IsType<Call>(variable.Initializer);

        var constructorCall = (Call)variable.Initializer;
        Assert.Empty(constructorCall.ArgumentList.Arguments);
        Assert.IsType<MemberAccess>(constructorCall.Callee);

        var constructor = (MemberAccess)constructorCall.Callee;
        Assert.IsType<IdentifierName>(constructor.Expression);
        Assert.IsType<IdentifierName>(constructor.Name);
        Assert.Equal("Abc", constructor.Expression.ToString());
        Assert.Equal("new", constructor.Name.ToString());
    }

    [Fact]
    public void Generates_HashSetType()
    {
        var ast = Generate("HashSet<int> set = [];");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("set", variable.Name.ToString());
        Assert.IsType<TableInitializer>(variable.Initializer);
        Assert.IsType<MappedType>(variable.Type);

        var mappedType = (MappedType)variable.Type;
        Assert.Equal("number", mappedType.KeyType.Path);
        Assert.Equal("boolean", mappedType.ValueType.Path);
    }

    [Fact]
    public void Generates_NestedHashSetType()
    {
        var ast = Generate("HashSet<HashSet<int>> set = [];");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("set", variable.Name.ToString());
        Assert.IsType<TableInitializer>(variable.Initializer);
        Assert.IsType<MappedType>(variable.Type);

        var mappedType = (MappedType)variable.Type;
        Assert.IsType<MappedType>(mappedType.KeyType);
        Assert.Equal("boolean", mappedType.ValueType.Path);

        var nestedMappedType = (MappedType)mappedType.KeyType;
        Assert.Equal("number", nestedMappedType.KeyType.Path);
        Assert.Equal("boolean", nestedMappedType.ValueType.Path);
    }

    [Theory]
    [InlineData("var set = new HashSet<int>() { 1 };")]
    [InlineData("HashSet<int> set = [1];", false)]
    public void Generates_HashSetCreation(string source, bool isMacro = true)
    {
        var ast = Generate(source);
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("set", variable.Name.ToString());
        Assert.NotNull(variable.Initializer);
        if (isMacro)
        {
            Assert.NotNull(variable.Initializer.ExpandedByMacro);
            Assert.Equal(MacroKind.HashSetConstruction, variable.Initializer.ExpandedByMacro);
        }

        Assert.IsType<TableInitializer>(variable.Initializer);

        var table = (TableInitializer)variable.Initializer;
        Assert.Single(table.Values);

        var pair = table.KeyValuePairs.First();
        Assert.IsType<Literal>(pair.Key);
        Assert.IsType<Literal>(pair.Value);

        var value = (Literal)pair.Key;
        var initializer = (Literal)pair.Value;
        Assert.Equal("1", value.ValueText);
        Assert.Equal("true", initializer.ValueText);
    }

    [Fact]
    public void Generates_ListType()
    {
        var ast = Generate("List<int> l = [];");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("l", variable.Name.ToString());
        Assert.IsType<TableInitializer>(variable.Initializer);
        Assert.IsType<ArrayType>(variable.Type);

        var arrayType = (ArrayType)variable.Type;
        Assert.Equal("number", arrayType.ElementType.Path);
    }

    [Fact]
    public void Generates_NestedListType()
    {
        var ast = Generate("List<List<int>> l = [];");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("l", variable.Name.ToString());
        Assert.IsType<TableInitializer>(variable.Initializer);
        Assert.IsType<ArrayType>(variable.Type);

        var arrayType = (ArrayType)variable.Type;
        Assert.IsType<ArrayType>(arrayType.ElementType);

        var nestedArrayType = (ArrayType)arrayType.ElementType;
        Assert.Equal("number", nestedArrayType.ElementType.Path);
    }

    [Theory]
    [InlineData("var list = new List<int>([1]);")]
    [InlineData("var list = new List<int>() { 1 };")]
    [InlineData("List<int> list = [1];", false)]
    [InlineData("List<int> list = new([1]);")]
    public void Generates_ListCreation(string source, bool isMacro = true)
    {
        var ast = Generate(source);
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("list", variable.Name.ToString());
        Assert.NotNull(variable.Initializer);
        if (isMacro)
        {
            Assert.NotNull(variable.Initializer.ExpandedByMacro);
            Assert.Equal(MacroKind.ListConstruction, variable.Initializer.ExpandedByMacro);
        }

        Assert.IsType<TableInitializer>(variable.Initializer);

        var table = (TableInitializer)variable.Initializer;
        Assert.Single(table.Values);

        var value = table.Values.First();
        Assert.IsType<Literal>(value);

        var literal = (Literal)value;
        Assert.Equal("1", literal.ValueText);
    }

    [Fact]
    public void Generates_DictionaryType()
    {
        var ast = Generate("Dictionary<string, int> d = [];");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("d", variable.Name.ToString());
        Assert.IsType<TableInitializer>(variable.Initializer);
        Assert.IsType<MappedType>(variable.Type);

        var mappedType = (MappedType)variable.Type;
        Assert.Equal("string", mappedType.KeyType.Path);
        Assert.Equal("number", mappedType.ValueType.Path);
    }

    [Fact]
    public void Generates_NestedDictionaryType()
    {
        var ast = Generate("Dictionary<string, Dictionary<string, int>> d = [];");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("d", variable.Name.ToString());
        Assert.IsType<TableInitializer>(variable.Initializer);
        Assert.IsType<MappedType>(variable.Type);

        var mappedType = (MappedType)variable.Type;
        Assert.IsType<MappedType>(mappedType.ValueType);
        Assert.Equal("string", mappedType.KeyType.Path);

        var nestedMappedType = (MappedType)mappedType.ValueType;
        Assert.Equal("string", nestedMappedType.KeyType.Path);
        Assert.Equal("number", nestedMappedType.ValueType.Path);
    }

    [Theory]
    [InlineData("var dict = new Dictionary<string, int>();")]
    [InlineData("Dictionary<string, int> dict = new();")]
    [InlineData("Dictionary<string, int> dict = [];", false)]
    public void Generates_EmptyDictionaryCreation(string source, bool isMacro = true)
    {
        var ast = Generate(source);
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("dict", variable.Name.ToString());
        Assert.NotNull(variable.Initializer);
        if (isMacro)
        {
            Assert.NotNull(variable.Initializer.ExpandedByMacro);
            Assert.Equal(MacroKind.DictionaryConstruction, variable.Initializer.ExpandedByMacro);
        }

        Assert.IsType<TableInitializer>(variable.Initializer);

        var table = (TableInitializer)variable.Initializer;
        Assert.Empty(table.KeyValuePairs);
    }

    [Theory]
    [InlineData("var dict = new Dictionary<string, int> { [\"abc\"] = 69 };")]
    [InlineData("var dict = new Dictionary<string, int> { { \"abc\", 69 } };")]
    [InlineData("Dictionary<string, int> dict = new() { [\"abc\"] = 69 };")]
    [InlineData("Dictionary<string, int> dict = new() { { \"abc\", 69 } };")]
    public void Generates_DictionaryCreation(string source)
    {
        var ast = Generate(source);
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

        var variableList = (VariableList)statement;
        var variable = variableList.Variables.First();
        Assert.Equal("dict", variable.Name.ToString());
        Assert.NotNull(variable.Initializer);
        Assert.NotNull(variable.Initializer.ExpandedByMacro);
        Assert.Equal(MacroKind.DictionaryConstruction, variable.Initializer.ExpandedByMacro);
        Assert.IsType<TableInitializer>(variable.Initializer);

        var table = (TableInitializer)variable.Initializer;
        Assert.Single(table.KeyValuePairs);

        var pair = table.KeyValuePairs.First();
        Assert.IsType<Literal>(pair.Key);
        Assert.IsType<Literal>(pair.Value);

        var key = (Literal)pair.Key;
        var value = (Literal)pair.Value;
        Assert.Equal("\"abc\"", key.ValueText);
        Assert.Equal("69", value.ValueText);
    }

    // TODO: actually implement fully
    [Fact]
    public void Generates_SafeNavigation()
    {
        var ast = Generate("a?.b?.c;");
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(1).ToList();
        var originalVariable = statements[0];
        var statement = statements[1];
        Assert.IsType<Variable>(originalVariable);
        Assert.IsType<If>(statement);

        var ifStatement = (If)statement;
        Assert.IsType<BinaryOperator>(ifStatement.Condition);

        var condition = (BinaryOperator)ifStatement.Condition;
        Assert.IsType<IdentifierName>(condition.Left);
        Assert.IsType<Literal>(condition.Right);
        Assert.Equal("~=", condition.Operator);

        var conditionName = (IdentifierName)condition.Left;
        var conditionValue = (Literal)condition.Right;
        Assert.Equal("_c", conditionName.ToString());
        Assert.Equal("nil", conditionValue.ValueText);
        Assert.Null(ifStatement.ElseBranch);
    }

    [Fact]
    public void Generates_ShorthandNumericFor()
    {
        var ast = Generate("for (var i = 0; i < 10; i++) continue;");
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(1).ToList();
        Assert.Single(statements);
        Assert.IsType<NumericFor>(statements.First());

        var numericFor = (NumericFor)statements.First();
        Assert.IsType<Literal>(numericFor.Minimum);
        Assert.IsType<Literal>(numericFor.Maximum);
        Assert.Null(numericFor.IncrementBy);

        var minimum = (Literal)numericFor.Minimum;
        var maximum = (Literal)numericFor.Maximum;
        Assert.Equal("0", minimum.ValueText);
        Assert.Equal("9", maximum.ValueText);
        Assert.Equal("i", numericFor.Name.ToString());

        var block = (Block)numericFor.Body;
        Assert.Single(block.Statements);
        Assert.IsType<Continue>(block.Statements.First());
    }

    [Fact]
    public void Generates_NativeAttribute()
    {
        var ast = Generate("[Native] void abc() {}");
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(1).ToList();
        Assert.Single(statements);
        Assert.IsType<Function>(statements.First());

        var function = (Function)statements.First();
        Assert.Single(function.AttributeLists);

        var attributeList = function.AttributeLists.First();
        Assert.Single(attributeList.Attributes);
        Assert.IsType<BuiltInAttribute>(attributeList.Attributes.First());

        var attribute = (BuiltInAttribute)attributeList.Attributes.First();
        Assert.Equal("native", attribute.Name.ToString());
    }

    [Fact]
    public void Generates_AssignmentExpressionResult()
    {
        var ast = Generate("var a = 1; var x = a = 2;");
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(2).ToList();
        Assert.Equal(2, statements.Count);

        var firstStatement = statements[0];
        var secondStatement = statements[1];
        Assert.IsType<Assignment>(firstStatement);
        Assert.IsType<VariableList>(secondStatement);

        var assignment = (Assignment)firstStatement;
        Assert.IsType<IdentifierName>(assignment.Target);
        Assert.IsType<Literal>(assignment.Value);

        var left = (IdentifierName)assignment.Target;
        var right = (Literal)assignment.Value;
        Assert.Equal("a", left.ToString());
        Assert.Equal("2", right.ValueText);

        var variableList = (VariableList)secondStatement;
        Assert.Single(variableList.Variables);

        var variable = variableList.Variables.First();
        Assert.Equal("x", variable.Name.ToString());
        Assert.IsType<IdentifierName>(variable.Initializer);

        var initializer = (IdentifierName)variable.Initializer;
        Assert.Equal("a", initializer.ToString());
    }

    [Fact]
    public void Generates_IncrementExpressionResult()
    {
        var ast = Generate("var x = a++;");
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(1).ToList();
        Assert.Equal(3, statements.Count);

        var firstStatement = statements[0];
        var secondStatement = statements[1];
        var thirdStatement = statements[2];
        Assert.IsType<Variable>(firstStatement);
        Assert.IsType<ExpressionStatement>(secondStatement);
        Assert.IsType<VariableList>(thirdStatement);

        var tempVariable = (Variable)firstStatement;
        Assert.Equal("_original", tempVariable.Name.ToString());
        Assert.IsType<IdentifierName>(tempVariable.Initializer);

        var tempInitializer = (IdentifierName)tempVariable.Initializer;
        Assert.Equal("a", tempInitializer.ToString());

        var expressionStatement = (ExpressionStatement)secondStatement;
        Assert.IsType<BinaryOperator>(expressionStatement.Expression);

        var binaryOperator = (BinaryOperator)expressionStatement.Expression;
        Assert.Equal("+=", binaryOperator.Operator);
        Assert.IsType<IdentifierName>(binaryOperator.Left);
        Assert.IsType<Literal>(binaryOperator.Right);

        var left = (IdentifierName)binaryOperator.Left;
        var right = (Literal)binaryOperator.Right;
        Assert.Equal("a", left.ToString());
        Assert.Equal("1", right.ValueText);

        var variableList = (VariableList)thirdStatement;
        Assert.Single(variableList.Variables);

        var variable = variableList.Variables.First();
        Assert.Equal("x", variable.Name.ToString());
        Assert.IsType<IdentifierName>(variable.Initializer);

        var initializer = (IdentifierName)variable.Initializer;
        Assert.Equal("_original", initializer.ToString());
    }

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
        Assert.Equal("+=", binaryOperator.Operator);
        Assert.IsType<IdentifierName>(binaryOperator.Left);
        Assert.IsType<Literal>(binaryOperator.Right);

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
        Assert.Equal(3, doubleNestedBlock.Statements.Count);

        var memberAssignment = doubleNestedBlock.Statements.SkipLast(1).Last();
        Assert.IsType<Assignment>(memberAssignment);

        var assignment = (Assignment)memberAssignment;
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
        Assert.Equal(4, statements.Count);

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
        var ast = Generate($"namespace {name} {{ class Abc; }}");
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

        var classBlock = (Block)scopedBlock.Statements.First();
        Assert.Equal(4, classBlock.Statements.Count);
        Assert.IsType<Variable>(classBlock.Statements[0]);
        Assert.IsType<ScopedBlock>(classBlock.Statements[1]);
        Assert.IsType<Assignment>(classBlock.Statements[2]);
        Assert.IsType<TypeAlias>(classBlock.Statements[3]);
    }

    [Fact]
    public void Generates_Enums()
    {
        const string name = "Abc";
        var ast = Generate($"enum {name} {{ A, B = 5, C }}; Abc.B;");
        Assert.NotEmpty(ast.Statements);

        var globalStatements = ast.Statements.Skip(2).ToList();
        Assert.IsType<ExpressionStatement>(globalStatements.First());

        var expression = ((ExpressionStatement)globalStatements.First()).Expression;
        Assert.IsType<Literal>(expression);

        var literal = (Literal)expression;
        Assert.Equal("5", literal.ValueText);
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
        Assert.Equal("getInt", function.Name.ToString());
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
    public void Generates_Parameters()
    {
        const string source = "void blahrah(int x) {}";

        var ast = Generate(source);
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<Function>(statement);

        var function = (Function)statement;
        Assert.Equal("blahrah", function.Name.ToString());
        Assert.NotNull(function.ReturnType);
        Assert.Equal("()", function.ReturnType.ToString());
        Assert.NotNull(function.Body);
        Assert.Empty(function.Body.Statements);
        Assert.Single(function.ParameterList.Parameters);

        var parameter = function.ParameterList.Parameters.First();
        Assert.Equal("x", parameter.Name.ToString());
        Assert.NotNull(parameter.Type);
        Assert.Equal("number", parameter.Type.ToString());
    }

    [Fact]
    public void Generates_DefaultParameters_WithNullableTypes()
    {
        const string source = "void blah(int y = 69) {}";

        var ast = Generate(source);
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<Function>(statement);

        var function = (Function)statement;
        Assert.Equal("blah", function.Name.ToString());
        Assert.NotNull(function.ReturnType);
        Assert.Equal("()", function.ReturnType.ToString());
        Assert.NotNull(function.Body);
        Assert.Empty(function.Body.Statements);
        Assert.Single(function.ParameterList.Parameters);

        var parameter = function.ParameterList.Parameters.First();
        Assert.Equal("y", parameter.Name.ToString());
        Assert.IsType<OptionalType>(parameter.Type);
        Assert.Equal("number?", parameter.Type.ToString());
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
        foreach (var variable in variableList.Variables) Assert.Equal((index++).ToString(), (variable.Initializer as Literal)?.ValueText);
    }

    [Theory]
    [InlineData("const int a = 1; a;", "1")]
    [InlineData("const bool a = false; a;", "false")]
    [InlineData("const string a = \"foobar\"; a;", "\"foobar\"")]
    public void Generates_ConstantDeclarations(string csharpSource, string expectedValueText)
    {
        var ast = Generate(csharpSource);
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<ExpressionStatement>(statement);

        var expression = ((ExpressionStatement)statement).Expression;
        Assert.IsType<Literal>(expression);

        var literal = (Literal)expression;
        Assert.Equal(expectedValueText, literal.ValueText);
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

        if (expectedValueText == null) return;

        Assert.IsType<Literal>(variable.Initializer);

        var literal = (Literal)variable.Initializer;
        Assert.Equal(expectedValueText, literal.ValueText);
    }

    [Theory]
    [InlineData("object abc123;", "abc123")]
    [InlineData("object @bruh;", "bruh")]
    [InlineData("object yang;", "yang")]
    public void Generates_Identifiers(string csharpSource, string expectedLuauIdentifier)
    {
        var ast = Generate(csharpSource);
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(1).First();
        Assert.IsType<VariableList>(statement);

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

    [Theory]
    [InlineData("""
                void setValue(out int m) {
                    m = 3;
                }
                setValue(out var a);
                """)]
    [InlineData("""
                void setValue(out int m) {
                    m = 3;
                }

                int a;
                setValue(out a);
                """)]
    [InlineData("""
                void setValue(ref int m) {
                    m = 3;
                }
                int a = 4;
                setValue(ref a);
                """)]
    public void Generates_RefKind(string csSource)
    {
        var ast = Generate(csSource);
        Assert.NotEmpty(ast.Statements);

        var statements = ast.Statements.Skip(1).ToList();
        var refFunc = (Function)statements.First();
        Assert.IsType<Function>(refFunc);
        Assert.IsType<Call>(((ExpressionStatement)refFunc.Body!.Statements.First()).Expression);

        var callExpressionStatement = statements.Last() as ExpressionStatement
                                   ?? (statements.Last() as Block)?.Statements.Last() as ExpressionStatement;

        Assert.NotNull(callExpressionStatement);
        Assert.IsType<Call>(callExpressionStatement.Expression);
        Assert.IsType<AnonymousFunction>(((Call)callExpressionStatement.Expression).ArgumentList.Arguments.First().Expression);
    }
}