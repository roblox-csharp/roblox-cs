namespace RobloxCS.Luau;

public class LuauWriter : BaseWriter
{
    public string Render(AST ast)
    {
        ast.Render(this);

        return ToString();
    }

    public void WriteNodesCommaSeparated<TNode>(IEnumerable<TNode> nodes)
        where TNode : Node
    {
        var nodesList = nodes.ToList();
        var index = 0;
        foreach (var node in nodesList)
        {
            node.Render(this);
            if (index++ != nodesList.Count - 1) Write(", ");
        }
    }

    public void WriteNodes<TNode>(List<TNode> nodes)
        where TNode : Node
    {
        foreach (var node in nodes) node.Render(this);
    }

    public void WriteRequire(string requirePath) => WriteLine($"require({requirePath})");

    public void WriteFunction(Name? name,
                              bool isLocal,
                              ParameterList parameterList,
                              TypeRef? returnType = null,
                              Block? body = null,
                              List<AttributeList>? attributeLists = null,
                              List<IdentifierName>? typeParameters = null,
                              bool inlineAttributes = false,
                              bool createNewline = true)
    {
        foreach (var attributeList in attributeLists ?? [])
        {
            attributeList.Inline = inlineAttributes;
            attributeList.Render(this);
        }

        if (isLocal) Write("local ");

        Write("function");
        if (name != null)
        {
            Write(' ');
            name.Render(this);
        }

        if (typeParameters != null)
        {
            Write('<');
            WriteNodes(typeParameters);
            Write('>');
        }

        parameterList.Render(this);
        WriteTypeAnnotation(returnType);
        WriteLine();
        PushIndent();

        body ??= new Block([]);
        foreach (var parameter in parameterList.Parameters)
            if (parameter.IsVararg)
            {
                var type = parameter.Type != null ? AstUtility.CreateTypeRef(parameter.Type.Path + "[]") : null;
                var value = new TableInitializer([AstUtility.Vararg]);
                body.Statements.Insert(0, new Variable(parameter.Name, true, value, type));
            }
            else if (parameter.Initializer != null)
            {
                body.Statements.Insert(0, AstUtility.DefaultValueInitializer(parameter.Name, parameter.Initializer));
            }

        body.Render(this);

        PopIndent();
        if (createNewline)
            WriteLine("end");
        else
            Write("end");
    }

    public void WriteAssignment(Expression name, Expression initializer)
    {
        name.Render(this);
        Write(" = ");
        initializer.Render(this);
        WriteLine();
    }

    public void WriteVariable(HashSet<IdentifierName> names, bool isLocal, List<Expression> initializers, TypeRef? type = null)
    {
        if (isLocal) Write("local ");

        WriteNodesCommaSeparated(names);
        WriteTypeAnnotation(type);
        if (initializers.Count > 0)
        {
            Write(" = ");
            WriteNodesCommaSeparated(initializers);
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
}