namespace RobloxCS.Luau;

public class TransformState {
    public List<Statement> PrereqStatements { get; } = [];
        
    public void Prereq(Statement statement) => PrereqStatements.Add(statement);
    public void PrereqList(List<Statement> statements) => PrereqStatements.AddRange(statements);
}