namespace RobloxCS
{
#pragma warning disable CS8618
    public sealed class CSharpOptions
    {
        public string EntryPointName { get; set; }
        public string MainMethodName { get; set; }
        public string AssemblyName { get; set; }
        public bool EntryPointRequired { get; set; } = true;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(EntryPointName)
                && !string.IsNullOrEmpty(MainMethodName)
                && !string.IsNullOrEmpty(AssemblyName);
        }
    }

    public sealed class ConfigData
    {
        public string SourceFolder { get; set; }
        public string OutputFolder { get; set; }
        public string RojoProjectName { get; set; } = "default";
        public bool EmitNativeAttributeOnClassOrNamespaceCallbacks { get; set; } = true;
        public HashSet<string> EnabledBuiltInTransformers { get; set; } = ["Debug"];
        public CSharpOptions CSharpOptions { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(SourceFolder)
                && !string.IsNullOrEmpty(OutputFolder)
                && !string.IsNullOrEmpty(RojoProjectName)
                && CSharpOptions != null
                && CSharpOptions.IsValid();
        }
    }
#pragma warning restore CS8618
}
