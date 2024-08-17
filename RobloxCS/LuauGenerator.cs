using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace RobloxCS
{
    public sealed class LuauGenerator(SyntaxTree tree, CSharpCompilation compiler) : Luau.BaseGenerator(tree, compiler)
    {
        public Luau.AST GetLuauAST()
        {
            return Visit<Luau.AST>(_tree.GetRoot());
        }

        public override Luau.AST VisitCompilationUnit(CompilationUnitSyntax node)
        {
            List<Luau.Statement> statements = [];
            foreach (var member in node.Members)
            {
                var statement = Visit<Luau.Statement?>(member);
                if (statement == null)
                {
                    throw Logger.CompilerError($"Unhandled syntax node within {member.Kind()}:\n{member}");
                }
                statements.Add(statement);
            }
            return new Luau.AST(statements);
        }

        public override Luau.Name VisitPredefinedType(PredefinedTypeSyntax node)
        {
            return new Luau.IdentifierName(node.Keyword.Text);
        }

        public override Luau.Statement VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (!IsStatic(node) || node.Parent is not ClassDeclarationSyntax || node.Initializer == null)
            {
                return new Luau.NoOp();
            }

            var classDeclaration = (ClassDeclarationSyntax)node.Parent!;
            var initializer = Visit<Luau.Expression>(node.Initializer);
            return new Luau.ExpressionStatement(
                new Luau.Assignment(
                    new Luau.MemberAccess(
                        Luau.AstUtility.CreateIdentifierName(classDeclaration),
                        Luau.AstUtility.CreateIdentifierName(node)
                    ),
                    initializer
                )
            );
        }

        public override Luau.Statement VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (!IsStatic(node) || node.Parent is not ClassDeclarationSyntax)
            {
                return new Luau.NoOp();
            }

            var classDeclaration = (ClassDeclarationSyntax)node.Parent!;
            var staticFields = classDeclaration.Members
                .OfType<FieldDeclarationSyntax>()
                .Where(field => HasSyntax(field.Modifiers, SyntaxKind.StaticKeyword));

            List<Luau.Statement> statements = [];
            foreach (var declarator in node.Declaration.Variables)
            {
                if (declarator.Initializer == null) continue;

                var initializer = Visit<Luau.Expression>(declarator.Initializer);
                statements.Add(new Luau.ExpressionStatement(
                    new Luau.Assignment(
                        new Luau.MemberAccess(
                            Luau.AstUtility.CreateIdentifierName(classDeclaration),
                            Luau.AstUtility.CreateIdentifierName(declarator)
                        ),
                        initializer
                    )
                ));
            }
            return new Luau.Block(statements);
        }

        public override Luau.Function VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var classDeclaration = (ClassDeclarationSyntax)node.Parent!;
            var className = Luau.AstUtility.CreateIdentifierName(node.Parent!);
            var parameterList = Visit<Luau.ParameterList>(node.ParameterList);
            var body = Visit<Luau.Block?>(node.Body);
            var attributeLists = node.AttributeLists.Select(Visit<Luau.AttributeList>).ToList();
            return GenerateConstructor(classDeclaration, parameterList, body, attributeLists);
        }

        public override Luau.Function VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var name = Luau.AstUtility.CreateIdentifierName(node);
            var className = Luau.AstUtility.CreateIdentifierName(node.Parent!);
            var fullName = new Luau.QualifiedName(className, name, IsStatic(node) ? '.' : ':');
            var parameterList = Visit<Luau.ParameterList>(node.ParameterList);
            var returnType = new Luau.TypeRef(Visit<Luau.Name>(node.ReturnType).ToString());
            var body = Visit<Luau.Block?>(node.Body);
            var attributeLists = node.AttributeLists.Select(Visit<Luau.AttributeList>).ToList();
            return new Luau.Function(fullName, false, parameterList, returnType, body, attributeLists);
        }

        public override Luau.IdentifierName VisitThisExpression(ThisExpressionSyntax node)
        {
            return new Luau.IdentifierName("self");
        }

        // long as hell lol
        public override Luau.Block VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var name = Luau.AstUtility.CreateIdentifierName(node);
            var members = node.Members.Select(Visit<Luau.Statement>).ToList();
            var explicitConstructor = node.Members.FirstOrDefault(member => member.IsKind(SyntaxKind.ConstructorDeclaration)) as ConstructorDeclarationSyntax;

            List<Luau.Statement> classMemberStatements = [
                new Luau.ExpressionStatement(
                    new Luau.Assignment(
                        name,
                        new Luau.Call(
                            new Luau.IdentifierName("setmetatable"),
                            Luau.AstUtility.CreateArgumentList([
                                new Luau.TableInitializer(),
                                new Luau.TableInitializer(
                                    [new Luau.AnonymousFunction(
                                        new Luau.ParameterList([]),
                                        new Luau.Block([
                                            new Luau.Return(new Luau.Literal($"\"{name.Text}\""))
                                        ])
                                    )],
                                    [new Luau.IdentifierName("__tostring")]
                                )
                            ])
                        )
                    )
                ),
                new Luau.ExpressionStatement(
                    new Luau.Assignment(
                        new Luau.MemberAccess(
                            name,
                            new Luau.IdentifierName("__index")
                        ),
                        name
                    )
                ),
                new Luau.Function(
                    new Luau.AssignmentFunctionName(name, new Luau.IdentifierName("new")),
                    false,
                    new Luau.ParameterList([new Luau.Parameter(new Luau.IdentifierName("..."))]),
                    new Luau.TypeRef(name.Text),
                    new Luau.Block([
                        new Luau.Variable(
                            new Luau.IdentifierName("self"),
                            true,
                            new Luau.Call(
                                new Luau.IdentifierName("setmetatable"),
                                Luau.AstUtility.CreateArgumentList([
                                    new Luau.TableInitializer(),
                                    name
                                ])
                            )
                        ),
                        new Luau.Return(
                            new Luau.BinaryOperator(
                                new Luau.Call(
                                    new Luau.MemberAccess(new Luau.IdentifierName("self"), name, ':'),
                                    Luau.AstUtility.CreateArgumentList([new Luau.IdentifierName("...")])
                                ),
                                "or",
                                new Luau.IdentifierName("self")
                            )
                        )
                    ])
                )
            ];

            if (IsGlobal(node))
            {
                classMemberStatements.Insert(2, new Luau.ExpressionStatement(Luau.AstUtility.DefineGlobal(name, name)));
            }

            if (explicitConstructor == null)
            {
                classMemberStatements.Add(GenerateConstructor(node, new Luau.ParameterList([])));
            }
            classMemberStatements.AddRange(members);

            List<Luau.Statement> statements = [
                new Luau.Variable(Luau.AstUtility.CreateIdentifierName(node), true),
                new Luau.ScopedBlock(classMemberStatements)
            ];

            if (!IsGlobal(node))
            {
                var fullParentName = Luau.AstUtility.GetFullParentName(node);
                if (fullParentName != null)
                {
                    statements.Add(new Luau.ExpressionStatement(
                        new Luau.Assignment(
                            new Luau.MemberAccess(
                                fullParentName,
                                name
                            ),
                            name
                        )
                    ));
                }
            }

            return new Luau.Block(statements);
        }

        public override Luau.Block VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var name = Luau.AstUtility.CreateIdentifierName(node);
            var members = new Luau.Block(node.Members.Select(Visit<Luau.Statement>).ToList());
            List<Luau.Statement> statements = [
                new Luau.Variable(Luau.AstUtility.CreateIdentifierName(node), true, new Luau.TableInitializer())
            ];

            if (IsGlobal(node))
            {
                statements.Add(new Luau.ExpressionStatement(Luau.AstUtility.DefineGlobal(name, name)));
            }

            statements.Add(new Luau.ScopedBlock([
                members
            ]));
            return new Luau.Block(statements);
        }

        public override Luau.Repeat VisitDoStatement(DoStatementSyntax node)
        {
            var condition = Visit<Luau.Expression>(node.Condition);
            var body = Visit<Luau.Statement>(node.Statement);
            return new Luau.Repeat(new Luau.UnaryOperator("not ", condition), body);
        }

        public override Luau.While VisitWhileStatement(WhileStatementSyntax node)
        {
            var condition = Visit<Luau.Expression>(node.Condition);
            var body = Visit<Luau.Statement>(node.Statement);
            return new Luau.While(condition, body);
        }

        public override Luau.ExpressionalIf VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            var condition = Visit<Luau.Expression>(node.Condition);
            var body = Visit<Luau.Expression>(node.WhenTrue);
            var elseBranch = Visit<Luau.Expression>(node.WhenFalse);
            return new Luau.ExpressionalIf(condition, body, elseBranch);
        }

        public override Luau.If VisitIfStatement(IfStatementSyntax node)
        {
            var condition = Visit<Luau.Expression>(node.Condition);
            var body = Visit<Luau.Statement>(node.Statement);
            var elseBranch = Visit<Luau.Statement?>(node.Else?.Statement);
            return new Luau.If(condition, body, elseBranch);
        }

        public override Luau.NumericalFor VisitForStatement(ForStatementSyntax node)
        {
            var initializer = Visit<Luau.VariableList?>(node.Declaration)?.Variables.FirstOrDefault();
            var incrementBy = Visit<Luau.Expression?>(node.Incrementors.FirstOrDefault());
            var condition = Visit<Luau.Expression?>(node.Condition);
            var body = Visit<Luau.Statement>(node.Statement);
            return new Luau.NumericalFor(initializer, incrementBy, condition, body);
        }

        public override Luau.For VisitForEachStatement(ForEachStatementSyntax node)
        {
            List<Luau.IdentifierName> names = [Luau.AstUtility.CreateIdentifierName(node)];
            var iterator = Visit<Luau.Expression>(node.Expression);
            var body = Visit<Luau.Statement>(node.Statement);
            return new Luau.For(names, iterator, body);
        }

        public override Luau.Node? VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            var variableList = Visit<Luau.Statement>(node.Variable);
            if (variableList is Luau.Variable variable)
            {
                variableList = new Luau.VariableList([variable]);
            }

            List<Luau.IdentifierName> names = ((Luau.VariableList)variableList).Variables.Select(variable => variable.Name).ToList();
            var iterator = Visit<Luau.Expression>(node.Expression);
            var body = Visit<Luau.Statement>(node.Statement);
            return new Luau.For(names, iterator, body);
        }

        public override Luau.Node? VisitDeclarationExpression(DeclarationExpressionSyntax node)
        {
            return Visit(node.Designation);
        }

        public override Luau.Variable VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
        {
            return new Luau.Variable(Luau.AstUtility.CreateIdentifierName(node), true);
        }

        public override Luau.VariableList VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntax node)
        {
            var variableNodes = node.Variables.Select(Visit)
                .Where(variableNode => variableNode != null)
                .Select(variableNode => variableNode!)
                .SelectMany(variableNode =>
                {
                    if (variableNode is Luau.VariableList variableList)
                    {
                        return variableList.Variables;
                    }
                    return [(Luau.Variable)variableNode];
                })
                .ToList();

            return new Luau.VariableList(variableNodes);
        }

        public override Luau.TableInitializer VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type;
            if (typeSymbol == null)
            {
                Logger.CodegenError(node, "Unable to resolve type symbol of the type provided to typeof()");
                return null!;
            }

            var fullyQualifiedName = GetFullSymbolName(typeSymbol);
            var type = GetRuntimeType(node, fullyQualifiedName);
            return Luau.AstUtility.CreateTypeInfo(type);
        }

        public override Luau.Call VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            // TODO: handle null initializer
            Console.WriteLine(node.Type.Kind());
            var expression = Visit<Luau.Name>(node.Type);
            var argumentList = Visit<Luau.ArgumentList>(node.ArgumentList);
            var callee = new Luau.QualifiedName(expression, new Luau.IdentifierName("new"));
            return new Luau.Call(callee, argumentList);
        }

        public override Luau.Node VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var methodSymbolInfo = _semanticModel.GetSymbolInfo(node.Expression);
            if (methodSymbolInfo.Symbol == null &&
                methodSymbolInfo.CandidateSymbols.IsEmpty &&
                methodSymbolInfo.CandidateReason == CandidateReason.None)
            {
                var identifier = node.Expression as IdentifierNameSyntax;
                if (identifier != null && identifier.Identifier.IsKind(SyntaxKind.IdentifierToken))
                {
                    switch (identifier.Identifier.Text)
                    {
                        case "nameof":
                            return new Luau.Literal('"' + node.ArgumentList.Arguments.First().Expression.ToString() + '"');
                    }
                }
            }

            var callee = Visit<Luau.Expression>(node.Expression);
            if (callee is Luau.MemberAccess memberAccess)
            {
                memberAccess.Operator = methodSymbolInfo.Symbol!.IsStatic ? '.' : ':';
            }

            var argumentList = Visit<Luau.ArgumentList>(node.ArgumentList);
            return new Luau.Call(callee, argumentList);
        }

        public override Luau.ArgumentList VisitArgumentList(ArgumentListSyntax node)
        {
            var arguments = node.Arguments.Select(Visit<Luau.Argument>).ToList();
            return new Luau.ArgumentList(arguments);
        }

        public override Luau.Argument VisitArgument(ArgumentSyntax node)
        {
            var expression = Visit<Luau.Expression>(node.Expression);
            return new Luau.Argument(expression);
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
                name = Luau.AstUtility.CreateIdentifierName(node, expressionName);
            }

            var value = Visit<Luau.Expression?>(node.Expression);
            return new Luau.Variable(name, true, value);
        }

        public override Luau.Node VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var name = Visit<Luau.Expression>(node.Left);
            var value = Visit<Luau.Expression>(node.Right);
            if (!node.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                var mappedOperator = Luau.Utility.GetMappedOperator(node.OperatorToken.Text);
                var bit32MethodName = Luau.Utility.GetBit32MethodName(mappedOperator);
                if (bit32MethodName != null)
                {
                    return new Luau.Assignment(name, Luau.AstUtility.Bit32Call(bit32MethodName, name, value));
                }
                return new Luau.BinaryOperator(name, mappedOperator, value);
            }

            return new Luau.Assignment(name, value);
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

        public override Luau.Node VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var expression = Visit<Luau.Expression>(node.Expression);
            var name = Visit<Luau.IdentifierName>(node.Name);
            var memberAccess = new Luau.MemberAccess(expression, name);
            if (node.Parent is AssignmentExpressionSyntax assignment && assignment.Left == node)
            {
                return Luau.AstUtility.QualifiedNameFromMemberAccess(memberAccess);
            }

            return Luau.AstUtility.DiscardVariableIfExpressionStatement(node, memberAccess, node.Parent);
        }

        public override Luau.Node VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var expression = Visit<Luau.Expression>(node.Expression);
            var index = Visit<Luau.Expression>(node.ArgumentList.Arguments.First().Expression);
            var elementAccess = new Luau.ElementAccess(expression, index);
            return Luau.AstUtility.DiscardVariableIfExpressionStatement(node, elementAccess, node.Parent);
        }

        public override Luau.QualifiedName VisitQualifiedName(QualifiedNameSyntax node)
        {
            var left = Visit<Luau.Name>(node.Left);
            var right = Visit<Luau.IdentifierName>(node.Right);
            return new Luau.QualifiedName(left, right);
        }

        public override Luau.Name VisitIdentifierName(IdentifierNameSyntax node)
        {
            var classDeclaration = FindFirstAncestor<ClassDeclarationSyntax>(node);
            var isClassMember = classDeclaration != null
                && classDeclaration.Members.Any(member => member is not ConstructorDeclarationSyntax &&  TryGetName(member) == GetName(node))
                && (node.Parent is not MemberAccessExpressionSyntax memberAccess || memberAccess.Expression is not ThisExpressionSyntax);

            var name = Luau.AstUtility.CreateIdentifierName(node);
            return isClassMember ? new Luau.QualifiedName(new Luau.IdentifierName("self"), name) : name;
        }

        public override Luau.Break VisitBreakStatement(BreakStatementSyntax node)
        {
            return new Luau.Break();
        }

        public override Luau.Continue VisitContinueStatement(ContinueStatementSyntax node)
        {
            return new Luau.Continue();
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
                return Luau.AstUtility.Bit32Call(bit32MethodName, left, right);
            }

            return new Luau.BinaryOperator(left, mappedOperator, right);
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
            return new Luau.BinaryOperator(operand, mappedOperator, new Luau.Literal("1"));
        }

        public override Luau.Node? VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var operatorText = node.OperatorToken.Text;
            if (operatorText == "^")
            {
                Logger.UnsupportedError(node, "'^' unary operator", useIs: true);
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
                return Luau.AstUtility.Bit32Call(bit32MethodName, operand);
            }

            return new Luau.UnaryOperator(mappedOperator, operand);
        }

        public override Luau.Parenthesized VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            var expression = Visit<Luau.Expression>(node.Expression);
            return new Luau.Parenthesized(expression);
        }

        public override Luau.AnonymousFunction VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
            var body = node.ExpressionBody != null ?
                new Luau.Block([new Luau.ExpressionStatement(Visit<Luau.Expression>(node.ExpressionBody))])
                : Visit<Luau.Block?>(node.Block);

            return new Luau.AnonymousFunction(parameterList, body);
        }

        public override Luau.AnonymousFunction VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            var parameterList = new Luau.ParameterList([Visit<Luau.Parameter>(node.Parameter)]);
            var body = node.ExpressionBody != null ?
                new Luau.Block([new Luau.ExpressionStatement(Visit<Luau.Expression>(node.ExpressionBody))])
                : Visit<Luau.Block?>(node.Block);

            return new Luau.AnonymousFunction(parameterList, body);
        }

        public override Luau.AnonymousFunction VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
            var body = node.ExpressionBody != null ?
                new Luau.Block([new Luau.ExpressionStatement(Visit<Luau.Expression>(node.ExpressionBody))])
                : Visit<Luau.Block?>(node.Block);

            return new Luau.AnonymousFunction(parameterList, body);
        }

        public override Luau.Function VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var name = Luau.AstUtility.CreateIdentifierName(node);
            var parameterList = Visit<Luau.ParameterList?>(node.ParameterList) ?? new Luau.ParameterList([]);
            var returnType = Luau.AstUtility.CreateTypeRef(node.ReturnType);
            var body = node.ExpressionBody != null ?
                new Luau.Block([new Luau.ExpressionStatement(Visit<Luau.Expression>(node.ExpressionBody.Expression))])
                : Visit<Luau.Block?>(node.Body);

            var attributeLists = node.AttributeLists.Select(Visit<Luau.AttributeList>).ToList();
            return new Luau.Function(name, true, parameterList, returnType, body, attributeLists);
        }

        public override Luau.Parameter VisitParameter(ParameterSyntax node)
        {
            var name = Luau.AstUtility.CreateIdentifierName(node);
            var returnType = Luau.AstUtility.CreateTypeRef(node.Type);
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

            Logger.UnsupportedError(node, "Non-builtin attributes");
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

        public override Luau.Statement VisitGlobalStatement(GlobalStatementSyntax node)
        {
            var luauNode = Visit<Luau.Statement>(node.Statement);
            if (HasSyntax(node.Modifiers, SyntaxKind.PublicKeyword))
            {

            }
            return luauNode;
        }

        public override Luau.ParameterList VisitParameterList(ParameterListSyntax node)
        {
            return new Luau.ParameterList(node.Parameters.Select(Visit).OfType<Luau.Parameter>().ToList());
        }

        public override Luau.Statement VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            return Visit<Luau.Statement>(node.Declaration);
        }

        public override Luau.VariableList VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var typeRef = Luau.AstUtility.CreateTypeRef(node.Type);
            var variables = node.Variables.Select(Visit).OfType<Luau.Variable>().ToList();
            return new Luau.VariableList(variables);
        }

        public override Luau.Variable VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var declaration = node.Parent as VariableDeclarationSyntax;
            var initializer = node.Initializer != null ? Visit<Luau.Expression>(node.Initializer) : null;
            return new Luau.Variable(Luau.AstUtility.CreateIdentifierName(node), true, initializer, Luau.AstUtility.CreateTypeRef(declaration?.Type));
        }

        public override Luau.Node? VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            return Visit(node.Value);
        }

        public override Luau.Node VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var expressionNode = Visit<Luau.Node>(node.Expression);
            if (expressionNode is Luau.Expression expression)
            {
                return new Luau.ExpressionStatement(expression);
            }
            else
            {
                return expressionNode;
            }
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