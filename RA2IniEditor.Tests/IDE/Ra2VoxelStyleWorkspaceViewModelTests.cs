using System.Text;
using System.Text.Json;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AssetAuthoring;
using RA2IniEditor.IDE.ViewModels.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelStyleWorkspaceViewModelTests : IDisposable
{
    private static readonly ushort[] CubeIndices =
    [
        0, 2, 1, 0, 3, 2,
        4, 5, 6, 4, 6, 7,
        0, 1, 5, 0, 5, 4,
        3, 7, 6, 3, 6, 2,
        0, 4, 7, 0, 7, 3,
        1, 2, 6, 1, 6, 5
    ];

    private readonly string _root = Path.Combine(Path.GetTempPath(), "ra2-voxel-style-vm-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void WorkingGeometryState_AdvancesLinearlyAndIgnoresNoOpAdoption()
    {
        Ra2VoxelSceneSnapshot root = CreateSnapshot();
        Ra2VoxelWorkingGeometryState state = Ra2VoxelWorkingGeometryState.CreateRoot(
            root,
            Ra2VoxelWorkingGeometryOrigin.LoadedSource,
            "源模型");
        Ra2VoxelCell first = root.Cells[0];
        Ra2VoxelSceneSnapshot changed = new(
            root.SceneId,
            root.Part,
            root.Palette,
            root.Cells.Select(cell => cell.Coordinate == first.Coordinate ? cell with { PaletteIndex = 61 } : cell),
            root.SourceArtifactHashes);

        Assert.Null(state.Advance(root, Ra2VoxelWorkingGeometryOrigin.RefinedCandidate, "相同"));

        Ra2VoxelWorkingGeometryState next = Assert.IsType<Ra2VoxelWorkingGeometryState>(
            state.Advance(changed, Ra2VoxelWorkingGeometryOrigin.AgentGeometryCandidate, "Agent 修复"));
        Assert.Equal(1, next.Revision);
        Assert.Equal(root.CanonicalHash, next.RootSnapshotHash);
        Assert.Equal(root.CanonicalHash, next.ParentSnapshotHash);
        Assert.Equal(changed.CanonicalHash, next.Snapshot.CanonicalHash);
    }

    [Fact]
    public async Task QualityCandidateLifecycle_IsSessionOnlyAndClearsOnBaselineReload()
    {
        TestContext test = CreateContext();
        FakeClient client = new();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(client);

        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.SelectQualitySource(test.GlbPath);
        await viewModel.GenerateQualityCandidatesAsync();

        Assert.True(viewModel.HasQualityCandidates);
        Assert.True(viewModel.IsDirectMode);
        Assert.False(viewModel.HasRefinedCandidate);
        Assert.False(viewModel.HasQualityDifference);
        Assert.Contains("未准入任何平滑候选", viewModel.QualityAdmissionText, StringComparison.Ordinal);
        Assert.Contains("冻结", viewModel.StructureProtectionText, StringComparison.Ordinal);
        Assert.Contains("保守平滑", viewModel.QualityCandidatesText, StringComparison.Ordinal);
        Assert.Contains("平衡平滑", viewModel.QualityCandidatesText, StringComparison.Ordinal);
        Assert.Contains("强表面清理", viewModel.QualityCandidatesText, StringComparison.Ordinal);
        Assert.Contains("未通过门禁", viewModel.QualityCandidatesText, StringComparison.Ordinal);
        Assert.Equal("用户配对；无法从当前文件证明原始来源", viewModel.QualityProvenanceText);
        Assert.Equal(7, viewModel.QualityMetrics.Count);
        Assert.Contains(viewModel.QualityMetrics, row => row.Label == "组件数");
        Assert.Contains(viewModel.QualityMetrics, row => row.Label == "主体占比");
        Assert.NotEmpty(viewModel.SemanticRegions);
        Assert.Equal(0, client.CallCount);

        viewModel.UseCurrentQualityCandidateForSession();

        Assert.Contains("已载入源模型", viewModel.WorkingGeometryText, StringComparison.Ordinal);
        Assert.Contains("完全相同", viewModel.StatusText, StringComparison.Ordinal);
        Assert.NotNull(viewModel.ActiveGeometrySnapshot);
        Assert.False(viewModel.HasPreview);

        await viewModel.LoadSourceAsync(test.VoxPath);

        Assert.False(viewModel.HasQualityCandidates);
        Assert.Equal("尚未选择 GLB", viewModel.QualitySourceName);
        Assert.Contains("已载入源模型 · r0", viewModel.WorkingGeometryText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OriginalCandidate_CanBeFrozenWithoutCallingProvider()
    {
        TestContext test = CreateContext();
        FakeClient client = new();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(client);

        await viewModel.LoadSourceAsync(test.VoxPath);

        Assert.True(viewModel.IsOriginalMode);
        Assert.True(viewModel.CanAccept);
        viewModel.AcceptCurrentSession();

        Ra2VoxelAcceptedCandidate candidate = Assert.IsType<Ra2VoxelAcceptedCandidate>(viewModel.AcceptedCandidate);
        Assert.Equal(Ra2VoxelAcceptedCandidateKind.Original, candidate.Kind);
        Assert.Equal(viewModel.CurrentPreviewSnapshot!.CanonicalHash, candidate.CanonicalHash);
        Assert.EndsWith("-candidate.vox", candidate.SuggestedFileName, StringComparison.Ordinal);
        Assert.True(viewModel.IsAccepted);
        Assert.Equal(0, client.CallCount);
    }

    [Fact]
    public async Task FrozenStyledCandidate_SurvivesReviewNavigationAndInvalidatesOnStyleChange()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient(ContrastProposalResponse()));
        await viewModel.LoadSourceAsync(test.VoxPath);
        await PrepareColourInputsAsync(viewModel);
        await viewModel.CompileAsync();
        viewModel.QualityWarningsAccepted = true;
        viewModel.AcceptCurrentSession();
        Ra2VoxelAcceptedCandidate accepted = Assert.IsType<Ra2VoxelAcceptedCandidate>(viewModel.AcceptedCandidate);

        viewModel.SetPreviewMode(Ra2VoxelStylePreviewMode.RegionMask);

        Assert.Same(accepted, viewModel.AcceptedCandidate);
        Assert.True(viewModel.IsAccepted);
        Assert.False(viewModel.CanAccept);

        viewModel.StyleOverride = "改为更暗的军绿色。";

        Assert.Null(viewModel.AcceptedCandidate);
        Assert.False(viewModel.IsAccepted);
    }

    [Fact]
    public async Task ReadOnlyQualityGeneration_PreservesStylePreviewAndFrozenStyledCandidate()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient(ContrastProposalResponse()));
        await viewModel.LoadSourceAsync(test.VoxPath);
        await PrepareColourInputsAsync(viewModel);
        await viewModel.CompileAsync();
        viewModel.QualityWarningsAccepted = true;
        viewModel.AcceptCurrentSession();
        Ra2VoxelAcceptedCandidate frozen = Assert.IsType<Ra2VoxelAcceptedCandidate>(viewModel.AcceptedCandidate);
        viewModel.SelectQualitySource(test.GlbPath);

        await viewModel.GenerateQualityCandidatesAsync();

        Assert.True(viewModel.HasPreview);
        Assert.Same(frozen, viewModel.AcceptedCandidate);
        Assert.True(viewModel.IsAccepted);
    }

    [Fact]
    public async Task DifferentMaterializablePreview_CanReplaceFrozenCandidateExplicitly()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient(ContrastProposalResponse()));
        await viewModel.LoadSourceAsync(test.VoxPath);
        await PrepareColourInputsAsync(viewModel);
        await viewModel.CompileAsync();
        viewModel.QualityWarningsAccepted = true;
        viewModel.AcceptCurrentSession();
        Assert.Equal(Ra2VoxelAcceptedCandidateKind.Styled, viewModel.AcceptedCandidate!.Kind);

        viewModel.SetPreviewMode(Ra2VoxelStylePreviewMode.Contrast);

        Assert.True(viewModel.CanAccept);
        viewModel.AcceptCurrentSession();
        Assert.Equal(Ra2VoxelAcceptedCandidateKind.ContrastStyled, viewModel.AcceptedCandidate!.Kind);
        Assert.False(viewModel.CanAccept);
    }

    [Fact]
    public async Task ReviewOnlyModes_CannotBeFrozenAsFinalCandidate()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient(ContrastProposalResponse()));
        await viewModel.LoadSourceAsync(test.VoxPath);
        await PrepareColourInputsAsync(viewModel);
        await viewModel.CompileAsync();

        viewModel.SetPreviewMode(Ra2VoxelStylePreviewMode.RegionMask);

        Assert.False(viewModel.CanAccept);
        viewModel.AcceptCurrentSession();
        Assert.Null(viewModel.AcceptedCandidate);
    }

    [Fact]
    public async Task FrozenCandidate_ExportsThroughViewModelAndRemainsAvailable()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient());
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.AcceptCurrentSession();
        string target = Path.Combine(test.ProjectRoot, "exported-candidate.vox");

        await viewModel.ExportAcceptedVoxAsync(target, overwriteExisting: false);

        Assert.True(File.Exists(target));
        Assert.True(viewModel.CanExportVox);
        Assert.NotNull(viewModel.AcceptedCandidate);
        Assert.Contains("通过回读验证", viewModel.StatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExportWithoutFrozenCandidate_DoesNotCreateFile()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient());
        await viewModel.LoadSourceAsync(test.VoxPath);
        string target = Path.Combine(test.ProjectRoot, "must-not-exist.vox");

        await viewModel.ExportAcceptedVoxAsync(target, overwriteExisting: false);

        Assert.False(File.Exists(target));
        Assert.Contains("请先固化", viewModel.StatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DifferencePreview_IsUnavailableWhenNoCandidateWasAdmitted()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient());
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.SelectQualitySource(test.GlbPath);
        await viewModel.GenerateQualityCandidatesAsync();

        viewModel.SetPreviewMode(Ra2VoxelStylePreviewMode.Difference);

        Assert.False(viewModel.IsDifferenceMode);
        Assert.True(viewModel.IsDirectMode);
        Assert.NotNull(viewModel.CurrentPreviewSnapshot);
        Assert.Null(viewModel.CurrentPreviewComparisonSnapshot);
        Assert.Null(viewModel.CurrentPreviewProtectionMask);
        Assert.True(viewModel.CanUseQualityCandidate);
    }

    [Fact]
    public async Task PendingQualitySource_DisablesOldCandidateSelectionAndProjectChangeClearsState()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient());
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.SelectQualitySource(test.GlbPath);
        await viewModel.GenerateQualityCandidatesAsync();
        Assert.True(viewModel.CanUseQualityCandidate);

        string secondGlb = Path.Combine(test.ProjectRoot, "second.glb");
        File.Copy(test.GlbPath, secondGlb);
        viewModel.SelectQualitySource(secondGlb);

        Assert.False(viewModel.CanUseQualityCandidate);

        viewModel.NotifyProjectChanged();

        Assert.False(viewModel.HasSource);
        Assert.False(viewModel.HasQualityCandidates);
        Assert.Empty(viewModel.QualityMetrics);
        Assert.Empty(viewModel.SemanticRegions);
    }

    [Fact]
    public async Task SelectedGeometry_ComposesWithOrdinaryAndOptionalContrastStylePreviews()
    {
        TestContext test = CreateContext();
        FakeClient client = new(ContrastProposalResponse());
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(client);
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.SelectQualitySource(test.GlbPath);
        await viewModel.GenerateQualityCandidatesAsync();
        viewModel.UseCurrentQualityCandidateForSession();
        Ra2VoxelSceneSnapshot working = Assert.IsType<Ra2VoxelSceneSnapshot>(viewModel.ActiveGeometrySnapshot);

        await PrepareColourInputsAsync(viewModel);
        await viewModel.CompileAsync();
        viewModel.QualityWarningsAccepted = true;

        Assert.True(viewModel.HasPreview);
        Assert.True(viewModel.IsResultMode);
        Assert.True(viewModel.CanAccept);
        Assert.True(viewModel.HasContrastCandidate);
        Assert.Contains("调整了", viewModel.PaletteContrastText, StringComparison.Ordinal);
        Assert.Equal(working.CanonicalHash, viewModel.ActiveGeometrySnapshot!.CanonicalHash);
        Assert.Equal(working.OccupancyCount, viewModel.CurrentPreviewSnapshot!.OccupancyCount);
        Assert.Equal(2, client.CallCount);

        viewModel.SetPreviewMode(Ra2VoxelStylePreviewMode.Contrast);

        Assert.True(viewModel.IsContrastMode);
        Assert.NotNull(viewModel.CurrentPreviewSnapshot);
        Assert.True(viewModel.CanAccept);
    }

    [Fact]
    public async Task AdoptedGeometry_RemainsTheBaselineForTheNextQualityPass()
    {
        TestContext test = CreateContext();
        File.WriteAllBytes(test.VoxPath, Ra2MagicaVoxelCodec.Write(CreateAgentRepairableSnapshot()));
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new SemanticEchoClient());
        await viewModel.LoadSourceAsync(test.VoxPath);
        string sourceHash = Assert.IsType<Ra2VoxelSceneSnapshot>(viewModel.ActiveGeometrySnapshot).CanonicalHash;
        viewModel.SelectQualitySource(test.GlbPath);
        await viewModel.GenerateQualityCandidatesAsync();
        await viewModel.AnalyzeStructureAsync();
        Assert.True(viewModel.HasSymmetryCandidate);
        viewModel.SetPreviewMode(Ra2VoxelStylePreviewMode.Symmetry);
        viewModel.UseCurrentQualityCandidateForSession();
        string adoptedHash = Assert.IsType<Ra2VoxelSceneSnapshot>(viewModel.ActiveGeometrySnapshot).CanonicalHash;

        Assert.NotEqual(sourceHash, adoptedHash);

        await viewModel.GenerateQualityCandidatesAsync();

        Assert.Equal(adoptedHash, viewModel.ActiveGeometrySnapshot!.CanonicalHash);
        Assert.Equal(adoptedHash, viewModel.CurrentPreviewComparisonSnapshot?.CanonicalHash ??
            viewModel.CurrentPreviewSnapshot!.CanonicalHash);
    }

    [Fact]
    public async Task ReadOnlyReviewPreservesFrozenCandidateAndAdoptionInvalidatesIt()
    {
        TestContext test = CreateContext();
        File.WriteAllBytes(test.VoxPath, Ra2MagicaVoxelCodec.Write(CreateAgentRepairableSnapshot()));
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new SemanticEchoClient());
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.AcceptCurrentSession();
        Ra2VoxelAcceptedCandidate frozen = Assert.IsType<Ra2VoxelAcceptedCandidate>(viewModel.AcceptedCandidate);
        viewModel.SelectQualitySource(test.GlbPath);

        await viewModel.GenerateQualityCandidatesAsync();
        await viewModel.AnalyzeStructureAsync();

        Assert.Same(frozen, viewModel.AcceptedCandidate);
        Assert.True(viewModel.HasSymmetryCandidate);
        viewModel.SetPreviewMode(Ra2VoxelStylePreviewMode.Symmetry);
        viewModel.UseCurrentQualityCandidateForSession();

        Assert.Null(viewModel.AcceptedCandidate);
        Assert.False(viewModel.HasPreview);
        Assert.False(viewModel.CanUseQualityCandidate);
        Assert.Equal(1, viewModel.WorkingGeometryState!.Revision);
        Assert.True(viewModel.CanAccept);
        viewModel.AcceptCurrentSession();
        Assert.Equal(viewModel.ActiveGeometrySnapshot!.CanonicalHash, viewModel.AcceptedCandidate!.CanonicalHash);
        string exported = Path.Combine(test.ProjectRoot, "adopted-working.vox");
        await viewModel.ExportAcceptedVoxAsync(exported, overwriteExisting: false);
        Assert.True(File.Exists(exported));
        viewModel.SetPreviewMode(Ra2VoxelStylePreviewMode.Original);
        Assert.False(viewModel.CanAccept);
        viewModel.SetPreviewMode(Ra2VoxelStylePreviewMode.Direct);
        Assert.False(viewModel.CanAccept);
    }

    [Fact]
    public async Task StructureRecognition_IsExplicitPublishesOnlyAfterTwoRoundsAndInvalidatesOnGlbChange()
    {
        TestContext test = CreateContext();
        SemanticEchoClient client = new();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(client);
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.SelectQualitySource(test.GlbPath);
        await viewModel.GenerateQualityCandidatesAsync();

        Assert.Equal(0, client.CallCount);
        Assert.True(viewModel.CanAnalyzeStructure);
        Assert.Contains("结构证据已就绪", viewModel.StatusText, StringComparison.Ordinal);
        Assert.False(viewModel.HasStructurePartition);

        await viewModel.AnalyzeStructureAsync();

        Assert.Equal(2, client.CallCount);
        Assert.True(viewModel.HasStructurePartition);
        Assert.True(viewModel.IsStructureRegionsMode);
        Assert.Contains("主分析与审阅一致", viewModel.SemanticReviewText, StringComparison.Ordinal);

        string next = Path.Combine(test.ProjectRoot, "next.glb");
        File.Copy(test.GlbPath, next);
        viewModel.SelectQualitySource(next);

        Assert.False(viewModel.HasStructurePartition);
        Assert.False(viewModel.HasSymmetryCandidate);
        Assert.True(viewModel.IsOriginalMode);
    }

    [Fact]
    public async Task StructureRecognition_RemainsClickableAndExplainsMissingConfiguration()
    {
        TestContext test = CreateContext();
        FakeClient client = new();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(client, configurationReady: false);
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.SelectQualitySource(test.GlbPath);
        await viewModel.GenerateQualityCandidatesAsync();

        Assert.True(viewModel.CanAnalyzeStructure);
        Assert.Contains("配置尚不可用", viewModel.StructureRecognitionToolTip, StringComparison.Ordinal);

        await viewModel.AnalyzeStructureAsync();

        Assert.Contains("DeepSeek 尚未配置", viewModel.StatusText, StringComparison.Ordinal);
        Assert.Equal(0, client.CallCount);
    }

    [Fact]
    public async Task StructureRecognition_CancelledByGlbChangeCannotPublishLateState()
    {
        TestContext test = CreateContext();
        BlockingSemanticClient client = new();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(client);
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.SelectQualitySource(test.GlbPath);
        await viewModel.GenerateQualityCandidatesAsync();

        Task pending = viewModel.AnalyzeStructureAsync();
        await client.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        string next = Path.Combine(test.ProjectRoot, "cancel-next.glb");
        File.Copy(test.GlbPath, next);
        viewModel.SelectQualitySource(next);
        await pending;

        Assert.False(viewModel.HasStructurePartition);
        Assert.False(viewModel.HasSymmetryCandidate);
        Assert.Equal("cancel-next.glb", viewModel.QualitySourceName);
    }

    [Fact]
    public async Task SemanticBrush_AutoPreparesUsesHitRegionAndSupportsAtomicUndoRedo()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient());
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.SemanticBrushPartRole = Ra2VoxelSemanticPartRole.Wheel;
        viewModel.SemanticBrushMaterialRole = Ra2VoxelSemanticMaterialRole.Rubber;
        await viewModel.ActivateSemanticEditModeAsync(Ra2VoxelSemanticEditMode.Paint);
        Assert.True(viewModel.HasSemanticEvidence);
        Assert.True(viewModel.IsSemanticsMode);
        Assert.True(viewModel.IsSemanticPaintMode);
        Assert.Null(viewModel.SelectedSemanticAssignment);
        Ra2VoxelSemanticAssignmentRow row = Assert.IsType<Ra2VoxelSemanticAssignmentRow>(viewModel.SemanticAssignments.First());
        Ra2VoxelCoordinate coordinate = viewModel.ActiveGeometrySnapshot!.Cells[0].Coordinate;

        Assert.True(viewModel.HandleSemanticCellClick(row.RegionId, coordinate));
        Assert.Same(row, viewModel.SelectedSemanticAssignment);
        Ra2VoxelSemanticMaskComposition painted = Assert.IsType<Ra2VoxelSemanticMaskComposition>(viewModel.CurrentPreviewSemanticComposition);
        Assert.Equal(2, painted.Assignments.Count(value => value.MaterialRole == Ra2VoxelSemanticMaterialRole.Rubber));
        Assert.True(viewModel.CanUndoSemanticBrush);

        viewModel.UndoSemanticBrush();
        Assert.DoesNotContain(viewModel.CurrentPreviewSemanticComposition!.Assignments,
            value => value.MaterialRole == Ra2VoxelSemanticMaterialRole.Rubber);
        Assert.True(viewModel.CanRedoSemanticBrush);

        viewModel.RedoSemanticBrush();
        Assert.Equal(2, viewModel.CurrentPreviewSemanticComposition!.Assignments.Count(
            value => value.MaterialRole == Ra2VoxelSemanticMaterialRole.Rubber));

        viewModel.SetPreviewMode(Ra2VoxelStylePreviewMode.Original);
        Assert.True(viewModel.IsOriginalMode);
        Assert.True(viewModel.IsSemanticBrowseMode);
    }

    [Fact]
    public async Task SemanticStroke_CommitsManySeedsAsOneUndoAndReviewDimensionIsPresentationOnly()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient());
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.SemanticBrushPartRole = Ra2VoxelSemanticPartRole.Track;
        viewModel.SemanticBrushMaterialRole = Ra2VoxelSemanticMaterialRole.Rubber;
        await viewModel.ActivateSemanticEditModeAsync(Ra2VoxelSemanticEditMode.Paint);
        Ra2VoxelSemanticAssignmentRow row = viewModel.SemanticAssignments.First();
        Ra2VoxelCoordinate first = viewModel.ActiveGeometrySnapshot!.Cells[0].Coordinate;
        Ra2VoxelCoordinate second = viewModel.ActiveGeometrySnapshot.Cells[1].Coordinate;

        Assert.True(viewModel.BeginSemanticStroke(row.RegionId, first));
        Assert.False(viewModel.CanUndoSemanticBrush);
        viewModel.ReportSemanticStrokeProgress(3);
        Assert.Contains("3 个表面采样点", viewModel.SemanticEditStatus, StringComparison.Ordinal);
        int compositionPublications = 0;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewSemanticComposition))
                compositionPublications++;
        };
        Assert.True(viewModel.CommitSemanticStroke([first, second, first]));
        Assert.Equal(1, compositionPublications);
        Assert.True(viewModel.CanUndoSemanticBrush);
        string compositionHash = viewModel.CurrentPreviewSemanticComposition!.CompositionHash;

        viewModel.SetSemanticReviewDimension(Ra2VoxelSemanticReviewDimension.Material);
        Assert.True(viewModel.IsSemanticMaterialReview);
        Assert.Equal(compositionHash, viewModel.CurrentPreviewSemanticComposition!.CompositionHash);
        Assert.Equal(8, viewModel.SemanticReviewLegend.Count);

        viewModel.UndoSemanticBrush();
        Assert.DoesNotContain(viewModel.CurrentPreviewSemanticComposition!.Assignments,
            value => value.PartRole == Ra2VoxelSemanticPartRole.Track &&
                     value.MaterialRole == Ra2VoxelSemanticMaterialRole.Rubber);
    }

    [Fact]
    public async Task SemanticStroke_CancelPreservesLayerAndHistory()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient());
        await viewModel.LoadSourceAsync(test.VoxPath);
        viewModel.SemanticBrushPartRole = Ra2VoxelSemanticPartRole.Wheel;
        viewModel.SemanticBrushMaterialRole = Ra2VoxelSemanticMaterialRole.Rubber;
        await viewModel.ActivateSemanticEditModeAsync(Ra2VoxelSemanticEditMode.Paint);
        Ra2VoxelSemanticAssignmentRow row = viewModel.SemanticAssignments.First();
        Ra2VoxelCoordinate coordinate = viewModel.ActiveGeometrySnapshot!.Cells[0].Coordinate;

        Assert.True(viewModel.BeginSemanticStroke(row.RegionId, coordinate));
        string before = viewModel.CurrentPreviewSemanticComposition!.CompositionHash;
        viewModel.CancelSemanticStroke("测试取消");

        Assert.Equal(before, viewModel.CurrentPreviewSemanticComposition!.CompositionHash);
        Assert.False(viewModel.CanUndoSemanticBrush);
        Assert.Contains("测试取消", viewModel.SemanticEditStatus, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SemanticSidecar_SaveModifyLoad_RestoresLayersAndClearsDirtyHistory()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient());
        await viewModel.LoadSourceAsync(test.VoxPath);
        await viewModel.PrepareSemanticRegionsAsync();
        Ra2VoxelSemanticAssignmentRow row = viewModel.SemanticAssignments.First();
        row.PartRole = Ra2VoxelSemanticPartRole.Turret;
        row.MaterialRole = Ra2VoxelSemanticMaterialRole.BareMetal;
        viewModel.SemanticBrushPartRole = Ra2VoxelSemanticPartRole.Wheel;
        viewModel.SemanticBrushMaterialRole = Ra2VoxelSemanticMaterialRole.Rubber;
        viewModel.SetSemanticEditMode(Ra2VoxelSemanticEditMode.Paint);
        Assert.True(viewModel.HandleSemanticCellClick(row.RegionId, viewModel.ActiveGeometrySnapshot!.Cells[0].Coordinate));
        Assert.True(viewModel.IsSemanticSidecarDirty);
        string savedComposition = viewModel.CurrentPreviewSemanticComposition!.CompositionHash;
        string path = Path.Combine(test.ProjectRoot, "sample.semantic.json");

        await viewModel.SaveSemanticSidecarAsync(path);
        Assert.False(viewModel.IsSemanticSidecarDirty);
        Assert.Contains("已保存", viewModel.SemanticPersistenceStatus, StringComparison.Ordinal);

        row = viewModel.SemanticAssignments.First(value => value.RegionId == row.RegionId);
        row.PartRole = Ra2VoxelSemanticPartRole.Attachment;
        Assert.True(viewModel.IsSemanticSidecarDirty);
        Assert.Equal(Ra2VoxelSemanticPartRole.Attachment, viewModel.SemanticAssignments.First(value => value.RegionId == row.RegionId).PartRole);

        await viewModel.LoadSemanticSidecarAsync(path);
        Assert.False(viewModel.IsSemanticSidecarDirty);
        Assert.False(viewModel.CanUndoSemanticBrush);
        Assert.False(viewModel.CanRedoSemanticBrush);
        Assert.True(viewModel.IsSemanticBrowseMode);
        Assert.Equal(savedComposition, viewModel.CurrentPreviewSemanticComposition!.CompositionHash);
        Assert.Equal(Ra2VoxelSemanticPartRole.Turret, viewModel.SemanticAssignments.First(value => value.RegionId == row.RegionId).PartRole);
    }

    [Fact]
    public void SemanticPointerFeedback_IsVisibleAndPreservesFailureSeverity()
    {
        TestContext test = CreateContext();
        using Ra2VoxelStyleWorkspaceViewModel viewModel = test.CreateViewModel(new FakeClient());

        viewModel.ReportSemanticPointerFeedback("未命中模型表面。", isError: false);

        Assert.Equal("未命中模型表面。", viewModel.SemanticEditStatus);
        Assert.Equal("未命中模型表面。", viewModel.StatusText);
        Assert.False(viewModel.IsError);

        viewModel.ReportSemanticPointerFeedback("当前场景命中数据已过期。", isError: true);

        Assert.Equal("当前场景命中数据已过期。", viewModel.SemanticEditStatus);
        Assert.Equal("当前场景命中数据已过期。", viewModel.StatusText);
        Assert.True(viewModel.IsError);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private TestContext CreateContext()
    {
        string projectRoot = Directory.CreateDirectory(Path.Combine(_root, "project")).FullName;
        string stylePath = Path.Combine(_root, "VOXEL_STYLE.md");
        string instructionsPath = Path.Combine(_root, "COMPILER.md");
        string cacheRoot = Path.Combine(_root, "cache");
        Directory.CreateDirectory(_root);
        File.WriteAllText(stylePath, "低饱和装甲载具风格。", new UTF8Encoding(false));
        File.WriteAllText(instructionsPath, "Compile a bounded voxel style plan.", new UTF8Encoding(false));
        string voxPath = Path.Combine(projectRoot, "sample.vox");
        File.WriteAllBytes(voxPath, Ra2MagicaVoxelCodec.Write(CreateSnapshot()));
        string glbPath = Path.Combine(projectRoot, "sample.glb");
        File.WriteAllBytes(glbPath, CreateCubeGlb());
        return new(projectRoot, voxPath, glbPath, stylePath, instructionsPath, cacheRoot);
    }

    private static Ra2VoxelSceneSnapshot CreateSnapshot()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index))
            .ToArray();
        colours[0] = new Ra2Rgba32(0, 0, 0, 0);
        Ra2VoxelPaletteProfile palette = new("ui-vm-test", colours, [0]);
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "sample", 4, 4, 4);
        return new(
            "sample",
            part,
            palette,
            [
                new(new Ra2VoxelCoordinate(1, 1, 1), 60),
                new(new Ra2VoxelCoordinate(2, 1, 1), 60),
                new(new Ra2VoxelCoordinate(1, 2, 1), 60),
                new(new Ra2VoxelCoordinate(2, 2, 1), 60),
                new(new Ra2VoxelCoordinate(1, 1, 2), 60),
                new(new Ra2VoxelCoordinate(2, 1, 2), 60),
                new(new Ra2VoxelCoordinate(1, 2, 2), 60),
                new(new Ra2VoxelCoordinate(2, 2, 2), 60)
            ]);
    }

    private static Ra2VoxelSceneSnapshot CreateAgentRepairableSnapshot()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot();
        List<Ra2VoxelCell> cells = [];
        for (int z = 3; z <= 10; z++)
        for (int y = 3; y <= 12; y++)
        for (int x = 3; x <= 12; x++)
            cells.Add(new(new Ra2VoxelCoordinate(x, y, z), 60));
        for (int z = 6; z <= 7; z++)
        for (int y = 5; y <= 6; y++)
            cells.Add(new(new Ra2VoxelCoordinate(2, y, z), 60));
        return new(
            "rugged",
            new Ra2VoxelPartDescriptor("body", Ra2VoxelAssemblyPartRole.Body, "Body", "rugged", 16, 16, 16),
            source.Palette,
            cells);
    }

    private static byte[] CreateCubeGlb()
    {
        float[] positions =
        [
            0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0,
            0, 0, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1
        ];
        using MemoryStream binStream = new();
        using (BinaryWriter writer = new(binStream, Encoding.UTF8, leaveOpen: true))
        {
            foreach (float position in positions)
                writer.Write(position);
            foreach (ushort index in CubeIndices)
                writer.Write(index);
        }
        while ((binStream.Length & 3) != 0)
            binStream.WriteByte(0);
        byte[] bin = binStream.ToArray();
        int positionBytes = positions.Length * sizeof(float);
        int indexBytes = CubeIndices.Length * sizeof(ushort);
        string json = $$"""
        {"asset":{"version":"2.0"},"scene":0,"scenes":[{"nodes":[0]}],"nodes":[{"mesh":0}],"meshes":[{"primitives":[{"attributes":{"POSITION":0},"indices":1,"mode":4}]}],"buffers":[{"byteLength":{{bin.Length}}}],"bufferViews":[{"buffer":0,"byteOffset":0,"byteLength":{{positionBytes}}},{"buffer":0,"byteOffset":{{positionBytes}},"byteLength":{{indexBytes}}}],"accessors":[{"bufferView":0,"componentType":5126,"count":8,"type":"VEC3"},{"bufferView":1,"componentType":5123,"count":{{CubeIndices.Length}},"type":"SCALAR"}]}
        """;
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        Array.Resize(ref jsonBytes, (jsonBytes.Length + 3) & ~3);
        for (int index = json.Length; index < jsonBytes.Length; index++)
            jsonBytes[index] = 0x20;
        using MemoryStream output = new();
        using (BinaryWriter writer = new(output, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(0x46546C67u);
            writer.Write(2u);
            writer.Write(checked((uint)(12 + 8 + jsonBytes.Length + 8 + bin.Length)));
            writer.Write(checked((uint)jsonBytes.Length));
            writer.Write(0x4E4F534Au);
            writer.Write(jsonBytes);
            writer.Write(checked((uint)bin.Length));
            writer.Write(0x004E4942u);
            writer.Write(bin);
        }
        return output.ToArray();
    }

    private sealed record TestContext(
        string ProjectRoot,
        string VoxPath,
        string GlbPath,
        string StylePath,
        string InstructionsPath,
        string CacheRoot)
    {
        internal Ra2VoxelStyleWorkspaceViewModel CreateViewModel(IRa2AiClient client, bool configurationReady = true)
        {
            Ra2VoxelStylePreviewCoordinator coordinator = new(
                _ => client,
                new Ra2VoxelStylePlanCache(CacheRoot),
                StylePath,
                InstructionsPath,
                _ => configurationReady,
                new Ra2VoxelUnitClassProposalCache(Path.Combine(CacheRoot, "unit-class")),
                Ra2AgentSkillCatalog.LoadBundled());
            return new(coordinator, () => ProjectRoot, () => DeepSeekRa2AiModel.V4Flash);
        }
    }

    private static async Task PrepareColourInputsAsync(Ra2VoxelStyleWorkspaceViewModel viewModel)
    {
        await viewModel.AnalyzeUnitClassAsync();
        Assert.NotNull(viewModel.SelectedUnitClass);
        viewModel.ConfirmUnitClass();
        Assert.True(viewModel.HasConfirmedUnitClass);
        viewModel.SelectedBaseColour = Assert.Single(
            viewModel.BaseColourOptions,
            value => value.PaletteIndex == 100);
        Assert.True(viewModel.CanCompile);
    }

    private static Ra2AiResponse ContrastProposalResponse() => Ra2AiResponse.CreateToolCalls(
    [
        new Ra2AiToolCall(
            "style-ui-vm-contrast",
            Ra2VoxelStyleCompiler.ToolName,
            """
            {"outcome":"proposal","message":"","title":"Soft olive vehicle","summary":"Ordinary candidate with optional contrast","remap_policy":"none","interior_role_id":"body.dark","roles":[{"id":"body.base","category":"body_base","exact_palette_index":-1,"target_rgb":[100,100,100],"source_scope_ids":["built-in"]},{"id":"body.light","category":"body_light","exact_palette_index":-1,"target_rgb":[102,102,102],"source_scope_ids":["built-in"]},{"id":"body.mid","category":"body_mid","exact_palette_index":-1,"target_rgb":[99,99,99],"source_scope_ids":["built-in"]},{"id":"body.dark","category":"body_dark","exact_palette_index":-1,"target_rgb":[97,97,97],"source_scope_ids":["built-in"]},{"id":"underside","category":"underside","exact_palette_index":-1,"target_rgb":[80,80,80],"source_scope_ids":["built-in"]},{"id":"edge","category":"body_light","exact_palette_index":-1,"target_rgb":[120,120,120],"source_scope_ids":["built-in"]}],"rules":[{"region":"whole_part","role_id":"body.base","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"top_exposed","role_id":"body.light","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"side_exposed","role_id":"body.mid","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"under_exposed","role_id":"underside","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"edge_or_ridge","role_id":"edge","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"interior","role_id":"body.dark","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]}],"semantic_bindings":[],"unresolved_assumptions":[]}
            """)
    ]);

    private sealed class FakeClient(params Ra2AiResponse[] responses) : IRa2AiClient
    {
        private readonly Queue<Ra2AiResponse> _responses = new(responses);
        internal int CallCount { get; private set; }

        public Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            if (request.Tools.Any(tool => string.Equals(
                    tool.Name,
                    Ra2VoxelUnitClassClassifier.ToolName,
                    StringComparison.Ordinal)))
            {
                string[] lines = request.UserContentText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                string hash = lines.Single(line => line.StartsWith("evidence_hash: ", StringComparison.Ordinal))["evidence_hash: ".Length..];
                string[] facts = lines.Where(line => line.StartsWith("fact: ", StringComparison.Ordinal))
                    .Select(line => line["fact: ".Length..line.IndexOf(" kind=", StringComparison.Ordinal)])
                    .Take(3)
                    .ToArray();
                string json = JsonSerializer.Serialize(new
                {
                    proposed_class = "ground",
                    confidence_band = "high",
                    evidence_fact_ids = facts,
                    reason = "Bounded geometry, semantic, and orientation facts support a ground-unit proposal.",
                    evidence_hash = hash
                });
                return Task.FromResult(Ra2AiResponse.CreateToolCalls(
                    [new Ra2AiToolCall("class-ui-vm", Ra2VoxelUnitClassClassifier.ToolName, json)]));
            }
            if (_responses.Count == 0)
                throw new InvalidOperationException("Quality generation must not call the AI client.");
            return Task.FromResult(_responses.Dequeue());
        }
    }

    private sealed class SemanticEchoClient : IRa2AiClient
    {
        internal int CallCount { get; private set; }

        public Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            string[] lines = request.UserContentText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            string hash = lines.Single(line => line.StartsWith("evidence_hash=", StringComparison.Ordinal))["evidence_hash=".Length..];
            int plane = int.Parse(lines.Single(line => line.StartsWith("selected_plane_twice_x=", StringComparison.Ordinal))["selected_plane_twice_x=".Length..]);
            string[] regionIds = lines.Where(line => line.Contains("|cells=", StringComparison.Ordinal))
                .Select(line => line[..line.IndexOf('|')])
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            string json = JsonSerializer.Serialize(new
            {
                outcome = "proposal",
                message = "",
                evidence_hash = hash,
                reviewed_plane_twice_x = plane,
                operations = new[]
                {
                    new
                    {
                    target_id = regionIds.FirstOrDefault(id => id.StartsWith("repair", StringComparison.Ordinal)) ?? regionIds.First(),
                    action = "add_mirror",
                    confidence = 0.96d,
                    reason = "bounded fixture evidence"
                    }
                },
                unresolved_assumptions = Array.Empty<string>()
            });
            return Task.FromResult(Ra2AiResponse.CreateToolCalls([
                new Ra2AiToolCall("semantic-echo", Ra2VoxelSemanticSymmetryCompiler.ToolName, json)
            ]));
        }
    }

    private sealed class BlockingSemanticClient : IRa2AiClient
    {
        internal TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Cancellation should end the blocking request.");
        }
    }
}
