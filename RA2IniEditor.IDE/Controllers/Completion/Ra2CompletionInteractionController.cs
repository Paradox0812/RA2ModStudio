using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Controllers.Completion;

internal interface IRa2CompletionInteractionController
{
    Ra2CompletionOpenResult OpenCompletions(Ra2CompletionOpenRequest request);

    Ra2CompletionCommitInteractionResult TryCommit(Ra2CompletionCommitInteractionRequest request);
}

internal sealed class Ra2CompletionInteractionController : IRa2CompletionInteractionController
{
    private readonly IRa2CompletionProvider _completionProvider;
    private readonly Ra2CompletionDisplayEnhancer _displayEnhancer;
    private readonly IRa2CompletionCommitCoordinator _commitCoordinator;

    public Ra2CompletionInteractionController(
        IRa2CompletionProvider completionProvider,
        Ra2CompletionDisplayEnhancer displayEnhancer,
        IRa2CompletionCommitCoordinator commitCoordinator)
    {
        _completionProvider = completionProvider ?? throw new ArgumentNullException(nameof(completionProvider));
        _displayEnhancer = displayEnhancer ?? throw new ArgumentNullException(nameof(displayEnhancer));
        _commitCoordinator = commitCoordinator ?? throw new ArgumentNullException(nameof(commitCoordinator));
    }

    public Ra2CompletionOpenResult OpenCompletions(Ra2CompletionOpenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Ra2CompletionResult result = _completionProvider.GetCompletions(new Ra2CompletionRequest(
            request.Model.Snapshot,
            request.Model,
            request.Context,
            request.CaretOffset,
            request.FieldProvider));
        Ra2CompletionResult displayResult = _displayEnhancer.Enhance(
            result,
            request.SectionKind,
            request.FieldDisplayResolver);

        return new Ra2CompletionOpenResult(
            displayResult,
            $"Completion dropdown opened with {displayResult.Items.Count} item(s).");
    }

    public Ra2CompletionCommitInteractionResult TryCommit(Ra2CompletionCommitInteractionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Session is null)
        {
            return Ra2CompletionCommitInteractionResult.Failed(
                "Completion commit skipped: no editable file is currently open.",
                shouldCloseDropdown: true);
        }

        if (request.CompletionResult is null)
        {
            return Ra2CompletionCommitInteractionResult.Failed(
                "Completion commit skipped: completion result is unavailable.",
                shouldCloseDropdown: true);
        }

        if (request.SelectedItem is null)
        {
            return Ra2CompletionCommitInteractionResult.Failed(
                "Completion commit skipped: no completion item is selected.",
                shouldCloseDropdown: true);
        }

        Ra2CompletionCommitApplyResult result = _commitCoordinator.TryCommit(
            request.Session,
            request.CompletionResult,
            request.SelectedItem);
        if (!result.Success || result.Session is null)
        {
            return Ra2CompletionCommitInteractionResult.Failed(
                $"Completion commit failed: {result.ErrorMessage ?? "Unknown error."}",
                shouldCloseDropdown: false);
        }

        return Ra2CompletionCommitInteractionResult.Succeeded(
            result.Session,
            result.CaretOffset,
            $"Committed completion '{request.SelectedLabel ?? request.SelectedItem.Label}' in memory.");
    }
}

internal sealed class Ra2CompletionOpenRequest
{
    public Ra2CompletionOpenRequest(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        int caretOffset,
        IRa2FieldDefinitionProvider fieldProvider,
        Ra2SectionKind sectionKind,
        IRa2FieldDisplayResolver fieldDisplayResolver)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        CaretOffset = caretOffset;
        FieldProvider = fieldProvider ?? throw new ArgumentNullException(nameof(fieldProvider));
        SectionKind = sectionKind;
        FieldDisplayResolver = fieldDisplayResolver ?? throw new ArgumentNullException(nameof(fieldDisplayResolver));
    }

    public Ra2DocumentSemanticModel Model { get; }

    public Ra2CaretContext Context { get; }

    public int CaretOffset { get; }

    public IRa2FieldDefinitionProvider FieldProvider { get; }

    public Ra2SectionKind SectionKind { get; }

    public IRa2FieldDisplayResolver FieldDisplayResolver { get; }
}

internal sealed class Ra2CompletionOpenResult
{
    public Ra2CompletionOpenResult(Ra2CompletionResult completionResult, string message)
    {
        CompletionResult = completionResult ?? throw new ArgumentNullException(nameof(completionResult));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public Ra2CompletionResult CompletionResult { get; }

    public string Message { get; }
}

internal sealed class Ra2CompletionCommitInteractionRequest
{
    public Ra2CompletionCommitInteractionRequest(
        Ra2EditableDocumentSession? session,
        Ra2CompletionResult? completionResult,
        Ra2CompletionItem? selectedItem,
        string? selectedLabel)
    {
        Session = session;
        CompletionResult = completionResult;
        SelectedItem = selectedItem;
        SelectedLabel = selectedLabel;
    }

    public Ra2EditableDocumentSession? Session { get; }

    public Ra2CompletionResult? CompletionResult { get; }

    public Ra2CompletionItem? SelectedItem { get; }

    public string? SelectedLabel { get; }
}

internal sealed class Ra2CompletionCommitInteractionResult
{
    private Ra2CompletionCommitInteractionResult(
        bool success,
        Ra2EditableDocumentSession? session,
        int caretOffset,
        string message,
        bool shouldCloseDropdown)
    {
        Success = success;
        Session = session;
        CaretOffset = caretOffset;
        Message = message;
        ShouldCloseDropdown = shouldCloseDropdown;
    }

    public bool Success { get; }

    public Ra2EditableDocumentSession? Session { get; }

    public int CaretOffset { get; }

    public string Message { get; }

    public bool ShouldCloseDropdown { get; }

    public static Ra2CompletionCommitInteractionResult Failed(string message, bool shouldCloseDropdown)
        => new(false, null, 0, message, shouldCloseDropdown);

    public static Ra2CompletionCommitInteractionResult Succeeded(
        Ra2EditableDocumentSession session,
        int caretOffset,
        string message)
        => new(true, session, caretOffset, message, shouldCloseDropdown: true);
}
