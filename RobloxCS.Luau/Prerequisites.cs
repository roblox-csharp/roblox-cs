using RobloxCS.Shared;

namespace RobloxCS.Luau;

public class Prerequisites
{
    private readonly Stack<List<Statement>> _statementsStack = [];

    public void Add(Statement statement) => _statementsStack.Peek().Add(statement);
    public void AddList(List<Statement> statements) => _statementsStack.Peek().AddRange(statements);

    public List<Statement> CaptureOnlyPrereqs(Action callback)
    {
        PushStatementsStack();
        callback();

        return PopStatementsStack();
    }

    public (T, List<Statement>) Capture<T>(Func<T> callback)
    {
        T? value = default;
        var prereqs = CaptureOnlyPrereqs(() => value = callback());

        return (value!, prereqs);
    }

    public Expression NoPrereqs(Func<Expression> callback)
    {
        Expression? expression = null;
        var statements = CaptureOnlyPrereqs(() => expression = callback());
        if (statements.Count > 0)
            Logger.CompilerError("Assertion of no prereqs failed for " + (expression?.ToString() ?? "expression"));

        return expression!;
    }

    private void PushStatementsStack()
    {
        List<Statement> statements = [];
        _statementsStack.Push(statements);
    }

    private List<Statement> PopStatementsStack()
    {
        if (!_statementsStack.TryPop(out var popped))
            Logger.CompilerError("Failed to pop prereq statements stack");

        return popped!;
    }
}