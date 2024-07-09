namespace RobloxCS
{
    internal sealed class CompiledFile
    {
        public readonly string Path;
        public readonly string LuaSource;

        public CompiledFile(string path, string luaSource)
        {
            Path = path;
            LuaSource = luaSource;
        }
    }
}
