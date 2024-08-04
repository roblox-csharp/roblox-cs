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
            return Visit<Luau.AST>(_tree.GetRoot());
        }

        public override Luau.AST VisitCompilationUnit(CompilationUnitSyntax node)
        {
            // TODO: handle usings

            List<Luau.Statement> statements = [];
            foreach (var member in node.Members)
            {
                var statement = Visit<Luau.Statement?>(member);
                if (statement == null)
                {
                    throw new Exception($"Unhandled syntax node within \"{member}\" ({member.Kind()})");
                }    
                statements.Add(statement);
            }
            return new Luau.AST(statements);
        }

        public override Luau.Call VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var methodSymbol = _semanticModel.GetDeclaredSymbol(node.Expression)!;
            var callee = Visit<Luau.Expression>(node.Expression);
            if (callee is Luau.MemberAccess memberAccess)
            {
                memberAccess.Operator = methodSymbol.IsStatic ? '.' : ':';
            }

            List<Luau.Expression> arguments = [];
            foreach (var argument in node.ArgumentList.Arguments)
            {
                arguments.Add(Visit<Luau.Expression>(argument.Expression));
            }
            return new Luau.Call(callee, arguments);
        }

        public override Luau.Variable VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
        {
            var name = Visit<Luau.IdentifierName?>(node.NameEquals?.Name);
            if (name == null)
            {
                var expressionName = _semanticModel.GetSymbolInfo(node.Expression).Symbol!.Name;
                if (expressionName.Contains('.'))
                {
                    expressionName = expressionName.Split('.').Last();
                }
                name = new Luau.IdentifierName(expressionName);
            }

            var value = Visit<Luau.Expression?>(node.Expression);
            return new Luau.Variable(name, true, value);
        }

        public override Luau.TableInitializer VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
        {
            List<Luau.Expression> values = [];
            List<Luau.Expression> keys = [];
            foreach (var member in node.Initializers)
            {
                var declaration = Visit<Luau.Variable>(member)!;
                var key = new Luau.Literal($"\"{declaration.Name}\"");
                var value = declaration.Initializer!;
                keys.Add(key);
                values.Add(value);
            }

            return new Luau.TableInitializer(values, keys);
        }

        public override Luau.MemberAccess VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var expression = Visit<Luau.Expression>(node.Expression);
            var name = Visit<Luau.IdentifierName>(node.Name);
            return new Luau.MemberAccess(expression, name);
        }

        public override Luau.IdentifierName VisitIdentifierName(IdentifierNameSyntax node)
        {
            return new Luau.IdentifierName(GetName(node));
        }

        public override Luau.Return VisitReturnStatement(ReturnStatementSyntax node)
        {
            return new Luau.Return(Visit<Luau.Expression?>(node.Expression));
        }

        public override Luau.Block VisitBlock(BlockSyntax node)
        {
            return new Luau.Block(node.Statements.Select(Visit).OfType<Luau.Statement>().ToList());
        }

        public override Luau.Node VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var left = Visit<Luau.Expression>(node.Left);
            var right = Visit<Luau.Expression>(node.Right);
            var mappedOperator = Luau.Utility.GetMappedOperator(node.OperatorToken.Text);
            var bit32MethodName = Luau.Utility.GetBit32MethodName(mappedOperator);
            if (bit32MethodName != null)
            {
                return new Luau.Call(
                    new Luau.MemberAccess(
                        new Luau.IdentifierName("bit32"),
                        new Luau.IdentifierName(bit32MethodName)
                    ),
                    [left, right]
                );
            }

            return new Luau.BinaryExpression(left, mappedOperator, right);
        }

        public override Luau.Node VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            var operand = Visit<Luau.Expression>(node.Operand);
            var operandType = _semanticModel.GetTypeInfo(node.Operand).Type!;
            if (node.OperatorToken.Text == "!")
            {
                return new Luau.TypeCast(operand, new Luau.TypeRef(operandType.Name.Replace("?", "")));
            }

            var mappedOperator = Luau.Utility.GetMappedOperator(node.OperatorToken.Text);
            return new Luau.BinaryExpression(operand, mappedOperator, new Luau.Literal("1"));
        }

        public override Luau.Node? VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var operatorText = node.OperatorToken.Text;
            if (operatorText == "^")
            {
                // TODO: error (unsupported)
                return null;
            }

            var operand = Visit<Luau.Expression>(node.Operand);
            if (operatorText == "+")
            {
                return operand;
            }

            var mappedOperator = Luau.Utility.GetMappedOperator(operatorText);
            var bit32MethodName = Luau.Utility.GetBit32MethodName(mappedOperator);
            if (bit32MethodName != null)
            {
                return new Luau.Call(
                    new Luau.MemberAccess(
                        new Luau.IdentifierName("bit32"),
                        new Luau.IdentifierName(bit32MethodName)
                    ),
                    [operand]
                );
            }

            return new Luau.UnaryExpression(mappedOperator, operand);
        }

        public override Luau.AnonymousFunction VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
            var body = Visit<Luau.Block?>(node.Body);
            return new Luau.AnonymousFunction(parameterList, body);
        }

        public override Luau.Function VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var name = CreateIdentifierName(node);
            var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
            var returnType = CreateTypeRef(node.ReturnType);
            var body = Visit<Luau.Block?>(node.Body);
            var attributeLists = node.AttributeLists.Select(Visit<Luau.AttributeList>).ToList();
            return new Luau.Function(name, true, parameterList, returnType, body, attributeLists);
        }

        public override Luau.Parameter VisitParameter(ParameterSyntax node)
        {
            var name = CreateIdentifierName(node);
            var returnType = CreateTypeRef(node.Type);
            var initializer = Visit<Luau.Expression?>(node.Default);
            var isParams = HasSyntax(node.Modifiers, SyntaxKind.ParamsKeyword);
            return new Luau.Parameter(name, isParams, initializer, returnType);
        }

        public override Luau.Node? VisitAttribute(AttributeSyntax node)
        {
            switch (GetName(node))
            {
                case "Native":
                    return new Luau.BuiltinAttribute(new Luau.IdentifierName("native"));
            }

            // TODO: throw cuz regular attributes aren't supported yet
            return null;
        }

        public override Luau.AttributeList VisitAttributeList(AttributeListSyntax node)
        {
            List<Luau.BaseAttribute> attributes = [];
            foreach (var attribute in node.Attributes)
            {
                attributes.Add(Visit<Luau.BaseAttribute>(attribute));
            }
            return new Luau.AttributeList(attributes);
        }

        public override Luau.ParameterList VisitParameterList(ParameterListSyntax node)
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

        public override Luau.VariableList VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var typeRef = CreateTypeRef(node.Type);
            var variables = node.Variables.Select(Visit).OfType<Luau.Variable>().ToList(); 
            return new Luau.VariableList(variables);
        }

        public override Luau.Variable VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var declaration = node.Parent as VariableDeclarationSyntax;
            var initializer = node.Initializer != null ? Visit<Luau.Expression>(node.Initializer) : null;
            return new Luau.Variable(CreateIdentifierName(node), true, initializer, CreateTypeRef(declaration?.Type));
        }

        public override Luau.Node? VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            return Visit(node.Value);
        }

        public override Luau.ExpressionStatement VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var expression = Visit<Luau.Expression>(node.Expression);
            return new Luau.ExpressionStatement(expression);
        }

        public override Luau.Literal VisitLiteralExpression(LiteralExpressionSyntax node)
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