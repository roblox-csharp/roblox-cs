namespace RobloxCS.Luau
{
    public class LuauWriter : BaseWriter
    {
        public string Render(AST ast)
        {
            ast.Render(this);
            return ToString();
        }

        public void WriteNodesCommaSeparated<TNode>(List<TNode> nodes) where TNode : Node
        {
            foreach (var node in nodes)
            {
                node.Render(this);
                if (node != nodes.Last())
                {
                    Write(", ");
                }
            }
        }
        public void WriteNodes<TNode>(List<TNode> nodes) where TNode : Node
        {
            foreach (var node in nodes)
            {
                node.Render(this);
            }
        }


        public void WriteRequire(string requirePath)
        {
            WriteLine($"require({requirePath})");
        }

        public void WriteFunction(Name? name, bool isLocal, ParameterList parameterList, TypeRef? returnType = null, Block? body = null, List<AttributeList>? attributeLists = null, bool inlineAttributes = false, bool createNewline = true)
        {
            foreach (var attributeList in attributeLists ?? [])
            {
                attributeList.Inline = inlineAttributes;
                attributeList.Render(this);
            }
            if (isLocal)
            {
                Write("local ");
            }
            Write("function");
            if (name != null)
            {
                Write(' ');
                name.Render(this);
            }
            parameterList.Render(this);
            WriteTypeAnnotation(returnType);
            WriteLine();
            PushIndent();

            body ??= new Block([new Luau.Return(Luau.AstUtility.Nil())]);
            foreach (var parameter in parameterList.Parameters)
            {
                if (parameter.IsVararg)
                {
                    var type = parameter.Type != null ? AstUtility.CreateTypeRef(parameter.Type.Path + "[]") : null;
                    var value = new TableInitializer([AstUtility.Vararg()]);
                    body.Statements.Insert(0, new Variable(parameter.Name, true, value, type));
                }
                else if (parameter.Initializer != null)
                {
                    body.Statements.Insert(0, AstUtility.Initializer(parameter.Name, parameter.Initializer));
                }
            }
            body.Render(this);

            PopIndent();
            if (createNewline)
            {
                WriteLine("end");
            }
            else
            {
                Write("end");
            }
        }

        public void WriteAssignment(Expression name, Expression initializer)
        {
            name.Render(this);
            Write(" = ");
            initializer.Render(this);
        }

        public void WriteVariable(Name name, bool isLocal, Expression? initializer = null, TypeRef? type = null)
        {
            if (initializer != null)
            {
                Node initializerNode = initializer;
                WriteDescendantStatements(ref initializerNode);
                initializer = (Expression)initializerNode;
            }
            if (isLocal)
            {
                Write("local ");
            }
            name.Render(this);
            WriteTypeAnnotation(type);
            if (initializer != null)
            {
                Write(" = ");
                initializer.Render(this);
            }
            WriteLine();
        }

        public void WriteReturn(Expression? expression = null)
        {
            Write("return ");
            (expression ?? new Literal("nil")).Render(this);
            WriteLine();
        }

        public void WriteTypeAnnotation(TypeRef? type)
        {
            if (type == null) return;
            Write(": ");
            type.Render(this);
        }

        public void WriteTypeCast(Expression expression, TypeRef type)
        {
            expression.Render(this);
            Write(" :: ");
            type.Render(this);
        }

        // wtf is this for??? i literally forgot
        public void WriteDescendantStatements(ref Node node)
        {
            if (node.Parent is not Variable && node is not Argument)
            {
                node = node switch
                {
                    Assignment assignment => assignment.Target,
                    BinaryOperator binaryOperator when binaryOperator.Operator.Contains('=') => binaryOperator.Left,
                    _ => node
                };
                
                FixNode(node);
            }

            var original = new IdentifierName("_original");
            var isCall = node is Call;
            var isElementAccess = node is ElementAccess;
            if (node.Parent is Variable)
            {
                switch (node)
                {
                    case Assignment assignment:
                        new Variable(original, true, assignment.Target).Render(this);
                        new ExpressionStatement(assignment).Render(this);
                        node = original;
                        break;
                    case BinaryOperator binaryOperator when binaryOperator.Operator.Contains('='):
                        new Variable(original, true, binaryOperator.Left).Render(this);
                        new ExpressionStatement(binaryOperator).Render(this);
                        node = original;
                        break;
                }
            }

            node.Descendants = node.Descendants.Select(descendant =>
            {
                if (isCall || isElementAccess)
                {
                    switch (descendant)
                    {
                        case Assignment assignment:
                            new Variable(original, true, assignment.Target).Render(this);
                            new ExpressionStatement(assignment).Render(this);
                            break;
                        case BinaryOperator binaryOperator when binaryOperator.Operator.Contains('='):
                            new Variable(original, true, binaryOperator.Left).Render(this);
                            new ExpressionStatement(binaryOperator).Render(this);
                            break;
                    }
                }

                WriteDescendantStatements(ref descendant);
                return descendant;
            }).ToList();
        }

        private Node FixNode(Node node)
        {
            var original = new IdentifierName("_original");
            switch (node)
            {
                case ElementAccess elementAccess:
                    elementAccess.Index = original;
                    break;
                case Variable variable:
                    variable.Initializer = original;
                    break;
                case Argument argument:
                    argument.Expression = original;
                    break;
            }

            return node;
        }
    }
}