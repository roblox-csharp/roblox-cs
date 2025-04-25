using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Shared;

namespace RobloxCS;

/// <summary>Basically just defines utility methods for LuauGenerator</summary>
public class BaseGenerator(SyntaxTree tree, CSharpCompilation compiler) : CSharpSyntaxVisitor<Node>
{
    private readonly SyntaxKind[] _commentSyntaxes =
    [
        SyntaxKind.SingleLineCommentTrivia,
        SyntaxKind.SingleLineDocumentationCommentTrivia,
        SyntaxKind.MultiLineCommentTrivia,
        SyntaxKind.MultiLineDocumentationCommentTrivia
    ];

    private readonly HashSet<SyntaxKind> _multiLineCommentSyntaxes = [SyntaxKind.MultiLineCommentTrivia, SyntaxKind.MultiLineDocumentationCommentTrivia];
    protected readonly SyntaxTree _tree = tree;
    protected SemanticModel _semanticModel = compiler.GetSemanticModel(tree);

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

        foreach (var field in nonStaticFields)
        {
            foreach (var declarator in field.Declaration.Variables)
            {
                var initializer = GetFieldInitializer(field.Declaration.Type, declarator.Initializer);

                // stupid hack
                body.Statements.Insert(0,
                                       new Assignment(new MemberAccess(new IdentifierName("self"),
                                                                       AstUtility.CreateSimpleName(declarator)),
                                                      initializer));
            }
        }

        foreach (var property in nonStaticProperties)
        {
            var initializer = GetFieldInitializer(property.Type, property.Initializer);
            body.Statements.Insert(0,
                                   new Assignment(new MemberAccess(new IdentifierName("self"),
                                                                   AstUtility.CreateSimpleName(property)),
                                                  initializer));
        }

        // add an explicit return (for native codegen) if there isn't one
        if (!body.Statements.Any(statement => statement is Return)) body.Statements.Add(new Return(AstUtility.Nil));

        return new Function(new QualifiedName(nonGenericName, className, ':'),
                            false,
                            parameterList,
                            new OptionalType(AstUtility.CreateTypeRef(className.ToString())!),
                            body,
                            attributeLists);
    }

    protected Expression GetFieldInitializer(TypeSyntax type, EqualsValueClauseSyntax? initializer)
    {
        var defaultValue = AstUtility.Nil;
        var explicitInitializer = Visit<Expression?>(initializer);

        if (initializer != null) return explicitInitializer ?? defaultValue;

        var typeSymbol = _semanticModel.GetTypeInfo(type).Type;

        if (typeSymbol == null) return explicitInitializer ?? defaultValue;

        defaultValue = new Literal(StandardUtility.GetDefaultValueForType(typeSymbol.Name));

        return explicitInitializer ?? defaultValue;
    }

    protected string GetName(SyntaxNode node) => StandardUtility.GetNamesFromNode(node).First();

    protected string? TryGetName(SyntaxNode? node) => StandardUtility.GetNamesFromNode(node).FirstOrDefault();

    protected bool IsStatic(MemberDeclarationSyntax node) => IsParentClassStatic(node) || HasSyntax(node.Modifiers, SyntaxKind.StaticKeyword);

    protected bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax) => tokens.Any(token => token.IsKind(syntax));

    protected bool IsDescendantOf<T>(SyntaxNode node)
        where T : SyntaxNode =>
        FindFirstAncestor<T>(node) != null;

    protected T? FindFirstAncestor<T>(SyntaxNode node)
        where T : SyntaxNode =>
        GetAncestors<T>(node).FirstOrDefault();

    private static List<T> GetAncestors<T>(SyntaxNode node)
        where T : SyntaxNode =>
        node.Ancestors().OfType<T>().ToList();

    private bool IsParentClassStatic(SyntaxNode node) =>
        node.Parent is ClassDeclarationSyntax classDeclaration && HasSyntax(classDeclaration.Modifiers, SyntaxKind.StaticKeyword);
}