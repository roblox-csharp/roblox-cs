using Microsoft.CodeAnalysis.CSharp;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS.Tests;

public class AstUtilityTest
{
    public AstUtilityTest() => Logger.Exit = false;

    [Theory]
    [InlineData("CS")]
    [InlineData("then")]
    [InlineData("and")]
    [InlineData("end")]
    [InlineData("do")]
    [InlineData("typeof")]
    [InlineData("type")]
    [InlineData("export")]
    public void ThrowsWithReservedIdentifier(string identifier)
    {
        Assert.Throws<CleanExitException>(() => AstUtility.CreateSimpleName(SyntaxFactory.LiteralExpression(SyntaxKind
                                                                                                                .NullLiteralExpression),
                                                                            identifier));
    }

    [Fact]
    public void AddOne()
    {
        var expression = AstUtility.AddOne(new IdentifierName("a"));
        Assert.IsType<BinaryOperator>(expression);

        var binaryOperator = (BinaryOperator)expression;
        Assert.IsType<IdentifierName>(binaryOperator.Left);
        Assert.IsType<Literal>(binaryOperator.Right);
        Assert.Equal("+", binaryOperator.Operator);

        var identifier = (IdentifierName)binaryOperator.Left;
        var literal = (Literal)binaryOperator.Right;
        Assert.Equal("a", identifier.Text);
        Assert.Equal("1", literal.ValueText);
    }

    [Fact]
    public void AddOne_AddsToLiteralValue()
    {
        var sum = AstUtility.AddOne(new Literal("1"));
        Assert.IsType<Literal>(sum);

        var literal = (Literal)sum;
        Assert.Equal("2", literal.ValueText);
    }

    [Fact]
    public void SubtractOne()
    {
        var expression = AstUtility.SubtractOne(new IdentifierName("a"));
        Assert.IsType<BinaryOperator>(expression);

        var binaryOperator = (BinaryOperator)expression;
        Assert.IsType<IdentifierName>(binaryOperator.Left);
        Assert.IsType<Literal>(binaryOperator.Right);
        Assert.Equal("-", binaryOperator.Operator);

        var identifier = (IdentifierName)binaryOperator.Left;
        var literal = (Literal)binaryOperator.Right;
        Assert.Equal("a", identifier.Text);
        Assert.Equal("1", literal.ValueText);
    }

    [Fact]
    public void SubtractOne_SubtractsFromLiteralValue()
    {
        var sum = AstUtility.SubtractOne(new Literal("2"));
        Assert.IsType<Literal>(sum);

        var literal = (Literal)sum;
        Assert.Equal("1", literal.ValueText);
    }

    [Theory]
    [InlineData("var")]
    [InlineData(null)]
    public void CreateTypeRef_ReturnsNull(string? path) => Assert.Null(AstUtility.CreateTypeRef(path));

    [Theory]
    [InlineData("string?", typeof(OptionalType))]
    [InlineData("{ number }", typeof(ArrayType))]
    [InlineData("{ [string]: number }", typeof(MappedType))]
    public void CreateTypeRef_ReturnsCorrectTypeNode(string? path, Type typeNodeType)
    {
        var typeRef = AstUtility.CreateTypeRef(path);
        Assert.NotNull(typeRef);
        Assert.IsType(typeNodeType, typeRef);
    }

    [Fact]
    public void CreateTypeRef_HandlesNestedPatterns()
    {
        const string rawPath = "{ [string]: { bool } }??";
        var typeRef = AstUtility.CreateTypeRef(rawPath);
        Assert.NotNull(typeRef);
        Assert.IsType<OptionalType>(typeRef);

        var optionalType = (OptionalType)typeRef;
        Assert.IsType<MappedType>(optionalType.NonNullableType);

        var mappedType = (MappedType)optionalType.NonNullableType;
        Assert.Equal("string", mappedType.KeyType.Path);
        Assert.IsType<ArrayType>(mappedType.ValueType);

        var arrayType = (ArrayType)mappedType.ValueType;
        Assert.Equal("boolean", arrayType.ElementType.Path);
    }
}