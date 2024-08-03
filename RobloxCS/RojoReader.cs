using Microsoft.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    public List<ulong> ServePlaceIds { get; set; } = [];

    [JsonPropertyName("placeId")]
    public string? PlaceId { get; set; }

    [JsonPropertyName("gameId")]
    public string? GameId { get; set; }

    [JsonPropertyName("serveAddress")]
    public string? ServeAddress { get; set; }

    [JsonPropertyName("globIgnorePaths")]
    public List<string> GlobIgnorePaths { get; set; } = [];

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
    public Dictionary<string, InstanceDescription> Instances { get; set; } = [];

    [JsonExtensionData]
    public IDictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>;

    public void OnDeserialized()
    {
        foreach (var kvp in AdditionalData)
        {
            var childInstance = kvp.Value.Deserialize<InstanceDescription>()!;
            Instances[kvp.Key] = childInstance;
            childInstance.OnDeserialized();
        }
    }
}
#pragma warning restore CS8618

namespace RobloxCS
{
    public static class RojoReader
    {
        private static readonly List<string> _services = ["ReplicatedStorage", "ReplicatedFirst", "ServerStorage", "ServerScriptService", "StarterPlayer", "StarterPlayerScripts"]; // things that will be converted into game:GetService("XXX")
        private static readonly Dictionary<string, string> _instanceNameMap = new Dictionary<string, string>
        {
            { "StarterPlayer", "game:GetService(\"Players\").LocalPlayer" },
            { "StarterPlayerScripts", "PlayerScripts" }
        };

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

            UpdateChildInstances(project!.Tree);
            return project!;
        }

        public static string? FindProjectPath(string directoryPath, string projectName)
        {
            return Directory.GetFiles(directoryPath).FirstOrDefault(file => Path.GetFileName(file) == $"{projectName}.project.json");
        }

        public static string? ResolveInstancePath(RojoProject project, string filePath)
        {
            var path = TraverseInstanceTree(project.Tree, Utility.FixPathSep(filePath));
            return path == null ? null : FormatInstancePath(Utility.FixPathSep(path));
        }

        private static string? TraverseInstanceTree(InstanceDescription instance, string filePath)
        {
            var instancePath = instance.Path != null ? Utility.FixPathSep(instance.Path) : null;
            if (instancePath != null && filePath.StartsWith(instancePath))
            {
                var remainingPath = filePath.Substring(instancePath.Length + 1); // +1 to omit '/'
                return Path.ChangeExtension(remainingPath, null);
            }

            foreach (var childInstance in instance.Instances)
            {
                var result = TraverseInstanceTree(childInstance.Value, filePath);
                var leftName = childInstance.Key;
                if (_instanceNameMap.TryGetValue(leftName, out var mappedName))
                {
                    leftName = mappedName;
                }

                if (result != null)
                {
                    return $"{leftName}/{result}";
                }
            }

            return null;
        }

        private static string FormatInstancePath(string path)
        {
            var segments = path.Split('/');
            var formattedPath = new StringBuilder();
            foreach (var segment in segments)
            {
                var isServiceIdentifier = _services.Contains(segment);
                if (segment == segments.First())
                {
                    formattedPath.Append(isServiceIdentifier ? $"game:GetService(\"{segment}\")" : segment);
                }
                else
                {
                    formattedPath.Append(formattedPath.Length > 0 ? "[\"" : "");
                    formattedPath.Append(segment);
                    formattedPath.Append("\"]");
                }
            }

            return formattedPath.ToString();
        }

        private static void UpdateChildInstances(InstanceDescription instance)
        {
            instance.OnDeserialized();
            foreach (var childInstance in instance.Instances.Values)
            {
                UpdateChildInstances(childInstance);
            }
        }

        private static void FailToRead(string configPath, string message)
        {
            Logger.Error($"Failed to read {configPath}!\n{message}");
        }
    }
}
