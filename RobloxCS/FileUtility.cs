using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using RobloxCS.Shared;

namespace RobloxCS;

public static class FileUtility
{
    private const string _runtimeAssemblyName = "Roblox";

    public static string? GetRbxcsDirectory()
    {
        var directoryName = Path.GetDirectoryName(GetAssemblyDirectory());
        return directoryName == null ? null : FixPathSeparator(directoryName);
    }
    
    public static List<PortableExecutableReference> GetCompilationReferences()
    {
        var runtimeLibAssemblyPath = string.Join('/', GetAssemblyDirectory(), _runtimeAssemblyName + ".dll");
        if (!File.Exists(runtimeLibAssemblyPath))
        {
            var directoryName = Path.GetDirectoryName(runtimeLibAssemblyPath);
            var location = directoryName == null ? "(could not find assembly directory)" : FixPathSeparator(directoryName);
            Logger.Error($"Failed to find {_runtimeAssemblyName}.dll in {location}");
        }

        List<PortableExecutableReference> references = [
            MetadataReference.CreateFromFile(runtimeLibAssemblyPath)
        ];
        
        references.AddRange(GetCoreLibReferences());
        return references;
    }

    private static HashSet<PortableExecutableReference> GetCoreLibReferences()
    {
        var coreLib = typeof(object).GetTypeInfo().Assembly.Location;
        HashSet<string> coreDlls = ["System.Runtime.dll", "System.Core.dll", "System.Collections.dll"];
        HashSet<PortableExecutableReference> references = [MetadataReference.CreateFromFile(coreLib)];

        foreach (var dllPath in coreDlls.Select(coreDll => Path.Combine(Path.GetDirectoryName(coreLib)!, coreDll)))
            references.Add(MetadataReference.CreateFromFile(dllPath));

        references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location));
        return references;
    }
    
    private static string FixPathSeparator(string path)
    {
        var cleanedPath = Path.TrimEndingDirectorySeparator(path)
            .Replace(@"\\", "/")
            .Replace('\\', '/')
            .Replace("//", "/");
        
        return Regex.Replace(cleanedPath, @"(?<!\.)\./", "");
    }
    
    private static string GetAssemblyDirectory()
    {
        var location = FixPathSeparator(Assembly.GetExecutingAssembly().Location);
        var directoryName = Path.GetDirectoryName(location)!;
        return FixPathSeparator(directoryName);
    }
}