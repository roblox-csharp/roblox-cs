using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    public sealed class LuauGenerator(SyntaxTree tree, CSharpCompilation compiler) : BaseGenerator
    {
        private readonly SyntaxTree _tree = tree;
        private readonly SemanticModel _semanticModel = compiler.GetSemanticModel(tree);

        public Luau.AST GetLuauAST()
        {
            return (Luau.AST)Visit(_tree.GetRoot())!;
        }

        public override Luau.Node? VisitCompilationUnit(CompilationUnitSyntax node)
        {
            // TODO: handle usings

            List<Luau.Statement> statements = [];
            foreach (var member in node.Members)
            {
                var statement = Visit(member) as Luau.Statement;
                if (statement == null)
                {
                    throw new Exception($"Unhandled syntax node within \"{member}\"");
                }    
                statements.Add(statement);
            }
            return new Luau.AST(statements);
        }

        public override Luau.Node? VisitIdentifierName(IdentifierNameSyntax node)
        {
            return new Luau.IdentifierName(GetName(node));
        }

        public override Luau.Node? VisitReturnStatement(ReturnStatementSyntax node)
        {
            return new Luau.Return(TryVisit(node.Expression) as Luau.Expression);
        }

        public override Luau.Node? VisitBlock(BlockSyntax node)
        {
            return new Luau.Block(node.Statements.Select(Visit).OfType<Luau.Statement>().ToList());
        }

        public override Luau.Node? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var parameterList = (Luau.ParameterList)Visit(node.ParameterList)!;
            var returnType = CreateTypeRef(node.ReturnType);
            var body = Visit(node.Body) as Luau.Block;
            return new Luau.Function(CreateIdentifierName(node), true, parameterList, returnType, body);
        }

        public override Luau.Node? VisitParameter(ParameterSyntax node)
        {
            var initializer = Visit(node.Default) as Luau.Expression;

            var isParams = HasSyntax(node.Modifiers, SyntaxKind.ParamsKeyword);
            var returnType = CreateTypeRef(node.Type);
            var name = isParams ?
                CreateIdentifierName("...")
                : CreateIdentifierName(node);

            return new Luau.Parameter(name, initializer, returnType);
        }

        public override Luau.Node? VisitParameterList(ParameterListSyntax node)
        {
            return new Luau.ParameterList(node.Parameters.Select(Visit).OfType<Luau.Parameter>().ToList());
        }

        public override Luau.Node? VisitGlobalStatement(GlobalStatementSyntax node)
        {
            return Visit(node.Statement);
        }

        public override Luau.Node? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            return Visit(node.Declaration);
        }

        public override Luau.Node? VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var typeRef = CreateTypeRef(node.Type);
            var variables = node.Variables.Select(Visit).OfType<Luau.Variable>().ToList(); 
            return new Luau.VariableList(variables);
        }

        public override Luau.Node? VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var declaration = node.Parent as VariableDeclarationSyntax;
            var initializer = node.Initializer != null ? Visit(node.Initializer) as Luau.Expression : null;
            return new Luau.Variable(CreateIdentifierName(node), true, initializer, CreateTypeRef(declaration?.Type));
        }

        public override Luau.Node? VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            return Visit(node.Value);
        }

        public override Luau.Node? VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            var valueText = "";
            switch (node.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.Utf8StringLiteralExpression:
                case SyntaxKind.CharacterLiteralExpression:
                    valueText = $"\"{node.Token.ValueText}\"";
                    break;

                case SyntaxKind.NullLiteralExpression:
                    valueText = "nil";
                    break;

                case SyntaxKind.DefaultLiteralExpression:
                    var typeSymbol = _semanticModel.GetTypeInfo(node).Type;
                    if (typeSymbol == null) break;

                    valueText = Utility.GetDefaultValueForType(typeSymbol.Name);
                    break;

                default:
                    valueText = node.Token.ValueText;
                    break;
            }

            return new Luau.Literal(valueText);
        }
    }
}