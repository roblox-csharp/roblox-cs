using CommandLine;
using System.Reflection;

namespace RobloxCS.CLI
{
    public static class Program
    {
        private static readonly Assembly _compilerAssembly = Assembly.Load("RobloxCS");
        private static readonly Version _version = _compilerAssembly.GetName().Version!;

        public class Options
        {
            [Option('v', "version", Required = false, HelpText = "Return the compiler version.")]
            public bool Version { get; set; }

            [Value(0, Required = false, HelpText = "Project directory path", MetaName = "Path")]
            public string? Path { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    if (options.Version)
                    {
                           
                        Console.WriteLine($"roblox-cs {_version.Major}.{_version.Minor}.{_version.Build}");
                    }
                    else
                    {
                        var transpiler = new Transpiler(options.Path ?? ".");
                        transpiler.Transpile();
                    }
                });
        }
    }
}