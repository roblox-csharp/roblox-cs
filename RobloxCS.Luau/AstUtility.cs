using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static RobloxCS.Luau.Constants;

namespace RobloxCS.Luau
{
    public static class AstUtility
    {
        public static If Initializer(Name name, Expression initializer)
        {
            return new If(
                new BinaryOperator(name, "==", Nil()),
                new ExpressionStatement(new Assignment(name, initializer)),
                null
            );
        }

        public static Call Bit32Call(string methodName, params Expression[] arguments)
        {
            return new Call(
                new MemberAccess(
                    new IdentifierName("bit32"),
                    new IdentifierName(methodName)
                ),
                new ArgumentList(arguments.ToList().ConvertAll(expression => new Argument(expression)))
            );
        }

        public static QualifiedName QualifiedNameFromMemberAccess(MemberAccess memberAccess)
        {
            Name left;
            if (memberAccess.Expression is MemberAccess leftMemberAccess)
            {
                left = QualifiedNameFromMemberAccess(leftMemberAccess);
            }
            else
            {
                left = (Name)memberAccess.Expression;
            }
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

        public static IdentifierName CreateIdentifierName(SyntaxNode node, string name)
        {
            if (RESERVED_IDENTIFIERS.Contains(name))
            {
                Logger.UnsupportedError(node, $"Using '{name}' as an identifier", useIs: true, useYet: false);
            }
            return new IdentifierName(name);
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