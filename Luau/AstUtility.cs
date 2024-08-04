using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static RobloxCS.Luau.Constants;

namespace RobloxCS
{
    public static class AstUtility
    {
        public static Luau.If Initializer(Luau.Name name, Luau.Expression initializer)
        {
            return new Luau.If(
                new Luau.BinaryOperator(name, "==", Nil()),
                new Luau.ExpressionStatement(new Luau.Assignment(name, initializer)),
                null
            );
        }

        public static Luau.Call Bit32Call(string methodName, params Luau.Expression[] arguments)
        {
            return new Luau.Call(
                new Luau.MemberAccess(
                    new Luau.IdentifierName("bit32"),
                    new Luau.IdentifierName(methodName)
                ),
                new Luau.ArgumentList(arguments.ToList().ConvertAll(expression => new Luau.Argument(expression)))
            );
        }

        public static Luau.QualifiedName QualifiedNameFromMemberAccess(Luau.MemberAccess memberAccess)
        {
            Luau.Name left;
            if (memberAccess.Expression is Luau.MemberAccess leftMemberAccess)
            {
                left = QualifiedNameFromMemberAccess(leftMemberAccess);
            }
            else
            {
                left = (Luau.Name)memberAccess.Expression;
            }
            return new Luau.QualifiedName(left, memberAccess.Name);
        }

        public static Luau.Node DiscardVariableIfExpressionStatement(Luau.Node value, SyntaxNode? valueParent)
        {
            if (valueParent is ExpressionStatementSyntax)
            {
                return DiscardVariable((Luau.Expression)value);
            }
            return value;
        }

        public static Luau.Variable DiscardVariable(Luau.Expression value)
        {
            return new Luau.Variable(CreateIdentifierName("_"), true, value);
        }

        public static Luau.IdentifierName CreateIdentifierName(SyntaxNode node)
        {
            return CreateIdentifierName(Utility.GetNamesFromNode(node).First());
        }

        public static Luau.IdentifierName CreateIdentifierName(string name)
        {
            if (RESERVED_IDENTIFIERS.Contains(name))
            {
                // TODO: throw 
            }
            return new Luau.IdentifierName(name);
        }

        public static Luau.TypeRef? CreateTypeRef(TypeSyntax? type)
        {
            if (type == null) return null;
            if (type.ToString() == "var") return null;
            return new(Luau.Utility.GetMappedType(type.ToString()));
        }

        public static Luau.Literal Vararg()
        {
            return new Luau.Literal("...");
        }

        public static Luau.Literal False()
        {
            return new Luau.Literal("false");
        }

        public static Luau.Literal True()
        {
            return new Luau.Literal("true");
        }

        public static Luau.Literal Nil()
        {
            return new Luau.Literal("nil");
        }
    }
}
