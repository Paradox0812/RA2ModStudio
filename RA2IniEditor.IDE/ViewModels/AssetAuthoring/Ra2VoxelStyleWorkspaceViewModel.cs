extern alias Ra2Application;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AssetAuthoring;
using Ra2CompiledVoxelStyleRole = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2CompiledVoxelStyleRole;
using Ra2CompiledVoxelStyleRule = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2CompiledVoxelStyleRule;
using Ra2VoxelColourReviewFlags = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourReviewFlags;
using Ra2VoxelGeometryRegionMask = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryRegionMask;
using Ra2VoxelFeatureProtectionMask = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelFeatureProtectionMask;
using Ra2VoxelGeometryProposalResolution = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryProposalResolution;
using Ra2VoxelSemanticPartition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticPartition;
using Ra2VoxelSemanticSymmetryFailureKind = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticSymmetryFailureKind;
using Ra2VoxelSceneSnapshot = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSceneSnapshot;
using Ra2VoxelSemanticAssignment = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticAssignment;
using Ra2VoxelSemanticAssignmentSource = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticAssignmentSource;
using Ra2VoxelSemanticEvidencePackage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEvidencePackage;
using Ra2VoxelSemanticEvidenceBuilder = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEvidenceBuilder;
using Ra2VoxelSemanticEffectiveAssignment = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEffectiveAssignment;
using Ra2VoxelSemanticLayerResolver = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticLayerResolver;
using Ra2VoxelSemanticMaterialRole = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaterialRole;
using Ra2VoxelSemanticPartRole = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticPartRole;
using Ra2VoxelSemanticRemapIntent = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticRemapIntent;
using Ra2VoxelCoordinate = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelCoordinate;
using Ra2VoxelSemanticBrushMode = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticBrushMode;
using Ra2VoxelSemanticBrushFailureKind = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticBrushFailureKind;
using Ra2VoxelSemanticManualMaskLayer = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticManualMaskLayer;
using Ra2VoxelSemanticMaskComposition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaskComposition;
using Ra2VoxelSemanticMaskComposer = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaskComposer;
using Ra2VoxelSemanticSurfaceCoverage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticSurfaceCoverage;
using Ra2VoxelSemanticSurfaceCoverageProjector = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticSurfaceCoverageProjector;
using Ra2VoxelSemanticMaskEditor = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaskEditor;
using Ra2Rgba32 = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2Rgba32;
using Ra2VoxelBaseColourSelection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelBaseColourSelection;
using Ra2VoxelColourAdmissionState = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourAdmissionState;
using Ra2VoxelColourQualityMetric = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourQualityMetric;
using Ra2VoxelColourQualityReport = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourQualityReport;
using Ra2VoxelColourTechniqueCatalog = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourTechniqueCatalog;
using Ra2VoxelColourTechniquePolicy = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourTechniquePolicy;
using Ra2VoxelConfirmedUnitClass = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelConfirmedUnitClass;
using Ra2VoxelUnitClass = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClass;
using Ra2VoxelUnitClassConfirmationSource = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassConfirmationSource;
using Ra2VoxelUnitClassEvidence = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassEvidence;
using Ra2VoxelUnitClassConfirmationResult = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassConfirmationResult;
using Ra2VoxelUnitAdaptationCatalog = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitAdaptationCatalog;
using Ra2VoxelForwardDirection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelForwardDirection;
using Ra2VoxelForwardDirectionSelection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelForwardDirectionSelection;
using Ra2VoxelFormZoneProjection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelFormZoneProjection;
using Ra2VoxelBoundaryIntentProjection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelBoundaryIntentProjection;
using Ra2VoxelFeatureScaleProjection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelFeatureScaleProjection;

namespace RA2IniEditor.IDE.ViewModels.AssetAuthoring;

internal enum Ra2VoxelStylePreviewMode
{
    Original = 0,
    Direct,
    Refined,
    Difference,
    StructureRegions,
    Symmetry,
    Semantics,
    Result,
    Contrast,
    RegionMask,
    Palette,
    FormZones,
    BoundaryIntent,
    RiskOverlay
}

internal enum Ra2VoxelWorkspaceStage
{
    Model = 0,
    Geometry,
    Semantics,
    Colour,
    Review
}

internal enum Ra2VoxelSemanticEditMode
{
    Browse = 0,
    Paint,
    Erase
}

internal sealed record Ra2VoxelStyleRoleRow(
    string Id,
    string Category,
    string PaletteIndex,
    string Sources);

internal sealed record Ra2VoxelStyleRuleRow(
    string Region,
    string Role,
    string Evidence,
    string State);

internal sealed record Ra2VoxelSemanticPartOption(Ra2VoxelSemanticPartRole Value, string Display);
internal sealed record Ra2VoxelSemanticMaterialOption(Ra2VoxelSemanticMaterialRole Value, string Display);
internal sealed record Ra2VoxelUnitClassOption(Ra2VoxelUnitClass Value, string Display);
internal sealed record Ra2VoxelForwardDirectionOption(Ra2VoxelForwardDirection Value, string Display);
internal sealed record Ra2VoxelTechniqueOption(Ra2VoxelColourTechniquePolicy Policy)
{
    public string TechniqueId => Policy.TechniqueId;
    public string DisplayName => Policy.DisplayName;
    public string Description => Policy.Description;
}
internal sealed record Ra2VoxelPaletteColourOption(byte PaletteIndex, string Display, Brush Swatch, string RgbHex);

internal sealed class Ra2VoxelSemanticAssignmentRow : INotifyPropertyChanged
{
    private static readonly IReadOnlyList<Ra2VoxelSemanticPartOption> PartOptions = Array.AsReadOnly(new[]
    {
        new Ra2VoxelSemanticPartOption(Ra2VoxelSemanticPartRole.Unknown, "未知"),
        new Ra2VoxelSemanticPartOption(Ra2VoxelSemanticPartRole.BodyShell, "车体主体"),
        new Ra2VoxelSemanticPartOption(Ra2VoxelSemanticPartRole.Turret, "炮塔"),
        new Ra2VoxelSemanticPartOption(Ra2VoxelSemanticPartRole.Barrel, "炮管"),
        new Ra2VoxelSemanticPartOption(Ra2VoxelSemanticPartRole.Wheel, "车轮"),
        new Ra2VoxelSemanticPartOption(Ra2VoxelSemanticPartRole.Track, "履带"),
        new Ra2VoxelSemanticPartOption(Ra2VoxelSemanticPartRole.Antenna, "天线"),
        new Ra2VoxelSemanticPartOption(Ra2VoxelSemanticPartRole.Attachment, "附加部件")
    });
    private static readonly IReadOnlyList<Ra2VoxelSemanticMaterialOption> MaterialOptions = Array.AsReadOnly(new[]
    {
        new Ra2VoxelSemanticMaterialOption(Ra2VoxelSemanticMaterialRole.Unknown, "未知"),
        new Ra2VoxelSemanticMaterialOption(Ra2VoxelSemanticMaterialRole.PaintedSurface, "涂装表面"),
        new Ra2VoxelSemanticMaterialOption(Ra2VoxelSemanticMaterialRole.Glass, "玻璃"),
        new Ra2VoxelSemanticMaterialOption(Ra2VoxelSemanticMaterialRole.Rubber, "橡胶"),
        new Ra2VoxelSemanticMaterialOption(Ra2VoxelSemanticMaterialRole.BareMetal, "裸露金属"),
        new Ra2VoxelSemanticMaterialOption(Ra2VoxelSemanticMaterialRole.Light, "灯光"),
        new Ra2VoxelSemanticMaterialOption(Ra2VoxelSemanticMaterialRole.DarkOpening, "深色开口"),
        new Ra2VoxelSemanticMaterialOption(Ra2VoxelSemanticMaterialRole.Accent, "强调色")
    });
    internal static IReadOnlyList<Ra2VoxelSemanticPartOption> AvailablePartOptions => PartOptions;
    internal static IReadOnlyList<Ra2VoxelSemanticMaterialOption> AvailableMaterialOptions => MaterialOptions;
    private readonly Action<Ra2VoxelSemanticAssignmentRow> _changed;
    private Ra2VoxelSemanticPartRole _partRole;
    private Ra2VoxelSemanticMaterialRole _materialRole;
    private bool _remapApproved;
    private bool _mirrorLinked = true;
    private bool _suppress;

    internal Ra2VoxelSemanticAssignmentRow(
        string regionId,
        string mirrorRegionId,
        string geometryFacts,
        Ra2VoxelSemanticPartRole suggestedPartRole,
        Ra2VoxelSemanticMaterialRole suggestedMaterialRole,
        double suggestionConfidence,
        string suggestionReason,
        Ra2VoxelSemanticEffectiveAssignment effective,
        Action<Ra2VoxelSemanticAssignmentRow> changed)
    {
        RegionId = regionId;
        MirrorRegionId = mirrorRegionId;
        GeometryFacts = geometryFacts;
        SuggestedPartRole = suggestedPartRole;
        SuggestedMaterialRole = suggestedMaterialRole;
        SuggestionConfidence = suggestionConfidence;
        SuggestionReason = suggestionReason;
        _changed = changed;
        Apply(effective);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public string RegionId { get; }
    public string MirrorRegionId { get; }
    public string GeometryFacts { get; }
    public Ra2VoxelSemanticPartRole SuggestedPartRole { get; }
    public Ra2VoxelSemanticMaterialRole SuggestedMaterialRole { get; }
    public string SuggestedPartDisplay => PartOptions.First(value => value.Value == SuggestedPartRole).Display;
    public string SuggestedMaterialDisplay => MaterialOptions.First(value => value.Value == SuggestedMaterialRole).Display;
    public double SuggestionConfidence { get; }
    public string SuggestionReason { get; }
    public IReadOnlyList<Ra2VoxelSemanticPartOption> PartRoleOptions => PartOptions;
    public IReadOnlyList<Ra2VoxelSemanticMaterialOption> MaterialRoleOptions => MaterialOptions;
    public string SourceText { get; private set; } = "未分类";
    public bool HasHumanOverride { get; private set; }

    public Ra2VoxelSemanticPartRole PartRole
    {
        get => _partRole;
        set { if (_partRole == value) return; _partRole = value; Changed(nameof(PartRole)); }
    }

    public Ra2VoxelSemanticMaterialRole MaterialRole
    {
        get => _materialRole;
        set { if (_materialRole == value) return; _materialRole = value; Changed(nameof(MaterialRole)); }
    }

    public bool RemapApproved
    {
        get => _remapApproved;
        set { if (_remapApproved == value) return; _remapApproved = value; Changed(nameof(RemapApproved)); }
    }

    public bool MirrorLinked
    {
        get => _mirrorLinked;
        set { if (_mirrorLinked == value) return; _mirrorLinked = value; PropertyChanged?.Invoke(this, new(nameof(MirrorLinked))); }
    }

    internal void Apply(Ra2VoxelSemanticEffectiveAssignment value)
    {
        _suppress = true;
        _partRole = value.PartRole;
        _materialRole = value.MaterialRole;
        _remapApproved = value.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved;
        HasHumanOverride = value.Source == Ra2VoxelSemanticAssignmentSource.HumanOverride;
        SourceText = value.Source switch
        {
            Ra2VoxelSemanticAssignmentSource.HumanOverride => "人工覆盖",
            Ra2VoxelSemanticAssignmentSource.AgentSuggestion => "AI 建议",
            _ => "未分类"
        };
        _suppress = false;
        PropertyChanged?.Invoke(this, new(nameof(PartRole)));
        PropertyChanged?.Invoke(this, new(nameof(MaterialRole)));
        PropertyChanged?.Invoke(this, new(nameof(RemapApproved)));
        PropertyChanged?.Invoke(this, new(nameof(SourceText)));
        PropertyChanged?.Invoke(this, new(nameof(HasHumanOverride)));
    }

    private void Changed(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
        if (!_suppress) _changed(this);
    }
}

internal sealed class Ra2VoxelStyleWorkspaceViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly Ra2VoxelStylePreviewCoordinator _coordinator;
    private readonly Ra2VoxelGenerationOrchestrator _generationOrchestrator;
    private readonly Ra2VoxelVoxExportService _voxExportService;
    private readonly Ra2VoxelSemanticSidecarStore _semanticSidecarStore;
    private readonly Func<string?> _projectRootAccessor;
    private readonly Func<DeepSeekRa2AiModel> _modelAccessor;
    private CancellationTokenSource? _operationCancellation;
    private long _generation;
    private Ra2VoxelStyleSourceLoadResult? _source;
    private Ra2VoxelQualityPreviewResult? _qualityPreview;
    private Ra2VoxelStructurePreviewResult? _structurePreview;
    private Ra2VoxelStylePreviewResult? _preview;
    private ImageSource? _originalImage;
    private ImageSource? _directImage;
    private ImageSource? _refinedImage;
    private ImageSource? _symmetryImage;
    private ImageSource? _resultImage;
    private ImageSource? _contrastImage;
    private ImageSource? _regionMaskImage;
    private ImageSource? _paletteImage;
    private ImageSource? _currentPreviewImage;
    private string? _qualitySourcePath;
    private Ra2VoxelWorkingGeometryState? _workingGeometryState;
    private string _styleOverride = string.Empty;
    private string _generationReferencePath = string.Empty;
    private string _generationPalettePath = string.Empty;
    private string _generationBrief = string.Empty;
    private string _generationNegativeConstraints = string.Empty;
    private int _generationResolution = 64;
    private int _generationTimeoutMinutes = 10;
    private string _generationProgressText = "尚未准备生成任务。";
    private Ra2VoxelGenerationSession? _generationSession;
    private string _statusText = "请选择当前项目内的 VOX 或 VXL 体素模型。";
    private bool _isBusy;
    private bool _isError;
    private Ra2VoxelAcceptedCandidate? _acceptedCandidate;
    private bool _hasPendingStyleChanges;
    private bool _isSliceFallback;
    private bool _disposed;
    private Ra2VoxelStylePreviewMode _previewMode;
    private Ra2VoxelWorkspaceStage _selectedWorkflowStage = Ra2VoxelWorkspaceStage.Model;
    private Ra2VoxelSemanticEvidencePackage? _semanticEvidence;
    private Ra2VoxelSemanticMaskCompilerResult? _semanticCompilerResult;
    private readonly Dictionary<string, Ra2VoxelSemanticAssignment> _semanticManualOverrides = new(StringComparer.Ordinal);
    private Ra2VoxelSemanticManualMaskLayer? _semanticManualMaskLayer;
    private readonly List<Ra2VoxelSemanticManualMaskLayer> _semanticUndo = [];
    private readonly List<Ra2VoxelSemanticManualMaskLayer> _semanticRedo = [];
    private Ra2VoxelSemanticEditMode _semanticEditMode;
    private int _semanticBrushSize = 1;
    private bool _semanticMirrorBrush = true;
    private Ra2VoxelSemanticPartRole _semanticBrushPartRole;
    private Ra2VoxelSemanticMaterialRole _semanticBrushMaterialRole;
    private bool _semanticBrushRemapApproved;
    private string _semanticEditStatus = "浏览模式：左键点击模型选择区域；右键拖动旋转。";
    private Ra2VoxelSemanticReviewDimension _semanticReviewDimension = Ra2VoxelSemanticReviewDimension.Part;
    private SemanticStrokeTransaction? _semanticStroke;
    private bool _semanticSuggestionsAccepted;
    private IReadOnlyList<Ra2VoxelSemanticAssignment> _loadedSemanticSuggestions = [];
    private long _semanticAuthoringRevision;
    private long _semanticSavedRevision;
    private bool _isSemanticPersistenceBusy;
    private string? _semanticSidecarPath;
    private string _semanticPersistenceStatus = "语义分划尚未保存。";
    private string _semanticInstructions = string.Empty;
    private Ra2VoxelSemanticAssignmentRow? _selectedSemanticAssignment;
    private Ra2VoxelUnitClassEvidence? _unitClassEvidence;
    private Ra2VoxelConfirmedUnitClass? _confirmedUnitClass;
    private Ra2VoxelColourSkillRoute? _colourSkillRoute;
    private Ra2VoxelUnitClassOption? _selectedUnitClass;
    private Ra2VoxelForwardDirectionOption _selectedForwardDirection;
    private Ra2VoxelPaletteColourOption? _selectedBaseColour;
    private Ra2VoxelBaseColourSelection? _baseColourSelection;
    private Ra2VoxelTechniqueOption _selectedTechnique;
    private string _unitClassStatus = "尚未选择";
    private bool _qualityWarningsAccepted;

    internal Ra2VoxelStyleWorkspaceViewModel(
        Ra2VoxelStylePreviewCoordinator coordinator,
        Func<string?> projectRootAccessor,
        Func<DeepSeekRa2AiModel> modelAccessor,
        Ra2VoxelGenerationOrchestrator? generationOrchestrator = null,
        Ra2VoxelVoxExportService? voxExportService = null,
        Ra2VoxelSemanticSidecarStore? semanticSidecarStore = null)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _projectRootAccessor = projectRootAccessor ?? throw new ArgumentNullException(nameof(projectRootAccessor));
        _modelAccessor = modelAccessor ?? throw new ArgumentNullException(nameof(modelAccessor));
        _generationOrchestrator = generationOrchestrator ?? new Ra2VoxelGenerationOrchestrator(coordinator);
        _voxExportService = voxExportService ?? new Ra2VoxelVoxExportService();
        _semanticSidecarStore = semanticSidecarStore ?? new Ra2VoxelSemanticSidecarStore();
        UnitClassOptions = Array.AsReadOnly(new[]
        {
            new Ra2VoxelUnitClassOption(Ra2VoxelUnitClass.Ground, "地面载具"),
            new Ra2VoxelUnitClassOption(Ra2VoxelUnitClass.Air, "空中载具"),
            new Ra2VoxelUnitClassOption(Ra2VoxelUnitClass.LargeSurface, "大型水面单位"),
            new Ra2VoxelUnitClassOption(Ra2VoxelUnitClass.Unknown, "未知 / 保守模式")
        });
        ForwardDirectionOptions = Array.AsReadOnly(new[]
        {
            new Ra2VoxelForwardDirectionOption(Ra2VoxelForwardDirection.Unknown, "尚未确认"),
            new Ra2VoxelForwardDirectionOption(Ra2VoxelForwardDirection.PositiveX, "+X"),
            new Ra2VoxelForwardDirectionOption(Ra2VoxelForwardDirection.NegativeX, "-X"),
            new Ra2VoxelForwardDirectionOption(Ra2VoxelForwardDirection.PositiveY, "+Y"),
            new Ra2VoxelForwardDirectionOption(Ra2VoxelForwardDirection.NegativeY, "-Y")
        });
        _selectedForwardDirection = ForwardDirectionOptions[0];
        TechniqueOptions = Array.AsReadOnly(Ra2VoxelColourTechniqueCatalog.All
            .Select(value => new Ra2VoxelTechniqueOption(value)).ToArray());
        _selectedTechnique = TechniqueOptions.Single(value =>
            string.Equals(value.TechniqueId, Ra2VoxelColourTechniqueCatalog.Default.TechniqueId, StringComparison.Ordinal));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> StyleSources { get; } = [];
    public ObservableCollection<Ra2VoxelStyleRoleRow> Roles { get; } = [];
    public ObservableCollection<Ra2VoxelStyleRuleRow> Rules { get; } = [];
    public ObservableCollection<string> ReviewIssues { get; } = [];
    public ObservableCollection<Ra2VoxelQualityMetricRow> QualityMetrics { get; } = [];
    public ObservableCollection<Ra2VoxelSemanticRegionRow> SemanticRegions { get; } = [];
    public ObservableCollection<Ra2VoxelSemanticAssignmentRow> SemanticAssignments { get; } = [];
    public ObservableCollection<Ra2VoxelPaletteColourOption> BaseColourOptions { get; } = [];
    public ObservableCollection<string> ColourQualityMetrics { get; } = [];
    public ObservableCollection<string> ColourQualityWarnings { get; } = [];
    public ObservableCollection<string> ColourQualityFormZones { get; } = [];
    public ObservableCollection<string> ColourQualityBoundaries { get; } = [];
    public ObservableCollection<string> ColourQualityAccents { get; } = [];
    public ObservableCollection<string> ColourQualityGameScale { get; } = [];
    public IReadOnlyList<Ra2VoxelUnitClassOption> UnitClassOptions { get; }
    public IReadOnlyList<Ra2VoxelForwardDirectionOption> ForwardDirectionOptions { get; }
    public IReadOnlyList<Ra2VoxelTechniqueOption> TechniqueOptions { get; }
    public Ra2VoxelSemanticAssignmentRow? SelectedSemanticAssignment
    {
        get => _selectedSemanticAssignment;
        set { if (ReferenceEquals(_selectedSemanticAssignment, value)) return; _selectedSemanticAssignment = value; OnPropertyChanged(); }
    }

    internal string? ProjectRootPath => _projectRootAccessor();
    public string SourcePath => _source?.IsGeneratedSession == true ? "会话内生成结果（未写入文件）" : _source?.FilePath ?? "尚未选择";
    public string SourceName => _source?.DisplayName ?? (_source?.FilePath is string path ? Path.GetFileName(path) : "未载入模型");
    public string SourceFacts => _source?.Snapshot is { } snapshot
        ? $"{snapshot.Part.XSize} × {snapshot.Part.YSize} × {snapshot.Part.ZSize} · {snapshot.OccupancyCount:N0} 体素"
        : "选择项目内的单模型 VOX，或带明确 PAL 的单 Section VXL";
    public string ModelDisplayName => DeepSeekRa2AiModelCatalog.GetOption(_modelAccessor()).DisplayName;
    public string QualitySourceName => _source?.IsGeneratedSession == true && _source.SourceGlb is not null
        ? "会话内生成 GLB"
        : _qualitySourcePath is string path ? Path.GetFileName(path) : "尚未选择 GLB";
    public string QualityProvenanceText => _qualityPreview?.Provenance switch
    {
        Ra2VoxelQualitySourceProvenance.Verified => "来源已由 mesh.glb 哈希确认",
        Ra2VoxelQualitySourceProvenance.UserPaired => "用户配对；无法从当前文件证明原始来源",
        Ra2VoxelQualitySourceProvenance.Mismatch => "来源哈希冲突",
        _ => "尚未生成质量候选"
    };
    public string WorkingGeometryText => _workingGeometryState is { } working
        ? $"当前几何：{working.DisplayName} · r{working.Revision} · {working.Snapshot.CanonicalHash[..12]}"
        : "当前几何：尚未载入";
    public string NormalComparisonText => _qualityPreview?.ReviewPackage is { } review
        ? Ra2VoxelQualityReviewProjection.ProjectNormalComparison(review)
        : "生成候选后显示直接转换和平滑候选的法线差异。";
    public string QualityCandidatesText => _qualityPreview?.ReviewPackage is { } review
        ? Ra2VoxelQualityReviewProjection.ProjectCandidateReviews(review)
        : "尚未生成本地质量候选。";
    public string QualityAdmissionText => _qualityPreview?.ReviewPackage?.Admission is { } admission
        ? admission.IsAdmitted
            ? $"已准入 · {admission.CandidateId}；新增 {admission.AddedCellCount:N0}，移除 {admission.RemovedCellCount:N0}。"
            : "未准入任何平滑候选；当前保留直接转换，不能把未改善候选用于本会话。"
        : "尚未执行本地候选准入。";
    public string StructureProtectionText => _qualityPreview?.ReviewPackage is { } qualityReview
        ? $"冻结 {qualityReview.ProtectionMask.FrozenCellCount:N0} · 过渡 {qualityReview.ProtectionMask.TransitionCellCount:N0} · " +
          $"端点 {qualityReview.SourceFacts.ProtectedEndpointCount:N0} · 分支 {qualityReview.SourceFacts.ProtectedBranchCellCount:N0} · " +
          $"受保护组件 {qualityReview.SourceFacts.ProtectedComponentCount:N0}"
        : "尚未生成结构保护事实。";
    public string SemanticLegendText => "Agent 提案图例：镜像补全=青 · 计划移除=琥珀 · 受保护薄结构=蓝 · 未选择/保留=紫 · 背景=灰";
    public string SemanticReviewText
    {
        get
        {
            if (!HasStructurePartition || _structurePreview?.Partition is not { } partition)
                return "尚未执行 Agent 几何提案；本地候选不会自动调用模型。";
            int selected = _structurePreview.CompilerResult?.Proposal?.Operations.Count ?? 0;
            string resolution = _structurePreview.CompilerResult?.Proposal?.Resolution == Ra2VoxelGeometryProposalResolution.Arbitration
                ? "第三轮已仲裁"
                : "主分析与审阅一致";
            string admission = _structurePreview.IsSuccess
                ? $"候选已通过最低安全线，新增 {_structurePreview.SymmetryResult!.AddedCellCount:N0} / 移除 {_structurePreview.SymmetryResult.RemovedCellCount:N0}"
                : "最终提案已生成，但没有候选通过最低安全线";
            return $"对称轴 2X={partition.Evidence.SelectedPlaneTwiceX} · 稀疏操作 {selected:N0} · {resolution} · " +
                   $"未选择体素 {partition.UncertainCellCount:N0} · {admission}";
        }
    }
    public string PaletteContrastText => _preview?.ContrastFacts is { } facts
        ? facts.ChangedRoleCount > 0
            ? $"可选对比度候选调整了 {facts.ChangedRoleCount} 个颜色角色；最小明度间隔 {facts.MinimumBodyLuminanceSeparationBefore:F1} → {facts.MinimumBodyLuminanceSeparationAfter:F1}，显式色板选择保持不变。"
            : $"当前普通着色的最小明度间隔为 {facts.MinimumBodyLuminanceSeparationBefore:F1}，无需生成额外对比度候选。"
        : "编译普通着色后显示可选的色板对比度建议。";
    public string StatusText => _statusText;
    public bool IsBusy => _isBusy;
    public bool IsError => _isError;
    public bool HasSource => _source?.IsSuccess == true;
    public Ra2VoxelWorkspaceStage SelectedWorkflowStage => _selectedWorkflowStage;
    public bool IsModelStage => _selectedWorkflowStage == Ra2VoxelWorkspaceStage.Model;
    public bool IsGeometryStage => _selectedWorkflowStage == Ra2VoxelWorkspaceStage.Geometry;
    public bool IsSemanticsStage => _selectedWorkflowStage == Ra2VoxelWorkspaceStage.Semantics;
    public bool IsColourStage => _selectedWorkflowStage == Ra2VoxelWorkspaceStage.Colour;
    public bool IsReviewStage => _selectedWorkflowStage == Ra2VoxelWorkspaceStage.Review;
    public string CurrentWorkflowStageText => _selectedWorkflowStage switch
    {
        Ra2VoxelWorkspaceStage.Model => "模型",
        Ra2VoxelWorkspaceStage.Geometry => "几何",
        Ra2VoxelWorkspaceStage.Semantics => "分划与标注",
        Ra2VoxelWorkspaceStage.Colour => "上色",
        _ => "审阅与导出"
    };
    public string ModelStageStatus => HasSource ? "已完成" : "待开始";
    public string GeometryStageStatus => !HasSource ? "待开始" : _workingGeometryState?.Revision > 0 ? "已完成" : "可选";
    public string SemanticsStageStatus => !HasSource ? "待开始" : IsSemanticSidecarDirty
        ? "有未保存更改"
        : !HasSemanticEvidence ? "可开始"
        : ResolveSemanticSurfaceCoverage() is { KnownVisibleSurfaceRatio: >= 0.98d } ? "已完成" : "可上色/待完善";
    public string ColourStageStatus => !HasSemanticEvidence ? "待开始" : HasPreview ? "已完成"
        : HasConfirmedUnitClass || SelectedBaseColour is not null ? "配置中" : "可开始";
    public string ReviewStageStatus => _acceptedCandidate is not null ? "已固化" : HasPreview ? "待审阅" : "待开始";
    public int UnclassifiedSemanticRegionCount => SemanticAssignments.Count(value =>
        value.PartRole == Ra2VoxelSemanticPartRole.Unknown || value.MaterialRole == Ra2VoxelSemanticMaterialRole.Unknown);
    public string WorkflowNextActionText
    {
        get
        {
            if (!HasSource) return "下一步：载入项目内的 VOX/VXL，或从参考图生成模型";
            if (!HasSemanticEvidence) return "下一步：创建人工区域或载入已有分划";
            string coverage = ResolveSemanticSurfaceCoverage() is { } surface
                ? $"可见表面已标注 {surface.KnownVisibleSurfaceCellCount:N0}/{surface.VisibleSurfaceCellCount:N0}（{surface.KnownVisibleSurfaceRatio * 100d:F1}%）"
                : "可见表面覆盖率暂不可用";
            string optionalRegions = UnclassifiedSemanticRegionCount > 0 ? "；其余未分类分区可按需处理" : string.Empty;
            if (!HasConfirmedUnitClass) return $"{coverage}{optionalRegions}。下一步：进入上色并人工确认单位类型";
            if (_baseColourSelection is null) return $"{coverage}{optionalRegions}。下一步：选择当前 RA2 色盘中的主体基准色";
            if (!HasPreview) return $"{coverage}{optionalRegions}。下一步：编译着色预览";
            if (_acceptedCandidate is null) return $"{coverage}{optionalRegions}。下一步：审阅质量警告并固化最终候选";
            return "已完成：可以导出固化的 VOX 候选";
        }
    }
    public bool HasPreview => _preview?.IsSuccess == true;
    public bool HasQualityCandidates => _qualityPreview?.IsSuccess == true;
    public bool HasRefinedCandidate => _qualityPreview?.ReviewPackage?.Admission.IsAdmitted == true &&
        _qualityPreview.RefinedCandidate is not null && _qualityPreview.DirectCandidate is not null &&
        !string.Equals(_qualityPreview.RefinedCandidate.CanonicalHash, _qualityPreview.DirectCandidate.CanonicalHash, StringComparison.Ordinal);
    public bool HasQualityDifference => HasRefinedCandidate &&
        (_qualityPreview!.ReviewPackage!.Admission.AddedCellCount > 0 ||
         _qualityPreview.ReviewPackage.Admission.RemovedCellCount > 0);
    public bool HasStructurePartition => _structurePreview?.Partition is not null && IsStructureResultCurrent();
    public bool HasSymmetryCandidate => _structurePreview?.IsSuccess == true &&
        (IsStructureResultCurrent() || IsCurrentWorkingSnapshot(_structurePreview.Candidate));
    public bool HasContrastCandidate => _preview?.ContrastResultSnapshot is not null;
    public bool CanChooseSource => !_isBusy;
    public bool CanGenerateModel => !_isBusy && ProjectRootPath is not null && !string.IsNullOrWhiteSpace(_generationReferencePath);
    public bool CanChooseQualitySource => HasSource && !_isBusy;
    public bool CanGenerateQuality => HasSource && (!_source!.IsGeneratedSession || _source.SourceGlb is not null) &&
        (_source.IsGeneratedSession || !string.IsNullOrWhiteSpace(_qualitySourcePath)) && !_isBusy;
    public bool CanAnalyzeStructure => HasQualityCandidates && IsQualityBatchCurrent() &&
        _qualityPreview?.SymmetryEvidence is not null && !_isBusy;
    public string StructureRecognitionToolTip => _coordinator.IsStructureRecognitionConfigured(_modelAccessor())
        ? "通常执行主分析和审阅；操作不一致时追加第三轮仲裁，必要时可先补查一次有界证据。只生成会话内审阅结果。"
        : "当前 DeepSeek 配置尚不可用；点击后会显示具体配置原因，不会静默执行或修改文件。";
    public string SemanticInstructions
    {
        get => _semanticInstructions;
        set { _semanticInstructions = value ?? string.Empty; OnPropertyChanged(); }
    }
    public bool CanAnalyzeSemantics => HasSource && ActiveGeometrySnapshot is not null && !_isBusy;
    public bool HasSemanticEvidence => _semanticEvidence is not null && ActiveGeometrySnapshot is { } snapshot &&
        string.Equals(_semanticEvidence.SourceSnapshotHash, snapshot.CanonicalHash, StringComparison.Ordinal);
    public bool HasSemanticSuggestions => CurrentSemanticSuggestions.Count > 0 && _semanticEvidence is not null;
    public bool CanAcceptSemanticSuggestions => HasSemanticSuggestions && !_semanticSuggestionsAccepted && !_isBusy;
    public bool CanDiscardSemanticSuggestions => HasSemanticSuggestions && !_isBusy;
    public string SemanticStatusText => _semanticEvidence is null
        ? "尚未生成语义区域。AI 只读取文本化几何证据；人工覆盖始终优先。"
        : $"{_semanticEvidence.Regions.Count} 个区域 · AI {(CurrentSemanticSuggestions.Count == 0 ? "无可用建议" : _semanticSuggestionsAccepted ? "建议已启用" : "建议待接受")} · 人工覆盖 {_semanticManualOverrides.Count} 项" +
          $" · 画笔体素 {_semanticManualMaskLayer?.Overrides.Count ?? 0}" +
          (_semanticCompilerResult?.UsedArbitration == true ? " · 已执行第三轮仲裁" : string.Empty);
    public bool IsSemanticSidecarDirty => _semanticAuthoringRevision != _semanticSavedRevision;
    public bool HasUnsavedSemanticSidecarChanges => IsSemanticSidecarDirty;
    public bool CanSaveSemanticSidecar => !_isBusy && !_isSemanticPersistenceBusy && HasSemanticEvidence &&
        (HasPersistableSemanticState || IsSemanticSidecarDirty);
    public bool CanLoadSemanticSidecar => !_isBusy && !_isSemanticPersistenceBusy && ActiveGeometrySnapshot is not null && ProjectRootPath is not null;
    public string SemanticPersistenceStatus => _semanticPersistenceStatus;
    public string SemanticSidecarInitialDirectory => ProjectRootPath ?? Environment.CurrentDirectory;
    public string SemanticSidecarSuggestedFileName
    {
        get
        {
            string sourceName = _source?.FilePath is string path ? Path.GetFileName(path) : "voxel-model";
            return $"{sourceName}.semantic.json";
        }
    }
    public Ra2VoxelSemanticEditMode SemanticEditMode => _semanticEditMode;
    public bool IsSemanticBrowseMode => _semanticEditMode == Ra2VoxelSemanticEditMode.Browse;
    public bool IsSemanticPaintMode => _semanticEditMode == Ra2VoxelSemanticEditMode.Paint;
    public bool IsSemanticEraseMode => _semanticEditMode == Ra2VoxelSemanticEditMode.Erase;
    public IReadOnlyList<int> SemanticBrushSizes { get; } = Array.AsReadOnly(new[] { 1, 2, 3 });
    public int SemanticBrushSize
    {
        get => _semanticBrushSize;
        set
        {
            int normalized = Math.Clamp(value, 1, 3);
            if (_semanticBrushSize == normalized) return;
            _semanticBrushSize = normalized;
            OnPropertyChanged();
            UpdateSemanticEditStatus();
        }
    }
    public bool SemanticMirrorBrush
    {
        get => _semanticMirrorBrush;
        set
        {
            if (_semanticMirrorBrush == value) return;
            _semanticMirrorBrush = value;
            OnPropertyChanged();
            UpdateSemanticEditStatus();
        }
    }
    public IReadOnlyList<Ra2VoxelSemanticPartOption> SemanticBrushPartOptions => Ra2VoxelSemanticAssignmentRow.AvailablePartOptions;
    public IReadOnlyList<Ra2VoxelSemanticMaterialOption> SemanticBrushMaterialOptions => Ra2VoxelSemanticAssignmentRow.AvailableMaterialOptions;
    public Ra2VoxelSemanticPartRole SemanticBrushPartRole
    {
        get => _semanticBrushPartRole;
        set { if (_semanticBrushPartRole == value) return; _semanticBrushPartRole = value; OnPropertyChanged(); UpdateSemanticEditStatus(); }
    }
    public Ra2VoxelSemanticMaterialRole SemanticBrushMaterialRole
    {
        get => _semanticBrushMaterialRole;
        set { if (_semanticBrushMaterialRole == value) return; _semanticBrushMaterialRole = value; OnPropertyChanged(); UpdateSemanticEditStatus(); }
    }
    public bool SemanticBrushRemapApproved
    {
        get => _semanticBrushRemapApproved;
        set { if (_semanticBrushRemapApproved == value) return; _semanticBrushRemapApproved = value; OnPropertyChanged(); UpdateSemanticEditStatus(); }
    }
    public bool CanUndoSemanticBrush => _semanticUndo.Count > 0 && !_isBusy && _semanticStroke is null;
    public bool CanRedoSemanticBrush => _semanticRedo.Count > 0 && !_isBusy && _semanticStroke is null;
    public string SemanticEditStatus => _semanticEditStatus;
    public Ra2VoxelSemanticReviewDimension SemanticReviewDimension => _semanticReviewDimension;
    public bool IsSemanticPartReview => _semanticReviewDimension == Ra2VoxelSemanticReviewDimension.Part;
    public bool IsSemanticMaterialReview => _semanticReviewDimension == Ra2VoxelSemanticReviewDimension.Material;
    public IReadOnlyList<Ra2VoxelSemanticLegendItem> SemanticReviewLegend => _semanticReviewDimension == Ra2VoxelSemanticReviewDimension.Part
        ? Ra2VoxelSemanticReviewPalette.PartLegend
        : Ra2VoxelSemanticReviewPalette.MaterialLegend;
    public Ra2VoxelUnitClassOption? SelectedUnitClass
    {
        get => _selectedUnitClass;
        set
        {
            if (Equals(_selectedUnitClass, value)) return;
            _selectedUnitClass = value;
            _confirmedUnitClass = null;
            _colourSkillRoute = null;
            OnPropertyChanged();
            InvalidateColourCandidate("单位类型选择已更改，请确认后重新编译。");
            RaiseColourInputProperties();
        }
    }
    public Ra2VoxelPaletteColourOption? SelectedBaseColour
    {
        get => _selectedBaseColour;
        set
        {
            if (Equals(_selectedBaseColour, value)) return;
            _selectedBaseColour = value;
            _baseColourSelection = null;
            if (value is not null && ActiveGeometrySnapshot is { } snapshot)
            {
                var selected = Ra2VoxelBaseColourSelection.Create(
                    snapshot.Palette, snapshot.Palette.ProfileHash, value.PaletteIndex);
                if (selected.IsSuccess) _baseColourSelection = selected.Selection;
            }
            OnPropertyChanged();
            InvalidateColourCandidate("主体基准色已更改；本地候选需要重新生成，不会增加模型调用。");
            RaiseColourInputProperties();
        }
    }
    public Ra2VoxelForwardDirectionOption SelectedForwardDirection
    {
        get => _selectedForwardDirection;
        set
        {
            if (value is null || Equals(_selectedForwardDirection, value)) return;
            _selectedForwardDirection = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ForwardDirectionStatusText));
            InvalidateColourCandidate("人工前向已更改；形体区、边界和上色候选需要重新生成。");
        }
    }
    public string ForwardDirectionStatusText => _selectedForwardDirection.Value == Ra2VoxelForwardDirection.Unknown
        ? "状态：尚未确认。前/后保持未知，候选必须人工审阅。"
        : $"状态：已确认 {_selectedForwardDirection.Display}。只用于区分前/后，不改变模型方向或几何。";
    public Ra2VoxelTechniqueOption SelectedTechnique
    {
        get => _selectedTechnique;
        set
        {
            if (value is null || Equals(_selectedTechnique, value)) return;
            _selectedTechnique = value;
            OnPropertyChanged();
            InvalidateColourCandidate("上色技法已更改；本地候选需要重新生成，不会增加模型调用。");
            RaiseColourInputProperties();
        }
    }
    public string UnitClassStatusText => _unitClassStatus;
    public string UnitClassSkillText => _colourSkillRoute is null
        ? "人工确认单位类型后显示唯一 colouring Skill。"
        : $"{_colourSkillRoute.ColourSkill.Name}@{_colourSkillRoute.ColourSkill.Version} · 人工选择，Host 确定性路由";
    public string BaseColourStatusText => _baseColourSelection is null
        ? "未选择。必须从当前 active palette 的 opaque / non-remap 条目中人工锁定。"
        : $"{ActiveGeometrySnapshot!.Palette.ProfileId} · {_baseColourSelection.PaletteProfileHash[..12]} · " +
          $"#{_baseColourSelection.PaletteIndex} · {SelectedBaseColour!.RgbHex} · 主体基准色由人工锁定 · " +
          $"当前连续索引色阶 #{(_baseColourSelection.PaletteIndex / 16) * 16}–#{((_baseColourSelection.PaletteIndex / 16) * 16) + 15}";
    public Brush? BaseColourSwatch => _selectedBaseColour?.Swatch;
    public string TechniqueDescription => $"{_selectedTechnique.DisplayName} · revision {_selectedTechnique.Policy.Revision} · " +
        $"{_selectedTechnique.Description} 只改变相对明暗、边缘和材质分离，不改变颜色主题。";
    public bool CanSelectUnitClass => HasSource && ActiveGeometrySnapshot is not null && !_isBusy;
    public bool CanConfirmUnitClass => CanSelectUnitClass && _selectedUnitClass is not null;
    public bool HasConfirmedUnitClass => _confirmedUnitClass is not null && _colourSkillRoute is not null;
    public bool CanEditColourInputs => HasSource && !_isBusy;
    public string ColourQualityStatusText => _preview?.Materialization?.Ordinary?.Quality is { } quality
        ? quality.State switch
        {
            Ra2VoxelColourAdmissionState.Blocked => "已阻止：候选未通过硬门，不能固化或导出。",
            Ra2VoxelColourAdmissionState.NeedsReview => _qualityWarningsAccepted
                ? "需要审阅：本 generation 的警告已由人工确认，可固化候选。"
                : "需要审阅：请查看警告并显式确认后再固化。",
            _ => "可审阅：技术硬门与自动质量门已通过；VisualAcceptance 仍为 Pending。"
        }
        : "尚未生成 4E 上色质量报告。";
    public bool HasReviewableColourWarnings => _preview?.Materialization?.Ordinary?.Quality.State ==
        Ra2VoxelColourAdmissionState.NeedsReview;
    public bool QualityWarningsAccepted
    {
        get => _qualityWarningsAccepted;
        set
        {
            if (_qualityWarningsAccepted == value) return;
            _qualityWarningsAccepted = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ColourQualityStatusText));
            OnPropertyChanged(nameof(CanAccept));
        }
    }
    public bool CanCompile => HasSource && !_isBusy && _confirmedUnitClass is not null &&
        _colourSkillRoute is not null && _baseColourSelection is not null && _selectedTechnique is not null;
    public bool CanCancel => _isBusy;
    public bool CanAccept
    {
        get
        {
            if (_isBusy || !TryGetMaterializableCandidate(out Ra2VoxelSceneSnapshot? snapshot, out Ra2VoxelAcceptedCandidateKind kind, out _))
                return false;
            if (!CanAdmitColourCandidate(kind))
                return false;
            return snapshot is not null && (_acceptedCandidate is null ||
                _acceptedCandidate.Kind != kind ||
                !string.Equals(_acceptedCandidate.CanonicalHash, snapshot.CanonicalHash, StringComparison.Ordinal));
        }
    }
    public bool IsAccepted => _acceptedCandidate is not null;
    public bool CanExportVox => _acceptedCandidate is not null && !_isBusy;
    public string AcceptanceText => _acceptedCandidate is { } candidate
        ? $"已固化最终候选：{candidate.DisplayName} · {candidate.Snapshot.OccupancyCount:N0} 体素 · {candidate.CanonicalHash[..12]}。"
        : "固化后，导出始终使用该不可变候选；切换审阅视图不会改变它。";
    public string ExportSuggestedFileName => _acceptedCandidate?.SuggestedFileName ?? "voxel-candidate.vox";
    internal string? ExportInitialDirectory => _source?.FilePath is string path
        ? Path.GetDirectoryName(path)
        : ProjectRootPath;
    public string PlanTitle => _preview?.Plan?.Title ?? "尚未编译风格计划";
    public string PlanSummary => _preview?.Plan?.Summary ?? "编译后可审阅颜色角色、区域规则和风险。";
    public ImageSource? CurrentPreviewImage => _currentPreviewImage;
    public Ra2VoxelStylePreviewMode PreviewMode => _previewMode;
    public int PreviewModeIndex
    {
        get => (int)_previewMode;
        set
        {
            if (Enum.IsDefined(typeof(Ra2VoxelStylePreviewMode), value))
                SetPreviewMode((Ra2VoxelStylePreviewMode)value);
        }
    }
    public bool IsOriginalMode => _previewMode == Ra2VoxelStylePreviewMode.Original;
    public bool IsDirectMode => _previewMode == Ra2VoxelStylePreviewMode.Direct;
    public bool IsRefinedMode => _previewMode == Ra2VoxelStylePreviewMode.Refined;
    public bool IsDifferenceMode => _previewMode == Ra2VoxelStylePreviewMode.Difference;
    public bool IsStructureRegionsMode => _previewMode == Ra2VoxelStylePreviewMode.StructureRegions;
    public bool IsSymmetryMode => _previewMode == Ra2VoxelStylePreviewMode.Symmetry;
    public bool IsSemanticsMode => _previewMode == Ra2VoxelStylePreviewMode.Semantics;
    public bool IsResultMode => _previewMode == Ra2VoxelStylePreviewMode.Result;
    public bool IsContrastMode => _previewMode == Ra2VoxelStylePreviewMode.Contrast;
    public bool IsRegionMaskMode => _previewMode == Ra2VoxelStylePreviewMode.RegionMask;
    public bool IsPaletteMode => _previewMode == Ra2VoxelStylePreviewMode.Palette;
    public bool IsFormZonesMode => _previewMode == Ra2VoxelStylePreviewMode.FormZones;
    public bool IsBoundaryIntentMode => _previewMode == Ra2VoxelStylePreviewMode.BoundaryIntent;
    public bool IsRiskOverlayMode => _previewMode == Ra2VoxelStylePreviewMode.RiskOverlay;
    public bool CanUseRev7DiagnosticPreviews => !_isBusy &&
        _preview?.Materialization?.SemanticIntegration is { FormZones: not null, BoundaryIntents: not null, FeatureScale: not null };
    public bool IsThreeDimensionalPreview => !_isSliceFallback && !IsPaletteMode && CurrentPreviewSnapshot is not null;
    public bool IsImagePreviewVisible => !IsThreeDimensionalPreview;
    public bool CanUseSliceFallback => !IsPaletteMode && CurrentPreviewSnapshot is not null;
    public bool CanUseQualityCandidate => !_isBusy && IsQualityCandidateMode(_previewMode) &&
        _qualityPreview?.IsSuccess == true &&
        IsQualityBatchCurrent() &&
        (_previewMode != Ra2VoxelStylePreviewMode.Refined || _qualityPreview.ReviewPackage?.Admission.IsAdmitted == true) &&
        (_previewMode != Ra2VoxelStylePreviewMode.Symmetry || HasSymmetryCandidate) &&
        (_qualityPreview?.IsGeneratedSession == true || string.Equals(_qualityPreview?.FilePath, _qualitySourcePath, PathComparison));
    public bool IsSliceFallback => _isSliceFallback;
    public string GenerationReferencePath => _generationReferencePath;
    public string GenerationReferenceFacts => string.IsNullOrWhiteSpace(_generationReferencePath)
        ? "尚未选择参考图"
        : Path.GetFileName(_generationReferencePath);
    public string GenerationPaletteFacts => _source?.IsSuccess == true
        ? "复用当前模型色板"
        : string.IsNullOrWhiteSpace(_generationPalettePath) ? "需要选择项目内 768 字节 PAL" : Path.GetFileName(_generationPalettePath);
    public string GenerationProgressText => _generationProgressText;
    public bool IsGenerationAwaitingConsent => _generationSession?.CanConfirm == true;
    public string GenerationProviderText => _generationSession?.CanConfirm == true
        ? $"{_generationSession.ProviderId} · {_generationSession.ModelId}"
        : "固定内置 Tencent Hunyuan 3D Provider；免费余额无法由本程序确认。";
    public string GenerationBrief
    {
        get => _generationBrief;
        set { _generationBrief = value ?? string.Empty; OnPropertyChanged(); }
    }
    public string GenerationNegativeConstraints
    {
        get => _generationNegativeConstraints;
        set { _generationNegativeConstraints = value ?? string.Empty; OnPropertyChanged(); }
    }
    public int GenerationResolution
    {
        get => _generationResolution;
        set { _generationResolution = value; OnPropertyChanged(); }
    }
    public int GenerationTimeoutMinutes
    {
        get => _generationTimeoutMinutes;
        set { _generationTimeoutMinutes = value; OnPropertyChanged(); }
    }
    internal Ra2VoxelSceneSnapshot? CurrentPreviewSnapshot => _previewMode switch
    {
        Ra2VoxelStylePreviewMode.Original => _source?.Snapshot,
        Ra2VoxelStylePreviewMode.Direct => _qualityPreview?.DirectCandidate,
        Ra2VoxelStylePreviewMode.Refined => _qualityPreview?.RefinedCandidate,
        Ra2VoxelStylePreviewMode.Difference => _qualityPreview?.RefinedCandidate,
        Ra2VoxelStylePreviewMode.StructureRegions => HasStructurePartition ? _qualityPreview?.RefinedCandidate : null,
        Ra2VoxelStylePreviewMode.Symmetry => HasSymmetryCandidate ? _structurePreview?.Candidate : null,
        Ra2VoxelStylePreviewMode.Semantics => _semanticEvidence is not null ? ActiveGeometrySnapshot : null,
        Ra2VoxelStylePreviewMode.Result => _preview?.ResultSnapshot,
        Ra2VoxelStylePreviewMode.Contrast => _preview?.ContrastResultSnapshot,
        Ra2VoxelStylePreviewMode.RegionMask => ActiveGeometrySnapshot,
        Ra2VoxelStylePreviewMode.FormZones => CanUseRev7DiagnosticPreviews ? ActiveGeometrySnapshot : null,
        Ra2VoxelStylePreviewMode.BoundaryIntent => CanUseRev7DiagnosticPreviews ? ActiveGeometrySnapshot : null,
        Ra2VoxelStylePreviewMode.RiskOverlay => CanUseRev7DiagnosticPreviews ? ActiveGeometrySnapshot : null,
        _ => null
    };
    internal Ra2VoxelSceneSnapshot? ActiveGeometrySnapshot => _workingGeometryState?.Snapshot;
    internal Ra2VoxelWorkingGeometryState? WorkingGeometryState => _workingGeometryState;
    internal Ra2VoxelAcceptedCandidate? AcceptedCandidate => _acceptedCandidate;
    internal Ra2VoxelGeometryRegionMask? CurrentPreviewRegionMask =>
        _previewMode == Ra2VoxelStylePreviewMode.RegionMask ? _preview?.GeometryMask : null;
    internal Ra2VoxelSceneSnapshot? CurrentPreviewComparisonSnapshot => _previewMode switch
    {
        Ra2VoxelStylePreviewMode.Difference => _qualityPreview?.DirectCandidate,
        Ra2VoxelStylePreviewMode.Symmetry when HasSymmetryCandidate => _qualityPreview?.RefinedCandidate,
        _ => null
    };
    internal Ra2VoxelFeatureProtectionMask? CurrentPreviewProtectionMask =>
        _previewMode == Ra2VoxelStylePreviewMode.Difference ? _qualityPreview?.ReviewPackage?.ProtectionMask : null;
    internal Ra2VoxelSemanticPartition? CurrentPreviewSemanticPartition =>
        _previewMode == Ra2VoxelStylePreviewMode.StructureRegions && HasStructurePartition
            ? _structurePreview?.Partition
            : null;
    internal Ra2VoxelSemanticEvidencePackage? CurrentPreviewSemanticEvidence =>
        _previewMode == Ra2VoxelStylePreviewMode.Semantics ? _semanticEvidence : null;
    internal IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment>? CurrentPreviewSemanticAssignments =>
        _previewMode == Ra2VoxelStylePreviewMode.Semantics && _semanticEvidence is not null ? ResolveSemanticAssignments() : null;
    internal Ra2VoxelSemanticMaskComposition? CurrentPreviewSemanticComposition =>
        _previewMode == Ra2VoxelStylePreviewMode.Semantics ? ResolveSemanticComposition() : null;
    internal Ra2VoxelFormZoneProjection? CurrentPreviewFormZones =>
        _previewMode == Ra2VoxelStylePreviewMode.FormZones ? _preview?.Materialization?.SemanticIntegration?.FormZones : null;
    internal Ra2VoxelBoundaryIntentProjection? CurrentPreviewBoundaryIntents =>
        _previewMode == Ra2VoxelStylePreviewMode.BoundaryIntent ? _preview?.Materialization?.SemanticIntegration?.BoundaryIntents : null;
    internal Ra2VoxelFeatureScaleProjection? CurrentPreviewFeatureScale =>
        _previewMode == Ra2VoxelStylePreviewMode.RiskOverlay ? _preview?.Materialization?.SemanticIntegration?.FeatureScale : null;
    internal Ra2VoxelSemanticMaskComposition? CurrentPreviewRiskComposition =>
        _previewMode == Ra2VoxelStylePreviewMode.RiskOverlay ? ResolveColourComposition(ActiveGeometrySnapshot!) : null;
    internal Ra2VoxelSceneSnapshot? CurrentPreviewRiskCandidate =>
        _previewMode == Ra2VoxelStylePreviewMode.RiskOverlay ? _preview?.ResultSnapshot : null;
    internal Ra2VoxelColourQualityReport? CurrentPreviewQuality =>
        _previewMode == Ra2VoxelStylePreviewMode.RiskOverlay ? _preview?.Materialization?.Ordinary?.Quality : null;

    public string StyleOverride
    {
        get => _styleOverride;
        set
        {
            string normalized = value ?? string.Empty;
            if (string.Equals(_styleOverride, normalized, StringComparison.Ordinal))
                return;
            _styleOverride = normalized;
            OnPropertyChanged();
            if (_source?.IsSuccess == true)
            {
                InvalidateOperation();
                bool hadPreview = _preview?.IsSuccess == true;
                ClearStylePreview();
                _qualityWarningsAccepted = false;
                SetStatus(hadPreview ? "风格说明已更改；旧计划和候选已失效，请重新编译预览。" : "可以编译风格预览。", isError: false);
                RaiseStateProperties();
            }
        }
    }

    internal async Task LoadSourceAsync(string filePath, string? palettePath = null)
    {
        ThrowIfDisposed();
        string? projectRoot = ProjectRootPath;
        (long generation, CancellationToken token) = BeginOperation("正在读取体素模型并生成原始切片预览…");
        Ra2VoxelStyleSourceLoadResult result = await Task.Run(
            () => _coordinator.LoadSource(projectRoot, filePath, palettePath, token),
            CancellationToken.None);
        if (!IsCurrent(generation))
            return;

        EndOperation();
        if (!result.IsSuccess)
        {
            SetStatus(result.Message, isError: result.FailureKind != Ra2VoxelStyleSourceLoadFailureKind.Cancelled);
            return;
        }

        _source = result;
        _workingGeometryState = Ra2VoxelWorkingGeometryState.CreateRoot(
            result.Snapshot!,
            Ra2VoxelWorkingGeometryOrigin.LoadedSource,
            "已载入源模型");
        ClearSemanticState();
        ClearQualityState();
        ClearStylePreview();
        ResetUnitClassState(clearBaseColour: true);
        PopulateBaseColourOptions(result.Snapshot!);
        _originalImage = CreateImage(result.OriginalSliceStackPng!);
        _isSliceFallback = false;
        ClearReviewProjection();
        RefreshStyleSources();
        SetPreviewMode(Ra2VoxelStylePreviewMode.Original);
        SetStatus("体素模型已载入。请准备分划，并在上色阶段人工选择单位类型、基准色和技法。", isError: false);
        RaiseStateProperties();
    }

    internal void SelectGenerationReference(string filePath)
    {
        _generationReferencePath = string.IsNullOrWhiteSpace(filePath) ? string.Empty : Path.GetFullPath(filePath);
        _generationSession = null;
        _generationProgressText = "参考图已选择；尚未探测 Provider。";
        RaiseGenerationProperties();
    }

    internal void SelectGenerationPalette(string filePath)
    {
        _generationPalettePath = string.IsNullOrWhiteSpace(filePath) ? string.Empty : Path.GetFullPath(filePath);
        _generationSession = null;
        RaiseGenerationProperties();
    }

    internal async Task<bool> PrepareGenerationAsync()
    {
        ThrowIfDisposed();
        if (ProjectRootPath is not string projectRoot)
        {
            SetStatus("请先打开一个项目。", isError: true);
            return false;
        }
        (long generation, CancellationToken token) = BeginOperation("正在离线探测固定 Provider…");
        var input = new Ra2VoxelGenerationInput(
            projectRoot,
            _generationReferencePath,
            string.IsNullOrWhiteSpace(_generationPalettePath) ? null : _generationPalettePath,
            _generationBrief,
            _generationNegativeConstraints,
            _generationResolution,
            TimeSpan.FromMinutes(_generationTimeoutMinutes));
        Ra2VoxelGenerationSession session = await _generationOrchestrator.PrepareAsync(input, token);
        if (!IsCurrent(generation))
            return false;
        EndOperation();
        _generationSession = session;
        _generationProgressText = session.Message;
        SetStatus(session.Message, isError: session.State == Ra2VoxelGenerationSessionState.Failed);
        RaiseGenerationProperties();
        RaiseStateProperties();
        return session.CanConfirm;
    }

    internal async Task GenerateConfirmedAsync(bool consentConfirmed)
    {
        ThrowIfDisposed();
        if (_generationSession?.CanConfirm != true)
        {
            SetStatus("请先准备生成任务并审阅发送确认。", isError: true);
            return;
        }
        Ra2VoxelGenerationSession prepared = _generationSession;
        (long generation, CancellationToken token) = BeginOperation("正在生成并转换会话内候选…");
        var progress = new Progress<RA2IniEditor.AssetHost.Ra2MeshGenerationProgress>(item =>
        {
            _generationProgressText = string.IsNullOrWhiteSpace(item.Message) ? item.Phase : item.Message;
            OnPropertyChanged(nameof(GenerationProgressText));
        });
        Ra2VoxelGenerationSession session = await _generationOrchestrator.GenerateAsync(
            prepared, consentConfirmed, _source, progress, token);
        if (!IsCurrent(generation))
            return;
        EndOperation();
        _generationSession = session;
        _generationProgressText = session.Message;
        if (!session.IsSuccess || session.Candidate is null)
        {
            SetStatus(session.Message, isError: session.State is not (Ra2VoxelGenerationSessionState.Canceled));
            RaiseGenerationProperties();
            RaiseStateProperties();
            return;
        }

        _source = session.Candidate;
        _workingGeometryState = Ra2VoxelWorkingGeometryState.CreateRoot(
            session.Candidate.Snapshot!,
            Ra2VoxelWorkingGeometryOrigin.GeneratedSource,
            "生成源模型");
        ClearSemanticState();
        ClearQualityState();
        ClearStylePreview();
        ResetUnitClassState(clearBaseColour: true);
        PopulateBaseColourOptions(session.Candidate.Snapshot!);
        _originalImage = CreateImage(session.Candidate.OriginalSliceStackPng!);
        _isSliceFallback = false;
        ClearReviewProjection();
        RefreshStyleSources();
        SetPreviewMode(Ra2VoxelStylePreviewMode.Original);
        SetStatus(session.Message, isError: false);
        RaiseGenerationProperties();
        RaiseStateProperties();
    }

    internal void SelectQualitySource(string filePath)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(filePath))
            return;
        InvalidateOperation();
        _qualitySourcePath = Path.GetFullPath(filePath);
        ClearQualityReviewState();
        SetStatus("已选择 GLB 质量源；生成前不会调用任何模型或改变当前候选。", isError: false);
        RaiseStateProperties();
    }

    internal async Task GenerateQualityCandidatesAsync()
    {
        ThrowIfDisposed();
        if (_source?.IsSuccess != true || _workingGeometryState is not { } working || ProjectRootPath is not string projectRoot ||
            (!_source.IsGeneratedSession && string.IsNullOrWhiteSpace(_qualitySourcePath)))
        {
            SetStatus("请先载入基线模型并选择项目内的 GLB 质量源。", isError: true);
            return;
        }

        Ra2VoxelStyleSourceLoadResult source = _source;
        Ra2VoxelSceneSnapshot workingSnapshot = working.Snapshot;
        long workingRevision = working.Revision;
        string? qualitySourcePath = _qualitySourcePath;
        ClearStructureState();
        (long generation, CancellationToken token) = BeginOperation("正在本地生成直接、平滑候选和有界结构证据…");
        Ra2VoxelQualityPreviewResult result = await Task.Run(
            () => source.IsGeneratedSession
                ? _coordinator.GenerateQualityCandidatesFromGenerated(source, workingSnapshot, workingRevision, token)
                : _coordinator.GenerateQualityCandidates(projectRoot, source, workingSnapshot, workingRevision, qualitySourcePath, token),
            CancellationToken.None);
        if (!IsCurrent(generation))
            return;

        EndOperation();
        if (_workingGeometryState is not { } currentWorking ||
            currentWorking.Revision != workingRevision ||
            !string.Equals(currentWorking.Snapshot.CanonicalHash, workingSnapshot.CanonicalHash, StringComparison.Ordinal))
        {
            SetStatus("质量候选对应的工作几何已过期，未发布到当前会话。", isError: true);
            return;
        }
        if (!result.IsSuccess)
        {
            SetStatus(result.Message, isError: result.FailureKind != Ra2VoxelQualityPreviewFailureKind.Cancelled);
            return;
        }

        _qualityPreview = result;
        _directImage = CreateImage(result.DirectSliceStackPng!);
        _refinedImage = CreateImage(result.RefinedSliceStackPng!);
        _symmetryImage = null;
        ProjectQualityPreview(result);
        SetPreviewMode(result.ReviewPackage!.Admission.IsAdmitted
            ? Ra2VoxelStylePreviewMode.Refined
            : Ra2VoxelStylePreviewMode.Direct);
        string evidence = BuildSymmetryEvidenceStatus(result);
        string connectivity = result.RefinedCandidate!.Connectivity.ComponentCount <= 1
            ? string.Empty
            : $"；平滑候选保留了 {result.RefinedCandidate.Connectivity.ComponentCount} 个组件，主体占比 " +
              $"{result.RefinedCandidate.Connectivity.LargestComponentCellCount / (double)result.RefinedCandidate.OccupancyCount:P1}";
        string admission = result.ReviewPackage.Admission.IsAdmitted
            ? "；最佳候选已通过全部硬门禁"
            : "；没有平滑候选同时满足安全与改善条件，已保留直接转换";
        SetStatus($"已基于当前几何 r{workingRevision} 生成质量候选{admission}{evidence}{connectivity}。请先查看差异再决定是否用于本会话。", isError: false);
        RaiseStateProperties();
    }

    private static string BuildSymmetryEvidenceStatus(Ra2VoxelQualityPreviewResult result)
    {
        if (result.SymmetryEvidence is not null)
            return $"；结构证据已就绪（{result.SymmetryEvidence.Regions.Count} 个区域），可按需执行 Agent 几何提案";
        return result.SymmetryEvidenceResult?.FailureKind switch
        {
            Ra2VoxelSemanticSymmetryFailureKind.EvidenceTooLarge => "；本地结构证据仍超过模型输入边界",
            Ra2VoxelSemanticSymmetryFailureKind.InvalidInput => "；本地结构证据输入不一致",
            Ra2VoxelSemanticSymmetryFailureKind.Cancelled => "；本地结构证据生成已取消",
            _ => "；本地结构证据未能生成"
        };
    }

    internal async Task AnalyzeStructureAsync()
    {
        ThrowIfDisposed();
        if (_qualityPreview?.IsSuccess != true || _qualityPreview.SymmetryEvidence is null || !IsQualityBatchCurrent())
        {
            SetStatus("请先生成当前 GLB 对应的本地质量候选。", isError: true);
            return;
        }
        DeepSeekRa2AiModel model = _modelAccessor();
        if (!_coordinator.IsStructureRecognitionConfigured(model))
        {
            SetStatus("DeepSeek 尚未配置，不能执行 Agent 几何提案；本地候选仍可继续使用。", isError: true);
            return;
        }

        Ra2VoxelQualityPreviewResult quality = _qualityPreview;
        ClearStructureState();
        (long generation, CancellationToken token) = BeginOperation("正在生成、审阅并按需仲裁 Agent 几何提案…");
        Ra2VoxelStructurePreviewResult result = await _coordinator.AnalyzeStructureAsync(quality, model, token);
        if (!IsCurrent(generation))
            return;

        EndOperation();
        if (!IsQualityBatchCurrent() || !IsStructureResultFor(result, quality, _modelAccessor()))
        {
            SetStatus("结构识别结果已过期，未发布到当前会话。", isError: true);
            return;
        }

        if (result.Partition is not null)
        {
            _structurePreview = result;
            _symmetryImage = result.SymmetrySliceStackPng is null ? null : CreateImage(result.SymmetrySliceStackPng);
            ProjectQualityPreview(quality);
            SetPreviewMode(Ra2VoxelStylePreviewMode.StructureRegions);
        }
        if (!result.IsSuccess)
        {
            SetStatus(result.Message, isError: result.FailureKind != Ra2VoxelStructurePreviewFailureKind.Cancelled);
            RaiseStateProperties();
            return;
        }

        SetStatus("Agent 几何提案与最低安全校验已完成。请先审阅“结构区”和“对称”，再决定是否用于本会话。", isError: false);
        RaiseStateProperties();
    }

    internal void UseCurrentQualityCandidateForSession()
    {
        if (!CanUseQualityCandidate || CurrentPreviewSnapshot is not { } candidate || _workingGeometryState is null)
            return;
        string displayName = _previewMode switch
        {
            Ra2VoxelStylePreviewMode.Direct => "质量基线",
            Ra2VoxelStylePreviewMode.Refined => "平滑候选",
            Ra2VoxelStylePreviewMode.Symmetry => "Agent 修复",
            _ => "当前几何"
        };
        Ra2VoxelWorkingGeometryOrigin origin = _previewMode == Ra2VoxelStylePreviewMode.Symmetry
            ? Ra2VoxelWorkingGeometryOrigin.AgentGeometryCandidate
            : Ra2VoxelWorkingGeometryOrigin.RefinedCandidate;
        Ra2VoxelWorkingGeometryState? next = _workingGeometryState.Advance(candidate, origin, displayName);
        if (next is null)
        {
            SetStatus("所选候选与当前几何完全相同；工作版本未变化。", isError: false);
            RaiseStateProperties();
            return;
        }
        _workingGeometryState = next;
        ClearSemanticState();
        ClearStylePreview();
        SetStatus($"已采用{displayName}作为当前几何 r{next.Revision}（{next.Snapshot.CanonicalHash[..12]}）；旧候选批次已不可再次采用，下一轮将从该版本继续。", isError: false);
        RaiseStateProperties();
    }

    internal async Task AnalyzeSemanticMasksAsync()
    {
        ThrowIfDisposed();
        bool replacesAcceptedSuggestions = _semanticSuggestionsAccepted && CurrentSemanticSuggestions.Count > 0;
        if (ActiveGeometrySnapshot is not { } snapshot)
        {
            SetStatus("请先载入体素模型。", isError: true);
            return;
        }
        DeepSeekRa2AiModel model = _modelAccessor();
        string instructions = _semanticInstructions;
        string workingHash = snapshot.CanonicalHash;
        (long generation, CancellationToken token) = BeginOperation("正在生成文本化几何证据并进行双轮语义识别…");
        Ra2VoxelSemanticAnalysisResult result = await _coordinator.AnalyzeSemanticMasksAsync(snapshot, instructions, model, token);
        if (!IsCurrent(generation)) return;
        EndOperation();
        if (result.Evidence is null)
        {
            SetStatus(result.Message, isError: true);
            return;
        }
        if (ActiveGeometrySnapshot is not { } current || !string.Equals(current.CanonicalHash, workingHash, StringComparison.Ordinal))
        {
            SetStatus("工作几何已变化，旧语义建议未发布。", isError: true);
            return;
        }
        bool preserveManualOverrides = _semanticEvidence is not null &&
            string.Equals(_semanticEvidence.SourceSnapshotHash, result.Evidence.SourceSnapshotHash, StringComparison.Ordinal);
        _semanticEvidence = result.Evidence;
        ResetUnitClassState(clearBaseColour: false);
        ClearStylePreview();
        _semanticCompilerResult = result.CompilerResult;
        _loadedSemanticSuggestions = [];
        _semanticSuggestionsAccepted = false;
        if (replacesAcceptedSuggestions)
            MarkSemanticAuthoringChanged();
        if (!preserveManualOverrides)
        {
            _semanticManualOverrides.Clear();
            ResetSemanticManualMask(snapshot);
        }
        else
        {
            EnsureSemanticManualMaskLayer(snapshot);
        }
        RefreshSemanticAssignments();
        SetPreviewMode(Ra2VoxelStylePreviewMode.Semantics);
        if (!result.IsSuccess || result.CompilerResult is null)
        {
            SetStatus($"{result.Message} 本地区域已保留，可直接人工覆盖。", isError: true);
            RaiseSemanticProperties();
            return;
        }
        SetStatus(result.CompilerResult.UsedArbitration
            ? "语义建议已生成，并因两轮差异执行了第三轮仲裁；请人工审阅后接受。"
            : "两轮语义分析一致；请人工审阅后接受。", isError: false);
        RaiseSemanticProperties();
    }

    internal async Task PrepareSemanticRegionsAsync()
    {
        ThrowIfDisposed();
        bool replacesAcceptedSuggestions = _semanticSuggestionsAccepted && CurrentSemanticSuggestions.Count > 0;
        if (ActiveGeometrySnapshot is not { } snapshot)
        {
            SetStatus("请先载入体素模型。", isError: true);
            return;
        }
        string hash = snapshot.CanonicalHash;
        (long generation, CancellationToken token) = BeginOperation("正在准备本地人工语义区域…");
        Ra2VoxelSemanticEvidencePackage evidence;
        try
        {
            evidence = await Task.Run(() => Ra2VoxelSemanticEvidenceBuilder.Build(snapshot, token), CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            if (IsCurrent(generation)) { EndOperation(); SetStatus("人工语义区域准备已取消。", isError: false); }
            return;
        }
        if (!IsCurrent(generation)) return;
        EndOperation();
        if (ActiveGeometrySnapshot is not { } current || !string.Equals(current.CanonicalHash, hash, StringComparison.Ordinal))
        {
            SetStatus("工作几何已变化，本地区域未发布。", isError: true);
            return;
        }
        bool preserveManual = _semanticEvidence is not null && string.Equals(_semanticEvidence.SourceSnapshotHash, hash, StringComparison.Ordinal);
        _semanticEvidence = evidence;
        ResetUnitClassState(clearBaseColour: false);
        ClearStylePreview();
        _semanticCompilerResult = null;
        _loadedSemanticSuggestions = [];
        _semanticSuggestionsAccepted = false;
        if (replacesAcceptedSuggestions)
            MarkSemanticAuthoringChanged();
        if (!preserveManual)
        {
            _semanticManualOverrides.Clear();
            ResetSemanticManualMask(snapshot);
        }
        else
        {
            EnsureSemanticManualMaskLayer(snapshot);
        }
        RefreshSemanticAssignments();
        SetPreviewMode(Ra2VoxelStylePreviewMode.Semantics);
        SetStatus("本地人工语义区域已准备；未调用 DeepSeek。可在 3D 中选择并人工覆盖。", isError: false);
        RaiseSemanticProperties();
    }

    internal void AcceptSemanticSuggestions()
    {
        if (!CanAcceptSemanticSuggestions) return;
        _semanticSuggestionsAccepted = true;
        MarkSemanticAuthoringChanged();
        RefreshSemanticAssignments();
        ClearStylePreview();
        SetStatus("AI 语义建议已启用；人工修改任一行会建立更高优先级覆盖。请重新编译着色预览。", isError: false);
        RaiseSemanticProperties();
    }

    internal void DiscardSemanticSuggestions()
    {
        if (!CanDiscardSemanticSuggestions) return;
        _semanticCompilerResult = null;
        _loadedSemanticSuggestions = [];
        _semanticSuggestionsAccepted = false;
        MarkSemanticAuthoringChanged();
        RefreshSemanticAssignments();
        ClearStylePreview();
        SetStatus("AI 建议已丢弃；确定性区域与人工覆盖已保留。", isError: false);
        RaiseSemanticProperties();
    }

    internal void ClearSemanticOverride(Ra2VoxelSemanticAssignmentRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        if (!_semanticManualOverrides.Remove(row.RegionId)) return;
        MarkSemanticAuthoringChanged();
        RefreshSemanticAssignments();
        ClearStylePreview();
        SetStatus("该区域的人工覆盖已撤销。", isError: false);
        RaiseSemanticProperties();
    }

    internal bool SelectSemanticRegion(string regionId)
    {
        Ra2VoxelSemanticAssignmentRow? row = SemanticAssignments.FirstOrDefault(value =>
            string.Equals(value.RegionId, regionId, StringComparison.Ordinal));
        if (row is null) return false;
        SelectedSemanticAssignment = row;
        _semanticBrushPartRole = row.PartRole;
        _semanticBrushMaterialRole = row.MaterialRole;
        _semanticBrushRemapApproved = row.RemapApproved;
        RaiseSemanticEditProperties();
        SetStatus($"已选择语义区域 {regionId}；在“语义”页可人工覆盖部件和材质。", isError: false);
        return true;
    }

    internal void SetSemanticEditMode(Ra2VoxelSemanticEditMode mode)
    {
        if (!Enum.IsDefined(mode)) return;
        if (_semanticEditMode != mode)
            CancelSemanticStroke("语义编辑模式已切换，未完成的笔划已取消。", reportStatus: false);
        _semanticEditMode = mode;
        if (mode != Ra2VoxelSemanticEditMode.Browse)
        {
            if (HasSemanticEvidence)
                SetPreviewMode(Ra2VoxelStylePreviewMode.Semantics);
            else
                SetStatus("正在进入语义编辑前，请先准备本地语义区域。", isError: false);
        }
        UpdateSemanticEditStatus();
        RaiseSemanticEditProperties();
    }

    internal async Task ActivateSemanticEditModeAsync(Ra2VoxelSemanticEditMode mode)
    {
        if (!Enum.IsDefined(mode)) return;
        if (!HasSemanticEvidence)
            await PrepareSemanticRegionsAsync();
        if (!HasSemanticEvidence)
            return;
        SetSemanticEditMode(mode);
    }

    internal void SetSemanticReviewDimension(Ra2VoxelSemanticReviewDimension dimension)
    {
        if (!Enum.IsDefined(dimension))
            return;
        if (_semanticReviewDimension != dimension)
        {
            _semanticReviewDimension = dimension;
            OnPropertyChanged(nameof(SemanticReviewDimension));
            OnPropertyChanged(nameof(IsSemanticPartReview));
            OnPropertyChanged(nameof(IsSemanticMaterialReview));
            OnPropertyChanged(nameof(SemanticReviewLegend));
        }
        if (HasSemanticEvidence)
            SetPreviewMode(Ra2VoxelStylePreviewMode.Semantics);
    }

    internal bool HandleSemanticCellClick(string regionId, Ra2VoxelCoordinate coordinate)
    {
        if (_semanticEditMode == Ra2VoxelSemanticEditMode.Browse)
            return SelectSemanticRegion(regionId);
        return BeginSemanticStroke(regionId, coordinate) && CommitSemanticStroke([coordinate]);
    }

    internal bool BeginSemanticStroke(string regionId, Ra2VoxelCoordinate coordinate)
    {
        if (_semanticEditMode == Ra2VoxelSemanticEditMode.Browse)
            return false;
        CancelSemanticStroke("新的笔划已开始。", reportStatus: false);
        if (ActiveGeometrySnapshot is not { } snapshot || _semanticEvidence is null ||
            !string.Equals(snapshot.CanonicalHash, _semanticEvidence.SourceSnapshotHash, StringComparison.Ordinal))
        {
            SetStatus("语义证据已过期，请重新准备区域。", isError: true);
            return false;
        }
        Ra2VoxelSemanticAssignmentRow? clickedRow = SemanticAssignments.FirstOrDefault(value =>
            string.Equals(value.RegionId, regionId, StringComparison.Ordinal));
        if (clickedRow is null)
        {
            SetStatus("点击位置不属于当前语义区域，请重新准备区域。", isError: true);
            return false;
        }
        SelectedSemanticAssignment = clickedRow;
        if (_semanticEditMode == Ra2VoxelSemanticEditMode.Paint &&
            (_semanticBrushPartRole == Ra2VoxelSemanticPartRole.Unknown ||
             _semanticBrushMaterialRole == Ra2VoxelSemanticMaterialRole.Unknown))
        {
            SetStatus("请先选择有效的画笔部位和材质。", isError: true);
            return false;
        }
        EnsureSemanticManualMaskLayer(snapshot);
        Ra2VoxelSemanticAssignment? assignment = _semanticEditMode == Ra2VoxelSemanticEditMode.Paint
            ? new(
                regionId,
                _semanticBrushPartRole,
                _semanticBrushMaterialRole,
                _semanticBrushRemapApproved ? Ra2VoxelSemanticRemapIntent.ExplicitlyApproved : Ra2VoxelSemanticRemapIntent.None,
                1d,
                "人工 3D 表面画笔")
            : null;
        _semanticStroke = new(
            snapshot,
            _semanticManualMaskLayer!,
            _semanticEditMode,
            _semanticBrushSize - 1,
            _semanticMirrorBrush,
            assignment);
        _semanticEditStatus = "正在绘制：1 个表面采样点。";
        OnPropertyChanged(nameof(SemanticEditStatus));
        OnPropertyChanged(nameof(CanUndoSemanticBrush));
        OnPropertyChanged(nameof(CanRedoSemanticBrush));
        return true;
    }

    internal void ReportSemanticStrokeProgress(int seedCount)
    {
        if (_semanticStroke is null)
            return;
        _semanticEditStatus = $"正在绘制：{Math.Max(1, seedCount):N0} 个表面采样点。";
        OnPropertyChanged(nameof(SemanticEditStatus));
    }

    internal bool CommitSemanticStroke(IReadOnlyList<Ra2VoxelCoordinate> seeds)
    {
        SemanticStrokeTransaction? stroke = _semanticStroke;
        _semanticStroke = null;
        OnPropertyChanged(nameof(CanUndoSemanticBrush));
        OnPropertyChanged(nameof(CanRedoSemanticBrush));
        if (stroke is null)
            return false;
        if (ActiveGeometrySnapshot is not { } current ||
            !string.Equals(current.CanonicalHash, stroke.Snapshot.CanonicalHash, StringComparison.Ordinal) ||
            _semanticEditMode != stroke.Mode)
        {
            SetStatus("笔划上下文已变化，本次修改已取消。", isError: true);
            return false;
        }

        var result = Ra2VoxelSemanticMaskEditor.ApplySurfaceStroke(
            stroke.Snapshot,
            stroke.BaseLayer,
            seeds,
            stroke.Radius,
            stroke.Mirror,
            stroke.Mode == Ra2VoxelSemanticEditMode.Paint
                ? Ra2VoxelSemanticBrushMode.Paint
                : Ra2VoxelSemanticBrushMode.Erase,
            stroke.Assignment);
        if (!result.IsSuccess)
        {
            _semanticEditStatus = result.Message;
            OnPropertyChanged(nameof(SemanticEditStatus));
            SetStatus(result.Message, result.FailureKind != Ra2VoxelSemanticBrushFailureKind.NoChange);
            return false;
        }

        PushSemanticHistory(_semanticUndo, stroke.BaseLayer);
        _semanticRedo.Clear();
        _semanticManualMaskLayer = result.Layer;
        MarkSemanticAuthoringChanged();
        ClearStylePreview();
        SetPreviewMode(Ra2VoxelStylePreviewMode.Semantics);
        _semanticEditStatus = $"已{(_semanticEditMode == Ra2VoxelSemanticEditMode.Paint ? "绘制" : "擦除")} {result.AffectedCellCount:N0} 个体素；" +
            $"当前人工画笔覆盖 {_semanticManualMaskLayer.Overrides.Count:N0} 个体素。";
        SetStatus(_semanticEditStatus + " 请重新编译着色预览。", isError: false);
        RaiseSemanticProperties();
        return true;
    }

    internal void CancelSemanticStroke(string message, bool reportStatus = true)
    {
        if (_semanticStroke is null)
            return;
        _semanticStroke = null;
        if (reportStatus)
        {
            _semanticEditStatus = string.IsNullOrWhiteSpace(message) ? "未完成的笔划已取消。" : message.Trim();
            OnPropertyChanged(nameof(SemanticEditStatus));
            SetStatus(_semanticEditStatus, isError: false);
        }
        OnPropertyChanged(nameof(CanUndoSemanticBrush));
        OnPropertyChanged(nameof(CanRedoSemanticBrush));
    }

    internal void ReportSemanticPointerFeedback(string message, bool isError)
    {
        _semanticEditStatus = string.IsNullOrWhiteSpace(message) ? "未命中模型表面。" : message.Trim();
        OnPropertyChanged(nameof(SemanticEditStatus));
        SetStatus(_semanticEditStatus, isError);
    }

    internal void UndoSemanticBrush()
    {
        if (!CanUndoSemanticBrush || _semanticManualMaskLayer is null) return;
        PushSemanticHistory(_semanticRedo, _semanticManualMaskLayer);
        _semanticManualMaskLayer = PopSemanticHistory(_semanticUndo);
        MarkSemanticAuthoringChanged();
        ClearStylePreview();
        SetPreviewMode(Ra2VoxelStylePreviewMode.Semantics);
        _semanticEditStatus = $"已撤销画笔；当前人工画笔覆盖 {_semanticManualMaskLayer.Overrides.Count:N0} 个体素。";
        SetStatus(_semanticEditStatus, isError: false);
        RaiseSemanticProperties();
    }

    internal void RedoSemanticBrush()
    {
        if (!CanRedoSemanticBrush || _semanticManualMaskLayer is null) return;
        PushSemanticHistory(_semanticUndo, _semanticManualMaskLayer);
        _semanticManualMaskLayer = PopSemanticHistory(_semanticRedo);
        MarkSemanticAuthoringChanged();
        ClearStylePreview();
        SetPreviewMode(Ra2VoxelStylePreviewMode.Semantics);
        _semanticEditStatus = $"已重做画笔；当前人工画笔覆盖 {_semanticManualMaskLayer.Overrides.Count:N0} 个体素。";
        SetStatus(_semanticEditStatus, isError: false);
        RaiseSemanticProperties();
    }

    internal async Task SaveSemanticSidecarAsync(string filePath)
    {
        ThrowIfDisposed();
        if (!CanSaveSemanticSidecar || ProjectRootPath is not string projectRoot ||
            ActiveGeometrySnapshot is not { } snapshot || _semanticEvidence is null)
        {
            SetStatus("当前没有可保存的语义分划。", isError: true);
            return;
        }
        EnsureSemanticManualMaskLayer(snapshot);
        long capturedRevision = _semanticAuthoringRevision;
        var state = new Ra2VoxelSemanticSidecarState(
            _semanticEvidence,
            _semanticSuggestionsAccepted,
            CurrentSemanticSuggestions.ToArray(),
            _semanticManualOverrides.Values.ToArray(),
            _semanticManualMaskLayer!);
        _isSemanticPersistenceBusy = true;
        _semanticPersistenceStatus = "正在保存语义分划…";
        RaiseSemanticPersistenceProperties();
        Ra2VoxelSemanticSidecarResult result = await Task.Run(() =>
            _semanticSidecarStore.Save(projectRoot, filePath, snapshot, state));
        _isSemanticPersistenceBusy = false;
        if (!result.IsSuccess)
        {
            _semanticPersistenceStatus = result.Message;
            SetStatus(result.Message, isError: result.FailureKind != Ra2VoxelSemanticSidecarFailureKind.Canceled);
            RaiseSemanticPersistenceProperties();
            return;
        }
        _semanticSidecarPath = Path.GetFullPath(filePath);
        if (_semanticAuthoringRevision == capturedRevision)
        {
            _semanticSavedRevision = capturedRevision;
            _semanticPersistenceStatus = $"已保存：{Path.GetFileName(_semanticSidecarPath)}";
            SetStatus("语义分划已原子保存到当前项目。", isError: false);
        }
        else
        {
            _semanticPersistenceStatus = $"已保存旧快照：{Path.GetFileName(_semanticSidecarPath)}；当前仍有未保存修改。";
            SetStatus(_semanticPersistenceStatus, isError: false);
        }
        RaiseSemanticPersistenceProperties();
    }

    internal async Task LoadSemanticSidecarAsync(string filePath)
    {
        ThrowIfDisposed();
        if (!CanLoadSemanticSidecar || ProjectRootPath is not string projectRoot || ActiveGeometrySnapshot is not { } snapshot)
        {
            SetStatus("请先打开项目并载入匹配的体素模型。", isError: true);
            return;
        }
        string workingHash = snapshot.CanonicalHash;
        _isSemanticPersistenceBusy = true;
        _semanticPersistenceStatus = "正在验证语义分划…";
        RaiseSemanticPersistenceProperties();
        Ra2VoxelSemanticSidecarResult result = await Task.Run(() =>
            _semanticSidecarStore.Load(projectRoot, filePath, snapshot));
        _isSemanticPersistenceBusy = false;
        if (!result.IsSuccess || result.State is null)
        {
            _semanticPersistenceStatus = result.Message;
            SetStatus(result.Message, isError: result.FailureKind != Ra2VoxelSemanticSidecarFailureKind.Canceled);
            RaiseSemanticPersistenceProperties();
            return;
        }
        if (ActiveGeometrySnapshot is not { } current || !string.Equals(current.CanonicalHash, workingHash, StringComparison.Ordinal))
        {
            _semanticPersistenceStatus = "工作几何已变化，已验证的语义分划未载入。";
            SetStatus(_semanticPersistenceStatus, isError: true);
            RaiseSemanticPersistenceProperties();
            return;
        }

        Ra2VoxelSemanticSidecarState state = result.State;
        _semanticEvidence = state.Evidence;
        ResetUnitClassState(clearBaseColour: false);
        ClearStylePreview();
        _semanticCompilerResult = null;
        _loadedSemanticSuggestions = state.AgentSuggestions.ToArray();
        _semanticSuggestionsAccepted = state.AgentSuggestionsAccepted;
        _semanticManualOverrides.Clear();
        foreach (Ra2VoxelSemanticAssignment value in state.HumanRegionOverrides)
            _semanticManualOverrides.Add(value.RegionId, value);
        _semanticManualMaskLayer = state.HumanCellLayer;
        _semanticUndo.Clear();
        _semanticRedo.Clear();
        _semanticStroke = null;
        _semanticEditMode = Ra2VoxelSemanticEditMode.Browse;
        ClearStylePreview();
        RefreshSemanticAssignments();
        SetPreviewMode(Ra2VoxelStylePreviewMode.Semantics);
        _semanticAuthoringRevision++;
        _semanticSavedRevision = _semanticAuthoringRevision;
        _semanticSidecarPath = Path.GetFullPath(filePath);
        _semanticPersistenceStatus = $"已载入：{Path.GetFileName(_semanticSidecarPath)}";
        UpdateSemanticEditStatus();
        SetStatus("语义分划已验证并替换当前会话状态；画笔撤销历史已清空。", isError: false);
        RaiseSemanticProperties();
    }

    internal void ConfirmUnitClass()
    {
        if (_source?.IsSuccess != true || ActiveGeometrySnapshot is not { } snapshot ||
            _selectedUnitClass is null || !CanConfirmUnitClass)
            return;
        try
        {
            Ra2VoxelSemanticMaskComposition composition = ResolveColourComposition(snapshot);
            Ra2VoxelStyleSourceLoadResult source = _source with { Snapshot = snapshot };
            _unitClassEvidence = _coordinator.BuildUnitClassEvidence(source, composition);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            _unitClassEvidence = null;
            _confirmedUnitClass = null;
            _colourSkillRoute = null;
            _unitClassStatus = "确认失败";
            SetStatus("当前几何与分划证据不一致，无法确认单位类型。", isError: true);
            RaiseColourInputProperties();
            return;
        }
        Ra2VoxelUnitClassConfirmationResult confirmation = Ra2VoxelConfirmedUnitClass.Create(
            _unitClassEvidence,
            _selectedUnitClass.Value,
            Ra2VoxelUnitClassConfirmationSource.HumanManualSelection,
            proposal: null);
        if (!confirmation.IsSuccess || confirmation.Confirmation is null)
        {
            SetStatus(confirmation.Message, isError: true);
            return;
        }
        Ra2VoxelColourSkillRouteResult route = _coordinator.ResolveColourSkill(
            _unitClassEvidence, confirmation.Confirmation);
        if (!route.IsSuccess || route.Route is null)
        {
            SetStatus(route.Message, isError: true);
            return;
        }
        _confirmedUnitClass = confirmation.Confirmation;
        _colourSkillRoute = route.Route;
        _unitClassStatus = $"已确认：{UnitClassName(_selectedUnitClass.Value)}";
        InvalidateColourCandidate("单位类型已确认；请选择人工基准色并编译着色预览。");
        RaiseColourInputProperties();
        RaiseStateProperties();
    }

    internal async Task CompileAsync()
    {
        ThrowIfDisposed();
        if (_source?.IsSuccess != true || ProjectRootPath is not string projectRoot)
        {
            SetStatus("请先打开项目并载入项目内的体素模型。", isError: true);
            return;
        }

        if (_unitClassEvidence is null || _confirmedUnitClass is null || _colourSkillRoute is null ||
            _baseColourSelection is null || _selectedTechnique is null || ActiveGeometrySnapshot is not { } activeSnapshot)
        {
            SetStatus("请先完成人工单位类型确认、人工基准色和上色技法选择。", isError: true);
            return;
        }

        Ra2VoxelStyleSourceLoadResult source = _source with { Snapshot = activeSnapshot };
        string styleOverride = _styleOverride;
        Ra2VoxelSemanticMaskComposition semanticComposition = ResolveColourComposition(activeSnapshot);
        var orientationResult = Ra2VoxelForwardDirectionSelection.Create(
            activeSnapshot,
            semanticComposition.CompositionHash,
            _selectedForwardDirection.Value);
        if (!orientationResult.IsSuccess || orientationResult.Selection is null)
        {
            SetStatus("人工前向选择与当前模型或语义分划不匹配，请重新选择。", isError: true);
            return;
        }
        DeepSeekRa2AiModel model = _modelAccessor();
        (long generation, CancellationToken token) = BeginOperation("正在编译结构化风格计划…");
        Ra2VoxelStylePreviewResult result = await _coordinator.CompilePreviewV2Async(
            source,
            projectRoot,
            styleOverride,
            model,
            semanticComposition,
            _unitClassEvidence,
            _confirmedUnitClass,
            _baseColourSelection,
            _selectedTechnique.Policy,
            orientationResult.Selection,
            token);
        if (!IsCurrent(generation))
            return;

        EndOperation();
        if (!result.IsSuccess)
        {
            SetStatus(result.Message, isError: result.FailureKind != Ra2VoxelStylePreviewFailureKind.Cancelled);
            return;
        }

        _preview = result;
        _resultImage = CreateImage(result.FindArtifactBytes("body-coloured-slicestack.png")!);
        _contrastImage = result.ContrastSliceStackPng is null ? null : CreateImage(result.ContrastSliceStackPng);
        _regionMaskImage = CreateImage(result.FindArtifactBytes("region-mask.png")!);
        _paletteImage = CreateImage(result.FindArtifactBytes("palette-swatch.png")!);
        ClearAcceptedCandidate();
        _hasPendingStyleChanges = false;
        _isSliceFallback = false;
        ProjectStyleSources(result.SourcePack!);
        ProjectPreview(result);
        ProjectColourQuality(result);
        SetPreviewMode(Ra2VoxelStylePreviewMode.Result);
        string cacheNote = result.CompilerV2Result?.CacheHit == true ? "（使用已验证缓存）" : string.Empty;
        SetStatus($"风格预览已生成{cacheNote}；请审阅后决定是否接受到当前会话。", isError: false);
        RaiseStateProperties();
    }

    internal void Cancel()
    {
        if (!_isBusy)
            return;
        _operationCancellation?.Cancel();
        SetStatus("正在取消当前体素工作区操作…", isError: false);
    }

    internal void ReportNoActiveProject() =>
        SetStatus("请先通过“文件 → 打开文件夹”打开项目，再选择项目内的体素模型。", isError: true);

    internal void NotifyProjectChanged()
    {
        InvalidateOperation();
        _source = null;
        _workingGeometryState = null;
        ClearSemanticState();
        ClearQualityState();
        ClearStylePreview();
        ResetUnitClassState(clearBaseColour: true);
        _originalImage = null;
        _currentPreviewImage = null;
        _isSliceFallback = false;
        StyleSources.Clear();
        ClearReviewProjection();
        SetStatus("项目已切换，请重新选择项目内的体素模型。", isError: false);
        OnPropertyChanged(nameof(CurrentPreviewImage));
        RaiseStateProperties();
    }

    internal void RefreshExternalModelContext()
    {
        if (_structurePreview is not null && !IsStructureResultCurrent())
        {
            ClearStructureState();
            SetStatus("AI 模型已更改；旧结构分区和对称候选已失效，请重新识别。", isError: false);
        }
        RaiseStateProperties();
    }

    internal void AcceptCurrentSession()
    {
        if (!CanAccept)
            return;
        if (!TryGetMaterializableCandidate(out Ra2VoxelSceneSnapshot? snapshot, out Ra2VoxelAcceptedCandidateKind kind, out string? displayName) ||
            snapshot is null || displayName is null)
        {
            return;
        }

        _acceptedCandidate = new Ra2VoxelAcceptedCandidate(
            snapshot,
            kind,
            displayName,
            BuildSuggestedVoxFileName(),
            Interlocked.Read(ref _generation));
        SetStatus($"已固化最终候选“{displayName}”；尚未写入文件。", isError: false);
        RaiseStateProperties();
    }

    internal async Task ExportAcceptedVoxAsync(string targetPath, bool overwriteExisting)
    {
        ThrowIfDisposed();
        if (_acceptedCandidate is not { } candidate)
        {
            SetStatus("请先固化一个最终候选，再导出 VOX。", isError: true);
            return;
        }

        string? currentSourcePath = _source?.FilePath;
        (long generation, CancellationToken token) = BeginOperation("正在编码并验证最终 VOX…");
        Ra2VoxelVoxExportResult result = await Task.Run(
            () => _voxExportService.Export(candidate, targetPath, currentSourcePath, overwriteExisting, token),
            CancellationToken.None);
        if (!IsCurrent(generation))
            return;

        EndOperation();
        if (!result.IsSuccess)
        {
            SetStatus(result.Message, isError: result.FailureKind != Ra2VoxelVoxExportFailureKind.Canceled);
            RaiseStateProperties();
            return;
        }

        SetStatus($"VOX 已导出并通过回读验证：{result.TargetPath} · {result.ByteCount:N0} 字节。", isError: false);
        RaiseStateProperties();
    }

    internal void ToggleSliceFallback()
    {
        if (!CanUseSliceFallback)
            return;
        _isSliceFallback = !_isSliceFallback;
        RaisePreviewProperties();
        SetStatus(_isSliceFallback
            ? "已切换到诊断切片；可随时返回交互式 3D。"
            : "已返回交互式 3D 预览。", isError: false);
    }

    internal void ReportViewportFallback(string message)
    {
        if (!CanUseSliceFallback)
            return;
        _isSliceFallback = true;
        RaisePreviewProperties();
        SetStatus(string.IsNullOrWhiteSpace(message)
            ? "3D 预览不可用，已切换到诊断切片。"
            : $"{message} 已切换到诊断切片。", isError: true);
    }

    internal void SetPreviewMode(Ra2VoxelStylePreviewMode mode)
    {
        if (mode == Ra2VoxelStylePreviewMode.Refined && !HasRefinedCandidate)
            return;
        if (mode == Ra2VoxelStylePreviewMode.Difference && !HasQualityDifference)
            return;
        if (mode == Ra2VoxelStylePreviewMode.StructureRegions && !HasStructurePartition)
            return;
        if (mode == Ra2VoxelStylePreviewMode.Symmetry && !HasSymmetryCandidate)
            return;
        if (mode == Ra2VoxelStylePreviewMode.Semantics && _semanticEvidence is null)
            return;
        if ((mode is Ra2VoxelStylePreviewMode.FormZones or Ra2VoxelStylePreviewMode.BoundaryIntent or
            Ra2VoxelStylePreviewMode.RiskOverlay) && !CanUseRev7DiagnosticPreviews)
        {
            SetPreviewMode(HasSemanticEvidence ? Ra2VoxelStylePreviewMode.Semantics : Ra2VoxelStylePreviewMode.Original);
            return;
        }
        ImageSource? image = mode switch
        {
            Ra2VoxelStylePreviewMode.Original => _originalImage,
            Ra2VoxelStylePreviewMode.Direct => _directImage,
            Ra2VoxelStylePreviewMode.Refined => _refinedImage,
            Ra2VoxelStylePreviewMode.Difference => _refinedImage,
            Ra2VoxelStylePreviewMode.StructureRegions => _refinedImage,
            Ra2VoxelStylePreviewMode.Symmetry => _symmetryImage,
            Ra2VoxelStylePreviewMode.Semantics => _originalImage,
            Ra2VoxelStylePreviewMode.Result => _resultImage,
            Ra2VoxelStylePreviewMode.Contrast => _contrastImage,
            Ra2VoxelStylePreviewMode.RegionMask => _regionMaskImage,
            Ra2VoxelStylePreviewMode.Palette => _paletteImage,
            Ra2VoxelStylePreviewMode.FormZones => _originalImage,
            Ra2VoxelStylePreviewMode.BoundaryIntent => _originalImage,
            Ra2VoxelStylePreviewMode.RiskOverlay => _originalImage,
            _ => null
        };
        if (image is null)
            return;
        _previewMode = mode;
        _isSliceFallback = false;
        _currentPreviewImage = image;
        if (mode != Ra2VoxelStylePreviewMode.Semantics && _semanticEditMode != Ra2VoxelSemanticEditMode.Browse)
        {
            _semanticEditMode = Ra2VoxelSemanticEditMode.Browse;
            UpdateSemanticEditStatus();
            RaiseSemanticEditProperties();
        }
        RaisePreviewProperties();
    }

    internal void SelectWorkflowStage(Ra2VoxelWorkspaceStage stage)
    {
        if (!Enum.IsDefined(stage) || _selectedWorkflowStage == stage)
            return;
        _selectedWorkflowStage = stage;
        RaiseWorkflowProperties();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        InvalidateOperation();
        _operationCancellation?.Dispose();
        _operationCancellation = null;
        _source = null;
        _workingGeometryState = null;
        ClearSemanticState();
        ClearQualityState();
        ClearStylePreview();
        _originalImage = null;
        _currentPreviewImage = null;
        _isSliceFallback = false;
        StyleSources.Clear();
        ClearReviewProjection();
    }

    private void RefreshStyleSources()
    {
        StyleSources.Clear();
        if (_source?.IsSuccess != true || ProjectRootPath is not string root)
            return;
        Ra2VoxelStyleSourceResolutionResult resolution = _coordinator.ResolveSourcePack(_source, root, _styleOverride);
        if (!resolution.IsSuccess || resolution.SourcePack is null)
        {
            StyleSources.Add("风格来源当前不可用；编译时会给出具体原因。");
            return;
        }
        ProjectStyleSources(resolution.SourcePack);
    }

    private void ProjectStyleSources(Ra2VoxelStyleSourcePack sourcePack)
    {
        StyleSources.Clear();
        foreach (Ra2VoxelStyleSource source in sourcePack.Sources)
            StyleSources.Add($"{SourceScopeName(source.Scope)} · {source.DisplayPath}");
    }

    private void ProjectPreview(Ra2VoxelStylePreviewResult result)
    {
        ClearReviewProjection();
        foreach (Ra2CompiledVoxelStyleRole role in result.Plan!.Roles)
        {
            Roles.Add(new(
                role.Id,
                RoleCategoryName(role.Category.ToString()),
                role.PaletteIndex.ToString(),
                string.Join(", ", role.SourceScopeIds)));
        }
        foreach (Ra2CompiledVoxelStyleRule rule in result.Plan.Rules)
        {
            Rules.Add(new(
                RegionName(rule.Region.ToString()),
                rule.RoleId,
                EvidenceName(rule.Evidence.ToString()),
                rule.IsPaintable ? "可执行" : "仅供审阅"));
        }

        foreach (string assumption in result.Plan.UnresolvedAssumptions)
            ReviewIssues.Add("待确认 · " + assumption);
        foreach (string unresolved in result.Facts!.UnresolvedRules)
            ReviewIssues.Add("未解析规则 · " + unresolved);
        foreach (Ra2VoxelColourReviewFlags flag in Enum.GetValues<Ra2VoxelColourReviewFlags>())
        {
            if (flag != Ra2VoxelColourReviewFlags.None && result.Facts.ReviewFlags.HasFlag(flag))
                ReviewIssues.Add(ReviewFlagName(flag));
        }
        ReviewIssues.Add(result.Facts.GeometryAndOccupancyUnchanged
            ? "通过 · 几何与占用未改变"
            : "失败 · 几何或占用发生变化");
        ReviewIssues.Add($"色板最大平方误差 · {result.Facts.MaximumSquaredPaletteError:N0}");
    }

    private void ProjectQualityPreview(Ra2VoxelQualityPreviewResult result)
    {
        QualityMetrics.Clear();
        SemanticRegions.Clear();
        foreach (Ra2VoxelQualityMetricRow row in Ra2VoxelQualityReviewProjection.ProjectMetrics(
                     result.BaselineFacts!,
                     result.ReviewPackage!,
                     result.DirectCandidate!,
                     result.DirectCandidate!,
                     result.RefinedCandidate!,
                     HasSymmetryCandidate ? _structurePreview?.Candidate : null))
        {
            QualityMetrics.Add(row);
        }
        IReadOnlyList<Ra2VoxelSemanticRegionRow> semanticRows = HasStructurePartition
            ? Ra2VoxelQualityReviewProjection.ProjectSemanticRegions(_structurePreview!.Partition!)
            : Ra2VoxelQualityReviewProjection.ProjectSemanticRegions(result.ReviewPackage!);
        foreach (Ra2VoxelSemanticRegionRow row in semanticRows)
            SemanticRegions.Add(row);
        OnPropertyChanged(nameof(NormalComparisonText));
        OnPropertyChanged(nameof(QualityCandidatesText));
        OnPropertyChanged(nameof(QualityAdmissionText));
        OnPropertyChanged(nameof(StructureProtectionText));
        OnPropertyChanged(nameof(SemanticLegendText));
        OnPropertyChanged(nameof(SemanticReviewText));
        OnPropertyChanged(nameof(SemanticReviewText));
    }

    private void ClearQualityState()
    {
        _qualityPreview = null;
        _qualitySourcePath = null;
        _directImage = null;
        _refinedImage = null;
        ClearStructureState();
        QualityMetrics.Clear();
        SemanticRegions.Clear();
        OnPropertyChanged(nameof(NormalComparisonText));
        OnPropertyChanged(nameof(QualityCandidatesText));
        OnPropertyChanged(nameof(QualityAdmissionText));
        OnPropertyChanged(nameof(StructureProtectionText));
    }

    private void ClearQualityReviewState()
    {
        _qualityPreview = null;
        _directImage = null;
        _refinedImage = null;
        ClearStructureState();
        QualityMetrics.Clear();
        SemanticRegions.Clear();
        if (_previewMode is Ra2VoxelStylePreviewMode.Direct or Ra2VoxelStylePreviewMode.Refined or
            Ra2VoxelStylePreviewMode.Difference or Ra2VoxelStylePreviewMode.StructureRegions or Ra2VoxelStylePreviewMode.Symmetry)
        {
            SetPreviewMode(Ra2VoxelStylePreviewMode.Original);
        }
        OnPropertyChanged(nameof(NormalComparisonText));
        OnPropertyChanged(nameof(QualityCandidatesText));
        OnPropertyChanged(nameof(QualityAdmissionText));
        OnPropertyChanged(nameof(StructureProtectionText));
    }

    private void ClearStructureState()
    {
        _structurePreview = null;
        _symmetryImage = null;
        if (_qualityPreview?.ReviewPackage is { } review)
        {
            SemanticRegions.Clear();
            foreach (Ra2VoxelSemanticRegionRow row in Ra2VoxelQualityReviewProjection.ProjectSemanticRegions(review))
                SemanticRegions.Add(row);
        }
        if ((_previewMode is Ra2VoxelStylePreviewMode.StructureRegions or Ra2VoxelStylePreviewMode.Symmetry) && _directImage is not null)
            SetPreviewMode(Ra2VoxelStylePreviewMode.Direct);
        OnPropertyChanged(nameof(SemanticReviewText));
        OnPropertyChanged(nameof(HasStructurePartition));
        OnPropertyChanged(nameof(HasSymmetryCandidate));
        OnPropertyChanged(nameof(CanAnalyzeStructure));
        OnPropertyChanged(nameof(StructureRecognitionToolTip));
        OnPropertyChanged(nameof(CurrentPreviewSemanticPartition));
    }

    private void ClearStylePreview()
    {
        bool redirectStyleDependentMode = _previewMode is Ra2VoxelStylePreviewMode.Result or
            Ra2VoxelStylePreviewMode.Contrast or Ra2VoxelStylePreviewMode.RegionMask or Ra2VoxelStylePreviewMode.Palette or
            Ra2VoxelStylePreviewMode.FormZones or Ra2VoxelStylePreviewMode.BoundaryIntent or Ra2VoxelStylePreviewMode.RiskOverlay;
        _preview = null;
        _resultImage = null;
        _contrastImage = null;
        _regionMaskImage = null;
        _paletteImage = null;
        ClearAcceptedCandidate();
        _hasPendingStyleChanges = false;
        ClearReviewProjection();
        ClearColourQualityProjection();
        if (redirectStyleDependentMode)
            SetPreviewMode(HasSemanticEvidence ? Ra2VoxelStylePreviewMode.Semantics : Ra2VoxelStylePreviewMode.Original);
    }

    private void ClearSemanticState()
    {
        bool wasSemanticPreview = _previewMode == Ra2VoxelStylePreviewMode.Semantics;
        _semanticEvidence = null;
        ResetUnitClassState(clearBaseColour: false);
        _semanticCompilerResult = null;
        _loadedSemanticSuggestions = [];
        _semanticSuggestionsAccepted = false;
        _semanticManualOverrides.Clear();
        _semanticManualMaskLayer = null;
        _semanticUndo.Clear();
        _semanticRedo.Clear();
        _semanticEditMode = Ra2VoxelSemanticEditMode.Browse;
        _semanticBrushPartRole = Ra2VoxelSemanticPartRole.Unknown;
        _semanticBrushMaterialRole = Ra2VoxelSemanticMaterialRole.Unknown;
        _semanticBrushRemapApproved = false;
        _semanticStroke = null;
        _semanticEditStatus = "浏览模式：左键点击模型选择区域；右键拖动旋转。";
        _semanticAuthoringRevision = 0;
        _semanticSavedRevision = 0;
        _semanticSidecarPath = null;
        _semanticPersistenceStatus = "语义分划尚未保存。";
        SemanticAssignments.Clear();
        SelectedSemanticAssignment = null;
        if (wasSemanticPreview && _originalImage is not null)
            SetPreviewMode(Ra2VoxelStylePreviewMode.Original);
        RaiseSemanticProperties();
    }

    private IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment> ResolveSemanticAssignments() =>
        _semanticEvidence is null
            ? []
            : Ra2VoxelSemanticLayerResolver.Resolve(
                _semanticEvidence,
                _semanticSuggestionsAccepted ? CurrentSemanticSuggestions : null,
                _semanticManualOverrides.Values);

    private Ra2VoxelSemanticMaskComposition? ResolveSemanticComposition()
    {
        if (ActiveGeometrySnapshot is not { } snapshot || _semanticEvidence is null ||
            !string.Equals(snapshot.CanonicalHash, _semanticEvidence.SourceSnapshotHash, StringComparison.Ordinal))
            return null;
        EnsureSemanticManualMaskLayer(snapshot);
        return Ra2VoxelSemanticMaskComposer.Compose(
            snapshot,
            _semanticEvidence,
            ResolveSemanticAssignments(),
            _semanticManualMaskLayer!);
    }

    private Ra2VoxelSemanticSurfaceCoverage? ResolveSemanticSurfaceCoverage()
    {
        if (ActiveGeometrySnapshot is not { } snapshot || ResolveSemanticComposition() is not { } composition)
            return null;
        return Ra2VoxelSemanticSurfaceCoverageProjector.Project(snapshot, composition);
    }

    private Ra2VoxelSemanticMaskComposition ResolveColourComposition(Ra2VoxelSceneSnapshot snapshot)
    {
        if (ResolveSemanticComposition() is { } composition)
            return composition;
        Ra2VoxelSemanticEffectiveAssignment unknown = new(
            "unclassified",
            Ra2VoxelSemanticPartRole.Unknown,
            Ra2VoxelSemanticMaterialRole.Unknown,
            Ra2VoxelSemanticRemapIntent.None,
            Ra2VoxelSemanticAssignmentSource.Unknown,
            0d,
            "尚未进行语义分划");
        return new Ra2VoxelSemanticMaskComposition(
            snapshot.CanonicalHash,
            Enumerable.Repeat(unknown, snapshot.OccupancyCount),
            snapshot.CanonicalHash);
    }

    private void PopulateBaseColourOptions(Ra2VoxelSceneSnapshot snapshot)
    {
        BaseColourOptions.Clear();
        for (int index = 0; index < 256; index++)
        {
            byte paletteIndex = checked((byte)index);
            if (snapshot.Palette.IsTransparent(paletteIndex) || snapshot.Palette.IsRemap(paletteIndex))
                continue;
            Ra2Rgba32 colour = snapshot.Palette[paletteIndex];
            SolidColorBrush brush = new(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue));
            brush.Freeze();
            string rgb = $"#{colour.Red:X2}{colour.Green:X2}{colour.Blue:X2}";
            BaseColourOptions.Add(new(paletteIndex, $"#{paletteIndex} · {rgb}", brush, rgb));
        }
        _selectedBaseColour = null;
        _baseColourSelection = null;
        OnPropertyChanged(nameof(SelectedBaseColour));
        RaiseColourInputProperties();
    }

    private void ResetUnitClassState(bool clearBaseColour)
    {
        _unitClassEvidence = null;
        _confirmedUnitClass = null;
        _colourSkillRoute = null;
        _selectedUnitClass = null;
        _unitClassStatus = "尚未选择";
        _selectedForwardDirection = ForwardDirectionOptions[0];
        _qualityWarningsAccepted = false;
        if (clearBaseColour)
        {
            _selectedBaseColour = null;
            _baseColourSelection = null;
            BaseColourOptions.Clear();
        }
        OnPropertyChanged(nameof(SelectedUnitClass));
        OnPropertyChanged(nameof(SelectedForwardDirection));
        OnPropertyChanged(nameof(ForwardDirectionStatusText));
        OnPropertyChanged(nameof(SelectedBaseColour));
        ClearColourQualityProjection();
        RaiseColourInputProperties();
    }

    private void InvalidateColourCandidate(string? message)
    {
        _qualityWarningsAccepted = false;
        ClearStylePreview();
        ClearColourQualityProjection();
        if (!string.IsNullOrWhiteSpace(message)) SetStatus(message, isError: false);
        RaiseColourInputProperties();
    }

    private void ClearColourQualityProjection()
    {
        ColourQualityMetrics.Clear();
        ColourQualityWarnings.Clear();
        ColourQualityFormZones.Clear();
        ColourQualityBoundaries.Clear();
        ColourQualityAccents.Clear();
        ColourQualityGameScale.Clear();
        OnPropertyChanged(nameof(ColourQualityStatusText));
        OnPropertyChanged(nameof(HasReviewableColourWarnings));
        OnPropertyChanged(nameof(QualityWarningsAccepted));
    }

    private void ProjectColourQuality(Ra2VoxelStylePreviewResult result)
    {
        ClearColourQualityProjection();
        if (result.Materialization?.Ordinary?.Quality is not { } quality)
            return;
        foreach (Ra2VoxelColourQualityMetric metric in quality.Metrics)
        {
            string item = $"{metric.Id} · {metric.Value}";
            ColourQualityMetrics.Add(item);
            QualityGroup(metric.Id).Add(item);
        }
        foreach (var warning in quality.Warnings)
        {
            string item = $"{warning.Code} · {warning.Message}";
            ColourQualityWarnings.Add(item);
            QualityGroup(warning.Code).Add($"警告 · {item}");
        }
        OnPropertyChanged(nameof(ColourQualityStatusText));
        OnPropertyChanged(nameof(HasReviewableColourWarnings));
        OnPropertyChanged(nameof(CanAccept));
    }

    private ObservableCollection<string> QualityGroup(string id)
    {
        if (id.Contains("game_scale", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("vpl", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("normal_context", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("GameScale", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("Vpl", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("NormalContext", StringComparison.OrdinalIgnoreCase))
            return ColourQualityGameScale;
        if (id.Contains("accent", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("isolated", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("detail", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("subpixel", StringComparison.OrdinalIgnoreCase))
            return ColourQualityAccents;
        if (id.Contains("boundary", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("material", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("seam", StringComparison.OrdinalIgnoreCase))
            return ColourQualityBoundaries;
        return ColourQualityFormZones;
    }

    private void RaiseColourInputProperties()
    {
        OnPropertyChanged(nameof(UnitClassStatusText));
        OnPropertyChanged(nameof(UnitClassSkillText));
        OnPropertyChanged(nameof(SelectedForwardDirection));
        OnPropertyChanged(nameof(ForwardDirectionStatusText));
        OnPropertyChanged(nameof(BaseColourStatusText));
        OnPropertyChanged(nameof(BaseColourSwatch));
        OnPropertyChanged(nameof(TechniqueDescription));
        OnPropertyChanged(nameof(CanSelectUnitClass));
        OnPropertyChanged(nameof(CanConfirmUnitClass));
        OnPropertyChanged(nameof(HasConfirmedUnitClass));
        OnPropertyChanged(nameof(CanEditColourInputs));
        OnPropertyChanged(nameof(CanCompile));
        OnPropertyChanged(nameof(CanAccept));
        RaiseWorkflowProperties();
    }

    private void RaiseWorkflowProperties()
    {
        OnPropertyChanged(nameof(SelectedWorkflowStage));
        OnPropertyChanged(nameof(IsModelStage));
        OnPropertyChanged(nameof(IsGeometryStage));
        OnPropertyChanged(nameof(IsSemanticsStage));
        OnPropertyChanged(nameof(IsColourStage));
        OnPropertyChanged(nameof(IsReviewStage));
        OnPropertyChanged(nameof(CurrentWorkflowStageText));
        OnPropertyChanged(nameof(ModelStageStatus));
        OnPropertyChanged(nameof(GeometryStageStatus));
        OnPropertyChanged(nameof(SemanticsStageStatus));
        OnPropertyChanged(nameof(ColourStageStatus));
        OnPropertyChanged(nameof(ReviewStageStatus));
        OnPropertyChanged(nameof(UnclassifiedSemanticRegionCount));
        OnPropertyChanged(nameof(WorkflowNextActionText));
    }

    private void EnsureSemanticManualMaskLayer(Ra2VoxelSceneSnapshot snapshot)
    {
        if (_semanticManualMaskLayer is not null &&
            string.Equals(_semanticManualMaskLayer.SourceSnapshotHash, snapshot.CanonicalHash, StringComparison.Ordinal) &&
            _semanticManualMaskLayer.CellCount == snapshot.OccupancyCount)
            return;
        ResetSemanticManualMask(snapshot);
    }

    private void ResetSemanticManualMask(Ra2VoxelSceneSnapshot snapshot)
    {
        _semanticManualMaskLayer = new(snapshot.CanonicalHash, snapshot.OccupancyCount);
        _semanticUndo.Clear();
        _semanticRedo.Clear();
    }

    private static void PushSemanticHistory(List<Ra2VoxelSemanticManualMaskLayer> history, Ra2VoxelSemanticManualMaskLayer layer)
    {
        history.Add(layer);
        if (history.Count > 100) history.RemoveAt(0);
    }

    private static Ra2VoxelSemanticManualMaskLayer PopSemanticHistory(List<Ra2VoxelSemanticManualMaskLayer> history)
    {
        int index = history.Count - 1;
        Ra2VoxelSemanticManualMaskLayer value = history[index];
        history.RemoveAt(index);
        return value;
    }

    private void UpdateSemanticEditStatus()
    {
        _semanticEditStatus = _semanticEditMode switch
        {
            Ra2VoxelSemanticEditMode.Browse => "浏览模式：左键点击模型选择区域；右键拖动旋转。",
            Ra2VoxelSemanticEditMode.Paint => $"画笔模式：{_semanticBrushPartRole}/{_semanticBrushMaterialRole}，左键点击或拖动绘制大小 {_semanticBrushSize} 的表面范围" +
                (_semanticMirrorBrush ? "，并同步存在的镜像体素。" : "。"),
            Ra2VoxelSemanticEditMode.Erase => $"擦除模式：左键点击或拖动移除大小 {_semanticBrushSize} 的人工体素覆盖" +
                (_semanticMirrorBrush ? "，并同步存在的镜像体素。" : "。"),
            _ => throw new ArgumentOutOfRangeException()
        };
        OnPropertyChanged(nameof(SemanticEditStatus));
    }

    private void RefreshSemanticAssignments()
    {
        SemanticAssignments.Clear();
        if (_semanticEvidence is null) return;
        Dictionary<string, Ra2VoxelSemanticAssignment> suggestions = CurrentSemanticSuggestions
            .ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        Dictionary<string, Ra2VoxelSemanticEffectiveAssignment> effective = ResolveSemanticAssignments()
            .ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        foreach (var region in _semanticEvidence.Regions)
        {
            suggestions.TryGetValue(region.RegionId, out Ra2VoxelSemanticAssignment? suggestion);
            SemanticAssignments.Add(new(
                region.RegionId,
                region.MirrorRegionId ?? "无",
                $"{region.CellCount:N0} 体素 · 范围 X {region.MinimumX}–{region.MaximumX} / Y {region.MinimumY}–{region.MaximumY} / Z {region.MinimumZ}–{region.MaximumZ} · 镜像覆盖 {region.MirrorCoverage:P0}",
                suggestion?.PartRole ?? Ra2VoxelSemanticPartRole.Unknown,
                suggestion?.MaterialRole ?? Ra2VoxelSemanticMaterialRole.Unknown,
                suggestion?.Confidence ?? 0d,
                suggestion?.Reason ?? "AI 未建议",
                effective[region.RegionId],
                OnSemanticRowChanged));
        }
    }

    private void OnSemanticRowChanged(Ra2VoxelSemanticAssignmentRow row)
    {
        _semanticManualOverrides[row.RegionId] = new(
            row.RegionId,
            row.PartRole,
            row.MaterialRole,
            row.RemapApproved ? Ra2VoxelSemanticRemapIntent.ExplicitlyApproved : Ra2VoxelSemanticRemapIntent.None,
            1d,
            "人工覆盖");
        if (row.MirrorLinked && !string.Equals(row.MirrorRegionId, "无", StringComparison.Ordinal) &&
            _semanticEvidence?.Regions.Any(value => string.Equals(value.RegionId, row.MirrorRegionId, StringComparison.Ordinal)) == true)
        {
            _semanticManualOverrides[row.MirrorRegionId] = new(
                row.MirrorRegionId,
                row.PartRole,
                row.MaterialRole,
                row.RemapApproved ? Ra2VoxelSemanticRemapIntent.ExplicitlyApproved : Ra2VoxelSemanticRemapIntent.None,
                1d,
                "人工覆盖（镜像联动）");
        }
        MarkSemanticAuthoringChanged();
        ClearStylePreview();
        Dictionary<string, Ra2VoxelSemanticEffectiveAssignment> effective = ResolveSemanticAssignments()
            .ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        foreach (Ra2VoxelSemanticAssignmentRow candidate in SemanticAssignments)
            if (effective.TryGetValue(candidate.RegionId, out Ra2VoxelSemanticEffectiveAssignment? assignment))
                candidate.Apply(assignment);
        SetStatus("人工语义覆盖已更新；请重新编译着色预览。", isError: false);
        RaiseSemanticProperties();
    }

    private void RaiseSemanticProperties()
    {
        OnPropertyChanged(nameof(SemanticStatusText));
        OnPropertyChanged(nameof(CanAnalyzeSemantics));
        OnPropertyChanged(nameof(HasSemanticSuggestions));
        OnPropertyChanged(nameof(HasSemanticEvidence));
        OnPropertyChanged(nameof(CanAcceptSemanticSuggestions));
        OnPropertyChanged(nameof(CanDiscardSemanticSuggestions));
        OnPropertyChanged(nameof(CurrentPreviewSemanticEvidence));
        OnPropertyChanged(nameof(CurrentPreviewSemanticAssignments));
        OnPropertyChanged(nameof(CurrentPreviewSemanticComposition));
        RaiseSemanticPersistenceProperties();
        RaiseSemanticEditProperties();
        RaiseWorkflowProperties();
    }

    private IReadOnlyList<Ra2VoxelSemanticAssignment> CurrentSemanticSuggestions =>
        _semanticCompilerResult?.Suggestions ?? _loadedSemanticSuggestions;

    private bool HasPersistableSemanticState =>
        (_semanticSuggestionsAccepted && CurrentSemanticSuggestions.Count > 0) ||
        _semanticManualOverrides.Count > 0 ||
        _semanticManualMaskLayer?.Overrides.Count > 0;

    private void MarkSemanticAuthoringChanged()
    {
        _semanticAuthoringRevision++;
        _semanticPersistenceStatus = "语义分划有未保存修改。";
        ResetUnitClassState(clearBaseColour: false);
        ClearStylePreview();
        RaiseSemanticPersistenceProperties();
    }

    private void RaiseSemanticPersistenceProperties()
    {
        OnPropertyChanged(nameof(IsSemanticSidecarDirty));
        OnPropertyChanged(nameof(HasUnsavedSemanticSidecarChanges));
        OnPropertyChanged(nameof(CanSaveSemanticSidecar));
        OnPropertyChanged(nameof(CanLoadSemanticSidecar));
        OnPropertyChanged(nameof(SemanticPersistenceStatus));
        OnPropertyChanged(nameof(SemanticSidecarInitialDirectory));
        OnPropertyChanged(nameof(SemanticSidecarSuggestedFileName));
    }

    private void RaiseSemanticEditProperties()
    {
        OnPropertyChanged(nameof(SemanticEditMode));
        OnPropertyChanged(nameof(IsSemanticBrowseMode));
        OnPropertyChanged(nameof(IsSemanticPaintMode));
        OnPropertyChanged(nameof(IsSemanticEraseMode));
        OnPropertyChanged(nameof(SemanticBrushSize));
        OnPropertyChanged(nameof(SemanticMirrorBrush));
        OnPropertyChanged(nameof(SemanticBrushPartRole));
        OnPropertyChanged(nameof(SemanticBrushMaterialRole));
        OnPropertyChanged(nameof(SemanticBrushRemapApproved));
        OnPropertyChanged(nameof(CanUndoSemanticBrush));
        OnPropertyChanged(nameof(CanRedoSemanticBrush));
        OnPropertyChanged(nameof(SemanticEditStatus));
        OnPropertyChanged(nameof(SemanticReviewDimension));
        OnPropertyChanged(nameof(IsSemanticPartReview));
        OnPropertyChanged(nameof(IsSemanticMaterialReview));
        OnPropertyChanged(nameof(SemanticReviewLegend));
    }

    private sealed record SemanticStrokeTransaction(
        Ra2VoxelSceneSnapshot Snapshot,
        Ra2VoxelSemanticManualMaskLayer BaseLayer,
        Ra2VoxelSemanticEditMode Mode,
        int Radius,
        bool Mirror,
        Ra2VoxelSemanticAssignment? Assignment);

    private void ClearReviewProjection()
    {
        Roles.Clear();
        Rules.Clear();
        ReviewIssues.Clear();
        OnPropertyChanged(nameof(PlanTitle));
        OnPropertyChanged(nameof(PlanSummary));
    }

    private (long Generation, CancellationToken Token) BeginOperation(string status)
    {
        InvalidateOperation();
        _operationCancellation = new CancellationTokenSource();
        _isBusy = true;
        SetStatus(status, isError: false);
        RaiseStateProperties();
        return (_generation, _operationCancellation.Token);
    }

    private void EndOperation()
    {
        _operationCancellation?.Dispose();
        _operationCancellation = null;
        _isBusy = false;
        RaiseStateProperties();
    }

    private void InvalidateOperation()
    {
        Interlocked.Increment(ref _generation);
        _operationCancellation?.Cancel();
        _operationCancellation?.Dispose();
        _operationCancellation = null;
        _isBusy = false;
        RaiseStateProperties();
    }

    private bool IsCurrent(long generation) => !_disposed && generation == Interlocked.Read(ref _generation);

    private bool IsStructureResultCurrent()
    {
        if (_structurePreview is null || _qualityPreview?.RefinedCandidate is null)
            return false;
        string modelIdentity = DeepSeekRa2AiModelCatalog.GetOption(_modelAccessor()).ApiModelId;
        return IsQualityBatchCurrent() &&
               (_qualityPreview.IsGeneratedSession || string.Equals(_structurePreview.SourceFilePath, _qualitySourcePath, PathComparison)) &&
               string.Equals(_structurePreview.SourceSnapshotHash, _qualityPreview.RefinedCandidate.CanonicalHash, StringComparison.Ordinal) &&
               string.Equals(_structurePreview.WorkingBaselineHash, _qualityPreview.WorkingBaselineHash, StringComparison.Ordinal) &&
               _structurePreview.WorkingRevision == _qualityPreview.WorkingRevision &&
               string.Equals(_structurePreview.QualityBatchHash, _qualityPreview.QualityBatchHash, StringComparison.Ordinal) &&
               string.Equals(_structurePreview.ModelIdentity, modelIdentity, StringComparison.Ordinal);
    }

    private static bool IsStructureResultFor(
        Ra2VoxelStructurePreviewResult result,
        Ra2VoxelQualityPreviewResult quality,
        DeepSeekRa2AiModel model) =>
        quality.RefinedCandidate is not null &&
        (quality.IsGeneratedSession || string.Equals(result.SourceFilePath, quality.FilePath, PathComparison)) &&
        string.Equals(result.SourceSnapshotHash, quality.RefinedCandidate.CanonicalHash, StringComparison.Ordinal) &&
        string.Equals(result.WorkingBaselineHash, quality.WorkingBaselineHash, StringComparison.Ordinal) &&
        result.WorkingRevision == quality.WorkingRevision &&
        string.Equals(result.QualityBatchHash, quality.QualityBatchHash, StringComparison.Ordinal) &&
        string.Equals(result.ModelIdentity, DeepSeekRa2AiModelCatalog.GetOption(model).ApiModelId, StringComparison.Ordinal);

    private bool IsQualityBatchCurrent() =>
        _qualityPreview is { IsSuccess: true } quality &&
        _workingGeometryState is { } working &&
        quality.WorkingRevision == working.Revision &&
        string.Equals(quality.WorkingBaselineHash, working.Snapshot.CanonicalHash, StringComparison.Ordinal);

    private void SetStatus(string message, bool isError)
    {
        _statusText = message;
        _isError = isError;
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(IsError));
    }

    private void RaiseStateProperties()
    {
        OnPropertyChanged(nameof(SourcePath));
        OnPropertyChanged(nameof(SourceName));
        OnPropertyChanged(nameof(SourceFacts));
        OnPropertyChanged(nameof(ModelDisplayName));
        OnPropertyChanged(nameof(QualitySourceName));
        OnPropertyChanged(nameof(QualityProvenanceText));
        OnPropertyChanged(nameof(WorkingGeometryText));
        OnPropertyChanged(nameof(NormalComparisonText));
        OnPropertyChanged(nameof(QualityCandidatesText));
        OnPropertyChanged(nameof(QualityAdmissionText));
        OnPropertyChanged(nameof(StructureProtectionText));
        OnPropertyChanged(nameof(PaletteContrastText));
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(HasSource));
        OnPropertyChanged(nameof(HasPreview));
        OnPropertyChanged(nameof(HasQualityCandidates));
        OnPropertyChanged(nameof(HasRefinedCandidate));
        OnPropertyChanged(nameof(HasQualityDifference));
        OnPropertyChanged(nameof(HasStructurePartition));
        OnPropertyChanged(nameof(HasSymmetryCandidate));
        OnPropertyChanged(nameof(HasContrastCandidate));
        OnPropertyChanged(nameof(CanChooseSource));
        OnPropertyChanged(nameof(CanGenerateModel));
        OnPropertyChanged(nameof(CanChooseQualitySource));
        OnPropertyChanged(nameof(CanGenerateQuality));
        OnPropertyChanged(nameof(CanAnalyzeStructure));
        OnPropertyChanged(nameof(CanCompile));
        OnPropertyChanged(nameof(CanCancel));
        OnPropertyChanged(nameof(CanAccept));
        OnPropertyChanged(nameof(CanUseQualityCandidate));
        OnPropertyChanged(nameof(IsAccepted));
        OnPropertyChanged(nameof(CanExportVox));
        OnPropertyChanged(nameof(AcceptanceText));
        OnPropertyChanged(nameof(ExportSuggestedFileName));
        OnPropertyChanged(nameof(PlanTitle));
        OnPropertyChanged(nameof(PlanSummary));
        OnPropertyChanged(nameof(ColourQualityStatusText));
        OnPropertyChanged(nameof(HasReviewableColourWarnings));
        RaiseColourInputProperties();
        RaisePreviewProperties();
        RaiseGenerationProperties();
        RaiseSemanticProperties();
    }

    private void RaiseGenerationProperties()
    {
        OnPropertyChanged(nameof(GenerationReferencePath));
        OnPropertyChanged(nameof(GenerationReferenceFacts));
        OnPropertyChanged(nameof(GenerationPaletteFacts));
        OnPropertyChanged(nameof(GenerationProgressText));
        OnPropertyChanged(nameof(IsGenerationAwaitingConsent));
        OnPropertyChanged(nameof(GenerationProviderText));
        OnPropertyChanged(nameof(CanGenerateModel));
    }

    private void RaisePreviewProperties()
    {
        OnPropertyChanged(nameof(CurrentPreviewImage));
        OnPropertyChanged(nameof(PreviewMode));
        OnPropertyChanged(nameof(PreviewModeIndex));
        OnPropertyChanged(nameof(IsOriginalMode));
        OnPropertyChanged(nameof(IsDirectMode));
        OnPropertyChanged(nameof(IsRefinedMode));
        OnPropertyChanged(nameof(IsDifferenceMode));
        OnPropertyChanged(nameof(IsStructureRegionsMode));
        OnPropertyChanged(nameof(IsSymmetryMode));
        OnPropertyChanged(nameof(IsSemanticsMode));
        OnPropertyChanged(nameof(IsResultMode));
        OnPropertyChanged(nameof(IsContrastMode));
        OnPropertyChanged(nameof(IsRegionMaskMode));
        OnPropertyChanged(nameof(IsPaletteMode));
        OnPropertyChanged(nameof(IsFormZonesMode));
        OnPropertyChanged(nameof(IsBoundaryIntentMode));
        OnPropertyChanged(nameof(IsRiskOverlayMode));
        OnPropertyChanged(nameof(CanUseRev7DiagnosticPreviews));
        OnPropertyChanged(nameof(IsThreeDimensionalPreview));
        OnPropertyChanged(nameof(IsImagePreviewVisible));
        OnPropertyChanged(nameof(CanUseSliceFallback));
        OnPropertyChanged(nameof(IsSliceFallback));
        OnPropertyChanged(nameof(CurrentPreviewSnapshot));
        OnPropertyChanged(nameof(CurrentPreviewRegionMask));
        OnPropertyChanged(nameof(CurrentPreviewComparisonSnapshot));
        OnPropertyChanged(nameof(CurrentPreviewProtectionMask));
        OnPropertyChanged(nameof(CurrentPreviewSemanticPartition));
        OnPropertyChanged(nameof(CurrentPreviewSemanticEvidence));
        OnPropertyChanged(nameof(CurrentPreviewSemanticAssignments));
        OnPropertyChanged(nameof(CurrentPreviewFormZones));
        OnPropertyChanged(nameof(CurrentPreviewBoundaryIntents));
        OnPropertyChanged(nameof(CurrentPreviewFeatureScale));
        OnPropertyChanged(nameof(CurrentPreviewRiskComposition));
        OnPropertyChanged(nameof(CurrentPreviewRiskCandidate));
        OnPropertyChanged(nameof(CurrentPreviewQuality));
        OnPropertyChanged(nameof(ActiveGeometrySnapshot));
        OnPropertyChanged(nameof(CanAccept));
    }

    private static bool IsQualityCandidateMode(Ra2VoxelStylePreviewMode mode) =>
        mode is Ra2VoxelStylePreviewMode.Direct or Ra2VoxelStylePreviewMode.Refined or Ra2VoxelStylePreviewMode.Symmetry;

    private bool CanAdmitColourCandidate(Ra2VoxelAcceptedCandidateKind kind)
    {
        var quality = kind switch
        {
            Ra2VoxelAcceptedCandidateKind.Styled => _preview?.Materialization?.Ordinary?.Quality,
            Ra2VoxelAcceptedCandidateKind.ContrastStyled => _preview?.Materialization?.Contrast?.Quality,
            _ => null
        };
        if (kind is not (Ra2VoxelAcceptedCandidateKind.Styled or Ra2VoxelAcceptedCandidateKind.ContrastStyled))
            return true;
        return quality?.State == Ra2VoxelColourAdmissionState.ReviewReady ||
               (quality?.State == Ra2VoxelColourAdmissionState.NeedsReview && _qualityWarningsAccepted);
    }

    private bool TryGetMaterializableCandidate(
        out Ra2VoxelSceneSnapshot? snapshot,
        out Ra2VoxelAcceptedCandidateKind kind,
        out string? displayName)
    {
        snapshot = null;
        kind = default;
        displayName = null;
        switch (_previewMode)
        {
            case Ra2VoxelStylePreviewMode.Original when _source?.Snapshot is { } original &&
                _workingGeometryState is { } originalWorking &&
                string.Equals(original.CanonicalHash, originalWorking.Snapshot.CanonicalHash, StringComparison.Ordinal):
                snapshot = original;
                kind = Ra2VoxelAcceptedCandidateKind.Original;
                displayName = "原始候选";
                return true;
            case Ra2VoxelStylePreviewMode.Direct when _qualityPreview?.DirectCandidate is { } direct &&
                (IsQualityBatchCurrent() || IsCurrentWorkingSnapshot(direct)):
                snapshot = direct;
                kind = Ra2VoxelAcceptedCandidateKind.Direct;
                displayName = "当前几何基线";
                return true;
            case Ra2VoxelStylePreviewMode.Refined when HasRefinedCandidate && _qualityPreview?.RefinedCandidate is { } refined &&
                (IsQualityBatchCurrent() || IsCurrentWorkingSnapshot(refined)):
                snapshot = refined;
                kind = Ra2VoxelAcceptedCandidateKind.Refined;
                displayName = "平滑候选";
                return true;
            case Ra2VoxelStylePreviewMode.Symmetry when HasSymmetryCandidate && _structurePreview?.Candidate is { } symmetry:
                snapshot = symmetry;
                kind = Ra2VoxelAcceptedCandidateKind.Symmetry;
                displayName = "Agent 几何候选";
                return true;
            case Ra2VoxelStylePreviewMode.Result when !_hasPendingStyleChanges && _preview?.ResultSnapshot is { } styled:
                snapshot = styled;
                kind = Ra2VoxelAcceptedCandidateKind.Styled;
                displayName = $"{_workingGeometryState?.DisplayName ?? "当前几何"} · 普通着色";
                return true;
            case Ra2VoxelStylePreviewMode.Contrast when !_hasPendingStyleChanges && _preview?.ContrastResultSnapshot is { } contrast:
                snapshot = contrast;
                kind = Ra2VoxelAcceptedCandidateKind.ContrastStyled;
                displayName = $"{_workingGeometryState?.DisplayName ?? "当前几何"} · 对比度着色";
                return true;
            default:
                return false;
        }
    }

    private string BuildSuggestedVoxFileName()
    {
        string stem = _source?.Snapshot?.Part.StableFileStem ?? "voxel";
        stem = Path.GetFileNameWithoutExtension(stem);
        return string.IsNullOrWhiteSpace(stem) ? "voxel-candidate.vox" : $"{stem}-candidate.vox";
    }

    private void ClearAcceptedCandidate()
    {
        _acceptedCandidate = null;
    }

    private bool IsCurrentWorkingSnapshot(Ra2VoxelSceneSnapshot? snapshot) =>
        snapshot is not null && _workingGeometryState is { } working &&
        string.Equals(snapshot.CanonicalHash, working.Snapshot.CanonicalHash, StringComparison.Ordinal);

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private static BitmapImage CreateImage(byte[] bytes)
    {
        using MemoryStream stream = new(bytes, writable: false);
        BitmapImage image = new();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }

    private static string SourceScopeName(Ra2VoxelStyleSourceScope scope) => scope switch
    {
        Ra2VoxelStyleSourceScope.BuiltIn => "内置基线",
        Ra2VoxelStyleSourceScope.ProjectRoot => "项目",
        Ra2VoxelStyleSourceScope.Directory => "目录",
        Ra2VoxelStyleSourceScope.RequestOverride => "本次要求",
        _ => "未知"
    };

    private static string UnitClassName(Ra2VoxelUnitClass value) => value switch
    {
        Ra2VoxelUnitClass.Ground => "地面载具",
        Ra2VoxelUnitClass.Air => "空中载具",
        Ra2VoxelUnitClass.LargeSurface => "大型水面单位",
        Ra2VoxelUnitClass.Unknown => "未知 / 保守模式",
        _ => value.ToString()
    };

    private static string RoleCategoryName(string value) => value switch
    {
        "BodyBase" => "车体基色",
        "BodyLight" => "车体亮部",
        "BodyMid" => "车体中间调",
        "BodyDark" => "车体暗部",
        "Underside" => "底面",
        "Glass" => "玻璃",
        "Rubber" => "橡胶",
        "BareMetal" => "裸露金属",
        "Accent" => "点缀",
        "Remap" => "阵营色",
        _ => value
    };

    private static string RegionName(string value) => value switch
    {
        "WholePart" => "整体",
        "TopExposed" => "顶部外露",
        "SideExposed" => "侧面外露",
        "UnderExposed" => "底部外露",
        "EdgeOrRidge" => "边缘/棱线",
        "Interior" => "内部",
        "ExplicitMask" => "显式蒙版",
        "DonorMask" => "范本投影蒙版",
        "SourceMaterialMask" => "来源材质蒙版",
        _ => value
    };

    private static string EvidenceName(string value) => value switch
    {
        "DeterministicGeometry" => "确定性几何",
        "ExplicitUserMask" => "用户蒙版",
        "DonorProjection" => "范本投影",
        "SourceMaterial" => "来源材质",
        "InferredTextOnly" => "仅文本推断",
        _ => value
    };

    private static string ReviewFlagName(Ra2VoxelColourReviewFlags flag) => flag switch
    {
        Ra2VoxelColourReviewFlags.StylePlanReviewRequired => "需审阅 · 风格计划尚未接受",
        Ra2VoxelColourReviewFlags.TextOnlyCoarseStyle => "提示 · 当前仅为粗粒度几何着色",
        Ra2VoxelColourReviewFlags.SemanticMaskReviewRequired => "需审阅 · 语义区域缺少显式蒙版",
        Ra2VoxelColourReviewFlags.RemapReviewRequired => "需审阅 · 阵营色区域需要确认",
        Ra2VoxelColourReviewFlags.PaletteErrorReviewRequired => "需审阅 · 目标颜色已映射到邻近色板色",
        Ra2VoxelColourReviewFlags.PivotReviewRequired => "待后续 · 枢轴尚未进行 VXL 阶段审阅",
        Ra2VoxelColourReviewFlags.NormalsNotGenerated => "待后续 · 尚未生成法线",
        Ra2VoxelColourReviewFlags.HvaNotGenerated => "待后续 · 尚未生成 HVA",
        Ra2VoxelColourReviewFlags.GameValidationNotRun => "待后续 · 尚未进行游戏内验证",
        _ => flag.ToString()
    };

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
