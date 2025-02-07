namespace RobloxCS.Luau;

    public Block(List<Statement> statements)
    {
        Statements = statements;
        AddChildren(Statements);
    }

    public override void Render(LuauWriter luau) => luau.WriteNodes(Statements);
}