using RobloxCS.Luau;

namespace RobloxCS.Tests;

public class AstUtilityTest
{
    [Theory]
    [InlineData("var")]
    [InlineData(null)]
    public void CreateTypeRef_ReturnsNull(string? path)
    {
        Assert.Null(AstUtility.CreateTypeRef(path));
    }
    
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