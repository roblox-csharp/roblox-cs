using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TypeGenerator
{
    internal class ReflectionMetadataReader
    {
        private readonly XElement _metadata;

        public ReflectionMetadataReader(string body)
        {
            _metadata = XElement.Parse(body);
        }

        public string ReadClassDesc(string name)
        {
            return Get($"{ClassPrefix(name)}Properties/string[@name='summary']");
        }

        public string ReadMemberDesc(string className, string name, string[] specifier)
        {
            string specifierString = string.Join(" or ", specifier.Select(v => $"@class='{v}'"));
            string query = $"{ClassPrefix(className)}Item[{specifierString}]/" +
                           "Item[@class='ReflectionMetadataMember']/" +
                           "Properties/" +
                           $"string[@name='Name'][text()='{name}']" +
                           "/../string[@name='summary']";

            return Get(query);
        }

        public string ReadPropDesc(string className, string name)
        {
            return ReadMemberDesc(className, name, ["ReflectionMetadataProperties"]);
        }

        public string ReadMethodDesc(string className, string name)
        {
            return ReadMemberDesc(className, name, ["ReflectionMetadataFunctions", "ReflectionMetadataYieldFunctions"]);
        }

        public string ReadCallbackDesc(string className, string name)
        {
            return ReadMemberDesc(className, name, ["ReflectionMetadataCallbacks"]);
        }

        public string ReadEventDesc(string className, string name)
        {
            return ReadMemberDesc(className, name, ["ReflectionMetadataEvents"]);
        }

        private string ClassPrefix(string name)
        {
            return $"//Item[@class='ReflectionMetadataClass']/Properties/string[@name='Name'][text()='{name}']/../../";
        }

        private string Get(string query)
        {
            var result = _metadata.XPathSelectElement(query);
            return result != null ? Filter(result.ToString()) : null!;
        }

        private string Filter(string s)
        {
            return Regex.Replace(s, "<a href=\"([^\"]+)\"[^>]+>([^<]+)</a>", m => $"[{m.Groups[2].Value}]({m.Groups[1].Value})");
        }
    }
}