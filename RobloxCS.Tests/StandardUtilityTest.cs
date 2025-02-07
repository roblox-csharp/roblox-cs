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
}