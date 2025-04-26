using RobloxCS.Luau;
using RobloxCS.Macros;

namespace RobloxCS.Tests.MacroTests;

public class ListMacrosTest : Base.Generation
{
    [Fact]
    public void Macros_Add()
    {
        var ast = Generate("List<int> l = []; l.Add(69);");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<ExpressionStatement>(statement);

        var expression = ((ExpressionStatement)statement).Expression;
        Assert.IsType<Call>(expression);

        var call = (Call)expression;
        Assert.NotNull(call.ExpandedByMacro);
        Assert.Equal(MacroKind.ListMethod, call.ExpandedByMacro);
        Assert.Equal(2, call.ArgumentList.Arguments.Count);
        Assert.IsType<MemberAccess>(call.Callee);

        var selfArgument = call.ArgumentList.Arguments.First().Expression;
        var elementArgument = call.ArgumentList.Arguments.Last().Expression;
        Assert.IsType<IdentifierName>(selfArgument);
        Assert.IsType<Literal>(elementArgument);
        Assert.Equal("l", selfArgument.ToString());

        var element = (Literal)elementArgument;
        Assert.Equal("69", element.ValueText);

        var memberAccess = (MemberAccess)call.Callee;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        Assert.Equal("table", memberAccess.Expression.ToString());
        Assert.Equal("insert", memberAccess.Name.ToString());
    }

    [Fact]
    public void Macros_Insert()
    {
        var ast = Generate("List<int> l = []; l.Insert(0, 69);");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<ExpressionStatement>(statement);

        var expression = ((ExpressionStatement)statement).Expression;
        Assert.IsType<Call>(expression);

        var call = (Call)expression;
        Assert.NotNull(call.ExpandedByMacro);
        Assert.Equal(MacroKind.ListMethod, call.ExpandedByMacro);
        Assert.Equal(3, call.ArgumentList.Arguments.Count);
        Assert.IsType<MemberAccess>(call.Callee);

        var selfArgument = call.ArgumentList.Arguments[0].Expression;
        var indexArgument = call.ArgumentList.Arguments[1].Expression;
        var elementArgument = call.ArgumentList.Arguments[2].Expression;
        Assert.IsType<IdentifierName>(selfArgument);
        Assert.IsType<Literal>(indexArgument);
        Assert.IsType<Literal>(elementArgument);
        Assert.Equal("l", selfArgument.ToString());

        var index = (Literal)indexArgument;
        Assert.Equal("1", index.ValueText);

        var element = (Literal)elementArgument;
        Assert.Equal("69", element.ValueText);

        var memberAccess = (MemberAccess)call.Callee;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        Assert.Equal("table", memberAccess.Expression.ToString());
        Assert.Equal("insert", memberAccess.Name.ToString());
    }

    [Fact]
    public void Macros_Clear()
    {
        var ast = Generate("List<int> l = []; l.Clear();");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<ExpressionStatement>(statement);

        var expression = ((ExpressionStatement)statement).Expression;
        Assert.IsType<Call>(expression);

        var call = (Call)expression;
        Assert.NotNull(call.ExpandedByMacro);
        Assert.Equal(MacroKind.ListMethod, call.ExpandedByMacro);
        Assert.Single(call.ArgumentList.Arguments);
        Assert.IsType<MemberAccess>(call.Callee);

        var selfArgument = call.ArgumentList.Arguments.First().Expression;
        Assert.IsType<IdentifierName>(selfArgument);
        Assert.Equal("l", selfArgument.ToString());

        var memberAccess = (MemberAccess)call.Callee;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        Assert.Equal("table", memberAccess.Expression.ToString());
        Assert.Equal("clear", memberAccess.Name.ToString());
    }

    [Fact]
    public void Macros_Contains()
    {
        var ast = Generate("List<int> l = []; l.Contains(69);");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<Variable>(statement);

        var expression = ((Variable)statement).Initializer;
        Assert.IsType<BinaryOperator>(expression);

        var binaryOperator = (BinaryOperator)expression;
        Assert.NotNull(binaryOperator.ExpandedByMacro);
        Assert.Equal(MacroKind.ListMethod, binaryOperator.ExpandedByMacro);
        Assert.Equal("~=", binaryOperator.Operator);
        Assert.IsType<Call>(binaryOperator.Left);
        Assert.IsType<Literal>(binaryOperator.Right);

        var call = (Call)binaryOperator.Left;
        Assert.Equal(2, call.ArgumentList.Arguments.Count);
        Assert.IsType<MemberAccess>(call.Callee);

        var selfArgument = call.ArgumentList.Arguments.First().Expression;
        var elementArgument = call.ArgumentList.Arguments.Last().Expression;
        Assert.IsType<IdentifierName>(selfArgument);
        Assert.IsType<Literal>(elementArgument);
        Assert.Equal("l", selfArgument.ToString());

        var memberAccess = (MemberAccess)call.Callee;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        Assert.Equal("table", memberAccess.Expression.ToString());
        Assert.Equal("find", memberAccess.Name.ToString());
    }

    [Fact]
    public void Macros_Remove()
    {
        var ast = Generate("List<int> l = []; l.Remove(69);");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<Variable>(statement);

        var expression = ((Variable)statement).Initializer;
        Assert.IsType<Call>(expression);

        var call = (Call)expression;
        Assert.NotNull(call.ExpandedByMacro);
        Assert.Equal(MacroKind.ListMethod, call.ExpandedByMacro);
        Assert.Equal(2, call.ArgumentList.Arguments.Count);
        Assert.IsType<MemberAccess>(call.Callee);

        var selfArgument = call.ArgumentList.Arguments.First().Expression;
        var indexArgument = call.ArgumentList.Arguments.Last().Expression;
        Assert.Equal("l", selfArgument.ToString());
        Assert.IsType<IdentifierName>(selfArgument);
        Assert.IsType<Call>(indexArgument);

        var indexCall = (Call)indexArgument;
        Assert.Equal(2, indexCall.ArgumentList.Arguments.Count);
        Assert.IsType<MemberAccess>(indexCall.Callee);

        var elementArgument = indexCall.ArgumentList.Arguments.Last().Expression;
        var element = (Literal)elementArgument;
        Assert.Equal("69", element.ValueText);

        var indexMemberAccess = (MemberAccess)indexCall.Callee;
        Assert.IsType<IdentifierName>(indexMemberAccess.Expression);
        Assert.IsType<IdentifierName>(indexMemberAccess.Name);
        Assert.Equal("table", indexMemberAccess.Expression.ToString());
        Assert.Equal("find", indexMemberAccess.Name.ToString());

        var memberAccess = (MemberAccess)call.Callee;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        Assert.Equal("table", memberAccess.Expression.ToString());
        Assert.Equal("remove", memberAccess.Name.ToString());
    }

    [Fact]
    public void Macros_RemoveAt()
    {
        var ast = Generate("List<int> l = []; l.RemoveAt(0);");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<ExpressionStatement>(statement);

        var expression = ((ExpressionStatement)statement).Expression;
        Assert.IsType<Call>(expression);

        var call = (Call)expression;
        Assert.NotNull(call.ExpandedByMacro);
        Assert.Equal(MacroKind.ListMethod, call.ExpandedByMacro);
        Assert.Equal(2, call.ArgumentList.Arguments.Count);
        Assert.IsType<MemberAccess>(call.Callee);

        var selfArgument = call.ArgumentList.Arguments.First().Expression;
        var indexArgument = call.ArgumentList.Arguments.Last().Expression;
        Assert.Equal("l", selfArgument.ToString());
        Assert.IsType<IdentifierName>(selfArgument);
        Assert.IsType<Literal>(indexArgument);

        var index = (Literal)indexArgument;
        Assert.Equal("1", index.ValueText);

        var memberAccess = (MemberAccess)call.Callee;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        Assert.Equal("table", memberAccess.Expression.ToString());
        Assert.Equal("remove", memberAccess.Name.ToString());
    }

    [Fact]
    public void Macros_AsReadOnly()
    {
        var ast = Generate("List<int> l = []; l.AsReadOnly();");
        Assert.NotEmpty(ast.Statements);

        var statement = ast.Statements.Skip(2).First();
        Assert.IsType<Variable>(statement);

        var expression = ((Variable)statement).Initializer;
        Assert.IsType<Call>(expression);

        var call = (Call)expression;
        Assert.NotNull(call.ExpandedByMacro);
        Assert.Equal(MacroKind.ListMethod, call.ExpandedByMacro);
        Assert.Single(call.ArgumentList.Arguments);
        Assert.IsType<MemberAccess>(call.Callee);

        var selfArgument = call.ArgumentList.Arguments.First().Expression;
        Assert.IsType<IdentifierName>(selfArgument);
        Assert.Equal("l", selfArgument.ToString());

        var memberAccess = (MemberAccess)call.Callee;
        Assert.IsType<IdentifierName>(memberAccess.Expression);
        Assert.IsType<IdentifierName>(memberAccess.Name);
        Assert.Equal("table", memberAccess.Expression.ToString());
        Assert.Equal("freeze", memberAccess.Name.ToString());
    }
}