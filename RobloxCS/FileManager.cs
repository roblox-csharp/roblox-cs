namespace RobloxCS
{
    internal static class FileManager
    {
        public static string[] GetSourceFiles(string sourceDirectory)
        {
            try
            {
                return Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read source files:");
                Console.WriteLine(e.Message);
                return [];
            }
        }
    }
}