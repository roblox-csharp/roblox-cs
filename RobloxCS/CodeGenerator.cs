using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Xml.Linq;

namespace RobloxCS
{
    internal class CodeGenerator : CSharpSyntaxWalker
    {
        private readonly StringBuilder _output = new StringBuilder();
        private readonly string _source;
        private readonly int _indentSize;
        private int _indent = 0;

        public CodeGenerator(string source, int indentSize = 2)
        {
            _source = source;
            _indentSize = indentSize;
        }

        public string GenerateLua()
        {
            var root = CSharpSyntaxTree.ParseText(_source).GetRoot();
            Visit(root);
            return _output.ToString();
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var names = GetNames(node);
            string currentNamePath = "";

            Write("local ");
            foreach (var name in names)
            {
                currentNamePath += (currentNamePath == "" ? "" : ".") + name;
                Write(currentNamePath);

                const string text = " = {}";
                if (name == names.Last())
                {
                    Write(text);
                }
                else
                {
                    WriteLine(text);
                }
            }

            WriteLine($" do");
            _indent++;
            base.VisitNamespaceDeclaration(node);
            _indent--;
            WriteLine("end");
        }

        private void WriteLine(string text)
        {
            _output.AppendLine(GetTabString() + text);

        }

        private void Write(string text)
        {
            _output.Append(GetTabString() + text);
        }

        private string GetTabString()
        {
            return string.Concat(Enumerable.Repeat(" ", _indentSize * _indent));
        }

        private List<string> GetNames(SyntaxNode node)
        {
            var names = new List<string>();
            var childNodes = node.ChildNodes();
            var qualifiedNameNodes = node.IsKind(SyntaxKind.QualifiedName) ? [(QualifiedNameSyntax)node] : childNodes.OfType<QualifiedNameSyntax>();
            var identifierNameNodes = node.IsKind(SyntaxKind.IdentifierName) ? [(IdentifierNameSyntax)node] : childNodes.OfType<IdentifierNameSyntax>();

            foreach (var qualifiedNameNode in qualifiedNameNodes)
            {
                foreach (var name in GetNames(qualifiedNameNode.Left))
                {
                    names.Add(name);
                }
                foreach (var name in GetNames(qualifiedNameNode.Right))
                {
                    names.Add(name);
                }
            }

            foreach (var identifierNameNode in identifierNameNodes)
            {
                names.Add(identifierNameNode.Identifier.Text);
            }

            return names;
        }
    }
}
