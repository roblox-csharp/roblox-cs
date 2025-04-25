using System.Text;

namespace RobloxCS.Luau;

public class BaseWriter
{
    public const int IndentSize = 2;
    private readonly StringBuilder _output = new();
    private int _indent;

    public override string ToString() => _output.ToString();
    public void PushIndent() => _indent++;
    public void PopIndent() => _indent--;
    public void WriteLine() => Write('\n');
    public void WriteLine(char text) => WriteLine(text.ToString());

    public void WriteLine(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            _output.AppendLine();

            return;
        }

        WriteIndent();
        _output.Append(text);
        _output.Append('\n');
    }

    public void Write(char text) => Write(text.ToString());

    public void Write(string text)
    {
        WriteIndent();
        _output.Append(text);
    }

    public void Remove(int amount) => _output.Remove(_output.Length - 1, amount);

    private void WriteIndent() => _output.Append(WasLastCharacter('\n') ? string.Concat(Enumerable.Repeat(" ", IndentSize * _indent)) : "");

    private bool WasLastCharacter(char character)
    {
        if (_output.Length == 0) return false;

        return _output[^1] == character;
    }
}