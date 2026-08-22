using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation.Experimental;

public readonly struct Ra2AutomationTextSpan
{
    public Ra2AutomationTextSpan(int start, int length)
    {
        if (start < 0)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (length > int.MaxValue - start)
            throw new ArgumentOutOfRangeException(nameof(length));

        Start = start;
        Length = length;
    }

    public int Start { get; }

    public int Length { get; }

    public int End => Start + Length;
}

public sealed class Ra2AutomationFieldRegistrySnapshot
{
    public Ra2AutomationFieldRegistrySnapshot(
        IRa2FieldDefinitionProvider provider,
        long revision)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (revision <= 0)
            throw new ArgumentOutOfRangeException(nameof(revision));

        Provider = provider;
        Revision = revision;
    }

    public IRa2FieldDefinitionProvider Provider { get; }

    public long Revision { get; }
}

public sealed class Ra2AutomationDocumentSnapshot
{
    public Ra2AutomationDocumentSnapshot(
        Guid documentId,
        int version,
        string filePath,
        string text,
        bool isEditable,
        Ra2AutomationFieldRegistrySnapshot fieldRegistry)
    {
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document identity is required.", nameof(documentId));

        if (version < 0)
            throw new ArgumentOutOfRangeException(nameof(version));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required.", nameof(filePath));

        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(fieldRegistry);

        DocumentId = documentId;
        Version = version;
        FilePath = filePath;
        Text = text;
        IsEditable = isEditable;
        FieldRegistry = fieldRegistry;
    }

    public Guid DocumentId { get; }

    public int Version { get; }

    public string FilePath { get; }

    public string Text { get; }

    public bool IsEditable { get; }

    public Ra2AutomationFieldRegistrySnapshot FieldRegistry { get; }
}
