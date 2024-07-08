namespace RobloxCS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var inputDirectory = args.FirstOrDefault(".");

            const string sourceFolderName = "src"; // temporary
            const string outFolderName = "dist"; // temporary

            var sourceDirectory = inputDirectory + "/" + sourceFolderName;
            var outDirectory = inputDirectory + "/" + outFolderName;
            if (!Directory.Exists(sourceDirectory))
            {
                Logger.Error($"Source folder \"{sourceFolderName}\" does not exist!");
            }

            var sourceFiles = FileManager.GetSourceFiles(sourceDirectory);
            var compiledFiles = new List<CompiledFile>();
            foreach (var sourceFile in sourceFiles)
            {
                var fileContents = File.ReadAllText(sourceFile);
                var codeGenerator = new CodeGenerator(fileContents);
                var luaSource = codeGenerator.GenerateLua();
                var targetPath = sourceFile.Replace(sourceFolderName, outFolderName).Replace(".cs", ".lua");
                var compiledFile = new CompiledFile(targetPath, luaSource);
                compiledFiles.Add(compiledFile);
            }

            FileManager.WriteCompiledFiles(outDirectory, compiledFiles);
        }
    }
}