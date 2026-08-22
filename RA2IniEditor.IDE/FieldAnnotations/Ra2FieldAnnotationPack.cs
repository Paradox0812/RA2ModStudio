namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2FieldAnnotationPack
{
    public Ra2FieldAnnotationPack(
        int version,
        string language,
        IReadOnlyList<Ra2FieldAnnotationEntry>? entries = null)
    {
        Version = version;
        Language = string.IsNullOrWhiteSpace(language) ? "zh-CN" : language.Trim();
        Entries = entries ?? [];
    }

    public int Version { get; }

    public string Language { get; }

    public IReadOnlyList<Ra2FieldAnnotationEntry> Entries { get; }

    public static Ra2FieldAnnotationPack Empty(string language = "zh-CN")
        => new(1, language, []);
}
