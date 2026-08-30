using System.IO;
using RA2IniEditor.AssetHost;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelGenerationSessionState
{
    Empty = 0,
    Ready,
    Probing,
    AwaitingConsent,
    Generating,
    AdoptingArtifact,
    Converting,
    CandidateReady,
    Failed,
    Canceled,
    TimedOut,
    Stale
}

internal sealed record Ra2VoxelGenerationInput(
    string ProjectRoot,
    string ReferenceImagePath,
    string? PalettePath,
    string DesignBrief,
    string NegativeConstraints,
    int TargetResolution,
    TimeSpan Timeout);

internal sealed record Ra2VoxelGenerationSession(
    Guid SessionId,
    Ra2VoxelGenerationSessionState State,
    string Message,
    Ra2VoxelGenerationInput Input,
    string ProviderId,
    string ModelId,
    Ra2VoxelStyleSourceLoadResult? Candidate)
{
    internal bool CanConfirm => State == Ra2VoxelGenerationSessionState.AwaitingConsent;
    internal bool IsSuccess => State == Ra2VoxelGenerationSessionState.CandidateReady && Candidate?.IsSuccess == true;
}

internal interface IRa2MeshGenerationGateway
{
    ValueTask<Ra2MeshGenerationResult> ProbeAsync(CancellationToken cancellationToken);
    ValueTask<Ra2MeshGenerationResult> GenerateAsync(
        Ra2MeshGenerationRequest request,
        IProgress<Ra2MeshGenerationProgress>? progress,
        CancellationToken cancellationToken);
}

internal sealed class Ra2BundledMeshGenerationGateway : IRa2MeshGenerationGateway
{
    private readonly Ra2MeshGenerationFacade _facade;

    internal Ra2BundledMeshGenerationGateway(string projectRoot)
    {
        string workspace = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RA2IniEditor", "AssetHost", "Runs");
        _facade = Ra2MeshGenerationFacade.CreateFromBundle(
            Path.Combine(AppContext.BaseDirectory, "Providers", "TencentHy3D", "provider.bundle.json"),
            workspace,
            [projectRoot],
            licenseAccepted: true);
    }

    public ValueTask<Ra2MeshGenerationResult> ProbeAsync(CancellationToken cancellationToken) =>
        _facade.ProbeAsync(cancellationToken);

    public ValueTask<Ra2MeshGenerationResult> GenerateAsync(
        Ra2MeshGenerationRequest request,
        IProgress<Ra2MeshGenerationProgress>? progress,
        CancellationToken cancellationToken) =>
        _facade.GenerateAsync(request, progress, cancellationToken);
}

/// <summary>Coordinates one explicit image-driven generation job. It never persists artifacts.</summary>
internal sealed class Ra2VoxelGenerationOrchestrator
{
    private const int MaximumReferenceBytes = 6 * 1024 * 1024;
    private readonly Ra2VoxelStylePreviewCoordinator _previewCoordinator;
    private readonly Func<string, IRa2MeshGenerationGateway> _gatewayFactory;

    internal Ra2VoxelGenerationOrchestrator(
        Ra2VoxelStylePreviewCoordinator previewCoordinator,
        Func<string, IRa2MeshGenerationGateway>? gatewayFactory = null)
    {
        _previewCoordinator = previewCoordinator ?? throw new ArgumentNullException(nameof(previewCoordinator));
        _gatewayFactory = gatewayFactory ?? (root => new Ra2BundledMeshGenerationGateway(root));
    }

    internal async Task<Ra2VoxelGenerationSession> PrepareAsync(
        Ra2VoxelGenerationInput input,
        CancellationToken cancellationToken)
    {
        Guid id = Guid.NewGuid();
        if (!TryReadReference(input, out _, out _, out string failure))
            return new(id, Ra2VoxelGenerationSessionState.Failed, failure, input, string.Empty, string.Empty, null);
        try
        {
            Ra2MeshGenerationResult probe = await _gatewayFactory(input.ProjectRoot)
                .ProbeAsync(cancellationToken).ConfigureAwait(false);
            if (!probe.Succeeded)
                return FromFailure(id, input, probe);
            return new(id, Ra2VoxelGenerationSessionState.AwaitingConsent,
                "Provider 已就绪；需要确认后才会发送参考图并创建一次生成任务。",
                input, probe.ProviderId, probe.ModelId, null);
        }
        catch (OperationCanceledException)
        {
            return new(id, Ra2VoxelGenerationSessionState.Canceled, "生成准备已取消。", input, string.Empty, string.Empty, null);
        }
    }

    internal async Task<Ra2VoxelGenerationSession> GenerateAsync(
        Ra2VoxelGenerationSession prepared,
        bool consentConfirmed,
        Ra2VoxelStyleSourceLoadResult? paletteSource,
        IProgress<Ra2MeshGenerationProgress>? progress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(prepared);
        if (!prepared.CanConfirm)
            return prepared with { State = Ra2VoxelGenerationSessionState.Failed, Message = "生成会话未处于待确认状态。" };
        if (!consentConfirmed)
            return prepared with { State = Ra2VoxelGenerationSessionState.Canceled, Message = "用户未确认发送参考图；未创建远程任务。" };
        if (!TryReadReference(prepared.Input, out byte[]? bytes, out Ra2ReferenceImageFormat format, out string failure))
            return prepared with { State = Ra2VoxelGenerationSessionState.Failed, Message = failure };

        try
        {
            var request = new Ra2MeshGenerationRequest(
                Path.GetFileName(prepared.Input.ReferenceImagePath),
                format,
                bytes,
                prepared.Input.DesignBrief,
                prepared.Input.NegativeConstraints,
                prepared.Input.Timeout,
                seed: 1);
            Ra2MeshGenerationResult generated = await _gatewayFactory(prepared.Input.ProjectRoot)
                .GenerateAsync(request, progress, cancellationToken).ConfigureAwait(false);
            if (!generated.Succeeded || !generated.HasArtifact)
                return FromFailure(prepared.SessionId, prepared.Input, generated);

            Ra2VoxelStyleSourceLoadResult candidate = await Task.Run(
                () => _previewCoordinator.ConvertGeneratedGlb(
                    prepared.Input.ProjectRoot,
                    generated.MeshGlb,
                    prepared.Input.PalettePath,
                    paletteSource,
                    prepared.Input.TargetResolution,
                    cancellationToken),
                CancellationToken.None).ConfigureAwait(false);
            if (!candidate.IsSuccess)
                return prepared with { State = Ra2VoxelGenerationSessionState.Failed, Message = candidate.Message };
            return prepared with
            {
                State = Ra2VoxelGenerationSessionState.CandidateReady,
                Message = "生成结果已转换为会话内体素候选；尚未写入任何文件。",
                Candidate = candidate
            };
        }
        catch (OperationCanceledException)
        {
            return prepared with { State = Ra2VoxelGenerationSessionState.Canceled, Message = "模型生成已取消。" };
        }
    }

    private static bool TryReadReference(
        Ra2VoxelGenerationInput input,
        out byte[]? bytes,
        out Ra2ReferenceImageFormat format,
        out string failure)
    {
        bytes = null;
        format = default;
        failure = string.Empty;
        if (string.IsNullOrWhiteSpace(input.ProjectRoot) || !Directory.Exists(input.ProjectRoot))
            failure = "请先打开一个有效项目。";
        else if (string.IsNullOrWhiteSpace(input.ReferenceImagePath))
            failure = "请选择一张 PNG 或 JPEG 参考图。";
        else
        {
            try
            {
                FileInfo file = new(Path.GetFullPath(input.ReferenceImagePath));
                if (!file.Exists || (file.Attributes & FileAttributes.ReparsePoint) != 0 || file.Length is < 1 or > MaximumReferenceBytes)
                    failure = "参考图不存在、是链接，或超过 6 MiB 上限。";
                else
                {
                    bytes = File.ReadAllBytes(file.FullName);
                    string extension = file.Extension;
                    if (string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) && bytes.Length >= 8 &&
                        bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
                        format = Ra2ReferenceImageFormat.Png;
                    else if ((string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)) &&
                             bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                        format = Ra2ReferenceImageFormat.Jpeg;
                    else
                        failure = "参考图扩展名与 PNG/JPEG 文件签名不一致。";
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                failure = "无法安全读取参考图。";
            }
        }

        if (failure.Length == 0 && input.TargetResolution is not (32 or 48 or 64 or 96 or 128))
            failure = "体素分辨率必须为 32、48、64、96 或 128。";
        if (failure.Length == 0 && (input.Timeout < TimeSpan.FromMinutes(1) || input.Timeout > TimeSpan.FromMinutes(20)))
            failure = "生成超时必须在 1 至 20 分钟之间。";
        return failure.Length == 0;
    }

    private static Ra2VoxelGenerationSession FromFailure(
        Guid id,
        Ra2VoxelGenerationInput input,
        Ra2MeshGenerationResult result) =>
        new(id,
            result.FailureKind == Ra2MeshGenerationFailureKind.Canceled
                ? Ra2VoxelGenerationSessionState.Canceled
                : result.FailureKind == Ra2MeshGenerationFailureKind.TimedOut
                    ? Ra2VoxelGenerationSessionState.TimedOut
                    : Ra2VoxelGenerationSessionState.Failed,
            string.IsNullOrWhiteSpace(result.Message) ? "模型生成未能完成。" : result.Message,
            input,
            result.ProviderId,
            result.ModelId,
            null);
}
