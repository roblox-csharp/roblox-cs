using RobloxCS.Shared;

namespace RobloxCS.Tests;

public class StandardUtilityTest
{
    [Theory]
    [InlineData("float", "0")]
    [InlineData("double", "0")]
    [InlineData("int", "0")]
    [InlineData("uint", "0")]
    [InlineData("short", "0")]
    [InlineData("ushort", "0")]
    [InlineData("byte", "0")]
    [InlineData("sbyte", "0")]
    [InlineData("string", "\"\"")]
    [InlineData("char", "\"\"")]
    [InlineData("bool", "false")]
    [InlineData("nil", "nil")]
    [InlineData("WhatTheFuck", "nil")]
    public void GetDefaultValueForType(string typeName, string expectedValueText)
    {
        var valueText = StandardUtility.GetDefaultValueForType(typeName);
        Assert.Equal(expectedValueText, valueText);
    }

    [Theory]
    [InlineData("abc", "Abc")]
    [InlineData("Abc", "Abc")]
    [InlineData("ABC", "ABC")]
    [InlineData("", "")]
    public void Capitalize(string input, string expectedOutput)
    {
        var output = StandardUtility.Capitalize(input);
        Assert.Equal(expectedOutput, output);
    }

    [Theory]
    [InlineData("&", "band")]
    [InlineData("&=", "band")]
    [InlineData("|", "bor")]
    [InlineData("|=", "bor")]
    [InlineData("^", "bxor")]
    [InlineData("^=", "bxor")]
    [InlineData(">>", "rshift")]
    [InlineData(">>=", "rshift")]
    [InlineData("<<", "lshift")]
    [InlineData("<<=", "lshift")]
    [InlineData(">>>", "arshift")]
    [InlineData(">>>=", "arshift")]
    [InlineData("~", "bnot")]
    public void GetBit32MethodName(string input, string expectedOutput)
    {
        var output = StandardUtility.GetBit32MethodName(input);
        Assert.Equal(expectedOutput, output);
    }
    
    [Theory]
    [InlineData("A<B, C>", "B", "C")]
    [InlineData("A<B>", "B")]
    [InlineData("A<B<C>>", "B<C>")]
    [InlineData("A<D, B<C>>", "D", "B<C>")]
    [InlineData("A<B<C>, D>", "B<C>", "D")]
    [InlineData("A<B<C>, D<E>>", "B<C>", "D<E>")]
    public void ExtractTypeArguments(string input, params string[] expectedOutput)
    {
        var output = StandardUtility.ExtractTypeArguments(input);
        Assert.Equal(expectedOutput.Length, output.Count);

        for (var i = 0; i < expectedOutput.Length; i++)
            Assert.Equal(expectedOutput[i], output[i]);
    }
    
    [Theory]
    [InlineData("++", "+=")]
    [InlineData("--", "-=")]
    [InlineData("!=", "~=")]
    [InlineData("!", "not ")]
    [InlineData("&&", "and")]
    [InlineData("||", "or")]
    [InlineData("*", "*")]
    public void GetMappedOperator(string input, string expectedOutput)
    {
        var output = StandardUtility.GetMappedOperator(input);
        Assert.Equal(expectedOutput, output);
    }
}