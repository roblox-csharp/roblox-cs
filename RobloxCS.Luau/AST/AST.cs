using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS.Luau
{
    public class AST : Node
    {
        public List<Statement> Statements { get; }
        public CompilationUnitSyntax CompilationUnit { get; }

        public AST(List<Statement> statements, CompilationUnitSyntax compilationUnit)
        {
            Statements = statements;
            CompilationUnit = compilationUnit;
            AddChildren(Statements);
        }

        public override void Render(LuauWriter luau)
        {
            var hasExports = CompilationUnit.Members.Any(member =>
                member.IsKind(SyntaxKind.NamespaceDeclaration)
                || (
                    member.Modifiers.Any(token => token.IsKind(SyntaxKind.PublicKeyword))
                    && (member.IsKind(SyntaxKind.ClassDeclaration)
                    || member.IsKind(SyntaxKind.EnumDeclaration)
                    || member.IsKind(SyntaxKind.InterfaceDeclaration))
                )
            );
            if (hasExports)
            {
                Statements.Insert(0, new Variable(AstUtility.CreateIdentifierName(CompilationUnit, AstUtility.FixIdentifierNameText(CompilationUnit, "_exports")), true, new TableInitializer()));
            }

            luau.WriteLine("-- Compiled with roblox-cs v2.0.0");
            luau.WriteLine();
            foreach (var statement in Statements)
            {
                statement.Render(luau);
            }

            luau.WriteReturn(hasExports ? AstUtility.CreateIdentifierName(CompilationUnit, "_exports") : null);
        }
    }
}
