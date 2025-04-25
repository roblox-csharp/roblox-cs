using Microsoft.CodeAnalysis;
using RobloxCS.Luau;

namespace RobloxCS;

public class OccupiedIdentifiersStack : Stack<List<IdentifierName>>
{
    public void Push()
    {
        List<IdentifierName> newList = [];
        Push(newList);
    }

    public string GetDuplicateText(string text)
    {
        text = text.Replace("@", "");

        var occurrences = CountOccurrences(text) - 1;
        var newText = occurrences > 0 ? "_" + occurrences : "";
        var halves = text.Split('<'); // generics, poopoo.
        var duplicateText = halves.First() + newText + (halves.Length > 1 ? "<" + halves.Last() : "");

        return duplicateText;
    }

    public int CountOccurrences(string text) => Peek().Count(identifier => identifier.Text == text);

    public IdentifierName AddIdentifier(SyntaxNode node, string text) => AddIdentifier(node.GetFirstToken(), text);
    public IdentifierName AddIdentifier(SyntaxToken token) => AddIdentifier(token, token.Text);

    public IdentifierName AddIdentifier(SyntaxToken token, string text)
    {
        var verbatim = text.Replace("@", "");

        if (AstUtility.CheckReservedName(token, verbatim)) return null!;

        return AddIdentifier(verbatim);
    }

    /// <summary><b>Note:</b> Skips reserved Luau name checks</summary>
    public IdentifierName AddIdentifier(string text)
    {
        AddIdentifier(new IdentifierName(text.Replace("@", "")));

        return new IdentifierName(GetDuplicateText(text));
    }

    private void AddIdentifier(IdentifierName identifierName) => Peek().Add(identifierName);
}