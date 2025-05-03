using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RobloxCS.Shared;

public sealed class RojoProject
{
    [JsonPropertyName("name")] public string Name { get; init; }

    [JsonPropertyName("tree")] public InstanceDescription Tree { get; init; }

    [JsonPropertyName("servePort")] public int ServePort { get; init; } = 34872;

    [JsonPropertyName("servePlaceIds")] public List<ulong> ServePlaceIDs { get; init; } = [];

    [JsonPropertyName("placeId")] public string? PlaceId { get; init; }

    [JsonPropertyName("gameId")] public string? GameId { get; init; }

    [JsonPropertyName("serveAddress")] public string? ServeAddress { get; init; }

    [JsonPropertyName("globIgnorePaths")] public List<string> GlobIgnorePaths { get; init; } = [];

    [JsonPropertyName("emitLegacyScripts")]
    public bool EmitLegacyScripts { get; init; } = true;

    public bool IsValid() => !string.IsNullOrEmpty(Name) && Tree != null;
}

public sealed class InstanceDescription
{
    [JsonPropertyName("$className")]
    public string? ClassName { get; init; }
    
    [JsonPropertyName("$path")]
    public string? Path { get; init; }
    
    [JsonPropertyName("$properties")]
    public Dictionary<string, object>? Properties { get; init; }
    
    [JsonPropertyName("$ignoreUnknownInstances")]
    public bool IgnoreUnknownInstances { get; init; } = true;
    public Dictionary<string, InstanceDescription> Instances { get; init; } = [];

    [JsonExtensionData]
    public IDictionary<string, JsonElement> AdditionalData { get; init; } = new Dictionary<string, JsonElement>();

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

public static class RojoReader
{
    public static RojoProject Read(string projectPath)
    {
        var jsonContent = "";
        RojoProject? project = null;

        try
        {
            jsonContent = File.ReadAllText(projectPath);
        }
        catch (Exception e)
        {
            FailToRead(projectPath, e.Message);
        }

        try
        {
            project = JsonSerializer.Deserialize<RojoProject>(jsonContent);
        }
        catch (Exception e)
        {
            FailToRead(projectPath, e.ToString());
        }

        if (project == null || !project.IsValid())
            FailToRead(projectPath, "Invalid Rojo project! Make sure it has all required fields ('name' and 'tree').");

        UpdateChildInstances(project!.Tree);
        return project!;
    }

    public static RojoProject? ReadFromDirectory(string inputDirectory, string projectName)
    {
        if (projectName == "UNIT_TESTING") return null;

        var path = FindProjectPath(inputDirectory, projectName);
        if (path == null)
            throw Logger.Error($"Failed to find Rojo project file \"{projectName}.project.json\"!");

        return Read(path);
    }

    public static string? ResolveInstancePath(RojoProject project, string filePath)
    {
        var path = TraverseInstanceTree(project.Tree, StandardUtility.FixPathSeparator(filePath));
        return path == null ? null : FormatInstancePath(StandardUtility.FixPathSeparator(path));
    }

    private static string? FindProjectPath(string directoryPath, string projectName) =>
        Directory.GetFiles(directoryPath)
                 .FirstOrDefault(file => Path.GetFileName(file) == $"{projectName}.project.json");

    private static string? TraverseInstanceTree(InstanceDescription instance, string filePath)
    {
        var instancePath = instance.Path != null ? StandardUtility.FixPathSeparator(instance.Path) : null;
        if (instancePath != null && filePath.StartsWith(instancePath))
        {
            var remainingPath = filePath[(instancePath.Length + 1)..]; // +1 to omit '/'
            return Path.ChangeExtension(remainingPath, null);
        }

        foreach (var (leftName, value) in instance.Instances)
        {
            var result = TraverseInstanceTree(value, filePath);
            if (result == null) continue;

            return $"{leftName}/{result}";
        }

        return null;
    }

    private static string FormatInstancePath(string path)
    {
        var segments = path.Split('/');
        var formattedPath = new StringBuilder();
        
        foreach (var segment in segments)
        {
            if (segment == segments.First())
            {
                formattedPath.Append(segment);
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
            UpdateChildInstances(childInstance);
    }

    private static void FailToRead(string configPath, string message) =>
        throw Logger.Error($"Failed to read {configPath}!\nReason: {message}");
}