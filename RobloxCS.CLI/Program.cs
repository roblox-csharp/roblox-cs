using System.Reflection;
using LibGit2Sharp;
using CommandLine;
using Spectre.Console;
using DotNet = Microsoft.DotNet.Cli.Utils;

namespace RobloxCS.CLI
{
    public static class Program
    {
        private const string _exampleProjectRepo = "https://github.com/roblox-csharp/example-project.git";
        private static readonly Assembly _compilerAssembly = Assembly.Load("RobloxCS");
        private static readonly System.Version _version = _compilerAssembly.GetName().Version!;

        public class Options
        {
            [Option('v', "version", Required = false, HelpText = "Return the compiler version.")]
            public bool Version { get; set; }

            [Option('w', "watch", Required = false, HelpText = "Watches the directory and re-compiles when changes are detected.")]
            public bool WatchMode { get; set; }

            [Option("init", Required = false, HelpText = "Initialize a new roblox-cs project.")]
            public bool Init { get; set; }

            [Value(0, Required = false, HelpText = "Project directory path", MetaName = "Path")]
            public string? Path { get; set; }

            [Value(1, Required = false, HelpText = "Project name", MetaName = "Name")]
            public string? Name { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    var name = options.Name ?? "Example";
                    var path = options.Path ?? ".";
                    if (options.Version)
                    {
                        Console.WriteLine($"roblox-cs {_version.Major}.{_version.Minor}.{_version.Build}");
                    }
                    else if (options.Init)
                    {
                        InitializeProject(path, name);
                    }
                    else if (options.WatchMode)
                    {
                        StartWatchMode(path);
                    }
                    else
                    {
                        new Transpiler(path).Transpile();
                    }
                });
        }

        private static void StartWatchMode(string path)
        {
            var transpiler = new Transpiler(path);
            var watcher = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                Filter = "*.*",
                IncludeSubdirectories = true
            };

            watcher.Changed += (sender, e) => OnFileChanged(path, e);
            watcher.Created += (sender, e) => OnFileChanged(path, e);
            watcher.Renamed += (sender, e) => OnFileChanged(path, e);
            watcher.Deleted += (sender, e) => OnFileChanged(path, e);
            watcher.EnableRaisingEvents = true;

            AnsiConsole.MarkupLine("[yellow]Watching for file changes. Press \"enter\" to exit.[/]");
            Console.WriteLine();
            Console.ReadLine();
        }

        private static void OnFileChanged(string path, FileSystemEventArgs e)
        {
            HashSet<WatcherChangeTypes> validChangeTypes = [WatcherChangeTypes.Changed, WatcherChangeTypes.Created, WatcherChangeTypes.Renamed];
            if (!validChangeTypes.Contains(e.ChangeType)) return;

            var filePath = Utility.FixPathSep(e.FullPath);
            var extension = Path.GetExtension(filePath);
            var transpiler = new Transpiler(path);
            var isValidLuaFile = extension == ".lua"
                && !filePath.EndsWith("include/RuntimeLib.lua")
                && !filePath.Contains($"/{transpiler.Config.OutputFolder}/");

            if (extension == ".cs" || isValidLuaFile || extension == ".yml")
            {
                AnsiConsole.MarkupLine($"[yellow]{(e.ChangeType == WatcherChangeTypes.Renamed ? "Changed" : e.ChangeType)}: {filePath}[/]");
                transpiler.Transpile();
            }
        }

        private static void InitializeProject(string path, string projectName)
        {
            try
            {
                Repository.Clone(_exampleProjectRepo, path);
                AnsiConsole.MarkupLine($"[cyan]Repository cloned to {Path.GetFullPath(path)}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to clone example project repository: {ex.Message}[/]");
                Environment.Exit(1);
            }

            DeleteDirectoryManual(Path.Combine(path, ".git"));
            DotNet.Command.Create("dotnet", ["restore", path]).Execute();
            AnsiConsole.MarkupLine("[cyan]Successfully restored .NET project.[/]");

            if (projectName != "Example")
            {
                var projectFolder = Path.Combine(path, projectName);
                var projectSourceFolder = Path.Combine(projectFolder, "src");
                var projectFile = Path.Combine(projectSourceFolder, $"{projectName}.csproj");
                var rojoManifestFile = Path.Combine(projectFolder, "default.project.json");

                AnsiConsole.MarkupLine("[yellow]Renaming project...[/]");
                File.Delete(Path.Combine(path, "Example.sln"));
                DotNet.Command.Create("dotnet", ["new", "sln", "-n", projectName, "-o", path]).Execute();
                Directory.Move(Path.Combine(path, "Example"), projectFolder);
                File.Move(Path.Combine(projectSourceFolder, "Example.csproj"), projectFile);

                // Dynamicallty replace <Title>...</Title> inside of the csproj file. This sucks, and we must edit the solution to not lose the reference to it.
                var lines = File.ReadAllText(projectFile);
                var start = lines.IndexOf("<Title>", StringComparison.InvariantCulture) + "<Title>".Length;
                var end = lines.IndexOf("</Title>", StringComparison.InvariantCulture);
                lines = lines.Remove(start, end - start);
                lines = lines.Insert(start, projectName);
                File.WriteAllText(projectFile, lines);

                DotNet.Command.Create("dotnet", ["sln", Path.Combine(path, $"{projectName}.sln"), "add", projectFile]).Execute();
                AnsiConsole.MarkupLine("[cyan]Successfully renamed project.[/]");

                AnsiConsole.MarkupLine("[yellow]Modifying rojo manifest...[/]");

                var rojoManifest = File.ReadAllText(rojoManifestFile);
                // Match for the name JSON key and its end and replace the name of it, horrible, but works.
                var idxOfStart = rojoManifest.IndexOf("\"name\": \"", StringComparison.InvariantCulture) + "\"name\": \"".Length;
                var idxOfEnd = rojoManifest.IndexOf("\",", idxOfStart, StringComparison.InvariantCulture);
                rojoManifest = rojoManifest.Remove(idxOfStart, idxOfEnd - idxOfStart);
                rojoManifest = rojoManifest.Insert(idxOfStart, projectName);
                File.WriteAllText(rojoManifestFile, rojoManifest);

            }

            Console.WriteLine($"Configuration:");
            var initRepo = AnsiConsole.Confirm("\t[yellow]Do you want to initialize a Git repository?[/]", true);
            if (initRepo)
            {
                Repository.Init(path);
                AnsiConsole.MarkupLine("[cyan]Successfully initialized Git repository.[/]");
            }
        }

        private static void DeleteDirectoryManual(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    File.SetAttributes(file, FileAttributes.Normal); // Ensure file is not read-only
                    File.Delete(file);
                }

                foreach (string subdir in Directory.GetDirectories(path))
                {
                    DeleteDirectoryManual(subdir);
                }

                Directory.Delete(path, recursive: true);
            }
        }
    }
}