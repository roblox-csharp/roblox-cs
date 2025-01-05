using Microsoft.CodeAnalysis.CSharp;
using RobloxCS.Luau;

namespace RobloxCS;

public static class Program
{
    public static void Main(string[] args)
    {
        var source = """
             using System.Collections.Generic;

             var m = new List<int>();
             m.Add(6);
            """.Trim(); //File.ReadAllText(args[0]).Trim();

        Console.WriteLine(Transpiler.Transpile(source));
    }
}