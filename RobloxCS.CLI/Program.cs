using System.Reflection;
using System.Xml.Linq;
using CommandLine;
using RobloxCS;
using RobloxCS.Shared;

Parser.Default
    .ParseArguments<Options>(args)
    .WithParsed(HandleOptions);

static void HandleOptions(Options opts)
{
    if (opts.Version)
    {
        var version = XDocument.Load("RobloxCS/RobloxCS.csproj")
            .Root?.Descendants("Version")
            .FirstOrDefault()?.Value;
        
        Console.WriteLine(version);
        return;
    }
    
    if (!File.Exists(opts.ProjectDirectory))
    {
        Console.WriteLine($"fatal: project directory does not exist at '{opts.ProjectDirectory}'");
        Environment.Exit(1);
    }

    var configPath = Path.Join(opts.ProjectDirectory, "roblox-cs.yml");
    if (!File.Exists(configPath))
    {
        Console.WriteLine("fatal: roblox-cs.yml does not exist in your project directory");
        Environment.Exit(1);
    }

    var config = ConfigReader.Read(configPath);
    Transpiler.Transpile(opts.ProjectDirectory, config, opts.Verbose);
}

internal class Options
{
    [Option('v', "version",
            Required = false,
            HelpText = "Returns the compiler version.")]
    public required bool Version { get; init; }
    
    [Option("verbose",
            Required = false,
            HelpText = "Verbosely outputs transpilation process.")]
    public required bool Verbose { get; init; }

    [Option('p', "project",
            Required = false,
            HelpText = "Explicitly specify the project directory to compile. If none specified, automatically attempts to locate one.")]
    public required string ProjectDirectory { get; init; } = ".";
}