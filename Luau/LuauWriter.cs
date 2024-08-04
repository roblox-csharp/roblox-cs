namespace RobloxCS.Luau
{
    public class LuauWriter : BaseWriter
    {
        public string Render(AST ast)
        {
            ast.Render(this);
            return ToString();
        }

        public void WriteRequire(string requirePath)
        {
            WriteLine($"require({requirePath})");
        }

        public void WriteIf(Expression condition, Statement body, Statement? elseBranch)
        {
            Write("if ");
            condition.Render(this);
            WriteLine(" then");
            WritePossibleBlock(body);
            WriteLine();
            if (elseBranch != null)
            {
                WriteLine("else");
                WritePossibleBlock(elseBranch);
            }
            WriteLine("end");
        }

        public void WriteFunction(Name? name, bool isLocal, ParameterList parameterList, TypeRef? returnType = null, Block? body = null, List<AttributeList>? attributeLists = null, bool inlineAttributes = false)
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

            body ??= new Block([]);
            foreach (var parameter in parameterList.Parameters)
            {
                if (parameter.IsVararg)
                {
                    var type = parameter.Type != null ? new TypeRef(parameter.Type.Path + "[]") : null;
                    var value = new TableInitializer([AstUtility.Vararg()]);
                    body.Statements.Insert(0, new Variable(parameter.Name, true, value, type));
                }
                else if (parameter.Initializer != null)
                {
                    body.Statements.Insert(0, AstUtility.Initializer(parameter.Name, parameter.Initializer));
                }
            }
            body.Render(this);

            WriteLine("end");
        }

        public void WriteVariable(Name name, bool isLocal, Expression? initializer = null, TypeRef? type = null)
        {
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

        public void WriteReturn(Expression? expression = null, TypeRef? type = null)
        {
            Write("return ");
            if (expression != null)
            {
                if (type != null)
                {
                    WriteTypeCast(expression, type);
                }
                else
                {
                    expression.Render(this);
                }
            }
            else
            {
                WriteTypeCast(new Luau.Literal("nil"), type ?? new Luau.TypeRef("any"));
            }
            WriteLine();
        }

        public void WriteTypeAnnotation(TypeRef? type)
        {
            if (type != null)
            {
                Write(": ");
                type.Render(this);
            }
        }

        public void WriteTypeCast(Expression expression, TypeRef type)
        {
            expression.Render(this);
            Write(" :: ");
            type.Render(this);
        }

        public void WritePossibleBlock(Statement statement)
        {
            if (statement is not Block)
            {
                PushIndent();
            }
            statement.Render(this);
            if (statement is not Block)
            {
                PopIndent();
            }
        }
    }
}