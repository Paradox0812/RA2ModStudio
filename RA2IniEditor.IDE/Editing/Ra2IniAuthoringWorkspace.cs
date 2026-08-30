namespace RA2IniEditor.IDE.Editing;

internal interface IRa2IniAuthoringWorkspace
{
    Ra2IniEditPreview Preview(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        CancellationToken cancellationToken = default);

    Ra2IniEditApplyResult Apply(Ra2IniEditApplyRequest request);

    Ra2ProjectEditPreview PreviewProject(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationProjectEditPlan plan,
        CancellationToken cancellationToken = default);

    Ra2ProjectEditApplyResult ApplyProject(Ra2ProjectEditApplyRequest request);

    bool TryDiscardActivePreview(Guid previewId);

    bool TryDiscardActiveProjectPreview(Guid projectPreviewId);

    void InvalidateActivePreview();
}

/// <summary>
/// 持有一个可消费的活动预览，并把最终提交委托给编辑器事务端口。
/// </summary>
internal sealed class Ra2IniAuthoringWorkspace : IRa2IniAuthoringWorkspace
{
    private readonly IRa2IniEditPreviewService _previewService;
    private readonly IRa2EditorTransactionPort _transactionPort;
    private readonly IRa2ProjectEditPreviewService _projectPreviewService;
    private readonly object _previewGate = new();
    private long _previewGeneration;
    private Ra2IniEditPreview? _activePreview;
    private Ra2ProjectEditPreview? _activeProjectPreview;

    public Ra2IniAuthoringWorkspace(
        IRa2IniEditPreviewService previewService,
        IRa2EditorTransactionPort transactionPort,
        IRa2ProjectEditPreviewService? projectPreviewService = null)
    {
        _previewService = previewService ?? throw new ArgumentNullException(nameof(previewService));
        _transactionPort = transactionPort ?? throw new ArgumentNullException(nameof(transactionPort));
        _projectPreviewService = projectPreviewService ?? new Ra2ProjectEditPreviewService();
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
            _activeProjectPreview = null;
        }

        Ra2IniEditPreview preview;
        try
        {
            preview = _previewService.Preview(snapshot, plan, cancellationToken);
            if (preview is null ||
                !ReferenceEquals(preview.Snapshot, snapshot) ||
                !ReferenceEquals(preview.Plan, plan))
            {
                preview = Ra2IniEditPreview.Failed(
                    snapshot,
                    plan,
                    Ra2IniEditPreviewFailureKind.UnexpectedFailure,
                    "结构化编辑预览未绑定本次 Host 请求。");
            }
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

    public Ra2ProjectEditPreview PreviewProject(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationProjectEditPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);
        long generation;
        lock (_previewGate)
        {
            generation = AdvanceGeneration();
            _activePreview = null;
            _activeProjectPreview = null;
        }

        Ra2ProjectEditPreview preview;
        try
        {
            preview = _projectPreviewService.Preview(snapshot, plan, cancellationToken);
            if (preview is null || !ReferenceEquals(preview.Snapshot, snapshot) || !ReferenceEquals(preview.Plan, plan))
                throw new InvalidOperationException("Project preview was not bound to this Host invocation.");
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            Ra2AutomationProjectEditPreviewResult failed = new Ra2AutomationProjectEditPreviewResult(
                snapshot,
                plan,
                Ra2AutomationProjectEditPreviewFailureKind.UnexpectedFailure,
                "Project edit preview could not be generated.",
                Guid.Empty,
                []);
            preview = Ra2ProjectEditPreview.FromAutomation(snapshot, plan, failed);
        }

        lock (_previewGate)
        {
            if (generation == _previewGeneration && preview.Succeeded)
                _activeProjectPreview = preview;
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

    public Ra2ProjectEditApplyResult ApplyProject(Ra2ProjectEditApplyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ra2ProjectEditPreview preview;
        lock (_previewGate)
        {
            if (_activeProjectPreview is null ||
                request.ProjectPreviewId == Guid.Empty ||
                _activeProjectPreview.ProjectPreviewId != request.ProjectPreviewId)
            {
                return Ra2ProjectEditApplyResult.Failed(
                    Ra2ProjectEditApplyOutcomeKind.PreviewUnavailable,
                    request.ProjectPreviewId,
                    "The project preview is no longer available.");
            }
            if (!request.ExplicitConfirmationGranted)
            {
                return Ra2ProjectEditApplyResult.Failed(
                    Ra2ProjectEditApplyOutcomeKind.ConfirmationRequired,
                    request.ProjectPreviewId,
                    "Explicit confirmation is required before applying a project preview.");
            }
            preview = _activeProjectPreview;
            _activeProjectPreview = null;
            AdvanceGeneration();
        }

        try
        {
            return _transactionPort.ApplyProject(preview) ??
                   Ra2ProjectEditApplyResult.Failed(Ra2ProjectEditApplyOutcomeKind.UnexpectedFailure, preview.ProjectPreviewId, "Project transaction returned no result.");
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            return Ra2ProjectEditApplyResult.Failed(Ra2ProjectEditApplyOutcomeKind.UnexpectedFailure, preview.ProjectPreviewId, "Project transaction failed unexpectedly.");
        }
        finally
        {
            lock (_previewGate)
            {
                _activeProjectPreview = null;
                AdvanceGeneration();
            }
        }
    }

    public void InvalidateActivePreview()
    {
        lock (_previewGate)
        {
            _activePreview = null;
            _activeProjectPreview = null;
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

    public bool TryDiscardActiveProjectPreview(Guid projectPreviewId)
    {
        if (projectPreviewId == Guid.Empty)
            return false;

        lock (_previewGate)
        {
            if (_activeProjectPreview?.ProjectPreviewId != projectPreviewId)
                return false;

            _activeProjectPreview = null;
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
