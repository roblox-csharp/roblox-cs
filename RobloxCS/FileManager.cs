namespace RobloxCS
{
    internal static class FileManager
    {
        public static IEnumerable<string> GetSourceFiles(string sourceDirectory)
        {
            try
            {
                return Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories).Select(path => path.Replace("\\", "/"));
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to read source files: {e.Message}");
                return [];
            }
        }

        public static void WriteCompiledFiles(string outDirectory, List<CompiledFile> compiledFiles)
        {
            Logger.Info($"Compiling {compiledFiles.Count} files...");
            TryCreateDirectory(outDirectory);

            foreach (var compiledFile in compiledFiles)
            {
                var directoryParts = compiledFile.Path.Split('/').ToList();
                directoryParts.Remove(directoryParts.Last());
                var parentDirectory = string.Join('/', directoryParts);
                TryCreateDirectory(parentDirectory);

                try
                {
                    File.WriteAllText(compiledFile.Path, compiledFile.LuaSource);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to write to \"{compiledFile.Path}\": {e.Message}");
                }
                Logger.Info($"Successfully wrote \"{compiledFile.Path}\"!");
            }
        }

        private static void TryCreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to create directory \"{path}\": {e.Message}");
            }
        }
    }
}