using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS;

/// <summary>Basically just defines utility methods for LuauGenerator</summary>
public class BaseGenerator(FileCompilation file, CSharpCompilation compiler) : CSharpSyntaxVisitor<Node>
{
    private readonly SyntaxKind[] _commentSyntaxes =
    [
        SyntaxKind.SingleLineCommentTrivia,
        SyntaxKind.SingleLineDocumentationCommentTrivia,
        SyntaxKind.MultiLineCommentTrivia,
        SyntaxKind.MultiLineDocumentationCommentTrivia
    ];
    protected readonly FileCompilation _file = file;

    private readonly HashSet<SyntaxKind> _multiLineCommentSyntaxes = [SyntaxKind.MultiLineCommentTrivia, SyntaxKind.MultiLineDocumentationCommentTrivia];
    protected SemanticModel _semanticModel = compiler.GetSemanticModel(file.Tree);

    protected TNode Visit<TNode>(SyntaxNode? node)
        where TNode : Node? =>
        (TNode)Visit(node)!;

    /// <summary>Generates a Luau class constructor from a C# class declaration</summary>
    protected Function GenerateConstructor(ClassDeclarationSyntax classDeclaration,
                                           ParameterList parameterList,
                                           Block? body = null,
                                           List<AttributeList>? attributeLists = null)
    {
        var className = AstUtility.CreateSimpleName(classDeclaration);
        var nonGenericName = AstUtility.GetNonGenericName(className);
        body ??= new Block([]);

        // visit fields/properties being assigned a value outside the constructor (aka non-static & with initializers)
        var nonStaticFields = classDeclaration.Members
                                              .OfType<FieldDeclarationSyntax>()
                                              .Where(field => !HasSyntax(field.Modifiers, SyntaxKind.StaticKeyword));

        var nonStaticProperties = classDeclaration.Members
                                                  .OfType<PropertyDeclarationSyntax>()
                                                  .Where(field => !HasSyntax(field.Modifiers, SyntaxKind.StaticKeyword));

        var nonStaticEvents = classDeclaration.Members
                                              .OfType<EventFieldDeclarationSyntax>()
                                              .Where(field => !HasSyntax(field.Modifiers, SyntaxKind.StaticKeyword));

        foreach (var field in nonStaticFields)
        {
            foreach (var declarator in field.Declaration.Variables)
            {
                var initializer = GetFieldOrPropertyInitializer(classDeclaration, field.Declaration.Type, declarator.Initializer);
                if (initializer == null) continue;

                body.Statements.Insert(0,
                                       new Assignment(new MemberAccess(new IdentifierName("self"),
                                                                       AstUtility.CreateSimpleName(declarator)),
                                                      initializer));
            }
        }

        foreach (var property in nonStaticProperties)
        {
            var initializer = GetFieldOrPropertyInitializer(classDeclaration, property.Type, property.Initializer);
            if (initializer == null) continue;

            body.Statements.Insert(0,
                                   new Assignment(new MemberAccess(new IdentifierName("self"),
                                                                   AstUtility.CreateSimpleName(property)),
                                                  initializer));
        }

        foreach (var eventField in nonStaticEvents)
        {
            foreach (var declarator in eventField.Declaration.Variables)
                body.Statements.Insert(0,
                                       new Assignment(new MemberAccess(new IdentifierName("self"),
                                                                       AstUtility.CreateSimpleName(declarator)),
                                                      AstUtility.NewSignal()));
        }

        // add an explicit return (for strict mode) if there isn't one
        if (!body.Statements.Any(statement => statement is Return))
            body.Statements.Add(new Return(AstUtility.Nil));

        return new Function(new QualifiedName(nonGenericName, className, ':'),
                            false,
                            parameterList,
                            new OptionalType(AstUtility.CreateTypeRef(className.ToString())!),
                            body,
                            attributeLists);
    }

    protected Expression? GetFieldOrPropertyInitializer(ClassDeclarationSyntax classDeclaration, TypeSyntax type, EqualsValueClauseSyntax? initializer)
    {
        var explicitInitializer = Visit<Expression?>(initializer);
        if (initializer != null)
            return explicitInitializer;

        var typeSymbol = _semanticModel.GetTypeInfo(type).Type;
        if (typeSymbol == null)
            return explicitInitializer;

        var symbol = _semanticModel.GetSymbolInfo(type).Symbol;
        if (symbol != null && IsInitializedInConstructor(classDeclaration, symbol))
            return null;

        var defaultValue = new Literal(StandardUtility.GetDefaultValueForType(typeSymbol.Name));
        return explicitInitializer ?? defaultValue;
    }

    protected static string GetName(SyntaxNode node) => StandardUtility.GetNamesFromNode(node).First();

    protected static string? TryGetName(SyntaxNode? node) => StandardUtility.GetNamesFromNode(node).FirstOrDefault();

    protected static bool IsStatic(MemberDeclarationSyntax node) => IsParentClassStatic(node) || HasSyntax(node.Modifiers, SyntaxKind.StaticKeyword);

    protected static bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax) => tokens.Any(token => token.IsKind(syntax));

    protected static T? FindFirstAncestor<T>(SyntaxNode node)
        where T : SyntaxNode =>
        GetAncestors<T>(node).FirstOrDefault();

    private bool IsInitializedInConstructor(ClassDeclarationSyntax classDeclaration, ISymbol symbol)
    {
        var constructors = classDeclaration.Members
                                           .OfType<ConstructorDeclarationSyntax>()
                                           .Where(c => !HasSyntax(c.Modifiers, SyntaxKind.StaticKeyword))
                                           .ToList();

        if (constructors.Count == 0)
            return false;

        foreach (var constructor in constructors)
        {
            if (constructor.ExpressionBody == null || constructor.Body == null) continue;

            var flow = constructor.ExpressionBody != null
                ? _semanticModel.AnalyzeDataFlow(constructor.ExpressionBody)
                : _semanticModel.AnalyzeDataFlow(constructor.Body);

            if (flow == null) continue;
            if (flow.DefinitelyAssignedOnExit.Contains(symbol)) continue;

            return false;
        }

        return true;
    }

    private static List<T> GetAncestors<T>(SyntaxNode node)
        where T : SyntaxNode =>
        node.Ancestors().OfType<T>().ToList();

    private static bool IsParentClassStatic(SyntaxNode node) =>
        node.Parent is ClassDeclarationSyntax classDeclaration && HasSyntax(classDeclaration.Modifiers, SyntaxKind.StaticKeyword);
}