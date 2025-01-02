using Microsoft.CodeAnalysis.CSharp;
using RobloxCS.Luau;

namespace RobloxCS;

public static class Program
{
    public static void Main(string[] args)
    {
        var source = File.ReadAllText(args[0]).Trim();

        Console.WriteLine(Transpiler.Transpile(source));
        Console.ReadLine();
    }
}