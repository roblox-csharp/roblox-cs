using System.Text;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RobloxCS
{
    internal class CodeGeneratorFlags
    {
        public bool ShouldCallGetAssemblyType { get; set; }
        public bool ClientEntryPointDefined { get; set; }
        public bool ServerEntryPointDefined { get; set; }
    }

    internal sealed class CodeGenerator : CSharpSyntaxWalker
    {
        private readonly SyntaxTree _tree;
        private readonly ConfigData _config;
        private readonly MemberCollectionResult _members;
        private readonly string _inputDirectory;
        private readonly CSharpCompilation _compiler;
        private readonly SemanticModel _semanticModel;
        private readonly SymbolAnalyzerResults _symbolAnalysis;
        private readonly INamespaceSymbol _globalNamespace;
        private readonly INamespaceSymbol _runtimeLibNamespace;
        private readonly RojoProject? _rojoProject;
        private readonly int _indentSize;
        private readonly CodeGeneratorFlags _flags = new CodeGeneratorFlags
        {
            ShouldCallGetAssemblyType = true,
            ClientEntryPointDefined = false,
            ServerEntryPointDefined = false
        };

        private readonly StringBuilder _output = new StringBuilder();
        private int _indent = 0;

        public CodeGenerator(
            SyntaxTree tree,
            CSharpCompilation compiler,
            RojoProject? rojoProject,
            MemberCollectionResult members,
            ConfigData config,
            string inputDirectory,
            int indentSize = 4
        )
        {
            _tree = tree;
            _config = config;
            _members = members;
            _inputDirectory = inputDirectory;
            _compiler = compiler;
            _semanticModel = compiler.GetSemanticModel(tree);

            var symbolAnalyzer = new SymbolAnalyzer(tree, _semanticModel);
            _symbolAnalysis = symbolAnalyzer.Analyze();
            _globalNamespace = compiler.GlobalNamespace;
            _runtimeLibNamespace = _globalNamespace.GetNamespaceMembers().FirstOrDefault(ns => ns.Name == Utility.RuntimeAssemblyName)!;
            _rojoProject = rojoProject;
            _indentSize = indentSize;
        }

        public string GenerateLua()
        {
            WriteHeader();
            Visit(_tree.GetRoot());
            WriteFooter();
            return _output.ToString().Trim();
        }

        private void WriteHeader()
        {
            Write($"local CS = ");
            WriteRequire(GetLuaRuntimeLibPath());
            WriteLine();

            foreach (var (namespaceName, declaringFiles) in GetNamespaceFilePaths())
            {
                var symbol = _tree.GetRoot()
                    .DescendantNodes()
                    .Select(node => _semanticModel.GetTypeInfo(node).Type)
                    .OfType<INamespaceOrTypeSymbol>()
                    .Where(namespaceSymbol => namespaceSymbol.ContainingAssembly != null && namespaceSymbol.ContainingAssembly.Name == _config.CSharpOptions.AssemblyName)
                    .FirstOrDefault(symbol => symbol.Name == namespaceName);

                var namespaceSymbol = symbol == null ? null : (symbol.IsNamespace ? symbol : symbol.ContainingNamespace);
                if (namespaceSymbol == null || !_symbolAnalysis.TypeHasMemberUsedAsValue(namespaceSymbol))
                {
                    continue;
                }

                WriteLine($"-- using {namespaceName};");
                foreach (var csharpFilePath in declaringFiles)
                {
                    WriteLine($"require({GetRequirePath(csharpFilePath)})");
                }
                WriteLine();
            }
        }

        private void WriteFooter()
        {
            if (!_tree.FilePath.EndsWith(".client.cs") && !_tree.FilePath.EndsWith(".server.cs"))
            {
                WriteLine("return {}");
            }
        }

        private string GetLuaRuntimeLibPath()
        {
            if (_rojoProject == null)
            {
                return $"\"{Utility.LuaRuntimeModuleName}\"";
            }
            else
            {
                return RojoReader.ResolveInstancePath(_rojoProject, $"include/{Utility.LuaRuntimeModuleName}.lua")!;
            }
        }

        private string GetRequirePath(string longCSharpFilePath)
        {
            var inputDirectory = _inputDirectory.EndsWith('/') ? _inputDirectory : _inputDirectory + '/';
            var csharpFilePath = longCSharpFilePath.Replace(inputDirectory, "").Replace(_config.SourceFolder, _config.OutputFolder);
            if (csharpFilePath.StartsWith('/'))
            {
                csharpFilePath = csharpFilePath.Substring(1);
            }

            if (_rojoProject == null)
            {
                return "\"./" + csharpFilePath.Replace(".cs", "") + '"';
            }
            else
            {
                return RojoReader.ResolveInstancePath(_rojoProject, csharpFilePath)!;
            }
        }

        private Dictionary<string, HashSet<string>> GetNamespaceFilePaths()
        {
            var globalNamespaceSymbols = _tree.GetRoot()
                .DescendantNodes()
                .Select(node => _semanticModel.GetTypeInfo(node).Type)
                .OfType<INamedTypeSymbol>()
                .Where(namespaceSymbol => namespaceSymbol.ContainingAssembly != null && namespaceSymbol.ContainingAssembly.Name == _config.CSharpOptions.AssemblyName);

            return new Dictionary<string, HashSet<string>>(
                Utility.FilterDuplicates(globalNamespaceSymbols, SymbolEqualityComparer.Default)
                    .OfType<INamedTypeSymbol>()
                    .Select(namespaceSymbol => KeyValuePair.Create(namespaceSymbol.Name, GetPathsForSymbolDeclarations(namespaceSymbol)))
                    .Where(pair => !string.IsNullOrEmpty(pair.Key) && !pair.Value.Contains(_tree.FilePath))
            );
        }

        private HashSet<string> GetPathsForSymbolDeclarations(ISymbol symbol)
        {
            var filePaths = new HashSet<string>();
            foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                var syntaxTree = syntaxReference.SyntaxTree;
                var filePath = syntaxTree.FilePath;
                if (!string.IsNullOrEmpty(filePath))
                {
                    filePaths.Add(filePath);
                }
            }

            return filePaths;
        }

        public override void VisitRefExpression(RefExpressionSyntax node)
        {
            Logger.UnsupportedError(node, "Refs");
        }

        public override void VisitUnsafeStatement(UnsafeStatementSyntax node)
        {
            Logger.UnsupportedError(node, "Unsafe contexts");
        }

        public override void VisitUsingStatement(UsingStatementSyntax node)
        {
            Logger.UnsupportedError(node, "Using statements");
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            if (GetName(node) == "Native")
            {
                WriteLine("@native");
            }
        }

        public override void VisitAttributeList(AttributeListSyntax node)
        {
            base.VisitAttributeList(node);
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            foreach (var attributeList in node.AttributeLists)
            {
                Visit(attributeList);
            }
            Write($"local function {GetName(node)}");
            Visit(node.ParameterList);
            _indent++;

            Visit(node.Body);
            WriteDefaultReturn(node.Body);

            _indent--;
            WriteLine("end");
        }

        public override void VisitCastExpression(CastExpressionSyntax node)
        {
            // ignore
            Visit(node.Expression);
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            Throw(node.Expression);
        }

        public override void VisitThrowExpression(ThrowExpressionSyntax node)
        {
            Throw(node.Expression);
        }

        private void Throw(ExpressionSyntax? exception)
        {
            if (exception == null)
            {
                WriteLine("_catch_rethrow()");
            }
            else
            {
                Visit(exception);
                Write(":Throw(");
                Write((exception.Parent?.Parent is TryStatementSyntax).ToString().ToLower());
                WriteLine(')');
            }
        }

        public override void VisitTryStatement(TryStatementSyntax node)
        {
            WriteLine("CS.try(function()");
            _indent++;

            Visit(node.Block);

            _indent--;
            Write("end, ");
            if (node.Finally != null)
            {
                WriteLine("function()");
                _indent++;

                Visit(node.Finally);

                _indent--;
                Write("end");
            }
            else
            {
                Write("nil");
            }
            WriteLine(", {");
            _indent++;

            foreach (var catchBlock in node.Catches)
            {
                WriteLine('{');
                _indent++;

                Write("exceptionClass = ");
                if (catchBlock.Declaration != null)
                {
                    Write('"');
                    Write(catchBlock.Declaration.Type.ToString());
                    Write('"');
                    WriteLine(", ");
                }
                Write("block = function(");
                Write(catchBlock.Declaration != null ? (GetName(catchBlock.Declaration) + ": CS.Exception") : "_");
                WriteLine(", _catch_rethrow: () -> nil)");
                _indent++;

                Visit(catchBlock.Block);

                _indent--;
                WriteLine("end");

                _indent--;
                WriteLine('}');
            }

            _indent--;
            WriteLine("})");
        }

        public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            Visit(node.Variable);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            Write("for _, ");
            Write(GetName(node));
            Write(" in ");
            Visit(node.Expression);
            WriteLine(" do");
            _indent++;

            Visit(node.Statement);

            _indent--;
            WriteLine("end");
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            var hasDeclaration = node.Declaration != null;
            if (hasDeclaration)
            {
                WriteLine("do");
                _indent++;
                Visit(node.Declaration);
            }
            foreach (var initializer in node.Initializers)
            {
                Visit(initializer);
            }
            Write("while ");
            if (node.Condition != null)
            {
                Visit(node.Condition);
            }
            else
            {
                Write("true");
            }
            WriteLine(" do");
            _indent++;


            Visit(node.Statement);
            foreach (var incrementor in node.Incrementors)
            {
                Visit(incrementor);
            }

            _indent--;
            WriteLine("end");

            if (hasDeclaration)
            {
                _indent--;
                WriteLine("end");
            }
        }

        public override void VisitLabeledStatement(LabeledStatementSyntax node)
        {
            Logger.UnsupportedError(node, "Labels & goto statements");
        }

        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            Logger.UnsupportedError(node, "Labels & goto statements");
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            WriteLine("repeat");
            _indent++;

            Visit(node.Statement);

            _indent--;
            Write("until ");
            Visit(node.Condition);
            WriteLine();
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            Write("while ");
            Visit(node.Condition);
            WriteLine(" do");
            _indent++;

            Visit(node.Statement);

            _indent--;
            WriteLine("end");
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            Write("if ");
            Visit(node.Condition);
            WriteLine(" then");
            _indent++;

            WritePatternDeclarations(node.Condition);
            Visit(node.Statement);

            _indent--;
            if (node.Else != null)
            {
                var isElseIf = node.Else.Statement.IsKind(SyntaxKind.IfStatement);

                Write("else");
                if (!isElseIf)
                {
                    WriteLine();
                    _indent++;
                }

                Visit(node.Else);
                if (!isElseIf)
                {
                    _indent--;
                    WriteLine("end");
                }
            }
            else
            {
                WriteLine("end");
            }
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            var condition = node.Expression;
            WriteLine("repeat");
            _indent++;

            var checkNoFallthrough = (StatementSyntax statement) =>
                !statement.IsKind(SyntaxKind.BreakStatement)
                && !statement.IsKind(SyntaxKind.ReturnStatement)
                && !statement.DescendantNodes().All(descendant => !descendant.IsKind(SyntaxKind.BreakStatement) && !descendant.IsKind(SyntaxKind.ReturnStatement));

            if (node.Sections.Count > 0 && !node.Sections.Any(section => section.Statements.Any(checkNoFallthrough))) // TODO: check for break/return
            {
                WriteLine("local _fallthrough = false");
            }
            foreach (var section in node.Sections)
            {
                var statementKinds = section.Statements.Select(stmt => stmt.Kind());
                HashSet<SyntaxKind> supportedPatterns = [
                    SyntaxKind.VarPattern,
                    SyntaxKind.DeclarationPattern
                ];

                var caseLabels = section.Labels.Where(label =>
                    !label.IsKind(SyntaxKind.DefaultSwitchLabel)
                        && !(label is CasePatternSwitchLabelSyntax casePattern
                            && supportedPatterns.Contains(casePattern.Pattern.Kind()))
                );

                foreach (var label in caseLabels)
                {
                    Write("if ");
                    if (label != caseLabels.FirstOrDefault())
                    {
                        Write("_fallthrough or ");
                    }

                    var expressions = label.ChildNodes();
                    var expression = expressions.First();
                    Write('(');
                    Visit(condition);

                    var op = expression.IsKind(SyntaxKind.NotPattern) ?
                        "~="
                        : expression is RelationalPatternSyntax relationalPattern ?
                            Utility.GetMappedOperator(relationalPattern.OperatorToken.Text)
                            : "==";

                    Write($" {op} ");
                    Visit(expression);
                    if (label is CasePatternSwitchLabelSyntax casePattern && casePattern.WhenClause != null)
                    {
                        Write(" and ");
                        Visit(casePattern.WhenClause.Condition);
                    }
                    WriteLine(") then");
                    _indent++;

                    if (label != caseLabels.Last())
                    {
                        WriteLine("_fallthrough = true");
                    }
                    else
                    {
                        foreach (var statement in section.Statements)
                        {
                            Visit(statement);
                            if (statement == section.Statements.Last() && !statement.IsKind(SyntaxKind.ReturnStatement))
                            {
                                WriteLine("break");
                            }
                        }
                    }

                    _indent--;
                    WriteLine("end");
                }
            }

            void visitDefaultStatements(SwitchLabelSyntax defaultLabel)
            {
                var section = (SwitchSectionSyntax)defaultLabel.Parent!;
                foreach (var statement in section.Statements)
                {
                    Visit(statement);
                }
            }

            var casePatternLabels = node.Sections.SelectMany(section => section.Labels.Where(label => label.IsKind(SyntaxKind.CasePatternSwitchLabel)));
            foreach (var casePatternLabel in casePatternLabels)
            {
                var pattern = casePatternLabel.ChildNodes().FirstOrDefault();
                if (pattern.IsKind(SyntaxKind.VarPattern) || pattern.IsKind(SyntaxKind.DeclarationPattern))
                {
                    Visit(pattern);
                    Write(" = ");
                    Visit(condition);
                    WriteLine();
                    visitDefaultStatements(casePatternLabel);
                }
            }

            var defaultLabel = node.Sections.SelectMany(section => section.Labels.Where(label => label.IsKind(SyntaxKind.DefaultSwitchLabel))).FirstOrDefault();
            if (defaultLabel != null)
            {
                visitDefaultStatements(defaultLabel);
            }

            _indent--;
            WriteLine("until true");
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            Write("function");
            Visit(node.ParameterList);
            WriteLine();
            _indent++;

            if (node.Block != null || node.ExpressionBody.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                Visit(node.Body);
            }
            else
            {
                Write("return ");
                Visit(node.ExpressionBody);
                WriteLine();
            }

            _indent--;
            Write("end");
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            Write("function(");
            Visit(node.Parameter);
            Write(')');
            WriteLine();
            _indent++;

            if (node.Block != null || node.ExpressionBody.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                Visit(node.Body);
            }
            else
            {
                Write("return ");
                Visit(node.ExpressionBody);
                WriteLine();
            }

            _indent--;
            Write("end");
        }

        public override void VisitBracketedArgumentList(BracketedArgumentListSyntax node)
        {
            Write('[');
            if (node.Arguments.Count > 1)
            {
                Logger.CodegenError(node.Arguments.Last(), "Cannot have more than one argument between brackets.");
            }

            foreach (var argument in node.Arguments)
            {
                if (argument.Expression is LiteralExpressionSyntax numericLiteral && numericLiteral.IsKind(SyntaxKind.NumericLiteralExpression))
                {
                    int.TryParse(numericLiteral.Token.ValueText, out var indexValue);
                    Write((indexValue + 1).ToString());
                }
                else
                {
                    Visit(argument);

                    var typeSymbol = _semanticModel.GetTypeInfo(argument.Expression).Type;
                    if (typeSymbol != null && Constants.INTEGER_TYPES.Contains(typeSymbol.Name))
                    {
                        Write(" + 1");
                    }
                }
            }
            Write(']');
        }

        public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            Write("if ");
            Visit(node.Expression);
            Write(" == nil then nil else ");
            Visit(node.WhenNotNull);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Write("if ");
            Visit(node.Condition);
            Write(" then ");
            Visit(node.WhenTrue);
            Write(" else ");
            Visit(node.WhenFalse);
        }

        public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            Write('(');
            Visit(node.Expression);
            Write(')');
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            Visit(node.Operand);
            if (node.IsKind(SyntaxKind.SuppressNullableWarningExpression))
            {
                var typeSymbol = _semanticModel.GetTypeInfo(node.Operand).Type;
                if (typeSymbol == null)
                {
                    Write(" :: any");
                }
                return;
            }

            var mappedOperator = Utility.GetMappedOperator(node.OperatorToken.Text);
            WriteLine($" {mappedOperator} 1");
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var mappedOperator = Utility.GetMappedOperator(node.OperatorToken.Text);
            var bit32Method = Utility.GetBit32MethodName(mappedOperator);
            if (bit32Method != mappedOperator)
            {
                Write($"bit32.{bit32Method}(");
                Visit(node.Operand);
                Write(")");
                return;
            }

            Write(mappedOperator);
            Visit(node.Operand);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var operatorText = node.OperatorToken.Text;
            var leftType = _semanticModel.GetTypeInfo(node.Left).Type;
            var rightType = _semanticModel.GetTypeInfo(node.Right).Type;
            var perTypeOperators = Constants.PER_TYPE_BINARY_OPERATOR_MAP;
            if (
                (leftType != null && perTypeOperators.Any(pair => pair.Key.Contains(leftType.Name)))
                || (rightType != null && perTypeOperators.Any(pair => pair.Key.Contains(rightType.Name)))
            )
            {
                var typeList = perTypeOperators.FirstOrDefault(pair => pair.Key.Contains(leftType!.Name) || pair.Key.Contains(rightType!.Name)).Key;
                var operatorMap = perTypeOperators[typeList];
                if (operatorText == operatorMap.Item1)
                {
                    operatorText = operatorMap.Item2;
                }
            }

            if (Constants.IGNORED_BINARY_OPERATORS.Contains(operatorText))
            {
                Visit(node.Left);
                return;
            }

            var mappedOperator = Utility.GetMappedOperator(operatorText);
            switch (mappedOperator)
            {
                case "??":
                    Write("if ");
                    Visit(node.Left);
                    Write(" == nil then ");
                    Visit(node.Right);
                    Write(" else ");
                    Visit(node.Left);
                    return;
            }

            var bit32Method = Utility.GetBit32MethodName(mappedOperator);
            if (bit32Method != mappedOperator)
            {
                Write($"bit32.{bit32Method}(");
                Visit(node.Left);
                Write(", ");
                Visit(node.Right);
                Write(")");

                if ((leftType == null || rightType == null) || (!Constants.UNSUPPORTED_BITWISE_TYPES.Contains(leftType.Name) && !Constants.UNSUPPORTED_BITWISE_TYPES.Contains(rightType.Name)))
                    return;

                Logger.CodegenWarning(node, "Using 128/64 bit integers when performing binary operations on Luau may result in undefined behaviour.");
                WriteLine();
                Write("--> rbxcsc warn: long/Int64 or Int128/UInt128 bit operations may lead to undefined behaviour (above this warning).");

                return;
            }

            Visit(node.Left);
            Write($" {mappedOperator} ");
            Visit(node.Right);
        }

        public override void VisitTupleExpression(TupleExpressionSyntax node)
        {
            WriteListTable(node.Arguments.Select(arg => arg.Expression).ToList());
        }

        public override void VisitCollectionExpression(CollectionExpressionSyntax node)
        {
            Write('{');
            foreach (var element in node.Elements)
            {
                var children = element.ChildNodes();
                foreach (var child in children)
                {
                    Visit(child);
                }
                if (element != node.Elements.Last())
                {
                    Write(", ");
                }
            }
            Write('}');
        }

        public override void VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            Visit(node.Initializer);
        }

        public override void VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
        {
            Visit(node.Initializer);
        }

        public override void VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            WriteListTable(node.Expressions.ToList());
        }

        public override void VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
        {
            if (node.NameEquals != null)
            {
                Write(GetName(node.NameEquals));
                Write(" = ");
            }
            Visit(node.Expression);
        }

        public override void VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
        {
            WriteLine('{');
            _indent++;

            foreach (var initializer in node.Initializers)
            {
                Visit(initializer);
                if (initializer != node.Initializers.Last())
                {
                    Write(", ");
                }
                WriteLine();
            }

            _indent--;
            WriteLine('}');
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

        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            Write("continue");
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
            Write('(');
            foreach (var argument in node.Arguments)
            {
                Visit(argument);
                if (argument != node.Arguments.Last())
                {
                    Write(", ");
                }
            }
            Write(')');
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {

            if (node.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var objectType = _semanticModel.GetTypeInfo(memberAccess.Expression).Type;
                var name = GetName(memberAccess.Name);

                switch (name)
                {
                    case "ToString":
                        Write("tostring(");
                        Visit(memberAccess.Expression);
                        Write(')');
                        return;
                    case "Equals":
                        Visit(memberAccess.Expression);
                        Write(" == ");

                        var value = node.ArgumentList.Arguments.FirstOrDefault();
                        if (value != null)
                        {
                            Visit(value);
                        }
                        else
                        {
                            Write("nil");
                        }
                        return;
                    case "Length":
                        if (objectType == null || !Constants.LENGTH_READABLE_TYPES.Contains(objectType.Name)) return;
                        Write('#');
                        Visit(memberAccess.Expression);
                        return;
                    case "ToLower":
                    case "ToUpper":
                    case "Replace":
                    case "Split":
                        if (objectType == null || objectType.Name.ToLower() != "string") return;
                        Write('(');
                        Visit(memberAccess.Expression);
                        Write(')');
                        Write(':');

                        var methodName = GetName(memberAccess.Name);
                        var mappedMethodName = Constants.MAPPED_STRING_METHODS.GetValueOrDefault(methodName, methodName);
                        Write(mappedMethodName);
                        Visit(node.ArgumentList);
                        return;
                    case "Create":
                        {
                            if (objectType == null || objectType.Name != "Instance") break;

                            var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
                            if (symbol is IMethodSymbol methodSymbol)
                            {
                                if (!methodSymbol.IsGenericMethod)
                                    Logger.CompilerError("Attempt to macro Instance.Create<T>() but it is not generic");

                                var arguments = node.ArgumentList.Arguments;
                                var instanceType = methodSymbol.TypeArguments.First();
                                Visit(memberAccess.Expression);
                                Write($".new(\"{instanceType.Name}\"");
                                if (arguments.Count > 0)
                                {
                                    Write(", ");
                                    foreach (var argument in arguments)
                                    {
                                        Visit(argument.Expression);
                                        if (argument != arguments.Last())
                                        {
                                            Write(", ");
                                        }
                                    }
                                }

                                Write(')');
                                return;
                            }
                            break;
                        }
                    case "GetService":
                    case "FindFirstChildOfClass":
                    case "FindFirstChildWhichIsA":
                    case "FindFirstAncestorOfClass":
                    case "FindFirstAncestorWhichIsA":
                    case "IsA":
                        {
                            if (objectType == null) return;

                            var superclasses = objectType.AllInterfaces.ToList();
                            if (objectType.Name != "Instance" && !superclasses.Select(@interface => @interface.Name).Contains("Instance")) return;

                            var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
                            var methodSymbol = (IMethodSymbol)symbol!;
                            if (!methodSymbol.IsGenericMethod)
                                Logger.CompilerError($"Attempt to macro {objectType.Name}.{name}<T>() but it is not generic");

                            var arguments = node.ArgumentList.Arguments;
                            var instanceType = methodSymbol.TypeArguments.First();
                            Visit(memberAccess.Expression);
                            Write($":{name}(\"{instanceType.Name}\"");
                            if (arguments.Count > 0)
                            {
                                Write(", ");
                                foreach (var argument in arguments)
                                {
                                    Visit(argument.Expression);
                                    if (argument != arguments.Last())
                                    {
                                        Write(", ");
                                    }
                                }
                            }
                            else
                            {
                                Write(')');
                            }
                            return;
                        }
                }
            }
            else if (node.Expression is IdentifierNameSyntax || node.Expression is GenericNameSyntax)
            {
                var name = GetName(node.Expression);
                switch (name)
                {
                    case "TypeOf":
                        Write("typeof");
                        Visit(node.ArgumentList);
                        return;

                    case "ToNumber":
                    case "ToFloat":
                    case "ToDouble":
                    case "ToInt":
                    case "ToUInt":
                    case "ToShort":
                    case "ToUShort":
                    case "ToByte":
                    case "ToSByte":
                        Write("tonumber");
                        Visit(node.ArgumentList);
                        return;

                    case "nameof":
                        Write('"');
                        Write(_semanticModel.GetConstantValue(node).Value?.ToString() ?? $"??? nameof({node}) ???");
                        Write('"');
                        return;
                }
            }

            Visit(node.Expression);
            Visit(node.ArgumentList);
        }

        public override void VisitThisExpression(ThisExpressionSyntax node)
        {
            Write("self");
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var leftIsLiteral = node.Expression is LiteralExpressionSyntax;
            var objectSymbol = _semanticModel.GetSymbolInfo(node.Expression).Symbol?.OriginalDefinition;
            var objectType = _semanticModel.GetTypeInfo(node.Expression).Type;
            var nameType = _semanticModel.GetSymbolInfo(node.Name).Symbol?.OriginalDefinition;
            var operatorText = '.';
            if (objectSymbol != null)
            {
                if (objectType is INamedTypeSymbol objectDefinitionSymbol && (objectDefinitionSymbol.Name == "Services" || objectDefinitionSymbol.AllInterfaces.Select(@interface => @interface.Name).Contains("Services")))
                {
                    Write("game:GetService(\"");
                    Visit(node.Name);
                    Write("\")");
                    return;
                }

                if (nameType is IMethodSymbol methodSymbol)
                {
                    operatorText = methodSymbol.IsStatic ? '.' : ':';
                }
            }

            var usings = GetUsings();
            var containingNamespace = objectSymbol?.ContainingNamespace;
            if (objectSymbol != null && (objectSymbol.Kind == SymbolKind.Namespace || (objectSymbol.Kind == SymbolKind.NamedType && objectSymbol.IsStatic)))
            {
                var namespaceName = objectSymbol.ToDisplayString();
                var filePathsContainingType = objectSymbol.Locations
                    .Where(location => location.SourceTree != null && location.SourceTree.FilePath != _tree.FilePath)
                    .Select(location => location.SourceTree!.FilePath);

                var isNoFullQualificationType = Constants.NO_FULL_QUALIFICATION_TYPES.Contains(namespaceName) || (containingNamespace != null ? Constants.NO_FULL_QUALIFICATION_TYPES.Contains(containingNamespace.Name) : false);
                var noFullQualification = isNoFullQualificationType && (objectType == null || !Constants.GLOBAL_LIBRARIES.Contains(objectType.Name));
                var typeIsImported = usings.Any(usingDirective => usingDirective.Name != null && Utility.GetNamesFromNode(usingDirective).Any(name => namespaceName.StartsWith(name)));
                if (noFullQualification && namespaceName != "System")
                {
                    if (node.Expression is IdentifierNameSyntax identifier)
                    {
                        // TODO: check parent classes of parent class
                        var parentClass = FindFirstAncestor<ClassDeclarationSyntax>(node);
                        var parentClassSymbol = parentClass != null ? _semanticModel.GetDeclaredSymbol(parentClass) : null;
                        if (parentClass != null && SymbolEqualityComparer.Default.Equals(objectType, parentClassSymbol))
                        {
                            Write("class");
                        }
                        else
                        {
                            if (GetName(node.Name) == "Globals") return;
                            Visit(node.Name);
                        }
                    }
                    else if (node.Expression is GenericNameSyntax genericName)
                    {
                        Visit(genericName);
                    }
                    else if (node.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        if (namespaceName == Utility.RuntimeAssemblyName && Utility.GetNamesFromNode(memberAccess.Expression).LastOrDefault() != "Globals")
                        {
                            Visit(memberAccess.Name);
                            Write('.');
                        }
                        Visit(node.Name);
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported node.Expression type for a NO_FULL_QUALIFICATION_TYPES member");
                    }
                    return;
                }
            }

            if (objectSymbol?.OriginalDefinition is ILocalSymbol typeSymbol)
            {
                switch (typeSymbol.Type.Name)
                {
                    case "ValueTuple":
                        var name = GetName(node.Name);
                        if (name.StartsWith("Item"))
                        {
                            var itemIndex = name.Split("Item").Last();
                            if (leftIsLiteral)
                            {
                                Write('(');
                            }
                            Visit(node.Expression);
                            if (leftIsLiteral)
                            {
                                Write(')');
                            }
                            Write($"[{itemIndex}]");
                            return;
                        }
                        break;
                }
            }

            if (leftIsLiteral)
            {
                Write('(');
            }
            Visit(node.Expression);
            if (leftIsLiteral)
            {
                Write(')');
            }
            Write(operatorText);
            Visit(node.Name);
        }

        private List<UsingDirectiveSyntax> GetUsings()
        {
            var root = _tree.GetRoot();
            var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();
            var compilationUnit = root as CompilationUnitSyntax;
            if (compilationUnit != null)
            {
                foreach (var usingDirective in compilationUnit.Usings)
                {
                    usings.Add(usingDirective);
                }
            }

            return usings;
        }

        private void FullyQualifyMemberAccess(INamespaceSymbol? namespaceType, List<UsingDirectiveSyntax> usings)
        {
            if (namespaceType == null) return;

            var typeIsImported = usings.Any(usingDirective => namespaceType.ContainingNamespace != null && usingDirective.Name != null && Utility.GetNamesFromNode(usingDirective).Contains(namespaceType.ContainingNamespace.Name));
            Write($"CS.getAssemblyType(\"{namespaceType.Name}\").");
            _flags.ShouldCallGetAssemblyType = false;
        }

        public override void VisitIsPatternExpression(IsPatternExpressionSyntax node)
        {
            var objectType = _semanticModel.GetTypeInfo(node.Expression).Type;
            var superclasses = objectType == null ? [] : objectType.AllInterfaces.ToList();
            var pattern = node.Pattern;

            void writeTypePattern(TypeSyntax type)
            {
                var typeSymbol = _semanticModel.GetTypeInfo(type).Type;
                var mappedType = Utility.GetMappedType(type.ToString());
                HashSet<TypeKind> valueTypes = [TypeKind.Class, TypeKind.Struct, TypeKind.Enum];
                var isValueType = typeSymbol != null && valueTypes.Contains(typeSymbol.TypeKind);
                if (!isValueType)
                {
                    Write('"');
                }
                Write(mappedType);
                if (!isValueType)
                {
                    Write('"');
                }
            }

            (bool, Action) getPatternWriter()
            {
                if (pattern is TypePatternSyntax typePattern)
                {
                    return (true, () => writeTypePattern(typePattern.Type));
                }
                else if (pattern is DeclarationPatternSyntax declarationPattern)
                {
                    return (true, () => writeTypePattern(declarationPattern.Type));
                }
                else if (pattern is VarPatternSyntax varPattern)
                {
                    return (true, () => Write("true"));
                }
                else
                {
                    return (false, () => Visit(pattern));
                }
            }

            var (willBeHandled, writePattern) = getPatternWriter();
            if (!willBeHandled && pattern.IsKind(SyntaxKind.NotPattern))
            {
                Write("not ");
            }
            Write("CS.is(");
            Visit(node.Expression);
            Write(", ");
            writePattern();
            Write(')');
        }

        public override void VisitDeclarationPattern(DeclarationPatternSyntax node)
        {
            Visit(node.Designation);
            WriteTypeAnnotation(node.Type);
        }

        public override void VisitDiscardDesignation(DiscardDesignationSyntax node)
        {
            // do nothing (discard)
        }

        public override void VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
        {
            Write($"local {GetName(node)}");
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            foreach (var declarator in node.Variables)
            {
                Visit(declarator);
            }
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            Write($"local {GetName(node)}");
            var parent = node.Parent;
            if (parent is VariableDeclarationSyntax declaration)
            {
                WriteTypeAnnotation(declaration.Type);
            }

            if (node.Initializer != null)
            {
                Write(" = ");
                Visit(node.Initializer);
                if (node.Initializer.Value.IsKind(SyntaxKind.IsPatternExpression))
                {
                    WriteLine();
                }
                WritePatternDeclarations(node.Initializer.Value);
            }
            WriteLine();
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            Visit(node.Left);
            Write(" = ");
            Visit(node.Right);
            if (node.Right.IsKind(SyntaxKind.IsPatternExpression))
            {
                WriteLine();
            }
            WritePatternDeclarations(node.Right);
        }

        public override void VisitGenericName(GenericNameSyntax node)
        {
            WriteName(node, node.Identifier);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            WriteName(node, node.Identifier);
        }

        public override void VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
        {
            Write(node.TextToken.Text);
        }

        public override void VisitInterpolation(InterpolationSyntax node)
        {
            Write('{');
            Visit(node.Expression);
            Write('}');
        }

        public override void VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            Write('`');
            foreach (var content in node.Contents)
            {
                Visit(content);
            }
            Write('`');
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.Utf8StringLiteralExpression:
                case SyntaxKind.CharacterLiteralExpression:
                    Write($"\"{node.Token.ValueText}\"");
                    break;

                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                case SyntaxKind.NumericLiteralExpression:
                    Write(node.Token.ValueText);
                    break;

                case SyntaxKind.DefaultLiteralExpression:
                    var typeSymbol = _semanticModel.GetTypeInfo(node).Type;
                    if (typeSymbol == null) break;

                    if (Constants.INTEGER_TYPES.Contains(typeSymbol.Name) || Constants.DECIMAL_TYPES.Contains(typeSymbol.Name))
                    {
                        Write("0");
                        break;
                    }

                    switch (typeSymbol.Name)
                    {
                        case "char":
                        case "Char":
                        case "string":
                        case "String":
                            Write("\"\"");
                            break;
                        case "bool":
                        case "Boolean":
                            Write("false");
                            break;
                        default:
                            Write("nil");
                            break;
                    }
                    break;
                case SyntaxKind.NullLiteralExpression:
                    Write("nil");
                    break;
            }

            base.VisitLiteralExpression(node);
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            if (node.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ParamsKeyword)))
            {
                Write("...");
                if (node.Type != null)
                    WriteTypeAnnotation(node.Type);
                return;
            }
            Write(GetName(node));
            if (node.Type != null)
            {
                WriteTypeAnnotation(node.Type);
            }
        }

        public override void VisitParameterList(ParameterListSyntax node)
        {
            var callable = node.Parent;
            Write('(');
            {
                if (callable is MethodDeclarationSyntax method && !HasSyntax(method.Modifiers, SyntaxKind.StaticKeyword))
                {
                    Write('_');
                    if (node.Parameters.Count > 0)
                    {
                        Write(", ");
                    }
                }
            }
            ParameterSyntax? vaargParameter = null;
            foreach (var parameter in node.Parameters)
            {
                Visit(parameter);
                if (parameter != node.Parameters.Last())
                {
                    Write(", ");
                }
                else
                {
                    if (parameter.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ParamsKeyword))) vaargParameter = parameter;
                }
            }
            Write(')');
            switch (callable)
            {
                case ConstructorDeclarationSyntax constructor:
                    WriteLine($": {GetName(constructor)}");
                    break;
                case MethodDeclarationSyntax method:
                    WriteTypeAnnotation(method.ReturnType, true);
                    break;
                case LocalFunctionStatementSyntax localFunction:
                    WriteTypeAnnotation(localFunction.ReturnType, true);
                    break;
                default:
                    WriteLine();
                    break;
            }
            _indent++;

            if (vaargParameter != null)
            {
                WriteLine("--> rbxcsc: vararg parameter conversion");
                Write($"local {GetName(vaargParameter)} = {{ ... }}");
                WriteLine();
            }


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
            var allNames = Utility.GetNamesFromNode(node);
            var firstName = allNames.First();
            allNames.Remove(firstName);

            var nativeAttribute = _config.EmitNativeAttributeOnClassOrNamespaceCallbacks ? "@native " : "";
            WriteLine($"{(isWithinNamespace ? "namespace:" : "CS.")}namespace(\"{firstName}\", {nativeAttribute}function(namespace: CS.Namespace)");
            _indent++;

            if (allNames.Count > 0)
            {
                foreach (var name in allNames)
                {
                    WriteLine($"namespace:namespace(\"{name}\", {nativeAttribute}function(namespace: CS.Namespace)");
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
            WriteLine();
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var isWithinNamespace = IsDescendantOf<NamespaceDeclarationSyntax>(node);
            Write($"CS.enum(\"{GetName(node)}\", {{");
            if (node.Members.Count > 0)
            {
                WriteLine();
                _indent++;

                var firstValue = node.Members.FirstOrDefault()?.EqualsValue?.Value;
                var lastIndex = firstValue != null ? (firstValue as LiteralExpressionSyntax)?.Token.Value as int? ?? -1 : -1;
                foreach (var enumMember in node.Members)
                {
                    Write(GetName(enumMember));
                    Write(" = ");
                    if (enumMember.EqualsValue != null)
                    {
                        Visit(enumMember.EqualsValue);
                        lastIndex = (int)((LiteralExpressionSyntax)enumMember.EqualsValue.Value).Token.Value!;
                    }
                    else
                    {
                        var index = (enumMember.EqualsValue?.Value is LiteralExpressionSyntax literal ? literal.Token.Value as int? : null) ?? lastIndex + 1;
                        lastIndex = index;
                        Write(index.ToString());
                    }
                    WriteLine(enumMember != node.Members.Last() ? ", " : "");
                }

                _indent--;
            }
            WriteLine($"}}, {(isWithinNamespace ? "namespace" : "nil")})");
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            foreach (var member in node.Members)
            {
                if (HasSyntax(member.Modifiers, SyntaxKind.VirtualKeyword))
                {
                    Logger.UnsupportedError(node, "Virtual methods on interfaces");
                }
            }
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (HasSyntax(node.Modifiers, SyntaxKind.AbstractKeyword))
            {
                Logger.UnsupportedError(node, "Abstract classes");
                return;
            }
            if (HasSyntax(node.Modifiers, SyntaxKind.PartialKeyword))
            {
                Logger.UnsupportedError(node, "Partial classes");
                return;
            }

            var isWithinNamespace = IsDescendantOf<NamespaceDeclarationSyntax>(node);
            var className = GetName(node);
            var nativeAttribute = _config.EmitNativeAttributeOnClassOrNamespaceCallbacks ? "@native " : "";
            WriteLine($"{(isWithinNamespace ? "namespace:" : "CS.")}class(\"{className}\", {nativeAttribute}function(namespace: CS.Namespace)");
            _indent++;

            Write($"local class = CS.classDef(\"{GetName(node)}\", ");
            Write(isWithinNamespace ? "namespace" : "nil");

            // TODO: check if superclass or mixin
            var ancestorIdentifiers = (node.BaseList?.Types ?? []).Select(ancestor =>
            {
                var typeSymbol = _semanticModel.GetTypeInfo(ancestor.Type).Type;
                return $"\"{Regex.Replace(typeSymbol?.ToString() ?? ancestor.Type.ToString(), @"<[^>]*>", "")}\"";
            });
            if (ancestorIdentifiers.Count() > 0)
            {
                Write(", ");
            }
            Write(string.Join(", ", ancestorIdentifiers));
            WriteLine(')');
            WriteLine();
            InitializeFields(
                node.Members
                    .OfType<FieldDeclarationSyntax>()
                    .Where(member => HasSyntax(member.Modifiers, SyntaxKind.StaticKeyword))
            );
            InitializeProperties(
                node.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(member => HasSyntax(member.Modifiers, SyntaxKind.StaticKeyword))
            );

            var isStatic = HasSyntax(node.Modifiers, SyntaxKind.StaticKeyword);
            var constructors = node.Members.OfType<ConstructorDeclarationSyntax>().ToList();
            constructors.Sort((a, b) => a.ParameterList.Parameters.Count - b.ParameterList.Parameters.Count);

            var constructor = constructors.FirstOrDefault();
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
                var filePath = Path.TrimEndingDirectorySeparator(_tree.FilePath);
                var isClientFile = filePath.EndsWith(".client.cs");
                if ((_flags.ClientEntryPointDefined && isClientFile) || (_flags.ServerEntryPointDefined && !isClientFile))
                {
                    Logger.CodegenError(node, $"No more than one main method can be defined on the {(isClientFile ? "client" : "server")}.");
                }
                if (isClientFile)
                {
                    _flags.ClientEntryPointDefined = true;
                }
                else
                {
                    _flags.ServerEntryPointDefined = true;
                }

                var mainMethod = methods.FirstOrDefault(method => GetName(method) == _config.CSharpOptions.MainMethodName);
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
            WriteLine($"return class");

            _indent--;
            WriteLine("end)");
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            foreach (var attributeList in node.AttributeLists)
            {
                Visit(attributeList);
            }

            var isStatic = HasSyntax(node.Modifiers, SyntaxKind.StaticKeyword);
            var isMetamethod = Constants.METAMETHODS.Contains(GetName(node));
            var objectName = "self";
            if (isStatic)
            {
                objectName = "class";
            }
            else if (isMetamethod)
            {
                objectName = "mt";
            }

            var name = GetName(node);
            Write($"function {objectName}.{name}");
            Visit(node.ParameterList);
            _indent++;

            Visit(node.Body);
            WriteDefaultReturn(node.Body);

            _indent--;
            WriteLine("end");
        }

        public override void VisitBaseExpression(BaseExpressionSyntax node)
        {
            Write("self[\"$superclass\"]");
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            // TODO: struct support?
            foreach (var attributeList in node.AttributeLists)
            {
                Visit(attributeList);
            }
            Write($"function class.new");
            Visit(node.ParameterList);
            _indent++;

            VisitConstructorBody(FindFirstAncestor<ClassDeclarationSyntax>(node)!, node.Body, node.Initializer?.ArgumentList);

            _indent--;
            WriteLine("end");
        }

        private void VisitConstructorBody(ClassDeclarationSyntax parentClass, BlockSyntax? block, ArgumentListSyntax? initializerArguments)
        {
            var isWithinNamespace = IsDescendantOf<NamespaceDeclarationSyntax>(parentClass);
            WriteLine("local mt = {}");
            Write("local self = CS.classInstance(class, mt");
            if (isWithinNamespace)
            {
                Write(", namespace");
            }
            WriteLine(')'); // TODO: (when typedefs are generated for classes) $") :: {GetName(parentClass)}" 
            WriteLine();
            if (initializerArguments != null)
            {
                Write("self[\"$base\"]");
                Visit(initializerArguments);
            }

            var isNotStatic = (MemberDeclarationSyntax member) => !HasSyntax(member.Modifiers, SyntaxKind.StaticKeyword);
            var isNotAbstract = (MemberDeclarationSyntax member) => !HasSyntax(member.Modifiers, SyntaxKind.AbstractKeyword);
            var nonStaticFields = parentClass.Members
                .Where(isNotStatic)
                .Where(isNotAbstract)
                .OfType<FieldDeclarationSyntax>();
            var nonStaticProperties = parentClass.Members
                .Where(isNotStatic)
                .Where(isNotAbstract)
                .OfType<PropertyDeclarationSyntax>();
            var nonStaticMethods = parentClass.Members
                .Where(isNotStatic)
                .Where(isNotAbstract)
                .OfType<MethodDeclarationSyntax>();

            InitializeFields(nonStaticFields);
            InitializeProperties(nonStaticProperties);
            if (block != null)
            {
                WriteLine();
                Visit(block);
            }
            if (nonStaticMethods.Count() > 0)
            {
                WriteLine();
            }
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

            VisitConstructorBody(node, null, null);

            _indent--;
            WriteLine("end");
        }

        private void InitializeFields(IEnumerable<FieldDeclarationSyntax> fields)
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

        private void InitializeProperties(IEnumerable<PropertyDeclarationSyntax> properties)
        {
            foreach (var property in properties)
            {
                if (property.Initializer == null) continue;

                var isStatic = HasSyntax(property.Modifiers, SyntaxKind.StaticKeyword);
                Write($"{(isStatic ? "class" : "self")}.{GetName(property)} = ");
                Visit(property.Initializer);
                WriteLine();
            }
        }

        private void WriteName(SyntaxNode node, SyntaxToken identifier)
        {
            var identifierText = identifier.ValueText;
            var originalIdentifierName = identifier.Text;
            if (identifierText == "var") return;
            if (Constants.LUAU_KEYWORDS.Contains(identifierText))
            {
                Logger.CodegenError(node, $"Using reserved Luau keywords as identifier names is unsupported!");
            }

            var isWithinClass = IsDescendantOf<ClassDeclarationSyntax>(node);
            var prefix = "";
            if (isWithinClass)
            {
                // Check the fields in classes that this node is a descendant of for the identifier name
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
                            if (name != identifierText) continue;
                            prefix = (isStatic ? "class" : "self") + '.';
                        }
                    }
                }
            }

            if (prefix == "")
            {
                var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
                var parentNamespace = FindFirstAncestor<NamespaceDeclarationSyntax>(node);
                var parentNamespaceSymbol = parentNamespace != null ? _semanticModel.GetDeclaredSymbol(parentNamespace) : null;
                var pluginClassesNamespace = _runtimeLibNamespace.GetNamespaceMembers().FirstOrDefault(ns => ns.Name == "PluginClasses");
                var runtimeNamespaceIncludesIdentifier = symbol != null ? (
                    IsDescendantOfNamespaceSymbol(symbol, _runtimeLibNamespace)
                    || (pluginClassesNamespace != null && IsDescendantOfNamespaceSymbol(symbol, pluginClassesNamespace))
                ) : false;

                HashSet<SyntaxKind> fullyQualifiedParentKinds = [SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.ObjectCreationExpression];
                if (
                    symbol != null
                    && symbol is ITypeSymbol typeSymbol
                    && node.Parent != null
                    && fullyQualifiedParentKinds.Contains(node.Parent.Kind())
                    && typeSymbol.ContainingNamespace != null
                    && (parentNamespace != null ? Utility.GetNamesFromNode(parentNamespace.Name).LastOrDefault() != typeSymbol.ContainingNamespace.Name : true)
                    && !Constants.NO_FULL_QUALIFICATION_TYPES.Contains(typeSymbol.ContainingNamespace.Name)
                )
                {
                    var usings = GetUsings();
                    FullyQualifyMemberAccess(typeSymbol.ContainingNamespace, usings);
                }

                var parentAccessExpression = FindFirstAncestor<MemberAccessExpressionSyntax>(node);
                var isLeftSide = parentAccessExpression == null ? true : node == parentAccessExpression.Expression;
                var parentBlocks = GetAncestors<SyntaxNode>(node);
                var localScopeIncludesIdentifier = parentBlocks.Any(block =>
                {
                    var descendants = block.DescendantNodes();
                    var localFunctions = descendants.OfType<LocalFunctionStatementSyntax>();
                    var variableDesignations = descendants.OfType<VariableDesignationSyntax>();
                    var variableDeclarators = descendants.OfType<VariableDeclaratorSyntax>();
                    var forEachStatements = descendants.OfType<ForEachStatementSyntax>();
                    var forStatements = descendants.OfType<ForStatementSyntax>();
                    var parameters = descendants.OfType<ParameterSyntax>();
                    var catchDeclarations = descendants.OfType<CatchDeclarationSyntax>();
                    var checkNamePredicate = (SyntaxNode node) => TryGetName(node) == identifierText;
                    return localFunctions.Any(checkNamePredicate)
                        || variableDesignations.Any(checkNamePredicate)
                        || variableDeclarators.Any(checkNamePredicate)
                        || parameters.Any(checkNamePredicate)
                        || catchDeclarations.Any(checkNamePredicate)
                        || forEachStatements.Any(checkNamePredicate)
                        || forStatements.Any(forStatement => forStatement.Initializers.Count() > 0);
                });

                if (isLeftSide && !localScopeIncludesIdentifier && !runtimeNamespaceIncludesIdentifier)
                {
                    var namespaceSymbol = parentNamespace != null ? _semanticModel.GetDeclaredSymbol(parentNamespace) : null;
                    var namespaceIncludesIdentifier = namespaceSymbol != null && Utility.FindMember(namespaceSymbol, originalIdentifierName) != null;
                    var parentClass = FindFirstAncestor<ClassDeclarationSyntax>(node);
                    var classSymbol = parentClass != null ? _semanticModel.GetDeclaredSymbol(parentClass) : null;
                    var classMemberSymbol = classSymbol != null ? Utility.FindMemberDeep(classSymbol, originalIdentifierName) : null;

                    if (namespaceIncludesIdentifier)
                    {
                        Write($"namespace[\"$getMember\"](namespace, \"{identifierText}\")");
                    }
                    else if (classMemberSymbol != null)
                    {
                        Write($"{(classMemberSymbol.IsStatic ? "class" : "self")}.{identifierText}");
                    }
                    else
                    {
                        if (_flags.ShouldCallGetAssemblyType)
                        {
                            Write($"CS.getAssemblyType(\"{identifierText}\")");
                        }
                        else
                        {
                            Write(identifierText);
                            _flags.ShouldCallGetAssemblyType = true;
                        }
                    }
                }
                else
                {
                    Write(identifierText);
                }
            }
            else
            {
                Write(prefix + identifierText);
            }
        }

        private void WriteListTable(List<ExpressionSyntax> expressions)
        {
            Write('{');
            foreach (var expression in expressions)
            {
                Visit(expression);
                if (expression != expressions.Last())
                {
                    Write(", ");
                }
            }
            Write('}');
        }

        private void WriteDefaultReturn(BlockSyntax? block)
        {
            if (block != null && !block.Statements.Any(stmt => stmt.IsKind(SyntaxKind.ReturnStatement)))
            {
                WriteLine("return nil :: any");
            }
        }

        private void WritePatternDeclarations(SyntaxNode node)
        {
            if (node is IsPatternExpressionSyntax isPattern)
            {
                void writeInitializer()
                {
                    Write(" = ");
                    Visit(isPattern.Expression);
                    WriteLine();
                }

                if (isPattern.Pattern is DeclarationPatternSyntax declarationPattern)
                {
                    Visit(declarationPattern.Designation);
                    writeInitializer();
                }
                else if (isPattern.Pattern is VarPatternSyntax varPattern)
                {
                    Visit(varPattern.Designation);
                    writeInitializer();
                }
            }
        }

        private void WriteTypeAnnotation(TypeSyntax type, bool isReturnType = false)
        {
            if (!type.IsVar)
            {
                var mappedType = Utility.GetMappedType(type.ToString());
                Write($": {mappedType}");
                if (isReturnType)
                {
                    WriteLine();
                }
            }
        }

        private void WriteRequire(string path)
        {
            WriteLine($"require({path})");
        }

        private void WriteLine()
        {
            WriteLine("");
        }

        private void WriteLine(char text)
        {
            WriteLine(text.ToString());
        }

        private void WriteLine(string text)
        {
            if (text == null)
            {
                _output.AppendLine();
                return;
            }

            WriteTab();
            _output.AppendLine(text);
        }

        private void Write(char text)
        {
            Write(text.ToString());
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

        private void RemoveLastCharacters(int amount)
        {
            _output.Remove(_output.Length - amount, amount);
        }

        private bool HasSyntax(SyntaxTokenList tokens, SyntaxKind syntax)
        {
            return tokens.Any(token => token.IsKind(syntax));
        }

        private bool IsDescendantOfNamespaceSymbol(ISymbol symbol, INamespaceSymbol ancestor)
        {
            var namespaceSymbol = symbol.ContainingNamespace;
            while (namespaceSymbol != null)
            {
                if (SymbolEqualityComparer.Default.Equals(namespaceSymbol, ancestor))
                {
                    return true;
                }
                namespaceSymbol = namespaceSymbol.ContainingNamespace;
            }
            return false;
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

        private string? TryGetName(SyntaxNode node)
        {
            return Utility.GetNamesFromNode(node).FirstOrDefault();
        }

        private string GetName(SyntaxNode node)
        {
            return Utility.GetNamesFromNode(node).First();
        }

        private bool MatchLastCharacter(char character)
        {
            if (_output.Length == 0) return false;
            return _output[_output.Length - 1] == character;
        }
    }
}