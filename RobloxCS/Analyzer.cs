using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS;

public sealed class AnalysisResult
{
    public Dictionary<ITypeSymbol, TypeClassInfo> TypeClassInfos { get; } = [];
}

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
        var finalTypeSymbol = _semanticModel.GetTypeInfo(node).Type;
        var nameText = propertyName.Identifier.Text;
        switch (expressionTypeSymbol)
        {
            case { ContainingNamespace.Name: "System", Name: "Type" }:
            {
                _result.TypeClassInfos.TryAdd(expressionTypeSymbol, new TypeClassInfo());

                var typeClassInfo = _result.TypeClassInfos[expressionTypeSymbol];
                typeClassInfo.MemberUses.Add(nameText);

                if (finalTypeSymbol is { ContainingNamespace: { ContainingNamespace.Name: "System", Name: "Reflection" }, Name: "Assembly" }
                 && nameText == "Assembly"
                 && node.Parent is MemberAccessExpressionSyntax parentMemberAccess)
                    typeClassInfo.AssemblyClassInfo.MemberUses.Add(parentMemberAccess.Name.ToString());

                break;
            }
        }

        base.VisitMemberAccessExpression(node);
    }
}