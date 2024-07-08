using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
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

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var parentNamespace = FindFirstAncestor<NamespaceDeclarationSyntax>(node);
            string namespaceName = string.Join(".", GetNames(parentNamespace));
            var className = GetNames(node).First();
            var luaClassName = namespaceName + (namespaceName == "" ? "" : ".") + className;
            Write(luaClassName);
            WriteLine(" = {} do");
            _indent++;

            WriteLine($"local {className} = {luaClassName}");
            if (!HasSyntax(node.Modifiers, SyntaxKind.StaticKeyword))
            {
                WriteLine($"{className}.__index = {className}");
            }

            PrintChildNodes(node);
            base.VisitClassDeclaration(node);

            _indent--;
            WriteLine("end");
        }

        private void WriteLine(string? text = null)
        {
            if (text == null)
            {
                _output.AppendLine();
                return;
            }

            WriteTab();
            _output.AppendLine(text);
        }

        private void Write(string text)
        {
            WriteTab();
            _output.Append(text);
        }

        private void WriteTab()
        {
            _output.Append(GetLastCharacter() == '\n' ? GetTabString() : "");
        }

        private string GetTabString()
        {
            return string.Concat(Enumerable.Repeat(" ", _indentSize * _indent));
        }

        private bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax)
        {
            return tokens.Any(token => token.IsKind(syntax));
        }

        private T? FindFirstAncestor<T>(SyntaxNode node) where T : SyntaxNode
        {
            return node.Ancestors().OfType<T>().FirstOrDefault();
        }

        private List<string> GetNames(SyntaxNode? node)
        {
            var names = new List<string>();
            if (node == null) return names;

            var identifierProperty = node.GetType().GetProperty("Identifier");
            var identifierValue = identifierProperty?.GetValue(node);
            if (identifierProperty != null && identifierValue != null && identifierValue is SyntaxToken)
            {
                names.Add(((SyntaxToken)identifierValue).Text.Trim());
                return names;
            }

            var childNodes = node.ChildNodes();
            var qualifiedNameNodes = node.IsKind(SyntaxKind.QualifiedName) ? [(QualifiedNameSyntax)node] : childNodes.OfType<QualifiedNameSyntax>();
            var identifierNameNodes = node.IsKind(SyntaxKind.IdentifierName) ? [(IdentifierNameSyntax)node] : childNodes.OfType<IdentifierNameSyntax>();

            foreach (var qualifiedNameNode in qualifiedNameNodes)
            {
                foreach (var name in GetNames(qualifiedNameNode.Left))
                {
                    names.Add(name.Trim());
                }
                foreach (var name in GetNames(qualifiedNameNode.Right))
                {
                    names.Add(name.Trim());
                }
            }

            foreach (var identifierNameNode in identifierNameNodes)
            {
                names.Add(identifierNameNode.Identifier.Text.Trim());
            }

            return names;
        }

        private char? GetLastCharacter()
        {
            if (_output.Length == 0) return null;
            return _output[_output.Length - 1];
        }

        private void PrintChildNodes(SyntaxNode node)
        {
            Console.WriteLine($"{node.Kind()} node children: {node.ChildNodes().Count()}");
            foreach (var child in node.ChildNodes())
            {
                Console.WriteLine(child.Kind().ToString() + ": " + child.GetText());
            }
        }

        private void PrintChildTokens(SyntaxNode node)
        {
            Console.WriteLine($"{node.Kind()} token children: {node.ChildTokens().Count()}");
            foreach (var child in node.ChildTokens())
            {
                Console.WriteLine(child.Kind().ToString() + ": " + child.Text);
            }
        }
    }
}
