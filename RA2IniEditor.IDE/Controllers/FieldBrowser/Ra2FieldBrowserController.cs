using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels.FieldBrowser;

namespace RA2IniEditor.IDE.Controllers.FieldBrowser;

internal interface IRa2FieldBrowserController
{
    Ra2AddPropertyOpenResult CreateAddPropertyViewModel(Ra2AddPropertyOpenRequest request);

    Ra2AddPropertyConfirmationResult ConfirmAddProperty(Ra2AddPropertyConfirmationRequest request);

    Ra2FieldBrowserActionResult ApplyInsertDuplicate(Ra2AddPropertyApplyRequest request);

    Ra2FieldBrowserActionResult ApplyReplaceExisting(Ra2AddPropertyReplaceApplyRequest request);
}

internal sealed class Ra2FieldBrowserController : IRa2FieldBrowserController
{
    private readonly Ra2AddPropertyInsertPlanner _insertPlanner;
    private readonly IRa2TextChangeApplier _textChangeApplier;

    public Ra2FieldBrowserController(
        Ra2AddPropertyInsertPlanner insertPlanner,
        IRa2TextChangeApplier textChangeApplier)
    {
        _insertPlanner = insertPlanner ?? throw new ArgumentNullException(nameof(insertPlanner));
        _textChangeApplier = textChangeApplier ?? throw new ArgumentNullException(nameof(textChangeApplier));
    }

    public Ra2AddPropertyOpenResult CreateAddPropertyViewModel(Ra2AddPropertyOpenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Ra2FieldAnnotationLoadResult annotationLoadResult = request.AnnotationStore.Load(request.AnnotationPath);
        Ra2AddPropertyViewModel viewModel = new(
            new Ra2FieldDisplayResolver(
                request.FieldProvider,
                new Ra2FieldAnnotationProvider(annotationLoadResult.Pack)),
            request.InitialSectionKind,
            request.EditorState,
            annotationStatus: Ra2FieldAnnotationStatusViewModel.FromLoadResult(
                request.AnnotationPath,
                annotationLoadResult),
            recentFieldUsageTracker: request.RecentFieldUsageTracker,
            document: request.Document,
            caretOffset: request.CaretOffset);

        return new Ra2AddPropertyOpenResult(viewModel, annotationLoadResult);
    }

    public Ra2AddPropertyConfirmationResult ConfirmAddProperty(Ra2AddPropertyConfirmationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ra2AddPropertyViewModel viewModel = request.ViewModel;

        if (viewModel.SelectedDuplicateAction == Ra2DuplicateKeyAction.Cancel)
        {
            return Ra2AddPropertyConfirmationResult.Create(
                Ra2AddPropertyConfirmationAction.Cancelled,
                message: "Add property cancelled.");
        }

        if (viewModel.SelectedDuplicateAction == Ra2DuplicateKeyAction.JumpExisting &&
            viewModel.DuplicateAction.Match is { } jumpMatch)
        {
            return Ra2AddPropertyConfirmationResult.Create(
                Ra2AddPropertyConfirmationAction.JumpExisting,
                jumpMatch,
                $"Jumped to existing field '{jumpMatch.Key}' at line {jumpMatch.LineNumber}.");
        }

        if (!request.HasEditableSession)
        {
            return Ra2AddPropertyConfirmationResult.Create(
                Ra2AddPropertyConfirmationAction.RequiresEditMode,
                message: "Add property skipped: no editable file is currently open.");
        }

        if (viewModel.SelectedDuplicateAction == Ra2DuplicateKeyAction.ReplaceExisting &&
            viewModel.DuplicateAction.Match is { } replaceMatch)
        {
            return Ra2AddPropertyConfirmationResult.Create(
                Ra2AddPropertyConfirmationAction.ReplaceExisting,
                replaceMatch);
        }

        return Ra2AddPropertyConfirmationResult.Create(Ra2AddPropertyConfirmationAction.InsertDuplicate);
    }

    public Ra2FieldBrowserActionResult ApplyInsertDuplicate(Ra2AddPropertyApplyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Ra2AddPropertyInsertPlan plan = _insertPlanner.PlanInsertDuplicate(
                request.Session.TextDocument,
                request.CaretOffset,
                request.ViewModel.OptionText,
                request.ViewModel.ValueText);
            string warningSuffix = plan.Warnings.Count == 0 ? string.Empty : $" {plan.Warnings[0]}";
            return ApplyPlan(
                request.Session,
                plan,
                $"Added field '{request.ViewModel.OptionText}' in memory.{warningSuffix}",
                "Add property");
        }
        catch (ArgumentException ex)
        {
            return Ra2FieldBrowserActionResult.Failed($"Add property failed: {ex.Message}");
        }
    }

    public Ra2FieldBrowserActionResult ApplyReplaceExisting(Ra2AddPropertyReplaceApplyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Ra2AddPropertyInsertPlan plan = _insertPlanner.PlanReplaceExisting(
                request.Match,
                request.ViewModel.OptionText,
                request.ViewModel.ValueText);
            return ApplyPlan(
                request.Session,
                plan,
                $"Replaced field '{request.ViewModel.OptionText}' in memory.",
                "Replace property");
        }
        catch (ArgumentException ex)
        {
            return Ra2FieldBrowserActionResult.Failed($"Replace property failed: {ex.Message}");
        }
    }

    private Ra2FieldBrowserActionResult ApplyPlan(
        Ra2EditableDocumentSession session,
        Ra2AddPropertyInsertPlan plan,
        string successMessage,
        string failureActionName)
    {
        Ra2TextChangeApplyResult applyResult = _textChangeApplier.Apply(session.DocumentState, plan.Change);
        if (!applyResult.Success || applyResult.DocumentState is null || applyResult.TextDocument is null)
        {
            string error = applyResult.ErrorMessage ?? "Unknown error.";
            return Ra2FieldBrowserActionResult.Failed($"{failureActionName} failed: {error}");
        }

        Ra2EditableDocumentSession updatedSession = session.ContinueWith(
            applyResult.DocumentState,
            applyResult.TextDocument);
        return Ra2FieldBrowserActionResult.Succeeded(
            updatedSession,
            applyResult.DocumentState.CurrentText,
            plan.CaretOffset,
            successMessage);
    }
}

internal sealed class Ra2AddPropertyOpenRequest
{
    public Ra2AddPropertyOpenRequest(
        IRa2FieldDefinitionProvider fieldProvider,
        IRa2FieldAnnotationStore annotationStore,
        string annotationPath,
        Ra2SectionKind? initialSectionKind,
        Ra2EditorDocumentState editorState,
        Ra2RecentFieldUsageTracker recentFieldUsageTracker,
        Ra2IniTextDocument document,
        int caretOffset)
    {
        FieldProvider = fieldProvider ?? throw new ArgumentNullException(nameof(fieldProvider));
        AnnotationStore = annotationStore ?? throw new ArgumentNullException(nameof(annotationStore));
        AnnotationPath = annotationPath ?? throw new ArgumentNullException(nameof(annotationPath));
        InitialSectionKind = initialSectionKind;
        EditorState = editorState;
        RecentFieldUsageTracker = recentFieldUsageTracker ?? throw new ArgumentNullException(nameof(recentFieldUsageTracker));
        Document = document ?? throw new ArgumentNullException(nameof(document));
        CaretOffset = caretOffset;
    }

    public IRa2FieldDefinitionProvider FieldProvider { get; }

    public IRa2FieldAnnotationStore AnnotationStore { get; }

    public string AnnotationPath { get; }

    public Ra2SectionKind? InitialSectionKind { get; }

    public Ra2EditorDocumentState EditorState { get; }

    public Ra2RecentFieldUsageTracker RecentFieldUsageTracker { get; }

    public Ra2IniTextDocument Document { get; }

    public int CaretOffset { get; }
}

internal sealed class Ra2AddPropertyOpenResult
{
    public Ra2AddPropertyOpenResult(
        Ra2AddPropertyViewModel viewModel,
        Ra2FieldAnnotationLoadResult annotationLoadResult)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        AnnotationLoadResult = annotationLoadResult ?? throw new ArgumentNullException(nameof(annotationLoadResult));
    }

    public Ra2AddPropertyViewModel ViewModel { get; }

    public Ra2FieldAnnotationLoadResult AnnotationLoadResult { get; }
}

internal sealed class Ra2AddPropertyConfirmationRequest
{
    public Ra2AddPropertyConfirmationRequest(
        Ra2AddPropertyViewModel viewModel,
        bool hasEditableSession)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        HasEditableSession = hasEditableSession;
    }

    public Ra2AddPropertyViewModel ViewModel { get; }

    public bool HasEditableSession { get; }
}

internal enum Ra2AddPropertyConfirmationAction
{
    None,
    Cancelled,
    JumpExisting,
    RequiresEditMode,
    ReplaceExisting,
    InsertDuplicate
}

internal sealed class Ra2AddPropertyConfirmationResult
{
    private Ra2AddPropertyConfirmationResult(
        Ra2AddPropertyConfirmationAction action,
        Ra2DuplicateKeyMatch? match,
        string? message)
    {
        Action = action;
        Match = match;
        Message = message;
    }

    public Ra2AddPropertyConfirmationAction Action { get; }

    public Ra2DuplicateKeyMatch? Match { get; }

    public string? Message { get; }

    public static Ra2AddPropertyConfirmationResult Create(
        Ra2AddPropertyConfirmationAction action,
        Ra2DuplicateKeyMatch? match = null,
        string? message = null)
        => new(action, match, message);
}

internal sealed class Ra2AddPropertyApplyRequest
{
    public Ra2AddPropertyApplyRequest(
        Ra2AddPropertyViewModel viewModel,
        Ra2EditableDocumentSession session,
        int caretOffset)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Session = session ?? throw new ArgumentNullException(nameof(session));
        CaretOffset = caretOffset;
    }

    public Ra2AddPropertyViewModel ViewModel { get; }

    public Ra2EditableDocumentSession Session { get; }

    public int CaretOffset { get; }
}

internal sealed class Ra2AddPropertyReplaceApplyRequest
{
    public Ra2AddPropertyReplaceApplyRequest(
        Ra2AddPropertyViewModel viewModel,
        Ra2EditableDocumentSession session,
        Ra2DuplicateKeyMatch match)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Match = match ?? throw new ArgumentNullException(nameof(match));
    }

    public Ra2AddPropertyViewModel ViewModel { get; }

    public Ra2EditableDocumentSession Session { get; }

    public Ra2DuplicateKeyMatch Match { get; }
}

internal sealed class Ra2FieldBrowserActionResult
{
    private Ra2FieldBrowserActionResult(
        bool success,
        Ra2EditableDocumentSession? updatedSession,
        string? updatedText,
        int? caretOffset,
        string message)
    {
        Success = success;
        UpdatedSession = updatedSession;
        UpdatedText = updatedText;
        CaretOffset = caretOffset;
        Message = message;
    }

    public bool Success { get; }

    public Ra2EditableDocumentSession? UpdatedSession { get; }

    public string? UpdatedText { get; }

    public int? CaretOffset { get; }

    public string Message { get; }

    public static Ra2FieldBrowserActionResult Succeeded(
        Ra2EditableDocumentSession updatedSession,
        string updatedText,
        int caretOffset,
        string message)
    {
        ArgumentNullException.ThrowIfNull(updatedSession);
        ArgumentNullException.ThrowIfNull(updatedText);
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Result message cannot be empty.", nameof(message));

        return new Ra2FieldBrowserActionResult(true, updatedSession, updatedText, caretOffset, message);
    }

    public static Ra2FieldBrowserActionResult Failed(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Result message cannot be empty.", nameof(message));

        return new Ra2FieldBrowserActionResult(false, null, null, null, message);
    }
}
