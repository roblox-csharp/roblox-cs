using System.Text;

namespace RobloxCS.CodeGeneration;

public class WritableTextBuffer
{
    private short _indentation;
    private int _indentationSize;
    private StringBuilder _stringBuilder;

    public WritableTextBuffer()
    {
        _indentationSize = 4;
        _indentation = 0;
        _stringBuilder = new StringBuilder();
    }

    private bool MatchLastCharacter(char character)
    {
        if (_stringBuilder.Length == 0) return false;
        return _stringBuilder[^1] == character;
    }

    public void IncreaseIndent()
    {
        _indentation++;
    }

    public void DecreaseIndentation()
    {
        _indentation--;
    }

    private void WriteTab()
    {
        _stringBuilder.Append(MatchLastCharacter('\n') ? string.Concat(Enumerable.Repeat(' ', _indentationSize * _indentation)) : "");
    }

    public void WriteLine(char? text, bool applyIndentation = true)
    {
        if (text == null)
            WriteLine((string?)null);

        WriteLine(text.ToString(), applyIndentation);
    }

    public void WriteLine(string? text, bool applyIndentation = true)
    {
        if (text == null)
        {
            _stringBuilder.AppendLine();
            return;
        }

        if (applyIndentation)
            WriteTab();
        _stringBuilder.AppendLine(text);
    }

    public void Write(char text, bool applyIndentation = true)
        => Write(text.ToString(), applyIndentation);


    public void Write(string text, bool applyIndentation = true)
    {
        if (applyIndentation)
            WriteTab();
        _stringBuilder.Append(text);
    }
}