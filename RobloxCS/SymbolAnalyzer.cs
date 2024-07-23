using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    internal sealed class SymbolAnalyzerResults
    {
        public List<INamespaceOrTypeSymbol> TypesWithMembersUsedAsValues { get; set; } = new List<INamespaceOrTypeSymbol>();

        public bool TypeHasMemberUsedAsValue(INamespaceOrTypeSymbol namespaceOrType)
        {
            return TypesWithMembersUsedAsValues.Any(symbol => SymbolEqualityComparer.Default.Equals(symbol, namespaceOrType));
        }
    }

    internal sealed class SymbolAnalyzer
    {
        private readonly SyntaxTree _tree;
        private readonly SemanticModel _semanticModel;
        private readonly SymbolAnalyzerResults _results = new SymbolAnalyzerResults();

        public SymbolAnalyzer(SyntaxTree tree, SemanticModel semanticModel)
        {
            _tree = tree;
            _semanticModel = semanticModel;
        }

        public SymbolAnalyzerResults Analyze()
        {
            AnalyzeUsings();
            AnalyzeNamespaces();
            return _results;
        }

        private void AnalyzeNamespaces()
        {
            var root = _tree.GetRoot();
            var namespaceDeclarations = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();

            foreach (var namespaceDeclaration in namespaceDeclarations)
            {
                var symbolInfo = _semanticModel.GetSymbolInfo(namespaceDeclaration.Name);
                if (symbolInfo.Symbol is INamespaceOrTypeSymbol namespaceOrTypeSymbol && HasMembersUsedAsValues(namespaceOrTypeSymbol, root))
                {
                    _results.TypesWithMembersUsedAsValues.Add(namespaceOrTypeSymbol);
                }
            }
        }

        private void AnalyzeUsings()
        {
            var root = _tree.GetRoot();
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

            foreach (var usingDirective in usingDirectives)
            {
                if (usingDirective.Name == null) continue;

                var symbolInfo = _semanticModel.GetSymbolInfo(usingDirective.Name);
                if (symbolInfo.Symbol is INamespaceOrTypeSymbol namespaceOrTypeSymbol && HasMembersUsedAsValues(namespaceOrTypeSymbol, root))
                {
                    _results.TypesWithMembersUsedAsValues.Add(namespaceOrTypeSymbol);
                }
            }
        }
        
        private bool HasMembersUsedAsValues(INamespaceOrTypeSymbol namespaceOrTypeSymbol, SyntaxNode root)
        {
            var typeUsages = root.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(identifier => 
                    _semanticModel.GetSymbolInfo(identifier).Symbol is INamedTypeSymbol symbol
                        && SymbolEqualityComparer.Default.Equals(symbol.ContainingNamespace, namespaceOrTypeSymbol)
                );

            foreach (var usage in typeUsages)
            {
                if (IsUsedAsValue(usage))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsUsedAsValue(SyntaxNode node)
        {
            // Check for object instantiation, member access, or variable declarator
            if (node.Parent is ObjectCreationExpressionSyntax ||
                node.Parent is MemberAccessExpressionSyntax ||
                node.Parent is VariableDeclaratorSyntax ||
                node.Parent is AssignmentExpressionSyntax)
            {
                return true;
            }

            // Check if used in a method invocation
            if (node.Parent is InvocationExpressionSyntax invocation)
            {
                var symbolInfo = _semanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                {
                    return methodSymbol.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type, _semanticModel.GetTypeInfo(node).Type));
                }
            }

            return false;
        }
    }
}