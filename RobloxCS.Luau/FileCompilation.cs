using Microsoft.CodeAnalysis;
using RobloxCS.Shared;

namespace RobloxCS.Luau;

public class FileCompilation
{
    public required SyntaxTree Tree { get; set; }
    public required ConfigData Config { get; init; }
    public required RojoProject? RojoProject { get; init; }
    public Prerequisites Prerequisites { get; } = new();
    public OccupiedIdentifiersStack OccupiedIdentifiers { get; } = new();
}