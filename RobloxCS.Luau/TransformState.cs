using RobloxCS.Shared;

namespace RobloxCS.Luau;

public class TransformState
{
    private readonly Stack<List<Statement>> _prereqStatementsStack = [];

    public void Prereq(Statement statement) => _prereqStatementsStack.Peek().Add(statement);
    public void PrereqList(List<Statement> statements) => _prereqStatementsStack.Peek().AddRange(statements);

    public List<Statement> PushPrereqStatementsStack()
    {
        List<Statement> statements = [];
        _prereqStatementsStack.Push(statements);

        return statements;
    }

    public List<Statement> PopPrereqStatementsStack()
    {
        if (!_prereqStatementsStack.TryPop(out var popped)) Logger.CompilerError("Failed to pop prereq statements stack");

        return popped!;
    }

    public List<Statement> CapturePrereqs(Action callback)
    {
        PushPrereqStatementsStack();
        callback();

        return PopPrereqStatementsStack();
    }

    public (T, List<Statement>) Capture<T>(Func<T> callback)
    {
        T? value = default;
        var prereqs = CapturePrereqs(() => value = callback());

        return (value!, prereqs);
    }

    public Expression NoPrereqs(Func<Expression> callback)
    {
        Expression? expression = null;
        var statements = CapturePrereqs(() => expression = callback());
        if (statements.Count > 0) Logger.CompilerError("Assertion of no prereqs failed for " + (expression?.ToString() ?? "expression"));

        return expression!;
    }
}