﻿using System.Reflection;
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

            [Option("init", Required = false, HelpText = "Initialize a new roblox-cs project.")]
            public bool Init { get; set; }

            [Value(0, Required = false, HelpText = "Project directory path", MetaName = "Path")]
            public string? Path { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    var path = options.Path ?? ".";
                    if (options.Version)
                    {
                        Console.WriteLine($"roblox-cs {_version.Major}.{_version.Minor}.{_version.Build}");
                    }
                    else if (options.Init)
                    {
                        try
                        {
                            Repository.Clone(_exampleProjectRepo, path);
                            AnsiConsole.MarkupLine($"[blue]Repository cloned to {Path.GetFullPath(path)}[/]");
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Failed to clone example project repository: {ex.Message}[/]");
                            Environment.Exit(1);
                        }

                        DeleteDirectoryManual(Path.Combine(path, ".git"));
                        DotNet.Command.Create("dotnet", ["restore", path]);
                        AnsiConsole.MarkupLine($"[blue]Successfully restored .NET project.[/]");

                        Console.WriteLine($"Configuration:");
                        var initRepo = AnsiConsole.Confirm("\t[yellow]Do you want to initialize a Git repository?[/]", true);
                        if (initRepo)
                        {
                            Repository.Init(path);
                            AnsiConsole.MarkupLine($"[blue]Successfully initialized Git repository.[/]");
                        }
                    }
                    else
                    {
                        new Transpiler(path).Transpile();
                    }
                });
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