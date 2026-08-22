using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.ViewModels.FieldBrowser;

namespace RA2IniEditor.IDE.Controllers.FieldAnnotations;

internal interface IRa2FieldAnnotationCoordinator
{
    string GetProjectAnnotationPath(string? projectRootPath, string language = "zh-CN");

    Ra2FieldAnnotationRefreshResult Refresh(Ra2FieldAnnotationRefreshRequest request);
}

internal sealed class Ra2FieldAnnotationCoordinator : IRa2FieldAnnotationCoordinator
{
    private readonly IRa2FieldAnnotationStore _annotationStore;
    private readonly Ra2FieldAnnotationPathService _pathService;

    public Ra2FieldAnnotationCoordinator(
        IRa2FieldAnnotationStore annotationStore,
        Ra2FieldAnnotationPathService pathService)
    {
        _annotationStore = annotationStore ?? throw new ArgumentNullException(nameof(annotationStore));
        _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
    }

    public string GetProjectAnnotationPath(string? projectRootPath, string language = "zh-CN")
        => _pathService.GetProjectAnnotationPath(projectRootPath, language);

    public Ra2FieldAnnotationRefreshResult Refresh(Ra2FieldAnnotationRefreshRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string annotationPath = GetProjectAnnotationPath(request.ProjectRootPath, request.Language);
        Ra2FieldAnnotationLoadResult loadResult = _annotationStore.Load(annotationPath);
        Ra2FieldAnnotationProvider provider = new(loadResult.Pack);
        Ra2FieldDisplayResolver resolver = new(request.FieldProvider, provider);
        Ra2FieldAnnotationStatusViewModel status =
            Ra2FieldAnnotationStatusViewModel.FromLoadResult(annotationPath, loadResult);
        string? message = loadResult.Success
            ? "Field annotation library loaded."
            : "Field annotation library failed to load; using field registry fallback.";

        return new Ra2FieldAnnotationRefreshResult(
            annotationPath,
            loadResult.Pack,
            provider,
            resolver,
            status,
            loadResult.Warnings,
            message,
            loadResult);
    }
}

internal sealed class Ra2FieldAnnotationRefreshRequest
{
    public Ra2FieldAnnotationRefreshRequest(
        IRa2FieldDefinitionProvider fieldProvider,
        string? projectRootPath,
        string language = "zh-CN")
    {
        FieldProvider = fieldProvider ?? throw new ArgumentNullException(nameof(fieldProvider));
        ProjectRootPath = projectRootPath;
        Language = string.IsNullOrWhiteSpace(language) ? "zh-CN" : language.Trim();
    }

    public IRa2FieldDefinitionProvider FieldProvider { get; }

    public string? ProjectRootPath { get; }

    public string Language { get; }
}

internal sealed class Ra2FieldAnnotationRefreshResult
{
    public Ra2FieldAnnotationRefreshResult(
        string annotationPath,
        Ra2FieldAnnotationPack pack,
        IRa2FieldAnnotationProvider provider,
        IRa2FieldDisplayResolver displayResolver,
        Ra2FieldAnnotationStatusViewModel status,
        IReadOnlyList<string> warnings,
        string? message,
        Ra2FieldAnnotationLoadResult loadResult)
    {
        AnnotationPath = annotationPath ?? string.Empty;
        Pack = pack ?? throw new ArgumentNullException(nameof(pack));
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        DisplayResolver = displayResolver ?? throw new ArgumentNullException(nameof(displayResolver));
        Status = status ?? throw new ArgumentNullException(nameof(status));
        Warnings = warnings ?? [];
        Message = message;
        LoadResult = loadResult ?? throw new ArgumentNullException(nameof(loadResult));
    }

    public string AnnotationPath { get; }

    public Ra2FieldAnnotationPack Pack { get; }

    public IRa2FieldAnnotationProvider Provider { get; }

    public IRa2FieldDisplayResolver DisplayResolver { get; }

    public Ra2FieldAnnotationStatusViewModel Status { get; }

    public IReadOnlyList<string> Warnings { get; }

    public string? Message { get; }

    public Ra2FieldAnnotationLoadResult LoadResult { get; }
}
