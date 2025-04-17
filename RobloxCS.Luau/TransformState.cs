namespace RobloxCS.Luau;

public class TransformState {
    public List<Statement> PreReqStatementStack { get; } = [];
        
    public void Prereq(Statement statement) => PreReqStatementStack.Add(statement);
    public void PrereqList(List<Statement> statements) => PreReqStatementStack.AddRange(statements);
}