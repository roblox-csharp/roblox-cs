using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RobloxCS
{
    public static class ConfigReader
    {
        public static ConfigData UnitTestingConfig
        {
            get
            {
                return new ConfigData()
                {
                    SourceFolder = "test-src",
                    OutputFolder = "test-dist",
                    RojoProjectName = "UNIT_TESTING",
                    CSharpOptions = new CSharpOptions()
                    {
                        EntryPointName = "UnitTest",
                        MainMethodName = "Main",
                        AssemblyName = "UnitTesting"
                    }
                };
            }
        }

        private const string _fileName = "roblox-cs.yml";

        public static ConfigData Read(string inputDirectory)
        {
            var configPath = inputDirectory + "/" + _fileName;
            ConfigData? config = default;
            string ymlContent = default!;

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
            {
                FailToRead("Invalid config! Make sure it has all required fields.");
            }

            return config!;
        }

        private static void FailToRead(string message)
        {
            Logger.Error($"Failed to read {_fileName}!\n{message}");
        }
    }
}