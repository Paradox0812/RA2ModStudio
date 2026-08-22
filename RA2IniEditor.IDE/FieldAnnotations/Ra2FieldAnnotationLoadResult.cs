namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2FieldAnnotationLoadResult
{
    public Ra2FieldAnnotationLoadResult(
        Ra2FieldAnnotationPack pack,
        IReadOnlyList<string>? warnings = null,
        bool success = true)
    {
        Pack = pack ?? throw new ArgumentNullException(nameof(pack));
        Warnings = warnings ?? [];
        Success = success;
    }

    public Ra2FieldAnnotationPack Pack { get; }

    public IReadOnlyList<string> Warnings { get; }

    public bool Success { get; }
}
