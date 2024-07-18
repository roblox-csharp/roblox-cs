namespace RobloxCS.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var inputDirectory = args.FirstOrDefault(".");
            var transpiler = new Transpiler(inputDirectory);
            transpiler.Transpile();
        }
    }
}