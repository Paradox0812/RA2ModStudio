using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2CompletionRequest
{
    public Ra2CompletionRequest(
        Ra2DocumentSnapshot snapshot,
        Ra2DocumentSemanticModel semanticModel,
        Ra2CaretContext caretContext,
        int caretOffset,
        IRa2FieldDefinitionProvider fieldProvider)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        CaretContext = caretContext ?? throw new ArgumentNullException(nameof(caretContext));
        CaretOffset = Math.Clamp(caretOffset, 0, snapshot.Text.Length);
        FieldProvider = fieldProvider ?? throw new ArgumentNullException(nameof(fieldProvider));
    }

    public Ra2DocumentSnapshot Snapshot { get; }

    public Ra2DocumentSemanticModel SemanticModel { get; }

    public Ra2CaretContext CaretContext { get; }

    public int CaretOffset { get; }

    public IRa2FieldDefinitionProvider FieldProvider { get; }
}
