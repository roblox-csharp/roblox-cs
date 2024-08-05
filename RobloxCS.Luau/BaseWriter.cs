using System.Text;

namespace RobloxCS.Luau
{
    public class BaseWriter
    {
        private const int _indentSize = 2;
        private readonly StringBuilder _output = new();
        private int _indent = 0;

        public override string ToString()
        {
            return _output.ToString();
        }

        public void PushIndent()
        {
            _indent++;
        }

        public void PopIndent()
        {
            _indent--;
        }

        public void WriteLine()
        {
            Write('\n');
        }

        public void WriteLine(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _output.AppendLine();
                return;
            }

            WriteIndent();
            _output.AppendLine(text);
        }

        public void WriteLine(char text)
        {
            WriteLine(text.ToString());
        }

        public void Write(string text)
        {
            WriteIndent();
            _output.Append(text);
        }

        public void Write(char text)
        {
            Write(text.ToString());
        }

        private void WriteIndent()
        {
            _output.Append(MatchLastCharacter('\n') ? string.Concat(Enumerable.Repeat(" ", _indentSize * _indent)) : "");
        }

        private bool MatchLastCharacter(char character)
        {
            if (_output.Length == 0) return false;
            return _output[^1] == character;
        }
    }
}
