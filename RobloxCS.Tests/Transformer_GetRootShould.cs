namespace RobloxCS.Tests
{
    public class Transformer_GetRootShould
    {
        [Theory]
        [InlineData("obj?.Name;")]
        public void GetRoot_TernaryOps_TransformsElseBranch(string source)
        {
            var cleanTree = GetCleanTree(source);
            var transformedTree = TransformTree(cleanTree);
            var cleanRoot = cleanTree.GetRoot();
            var cleanTernary = cleanRoot.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().First();
            var transformedRoot = transformedTree.GetRoot();
            var transformedTernary = transformedRoot.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().First();
            Assert.True(cleanTernary.WhenNotNull.IsKind(SyntaxKind.MemberBindingExpression));
            Assert.True(transformedTernary.WhenNotNull.IsKind(SyntaxKind.SimpleMemberAccessExpression));
        }

        private SyntaxTree TransformTree(SyntaxTree cleanTree)
        {
            var transformer = new Transformer(cleanTree, ConfigReader.UnitTestingConfig);
            var transformedTree = cleanTree.WithRootAndOptions(transformer.GetRoot(), cleanTree.Options);
            return transformedTree;
        }

        private SyntaxTree GetCleanTree(string source)
        {
            var cleanTree = CSharpSyntaxTree.ParseText(source);
            var compilationUnit = (CompilationUnitSyntax)cleanTree.GetRoot();
            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));
            var newRoot = compilationUnit.AddUsings(usingDirective);
            return cleanTree
                .WithRootAndOptions(newRoot, cleanTree.Options)
                .WithFilePath("TransformerTestFile.cs");
        }
    }
}