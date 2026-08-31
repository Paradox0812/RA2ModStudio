extern alias Ra2Application;

using System.IO;
using System.Security.Cryptography;
using System.Text;
using RA2IniEditor.IDE.AI;
using Ra2CompiledVoxelStylePlan = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2CompiledVoxelStylePlan;
using Ra2GlbMeshReader = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2GlbMeshReader;
using Ra2MagicaVoxelCodec = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2MagicaVoxelCodec;
using Ra2MeshSnapshot = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2MeshSnapshot;
using Ra2MeshVoxelizationException = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2MeshVoxelizationException;
using Ra2MeshVoxelizationOptions = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2MeshVoxelizationOptions;
using Ra2MeshVoxelizer = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2MeshVoxelizer;
using Ra2Rgb24 = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2Rgb24;
using Ra2Rgba32 = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2Rgba32;
using Ra2VoxelAssemblyPartRole = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelAssemblyPartRole;
using Ra2VoxelColourReviewPackageBuilder = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourReviewPackageBuilder;
using Ra2VoxelColourReviewPackageResult = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourReviewPackageResult;
using Ra2VoxelColourReviewFlags = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourReviewFlags;
using Ra2VoxelColourizationFacts = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourizationFacts;
using Ra2VoxelColourizer = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourizer;
using Ra2VoxelGeometryRegionMask = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryRegionMask;
using Ra2VoxelGeometryQualityFacts = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryQualityFacts;
using Ra2VoxelReviewArtifact = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelReviewArtifact;
using Ra2VoxelPaletteProfile = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelPaletteProfile;
using Ra2VoxelPaletteContrastFacts = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelPaletteContrastFacts;
using Ra2VoxelPaletteContrastOptimizer = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelPaletteContrastOptimizer;
using Ra2VoxelQualityRefinementResult = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelQualityRefinementResult;
using Ra2VoxelRefinementFailureKind = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelRefinementFailureKind;
using Ra2VoxelQualityAnalyzer = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelQualityAnalyzer;
using Ra2VoxelQualityRefiner = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelQualityRefiner;
using Ra2VoxelRefinementProfile = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelRefinementProfile;
using Ra2VoxelRefinementReviewPackage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelRefinementReviewPackage;
using Ra2VoxelSceneSnapshot = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSceneSnapshot;
using Ra2VoxelExplicitMask = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelExplicitMask;
using Ra2VoxelBaseColourSelection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelBaseColourSelection;
using Ra2VoxelColourMaterializationContext = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourMaterializationContext;
using Ra2VoxelColourMaterializationResult = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourMaterializationResult;
using Ra2VoxelColourTechniquePolicy = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourTechniquePolicy;
using Ra2VoxelConfirmedUnitClass = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelConfirmedUnitClass;
using Ra2VoxelSemanticColourMaterializer = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticColourMaterializer;
using Ra2VoxelSemanticColourRequirements = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticColourRequirements;
using Ra2VoxelSemanticColourRequirementsProjector = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticColourRequirementsProjector;
using Ra2VoxelSkillIdentity = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSkillIdentity;
using Ra2VoxelUnitClassEvidence = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassEvidence;
using Ra2VoxelUnitClassEvidenceBuilder = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassEvidenceBuilder;
using Ra2VoxelUnitClassProposal = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassProposal;
using Ra2VoxelSemanticEvidenceBuilder = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEvidenceBuilder;
using Ra2VoxelSemanticEvidencePackage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEvidencePackage;
using Ra2VoxelSemanticEffectiveAssignment = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEffectiveAssignment;
using Ra2VoxelSemanticStyleIntegrator = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticStyleIntegrator;
using Ra2VoxelSemanticMaskComposition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaskComposition;
using Ra2VoxelMeshCoverageEvidence = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelMeshCoverageEvidence;
using Ra2VoxelAgentGeometryProposalExecutor = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelAgentGeometryProposalExecutor;
using Ra2VoxelSemanticPartition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticPartition;
using Ra2VoxelSemanticSymmetryFailureKind = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticSymmetryFailureKind;
using Ra2VoxelSemanticSymmetryResult = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticSymmetryResult;
using Ra2VoxelSymmetryEvidenceBuilder = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSymmetryEvidenceBuilder;
using Ra2VoxelSymmetryEvidencePackage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSymmetryEvidencePackage;
using Ra2VoxelSymmetryEvidenceResult = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSymmetryEvidenceResult;
using Ra2VoxelSymmetryMode = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSymmetryMode;
using Ra2VoxelSliceStackCodec = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSliceStackCodec;
using Ra2VoxelStyleSourceFact = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelStyleSourceFact;
using Ra2VxlseSliceImportContract = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VxlseSliceImportContract;
using Ra2VxlseSliceDirection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VxlseSliceDirection;
using Ra2WestwoodVxlReader = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2WestwoodVxlReader;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelStyleSourceLoadFailureKind
{
    None = 0,
    NoActiveProject,
    InvalidPath,
    OutsideProject,
    UnsupportedFile,
    SourceRejected,
    SourceReadFailed,
    Cancelled
}

internal sealed record Ra2VoxelStyleSourceLoadResult(
    Ra2VoxelStyleSourceLoadFailureKind FailureKind,
    string Message,
    string? FilePath,
    Ra2VoxelSceneSnapshot? Snapshot,
    byte[]? OriginalSliceStackPng,
    bool IsGeneratedSession = false,
    string? StyleAnchorDirectory = null,
    string? DisplayName = null,
    byte[]? SourceGlb = null)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelStyleSourceLoadFailureKind.None &&
        (FilePath is not null || IsGeneratedSession) && Snapshot is not null && OriginalSliceStackPng is not null;
}

internal enum Ra2VoxelStylePreviewFailureKind
{
    None = 0,
    InvalidSource,
    StyleSourceFailure,
    CompilerFailure,
    ColourizationFailure,
    ReviewPackageFailure,
    MissingReviewArtifact,
    Cancelled,
    AnalysisFailed
}

internal sealed record Ra2VoxelStylePreviewResult(
    Ra2VoxelStylePreviewFailureKind FailureKind,
    string Message,
    Ra2VoxelStyleCompilerResult? CompilerResult,
    Ra2VoxelStyleSourcePack? SourcePack,
    Ra2CompiledVoxelStylePlan? Plan,
    Ra2VoxelColourizationFacts? Facts,
    Ra2VoxelSceneSnapshot? ResultSnapshot,
    Ra2VoxelGeometryRegionMask? GeometryMask,
    IReadOnlyList<Ra2VoxelReviewArtifact> Artifacts,
    Ra2CompiledVoxelStylePlan? ContrastPlan,
    Ra2VoxelPaletteContrastFacts? ContrastFacts,
    Ra2VoxelSceneSnapshot? ContrastResultSnapshot,
    byte[]? ContrastSliceStackPng,
    Ra2VoxelStyleCompilerV2Result? CompilerV2Result = null,
    Ra2VoxelColourMaterializationResult? Materialization = null)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelStylePreviewFailureKind.None &&
        Plan is not null && Facts is not null && ResultSnapshot is not null && GeometryMask is not null && Artifacts.Count > 0;

    internal byte[]? FindArtifactBytes(string fileName)
    {
        Ra2VoxelReviewArtifact? artifact = Artifacts.SingleOrDefault(candidate =>
            string.Equals(candidate.FileName, fileName, StringComparison.Ordinal));
        return artifact?.Content.ToArray();
    }
}

internal sealed record Ra2VoxelUnitClassPreviewResult(
    Ra2VoxelUnitClassAssessmentFailureKind FailureKind,
    string Message,
    Ra2VoxelUnitClassEvidence? Evidence,
    Ra2VoxelUnitClassAssessmentResult? Assessment)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelUnitClassAssessmentFailureKind.None &&
                               Evidence is not null && Assessment?.IsSuccess == true;
}

internal enum Ra2VoxelQualitySourceProvenance
{
    Unavailable = 0,
    Verified,
    UserPaired,
    Mismatch
}

internal enum Ra2VoxelQualityPreviewFailureKind
{
    None = 0,
    InvalidBaseline,
    InvalidPath,
    OutsideProject,
    SourceRejected,
    SourceReadFailed,
    SourceMismatch,
    EvidenceGridMismatch,
    RefinementFailed,
    Cancelled
}

internal sealed record Ra2VoxelQualityPreviewResult(
    Ra2VoxelQualityPreviewFailureKind FailureKind,
    string Message,
    string? FilePath,
    Ra2VoxelQualitySourceProvenance Provenance,
    Ra2VoxelSceneSnapshot? DirectCandidate,
    Ra2VoxelSceneSnapshot? RefinedCandidate,
    Ra2VoxelSceneSnapshot? SymmetryCandidate,
    Ra2VoxelGeometryQualityFacts? BaselineFacts,
    Ra2VoxelRefinementReviewPackage? ReviewPackage,
    Ra2VoxelMeshCoverageEvidence? MeshCoverageEvidence,
    Ra2VoxelSymmetryEvidenceResult? SymmetryEvidenceResult,
    byte[]? DirectSliceStackPng,
    byte[]? RefinedSliceStackPng,
    byte[]? SymmetrySliceStackPng,
    bool IsGeneratedSession = false,
    string WorkingBaselineHash = "",
    long WorkingRevision = 0,
    string MeshEvidenceHash = "",
    string QualityBatchHash = "")
{
    internal Ra2VoxelSymmetryEvidencePackage? SymmetryEvidence => SymmetryEvidenceResult?.Package;
    internal bool IsSuccess => FailureKind == Ra2VoxelQualityPreviewFailureKind.None &&
        (FilePath is not null || IsGeneratedSession) && DirectCandidate is not null && RefinedCandidate is not null && BaselineFacts is not null &&
        ReviewPackage is not null && MeshCoverageEvidence is not null && DirectSliceStackPng is not null && RefinedSliceStackPng is not null &&
        WorkingRevision >= 0 && WorkingBaselineHash.Length == 64 && MeshEvidenceHash.Length == 64 && QualityBatchHash.Length == 64;
}

internal enum Ra2VoxelStructurePreviewFailureKind
{
    None = 0,
    InvalidQualityPreview,
    CompilerFailure,
    NoSafeCandidate,
    Cancelled
}

internal sealed record Ra2VoxelStructurePreviewResult(
    Ra2VoxelStructurePreviewFailureKind FailureKind,
    string Message,
    string? SourceFilePath,
    string SourceSnapshotHash,
    string ModelIdentity,
    Ra2VoxelSemanticCompilerResult? CompilerResult,
    Ra2VoxelSemanticSymmetryResult? SymmetryResult,
    byte[]? SymmetrySliceStackPng,
    string WorkingBaselineHash = "",
    long WorkingRevision = 0,
    string QualityBatchHash = "")
{
    internal bool IsSuccess => FailureKind == Ra2VoxelStructurePreviewFailureKind.None &&
        CompilerResult?.IsSuccess == true && SymmetryResult?.IsSuccess == true && SymmetrySliceStackPng is not null;
    internal Ra2VoxelSemanticPartition? Partition => SymmetryResult?.EffectivePartition ?? CompilerResult?.Partition;
    internal Ra2VoxelSceneSnapshot? Candidate => SymmetryResult?.Candidate;
}

/// <summary>
/// Owns the read-only, review-first UI transaction. It never writes project or asset files.
/// </summary>
internal sealed class Ra2VoxelStylePreviewCoordinator
{
    private const int MaximumInstructionBytes = 64 * 1024;
    private readonly Func<DeepSeekRa2AiModel, IRa2AiClient> _clientFactory;
    private readonly Ra2VoxelStylePlanCache _cache;
    private readonly string _bundledStylePath;
    private readonly string _compilerInstructionsPath;
    private readonly Func<DeepSeekRa2AiModel, bool> _configurationReady;
    private readonly Ra2VoxelUnitClassProposalCache _unitClassCache;
    private readonly Ra2AgentSkillCatalog _skillCatalog;

    internal Ra2VoxelStylePreviewCoordinator(
        Func<DeepSeekRa2AiModel, IRa2AiClient> clientFactory,
        Ra2VoxelStylePlanCache cache,
        string bundledStylePath,
        string compilerInstructionsPath,
        Func<DeepSeekRa2AiModel, bool>? configurationReady = null,
        Ra2VoxelUnitClassProposalCache? unitClassCache = null,
        Ra2AgentSkillCatalog? skillCatalog = null)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _bundledStylePath = RequireFullyQualifiedPath(bundledStylePath, nameof(bundledStylePath));
        _compilerInstructionsPath = RequireFullyQualifiedPath(compilerInstructionsPath, nameof(compilerInstructionsPath));
        _configurationReady = configurationReady ?? (model =>
            DeepSeekRa2AiClientFactory.CreateConfigurationSnapshot(model).State == DeepSeekRa2AiConfigurationState.Ready);
        _unitClassCache = unitClassCache ?? new Ra2VoxelUnitClassProposalCache(Ra2VoxelUnitClassProposalCache.DefaultRoot);
        _skillCatalog = skillCatalog ?? Ra2AgentSkillCatalog.LoadBundled();
    }

    internal static Ra2VoxelStylePreviewCoordinator CreateDefault() => new(
        DeepSeekRa2AiClientFactory.CreateClientFromEnvironment,
        new Ra2VoxelStylePlanCache(Ra2VoxelStylePlanCache.DefaultRoot),
        Path.Combine(AppContext.BaseDirectory, "VoxelStyles", "default", Ra2VoxelStyleSourceResolver.FileName),
        Path.Combine(AppContext.BaseDirectory, "VoxelStyles", "compiler", "COMPILER.md"));

    internal async Task<Ra2VoxelUnitClassPreviewResult> AnalyzeUnitClassAsync(
        Ra2VoxelStyleSourceLoadResult source,
        Ra2VoxelSemanticMaskComposition composition,
        DeepSeekRa2AiModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(composition);
        if (!source.IsSuccess || source.Snapshot is null ||
            !string.Equals(source.Snapshot.CanonicalHash, composition.SourceSnapshotHash, StringComparison.Ordinal) ||
            source.Snapshot.OccupancyCount != composition.CellCount)
        {
            return new(Ra2VoxelUnitClassAssessmentFailureKind.MalformedProposal,
                "当前几何与语义证据不一致，无法安全判型。", null, null);
        }
        try
        {
            Ra2VoxelUnitClassEvidence evidence = Ra2VoxelUnitClassEvidenceBuilder.Build(source.Snapshot, composition);
            string modelIdentity = DeepSeekRa2AiModelCatalog.GetApiModelId(model);
            Ra2VoxelUnitClassClassifier classifier = new(
                _clientFactory(model),
                _unitClassCache,
                _skillCatalog);
            Ra2VoxelUnitClassAssessmentResult assessment = await classifier.AssessAsync(
                evidence, modelIdentity, cancellationToken).ConfigureAwait(false);
            return new(assessment.FailureKind, assessment.Message, evidence, assessment);
        }
        catch (OperationCanceledException)
        {
            return new(Ra2VoxelUnitClassAssessmentFailureKind.Cancelled, "单位类型判定已取消。", null, null);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return new(Ra2VoxelUnitClassAssessmentFailureKind.MalformedProposal,
                "单位类型证据无法安全构建。", null, null);
        }
    }

    internal Ra2VoxelColourSkillRouteResult ResolveColourSkill(
        Ra2VoxelUnitClassEvidence evidence,
        Ra2VoxelConfirmedUnitClass confirmation) =>
        Ra2VoxelColourSkillRouter.Resolve(evidence, confirmation, _skillCatalog);

    internal Ra2VoxelUnitClassEvidence BuildUnitClassEvidence(
        Ra2VoxelStyleSourceLoadResult source,
        Ra2VoxelSemanticMaskComposition composition)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(composition);
        if (!source.IsSuccess || source.Snapshot is null)
            throw new ArgumentException("A valid working voxel source is required.", nameof(source));
        return Ra2VoxelUnitClassEvidenceBuilder.Build(source.Snapshot, composition);
    }

    internal Ra2VoxelStyleSourceLoadResult LoadSource(
        string? projectRoot,
        string? filePath,
        CancellationToken cancellationToken = default)
        => LoadSource(projectRoot, filePath, palettePath: null, cancellationToken);

    internal Ra2VoxelStyleSourceLoadResult LoadSource(
        string? projectRoot,
        string? filePath,
        string? palettePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectRoot))
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.NoActiveProject, "请先打开一个项目，再选择项目内的体素模型。");
        if (string.IsNullOrWhiteSpace(filePath))
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.InvalidPath, "没有选择体素模型文件。");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            string root = NormalizeDirectory(projectRoot);
            string path = Path.GetFullPath(filePath);
            if (!Directory.Exists(root) || IsReparsePoint(root))
                return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceRejected, "当前项目目录不可用或不安全。");
            if (!IsSameOrDescendant(path, root))
                return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.OutsideProject, "请选择当前项目目录内的体素模型。");
            if (HasReparsePointInDirectoryChain(Path.GetDirectoryName(path)!, root))
                return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceRejected, "模型所在目录链包含链接，当前阶段拒绝读取。");
            string extension = Path.GetExtension(path);
            if (!string.Equals(extension, ".vox", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(extension, ".vxl", StringComparison.OrdinalIgnoreCase))
            {
                return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.UnsupportedFile, "当前工作区只接受 .vox 或 .vxl 体素模型。");
            }

            FileInfo file = new(path);
            if (!file.Exists || (file.Attributes & FileAttributes.ReparsePoint) != 0 ||
                file.Length is < 1 or > Ra2MagicaVoxelCodec.MaximumEncodedByteLength)
            {
                return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceRejected, "模型文件不存在、是链接，或超过安全大小限制。");
            }

            string identity = CreateIdentity(Path.GetFileNameWithoutExtension(path));
            Ra2VoxelSceneSnapshot snapshot;
            if (string.Equals(extension, ".vox", StringComparison.OrdinalIgnoreCase))
            {
                using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                snapshot = Ra2MagicaVoxelCodec.Read(
                    stream,
                    identity,
                    "body",
                    Ra2VoxelAssemblyPartRole.Body,
                    "Body",
                    identity);
            }
            else
            {
                Ra2VoxelStyleSourceLoadResult? paletteFailure = ValidatePalettePath(root, palettePath, out string? normalizedPalettePath);
                if (paletteFailure is not null)
                    return paletteFailure;
                Ra2VoxelPaletteProfile palette = ReadWestwoodPalette(normalizedPalettePath!);
                using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                IReadOnlyList<Ra2VoxelSceneSnapshot> sections = Ra2WestwoodVxlReader.Read(
                    stream,
                    identity,
                    identity,
                    Ra2VoxelAssemblyPartRole.Body,
                    palette);
                if (sections.Count != 1)
                {
                    return SourceFailure(
                        Ra2VoxelStyleSourceLoadFailureKind.UnsupportedFile,
                        $"该 VXL 包含 {sections.Count} 个 Section；当前预览工作区需要单 Section 模型，不能替你隐式选择。");
                }
                snapshot = sections[0];
            }
            cancellationToken.ThrowIfCancellationRequested();
            byte[] preview = Ra2VoxelSliceStackCodec.ExportPng(snapshot, Ra2VxlseSliceDirection.Downward);
            return new(Ra2VoxelStyleSourceLoadFailureKind.None, string.Empty, path, snapshot, preview);
        }
        catch (OperationCanceledException)
        {
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.Cancelled, "VOX 载入已取消。");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
            InvalidDataException or ArgumentException or OverflowException or NotSupportedException)
        {
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceReadFailed, "无法读取该体素模型：文件、PAL 或模型内容不符合当前受限输入契约。");
        }
    }

    internal async Task<Ra2VoxelStylePreviewResult> CompilePreviewAsync(
        Ra2VoxelStyleSourceLoadResult source,
        string projectRoot,
        string? requestOverride,
        DeepSeekRa2AiModel model,
        CancellationToken cancellationToken,
        Ra2VoxelSemanticEvidencePackage? semanticEvidence = null,
        IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment>? semanticAssignments = null,
        Ra2VoxelSemanticMaskComposition? semanticComposition = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.IsSuccess || source.Snapshot is null)
            return PreviewFailure(Ra2VoxelStylePreviewFailureKind.InvalidSource, "请先载入一个有效的体素模型。");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelStyleSourceResolutionResult resolution = ResolveSourcePack(
                source,
                projectRoot,
                requestOverride);
            if (!resolution.IsSuccess || resolution.SourcePack is null)
                return PreviewFailure(Ra2VoxelStylePreviewFailureKind.StyleSourceFailure, LocalizeSourceFailure(resolution));

            string instructions = ReadBoundedUtf8(_compilerInstructionsPath);
            string modelIdentity = DeepSeekRa2AiModelCatalog.GetApiModelId(model);
            Ra2VoxelStyleCompiler compiler = new(_clientFactory(model), _cache, instructions);
            Ra2VoxelStyleCompilerResult compilation = await compiler.CompileAsync(
                resolution.SourcePack,
                source.Snapshot.Palette,
                new Ra2VoxelStyleCompilationContext(
                    source.Snapshot.Part.Role.ToString(),
                    source.Snapshot.CanonicalHash,
                    modelIdentity),
                cancellationToken).ConfigureAwait(false);
            if (!compilation.IsSuccess || compilation.Plan is null)
            {
                Ra2VoxelStylePreviewFailureKind failureKind = compilation.FailureKind == Ra2VoxelStyleCompilerFailureKind.Cancelled
                    ? Ra2VoxelStylePreviewFailureKind.Cancelled
                    : Ra2VoxelStylePreviewFailureKind.CompilerFailure;
                return PreviewFailure(failureKind, LocalizeCompilerFailure(compilation), compilation);
            }

            Ra2CompiledVoxelStylePlan effectivePlan = compilation.Plan;
            IReadOnlyList<Ra2VoxelExplicitMask> explicitMasks = [];
            if (semanticComposition is not null)
            {
                if (!string.Equals(semanticComposition.SourceSnapshotHash, source.Snapshot.CanonicalHash, StringComparison.Ordinal) ||
                    semanticComposition.CellCount != source.Snapshot.OccupancyCount)
                    return PreviewFailure(Ra2VoxelStylePreviewFailureKind.InvalidSource, "人工语义蒙版已过期，请基于当前几何重新编辑。", compilation);
                var integration = Ra2VoxelSemanticStyleIntegrator.Integrate(compilation.Plan, semanticComposition);
                effectivePlan = integration.Plan;
                explicitMasks = integration.Masks;
            }
            else if (semanticEvidence is not null && semanticAssignments is not null)
            {
                if (!string.Equals(semanticEvidence.SourceSnapshotHash, source.Snapshot.CanonicalHash, StringComparison.Ordinal))
                    return PreviewFailure(Ra2VoxelStylePreviewFailureKind.InvalidSource, "语义掩码已过期，请基于当前几何重新分析。", compilation);
                var integration = Ra2VoxelSemanticStyleIntegrator.Integrate(compilation.Plan, semanticEvidence, semanticAssignments);
                effectivePlan = integration.Plan;
                explicitMasks = integration.Masks;
            }

            var colourization = Ra2VoxelColourizer.Colourize(source.Snapshot, effectivePlan, explicitMasks, cancellationToken);
            if (!colourization.IsSuccess || colourization.Facts is null ||
                colourization.Snapshot is null || colourization.GeometryMask is null)
                return PreviewFailure(Ra2VoxelStylePreviewFailureKind.ColourizationFailure,
                    string.IsNullOrWhiteSpace(colourization.Message) ? "体素着色未能生成可审阅结果。" : colourization.Message,
                    compilation);

            Ra2VoxelStyleSourceFact[] sourceFacts = resolution.SourcePack.Sources
                .Select(item => new Ra2VoxelStyleSourceFact(item.ScopeId, item.ContentHash, item.Text.Length))
                .ToArray();
            var review = Ra2VoxelColourReviewPackageBuilder.Build(
                sourceFacts,
                source.Snapshot,
                effectivePlan,
                colourization,
                explicitMasks);
            if (!review.IsSuccess)
                return PreviewFailure(Ra2VoxelStylePreviewFailureKind.ReviewPackageFailure,
                    string.IsNullOrWhiteSpace(review.Message) ? "无法生成体素审阅包。" : review.Message,
                    compilation);

            string[] required = ["palette-swatch.png", "region-mask.png", "body-coloured-slicestack.png"];
            if (required.Any(name => review.Artifacts.All(item => !string.Equals(item.FileName, name, StringComparison.Ordinal))))
                return PreviewFailure(Ra2VoxelStylePreviewFailureKind.MissingReviewArtifact, "体素审阅包缺少必要的可视化产物。", compilation);

            Ra2CompiledVoxelStylePlan? contrastPlan = null;
            Ra2VoxelPaletteContrastFacts? contrastFacts = null;
            Ra2VoxelSceneSnapshot? contrastSnapshot = null;
            byte[]? contrastPreview = null;
            try
            {
                var contrast = Ra2VoxelPaletteContrastOptimizer.Optimize(effectivePlan, source.Snapshot.Palette);
                contrastFacts = contrast.Facts;
                if (contrast.Facts.ChangedRoleCount > 0)
                {
                    var contrastColourization = Ra2VoxelColourizer.Colourize(
                        source.Snapshot,
                        contrast.Plan,
                        explicitMasks,
                        cancellationToken: cancellationToken);
                    if (contrastColourization.IsSuccess && contrastColourization.Snapshot is not null)
                    {
                        contrastPlan = contrast.Plan;
                        contrastSnapshot = contrastColourization.Snapshot;
                        contrastPreview = Ra2VoxelSliceStackCodec.ExportPng(
                            contrastSnapshot,
                            Ra2VxlseSliceDirection.Downward);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
            {
                // The ordinary valid style preview remains authoritative when the optional contrast candidate is unavailable.
                contrastPlan = null;
                contrastSnapshot = null;
                contrastPreview = null;
            }

            return new(
                Ra2VoxelStylePreviewFailureKind.None,
                string.Empty,
                compilation,
                resolution.SourcePack,
                effectivePlan,
                colourization.Facts,
                colourization.Snapshot,
                colourization.GeometryMask,
                review.Artifacts,
                contrastPlan,
                contrastFacts,
                contrastSnapshot,
                contrastPreview);
        }
        catch (OperationCanceledException)
        {
            return PreviewFailure(Ra2VoxelStylePreviewFailureKind.Cancelled, "风格编译已取消。");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
            DecoderFallbackException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            return PreviewFailure(Ra2VoxelStylePreviewFailureKind.AnalysisFailed, "风格预览事务未能安全完成。");
        }
    }

    internal async Task<Ra2VoxelStylePreviewResult> CompilePreviewV2Async(
        Ra2VoxelStyleSourceLoadResult source,
        string projectRoot,
        string? requestOverride,
        DeepSeekRa2AiModel model,
        Ra2VoxelSemanticMaskComposition composition,
        Ra2VoxelUnitClassEvidence evidence,
        Ra2VoxelConfirmedUnitClass confirmation,
        Ra2VoxelBaseColourSelection baseColour,
        Ra2VoxelColourTechniquePolicy technique,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(composition);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(confirmation);
        ArgumentNullException.ThrowIfNull(baseColour);
        ArgumentNullException.ThrowIfNull(technique);
        if (!source.IsSuccess || source.Snapshot is null)
            return PreviewFailure(Ra2VoxelStylePreviewFailureKind.InvalidSource, "请先载入一个有效的体素模型。");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(source.Snapshot.CanonicalHash, composition.SourceSnapshotHash, StringComparison.Ordinal) ||
                source.Snapshot.OccupancyCount != composition.CellCount ||
                !string.Equals(evidence.EvidenceHash, confirmation.EvidenceHash, StringComparison.Ordinal))
            {
                return PreviewFailure(Ra2VoxelStylePreviewFailureKind.InvalidSource,
                    "人工单位类型确认或语义证据已过期，请重新选择并确认。");
            }
            Ra2VoxelStyleSourceResolutionResult resolution = ResolveSourcePack(source, projectRoot, requestOverride);
            if (!resolution.IsSuccess || resolution.SourcePack is null)
                return PreviewFailure(Ra2VoxelStylePreviewFailureKind.StyleSourceFailure, LocalizeSourceFailure(resolution));

            Ra2VoxelSemanticColourRequirements requirements =
                Ra2VoxelSemanticColourRequirementsProjector.Project(composition);
            string instructions = ReadBoundedUtf8(_compilerInstructionsPath);
            string modelIdentity = DeepSeekRa2AiModelCatalog.GetApiModelId(model);
            Ra2VoxelStyleCompiler compiler = new(
                _clientFactory(model),
                _cache,
                instructions,
                _skillCatalog);
            Ra2VoxelStyleCompilerV2Result compilation = await compiler.CompileV2Async(
                resolution.SourcePack,
                source.Snapshot.Palette,
                new Ra2VoxelStyleCompilationV2Context(
                    source.Snapshot.Part.Role.ToString(),
                    evidence.GeometryFactsHash,
                    modelIdentity,
                    evidence,
                    confirmation,
                    requirements),
                cancellationToken).ConfigureAwait(false);
            if (!compilation.IsSuccess || compilation.Plan is null || compilation.BindingPlan is null ||
                compilation.SkillRoute is null)
            {
                Ra2VoxelStylePreviewFailureKind failure = compilation.FailureKind == Ra2VoxelStyleCompilerV2FailureKind.Cancelled
                    ? Ra2VoxelStylePreviewFailureKind.Cancelled
                    : Ra2VoxelStylePreviewFailureKind.CompilerFailure;
                return new(failure,
                    string.IsNullOrWhiteSpace(compilation.Message) ? "结构化上色计划编译失败。" : compilation.Message,
                    null, resolution.SourcePack, null, null, null, null, [], null, null, null, null,
                    compilation, null);
            }

            Ra2VoxelColourSkillRoute route = compilation.SkillRoute;
            Ra2VoxelColourMaterializationResult materialization = Ra2VoxelSemanticColourMaterializer.Materialize(
                new Ra2VoxelColourMaterializationContext(
                    source.Snapshot,
                    compilation.Plan,
                    composition,
                    requirements,
                    compilation.BindingPlan,
                    evidence,
                    confirmation,
                    new Ra2VoxelSkillIdentity(route.ColourSkill.Name, route.ColourSkill.Version, route.ColourSkill.ContentHash),
                    baseColour,
                    technique,
                    route.Adaptation),
                cancellationToken);
            if (!materialization.IsSuccess || materialization.Ordinary is null ||
                materialization.SemanticIntegration is null)
            {
                return new(Ra2VoxelStylePreviewFailureKind.ColourizationFailure,
                    string.IsNullOrWhiteSpace(materialization.Message) ? "本地确定性上色未通过质量硬门。" : materialization.Message,
                    null, resolution.SourcePack, null, null, null, null, [], null, null, null, null,
                    compilation, materialization);
            }

            Ra2VoxelStyleSourceFact[] sourceFacts = resolution.SourcePack.Sources
                .Select(item => new Ra2VoxelStyleSourceFact(item.ScopeId, item.ContentHash, item.Text.Length))
                .ToArray();
            Ra2VoxelColourReviewPackageResult review = Ra2VoxelColourReviewPackageBuilder.Build(
                sourceFacts,
                source.Snapshot,
                materialization.Ordinary.Plan,
                materialization.Ordinary.Colourization,
                materialization.SemanticIntegration.Masks,
                materialization.Ordinary.Quality);
            if (!review.IsSuccess)
            {
                return new(Ra2VoxelStylePreviewFailureKind.ReviewPackageFailure,
                    string.IsNullOrWhiteSpace(review.Message) ? "无法生成体素审阅包。" : review.Message,
                    null, resolution.SourcePack, null, null, null, null, [], null, null, null, null,
                    compilation, materialization);
            }
            string[] required = ["palette-swatch.png", "region-mask.png", "body-coloured-slicestack.png"];
            if (required.Any(name => review.Artifacts.All(item => !string.Equals(item.FileName, name, StringComparison.Ordinal))))
            {
                return new(Ra2VoxelStylePreviewFailureKind.MissingReviewArtifact,
                    "体素审阅包缺少必要的可视化产物。", null, resolution.SourcePack, null, null, null, null, [],
                    null, null, null, null, compilation, materialization);
            }

            byte[]? contrastPreview = materialization.Contrast?.Colourization.Snapshot is { } contrastSnapshot
                ? Ra2VoxelSliceStackCodec.ExportPng(contrastSnapshot, Ra2VxlseSliceDirection.Downward)
                : null;
            return new(
                Ra2VoxelStylePreviewFailureKind.None,
                string.Empty,
                null,
                resolution.SourcePack,
                materialization.Ordinary.Plan,
                materialization.Ordinary.Colourization.Facts,
                materialization.Ordinary.Colourization.Snapshot,
                materialization.Ordinary.Colourization.GeometryMask,
                review.Artifacts,
                materialization.Contrast?.Plan,
                materialization.Contrast?.ContrastFacts,
                materialization.Contrast?.Colourization.Snapshot,
                contrastPreview,
                compilation,
                materialization);
        }
        catch (OperationCanceledException)
        {
            return PreviewFailure(Ra2VoxelStylePreviewFailureKind.Cancelled, "风格编译已取消。");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
            DecoderFallbackException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            return PreviewFailure(Ra2VoxelStylePreviewFailureKind.AnalysisFailed, "4E 上色预览事务未能安全完成。");
        }
    }

    internal Ra2VoxelStyleSourceLoadResult ConvertGeneratedGlb(
        string? projectRoot,
        ReadOnlyMemory<byte> glb,
        string? palettePath,
        Ra2VoxelStyleSourceLoadResult? paletteSource,
        int targetLongestDimension,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectRoot))
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.NoActiveProject, "请先打开一个项目，再生成模型预览。");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            string root = NormalizeDirectory(projectRoot);
            if (!Directory.Exists(root) || IsReparsePoint(root))
                return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceRejected, "当前项目目录不可用或不安全。");
            if (glb.Length is < 28 or > 16 * 1024 * 1024)
                return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceRejected, "生成结果不存在或超过会话内 GLB 上限。");
            if (targetLongestDimension is not (32 or 48 or 64 or 96 or 128))
                return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceRejected, "体素分辨率不在允许范围内。");

            Ra2VoxelPaletteProfile palette;
            if (paletteSource?.IsSuccess == true && paletteSource.Snapshot is not null)
            {
                palette = paletteSource.Snapshot.Palette;
            }
            else
            {
                Ra2VoxelStyleSourceLoadResult? paletteFailure = ValidatePalettePath(root, palettePath, out string? normalizedPalettePath);
                if (paletteFailure is not null)
                    return paletteFailure;
                palette = ReadWestwoodPalette(normalizedPalettePath!);
            }

            string identity = "generated-" + Guid.NewGuid().ToString("N")[..12];
            var options = new Ra2MeshVoxelizationOptions(
                identity,
                "body",
                Ra2VoxelAssemblyPartRole.Body,
                "Body",
                identity,
                targetLongestDimension,
                padding: 1,
                palette,
                targetColour: new Ra2Rgba32(92, 100, 68));
            var result = Ra2MeshVoxelizer.ConvertGlb(glb, options, cancellationToken);
            if (!result.IsSuccess || result.Snapshot is null)
                return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceReadFailed,
                    string.IsNullOrWhiteSpace(result.Message) ? "生成的 GLB 无法转换为受限体素候选。" : result.Message);
            byte[] preview = Ra2VoxelSliceStackCodec.ExportPng(result.Snapshot, Ra2VxlseSliceDirection.Downward);
            return new(
                Ra2VoxelStyleSourceLoadFailureKind.None,
                string.Empty,
                null,
                result.Snapshot,
                preview,
                IsGeneratedSession: true,
                StyleAnchorDirectory: root,
                DisplayName: "生成候选（会话）",
                SourceGlb: glb.ToArray());
        }
        catch (OperationCanceledException)
        {
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.Cancelled, "生成候选转换已取消。");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
            InvalidDataException or ArgumentException or OverflowException or InvalidOperationException or Ra2MeshVoxelizationException)
        {
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceReadFailed, "生成的 GLB 未能安全转换为体素候选。");
        }
    }

    internal Ra2VoxelQualityPreviewResult GenerateQualityCandidates(
        string? projectRoot,
        Ra2VoxelStyleSourceLoadResult baseline,
        string? glbPath,
        CancellationToken cancellationToken = default) =>
        GenerateQualityCandidates(projectRoot, baseline, baseline.Snapshot!, 0, glbPath, cancellationToken);

    internal Ra2VoxelQualityPreviewResult GenerateQualityCandidates(
        string? projectRoot,
        Ra2VoxelStyleSourceLoadResult baseline,
        Ra2VoxelSceneSnapshot workingBaseline,
        long workingRevision,
        string? glbPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(workingBaseline);
        if (!baseline.IsSuccess || baseline.Snapshot is null || workingRevision < 0)
            return QualityFailure(Ra2VoxelQualityPreviewFailureKind.InvalidBaseline, "请先载入有效的 VOX 或 VXL 基线模型。");
        if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
            return QualityFailure(Ra2VoxelQualityPreviewFailureKind.InvalidPath, "当前项目目录不可用。");
        if (string.IsNullOrWhiteSpace(glbPath))
            return QualityFailure(Ra2VoxelQualityPreviewFailureKind.InvalidPath, "没有选择 GLB 质量源。");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            string root = NormalizeDirectory(projectRoot);
            if (IsReparsePoint(root))
                return QualityFailure(Ra2VoxelQualityPreviewFailureKind.SourceRejected, "当前项目目录不可用或不安全。");
            string path = Path.GetFullPath(glbPath);
            if (!IsSameOrDescendant(path, root))
                return QualityFailure(Ra2VoxelQualityPreviewFailureKind.OutsideProject, "请选择当前项目目录内的 GLB 质量源。");
            if (!string.Equals(Path.GetExtension(path), ".glb", StringComparison.OrdinalIgnoreCase) ||
                HasReparsePointInDirectoryChain(Path.GetDirectoryName(path)!, root))
            {
                return QualityFailure(Ra2VoxelQualityPreviewFailureKind.SourceRejected, "所选文件不是安全的项目内 GLB 模型。");
            }

            FileInfo file = new(path);
            if (!file.Exists || (file.Attributes & FileAttributes.ReparsePoint) != 0 ||
                file.Length is < 28 or > Ra2GlbMeshReader.MaximumGlbBytes)
            {
                return QualityFailure(Ra2VoxelQualityPreviewFailureKind.SourceRejected, "GLB 不存在、是链接，或超过受限读取大小。");
            }

            byte[] bytes = File.ReadAllBytes(path);
            Ra2MeshSnapshot mesh = Ra2GlbMeshReader.Read(bytes, cancellationToken);
            Ra2VoxelSceneSnapshot snapshot = workingBaseline;
            string? recordedHash = snapshot.SourceArtifactHashes
                .FirstOrDefault(pair => string.Equals(pair.Key, "mesh.glb", StringComparison.OrdinalIgnoreCase))
                .Value;
            Ra2VoxelQualitySourceProvenance provenance = string.IsNullOrWhiteSpace(recordedHash)
                ? Ra2VoxelQualitySourceProvenance.UserPaired
                : string.Equals(recordedHash, mesh.SourceHash, StringComparison.OrdinalIgnoreCase)
                    ? Ra2VoxelQualitySourceProvenance.Verified
                    : Ra2VoxelQualitySourceProvenance.Mismatch;
            if (provenance == Ra2VoxelQualitySourceProvenance.Mismatch)
            {
                return QualityFailure(
                    Ra2VoxelQualityPreviewFailureKind.SourceMismatch,
                    "所选 GLB 与当前基线记录的来源哈希不一致，未生成候选。",
                    path,
                    provenance);
            }

            byte paletteIndex = snapshot.Cells
                .GroupBy(cell => cell.PaletteIndex)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key)
                .Select(group => group.Key)
                .First();
            Ra2MeshVoxelizationOptions options = new(
                snapshot.SceneId,
                snapshot.Part.PartId,
                snapshot.Part.Role,
                snapshot.Part.VxlSectionName,
                snapshot.Part.StableFileStem,
                Math.Clamp(
                    Math.Max(snapshot.Part.XSize, Math.Max(snapshot.Part.YSize, snapshot.Part.ZSize)),
                    Ra2MeshVoxelizationOptions.MinimumTargetLongestDimension,
                    Ra2MeshVoxelizationOptions.MaximumTargetLongestDimension),
                padding: 2,
                snapshot.Palette,
                paletteIndex: paletteIndex);
            Ra2VoxelRefinementProfile profile = new();
            Ra2VoxelQualityRefinementResult result = Ra2VoxelQualityRefiner.RefineExisting(
                snapshot,
                mesh,
                options,
                profile,
                cancellationToken: cancellationToken);
            if (!result.IsSuccess)
            {
                return QualityFailure(
                    result.FailureKind == Ra2VoxelRefinementFailureKind.EvidenceGridMismatch
                        ? Ra2VoxelQualityPreviewFailureKind.EvidenceGridMismatch
                        : Ra2VoxelQualityPreviewFailureKind.RefinementFailed,
                    string.IsNullOrWhiteSpace(result.Message) ? "本地质量门禁未能生成安全的平滑候选。" : result.Message,
                    path,
                    provenance);
            }

            var baselineAnalysis = Ra2VoxelQualityAnalyzer.Analyze(snapshot, cancellationToken: cancellationToken);
            if (!baselineAnalysis.IsSuccess)
            {
                return QualityFailure(
                    Ra2VoxelQualityPreviewFailureKind.RefinementFailed,
                    string.IsNullOrWhiteSpace(baselineAnalysis.Message) ? "无法分析当前基线的质量事实。" : baselineAnalysis.Message,
                    path,
                    provenance);
            }

            byte[] directPreview = Ra2VoxelSliceStackCodec.ExportPng(result.DirectCandidate!, Ra2VxlseSliceDirection.Downward);
            byte[] refinedPreview = Ra2VoxelSliceStackCodec.ExportPng(result.RefinedCandidate!, Ra2VxlseSliceDirection.Downward);
            var evidence = Ra2VoxelSymmetryEvidenceBuilder.Build(
                result.RefinedCandidate!,
                result.DirectCandidate!,
                result.ReviewPackage!.ProtectionMask,
                result.MeshCoverageEvidence!,
                cancellationToken: cancellationToken);
            string batchHash = ComputeQualityBatchHash(snapshot, workingRevision, mesh.SourceHash, profile.ProfileHash, result);

            return new(
                Ra2VoxelQualityPreviewFailureKind.None,
                string.Empty,
                path,
                provenance,
                result.DirectCandidate,
                result.RefinedCandidate,
                null,
                baselineAnalysis.Facts,
                result.ReviewPackage,
                result.MeshCoverageEvidence,
                evidence,
                directPreview,
                refinedPreview,
                null,
                IsGeneratedSession: false,
                WorkingBaselineHash: snapshot.CanonicalHash,
                WorkingRevision: workingRevision,
                MeshEvidenceHash: mesh.SourceHash,
                QualityBatchHash: batchHash);
        }
        catch (OperationCanceledException)
        {
            return QualityFailure(Ra2VoxelQualityPreviewFailureKind.Cancelled, "质量候选生成已取消。");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
            ArgumentException or InvalidOperationException or OverflowException or Ra2MeshVoxelizationException)
        {
            return QualityFailure(Ra2VoxelQualityPreviewFailureKind.SourceReadFailed, "无法安全读取或转换该 GLB 质量源。");
        }
    }

    internal Ra2VoxelQualityPreviewResult GenerateQualityCandidatesFromGenerated(
        Ra2VoxelStyleSourceLoadResult baseline,
        CancellationToken cancellationToken = default) =>
        GenerateQualityCandidatesFromGenerated(baseline, baseline.Snapshot!, 0, cancellationToken);

    internal Ra2VoxelQualityPreviewResult GenerateQualityCandidatesFromGenerated(
        Ra2VoxelStyleSourceLoadResult baseline,
        Ra2VoxelSceneSnapshot workingBaseline,
        long workingRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(workingBaseline);
        if (!baseline.IsSuccess || !baseline.IsGeneratedSession || baseline.Snapshot is null || baseline.SourceGlb is null || workingRevision < 0)
            return QualityFailure(Ra2VoxelQualityPreviewFailureKind.InvalidBaseline, "当前会话没有可复用的生成 GLB。");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2MeshSnapshot mesh = Ra2GlbMeshReader.Read(baseline.SourceGlb, cancellationToken);
            Ra2VoxelSceneSnapshot snapshot = workingBaseline;
            byte paletteIndex = snapshot.Cells
                .GroupBy(cell => cell.PaletteIndex)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key)
                .Select(group => group.Key)
                .First();
            var options = new Ra2MeshVoxelizationOptions(
                snapshot.SceneId,
                snapshot.Part.PartId,
                snapshot.Part.Role,
                snapshot.Part.VxlSectionName,
                snapshot.Part.StableFileStem,
                Math.Clamp(Math.Max(snapshot.Part.XSize, Math.Max(snapshot.Part.YSize, snapshot.Part.ZSize)),
                    Ra2MeshVoxelizationOptions.MinimumTargetLongestDimension,
                    Ra2MeshVoxelizationOptions.MaximumTargetLongestDimension),
                padding: 2,
                snapshot.Palette,
                paletteIndex: paletteIndex);
            Ra2VoxelRefinementProfile profile = new();
            Ra2VoxelQualityRefinementResult result = Ra2VoxelQualityRefiner.RefineExisting(
                snapshot, mesh, options, profile, cancellationToken);
            if (!result.IsSuccess)
                return QualityFailure(result.FailureKind == Ra2VoxelRefinementFailureKind.EvidenceGridMismatch
                        ? Ra2VoxelQualityPreviewFailureKind.EvidenceGridMismatch
                        : Ra2VoxelQualityPreviewFailureKind.RefinementFailed,
                    string.IsNullOrWhiteSpace(result.Message) ? "本地质量门禁未能生成安全候选。" : result.Message);
            var baselineAnalysis = Ra2VoxelQualityAnalyzer.Analyze(snapshot, cancellationToken: cancellationToken);
            if (!baselineAnalysis.IsSuccess)
                return QualityFailure(Ra2VoxelQualityPreviewFailureKind.RefinementFailed, "无法分析当前生成候选的质量事实。");
            byte[] directPreview = Ra2VoxelSliceStackCodec.ExportPng(result.DirectCandidate!, Ra2VxlseSliceDirection.Downward);
            byte[] refinedPreview = Ra2VoxelSliceStackCodec.ExportPng(result.RefinedCandidate!, Ra2VxlseSliceDirection.Downward);
            var evidence = Ra2VoxelSymmetryEvidenceBuilder.Build(
                result.RefinedCandidate!, result.DirectCandidate!, result.ReviewPackage!.ProtectionMask,
                result.MeshCoverageEvidence!, cancellationToken: cancellationToken);
            string batchHash = ComputeQualityBatchHash(snapshot, workingRevision, mesh.SourceHash, profile.ProfileHash, result);
            return new(
                Ra2VoxelQualityPreviewFailureKind.None,
                string.Empty,
                null,
                Ra2VoxelQualitySourceProvenance.Verified,
                result.DirectCandidate,
                result.RefinedCandidate,
                null,
                baselineAnalysis.Facts,
                result.ReviewPackage,
                result.MeshCoverageEvidence,
                evidence,
                directPreview,
                refinedPreview,
                null,
                IsGeneratedSession: true,
                WorkingBaselineHash: snapshot.CanonicalHash,
                WorkingRevision: workingRevision,
                MeshEvidenceHash: mesh.SourceHash,
                QualityBatchHash: batchHash);
        }
        catch (OperationCanceledException)
        {
            return QualityFailure(Ra2VoxelQualityPreviewFailureKind.Cancelled, "质量候选生成已取消。");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
            ArgumentException or InvalidOperationException or OverflowException or Ra2MeshVoxelizationException)
        {
            return QualityFailure(Ra2VoxelQualityPreviewFailureKind.SourceReadFailed, "无法安全分析会话内生成 GLB。");
        }
    }

    internal async Task<Ra2VoxelStructurePreviewResult> AnalyzeStructureAsync(
        Ra2VoxelQualityPreviewResult quality,
        DeepSeekRa2AiModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(quality);
        string modelIdentity = DeepSeekRa2AiModelCatalog.GetOption(model).ApiModelId;
        if (!quality.IsSuccess || quality.RefinedCandidate is null ||
            quality.MeshCoverageEvidence is null || quality.SymmetryEvidence is null)
        {
            return StructureFailure(
                Ra2VoxelStructurePreviewFailureKind.InvalidQualityPreview,
                "当前本地候选没有可供结构识别的完整证据，请重新生成候选。",
                quality.FilePath,
                quality.RefinedCandidate?.CanonicalHash,
                modelIdentity,
                workingBaselineHash: quality.WorkingBaselineHash,
                workingRevision: quality.WorkingRevision,
                qualityBatchHash: quality.QualityBatchHash);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelSemanticCompilerResult compilation = await new Ra2VoxelSemanticSymmetryCompiler(_clientFactory(model))
                .CompileAsync(quality.SymmetryEvidence, quality.MeshCoverageEvidence, cancellationToken)
                .ConfigureAwait(false);
            if (!compilation.IsSuccess)
            {
                return StructureFailure(
                    compilation.FailureKind == Ra2VoxelSemanticCompilerFailureKind.Cancelled
                        ? Ra2VoxelStructurePreviewFailureKind.Cancelled
                        : Ra2VoxelStructurePreviewFailureKind.CompilerFailure,
                    LocalizeSemanticCompilerFailure(compilation),
                    quality.FilePath,
                    quality.RefinedCandidate.CanonicalHash,
                    modelIdentity,
                    compilation,
                    workingBaselineHash: quality.WorkingBaselineHash,
                    workingRevision: quality.WorkingRevision,
                    qualityBatchHash: quality.QualityBatchHash);
            }

            Ra2VoxelSemanticSymmetryResult symmetry = Ra2VoxelAgentGeometryProposalExecutor.BuildCandidate(
                quality.RefinedCandidate,
                quality.SymmetryEvidence,
                compilation.Proposal!,
                quality.MeshCoverageEvidence,
                cancellationToken: cancellationToken);
            if (!symmetry.IsSuccess)
            {
                return StructureFailure(
                    symmetry.FailureKind == Ra2VoxelSemanticSymmetryFailureKind.Cancelled
                        ? Ra2VoxelStructurePreviewFailureKind.Cancelled
                        : Ra2VoxelStructurePreviewFailureKind.NoSafeCandidate,
                    LocalizeSemanticSymmetryFailure(symmetry),
                    quality.FilePath,
                    quality.RefinedCandidate.CanonicalHash,
                    modelIdentity,
                    compilation,
                    symmetry,
                    quality.WorkingBaselineHash,
                    quality.WorkingRevision,
                    quality.QualityBatchHash);
            }

            byte[] preview = Ra2VoxelSliceStackCodec.ExportPng(symmetry.Candidate!, Ra2VxlseSliceDirection.Downward);
            return new(
                Ra2VoxelStructurePreviewFailureKind.None,
                string.Empty,
                quality.FilePath,
                quality.RefinedCandidate.CanonicalHash,
                modelIdentity,
                compilation,
                symmetry,
                preview,
                quality.WorkingBaselineHash,
                quality.WorkingRevision,
                quality.QualityBatchHash);
        }
        catch (OperationCanceledException)
        {
            return StructureFailure(
                Ra2VoxelStructurePreviewFailureKind.Cancelled,
                "AI 结构识别已取消。",
                quality.FilePath,
                quality.RefinedCandidate.CanonicalHash,
                modelIdentity,
                workingBaselineHash: quality.WorkingBaselineHash,
                workingRevision: quality.WorkingRevision,
                qualityBatchHash: quality.QualityBatchHash);
        }
    }

    private static string ComputeQualityBatchHash(
        Ra2VoxelSceneSnapshot baseline,
        long workingRevision,
        string meshEvidenceHash,
        string profileHash,
        Ra2VoxelQualityRefinementResult result)
    {
        string value = string.Join(
            ':',
            baseline.CanonicalHash,
            workingRevision.ToString(System.Globalization.CultureInfo.InvariantCulture),
            meshEvidenceHash,
            profileHash,
            result.MeshCoverageEvidence!.EvidenceHash,
            result.ReviewPackage!.ProtectionMask.MaskHash,
            result.DirectCandidate!.CanonicalHash,
            result.RefinedCandidate!.CanonicalHash);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static string LocalizeSemanticSymmetryFailure(Ra2VoxelSemanticSymmetryResult symmetry)
    {
        if (symmetry.FailureKind == Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate &&
            string.Equals(symmetry.Message, "The Agent proposal did not require a geometry change.", StringComparison.Ordinal))
        {
            return "Agent 的最终提案没有产生实际几何变化，因此未生成候选；本地直接候选和平滑候选保持不变。";
        }

        return string.IsNullOrWhiteSpace(symmetry.Message)
            ? "Agent 几何提案已完成，但没有候选通过最低几何安全线。"
            : $"Agent 几何提案已完成，但没有候选通过最低几何安全线：{symmetry.Message}";
    }

    internal bool IsStructureRecognitionConfigured(DeepSeekRa2AiModel model) =>
        _configurationReady(model);

    internal async Task<Ra2VoxelSemanticAnalysisResult> AnalyzeSemanticMasksAsync(
        Ra2VoxelSceneSnapshot snapshot,
        string? userInstructions,
        DeepSeekRa2AiModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        try
        {
            Ra2VoxelSemanticEvidencePackage evidence = await Task.Run(
                () => Ra2VoxelSemanticEvidenceBuilder.Build(snapshot, cancellationToken),
                CancellationToken.None).ConfigureAwait(false);
            Ra2VoxelSemanticMaskCompilerResult result = await new Ra2VoxelSemanticMaskCompiler(_clientFactory(model))
                .CompileAsync(evidence, userInstructions, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess
                ? new(string.Empty, evidence, result)
                : new(string.IsNullOrWhiteSpace(result.Message) ? "DeepSeek 未返回可用的语义建议。" : result.Message, evidence, result);
        }
        catch (OperationCanceledException)
        {
            return new("语义识别已取消。", null, null);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            return new("无法从当前工作几何生成有界语义证据。", null, null);
        }
    }

    internal Ra2VoxelStyleSourceResolutionResult ResolveSourcePack(
        Ra2VoxelStyleSourceLoadResult source,
        string projectRoot,
        string? requestOverride)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.IsSuccess)
            return new(Ra2VoxelStyleSourceFailureKind.AnalysisFailed, "A valid voxel source is required.", null);
        return Ra2VoxelStyleSourceResolver.Resolve(
            _bundledStylePath,
            projectRoot,
            source.IsGeneratedSession ? source.StyleAnchorDirectory : Path.GetDirectoryName(source.FilePath!),
            requestOverride);
    }

    private static string LocalizeSourceFailure(Ra2VoxelStyleSourceResolutionResult result) => result.FailureKind switch
    {
        Ra2VoxelStyleSourceFailureKind.NoStyleSource => "内置体素风格基线缺失。",
        Ra2VoxelStyleSourceFailureKind.InvalidEncoding => "风格文件不是有效 UTF-8。",
        Ra2VoxelStyleSourceFailureKind.SourceTooLarge => "风格说明超过安全长度限制。",
        Ra2VoxelStyleSourceFailureKind.TooManySources => "风格继承层级超过安全数量限制。",
        Ra2VoxelStyleSourceFailureKind.SourcePathOutsideProject => "风格目录不在当前项目内。",
        Ra2VoxelStyleSourceFailureKind.SourcePathRejected => "风格目录或文件不安全。",
        _ => "无法解析体素风格来源。"
    };

    private static string LocalizeCompilerFailure(Ra2VoxelStyleCompilerResult result) => result.FailureKind switch
    {
        Ra2VoxelStyleCompilerFailureKind.CompilerUnavailable => "DeepSeek 尚未配置，无法编译自然语言风格。",
        Ra2VoxelStyleCompilerFailureKind.CompilerTimeout => "DeepSeek 风格编译超时，请稍后手动重试。",
        Ra2VoxelStyleCompilerFailureKind.ClarificationRequired => string.IsNullOrWhiteSpace(result.Message) ? "风格要求需要补充说明。" : result.Message,
        Ra2VoxelStyleCompilerFailureKind.UnsupportedStyleRequirement => string.IsNullOrWhiteSpace(result.Message) ? "当前风格要求超出 1E 的粗粒度着色能力。" : result.Message,
        Ra2VoxelStyleCompilerFailureKind.Cancelled => "风格编译已取消。",
        Ra2VoxelStyleCompilerFailureKind.PaletteValidationFailed => LocalizePlanValidationFailure(result.Message),
        Ra2VoxelStyleCompilerFailureKind.MalformedProposal => "DeepSeek 返回的结构化风格计划无效。",
        _ => "DeepSeek 未返回可编译的体素风格计划。"
    };

    private static string LocalizeSemanticCompilerFailure(Ra2VoxelSemanticCompilerResult result) => result.FailureKind switch
    {
        Ra2VoxelSemanticCompilerFailureKind.CompilerUnavailable => "DeepSeek 尚未配置，无法执行 AI 结构识别。",
        Ra2VoxelSemanticCompilerFailureKind.CompilerTimeout => "AI 结构识别超时；本地直接和平滑候选仍可继续审阅。",
        Ra2VoxelSemanticCompilerFailureKind.ClarificationRequired => string.IsNullOrWhiteSpace(result.Message)
            ? "AI 结构识别需要补充信息；未生成对称候选。"
            : result.Message,
        Ra2VoxelSemanticCompilerFailureKind.UnsupportedGeometry => string.IsNullOrWhiteSpace(result.Message)
            ? "当前几何证据不足以进行结构识别。"
            : result.Message,
        Ra2VoxelSemanticCompilerFailureKind.EvidenceQueryRejected => string.IsNullOrWhiteSpace(result.Message)
            ? "Agent 请求的补充几何证据超出有界范围；未生成候选。"
            : $"Agent 请求的补充几何证据无法提供：{result.Message}",
        Ra2VoxelSemanticCompilerFailureKind.ArbitrationFailed => string.IsNullOrWhiteSpace(result.Message)
            ? "两份几何提案存在差异，但第三轮仲裁未返回有效结果。"
            : $"第三轮几何仲裁失败：{result.Message}",
        Ra2VoxelSemanticCompilerFailureKind.InvalidPartition => LocalizeSemanticEvidenceMismatch(result.Message),
        Ra2VoxelSemanticCompilerFailureKind.Cancelled => "AI 结构识别已取消。",
        Ra2VoxelSemanticCompilerFailureKind.MalformedProposal => LocalizeMalformedSemanticProposal(result.Message),
        _ => "DeepSeek 未返回可用的几何提案；本地候选保持不变。"
    };

    private static string LocalizeMalformedSemanticProposal(string message)
    {
        string detail = message switch
        {
            _ when message.StartsWith("tool_call_count:", StringComparison.Ordinal) => "未返回唯一的结构工具调用",
            "tool_arguments_too_large" => "结构工具参数超过本地资源上限",
            "root_not_object" => "返回根节点不是 JSON 对象",
            "invalid_json" => "工具参数不是可解析的 JSON",
            _ when message.StartsWith("duplicate_alias:", StringComparison.Ordinal) => $"字段重复或别名冲突（{message[16..]}）",
            _ when message.StartsWith("missing_property:", StringComparison.Ordinal) => $"缺少必需字段（{message[17..]}）",
            _ when message.StartsWith("invalid_type:", StringComparison.Ordinal) => $"字段类型不正确（{message[13..]}）",
            _ when message.StartsWith("invalid_value:", StringComparison.Ordinal) => $"字段值超出边界（{message[14..]}）",
            "invalid_bounded_shape" => "区域列表或字段形态不完整",
            _ => "返回内容无法归一化为结构提案"
        };
        return $"DeepSeek 返回的几何提案格式无效：{detail}；未生成候选。";
    }

    private static string LocalizeSemanticEvidenceMismatch(string message)
    {
        string detail = message switch
        {
            _ when message.Contains("evidence_hash_mismatch", StringComparison.Ordinal) => "证据版本不匹配",
            _ when message.Contains("reviewed_plane_mismatch", StringComparison.Ordinal) => "对称轴与当前证据不匹配",
            _ when message.Contains("duplicate target", StringComparison.OrdinalIgnoreCase) => "返回了重复目标",
            _ when message.Contains("overlapping", StringComparison.OrdinalIgnoreCase) => "返回了重叠目标",
            _ when message.Contains("Unknown geometry target", StringComparison.Ordinal) => "返回了未知目标",
            _ => "提案与当前证据不一致"
        };
        return $"Agent 几何提案未通过本地证据校验：{detail}；未生成候选。";
    }

    private static string LocalizePlanValidationFailure(string message) => message switch
    {
        "A style colour role cannot be resolved in the active palette." => "风格计划引用了当前色板不存在的颜色范围；普通上色不需要阵营色，请重新编译。",
        "A style colour role selected a transparent palette index." => "风格计划选择了透明色作为实体颜色，已被本地校验拒绝。",
        "A style colour role violates the active remap palette policy." => "风格计划把普通颜色与阵营重映射色混用，已被本地校验拒绝。",
        "A style colour role id is invalid." => "风格计划包含无效的颜色角色标识；角色名必须以英文字母开头，且只能包含字母、数字、点、短横线或下划线。",
        "A style colour role category is invalid." => "风格计划包含当前编译器不支持的颜色角色类别。",
        _ when message.StartsWith("The style colour role id '", StringComparison.Ordinal) && message.EndsWith("' is duplicated.", StringComparison.Ordinal) =>
            $"风格计划包含重复的颜色角色：{message[26..^16]}",
        _ when message.StartsWith("The style colour role '", StringComparison.Ordinal) && message.EndsWith("' does not define a colour source.", StringComparison.Ordinal) =>
            $"风格计划中的颜色角色没有提供色盘索引或 RGB：{message[23..^34]}",
        _ when message.StartsWith("The style colour role '", StringComparison.Ordinal) && message.EndsWith("' defines conflicting palette-index and RGB colour sources.", StringComparison.Ordinal) =>
            $"风格计划中的颜色角色同时给出了互相冲突的色盘索引和 RGB：{message[23..^59]}",
        "The style plan requires one paintable WholePart base rule." => "风格计划缺少可执行的整体基础着色规则。",
        "The style plan interior role does not exist." => "风格计划指定的内部颜色角色不存在。",
        "A style region rule is invalid or conflicts with another rule." => "风格计划包含冲突或无效的区域规则。",
        "A style colour role references an unknown source scope." or
        "A style region rule references an unknown source scope." => "风格计划引用了当前风格来源之外的内容。",
        _ when !string.IsNullOrWhiteSpace(message) => $"风格计划未通过本地校验：{message}",
        _ => "模型返回的风格计划未通过本地色板与区域规则校验。"
    };

    private static Ra2VoxelStyleSourceLoadResult? ValidatePalettePath(
        string projectRoot,
        string? palettePath,
        out string? normalizedPalettePath)
    {
        normalizedPalettePath = null;
        if (string.IsNullOrWhiteSpace(palettePath))
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceRejected, "VXL 着色预览需要明确选择对应的 Westwood .pal 色板。");

        string path = Path.GetFullPath(palettePath);
        if (!IsSameOrDescendant(path, projectRoot))
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.OutsideProject, "请选择当前项目目录内的 Westwood .pal 色板。");
        if (!string.Equals(Path.GetExtension(path), ".pal", StringComparison.OrdinalIgnoreCase) ||
            HasReparsePointInDirectoryChain(Path.GetDirectoryName(path)!, projectRoot))
        {
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceRejected, "所选色板不是安全的项目内 .pal 文件。");
        }

        FileInfo file = new(path);
        if (!file.Exists || (file.Attributes & FileAttributes.ReparsePoint) != 0 ||
            file.Length != Ra2VxlseSliceImportContract.WestwoodPaletteByteLength)
        {
            return SourceFailure(Ra2VoxelStyleSourceLoadFailureKind.SourceRejected, "Westwood PAL 必须是项目内恰好 768 字节的常规文件。");
        }
        normalizedPalettePath = path;
        return null;
    }

    private static Ra2VoxelPaletteProfile ReadWestwoodPalette(string palettePath)
    {
        byte[] bytes = File.ReadAllBytes(palettePath);
        Ra2Rgb24[] decoded = Ra2VxlseSliceImportContract.DecodeWestwoodPalette(bytes);
        Ra2Rgba32[] colours = decoded
            .Select(colour => new Ra2Rgba32(colour.Red, colour.Green, colour.Blue))
            .ToArray();
        string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
        return new Ra2VoxelPaletteProfile($"westwood-pal-{hash[..12].ToLowerInvariant()}", colours, []);
    }

    private static string ReadBoundedUtf8(string path)
    {
        FileInfo file = new(path);
        if (!file.Exists || (file.Attributes & FileAttributes.ReparsePoint) != 0 ||
            file.Length is < 1 or > MaximumInstructionBytes)
        {
            throw new IOException("Voxel style compiler instructions are unavailable.");
        }
        return new UTF8Encoding(false, true).GetString(File.ReadAllBytes(path));
    }

    private static string CreateIdentity(string value)
    {
        string normalized = new(value.Where(character => char.IsLetterOrDigit(character) || character is '_' or '-').ToArray());
        if (string.IsNullOrWhiteSpace(normalized))
            normalized = "voxel";
        return normalized.Length <= Ra2VoxelSceneSnapshot.MaximumIdentityLength
            ? normalized
            : normalized[..Ra2VoxelSceneSnapshot.MaximumIdentityLength];
    }

    private static string NormalizeDirectory(string path) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static bool IsSameOrDescendant(string path, string root)
    {
        string normalizedPath = Path.GetFullPath(path);
        string normalizedRoot = NormalizeDirectory(root);
        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(normalizedPath, normalizedRoot, comparison) ||
            normalizedPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, comparison);
    }

    private static bool IsReparsePoint(string path) =>
        (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;

    private static bool HasReparsePointInDirectoryChain(string directoryPath, string root)
    {
        string current = NormalizeDirectory(directoryPath);
        string normalizedRoot = NormalizeDirectory(root);
        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        while (true)
        {
            if (IsReparsePoint(current))
                return true;
            if (string.Equals(current, normalizedRoot, comparison))
                return false;
            string? parent = Directory.GetParent(current)?.FullName;
            if (parent is null || !IsSameOrDescendant(parent, normalizedRoot))
                return true;
            current = NormalizeDirectory(parent);
        }
    }

    private static string RequireFullyQualifiedPath(string path, string parameterName) =>
        !string.IsNullOrWhiteSpace(path) && Path.IsPathFullyQualified(path)
            ? Path.GetFullPath(path)
            : throw new ArgumentException("A fully-qualified file path is required.", parameterName);

    private static Ra2VoxelStyleSourceLoadResult SourceFailure(
        Ra2VoxelStyleSourceLoadFailureKind kind,
        string message) => new(kind, message, null, null, null);

    private static Ra2VoxelStylePreviewResult PreviewFailure(
        Ra2VoxelStylePreviewFailureKind kind,
        string message,
        Ra2VoxelStyleCompilerResult? compilerResult = null) =>
        new(kind, message, compilerResult, null, null, null, null, null, [], null, null, null, null);

    private static Ra2VoxelQualityPreviewResult QualityFailure(
        Ra2VoxelQualityPreviewFailureKind kind,
        string message,
        string? filePath = null,
        Ra2VoxelQualitySourceProvenance provenance = Ra2VoxelQualitySourceProvenance.Unavailable) =>
        new(kind, message, filePath, provenance, null, null, null, null, null, null, null, null, null, null);

    private static Ra2VoxelStructurePreviewResult StructureFailure(
        Ra2VoxelStructurePreviewFailureKind kind,
        string message,
        string? sourceFilePath,
        string? sourceSnapshotHash,
        string modelIdentity,
        Ra2VoxelSemanticCompilerResult? compilerResult = null,
        Ra2VoxelSemanticSymmetryResult? symmetryResult = null,
        string workingBaselineHash = "",
        long workingRevision = 0,
        string qualityBatchHash = "") =>
        new(
            kind,
            message,
            sourceFilePath ?? string.Empty,
            sourceSnapshotHash ?? string.Empty,
            modelIdentity,
            compilerResult,
            symmetryResult,
            null,
            workingBaselineHash,
            workingRevision,
            qualityBatchHash);
}
