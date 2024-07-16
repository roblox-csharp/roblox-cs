using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

#pragma warning disable CS8618
public sealed class RojoProject
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("tree")]
    public InstanceDescription Tree { get; set; }

    [JsonPropertyName("servePort")]
    public int ServePort { get; set; } = 34872;

    [JsonPropertyName("servePlaceIds")]
    public List<string> ServePlaceIds { get; set; } = new List<string>();

    [JsonPropertyName("placeId")]
    public string? PlaceId { get; set; }

    [JsonPropertyName("gameId")]
    public string? GameId { get; set; }

    [JsonPropertyName("serveAddress")]
    public string? ServeAddress { get; set; }

    [JsonPropertyName("globIgnorePaths")]
    public List<string> GlobIgnorePaths { get; set; } = new List<string>();

    [JsonPropertyName("emitLegacyScripts")]
    public bool EmitLegacyScripts { get; set; } = true;

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Name) && Tree != null;
    }
}

public sealed class InstanceDescription
{
    [JsonPropertyName("$className")]
    public string? ClassName { get; set; }
    [JsonPropertyName("$path")]
    public string? Path { get; set; }
    [JsonPropertyName("$properties")]
    public Dictionary<string, object>? Properties { get; set; }
    [JsonPropertyName("$ignoreUnknownInstances")]
    public bool IgnoreUnknownInstances { get; set; } = true;
    public Dictionary<string, InstanceDescription> Instances { get; set; } = new Dictionary<string, InstanceDescription>();

    [JsonExtensionData]
    private IDictionary<string, JsonElement> _additionalData = new Dictionary<string, JsonElement>();

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        foreach (var kvp in _additionalData)
        {
            Instances[kvp.Key] = kvp.Value.Deserialize<InstanceDescription>()!;
        }
    }
}
#pragma warning restore CS8618

namespace RobloxCS
{
    public static class RojoReader
    {
        public static RojoProject Read(string configPath)
        {
            var jsonContent = "";
            RojoProject? project = default;

            try
            {
                jsonContent = File.ReadAllText(configPath);
            }
            catch (Exception e)
            {
                FailToRead(configPath, e.Message);
            }

            try
            {
                project = JsonSerializer.Deserialize<RojoProject>(jsonContent);
            }
            catch (Exception e)
            {
                FailToRead(configPath, e.ToString());
            }

            if (project == null || !project.IsValid())
            {
                FailToRead(configPath, "Invalid Rojo project! Make sure it has all required fields ('name' and 'tree').");
            }

            return project!;
        }

        public static string? FindProjectPath(string directoryPath)
        {
            foreach (var descendant in Directory.EnumerateFiles(directoryPath, "**/*.default.json"))
            {
                return descendant;
            }
            return null;
        }

        private static void FailToRead(string configPath, string message)
        {
            Logger.Error($"Failed to read {configPath}!\n{message}");
        }
    }
}