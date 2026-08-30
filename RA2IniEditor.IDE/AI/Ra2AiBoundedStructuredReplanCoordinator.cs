namespace RA2IniEditor.IDE.AI;

internal sealed record Ra2AiBoundedStructuredReplanRequest(
    string UserPrompt,
    Ra2AiContext Context,
    Ra2AiConversationContext? ConversationContext,
    Ra2AiCurrentSubject? CurrentSubject,
    Ra2AiInteractionRoute InteractionRoute,
    Ra2AiContextSourceSet ContextSources);

internal sealed record Ra2AiStructuredRepairAttemptResult(
    Ra2AiRequest Request,
    Ra2AiResponse Response);

internal sealed class Ra2AiBoundedStructuredReplanResult
{
    public Ra2AiBoundedStructuredReplanResult(
        Ra2AiAssistantPipelineResult initialPipelineResult,
        Ra2AiEditProposalResult? initialProposalResult,
        Ra2AiStructuredRepairDecision repairDecision,
        Ra2AiStructuredRepairAttemptResult? repairAttempt,
        Ra2AiEditProposalResult? finalProposalResult)
    {
        InitialPipelineResult = initialPipelineResult ?? throw new ArgumentNullException(nameof(initialPipelineResult));
        InitialProposalResult = initialProposalResult;
        RepairDecision = repairDecision ?? throw new ArgumentNullException(nameof(repairDecision));
        RepairAttempt = repairAttempt;
        FinalProposalResult = finalProposalResult;
        if ((repairAttempt is not null) != RepairDecision.IsEligible)
        {
            throw new ArgumentException(
                "A repair attempt must correspond to an eligible repair decision.",
                nameof(repairAttempt));
        }
    }

    public Ra2AiAssistantPipelineResult InitialPipelineResult { get; }

    public Ra2AiEditProposalResult? InitialProposalResult { get; }

    public Ra2AiStructuredRepairDecision RepairDecision { get; }

    public Ra2AiStructuredRepairAttemptResult? RepairAttempt { get; }

    public bool RepairAttempted => RepairAttempt is not null;

    public Ra2AiRequest FinalRequest => RepairAttempt?.Request ?? InitialPipelineResult.Request;

    public Ra2AiResponse FinalResponse => RepairAttempt?.Response ?? InitialPipelineResult.Response;

    public Ra2AiEditProposalResult? FinalProposalResult { get; }
}

/// <summary>组合现有 Work pipeline 与 canonical proposal runner，并把结构化修复硬限制为一次。</summary>
internal sealed class Ra2AiBoundedStructuredReplanCoordinator
{
    private readonly Ra2AiAssistantPipeline _pipeline;
    private readonly Ra2AiProposalPreparationRunner _proposalRunner;
    private readonly IRa2AiAuthoringContextRecapturePort _recapturePort;

    public Ra2AiBoundedStructuredReplanCoordinator(
        Ra2AiAssistantPipeline pipeline,
        Ra2AiProposalPreparationRunner proposalRunner,
        IRa2AiAuthoringContextRecapturePort recapturePort)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _proposalRunner = proposalRunner ?? throw new ArgumentNullException(nameof(proposalRunner));
        _recapturePort = recapturePort ?? throw new ArgumentNullException(nameof(recapturePort));
    }

    public async Task<Ra2AiBoundedStructuredReplanResult> ExecuteAsync(
        Ra2AiBoundedStructuredReplanRequest request,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Context);
        ArgumentNullException.ThrowIfNull(request.ContextSources);
        ArgumentNullException.ThrowIfNull(onContentDelta);

        Ra2AiAssistantPipelineResult initial = await _pipeline.SendStreamingAsync(
            request.UserPrompt,
            request.Context,
            request.ConversationContext,
            request.CurrentSubject,
            request.InteractionRoute,
            request.ContextSources,
            onContentDelta,
            cancellationToken).ConfigureAwait(false);

        Ra2AiAuthoringRequestContext? originalContext = SelectRequestContext(
            initial.ResolvedInteractionRoute,
            request.ContextSources);
        Ra2AiEditProposalResult? initialProposal = await PrepareProposalIfPresentAsync(
            originalContext,
            initial.Response,
            cancellationToken).ConfigureAwait(false);
        Ra2AiStructuredRepairDecision decision = Ra2AiStructuredRepairPolicy.Evaluate(
            initial.Response,
            initialProposal,
            repairAlreadyAttempted: false);
        if (!decision.IsEligible || decision.Evidence is null)
        {
            return new Ra2AiBoundedStructuredReplanResult(
                initial,
                initialProposal,
                decision,
                repairAttempt: null,
                finalProposalResult: initialProposal);
        }

        if (originalContext is null || initial.ExecutionSeed is null)
        {
            Ra2AiEditProposalResult unavailable = ContextFailure(
                Ra2AiEditProposalFailureKind.RequestContextUnavailable,
                "当前请求没有绑定可修复的编辑快照。");
            return NoRepair(initial, initialProposal, unavailable, "repair-context-unavailable");
        }

        Ra2AiAuthoringContextRecaptureResult preRepairCapture = await _recapturePort.RecaptureAsync(
            originalContext,
            cancellationToken).ConfigureAwait(false);
        if (!preRepairCapture.Succeeded ||
            !Ra2AiAuthoringContextCurrency.Matches(originalContext, preRepairCapture.Context!))
        {
            Ra2AiEditProposalResult stale = ContextFailure(
                preRepairCapture.Succeeded
                    ? Ra2AiEditProposalFailureKind.RequestContextStale
                    : Ra2AiEditProposalFailureKind.RequestContextUnavailable,
                preRepairCapture.Succeeded
                    ? "请求期间当前文档或项目已经变化，本次未执行自动修复。"
                    : preRepairCapture.Message);
            return NoRepair(initial, initialProposal, stale, "repair-context-stale");
        }

        cancellationToken.ThrowIfCancellationRequested();
        Ra2AiStructuredRepairAttemptResult repairAttempt = await _pipeline.SendStructuredRepairAsync(
            initial.ExecutionSeed,
            decision.Evidence,
            cancellationToken).ConfigureAwait(false);

        Ra2AiEditProposalResult? finalProposal = await PrepareProposalIfPresentAsync(
            originalContext,
            repairAttempt.Response,
            cancellationToken).ConfigureAwait(false);
        return new Ra2AiBoundedStructuredReplanResult(
            initial,
            initialProposal,
            decision,
            repairAttempt,
            finalProposal);
    }

    private async Task<Ra2AiEditProposalResult?> PrepareProposalIfPresentAsync(
        Ra2AiAuthoringRequestContext? originalContext,
        Ra2AiResponse response,
        CancellationToken cancellationToken)
    {
        if (response.Kind != Ra2AiResponseKind.ToolCalls)
            return null;
        if (originalContext is null)
        {
            return ContextFailure(
                Ra2AiEditProposalFailureKind.RequestContextUnavailable,
                "当前请求没有绑定可编辑文档快照，结构化修改建议已拒绝。");
        }

        Ra2AiAuthoringContextRecaptureResult capture = await _recapturePort.RecaptureAsync(
            originalContext,
            cancellationToken).ConfigureAwait(false);
        if (!capture.Succeeded)
        {
            return ContextFailure(
                Ra2AiEditProposalFailureKind.RequestContextUnavailable,
                capture.Message);
        }

        return await _proposalRunner.PrepareAsync(
            originalContext,
            capture.Context!,
            response,
            cancellationToken).ConfigureAwait(false);
    }

    internal static Ra2AiAuthoringRequestContext? SelectRequestContext(
        Ra2AiInteractionRoute? resolvedRoute,
        Ra2AiContextSourceSet sources)
        => resolvedRoute is { } route && Ra2AiAuthoringToolCatalog.UsesProjectContext(route.CapabilityMode)
            ? sources.RulesArtProject
            : sources.CurrentDocument;

    private static Ra2AiBoundedStructuredReplanResult NoRepair(
        Ra2AiAssistantPipelineResult initial,
        Ra2AiEditProposalResult? initialProposal,
        Ra2AiEditProposalResult terminalProposal,
        string reason)
        => new(
            initial,
            initialProposal,
            Ra2AiStructuredRepairDecision.NotEligible(reason),
            repairAttempt: null,
            finalProposalResult: terminalProposal);

    private static Ra2AiEditProposalResult ContextFailure(
        Ra2AiEditProposalFailureKind failureKind,
        string message)
        => Ra2AiEditProposalResult.Failed(
            failureKind,
            message,
            Ra2AiStructuredFailureEvidence.FromHost(failureKind, message));
}
