namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiCurrentSubject
{
    public Ra2AiSubjectKind Kind { get; init; }

    public string? SubjectId { get; init; }

    public Ra2AiSubjectSource Source { get; init; }

    public string Summary { get; init; } = string.Empty;

    public double Confidence { get; init; }

    public bool IsDraft { get; init; }
}
