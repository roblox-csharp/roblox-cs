using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS;

public sealed class Analyzer(FileCompilation file, CSharpCompilation compiler) : CSharpSyntaxWalker
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
        if (node.Name is not IdentifierNameSyntax propertyName) return;

        var expressionTypeSymbol = _semanticModel.GetTypeInfo(node.Expression).Type;
        var nameText = propertyName.Identifier.Text;
        switch (expressionTypeSymbol)
        {
            case { ContainingNamespace: { ContainingNamespace.Name: "System", Name: "Reflection" }, Name: "PropertyInfo" }:
            {
                _result.PropertyClassInfo.MemberUses.Add(nameText);
                break;
            }
            case { ContainingNamespace: { ContainingNamespace.Name: "System", Name: "Reflection" }, Name: "Assembly" }:
            {
                _result.AssemblyClassInfo.MemberUses.Add(nameText);
                break;
            }
            case { ContainingNamespace: { ContainingNamespace.Name: "System", Name: "Reflection" }, Name: "MemberInfo" }:
            {
                _result.MemberClassInfo.MemberUses.Add(nameText);
                break;
            }
            case { ContainingNamespace.Name: "System", Name: "Type" }:
            {
                _result.TypeClassInfo.MemberUses.Add(nameText);
                break;
            }
        }

        base.VisitMemberAccessExpression(node);
    }
}