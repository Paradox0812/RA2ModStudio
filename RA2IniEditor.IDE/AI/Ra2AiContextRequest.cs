using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiContextRequest
{
    public Ra2AiContextRequest(
        string? documentDisplayName,
        Ra2DocumentSemanticModel? semanticModel,
        int caretOffset,
        string? selectedText = null,
        int nearbyLineRadius = 5,
        bool includeNearbyText = true,
        string? promptText = null,
        IRa2FieldDefinitionProvider? fieldDefinitionProvider = null,
        IFieldRegistryProvenanceProvider? fieldProvenanceProvider = null,
        int maxFieldEvidenceCount = Ra2FieldRegistryAiEvidenceProvider.DefaultMaxEvidenceCount,
        IReadOnlyList<IdeDiagnosticIssueViewModel>? diagnosticIssues = null,
        string? documentFilePath = null,
        int documentVersion = 0,
        int maxDiagnosticCount = Ra2CurrentFileAiDiagnosticSummaryProvider.DefaultMaxDiagnosticCount,
        Ra2AiConversationContext? conversationContext = null,
        Ra2AiCurrentSubject? currentSubject = null)
    {
        DocumentDisplayName = string.IsNullOrWhiteSpace(documentDisplayName) ? null : documentDisplayName.Trim();
        SemanticModel = semanticModel;
        CaretOffset = caretOffset;
        SelectedText = string.IsNullOrWhiteSpace(selectedText) ? null : selectedText;
        NearbyLineRadius = Math.Max(0, nearbyLineRadius);
        IncludeNearbyText = includeNearbyText;
        PromptText = string.IsNullOrWhiteSpace(promptText) ? null : promptText.Trim();
        FieldDefinitionProvider = fieldDefinitionProvider;
        FieldProvenanceProvider = fieldProvenanceProvider;
        MaxFieldEvidenceCount = Math.Max(0, maxFieldEvidenceCount);
        DiagnosticIssues = Array.AsReadOnly((diagnosticIssues ?? []).ToArray());
        DocumentFilePath = string.IsNullOrWhiteSpace(documentFilePath) ? null : documentFilePath;
        DocumentVersion = Math.Max(0, documentVersion);
        MaxDiagnosticCount = Math.Max(0, maxDiagnosticCount);
        ConversationContext = conversationContext;
        CurrentSubject = currentSubject;
    }

    public string? DocumentDisplayName { get; }

    public Ra2DocumentSemanticModel? SemanticModel { get; }

    public int CaretOffset { get; }

    public string? SelectedText { get; }

    public int NearbyLineRadius { get; }

    public bool IncludeNearbyText { get; }

    public string? PromptText { get; }

    public IRa2FieldDefinitionProvider? FieldDefinitionProvider { get; }

    public IFieldRegistryProvenanceProvider? FieldProvenanceProvider { get; }

    public int MaxFieldEvidenceCount { get; }

    public IReadOnlyList<IdeDiagnosticIssueViewModel> DiagnosticIssues { get; }

    public string? DocumentFilePath { get; }

    public int DocumentVersion { get; }

    public int MaxDiagnosticCount { get; }

    public Ra2AiConversationContext? ConversationContext { get; }

    public Ra2AiCurrentSubject? CurrentSubject { get; }
}
