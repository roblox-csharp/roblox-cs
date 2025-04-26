using RobloxCS;

var path = args.ElementAtOrDefault(0);
if (path == null)
{
    Console.WriteLine("No path was provided!");
    Environment.Exit(1);
}

var source = File.ReadAllText(path).Trim();
Console.WriteLine(Transpiler.TranspileSource(source, new RojoProject(), null)); // temporary!