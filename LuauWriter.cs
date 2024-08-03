namespace RobloxCS
{
    public class LuauWriter : BaseWriter
    {
        public string Render(Luau.AST ast)
        {
            ast.Render(this);
            return ToString();
        }

        public void WriteRequire(string requirePath)
        {
            WriteLine($"require({requirePath})");
        }

        public void WriteIf(Luau.Expression condition, Luau.Statement body, Luau.Statement? elseBranch)
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

        public void WriteFunction(Luau.Name? name, bool isLocal, Luau.ParameterList parameterList, Luau.TypeRef? returnType = null, Luau.Block? body = null)
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

        public void WriteVariable(Luau.Name name, bool isLocal, Luau.Expression? initializer = null, Luau.TypeRef? type = null)
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

        public void WriteReturn(Luau.Expression? expression = null, Luau.TypeRef? type = null)
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

        public void WriteTypeAnnotation(Luau.TypeRef? type)
        {
            if (type != null)
            {
                Write(": ");
                type.Render(this);
            }
        }

        public void WriteTypeCast(Luau.Expression expression, Luau.TypeRef type)
        {
            expression.Render(this);
            Write(" :: ");
            type.Render(this);
        }

        public void WritePossibleBlock(Luau.Statement statement)
        {
            if (statement is not Luau.Block)
            {
                PushIndent();
            }
            statement.Render(this);
            if (statement is not Luau.Block)
            {
                PopIndent();
            }
        }
    }
}