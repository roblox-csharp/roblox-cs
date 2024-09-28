﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

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

        protected Type GetRuntimeType(SyntaxNode node, string fullyQualifiedName)
        {
            Type? type;
            using (var memoryStream = new MemoryStream())
            {
                var result = _semanticModel.Compilation.Emit(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(memoryStream.ToArray());

                // get the type from the loaded assembly
                type = assembly.GetType(fullyQualifiedName)!;
            }

            type ??= Type.GetType(fullyQualifiedName);
            if (type == null)
            {
                throw Logger.CodegenError(node, $"Unable to resolve type '{fullyQualifiedName}'.");
            }

            return type;
        }

        protected Function GenerateConstructor(ClassDeclarationSyntax classDeclaration, ParameterList parameterList, Block? body = null, List<AttributeList>? attributeLists = null)
        {
            var className = AstUtility.CreateIdentifierName(classDeclaration);
            body ??= new Block([]);

            // visit fields/properties being assigned a value outside of the constructor (aka non-static & with initializers)
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

            foreach (var property in nonStaticProperties)
            {
                if (property.Initializer == null) continue;

                var initializer = Visit<Expression>(property.Initializer);
                body.Statements.Add(new ExpressionStatement(
                    new Assignment(
                        new MemberAccess(
                            new IdentifierName("self"),
                            AstUtility.CreateIdentifierName(property)
                        ),
                        initializer
                    )
                ));
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
                new OptionalType(new TypeRef(className.ToString())),
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

        protected string GetFullSymbolName(ISymbol symbol)
        {
            var containerName = symbol.ContainingNamespace != null || symbol.ContainingType != null ? GetFullSymbolName(symbol.ContainingNamespace ?? (ISymbol)symbol.ContainingType) : null;
            return (!string.IsNullOrEmpty(containerName) ? containerName + "." : "") + symbol.Name;
        }

        protected bool IsGlobal(SyntaxNode node)
        {
            return node.Parent.IsKind(SyntaxKind.GlobalStatement) || node.Parent.IsKind(SyntaxKind.CompilationUnit);
        }

        protected bool IsStatic(MemberDeclarationSyntax node)
        {
            var classIsStatic = IsParentClassStatic(node);
            return classIsStatic || HasSyntax(node.Modifiers, SyntaxKind.StaticKeyword);
        }

        protected bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax)
        {
            return tokens.Any(token => token.IsKind(syntax));
        }

        protected bool IsDescendantOf<T>(SyntaxNode node) where T : SyntaxNode
        {
            return FindFirstAncestor<T>(node) != null;
        }

        protected T? FindFirstAncestor<T>(SyntaxNode node) where T : SyntaxNode
        {
            return GetAncestors<T>(node).FirstOrDefault();
        }

        protected List<T> GetAncestors<T>(SyntaxNode node) where T : SyntaxNode
        {
            return node.Ancestors().OfType<T>().ToList();
        }

        private bool IsParentClassStatic(SyntaxNode node)
        {
            return node.Parent is ClassDeclarationSyntax classDeclaration ?
                HasSyntax(classDeclaration.Modifiers, SyntaxKind.StaticKeyword)
                : false;
        }
    }
}