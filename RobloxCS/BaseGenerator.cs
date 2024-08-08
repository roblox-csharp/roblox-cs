using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS.Luau
{
    /// <summary>Basically just defines utility methods for LuauGenerator</summary>
    public class BaseGenerator(SyntaxTree tree, CSharpCompilation compiler) : CSharpSyntaxVisitor<Node>
    {
        protected readonly SyntaxTree _tree = tree;
        protected readonly SemanticModel _semanticModel = compiler.GetSemanticModel(tree);

        private readonly HashSet<SyntaxKind> multiLineCommentSyntaxes = [
            SyntaxKind.MultiLineCommentTrivia,
            SyntaxKind.MultiLineDocumentationCommentTrivia
        ];
        private readonly SyntaxKind[] commentSyntaxes = [
            SyntaxKind.SingleLineCommentTrivia,
            SyntaxKind.SingleLineDocumentationCommentTrivia,
            SyntaxKind.MultiLineCommentTrivia,
            SyntaxKind.MultiLineDocumentationCommentTrivia
        ];

        protected TNode Visit<TNode>(SyntaxNode? node) where TNode : Node?
        {
            return (TNode)Visit(node)!;
        }

        protected Function GenerateConstructor(ClassDeclarationSyntax classDeclaration, ParameterList parameterList, Block? body = null, List<AttributeList>? attributeLists = null)
        {
            var className = AstUtility.CreateIdentifierName(classDeclaration);
            body ??= new Block([]);

            // visit fields being assigned a value outside of the constructor
            var nonStaticFields = classDeclaration.Members
                .OfType<FieldDeclarationSyntax>()
                .Where(field => !HasSyntax(field.Modifiers, SyntaxKind.StaticKeyword));
            foreach (var field in nonStaticFields)
            {
                foreach (var declarator in field.Declaration.Variables)
                {
                    if (declarator.Initializer == null) continue;

                    var initializer = Visit<Expression>(declarator.Initializer);
                    body.Statements.Add(new ExpressionStatement(
                        new Assignment(
                            new MemberAccess(
                                new IdentifierName("self"),
                                AstUtility.CreateIdentifierName(declarator)
                            ),
                            initializer
                        )
                    ));
                }
            }

            // add an explicit return (for native codegen) if there isn't one
            if (!body.Statements.Any(statement => statement is Return))
            {
                body.Statements.Add(new Return(AstUtility.Nil()));
            }

            return new Function(
                new AssignmentFunctionName(className, className, ':'),
                false,
                parameterList,
                new TypeRef("nil"),
                body,
                attributeLists
            );
        }

        protected string GetName(SyntaxNode node)
        {
            return Utility.GetNamesFromNode(node).First();
        }

        protected string? TryGetName(SyntaxNode? node)
        {
            return Utility.GetNamesFromNode(node).FirstOrDefault();
        }

        protected bool IsGlobal(SyntaxNode node)
        {
            return node.Parent.IsKind(SyntaxKind.GlobalStatement) || node.Parent.IsKind(SyntaxKind.CompilationUnit);
        }

        protected bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax)
        {
            return tokens.Any(token => token.IsKind(syntax));
        }
    }
}