namespace RobloxCS.Shared;

public sealed class ConfigData
{
    public required string SourceFolder { get; init; }
    public required string OutputFolder { get; init; }
    public required string RojoProjectName { get; init; } = "default";

    public bool IsValid() =>
        !string.IsNullOrEmpty(SourceFolder)
        && !string.IsNullOrEmpty(OutputFolder)
        && !string.IsNullOrEmpty(RojoProjectName);
}