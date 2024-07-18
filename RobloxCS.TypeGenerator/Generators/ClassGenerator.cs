using System.Text.Json;

namespace TypeGenerator.Generators
{
    static class Utility
    {
        public static bool ContainsBadChar(string name)
        {
            foreach (var badChar in Constants.BAD_NAME_CHARS)
            {
                if (name.Contains(badChar))
                    return true;
            }
            return false;
        }

        public static string SafeName(string name)
        {
            return ContainsBadChar(name) ? $"[\"{name.Replace("\"", "\\\"")}\"]" : name;
        }

        public static string? SafePropType(string? valueType)
        {
            return string.IsNullOrEmpty(valueType) ? null : Constants.PROP_TYPE_MAP.ContainsKey(valueType) ? Constants.PROP_TYPE_MAP[valueType] : valueType;
        }

        public static string? SafeRenamedInstance(string? name)
        {
            return name != null && Constants.RENAMEABLE_AUTO_TYPES.ContainsKey(name) ? Constants.RENAMEABLE_AUTO_TYPES[name] : name;
        }

        public static string? SafeValueType(APITypes.ValueType valueType)
        {
            if (valueType.Category == "Enum")
                return $"Enum.{valueType.Name}";

            if (!string.IsNullOrEmpty(valueType.Name) && valueType.Name.EndsWith('?'))
            {
                var nonOptionalType = valueType.Name.Substring(0, valueType.Name.Length - 1);
                var mappedType = Constants.VALUE_TYPE_MAP.ContainsKey(nonOptionalType) ? Constants.VALUE_TYPE_MAP[nonOptionalType] : nonOptionalType;
                return $"{mappedType}?";
            }

            return Constants.VALUE_TYPE_MAP.ContainsKey(valueType.Name) ? Constants.VALUE_TYPE_MAP[valueType.Name] : valueType.Name;
        }

        public static string? SafeReturnType(string? valueType)
        {
            return string.IsNullOrEmpty(valueType) ? null : Constants.RETURN_TYPE_MAP.ContainsKey(valueType) ? Constants.RETURN_TYPE_MAP[valueType] : valueType;
        }

        public static string? SafeArgName(string name)
        {
            return name != null && Constants.ARG_NAME_MAP.ContainsKey(name) ? Constants.ARG_NAME_MAP[name] : name;
        }

        public static APITypes.Security GetSecurity(string className, APITypes.MemberBase member)
        {
            Dictionary<string, APITypes.Security> classSecurity;
            if (Constants.SECURITY_OVERRIDES.TryGetValue(className, out classSecurity!))
            {
                APITypes.Security securityOverride;
                if (classSecurity.TryGetValue(member.Name, out securityOverride!))
                {
                    return securityOverride;
                }
            }

            switch (member.MemberType)
            {
                case "Callback":
                    return new APITypes.Security
                    {
                        Read = "NotAccessibleSecurity",
                        Write = member.Security?.ToString()!
                    };
                case "Function":
                    return new APITypes.Security
                    {
                        Read = member.Security?.ToString()!,
                        Write = "NotAccessibleSecurity"
                    };
                case "Event":
                    return new APITypes.Security
                    {
                        Read = member.Security?.ToString()!,
                        Write = "NotAccessibleSecurity"
                    };
                case "Property":
                    return JsonSerializer.Deserialize<APITypes.Security>(member.Security?.ToString()!)!;
                default:
                    throw new NotSupportedException($"Member type not supported: {member.MemberType}");
            }
        }

        public static bool HasTag(APITypes.MemberBase container, string tag)
        {
            return container.Tags != null && container.Tags.ConvertAll(tag => tag.ToString()).Contains(tag);
        }

        public static bool HasTag(APITypes.Class container, string tag)
        {
            return container.Tags != null && container.Tags.ConvertAll(tag => tag.ToString()).Contains(tag);
        }

        public static bool IsCreatable(APITypes.Class rbxClass)
        {
            return !Constants.CREATABLE_BLACKLIST.Contains(rbxClass.Name) &&
                !HasTag(rbxClass, "NotCreatable") &&
                !HasTag(rbxClass, "Service");
        }

        public static string FormatComment(string s)
        {
            return string.Join('\n', s.Trim().Split('\n').Select(d => $"# {d}"));
        }

        public static List<List<T>> Multifilter<T>(List<T> list, int resultArrAmount, Func<T, int> condition)
        {
            var results = new List<List<T>>();
            for (int i = 0; i < resultArrAmount; i++)
            {
                results.Add(new List<T>());
            }

            foreach (var element in list)
            {
                results[condition(element)].Add(element);
            }

            return results;
        }
    }

    internal sealed class ClassGenerator : Generator
    {
        private readonly Dictionary<string, APITypes.Class> _classRefs = new Dictionary<string, APITypes.Class>();
        private readonly ReflectionMetadataReader _metadata;
        private readonly HashSet<string> _definedClassNames;
        private readonly Dictionary<string, HashSet<string>> _definedMemberNames = new Dictionary<string, HashSet<string>>();
        private readonly string _security;
        private readonly string? _lowerSecurity;

        public ClassGenerator(string filePath, ReflectionMetadataReader metadata, HashSet<string> definedClassNames, string security, string? lowerSecurity = null)
            : base(filePath, metadata)
        {
            _metadata = metadata;
            _definedClassNames = definedClassNames;
            _security = security;
            _lowerSecurity = lowerSecurity;
        }

        public void Generate(List<APITypes.Class> rbxClasses)
        {
            foreach (var rbxClass in rbxClasses)
            {
                var className = rbxClass.Name;
                rbxClass.Subclasses = [];
                _classRefs[className] = rbxClass;

                var superclass = rbxClass.Superclass != Constants.ROOT_CLASS_NAME ? _classRefs[rbxClass.Superclass] : null;
                if (superclass != null && superclass.Subclasses != null)
                {
                    superclass.Subclasses.Add(className);
                }
            }

            var classesToGenerate = rbxClasses.Where(ShouldGenerateClass).ToList();
            GenerateHeader();
            Write($"namespace Roblox{(_security == "PluginSecurity" ? ".PluginClasses" : "")}");
            Write("{");
            PushIndent();

            GenerateServices(rbxClasses.Where(rbxClass => !_definedClassNames.Contains(rbxClass.Name)).ToList());
            GenerateClasses(classesToGenerate);

            PopIndent();
            Write("}");

            WriteFile();
        }

        private bool CanRead(string className, APITypes.MemberBase member)
        {
            var readSecurity = Utility.GetSecurity(className, member).Read;
            return readSecurity == _security ||
                (Constants.PLUGIN_ONLY_CLASSES.Contains(className) && readSecurity == _lowerSecurity);
        }

        private bool CanWrite(string className, APITypes.MemberBase member)
        {
            var security = Utility.GetSecurity(className, member);

            // dumb hack to fix PluginSecurity writable things being marked as readonly in None.cs
            if (security.Read == "None" && security.Write == "PluginSecurity")
            {
                return true;
            }

            return security.Write == _security ||
                (Constants.PLUGIN_ONLY_CLASSES.Contains(className) && security.Write == _lowerSecurity);
        }

        private bool IsPluginOnlyClass(APITypes.Class rbxClass)
        {
            if (Constants.PLUGIN_ONLY_CLASSES.Contains(rbxClass.Name))
            {
                return true;
            }
            else
            {
                var superClass = rbxClass.Superclass != Constants.ROOT_CLASS_NAME ? _classRefs[rbxClass.Superclass] : null;
                return superClass != null ? IsPluginOnlyClass(superClass) : false;
            }
        }

        private bool ShouldGenerateClass(APITypes.Class rbxClass)
        {
            var superClass = rbxClass.Superclass != Constants.ROOT_CLASS_NAME ? _classRefs[rbxClass.Superclass] : null;
            if (superClass != null && !ShouldGenerateClass(superClass))
            {
                return false;
            }
            if (Constants.CLASS_BLACKLIST.Contains(rbxClass.Name))
            {
                return false;
            }
            if (_security != "PluginSecurity" && Constants.PLUGIN_ONLY_CLASSES.Contains(rbxClass.Name))
            {
                return false;
            }
            return true;
        }

        private bool ShouldGenerateMember(APITypes.Class rbxClass, APITypes.MemberBase member)
        {
            if (member.Name == null)
                return false;

            if (Constants.MEMBER_BLACKLIST.ContainsKey(rbxClass.Name) &&
                Constants.MEMBER_BLACKLIST[rbxClass.Name].Contains(member.Name))
            {
                return false;
            }

            if (!CanRead(rbxClass.Name, member))
                return false;

            if (Utility.HasTag(member, "Deprecated"))
            {
                var firstChar = member.Name[0];
                if (char.IsLower(firstChar))
                {
                    var pascalCaseName = char.ToUpper(firstChar) + member.Name.Substring(1);
                    var pascalCaseMember = rbxClass.Members.FirstOrDefault(v => v.Name == pascalCaseName);
                    if (pascalCaseMember != null)
                        return false;
                }
            }

            if (Utility.HasTag(member, "Hidden"))
                return false;

            if (Utility.HasTag(member, "NotScriptable"))
                return false;

            if (_definedMemberNames[rbxClass.Name].Contains(member.Name.Trim()))
                return false;

            return true;
        }

        // for writing documentation
        private void WriteDescription(ClassInformation.Member member, string description)
        {
            // Implementation for writing description
        }

        private int ByName(APITypes.Class a, APITypes.Class b)
        {
            return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        }

        // Returns the given className if it's in ClassRefs
        // Throws if not
        private string AssertClassName(string className)
        {
            if (_classRefs.ContainsKey(className))
            {
                return className;
            }
            else
            {
                throw new Exception($"Undefined class name: {className}");
            }
        }

        private void GenerateClass(APITypes.Class rbxClass)
        {
            _definedClassNames.Add(rbxClass.Name);
            _definedMemberNames[rbxClass.Name] = new HashSet<string>();

            var className = AssertClassName(rbxClass.Name);
            var members = rbxClass.Members;
            var noSecurity = _security == "None" || IsPluginOnlyClass(rbxClass);
            if (noSecurity)
            {
                var desc = rbxClass.Description;
                if (desc != null)
                {
                    Write(Utility.FormatComment(desc));
                }
            }

            if (!noSecurity && members.Count == 0) return;
            if (className == "Studio") return;

            var superclasses = new List<string>();
            if (Utility.IsCreatable(rbxClass))
            {
                if (rbxClass.Superclass != "Instance")
                {
                    superclasses.Add(rbxClass.Superclass);
                }
                superclasses.Add("ICreatableInstance");
            }
            else if (Utility.HasTag(rbxClass, "Service"))
            {
                if (rbxClass.Superclass != "Instance")
                {
                    superclasses.Add(rbxClass.Superclass);
                }
                superclasses.Add("IServiceInstance");
            }
            else
            {
                superclasses.Add(rbxClass.Superclass);
            }

            var isPartial = Constants.PARTIAL_INTERFACES.Contains(className);
            Write($"public{(isPartial ? " partial" : "")} interface {className}{(rbxClass.Superclass != Constants.ROOT_CLASS_NAME ? $" : {string.Join(", ", superclasses)}" : "")}");
            Write("{");
            PushIndent();

            foreach (var member in members)
            {
                if (!ShouldGenerateMember(rbxClass, member)) continue;

                _definedMemberNames[rbxClass.Name].Add(member.Name.Trim());
                switch (member.MemberType)
                {
                    case "Callback":
                        GenerateCallback((APITypes.Callback)member, rbxClass);
                        break;
                    case "Event":
                        GenerateEvent((APITypes.Event)member, rbxClass);
                        break;
                    case "Function":
                        GenerateFunction((APITypes.Function)member, rbxClass);
                        break;
                    case "Property":
                        GenerateProperty((APITypes.Property)member, rbxClass);
                        break;
                    default:
                        throw new NotSupportedException($"Received unsupported member type: {member.MemberType}");
                }
            }

            PopIndent();
            Write("}");
            Write();
        }

        private List<string> GetParamNames(List<APITypes.Parameter> parameters)
        {
            var paramNames = parameters.ConvertAll(param => param.Name);
            for (int i = 0; i < paramNames.Count; i++)
            {
                if (paramNames.IndexOf(paramNames[i]) == i + 1)
                {
                    int n = 0;
                    for (int j = i; j < parameters.Count; j++)
                    {
                        paramNames[j] = $"{paramNames[i]}{n}";
                        n++;
                    }
                }
            }
            return paramNames;
        }

        private List<string> GetParamTypes(List<APITypes.Parameter> parameters)
        {
            return parameters.ConvertAll(param => Utility.SafeValueType(param.Type) ?? "null");
        }

        private string GenerateArgs(List<APITypes.Parameter> parameters)
        {
            var args = new List<string>();
            var paramNames = GetParamNames(parameters);
            bool optional = false;

            foreach (var param in parameters)
            {
                var paramType = Utility.SafeValueType(param.Type);
                var argName = Utility.SafeArgName(paramNames[parameters.IndexOf(param)]);
                optional |= !string.IsNullOrEmpty(param.Default) || paramType == "any";

                if (!string.IsNullOrEmpty(argName) && paramType == "Instance")
                {
                    var findings = _classRefs.Keys.Concat(new string[] { "Character", "Input" })
                        .Where(k => k != "Instance" && argName.ToLower().Contains(k.ToLower())).ToList();

                    if (findings.Count != 0)
                    {
                        var partPos = findings.IndexOf("Part");
                        var doSplice = !findings.Contains("Part") && findings.Count != 0 && !argName.ToLower().Contains("or");
                        if (doSplice && partPos != -1)
                        {
                            findings.RemoveAt(partPos);
                        }
                        paramType = Utility.SafeRenamedInstance(findings.FirstOrDefault(found => found.ToLower() == argName.ToLower())) ?? "Instance";
                    }
                }

                var isOptional = optional || (paramType != null && paramType.EndsWith("?"));
                args.Add($"{(!string.IsNullOrEmpty(paramType) ? $"{paramType}{(optional && !paramType.EndsWith("?") ? "?" : "")}" : "object")} {argName ?? $"arg{parameters.IndexOf(param)}"}{(isOptional ? " = null" : "")}");
            }
            return string.Join(", ", args);
        }

        private void GenerateCallback(APITypes.Callback callback, APITypes.Class rbxClass)
        {
            var paramTypeList = callback.Parameters.Count > 0 ?
                $"<{string.Join(", ", GetParamTypes(callback.Parameters))}>"
                : "";

            var description = !string.IsNullOrWhiteSpace(callback.Description) ?
                callback.Description :
                _metadata.ReadCallbackDesc(rbxClass.Name, callback.Name);

            Write($"public Action{paramTypeList} {callback.Name} {{ get; set; }}");
        }

        private void GenerateEvent(APITypes.Event @event, APITypes.Class rbxClass)
        {
            var paramTypeList = @event.Parameters.Count > 0 ?
                $"<{string.Join(", ", GetParamTypes(@event.Parameters))}>"
                : "";

            var description = !string.IsNullOrWhiteSpace(@event.Description) ?
                @event.Description :
                _metadata.ReadEventDesc(rbxClass.Name, @event.Name);

            Write($"public ScriptSignal{paramTypeList} {@event.Name} {{ get; }}");
        }

        private void GenerateFunction(APITypes.Function function, APITypes.Class rbxClass)
        {
            var args = GenerateArgs(function.Parameters);
            var returnType = Utility.SafeReturnType(Utility.SafeValueType(function.ReturnType));
            var description = !string.IsNullOrWhiteSpace(function.Description) ?
                function.Description :
                _metadata.ReadMethodDesc(rbxClass.Name, function.Name);

            Write($"public {returnType} {function.Name}({args});");
        }

        private void GenerateProperty(APITypes.Property property, APITypes.Class rbxClass)
        {
            var valueType = Utility.SafePropType(Utility.SafeValueType(property.ValueType))!;
            var description = !string.IsNullOrWhiteSpace(property.Description) ?
                property.Description :
                _metadata.ReadPropDesc(rbxClass.Name, property.Name);

            var definitelyDefined = property.ValueType.Category != "Class";
            var extraPropertyData = CanWrite(rbxClass.Name, property) && !Utility.HasTag(property, "ReadOnly") ? " set;" : "";
            Write($"public {valueType}{(definitelyDefined || valueType.EndsWith("?") ? "" : "?")} {property.Name.Replace(" ", "")} {{ get;{extraPropertyData} }}");
        }

        private void GenerateServices(List<APITypes.Class> rbxClasses)
        {
            var services = rbxClasses.Where(rbxClass =>
            {
                var isPluginOnly = Constants.PLUGIN_ONLY_CLASSES.Contains(rbxClass.Name);
                return Utility.HasTag(rbxClass, "Service")
                    && !Utility.HasTag(rbxClass, "Hidden")
                    && !Constants.CLASS_BLACKLIST.Contains(rbxClass.Name)
                    && (isPluginOnly ? _security == "PluginSecurity" : _security == "None");
            });
            Write("public static class Services");
            Write("{");
            PushIndent();

            foreach (var service in services)
            {
                Write($"public static {service.Name} {service.Name} {{ get; }} = null!;");
            }

            PopIndent();
            Write("}");
            Write();
        }

        private void GenerateClasses(List<APITypes.Class> rbxClasses)
        {
            Write("// GENERATED ROBLOX INSTANCE CLASSES");
            Write();

            foreach (var rbxClass in rbxClasses)
            {
                GenerateClass(rbxClass);
            }
        }

        private void GenerateHeader()
        {
            Write("// THIS FILE IS AUTOMATICALLY GENERATED AND SHOULD NOT BE EDITED MANUALLY!");
            Write();
        }
    }
}