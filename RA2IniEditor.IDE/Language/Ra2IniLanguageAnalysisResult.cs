using RA2IniEditor.IDE.TextModel;

namespace RA2IniEditor.IDE.Language;

/// <summary>
/// 表示一次只读语言分析的成功结果或显式失败。
/// </summary>
internal sealed class Ra2IniLanguageAnalysisResult
{
    internal Ra2IniLanguageAnalysisResult(
        Ra2LanguageAnalysisRequest request,
        Ra2LanguageAnalysisFailureKind failureKind,
        string? failureMessage,
        Ra2IniTextDocument? textDocument,
        Ra2DocumentSemanticModel? semanticModel,
        IReadOnlyList<Ra2DiagnosticFact> diagnostics)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

        bool succeeded = failureKind == Ra2LanguageAnalysisFailureKind.None;
        if (succeeded)
        {
            if (failureMessage is not null)
                throw new ArgumentException("Successful results cannot carry a failure message.", nameof(failureMessage));
            if (textDocument is null)
                throw new ArgumentNullException(nameof(textDocument));
            if (semanticModel is null)
                throw new ArgumentNullException(nameof(semanticModel));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(failureMessage))
                throw new ArgumentException("Failed results require a safe failure message.", nameof(failureMessage));
            if (textDocument is not null)
                throw new ArgumentException("Failed results cannot carry a text document.", nameof(textDocument));
            if (semanticModel is not null)
                throw new ArgumentException("Failed results cannot carry a semantic model.", nameof(semanticModel));
            if (diagnostics.Count != 0)
                throw new ArgumentException("Failed results cannot carry partial diagnostics.", nameof(diagnostics));
        }

        Succeeded = succeeded;
        FailureKind = failureKind;
        FailureMessage = failureMessage;
        TextDocument = textDocument;
        SemanticModel = semanticModel;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }

    public Ra2LanguageAnalysisRequest Request { get; }

    public bool Succeeded { get; }

    public Ra2LanguageAnalysisFailureKind FailureKind { get; }

    public string? FailureMessage { get; }

    public Ra2IniTextDocument? TextDocument { get; }

    public Ra2DocumentSemanticModel? SemanticModel { get; }

    public IReadOnlyList<Ra2DiagnosticFact> Diagnostics { get; }

    public long FieldRegistryRevision => Request.FieldRegistry.Revision;
}
