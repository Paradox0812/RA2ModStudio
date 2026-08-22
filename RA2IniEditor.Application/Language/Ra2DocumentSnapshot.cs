namespace RA2IniEditor.Application.Language;

internal sealed class Ra2DocumentSnapshot
{
    public Ra2DocumentSnapshot(string? filePath, string text, int version)
    {
        FilePath = string.IsNullOrWhiteSpace(filePath) ? null : filePath;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Version = version;
    }

    public string? FilePath { get; }

    public string Text { get; }

    public int Version { get; }
}
