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

        public void WriteFunction(Name? name, bool isLocal, ParameterList parameterList, TypeRef? returnType = null, Block? body = null)
        {
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

            body ??= new Luau.Block([]);
            foreach (var parameter in parameterList.Parameters)
            {
                if (parameter.Initializer == null) continue;
                body.Statements.Insert(0, LuauUtility.Initializer(parameter.Name, parameter.Initializer));
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
            Write("return");
            if (expression != null)
            {
                Write(" ");
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