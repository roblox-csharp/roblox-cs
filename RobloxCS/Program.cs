namespace RobloxCS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string? inputDirectory = args.FirstOrDefault();
            if (inputDirectory != null && !Directory.Exists(inputDirectory))
            {
                throw new Exception($"Directory \"{inputDirectory}\" does not exist.");
            }

            bool hasSrcDirectory = Directory.Exists("src");
            string sourceDirectory = inputDirectory == null ? (hasSrcDirectory ? "src" : ".") : inputDirectory;
            string[] sourceFiles = FileManager.GetSourceFiles(sourceDirectory);
            foreach (var sourceFile in sourceFiles)
            {
                var fileContents = File.ReadAllText(sourceFile);
                var codeGenerator = new CodeGenerator(fileContents);
                var luaResult = codeGenerator.GenerateLua();
                Console.WriteLine(luaResult);
            }
        }
    }
}