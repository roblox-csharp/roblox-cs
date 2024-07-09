using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics;

namespace RobloxCS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var inputDirectory = args.FirstOrDefault(".");
            var transpiler = new Transpiler(inputDirectory);
            transpiler.Transpile();
        }
    }
}