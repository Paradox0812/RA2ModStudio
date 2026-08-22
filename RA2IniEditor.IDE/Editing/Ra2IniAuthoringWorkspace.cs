namespace RA2IniEditor.IDE.Editing;

internal interface IRa2IniAuthoringWorkspace
{
    Ra2IniEditPreview Preview(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        CancellationToken cancellationToken = default);

    Ra2IniEditApplyResult Apply(Ra2IniEditApplyRequest request);

    bool TryDiscardActivePreview(Guid previewId);

    void InvalidateActivePreview();
}

/// <summary>
/// 持有一个可消费的活动预览，并把最终提交委托给编辑器事务端口。
/// </summary>
internal sealed class Ra2IniAuthoringWorkspace : IRa2IniAuthoringWorkspace
{
    private readonly IRa2IniEditPreviewService _previewService;
    private readonly IRa2EditorTransactionPort _transactionPort;
    private readonly object _previewGate = new();
    private long _previewGeneration;
    private Ra2IniEditPreview? _activePreview;

    public Ra2IniAuthoringWorkspace(
        IRa2IniEditPreviewService previewService,
        IRa2EditorTransactionPort transactionPort)
    {
        _previewService = previewService ?? throw new ArgumentNullException(nameof(previewService));
        _transactionPort = transactionPort ?? throw new ArgumentNullException(nameof(transactionPort));
    }

    public Ra2IniEditPreview Preview(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);

        long generation;
        lock (_previewGate)
        {
            generation = AdvanceGeneration();
            _activePreview = null;
        }

        Ra2IniEditPreview preview;
        try
        {
            preview = _previewService.Preview(snapshot, plan, cancellationToken);
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            preview = Ra2IniEditPreview.Failed(
                snapshot,
                plan,
                Ra2IniEditPreviewFailureKind.UnexpectedFailure,
                "无法生成结构化编辑预览。");
        }

        lock (_previewGate)
        {
            if (generation == _previewGeneration && preview.Succeeded)
                _activePreview = preview;
        }

        return preview;
    }

    public Ra2IniEditApplyResult Apply(Ra2IniEditApplyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Ra2IniEditPreview preview;
        lock (_previewGate)
        {
            if (_activePreview is null ||
                request.PreviewId == Guid.Empty ||
                _activePreview.PreviewId != request.PreviewId)
            {
                return Ra2IniEditApplyResult.PreviewUnavailable(request.PreviewId);
            }

            if (!request.ExplicitConfirmationGranted)
                return Ra2IniEditApplyResult.ConfirmationRequired(request.PreviewId);

            preview = _activePreview;
            _activePreview = null;
            AdvanceGeneration();
        }

        try
        {
            return _transactionPort.Apply(preview) ??
                   Ra2IniEditApplyResult.UnexpectedFailure(preview.PreviewId);
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            return Ra2IniEditApplyResult.UnexpectedFailure(preview.PreviewId);
        }
        finally
        {
            lock (_previewGate)
            {
                _activePreview = null;
                AdvanceGeneration();
            }
        }
    }

    public void InvalidateActivePreview()
    {
        lock (_previewGate)
        {
            _activePreview = null;
            AdvanceGeneration();
        }
    }

    public bool TryDiscardActivePreview(Guid previewId)
    {
        if (previewId == Guid.Empty)
            return false;

        lock (_previewGate)
        {
            if (_activePreview?.PreviewId != previewId)
                return false;

            _activePreview = null;
            AdvanceGeneration();
            return true;
        }
    }

    private long AdvanceGeneration()
        => _previewGeneration = unchecked(_previewGeneration + 1);

    private static bool IsFatal(Exception exception)
        => exception is OutOfMemoryException
            or AccessViolationException
            or AppDomainUnloadedException
            or BadImageFormatException
            or StackOverflowException;
}
