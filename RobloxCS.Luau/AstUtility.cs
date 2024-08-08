using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static RobloxCS.Luau.Constants;

namespace RobloxCS.Luau
{
    public static class AstUtility
    {
        /// <summary>file path -> dictionary(identifier name, amount of times identifier is used)</summary>
        private static Dictionary<string, Dictionary<string, uint>> _identifierDeclarations = [];

        public static Call DefineGlobal(Name name, Expression value)
        {
            return CSCall("defineGlobal", new Literal($"\"{name}\""), value);
        }

        public static Call GetGlobal(Name name)
        {
            return CSCall("getGlobal", new Literal($"\"{name}\""));
        }

        public static Call CSCall(string methodName, params Expression[] arguments)
        {
            return new Call(
                new MemberAccess(
                    new IdentifierName("CS"),
                    new IdentifierName(methodName)
                ),
                CreateArgumentList(arguments.ToList())
            );
        }

        public static Call Bit32Call(string methodName, params Expression[] arguments)
        {
            return new Call(
                new MemberAccess(
                    new IdentifierName("bit32"),
                    new IdentifierName(methodName)
                ),
                CreateArgumentList(arguments.ToList())
            );
        }

        public static ArgumentList CreateArgumentList(List<Expression> arguments)
        {
            return new ArgumentList(arguments.ConvertAll(expression => new Argument(expression)));
        }

        public static Expression? GetFullParentName(SyntaxNode node)
        {
            if (node.Parent == null) return null;

            var parentLocation = GetFullParentName(node.Parent);
            switch (node.Parent)
            {
                case CompilationUnitSyntax:
                    return null;
            }

            var parentName = CreateIdentifierName(node.Parent);
            return parentLocation == null ?
                (
                    node.Parent.SyntaxTree == node.SyntaxTree ?
                        parentName
                        : CSCall("getGlobal", new Literal($"\"{parentName.Text}\""))
                )
                : new MemberAccess(parentLocation, parentName);
        }

        public static If Initializer(Name name, Expression initializer)
        {
            return new If(
                new BinaryOperator(name, "==", Nil()),
                new ExpressionStatement(new Assignment(name, initializer)),
                null
            );
        }

        public static QualifiedName QualifiedNameFromMemberAccess(MemberAccess memberAccess)
        {
            var left = memberAccess.Expression is MemberAccess leftMemberAccess ?
                QualifiedNameFromMemberAccess(leftMemberAccess)
                : (Name)memberAccess.Expression;

            return new QualifiedName(left, memberAccess.Name);
        }

        public static Node DiscardVariableIfExpressionStatement(SyntaxNode node, Node value, SyntaxNode? valueParent)
        {
            if (valueParent is ExpressionStatementSyntax)
            {
                return DiscardVariable(node, (Expression)value);
            }
            return value;
        }

        public static Variable DiscardVariable(SyntaxNode node, Expression value)
        {
            return new Variable(CreateIdentifierName(node, "_"), true, value);
        }

        public static IdentifierName CreateIdentifierName(SyntaxNode node)
        {
            return CreateIdentifierName(node, Utility.GetNamesFromNode(node).First());
        }

        public static IdentifierName CreateIdentifierName(SyntaxNode node, string name, bool bypassReserved = false)
        {
            if (RESERVED_IDENTIFIERS.Contains(name) && !bypassReserved)
            {
                Logger.UnsupportedError(node, $"Using '{name}' as an identifier", useIs: true, useYet: false);
            }

            return new IdentifierName(name);
        }

        // TODO: reference the correct identifier names
        public static string FixIdentifierNameText(SyntaxNode node, string name)
        {
            if (!_identifierDeclarations.ContainsKey(node.SyntaxTree.FilePath))
            {
                _identifierDeclarations[node.SyntaxTree.FilePath] = [];
            }
            if (!_identifierDeclarations[node.SyntaxTree.FilePath].ContainsKey(name))
            {
                _identifierDeclarations[node.SyntaxTree.FilePath][name] = 0;
            }

            var useCount = _identifierDeclarations[node.SyntaxTree.FilePath][name]++;
            if (useCount > 0)
            {
                if (!name.EndsWith('_'))
                {
                    name += '_';
                }
                name += useCount;
            }

            return name;
        }

        public static TypeRef? CreateTypeRef(TypeSyntax? type)
        {
            if (type == null) return null;
            if (type.ToString() == "var") return null;
            return new(Utility.GetMappedType(type.ToString()));
        }

        public static Literal Vararg()
        {
            return new Literal("...");
        }

        public static Literal False()
        {
            return new Literal("false");
        }

        public static Literal True()
        {
            return new Literal("true");
        }

        public static Literal Nil()
        {
            return new Literal("nil");
        }
    }
}