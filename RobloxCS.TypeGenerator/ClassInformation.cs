namespace TypeGenerator.ClassInformation
{
#pragma warning disable CS8618

    class Summarized
    {
        public string Summary { get; }
    }

    class Argument : Summarized
    {
        public string Name { get; }
    }

    internal sealed class ExampleCode
    {
        public string DisplayTitle { get; }
        public string CodeSummary { get; }
        public string CodeSample { get; }
    }

    internal class Member
    {
        public string Title { get; }
        public string Description { get; }
        public ExampleCode[]? CodeSample { get; }
    }

    sealed class Property : Member
    {
    }

    sealed class Event : Member
    {
    }

    sealed class Method : Member
    {
    }

    sealed class Callback : Member
    {
    }

    internal sealed class ClassDescription : Member
    {
        public Property[] Property { get; }
        public Method[] Function { get; }
        public Event[] Event { get; }
        public Callback[] Callback { get; }
    }
#pragma warning restore CS8618
}