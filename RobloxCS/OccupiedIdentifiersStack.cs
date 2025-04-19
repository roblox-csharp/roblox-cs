using Microsoft.CodeAnalysis;
using RobloxCS.Luau;

namespace RobloxCS;

public class OccupiedIdentifiersStack : Stack<List<IdentifierName>>
{
    public List<IdentifierName> Push()
    {
        List<IdentifierName> newList = [];
        Push(newList);
        return newList;
    }
    
    public List<IdentifierName> Capture(Action callback)
    {
        Push();
        callback();
        return Pop();
    }

    public string GetDuplicateText(string text)
    {
        var occurences = HasIdentifier(text) ? "_" + CountOccurrences(text) : "";
        var halves = text.Split('<'); // generics, poopoo.
        var duplicateText = halves.First() + occurences + (halves.Length > 1 ? "<" + halves.Last() : "");
        return duplicateText.Replace('@', '_');
    }
    
    public IdentifierName AddIdentifier(string text) =>
        AddIdentifier(new IdentifierName(GetDuplicateText(text)));

    public IdentifierName AddIdentifier(SyntaxToken token) => AddIdentifier(token.Text);
    private IdentifierName AddIdentifier(IdentifierName identifierName)
    {
        Peek().Add(identifierName);
        return identifierName;
    }
    
    private bool HasIdentifier(string text) => CountOccurrences(text) > 0;
    private int CountOccurrences(string text) => Peek().Count(identifier => identifier.Text == text);
}