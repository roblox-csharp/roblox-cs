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
        var assembly = typeof(Transpiler).Assembly;
        var informationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        Console.WriteLine(informationalVersionAttribute?.InformationalVersion.Split('+').First());
        return;
    }

    if (opts.SingleFile != null)
    {
        var transpiledLuau = Transpiler.TranspileSources([opts.SingleFile],
                                                              new RojoProject(),
                                                              ConfigReader.UnitTestingConfig);
        
        Console.WriteLine(transpiledLuau);
        return;
    }
    
    if (!Directory.Exists(opts.ProjectDirectory))
        throw Logger.Error($"Project directory does not exist at '{opts.ProjectDirectory}'");
    
    var config = ConfigReader.Read(opts.ProjectDirectory);
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
    
    [Option('f', "single-file",
        Required = false,
        HelpText = "Transpiles a single file and spits the emitted Luau out into the console.")]
    public required string? SingleFile { get; init; }

    [Option('p', "project",
            Required = false,
            HelpText = "Explicitly specify the project directory to compile. If none specified, automatically attempts to locate one.")]
    public required string ProjectDirectory { get; init; } = ".";
}