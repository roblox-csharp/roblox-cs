namespace RobloxCS.Tests
{
    public class MainTransformer_Should
    {
        [Theory]
        [InlineData("obj?.Name;")]
        [InlineData("hello?.World;")]
        [InlineData("a.b?.c;")]
        public void SafeNavigation_TransformsWhenNotNull(string source)
        {
            var cleanTree = TranspilerUtility.ParseTree(source);
            var transformedTree = TranspilerUtility.TransformTree(cleanTree, [BuiltInTransformers.Main()]);
            var cleanRoot = cleanTree.GetRoot();
            var cleanTernary = cleanRoot.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().First();
            var transformedRoot = transformedTree.GetRoot();
            var transformedTernary = transformedRoot.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().First();
            Assert.True(cleanTernary.WhenNotNull.IsKind(SyntaxKind.MemberBindingExpression));
            Assert.True(transformedTernary.WhenNotNull.IsKind(SyntaxKind.SimpleMemberAccessExpression));
        }
    }
}