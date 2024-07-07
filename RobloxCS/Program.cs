namespace RobloxCS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string? inputtedDirectory = args.FirstOrDefault();
            if (inputtedDirectory != null && !Directory.Exists(inputtedDirectory))
            {
                throw new Exception($"Directory \"{inputtedDirectory}\" does not exist.");
            }

            bool hasSrcDirectory = Directory.Exists("src");
            string sourceDirectory = inputtedDirectory == null ? (hasSrcDirectory ? "src" : ".") : inputtedDirectory;
            string[] sourceFiles = FileManager.GetSourceFiles(sourceDirectory);
            foreach (var sourceFile in sourceFiles)
            {
                var fileContents = File.ReadAllText(sourceFile);
            }
        }
    }
}