namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2EditorTextEncodingMetadata
{
    public Ra2EditorTextEncodingMetadata(
        Ra2EditorTextEncodingKind kind,
        string displayName,
        bool hasBom,
        string? codePageName = null)
    {
        Kind = kind;
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? throw new ArgumentException("Encoding display name cannot be empty.", nameof(displayName))
            : displayName;
        HasBom = hasBom;
        CodePageName = codePageName;
    }

    public Ra2EditorTextEncodingKind Kind { get; }

    public string DisplayName { get; }

    public bool HasBom { get; }

    public string? CodePageName { get; }

    public static Ra2EditorTextEncodingMetadata Unknown { get; } =
        new(Ra2EditorTextEncodingKind.Unknown, "Unknown encoding", false);
}
