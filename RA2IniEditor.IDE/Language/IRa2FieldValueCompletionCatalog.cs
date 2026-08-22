namespace RA2IniEditor.IDE.Language;

internal interface IRa2FieldValueCompletionCatalog
{
    IReadOnlyList<Ra2FieldValueCompletionCandidate> GetCandidates(
        Ra2FieldValueCompletionRequest request);
}
