using Microsoft.CodeAnalysis.CSharp;
using RobloxCS.Luau;

namespace RobloxCS;

public static class Program
{
    public static void Main(string[] args)
    {
        var source = """
            var a = 1;
            switch (a)
            {
                case 1:
                case 2:
                {
                    var blah = "blah";
                    break;
                }

                case 3:
                    break;
            }
            """.Trim(); //File.ReadAllText(args[0]).Trim();

        Console.WriteLine(Transpiler.Transpile(source));
    }
}