using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels.Language;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.Controllers.Hover;

internal interface IRa2SourceEditorHoverController
{
    Ra2SourceEditorHoverPointerMoveResult OnPointerMoved(
        bool isCompletionDropdownOpen,
        int? documentOffset,
        bool isDelayTimerEnabled);

    int? ConsumePendingOffset();

    void MarkHoverShown(int documentOffset);

    Ra2SourceEditorHoverResolveResult ResolveHover(Ra2SourceEditorHoverRequest request);

    void Reset();
}

internal sealed class Ra2SourceEditorHoverController : IRa2SourceEditorHoverController
{
    private const int KeyHitPadding = 4;
    private readonly IRa2HoverProvider _hoverProvider;
    private int? _pendingOffset;
    private int? _activeOffset;

    public Ra2SourceEditorHoverController(IRa2HoverProvider hoverProvider)
    {
        _hoverProvider = hoverProvider ?? throw new ArgumentNullException(nameof(hoverProvider));
    }

    public Ra2SourceEditorHoverPointerMoveResult OnPointerMoved(
        bool isCompletionDropdownOpen,
        int? documentOffset,
        bool isDelayTimerEnabled)
    {
        if (isCompletionDropdownOpen || documentOffset is null)
        {
            Reset();
            return Ra2SourceEditorHoverPointerMoveResult.Close;
        }

        if (_activeOffset == documentOffset.Value)
            return Ra2SourceEditorHoverPointerMoveResult.Ignore;

        if (_pendingOffset == documentOffset.Value && isDelayTimerEnabled)
            return Ra2SourceEditorHoverPointerMoveResult.Ignore;

        _activeOffset = null;
        _pendingOffset = documentOffset.Value;
        return Ra2SourceEditorHoverPointerMoveResult.StartDelay;
    }

    public int? ConsumePendingOffset()
        => _pendingOffset;

    public void MarkHoverShown(int documentOffset)
    {
        _activeOffset = documentOffset;
        _pendingOffset = null;
    }

    public Ra2SourceEditorHoverResolveResult ResolveHover(Ra2SourceEditorHoverRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!TryCreateHoverContext(request, out Ra2CaretContext hoverContext))
            return Ra2SourceEditorHoverResolveResult.Empty;

        Ra2HoverInfo? hover = _hoverProvider.GetHover(
            request.Model,
            hoverContext,
            request.FieldDisplayResolver,
            request.ProvenanceProvider);
        if (hover is null)
            return Ra2SourceEditorHoverResolveResult.Empty;

        Ra2HoverDisplayViewModel display = Ra2HoverDisplayViewModel.FromHoverInfo(hover);
        return Ra2SourceEditorHoverResolveResult.Create(display);
    }

    public void Reset()
    {
        _pendingOffset = null;
        _activeOffset = null;
    }

    private static bool TryCreateHoverContext(
        Ra2SourceEditorHoverRequest request,
        out Ra2CaretContext hoverContext)
    {
        hoverContext = null!;
        if (request.Context.Region == Ra2CaretRegion.Value &&
            request.Model.References.Any(reference => reference.ValueSpan.Contains(request.Offset)))
        {
            hoverContext = request.Context;
            return true;
        }

        return TryCreateKeyHoverContext(request.Context, request.Offset, out hoverContext);
    }

    private static bool TryCreateKeyHoverContext(
        Ra2CaretContext context,
        int offset,
        out Ra2CaretContext keyHoverContext)
    {
        keyHoverContext = null!;
        if (context.KeyValue is not Ra2KeyValueSymbol keyValue)
            return false;

        if (context.Region == Ra2CaretRegion.Key || IsKeyHoverHitCandidate(offset, keyValue))
        {
            keyHoverContext = new Ra2CaretContext(
                offset,
                Ra2CaretRegion.Key,
                context.Section,
                keyValue,
                keyValue.Key,
                keyValue.KeySpan);
            return true;
        }

        return false;
    }

    private static bool IsKeyHoverHitCandidate(int offset, Ra2KeyValueSymbol keyValue)
    {
        int start = Math.Max(keyValue.LineSpan.Start, keyValue.KeySpan.Start - 1);
        int paddedEnd = Math.Min(
            keyValue.LineSpan.End,
            keyValue.KeySpan.End + KeyHitPadding);
        int end = keyValue.ValueSpan is Ra2TextSpan valueSpan
            ? Math.Min(paddedEnd, Math.Max(keyValue.KeySpan.End, valueSpan.Start - 1))
            : paddedEnd;

        return offset >= start && offset <= end;
    }
}

internal sealed class Ra2SourceEditorHoverRequest
{
    public Ra2SourceEditorHoverRequest(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        int offset,
        IRa2FieldDisplayResolver fieldDisplayResolver,
        IFieldRegistryProvenanceProvider provenanceProvider)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Offset = offset;
        FieldDisplayResolver = fieldDisplayResolver ?? throw new ArgumentNullException(nameof(fieldDisplayResolver));
        ProvenanceProvider = provenanceProvider ?? throw new ArgumentNullException(nameof(provenanceProvider));
    }

    public Ra2DocumentSemanticModel Model { get; }

    public Ra2CaretContext Context { get; }

    public int Offset { get; }

    public IRa2FieldDisplayResolver FieldDisplayResolver { get; }

    public IFieldRegistryProvenanceProvider ProvenanceProvider { get; }
}

internal enum Ra2SourceEditorHoverPointerMoveAction
{
    Ignore,
    Close,
    StartDelay
}

internal sealed class Ra2SourceEditorHoverPointerMoveResult
{
    private Ra2SourceEditorHoverPointerMoveResult(Ra2SourceEditorHoverPointerMoveAction action)
    {
        Action = action;
    }

    public Ra2SourceEditorHoverPointerMoveAction Action { get; }

    public static Ra2SourceEditorHoverPointerMoveResult Ignore { get; } =
        new(Ra2SourceEditorHoverPointerMoveAction.Ignore);

    public static Ra2SourceEditorHoverPointerMoveResult Close { get; } =
        new(Ra2SourceEditorHoverPointerMoveAction.Close);

    public static Ra2SourceEditorHoverPointerMoveResult StartDelay { get; } =
        new(Ra2SourceEditorHoverPointerMoveAction.StartDelay);
}

internal sealed class Ra2SourceEditorHoverResolveResult
{
    private Ra2SourceEditorHoverResolveResult(bool success, Ra2HoverDisplayViewModel? display)
    {
        Success = success;
        Display = display;
        ToolTipText = display?.ToToolTipText();
    }

    public bool Success { get; }

    public Ra2HoverDisplayViewModel? Display { get; }

    public string? ToolTipText { get; }

    public static Ra2SourceEditorHoverResolveResult Empty { get; } = new(false, null);

    public static Ra2SourceEditorHoverResolveResult Create(Ra2HoverDisplayViewModel display)
        => new(true, display ?? throw new ArgumentNullException(nameof(display)));
}
