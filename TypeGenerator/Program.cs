using CommandLine;
using CommandLine.Text;
using TypeGenerator.Generators;

namespace TypeGenerator
{
    class Options
    {
        [Option('o', "output", HelpText = "Set the output directory.")]
        public string? OutputDirectory { get; set; }
    }

    public static class Program
    {
        private static readonly string[] _securityLevels = ["None", "PluginSecurity"];

        public static async Task Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
            if (result.Errors.Count() > 0) return;

            var options = result.Value;
            if (options.OutputDirectory == null)
            {
                Console.WriteLine("Required option 'o, output' is missing.");
                return;
            }

           await Generate(options.OutputDirectory);
        }

        private static async Task Generate(string outputDirectory)
        {
            var dump = await StudioAPI.GetDump();
            var reflectionMetadata = await StudioAPI.GetReflectionMetadata();

            var enumsFilePath = Path.Combine(outputDirectory, "generated", "Enums.cs");
            var enumGenerator = new EnumGenerator(enumsFilePath, reflectionMetadata);
            enumGenerator.Generate(dump.Enums);
        }
    }
}