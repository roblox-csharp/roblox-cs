using System.Xml.Linq;

namespace RobloxCS.Luau
{
    public sealed class Assignment : Expression
    {
        public Name Name { get; }
        public Expression Value;

        public Assignment(Name name, Expression value)
        {
            Name = name;
            Value = value;
            AddChildren([Name, Value]);
        }

        public override void Render(LuauWriter luau)
        {
            Node value = Value;
            luau.WriteDescendantStatements(ref value);
            Value = (Expression)value;
            luau.WriteAssignment(Name, Value);
        }
    }
}