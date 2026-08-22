namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestWarning
{
    public FieldRegistryHarvestWarning(string sourceName, int lineNumber, string message)
    {
        SourceName = sourceName;
        LineNumber = lineNumber;
        Message = message;
    }

    public string SourceName { get; }

    public int LineNumber { get; }

    public string Message { get; }
}

