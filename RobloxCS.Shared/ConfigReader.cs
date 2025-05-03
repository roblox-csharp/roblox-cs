using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RobloxCS.Shared;

public static class ConfigReader
{
    public static ConfigData UnitTestingConfig { get; } = new()
    {
        SourceFolder = "test-src",
        OutputFolder = "test-dist",
        RojoProjectName = "UNIT_TESTING"
    };

    private const string _fileName = "roblox-cs.yml";

    public static ConfigData Read(string inputDirectory)
    {
        var configPath = inputDirectory + "/" + _fileName;
        ConfigData? config = null;
        var ymlContent = "";

        try
        {
            ymlContent = File.ReadAllText(configPath);
        }
        catch (Exception e)
        {
            FailToRead(e.Message);
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithAttemptingUnquotedStringTypeDeserialization()
            .WithDuplicateKeyChecking()
            .Build();

        try
        {
            config = deserializer.Deserialize<ConfigData>(ymlContent);
        }
        catch (Exception e)
        {
            FailToRead(e.ToString());
        }

        if (config == null || !config.IsValid())
            FailToRead($"Invalid {_fileName}! Make sure it has all required fields.");

        return config!;
    }

    private static void FailToRead(string message) =>
        throw Logger.Error($"Failed to read {_fileName}!\n{message}");
}