namespace RobloxCS.Luau
{
    public class Literal : Expression
    {
        public string ValueText { get; }

        public Literal(string valueText)
        {
            ValueText = valueText;
        }

        public override void Render(LuauWriter luau)
        {
            luau.Write(ValueText);
        }
    }
}