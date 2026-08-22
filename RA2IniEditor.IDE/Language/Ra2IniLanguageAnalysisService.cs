using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.Language;

/// <summary>
/// 组合现有文本模型、语义模型和诊断服务，生成稳定的只读分析结果。
/// </summary>
internal sealed class Ra2IniLanguageAnalysisService : IRa2IniLanguageAnalysisService
{
    private const string SafeFailureMessage = "Language analysis failed.";

    private readonly IRa2IniTextDocumentParser _textDocumentParser;
    private readonly IRa2DocumentSemanticModelBuilder _semanticModelBuilder;
    private readonly CurrentFileReadonlyDiagnosticService _diagnosticService;

    public Ra2IniLanguageAnalysisService()
        : this(
            new Ra2IniTextDocumentParser(),
            new Ra2DocumentSemanticModelBuilder(),
            new CurrentFileReadonlyDiagnosticService())
    {
    }

    internal Ra2IniLanguageAnalysisService(
        IRa2IniTextDocumentParser textDocumentParser,
        IRa2DocumentSemanticModelBuilder semanticModelBuilder,
        CurrentFileReadonlyDiagnosticService diagnosticService)
    {
        _textDocumentParser = textDocumentParser ?? throw new ArgumentNullException(nameof(textDocumentParser));
        _semanticModelBuilder = semanticModelBuilder ?? throw new ArgumentNullException(nameof(semanticModelBuilder));
        _diagnosticService = diagnosticService ?? throw new ArgumentNullException(nameof(diagnosticService));
    }

    public Ra2IniLanguageAnalysisResult Analyze(Ra2LanguageAnalysisRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Ra2IniTextDocument textDocument = _textDocumentParser.Parse(request.Text);
            Ra2DocumentSnapshot documentSnapshot = new(
                request.FilePath,
                request.Text,
                request.AnalysisVersion);
            Ra2DocumentSemanticModel semanticModel = _semanticModelBuilder.Build(
                documentSnapshot,
                request.FieldRegistry.Provider);
            CurrentSourceSnapshot diagnosticSnapshot = new(
                request.ProjectRootPath,
                request.FilePath,
                request.FileName,
                request.Text,
                request.AnalysisVersion,
                SourceEditorState.Loaded);
            IReadOnlyList<IdeDiagnosticIssueViewModel> issues = _diagnosticService.Analyze(
                diagnosticSnapshot,
                request.FieldRegistry.Provider);
            IReadOnlyList<Ra2DiagnosticFact> diagnostics = issues
                .Select(issue => new Ra2DiagnosticFact(
                    issue.Code,
                    issue.SourceKind,
                    issue.Severity,
                    issue.Message,
                    issue.FilePath,
                    issue.LineNumber,
                    issue.ColumnNumber,
                    issue.SectionId,
                    issue.Key,
                    issue.Version))
                .ToArray();

            return new Ra2IniLanguageAnalysisResult(
                request,
                Ra2LanguageAnalysisFailureKind.None,
                null,
                textDocument,
                semanticModel,
                diagnostics);
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            return new Ra2IniLanguageAnalysisResult(
                request,
                Ra2LanguageAnalysisFailureKind.UnexpectedFailure,
                SafeFailureMessage,
                null,
                null,
                []);
        }
    }

    private static bool IsFatal(Exception exception)
        => exception is OutOfMemoryException
            or AccessViolationException
            or AppDomainUnloadedException
            or BadImageFormatException;
}
