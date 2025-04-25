namespace RobloxCS.Luau;

public class InterfaceType : TypeRef
{
    public HashSet<FieldType> Fields { get; }
    public MappedType? ExtraMapping { get; }
    public bool IsCompact { get; }

    public InterfaceType(HashSet<FieldType> fields, MappedType? extraMapping = null, bool isCompact = true)
        : base("", true)
    {
        Fields = fields;
        ExtraMapping = extraMapping;
        IsCompact = Fields.Count == 0 && isCompact;
        Path = ToString();
    }

    public string ToString(int indent = 0)
    {
        var tabsOutside = new string(' ', indent * BaseWriter.IndentSize);
        var tabsInside = new string(' ', (indent + 1) * BaseWriter.IndentSize);
        var newline = IsCompact ? "" : "\n";

        return tabsOutside
             + "{"
             + newline
             + string.Join(newline, Fields.Select(field => tabsInside + field.Path))
             + tabsOutside
             + newline
             + "}";
    }
}