using System.Text;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS
{
    enum CodeGenFlag
    {
        ShouldCallGetAssemblyType
    }

    internal sealed class CodeGenerator : CSharpSyntaxWalker
    {
        private readonly List<SyntaxKind> _memberParentSyntaxes = new List<SyntaxKind>([
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.StructDeclaration
        ]);

        private readonly SyntaxTree _tree;
        private readonly ConfigData _config;
        private readonly MemberCollectionResult _members;
        private readonly string _inputDirectory;
        private readonly CSharpCompilation _compiler;
        private readonly SemanticModel _semanticModel;
        private readonly INamespaceSymbol _globalNamespace;
        private readonly INamespaceSymbol _runtimeLibNamespace;
        private readonly RojoProject _rojoProject; // make sure you check that it is not in debug mode before using this field!
        private readonly int _indentSize;
        private readonly Dictionary<CodeGenFlag, bool> _flags = new Dictionary<CodeGenFlag, bool>
        {
            [CodeGenFlag.ShouldCallGetAssemblyType] = true
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
            _globalNamespace = compiler.GlobalNamespace;
            _runtimeLibNamespace = _globalNamespace.GetNamespaceMembers().FirstOrDefault(ns => ns.Name == Utility.RuntimeAssemblyName)!;
            _rojoProject = rojoProject!;
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
            if (Utility.IsDebug())
            {
                WriteLine($"package.path = \"{Utility.GetRbxcsDirectory()}/{Utility.RuntimeAssemblyName}/?.lua;\" .. package.path");
            }
            Write($"local CS = ");
            WriteRequire(GetLuaRuntimeLibPath());
            WriteLine();

            foreach (var (namespaceName, declaringFiles) in GetNamespaceFilePaths())
            {
                WriteLine($"-- Imports for \"{namespaceName}\"");
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
            if (Utility.IsDebug())
            {
                return "\"RuntimeLib\"";
            }
            else
            {
                // TODO: read rojo project file and locate rbxcs_include
                return "game:GetService(\"ReplicatedStorage\").rbxcs_include.RuntimeLib";
            }
        }

        private string GetRequirePath(string longCSharpFilePath)
        {
            var inputDirectory = Path.Combine(_inputDirectory);
            var csharpFilePath = longCSharpFilePath.Replace(inputDirectory, "").Replace(_config.SourceFolder, _config.OutputFolder);
            if (Utility.IsDebug())
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
                .Where(namespaceSymbol => namespaceSymbol.ContainingAssembly.Name == _config.CSharpOptions.AssemblyName);

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

        public override void VisitCastExpression(CastExpressionSyntax node)
        {
            // ignore
            Visit(node.Expression);
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
            WriteLine("do");
            _indent++;

            Visit(node.Declaration);
            foreach (var initializer in node.Initializers)
            {
                Visit(initializer);
            }
            WriteLine("while true do");
            _indent++;

            Write("if not (");
            Visit(node.Condition);
            WriteLine(") then break end");
            Visit(node.Statement);
            foreach (var incrementor in node.Incrementors)
            {
                Visit(incrementor);
            }

            _indent--;
            WriteLine("end");

            _indent--;
            WriteLine("end");
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

            if (
                (node.Statement is ReturnStatementSyntax returnStatement && returnStatement.Expression == null
                || node.Statement is ContinueStatementSyntax)
                && node.Else == null
            )
            {
                Write(" then ");
                Visit(node.Statement);
                if (node.Statement is ReturnStatementSyntax)
                {
                    RemoveLastCharacters(3);
                }
                WriteLine(" end");
                return;
            }
            else
            {
                WriteLine(" then");
                _indent++;

                Visit(node.Statement);

                _indent--;
            }

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

        private void RemoveLastCharacters(int amount)
        {
            _output.Remove(_output.Length - amount, amount);
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
                    var isNumericalIndex = typeSymbol != null && Constants.INTEGER_TYPES.Contains(typeSymbol.Name);
                    if (isNumericalIndex)
                    {
                        Write(" + 1");
                    }
                }

                if (argument != node.Arguments.Last())
                {
                    Write(", ");
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

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            Visit(node.Operand);
            if (node.IsKind(SyntaxKind.SuppressNullableWarningExpression)) return;

            var mappedOperator = Utility.GetMappedOperator(node.OperatorToken.Text);
            WriteLine($" {mappedOperator} 1");
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            Write(Utility.GetMappedOperator(node.OperatorToken.Text));
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
                var objectName = GetName(memberAccess.Expression);
                var name = GetName(memberAccess.Name);

                switch (name)
                {
                    case "ToString":
                        Write("tostring(");
                        Visit(memberAccess.Expression);
                        Write(')');
                        return;
                    case "Write":
                    case "WriteLine":
                        {
                            if (objectType == null || objectType.Name != "Console") break;

                            Write("print");
                            Visit(node.ArgumentList);
                            return;
                        }
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

                var nameSymbolInfo = _semanticModel.GetSymbolInfo(node);
                {
                    if (nameSymbolInfo.Symbol is IMethodSymbol methodSymbol)
                    {
                        var operatorText = methodSymbol.IsStatic ? "." : ":";
                        Visit(memberAccess.Expression);
                        Write(operatorText);
                        Visit(memberAccess.Name);
                        Visit(node.ArgumentList);
                        return;
                    }
                }
            } else if (node.Expression is IdentifierNameSyntax identifier)
            {
                var name = GetName(identifier);
                switch (name)
                {
                    case "typeOf":
                        Write("typeof");
                        Visit(node.ArgumentList);
                        return;
                }
            }

            Visit(node.Expression);
            Visit(node.ArgumentList);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var objectSymbolInfo = _semanticModel.GetSymbolInfo(node.Expression);
            var objectType = _semanticModel.GetTypeInfo(node.Expression).Type;
            if (objectType != null && objectType.OriginalDefinition is INamedTypeSymbol objectDefinitionSymbol)
            {
                var superclasses = objectDefinitionSymbol.AllInterfaces;
                if (objectDefinitionSymbol.Name == "Services" || superclasses.Select(@interface => @interface.Name).Contains("Services"))
                {
                    Write("game:GetService(\"");
                    Visit(node.Name);
                    Write("\")");
                    return;
                }
            }

            var usings = GetUsings();
            if (objectSymbolInfo.Symbol != null && (objectSymbolInfo.Symbol.Kind == SymbolKind.Namespace || (objectSymbolInfo.Symbol.Kind == SymbolKind.NamedType && objectSymbolInfo.Symbol.IsStatic)))
            {
                var namespaceName = objectSymbolInfo.Symbol.ToDisplayString();
                var filePathsContainingType = objectSymbolInfo.Symbol.Locations
                    .Where(location => location.SourceTree != null && location.SourceTree.FilePath != _tree.FilePath)
                    .Select(location => location.SourceTree!.FilePath);

                var noFullQualification = Constants.NO_FULL_QUALIFICATION_TYPES.Contains(namespaceName);
                var typeIsImported = usings.Any(usingDirective => usingDirective.Name != null && Utility.GetNamesFromNode(usingDirective).Any(name => namespaceName.StartsWith(name)));
                if (noFullQualification && namespaceName != "System")
                {
                    if (node.Expression is IdentifierNameSyntax identifier)
                    {
                        Visit(node.Name);
                    }
                    else if (node.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        if (namespaceName == Utility.RuntimeAssemblyName && GetName(memberAccess) != "Globals")
                        {
                            Visit(memberAccess.Name);
                            Write(".");
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

            if (objectSymbolInfo.Symbol?.OriginalDefinition is ILocalSymbol typeSymbol)
            {
                switch (typeSymbol.Type.Name)
                {
                    case "ValueTuple":
                        var name = GetName(node.Name);
                        if (name.StartsWith("Item"))
                        {
                            var itemIndex = name.Split("Item").Last();
                            Visit(node.Expression);
                            Write($"[{itemIndex}]");
                            return;
                        }
                        break;
                }
            }

            Visit(node.Expression);
            Write(".");
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

            var typeIsImported = usings.Any(usingDirective => usingDirective.Name != null && Utility.GetNamesFromNode(usingDirective).Contains(namespaceType.ContainingNamespace.Name));
            Write($"CS.getAssemblyType(\"{namespaceType.Name}\")");
            Write(".");
            _flags[CodeGenFlag.ShouldCallGetAssemblyType] = false;
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
                var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
                var parentNamespace = FindFirstAncestor<NamespaceDeclarationSyntax>(node);
                var parentNamespaceSymbol = parentNamespace != null ? _semanticModel.GetSymbolInfo(parentNamespace).Symbol : null;
                var pluginClassesNamespace = _runtimeLibNamespace.GetNamespaceMembers().FirstOrDefault(ns => ns.Name == "PluginClasses");
                var runtimeNamespaceIncludesIdentifier = symbol != null ? (
                    IsDescendantOfNamespaceSymbol(symbol, _runtimeLibNamespace)
                    || (pluginClassesNamespace != null && IsDescendantOfNamespaceSymbol(symbol, pluginClassesNamespace))
                ) : false;

                List<SyntaxKind> fullyQualifiedParentKinds = [SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.ObjectCreationExpression];
                if (
                    symbol != null
                        && symbol is ITypeSymbol typeSymbol
                        && node.Parent != null
                        && fullyQualifiedParentKinds.Contains(node.Parent.Kind())
                        && typeSymbol.ContainingNamespace != null
                        && (parentNamespace != null && Utility.GetNamesFromNode(parentNamespace.Name).LastOrDefault() != typeSymbol.ContainingNamespace.Name)
                        && !Constants.NO_FULL_QUALIFICATION_TYPES.Contains(typeSymbol.ContainingNamespace.Name)
                )
                {
                    var usings = GetUsings();
                    FullyQualifyMemberAccess(typeSymbol.ContainingNamespace, usings);
                }

                var parentAccessExpression = FindFirstAncestor<MemberAccessExpressionSyntax>(node);
                var isLeftSide = parentAccessExpression == null ? true : node == parentAccessExpression.Expression;
                var parentBlocks = GetAncestors<SyntaxNode>(node);
                var localScopeIncludesIdentifier = parentBlocks
                    .Any(block =>
                    {
                        var descendants = block.DescendantNodes();
                        var variableDeclarators = descendants.OfType<VariableDeclaratorSyntax>();
                        var forEachStatements = descendants.OfType<ForEachStatementSyntax>();
                        var forStatements = descendants.OfType<ForStatementSyntax>();
                        var parameters = descendants.OfType<ParameterSyntax>();
                        var checkNamePredicate = (SyntaxNode node) => GetName(node) == identifierName;
                        return variableDeclarators.Where(checkNamePredicate).Count() > 0
                            || parameters.Where(checkNamePredicate).Count() > 0
                            || forEachStatements.Where(checkNamePredicate).Count() > 0
                            || forStatements.Any(forStatement => forStatement.Initializers.Count() > 0);
                    });

                if (isLeftSide && !localScopeIncludesIdentifier && !runtimeNamespaceIncludesIdentifier)
                {
                    // TODO: check for inherited members
                    var namespaceSymbol = parentNamespace != null ? _semanticModel.GetDeclaredSymbol(parentNamespace) : null;
                    var namespaceIncludesIdentifier = namespaceSymbol != null && namespaceSymbol.GetMembers()
                        .Where(member => member.Name == identifierName)
                        .Count() > 0;

                    var parentClass = FindFirstAncestor<ClassDeclarationSyntax>(node);
                    var classMember= parentClass?.Members
                        .Where(member => GetName(member) == identifierName)
                        .FirstOrDefault();

                    if (namespaceIncludesIdentifier)
                    {
                        Write($"namespace[\"$getMember\"](namespace, \"{identifierName}\")");
                    }
                    else if (classMember != null)
                    {
                        Write($"{(HasSyntax(classMember.Modifiers, SyntaxKind.StaticKeyword) ? "class" : "self")}.{identifierName}");
                    }
                    else
                    {
                        if (_flags[CodeGenFlag.ShouldCallGetAssemblyType])
                        {
                            Write($"CS.getAssemblyType(\"{identifierName}\")");
                        }
                        else
                        {
                            Write(identifierName);
                            _flags[CodeGenFlag.ShouldCallGetAssemblyType] = true;
                        }
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
                case SyntaxKind.NullLiteralExpression:
                    Write("nil");
                    break;
            }

            base.VisitLiteralExpression(node);
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            Write(GetName(node));
        }

        public override void VisitParameterList(ParameterListSyntax node)
        {
            Write('(');
            foreach (var parameter in node.Parameters)
            {
                Visit(parameter);
                if (parameter != node.Parameters.Last())
                {
                    Write(", ");
                }
            }
            WriteLine(')');
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
            var allNames = Utility.GetNamesFromNode(node);
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
            WriteLine();
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var parentNamespace = FindFirstAncestor<NamespaceDeclarationSyntax>(node);
            var className = GetName(node);
            WriteLine($"{(parentNamespace != null ? "namespace:" : "CS.")}class(\"{className}\", function(namespace)");
            _indent++;

            WriteLine("local class = {}");
            WriteLine("class.__index = class");
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

        private void VisitConstructorBody(ClassDeclarationSyntax parentClass, BlockSyntax? block)
        {
            WriteLine("local self = setmetatable({}, class)");
            WriteLine();

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
                Visit(block);
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

            VisitConstructorBody(node, null);

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