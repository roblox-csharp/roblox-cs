using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using static RobloxCS.Constants;

namespace RobloxCS
{
    public static class Utility
    {
        public const string RuntimeAssemblyName = "Roblox";

        public static string GetDefaultValueForType(string typeName)
        {
            if (INTEGER_TYPES.Contains(typeName) || DECIMAL_TYPES.Contains(typeName))
            {
                return "0";
            }

            switch (typeName)
            {
                case "char":
                case "Char":
                case "string":
                case "String":
                    return "\"\"";
                case "bool":
                case "Boolean":
                    return "false";
                default:
                    return "nil";
            }
        }

        public static ISymbol? FindMember(INamespaceSymbol namespaceSymbol, string memberName)
        {
            var member = namespaceSymbol.GetMembers().FirstOrDefault<ISymbol?>(member => member?.Name == memberName, null);
            if (member == null && namespaceSymbol.ContainingNamespace != null)
            {
                member = FindMember(namespaceSymbol.ContainingNamespace, memberName);
            }
            return member;
        }

        public static ISymbol? FindMemberDeep(INamedTypeSymbol namedTypeSymbol, string memberName)
        {
            var member = namedTypeSymbol.GetMembers().FirstOrDefault(member => member.Name == memberName);
            if (namedTypeSymbol.BaseType != null && member == null)
            {
                return FindMemberDeep(namedTypeSymbol.BaseType, memberName);
            }
            return member;
        }

        public static void PrettyPrint(object? obj)
        {
            if (obj == null)
            {
                Console.WriteLine("null");
                return;
            }

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = $"{type.Name}:\n";

            foreach (var property in properties)
            {
                var value = property.GetValue(obj, null);
                result += $"  {property.Name}: {value}\n";
            }

            Console.WriteLine(result);
        }

        public static List<T> FilterDuplicates<T>(IEnumerable<T> items, IEqualityComparer<T> comparer) where T : notnull
        {
            var seen = new Dictionary<T, bool>(comparer);
            var result = new List<T>();
            foreach (var item in items)
            {
                if (!seen.ContainsKey(item))
                {
                    seen[item] = true;
                    result.Add(item);
                }
            }

            return result;
        }
    }
}