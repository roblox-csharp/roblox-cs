using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;

namespace RobloxCS;

public class AnalysisResult
{
    public Dictionary<ITypeSymbol, HashSet<string>> TypeMemberUses { get; } = [];
}

public class Analyzer(FileCompilation file, CSharpCompilation compiler) : CSharpSyntaxWalker
{
    private readonly AnalysisResult _result = new();
    private readonly SemanticModel _semanticModel = compiler.GetSemanticModel(file.Tree);

    public AnalysisResult Analyze(SyntaxNode? root)
    {
        Visit(root);
        return _result;
    }

    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var expressionTypeSymbol = _semanticModel.GetTypeInfo(node.Expression).Type;
        if (expressionTypeSymbol is not { ContainingNamespace.Name: "System", Name: "Type" }) return;
        if (node.Name is not IdentifierNameSyntax propertyName) return;

        _result.TypeMemberUses.TryAdd(expressionTypeSymbol, []);
        _result.TypeMemberUses[expressionTypeSymbol].Add(propertyName.Identifier.Text);
    }
}