using System.Text;

namespace TypeGenerator
{
    internal abstract class Generator
    {
        private MemoryStream _stream = new MemoryStream();
        private string _indent = "";

        private string _filePath;
        private ReflectionMetadataReader? _metadata;

        public Generator(string filePath, ReflectionMetadataReader? metadata = null)
        {
            _filePath = filePath;
            _metadata = metadata;
        }

        protected void WriteFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, GetStringFromMemoryStream());
        }

        protected void Write(string line = "")
        {
            _stream.Write(Encoding.UTF8.GetBytes($"{_indent}{line}\n"));
        }

        protected void PushIndent()
        {
            _indent += "\t";
        }

        protected void PopIndent()
        {
            _indent = _indent.Substring(1);
        }

        private string GetStringFromMemoryStream()
        {
            return Encoding.UTF8.GetString(_stream.ToArray());
        }
    }
}