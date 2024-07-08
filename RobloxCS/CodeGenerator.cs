using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace RobloxCS
{
    internal class CodeGenerator : CSharpSyntaxWalker
    {
        private readonly List<SyntaxKind> _memberParentSyntaxes = new List<SyntaxKind>([
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration
        ]);

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

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.Utf8StringLiteralExpression:
                case SyntaxKind.CharacterLiteralExpression:
                    {
                        Write($"\"{node.Token.ValueText}\"");
                        break;
                    }

                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                case SyntaxKind.NumericLiteralExpression:
                    {
                        Write(node.Token.ValueText);
                        break;
                    }
                case SyntaxKind.NullLiteralExpression:
                    {
                        Write("nil");
                        break;
                    }

                case SyntaxKind.DefaultLiteralExpression:
                    {
                        throw new Exception("\"default\" keyword is not supported!");
                    }
            }

            base.VisitLiteralExpression(node);
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
            var namespaceName = string.Join(".", GetNames(parentNamespace));
            var className = GetNames(node).First();
            var luaClassName = namespaceName + (namespaceName == "" ? "" : ".") + className;
            Write(luaClassName);
            WriteLine(" = {} do");
            _indent++;

            WriteLine($"local {className} = {luaClassName}");
            var isStatic = HasSyntax(node.Modifiers, SyntaxKind.StaticKeyword);
            if (!isStatic)
            {
                WriteLine($"{className}.__index = {className}");
            }

            WriteLine();
            var staticFields = node.Members.OfType<FieldDeclarationSyntax>()
                .Where(member => member.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword)));

            InitializeDefaultFields(staticFields);

            var constructor = node.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (!isStatic)
            {
                if (constructor == null)
                {
                    CreateDefaultConstructor(node);
                }
                else
                {
                    VisitConstructorDeclaration(constructor);
                }
            }

            _indent--;
            WriteLine("end");
        }

        public override void VisitParameterList(ParameterListSyntax node)
        {
            Write("(");
            foreach (var parameter in node.Parameters)
            {
                Write(GetNames(parameter).First());
                if (parameter != node.Parameters.Last())
                {
                    Write(",");
                }
            }
            WriteLine(")");
            _indent++;

            foreach (var parameter in node.Parameters)
            {
                if (parameter.Default == null) continue;

                var name = GetNames(parameter).First();
                Write(name);
                Write(" = ");
                Write($"if {name} == nil then ");
                Visit(parameter.Default);
                WriteLine($" else {name}");
            }

            _indent--;
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            // TODO: struct support?
            var parentClass = FindFirstAncestor<ClassDeclarationSyntax>(node)!;
            var parentName = GetNames(parentClass).First();

            Write($"function {parentName}.new");
            Visit(node.ParameterList);
            _indent++;

            VisitConstructorBody(parentClass);

            _indent--;
            WriteLine("end");
        }

        private void VisitConstructorBody(ClassDeclarationSyntax parentClass)
        {
            var parentName = GetNames(parentClass).First();
            WriteLine($"local self = setmetatable({{}}, {parentName})");
            if (parentClass != null)
            {
                var nonStaticFields = parentClass.Members.OfType<FieldDeclarationSyntax>()
                    .Where(member => member.Modifiers.All(modifier => !modifier.IsKind(SyntaxKind.StaticKeyword)));

                InitializeDefaultFields(nonStaticFields); // TODO: structs
            }
            WriteLine("return self");
        }

        private void CreateDefaultConstructor(ClassDeclarationSyntax node)
        {
            string parentName = GetNames(node).First();
            WriteLine($"function {parentName}.new()");
            _indent++;

            VisitConstructorBody(node);

            _indent--;
            WriteLine("end");
        }

        private void InitializeDefaultFields(IEnumerable<FieldDeclarationSyntax> fields)
        {
            foreach (var field in fields)
            {
                var parentClass = FindFirstAncestor<ClassDeclarationSyntax>(field)!;
                var parentName = GetNames(parentClass).First();
                var isStatic = HasSyntax(field.Modifiers, SyntaxKind.StaticKeyword);

                foreach (var declarator in field.Declaration.Variables)
                {
                    if (declarator.Initializer == null) continue;

                    var name = GetNames(declarator).First();
                    Write($"{(isStatic ? parentName : "self")}.{name} = ");
                    Visit(declarator.Initializer);
                    WriteLine();
                }
            }
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
            _output.Append(MatchLastCharacter('\n') ? GetTabString() : "");
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

        private bool MatchLastCharacter(char character)
        {
            if (_output.Length == 0) return false;
            return _output[_output.Length - 1] == character;
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
