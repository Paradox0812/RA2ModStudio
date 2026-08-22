namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestDocument
{
    public FieldRegistryHarvestDocument(string sourceName, string text)
    {
        SourceName = string.IsNullOrWhiteSpace(sourceName)
            ? throw new ArgumentException("Source name cannot be empty.", nameof(sourceName))
            : sourceName;
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public string SourceName { get; }

    public string Text { get; }
}

