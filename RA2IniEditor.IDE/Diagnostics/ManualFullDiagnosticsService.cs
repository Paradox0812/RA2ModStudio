using System.IO;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.IO;

namespace RA2IniEditor.IDE.Diagnostics;

/// <summary>
/// Runs user-triggered readonly structure diagnostics across discovered INI files.
/// </summary>
internal sealed class ManualFullDiagnosticsService
{
    public const long MaxAnalyzedFileSizeBytes = 8 * 1024 * 1024;

    private readonly IIniFileStore _fileStore;
    private readonly CurrentFileReadonlyDiagnosticService _diagnosticService;
    private readonly IRa2DocumentSemanticModelBuilder _semanticModelBuilder;
    private readonly Ra2ReferenceDiagnosticCatalogBuilder _referenceCatalogBuilder;

    public ManualFullDiagnosticsService()
        : this(new IniFileStore(), new CurrentFileReadonlyDiagnosticService())
    {
    }

    internal ManualFullDiagnosticsService(
        IIniFileStore fileStore,
        CurrentFileReadonlyDiagnosticService diagnosticService)
    {
        _fileStore = fileStore;
        _diagnosticService = diagnosticService;
        _semanticModelBuilder = new Ra2DocumentSemanticModelBuilder();
        _referenceCatalogBuilder = new Ra2ReferenceDiagnosticCatalogBuilder();
    }

    public ManualFullDiagnosticsResult Analyze(ManualFullDiagnosticsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        List<IdeDiagnosticIssueViewModel> issues = [];
        List<string> skippedFiles = [];
        List<ManualFullDiagnosticsDocument> documents = [];
        int analyzedCount = 0;

        foreach (ReadonlyIniFileDescriptor file in request.Files)
        {
            if (ShouldSkipPath(file.FilePath))
            {
                skippedFiles.Add($"{file.FileName}: ignored path skipped");
                continue;
            }

            if (file.FileSizeBytes > MaxAnalyzedFileSizeBytes)
            {
                skippedFiles.Add($"{file.FileName}: large file skipped");
                continue;
            }

            try
            {
                string text = ResolveText(request, file);
                int version = ResolveVersion(request, file);
                CurrentSourceSnapshot snapshot = new(
                    request.ProjectRootPath,
                    file.FilePath,
                    file.FileName,
                    text,
                    version,
                    SourceEditorState.Loaded);
                Ra2DocumentSemanticModel? semanticModel = request.FieldProvider is null
                    ? null
                    : _semanticModelBuilder.Build(new Ra2DocumentSnapshot(snapshot.FilePath, snapshot.Text, snapshot.Version), request.FieldProvider);
                documents.Add(new ManualFullDiagnosticsDocument(snapshot, semanticModel));
                analyzedCount++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
            {
                skippedFiles.Add($"{file.FileName}: {ex.Message}");
            }
        }

        Ra2ReferenceDiagnosticCatalog? projectReferenceCatalog = BuildProjectReferenceCatalog(documents);
        foreach (ManualFullDiagnosticsDocument document in documents)
        {
            issues.AddRange(projectReferenceCatalog is null
                ? _diagnosticService.Analyze(document.Snapshot, request.FieldProvider)
                : _diagnosticService.AnalyzeWithReferenceCatalog(document.Snapshot, request.FieldProvider, projectReferenceCatalog, "当前项目"));
        }

        string statusText = BuildStatusText(issues.Count, analyzedCount, skippedFiles.Count);
        return new ManualFullDiagnosticsResult(issues, skippedFiles, analyzedCount, statusText);
    }

    private string ResolveText(ManualFullDiagnosticsRequest request, ReadonlyIniFileDescriptor file)
    {
        if (request.CurrentSnapshot is not null &&
            string.Equals(request.CurrentSnapshot.FilePath, file.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            return request.CurrentEditorText;
        }

        return _fileStore.ReadText(file.FilePath).Text;
    }

    private static int ResolveVersion(ManualFullDiagnosticsRequest request, ReadonlyIniFileDescriptor file)
        => request.CurrentSnapshot is not null &&
           string.Equals(request.CurrentSnapshot.FilePath, file.FilePath, StringComparison.OrdinalIgnoreCase)
            ? request.CurrentSnapshot.Version
            : 0;

    private Ra2ReferenceDiagnosticCatalog? BuildProjectReferenceCatalog(IReadOnlyList<ManualFullDiagnosticsDocument> documents)
    {
        if (documents.All(document => document.SemanticModel is null))
            return null;

        return _referenceCatalogBuilder.BuildFromDocuments(documents
            .Where(document => document.SemanticModel is not null)
            .Select(document => new Ra2ReferenceCatalogDocument(document.Snapshot.FilePath, document.SemanticModel!)));
    }

    private static bool ShouldSkipPath(string filePath)
    {
        string normalizedPath = filePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        string[] segments = normalizedPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        return segments.Any(segment =>
            segment.Equals("Backups", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("artifacts", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals(".vs", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("obj", StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildStatusText(int issueCount, int analyzedCount, int skippedCount)
    {
        string issueText = issueCount == 1 ? "1 issue" : $"{issueCount} issues";
        string skippedText = skippedCount == 0 ? string.Empty : $", skipped {skippedCount}";
        return $"Manual diagnostics complete: {issueText}, analyzed {analyzedCount} file(s){skippedText}.";
    }
}

internal sealed class ManualFullDiagnosticsDocument
{
    public ManualFullDiagnosticsDocument(
        CurrentSourceSnapshot snapshot,
        Ra2DocumentSemanticModel? semanticModel)
    {
        Snapshot = snapshot;
        SemanticModel = semanticModel;
    }

    public CurrentSourceSnapshot Snapshot { get; }

    public Ra2DocumentSemanticModel? SemanticModel { get; }
}

internal sealed class ManualFullDiagnosticsRequest
{
    public ManualFullDiagnosticsRequest(
        string projectRootPath,
        IReadOnlyList<ReadonlyIniFileDescriptor> files,
        CurrentSourceSnapshot? currentSnapshot,
        string currentEditorText,
        IRa2FieldDefinitionProvider? fieldProvider = null)
    {
        ProjectRootPath = projectRootPath;
        Files = files;
        CurrentSnapshot = currentSnapshot;
        CurrentEditorText = currentEditorText;
        FieldProvider = fieldProvider;
    }

    public string ProjectRootPath { get; }

    public IReadOnlyList<ReadonlyIniFileDescriptor> Files { get; }

    public CurrentSourceSnapshot? CurrentSnapshot { get; }

    public string CurrentEditorText { get; }

    public IRa2FieldDefinitionProvider? FieldProvider { get; }
}

internal sealed class ManualFullDiagnosticsResult
{
    public ManualFullDiagnosticsResult(
        IReadOnlyList<IdeDiagnosticIssueViewModel> issues,
        IReadOnlyList<string> skippedFiles,
        int analyzedFileCount,
        string statusText)
    {
        Issues = issues;
        SkippedFiles = skippedFiles;
        AnalyzedFileCount = analyzedFileCount;
        StatusText = statusText;
    }

    public IReadOnlyList<IdeDiagnosticIssueViewModel> Issues { get; }

    public IReadOnlyList<string> SkippedFiles { get; }

    public int AnalyzedFileCount { get; }

    public string StatusText { get; }
}
