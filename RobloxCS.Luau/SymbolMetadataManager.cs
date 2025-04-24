using Microsoft.CodeAnalysis;

namespace RobloxCS.Luau;

public class SymbolMetadata
{
    public IdentifierName? EventConnectionName { get; set; }
}

public static class SymbolMetadataManager
{
    private static readonly Dictionary<ISymbol, SymbolMetadata> _metadata = [];

    public static SymbolMetadata Get(ISymbol symbol)
    {
        var metadata = _metadata.GetValueOrDefault(symbol);
        if (metadata == null)
            _metadata.Add(symbol, metadata = new SymbolMetadata());
        
        return metadata;
    }
}