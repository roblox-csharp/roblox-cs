using System.Linq;

namespace RobloxCS.Luau
{
    public class InterfaceType : TypeRef
    {
        public HashSet<FieldType> Fields { get; }
        public MappedType? ExtraMapping { get; }
        public bool IsCompact { get; }

        public InterfaceType(HashSet<FieldType> fields, MappedType? extraMapping = null, bool? isCompact = null) : base("", true)
        {
            Fields = fields;
            ExtraMapping = extraMapping;
            IsCompact = Fields.Count == 0 && isCompact == null ? true : (isCompact ?? false);
            Path = ToString();
        }

        public string ToString(uint indent = 0)
        {
            var tabsOutside = new string(' ', (int)indent * BaseWriter.IndentSize);
            var tabsInside = new string(' ', ((int)indent + 1) * BaseWriter.IndentSize);
            var newline = (IsCompact ? "" : "\n");
            return tabsOutside + "{" + newline
                + string.Join(";" + newline, Fields.Select(field => tabsInside + field.Path))
                + tabsOutside + newline + "}";
        }
    }
}