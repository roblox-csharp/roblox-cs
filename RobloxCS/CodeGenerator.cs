using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Text;

namespace RobloxCS
{
    internal sealed class CodeGenerator : CSharpSyntaxWalker
    {
        private readonly List<SyntaxKind> _memberParentSyntaxes = new List<SyntaxKind>([
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration
        ]);

        private readonly SyntaxNode _root;
        private readonly ConfigData _config;
        private readonly int _indentSize;

        private readonly StringBuilder _output = new StringBuilder();
        private int _indent = 0;

        public CodeGenerator(SyntaxNode root, ConfigData config, int indentSize = 4)
        {
            _root = root;
            _config = config;
            _indentSize = indentSize;
        }

        public string GenerateLua()
        {
            WriteHeader();
            Visit(_root);
            return _output.ToString().Trim();
        }

        private void WriteHeader()
        {
            if (Util.IsDebug())
            {
                WriteLine($"package.path = \"{Util.GetRbxcsDirectory()}/{Util.RuntimeAssemblyName}/?.lua;\" .. package.path");
            }
            WriteLine($"local CS = require({GetLuaRuntimeLibPath()})");
            WriteLine();
        }

        private string GetLuaRuntimeLibPath()
        {
            if (Util.IsDebug())
            {
                return "\"RuntimeLib\"";
            }
            else
            {
                // TODO: read rojo project file and locate rbxcs_include
                return "game:GetService(\"ReplicatedStorage\").rbxcs_include.RuntimeLib";
            }
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            Visit(node.Left);
            Write($" {Util.GetMappedOperator(node.OperatorToken.Text)} ");
            Visit(node.Right);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            Visit(node.Type);
            Write(".new");
            Visit(node.ArgumentList);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            // do nothing (for now)
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            Write("return ");
            Visit(node.Expression);
            WriteLine();
        }

        public override void VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            base.VisitExpressionStatement(node);
            WriteLine();
        }

        public override void VisitArgumentList(ArgumentListSyntax node)
        {
            Write("(");
            foreach (var argument in node.Arguments)
            {
                Visit(argument);
                if (argument != node.Arguments.Last())
                {
                    Write(", ");
                }
            }
            Write(")");
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression is MemberAccessExpressionSyntax)
            {
                var memberAccess = (MemberAccessExpressionSyntax)node.Expression;
                if (GetName(memberAccess.Name) == "ToString")
                {
                    Write("tostring(");
                    Visit(memberAccess.Expression);
                    Write(")");
                    return;
                }
            }

            Visit(node.Expression);
            Visit(node.ArgumentList);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            Visit(node.Expression);
            Write(".");
            Visit(node.Name);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            Write($"local {node.Identifier.ValueText}");
            if (node.Initializer != null)
            {
                Write(" = ");
                Visit(node.Initializer);
            }
            WriteLine();
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            Visit(node.Left);
            Write(" = ");
            Visit(node.Right);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            var identifierName = node.Identifier.ValueText;
            if (identifierName == "var") return;

            var isWithinClass = IsDescendantOf<ClassDeclarationSyntax>(node);
            var prefix = "";
            if (isWithinClass)
            {
                // Check the fields in classes that this node
                // is a descendant of for the identifier name
                var ancestorClasses = GetAncestors<ClassDeclarationSyntax>(node);
                for (int i = 0; i < ancestorClasses.Length; i++)
                {
                    var ancestorClass = ancestorClasses[i];
                    var fields = ancestorClass.Members.OfType<FieldDeclarationSyntax>();
                    foreach (var field in fields)
                    {
                        var isStatic = HasSyntax(field.Modifiers, SyntaxKind.StaticKeyword);
                        foreach (var declarator in field.Declaration.Variables)
                        {
                            var name = GetName(declarator);
                            if (name != identifierName) continue;
                            prefix = (isStatic ? GetName(ancestorClass) : "self") + ".";
                        }
                    }
                }
            }

            if (prefix == "")
            {
                var parentNamespace = FindFirstAncestor<NamespaceDeclarationSyntax>(node);
                var namespaceIncludesIdentifier = parentNamespace != null && parentNamespace.Members
                    .Where(member => GetNames(member).Contains(identifierName))
                    .Count() > 0;

                var parentAccessExpression = FindFirstAncestor<MemberAccessExpressionSyntax>(node);
                var isLeftSide = parentAccessExpression == null ? true : node == parentAccessExpression.Expression;
                var parentBlocks = GetAncestors<SyntaxNode>(node);
                var localScopeIncludesIdentifier = parentBlocks
                    .Any(block =>
                    {
                        var descendants = block.DescendantNodes();
                        var variableDeclarators = descendants.OfType<VariableDeclaratorSyntax>();
                        var parameters = descendants.OfType<ParameterSyntax>();
                        var checkNamePredicate = (SyntaxNode node) => GetName(node) == identifierName;
                        return variableDeclarators.Where(checkNamePredicate).Count() > 0
                            || parameters.Where(checkNamePredicate).Count() > 0;
                    });

                if (isLeftSide && !localScopeIncludesIdentifier)
                {
                    if (namespaceIncludesIdentifier)
                    {
                        Write($"namespace[\"$getMember\"](namespace, \"{identifierName}\")");
                    }
                    else
                    {
                        Write($"CS.getAssemblyType(\"{identifierName}\")");
                    }
                }
                else
                {
                    Write(identifierName);
                }
            }
            else
            {
                Write(prefix + identifierName);
            }
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
                        Logger.CodegenError(node, "\"default\" keyword is not supported!");
                        break;
                    }
            }

            base.VisitLiteralExpression(node);
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            Write(GetName(node));
        }

        public override void VisitParameterList(ParameterListSyntax node)
        {
            Write("(");
            foreach (var parameter in node.Parameters)
            {
                Visit(parameter);
                if (parameter != node.Parameters.Last())
                {
                    Write(", ");
                }
            }
            WriteLine(")");
            _indent++;

            foreach (var parameter in node.Parameters)
            {
                if (parameter.Default == null) continue;

                var name = GetName(parameter);
                Write(name);
                Write(" = ");
                Write($"if {name} == nil then ");
                Visit(parameter.Default);
                WriteLine($" else {name}");
            }

            _indent--;
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var isWithinNamespace = IsDescendantOf<NamespaceDeclarationSyntax>(node);
            var allNames = GetNames(node);
            var firstName = allNames.First();
            allNames.Remove(firstName);

            WriteLine($"{(isWithinNamespace ? "namespace:" : "CS.")}namespace(\"{firstName}\", function(namespace)");
            _indent++;

            if (allNames.Count > 0)
            {
                foreach (var name in allNames)
                {
                    WriteLine($"namespace:namespace(\"{name}\", function(namespace)");
                    _indent++;

                    foreach (var member in node.Members)
                    {
                        Visit(member);
                    }

                    _indent--;
                    WriteLine("end)");
                }
            }
            else
            {
                foreach (var member in node.Members)
                {
                    Visit(member);
                }
            }

            _indent--;
            WriteLine("end)");
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var parentNamespace = FindFirstAncestor<NamespaceDeclarationSyntax>(node);
            var className = GetName(node);
            WriteLine($"namespace:class(\"{className}\", function(namespace)");
            _indent++;

            WriteLine("local class = {}");
            WriteLine("class.__index = class");
            WriteLine();
            InitializeDefaultFields(
                node.Members
                    .OfType<FieldDeclarationSyntax>()
                    .Where(member => HasSyntax(member.Modifiers, SyntaxKind.StaticKeyword))
            );

            var isStatic = HasSyntax(node.Modifiers, SyntaxKind.StaticKeyword);
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

            var methods = node.Members.OfType<MethodDeclarationSyntax>();
            var staticMethods = methods.Where(method => HasSyntax(method.Modifiers, SyntaxKind.StaticKeyword));
            foreach (var method in staticMethods)
            {
                Visit(method);
            }

            var isEntryPointClass = GetName(node) == _config.CSharpOptions.EntryPointName;
            if (isEntryPointClass)
            {
                var mainMethod = methods
                    .Where(method => GetName(method) == _config.CSharpOptions.MainMethodName)
                    .FirstOrDefault();

                if (mainMethod == null)
                {
                    Logger.CodegenError(node.Identifier, $"No main method \"{_config.CSharpOptions.MainMethodName}\" found in entry point class");
                    return;
                }
                if (!HasSyntax(mainMethod.Modifiers, SyntaxKind.StaticKeyword))
                {
                    Logger.CodegenError(node.Identifier, $"Main method must be static.");
                }

                WriteLine();
                WriteLine("if namespace == nil then");
                _indent++;
                WriteLine($"class.{_config.CSharpOptions.MainMethodName}()");
                _indent--;
                WriteLine("else");
                _indent++;
                WriteLine($"namespace[\"$onLoaded\"](namespace, class.{_config.CSharpOptions.MainMethodName})");
                _indent--;
                Write("end");
            }

            WriteLine();
            WriteLine($"return {(isStatic ? "class" : "setmetatable({}, class)")}");

            _indent--;
            WriteLine("end)");
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var isStatic = HasSyntax(node.Modifiers, SyntaxKind.StaticKeyword);
            var name = GetName(node);
            Write($"function {(isStatic ? "class" : "self")}.{name}");
            Visit(node.ParameterList);
            _indent++;

            Visit(node.Body);

            _indent--;
            WriteLine("end");
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            // TODO: struct support?
            Write($"function class.new");
            Visit(node.ParameterList);
            _indent++;

            VisitConstructorBody(FindFirstAncestor<ClassDeclarationSyntax>(node)!, node.Body);

            _indent--;
            WriteLine("end");
        }

        private void VisitNamespace(string name, bool isDescendantOfNamespace)
        {

        }

        private void VisitConstructorBody(ClassDeclarationSyntax parentClass, BlockSyntax? block)
        {
            WriteLine("local self = setmetatable({}, class)");
            WriteLine();
            var nonStaticFields = parentClass.Members
                .OfType<FieldDeclarationSyntax>()
                .Where(member => !HasSyntax(member.Modifiers, SyntaxKind.StaticKeyword));

            InitializeDefaultFields(nonStaticFields);
            var nonStaticMethods = parentClass.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(member => !HasSyntax(member.Modifiers, SyntaxKind.StaticKeyword));

            if (block != null) Visit(block);
            WriteLine();

            foreach (var method in nonStaticMethods)
            {
                Visit(method);
            }
            WriteLine();
            WriteLine("return self");
        }

        private void CreateDefaultConstructor(ClassDeclarationSyntax node)
        {
            WriteLine($"function class.new()");
            _indent++;

            VisitConstructorBody(node, null);

            _indent--;
            WriteLine("end");
        }

        private void InitializeDefaultFields(IEnumerable<FieldDeclarationSyntax> fields)
        {
            foreach (var field in fields)
            {
                var isStatic = HasSyntax(field.Modifiers, SyntaxKind.StaticKeyword);
                foreach (var declarator in field.Declaration.Variables)
                {
                    if (declarator.Initializer == null) continue;
                    Write($"{(isStatic ? "class" : "self")}.{GetName(declarator)} = ");
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

        private bool IsDescendantOf<T>(SyntaxNode node) where T : SyntaxNode
        {
            return FindFirstAncestor<T>(node) != null;
        }

        private T? FindFirstAncestor<T>(SyntaxNode node) where T : SyntaxNode
        {
            return GetAncestors<T>(node).FirstOrDefault();
        }

        private T[] GetAncestors<T>(SyntaxNode node) where T : SyntaxNode
        {
            return node.Ancestors().OfType<T>().ToArray();
        }

        private string GetName(SyntaxNode node)
        {
            return GetNames(node).First();
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
    }
}
