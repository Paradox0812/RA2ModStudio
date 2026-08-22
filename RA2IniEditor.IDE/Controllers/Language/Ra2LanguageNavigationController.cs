using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.Controllers.Language;

internal interface IRa2LanguageNavigationController
{
    Ra2GoToDefinitionResult GoToDefinition(Ra2LanguageNavigationRequest request);

    Ra2PeekDefinitionResult PeekDefinition(Ra2LanguageNavigationRequest request);

    Ra2FindReferencesNavigationResult FindReferences(Ra2LanguageNavigationRequest request);
}

internal sealed class Ra2LanguageNavigationController : IRa2LanguageNavigationController
{
    private readonly IRa2DefinitionProvider _definitionProvider;
    private readonly IRa2ReferenceFinder _referenceFinder;

    public Ra2LanguageNavigationController(
        IRa2DefinitionProvider definitionProvider,
        IRa2ReferenceFinder referenceFinder)
    {
        _definitionProvider = definitionProvider ?? throw new ArgumentNullException(nameof(definitionProvider));
        _referenceFinder = referenceFinder ?? throw new ArgumentNullException(nameof(referenceFinder));
    }

    public Ra2GoToDefinitionResult GoToDefinition(Ra2LanguageNavigationRequest request)
    {
        Ra2DefinitionTarget? target = GetDefinitionTarget(request);
        if (target is null)
            return Ra2GoToDefinitionResult.Failure("No definition is available at the current caret position.");

        if (target.Kind == Ra2DefinitionTargetKind.SectionDefinition && target.TargetSpan is Ra2TextSpan span)
        {
            return Ra2GoToDefinitionResult.Jump(
                target,
                span.Start,
                $"Jumped to definition {target.Title} at Line {target.TargetLineNumber}.",
                TrimSectionTitle(target.Title));
        }

        return Ra2GoToDefinitionResult.Preview(
            target,
            $"Opened definition preview for {target.Title}.");
    }

    public Ra2PeekDefinitionResult PeekDefinition(Ra2LanguageNavigationRequest request)
    {
        Ra2DefinitionTarget? target = GetDefinitionTarget(request);
        return target is null
            ? Ra2PeekDefinitionResult.Failure("No definition is available at the current caret position.")
            : Ra2PeekDefinitionResult.Create(target, $"Opened definition preview for {target.Title}.");
    }

    public Ra2FindReferencesNavigationResult FindReferences(Ra2LanguageNavigationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Ra2ReferenceResult result = _referenceFinder.FindReferences(
            request.SemanticModel,
            request.CaretContext,
            request.SelectionSpan);
        return Ra2FindReferencesNavigationResult.Create(
            result,
            $"Found {result.Items.Count} reference(s) for {FormatTargetName(result.TargetName)} in current document.");
    }

    private Ra2DefinitionTarget? GetDefinitionTarget(Ra2LanguageNavigationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _definitionProvider.GetDefinition(
            request.SemanticModel,
            request.CaretContext,
            request.FieldProvider,
            request.ProvenanceProvider);
    }

    private static string FormatTargetName(string targetName)
        => string.IsNullOrWhiteSpace(targetName) ? "current caret" : $"[{targetName}]";

    private static string TrimSectionTitle(string title)
    {
        if (title.Length >= 2 && title[0] == '[' && title[^1] == ']')
            return title[1..^1];

        return title;
    }
}

internal sealed class Ra2LanguageNavigationRequest
{
    public Ra2LanguageNavigationRequest(
        Ra2DocumentSemanticModel semanticModel,
        Ra2CaretContext caretContext,
        IRa2FieldDefinitionProvider fieldProvider,
        IFieldRegistryProvenanceProvider provenanceProvider,
        Ra2TextSpan? selectionSpan = null)
    {
        SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        CaretContext = caretContext ?? throw new ArgumentNullException(nameof(caretContext));
        FieldProvider = fieldProvider ?? throw new ArgumentNullException(nameof(fieldProvider));
        ProvenanceProvider = provenanceProvider ?? throw new ArgumentNullException(nameof(provenanceProvider));
        SelectionSpan = selectionSpan;
    }

    public Ra2DocumentSemanticModel SemanticModel { get; }

    public Ra2CaretContext CaretContext { get; }

    public IRa2FieldDefinitionProvider FieldProvider { get; }

    public IFieldRegistryProvenanceProvider ProvenanceProvider { get; }

    public Ra2TextSpan? SelectionSpan { get; }
}

internal enum Ra2GoToDefinitionAction
{
    None,
    JumpToDefinition,
    ShowPreview,
}

internal sealed class Ra2GoToDefinitionResult
{
    private Ra2GoToDefinitionResult(
        Ra2GoToDefinitionAction action,
        Ra2DefinitionTarget? target,
        int? targetOffset,
        string? sectionName,
        string message)
    {
        Action = action;
        Target = target;
        TargetOffset = targetOffset;
        SectionName = sectionName;
        Message = message;
    }

    public bool Success => Action != Ra2GoToDefinitionAction.None;

    public Ra2GoToDefinitionAction Action { get; }

    public Ra2DefinitionTarget? Target { get; }

    public int? TargetOffset { get; }

    public string? SectionName { get; }

    public string Message { get; }

    public static Ra2GoToDefinitionResult Failure(string message)
        => new(Ra2GoToDefinitionAction.None, null, null, null, message);

    public static Ra2GoToDefinitionResult Jump(
        Ra2DefinitionTarget target,
        int targetOffset,
        string message,
        string? sectionName)
        => new(Ra2GoToDefinitionAction.JumpToDefinition, target, targetOffset, sectionName, message);

    public static Ra2GoToDefinitionResult Preview(Ra2DefinitionTarget target, string message)
        => new(Ra2GoToDefinitionAction.ShowPreview, target, null, null, message);
}

internal sealed class Ra2PeekDefinitionResult
{
    private Ra2PeekDefinitionResult(bool success, Ra2DefinitionTarget? target, string message)
    {
        Success = success;
        Target = target;
        Message = message;
    }

    public bool Success { get; }

    public Ra2DefinitionTarget? Target { get; }

    public string Message { get; }

    public static Ra2PeekDefinitionResult Failure(string message)
        => new(false, null, message);

    public static Ra2PeekDefinitionResult Create(Ra2DefinitionTarget target, string message)
        => new(true, target, message);
}

internal sealed class Ra2FindReferencesNavigationResult
{
    private Ra2FindReferencesNavigationResult(bool success, Ra2ReferenceResult? references, string message)
    {
        Success = success;
        References = references;
        Message = message;
    }

    public bool Success { get; }

    public Ra2ReferenceResult? References { get; }

    public string Message { get; }

    public static Ra2FindReferencesNavigationResult Create(Ra2ReferenceResult references, string message)
        => new(true, references, message);
}
