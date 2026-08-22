using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.AI;

/// <summary>在后台安全准备结构化预览，并把所有非致命终态转换为显式结果。</summary>
internal sealed class Ra2AiProposalPreparationRunner
{
    private readonly Ra2AiAuthoringCoordinator _coordinator;

    internal Ra2AiProposalPreparationRunner(Ra2AiAuthoringCoordinator coordinator)
        => _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));

    internal async Task<Ra2AiEditProposalResult> PrepareAsync(
        Ra2AiAuthoringRequestContext requestContext,
        Ra2AuthoringSnapshot currentSnapshot,
        Ra2AiResponse response,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestContext);
        ArgumentNullException.ThrowIfNull(currentSnapshot);
        ArgumentNullException.ThrowIfNull(response);

        try
        {
            return await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return Cancelled();

                return _coordinator.PrepareProposal(
                    requestContext,
                    currentSnapshot,
                    response,
                    cancellationToken);
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return Cancelled();
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            return Ra2AiEditProposalResult.Failed(
                Ra2AiEditProposalFailureKind.UnexpectedFailure,
                "生成结构化修改预览时发生未预期错误。");
        }
    }

    private static Ra2AiEditProposalResult Cancelled()
        => Ra2AiEditProposalResult.Failed(
            Ra2AiEditProposalFailureKind.PreviewCancelled,
            "已取消生成结构化修改预览。");

    private static bool IsFatal(Exception exception)
        => exception is OutOfMemoryException
            or AccessViolationException
            or AppDomainUnloadedException
            or BadImageFormatException
            or StackOverflowException;
}
