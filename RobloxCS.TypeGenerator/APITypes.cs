using System.Text.Json.Serialization;
using System.Text.Json;

namespace TypeGenerator.APITypes
{
#pragma warning disable CS8618
    internal sealed class Dump
    {
        public Class[] Classes { get; set; }
        public Enum[] Enums { get; set; }
        public float Version { get; set; }
    }

    internal sealed class Enum
    {
        public string Name { get; set; }
        public EnumItem[] Items { get; set; }
    }

    internal sealed class EnumItem
    {
        public string[]? LegacyNames { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    internal sealed class ClassTags
    {
        public string PreferredDescriptorName { get; set; }
        public string ThreadSafety { get; set; }
    }

    [JsonConverter(typeof(MemberConverter))]
    internal abstract class MemberBase
    {
        public string MemberType { get; set; }
        public string Name { get; set; }
        public object? Security { get; set; } // string or Security
        public List<object> Tags { get; set; }
        public string Description { get; set; }
    }

    internal class Callback : MemberBase
    {
        public List<Parameter> Parameters { get; set; }
    }

    internal sealed class Event : Callback
    {
    }

    internal sealed class Function : Callback
    {
        public ValueType ReturnType { get; set; }
    }

    internal sealed class Property : MemberBase
    {
        public string Category { get; set; }
        public string Default { get; set; }
        public Serialization Serialization { get; set; }
        public string ThreadSafety { get; set; }
        public ValueType ValueType { get; set; }
    }

    internal sealed class Class
    {
        public List<MemberBase> Members { get; set; }
        public string MemberCategory { get; set; }
        public List<object> Tags { get; set; }
        public string ThreadSafety { get; set; }
        public string Name { get; set; }
        public string Superclass { get; set; }
        public List<string> Subclasses { get; set; }
        public string Description { get; set; }
    }

    internal sealed class Serialization
    {
        public bool CanLoad { get; set; }
        public bool CanSave { get; set; }
    }

    internal sealed class ValueType
    {
        public string Category { get; set; }
        public string Name { get; set; }
    }

    internal sealed class Security
    {
        public string Read { get; set; }
        public string Write { get; set; }
    }

    internal sealed class Parameter
    {
        public string Name { get; set; }
        public ValueType Type { get; set; }
        public string Default { get; set; }
    }
#pragma warning restore CS8618

    internal sealed class MemberConverter : JsonConverter<MemberBase>
    {
        public override MemberBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                var memberType = root.GetProperty("MemberType").GetString();
                MemberBase member = memberType switch
                {
                    "Callback" => new Callback(),
                    "Event" => new Event(),
                    "Function" => new Function(),
                    "Property" => new Property(),
                    _ => throw new NotSupportedException($"MemberType '{memberType}' is not supported.")
                };

                foreach (var property in root.EnumerateObject())
                {
                    switch (property.Name)
                    {
#pragma warning disable CS8601
                        case "MemberType":
                            member.MemberType = property.Value.GetString();
                            break;
                        case "Name":
                            member.Name = property.Value.GetString();
                            break;
                        case "Security":
                            member.Security = JsonSerializer.Deserialize<object?>(property.Value.GetRawText(), options);
                            break;
                        case "Tags":
                            member.Tags = JsonSerializer.Deserialize<List<object>>(property.Value.GetRawText(), options);
                            break;
                        case "Description":
                            member.Description = property.Value.GetString();
                            break;
                        case "Parameters":
                            if (member is Callback callback)
                            {
                                callback.Parameters = JsonSerializer.Deserialize<List<Parameter>>(property.Value.GetRawText(), options);
                            }
                            break;
                        case "ReturnType":
                            if (member is Function function)
                            {
                                var rawJson = property.Value.GetRawText();
                                var type = rawJson.StartsWith("[") ?
                                    JsonSerializer.Deserialize<List<ValueType>>(rawJson, options)?.First()
                                    : JsonSerializer.Deserialize<ValueType>(rawJson, options);

                                function.ReturnType = type;
                            }
                            break;
                        case "Category":
                            if (member is Property prop)
                            {
                                prop.Category = property.Value.GetString();
                            }
                            break;
                        case "Default":
                            if (member is Property propDefault)
                            {
                                propDefault.Default = property.Value.GetString();
                            }
                            break;
                        case "Serialization":
                            if (member is Property propSerialization)
                            {
                                propSerialization.Serialization = JsonSerializer.Deserialize<Serialization>(property.Value.GetRawText(), options);
                            }
                            break;
                        case "ThreadSafety":
                            if (member is Property propThreadSafety)
                            {
                                propThreadSafety.ThreadSafety = property.Value.GetString();
                            }
                            break;
                        case "ValueType":
                            if (member is Property propValueType)
                            {
                                propValueType.ValueType = JsonSerializer.Deserialize<ValueType>(property.Value.GetRawText(), options);
                            }
                            break;
#pragma warning restore CS8601
                    }
                }

                return member;
            }
        }

        public override void Write(Utf8JsonWriter writer, MemberBase value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}