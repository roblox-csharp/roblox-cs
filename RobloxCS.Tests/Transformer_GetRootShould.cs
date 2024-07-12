namespace RobloxCS.Tests
{
    public class Transformer_GetRootShould
    {
        [Theory]
        [InlineData("obj?.Name;")]
        [InlineData("hello?.World;")]
        [InlineData("a.b?.c;")]
        public void GetRoot_TernaryOps_TransformsElseBranch(string source)
        {
            var cleanTree = TranspilerUtility.ParseTree(source);
            var transformedTree = TranspilerUtility.TransformTree(cleanTree);
            var cleanRoot = cleanTree.GetRoot();
            var cleanTernary = cleanRoot.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().First();
            var transformedRoot = transformedTree.GetRoot();
            var transformedTernary = transformedRoot.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().First();
            Assert.True(cleanTernary.WhenNotNull.IsKind(SyntaxKind.MemberBindingExpression));
            Assert.True(transformedTernary.WhenNotNull.IsKind(SyntaxKind.SimpleMemberAccessExpression));
        }
    }
}