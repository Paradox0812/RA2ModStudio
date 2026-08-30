using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AssetAuthoring;
using System.Text;
using System.Text.Json;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelStylePreviewCoordinatorTests : IDisposable
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

    private readonly string _root = Path.Combine(Path.GetTempPath(), "ra2-voxel-style-ui-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void LoadSource_AcceptsContainedVoxAndRejectsOutsideProject()
    {
        TestContext test = CreateContext();
        FakeClient client = new(ProposalResponse());
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(client);

        Ra2VoxelStyleSourceLoadResult accepted = coordinator.LoadSource(test.ProjectRoot, test.VoxPath);
        string outside = Path.Combine(test.CaseRoot, "outside.vox");
        File.Copy(test.VoxPath, outside);
        Ra2VoxelStyleSourceLoadResult rejected = coordinator.LoadSource(test.ProjectRoot, outside);

        Assert.True(accepted.IsSuccess, accepted.Message);
        Assert.NotEmpty(accepted.OriginalSliceStackPng!);
        Assert.Equal(Ra2VoxelStyleSourceLoadFailureKind.OutsideProject, rejected.FailureKind);
        Assert.Equal(0, client.CallCount);
    }

    [Fact]
    public void LoadSource_AcceptsContainedSingleSectionVxlWithExplicitPaletteAndRejectsMissingPalette()
    {
        TestContext test = CreateContext();
        FakeClient client = new(ProposalResponse());
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(client);

        Ra2VoxelStyleSourceLoadResult missingPalette = coordinator.LoadSource(test.ProjectRoot, test.VxlPath);
        Ra2VoxelStyleSourceLoadResult accepted = coordinator.LoadSource(test.ProjectRoot, test.VxlPath, test.PalettePath);

        Assert.Equal(Ra2VoxelStyleSourceLoadFailureKind.SourceRejected, missingPalette.FailureKind);
        Assert.Contains(".pal", missingPalette.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(accepted.IsSuccess, accepted.Message);
        Assert.Equal(2, accepted.Snapshot!.OccupancyCount);
        Assert.Equal((byte)16, accepted.Snapshot.Palette.RemapIndices.First());
        Assert.NotEmpty(accepted.OriginalSliceStackPng!);
        Assert.Equal(0, client.CallCount);
    }

    [Fact]
    public void ConvertGeneratedGlb_CreatesSessionSourceWithoutInventingAFilePathOrWritingProjectFiles()
    {
        TestContext test = CreateContext();
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(new FakeClient(ProposalResponse()));
        string[] before = Directory.GetFiles(test.ProjectRoot, "*", SearchOption.AllDirectories)
            .Order(StringComparer.OrdinalIgnoreCase).ToArray();

        Ra2VoxelStyleSourceLoadResult result = coordinator.ConvertGeneratedGlb(
            test.ProjectRoot,
            File.ReadAllBytes(test.GlbPath),
            test.PalettePath,
            paletteSource: null,
            targetLongestDimension: 32);

        Assert.True(result.IsSuccess, result.Message);
        Assert.True(result.IsGeneratedSession);
        Assert.Null(result.FilePath);
        Assert.Equal(test.ProjectRoot, result.StyleAnchorDirectory);
        Assert.Equal("生成候选（会话）", result.DisplayName);
        Assert.NotNull(result.Snapshot);
        Assert.Equal(32, Math.Max(result.Snapshot!.Part.XSize, Math.Max(result.Snapshot.Part.YSize, result.Snapshot.Part.ZSize)));
        Ra2VoxelQualityPreviewResult quality = coordinator.GenerateQualityCandidatesFromGenerated(result);
        Assert.True(quality.IsSuccess, quality.Message);
        Assert.True(quality.IsGeneratedSession);
        Assert.Null(quality.FilePath);
        Assert.NotNull(quality.DirectCandidate);
        Assert.NotNull(quality.RefinedCandidate);
        Ra2VoxelCell first = result.Snapshot.Cells[0];
        Ra2VoxelSceneSnapshot adopted = new(
            result.Snapshot.SceneId,
            result.Snapshot.Part,
            result.Snapshot.Palette,
            result.Snapshot.Cells.Select(cell => cell.Coordinate == first.Coordinate ? cell with { PaletteIndex = 61 } : cell),
            result.Snapshot.SourceArtifactHashes);
        Ra2VoxelQualityPreviewResult continued = coordinator.GenerateQualityCandidatesFromGenerated(result, adopted, 2);
        Assert.True(continued.IsSuccess, continued.Message);
        Assert.Equal(adopted.CanonicalHash, continued.DirectCandidate!.CanonicalHash);
        Assert.Equal(2, continued.WorkingRevision);
        Assert.Equal(before, Directory.GetFiles(test.ProjectRoot, "*", SearchOption.AllDirectories)
            .Order(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    [Fact]
    public async Task CompilePreview_UsesOneStructuredCallAndKeepsProjectReadOnly()
    {
        TestContext test = CreateContext();
        FakeClient client = new(ProposalResponse());
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(client);
        Ra2VoxelStyleSourceLoadResult source = coordinator.LoadSource(test.ProjectRoot, test.VoxPath);
        string sourceHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(test.VoxPath)));
        string[] filesBefore = Directory.GetFiles(test.ProjectRoot, "*", SearchOption.AllDirectories)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Ra2VoxelStylePreviewResult result = await coordinator.CompilePreviewAsync(
            source,
            test.ProjectRoot,
            "保持低饱和军绿色，并用几何明暗增强轮廓。",
            DeepSeekRa2AiModel.V4Flash,
            CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(1, client.CallCount);
        Assert.Contains("no remap indices", client.LastRequest!.SystemPromptText, StringComparison.Ordinal);
        Assert.NotNull(result.FindArtifactBytes("body-coloured-slicestack.png"));
        Assert.NotNull(result.FindArtifactBytes("palette-swatch.png"));
        Assert.NotNull(result.FindArtifactBytes("region-mask.png"));
        Assert.NotNull(result.ResultSnapshot);
        Assert.NotNull(result.GeometryMask);
        Assert.Equal(source.Snapshot!.OccupancyCount, result.ResultSnapshot!.OccupancyCount);
        Assert.Equal(source.Snapshot.CanonicalHash, result.GeometryMask!.SourceSnapshotHash);
        Assert.Equal(
            filesBefore,
            Directory.GetFiles(test.ProjectRoot, "*", SearchOption.AllDirectories)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray());
        Assert.Equal(sourceHash, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(test.VoxPath))));
    }

    [Fact]
    public async Task CompilePreview_PublishesOptionalContrastWithoutReplacingTheOrdinaryResult()
    {
        TestContext test = CreateContext();
        FakeClient client = new(ContrastProposalResponse());
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(client);
        Ra2VoxelStyleSourceLoadResult source = coordinator.LoadSource(test.ProjectRoot, test.VoxPath);

        Ra2VoxelStylePreviewResult result = await coordinator.CompilePreviewAsync(
            source,
            test.ProjectRoot,
            null,
            DeepSeekRa2AiModel.V4Flash,
            CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.NotNull(result.ResultSnapshot);
        Assert.NotNull(result.ContrastPlan);
        Assert.NotNull(result.ContrastFacts);
        Assert.True(result.ContrastFacts!.ChangedRoleCount > 0);
        Assert.True(result.ContrastFacts.ExactPaletteSelectionsPreserved);
        Assert.NotNull(result.ContrastResultSnapshot);
        Assert.NotEmpty(result.ContrastSliceStackPng!);
        Assert.NotEqual(result.ResultSnapshot!.CanonicalHash, result.ContrastResultSnapshot!.CanonicalHash);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task CompilePreview_ReportsSpecificRemapFailureForVoxWithoutRemapMetadata()
    {
        TestContext test = CreateContext();
        FakeClient client = new(RemapProposalResponse());
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(client);
        Ra2VoxelStyleSourceLoadResult source = coordinator.LoadSource(test.ProjectRoot, test.VoxPath);

        Ra2VoxelStylePreviewResult result = await coordinator.CompilePreviewAsync(
            source,
            test.ProjectRoot,
            null,
            DeepSeekRa2AiModel.V4Flash,
            CancellationToken.None);

        Assert.Equal(Ra2VoxelStylePreviewFailureKind.CompilerFailure, result.FailureKind);
        Assert.Equal(Ra2VoxelStyleCompilerFailureKind.PaletteValidationFailed, result.CompilerResult!.FailureKind);
        Assert.Equal("风格计划引用了当前色板不存在的颜色范围；普通上色不需要阵营色，请重新编译。", result.Message);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task CompilePreview_ReportsTheExactRoleWhenDeepSeekOmitsItsColourSource()
    {
        TestContext test = CreateContext();
        FakeClient client = new(MissingColourProposalResponse());
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(client);
        Ra2VoxelStyleSourceLoadResult source = coordinator.LoadSource(test.ProjectRoot, test.VoxPath);

        Ra2VoxelStylePreviewResult result = await coordinator.CompilePreviewAsync(
            source,
            test.ProjectRoot,
            null,
            DeepSeekRa2AiModel.V4Flash,
            CancellationToken.None);

        Assert.Equal(Ra2VoxelStylePreviewFailureKind.CompilerFailure, result.FailureKind);
        Assert.Equal("风格计划中的颜色角色没有提供色盘索引或 RGB：body.base", result.Message);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task CompilePreview_MapsCancellationWithoutRetry()
    {
        TestContext test = CreateContext();
        BlockingClient client = new();
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(client);
        Ra2VoxelStyleSourceLoadResult source = coordinator.LoadSource(test.ProjectRoot, test.VoxPath);
        using CancellationTokenSource cancellation = new();

        Task<Ra2VoxelStylePreviewResult> pending = coordinator.CompilePreviewAsync(
            source,
            test.ProjectRoot,
            null,
            DeepSeekRa2AiModel.V4Flash,
            cancellation.Token);
        await client.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        Ra2VoxelStylePreviewResult result = await pending;

        Assert.Equal(Ra2VoxelStylePreviewFailureKind.Cancelled, result.FailureKind);
        Assert.Equal(Ra2VoxelStyleCompilerFailureKind.Cancelled, result.CompilerResult!.FailureKind);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public void GenerateQualityCandidates_UsesContainedGlbAndKeepsTheProjectReadOnly()
    {
        TestContext test = CreateContext();
        FakeClient client = new(ProposalResponse());
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(client);
        Ra2VoxelStyleSourceLoadResult source = coordinator.LoadSource(test.ProjectRoot, test.VoxPath);
        string[] filesBefore = Directory.GetFiles(test.ProjectRoot, "*", SearchOption.AllDirectories)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        string glbHashBefore = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(test.GlbPath)));

        Ra2VoxelQualityPreviewResult result = coordinator.GenerateQualityCandidates(
            test.ProjectRoot,
            source,
            test.GlbPath);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(Ra2VoxelQualitySourceProvenance.UserPaired, result.Provenance);
        Assert.NotNull(result.DirectCandidate);
        Assert.NotNull(result.RefinedCandidate);
        Assert.NotNull(result.ReviewPackage);
        Assert.Equal(0, client.CallCount);
        Assert.Equal(filesBefore, Directory.GetFiles(test.ProjectRoot, "*", SearchOption.AllDirectories)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray());
        Assert.Equal(glbHashBefore, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(test.GlbPath))));
    }

    [Fact]
    public void GenerateQualityCandidates_BindsTheBatchToTheExplicitWorkingBaseline()
    {
        TestContext test = CreateContext();
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(new FakeClient());
        Ra2VoxelStyleSourceLoadResult source = coordinator.LoadSource(test.ProjectRoot, test.VoxPath);
        Ra2VoxelCell first = source.Snapshot!.Cells[0];
        Ra2VoxelSceneSnapshot working = new(
            source.Snapshot.SceneId,
            source.Snapshot.Part,
            source.Snapshot.Palette,
            source.Snapshot.Cells.Select(cell => cell.Coordinate == first.Coordinate ? cell with { PaletteIndex = 61 } : cell),
            source.Snapshot.SourceArtifactHashes);

        Ra2VoxelQualityPreviewResult firstBatch = coordinator.GenerateQualityCandidates(
            test.ProjectRoot,
            source,
            working,
            3,
            test.GlbPath);
        Ra2VoxelQualityPreviewResult replay = coordinator.GenerateQualityCandidates(
            test.ProjectRoot,
            source,
            working,
            3,
            test.GlbPath);

        Assert.True(firstBatch.IsSuccess, firstBatch.Message);
        Assert.Equal(working.CanonicalHash, firstBatch.DirectCandidate!.CanonicalHash);
        Assert.Equal(working.CanonicalHash, firstBatch.WorkingBaselineHash);
        Assert.Equal(3, firstBatch.WorkingRevision);
        Assert.Equal(64, firstBatch.MeshEvidenceHash.Length);
        Assert.Equal(64, firstBatch.QualityBatchHash.Length);
        Assert.Equal(firstBatch.QualityBatchHash, replay.QualityBatchHash);
    }

    [Fact]
    public void GenerateQualityCandidates_RejectsOutsideAndConflictingSourcesWithoutPublishingCandidates()
    {
        TestContext test = CreateContext();
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(new FakeClient(ProposalResponse()));
        Ra2VoxelStyleSourceLoadResult source = coordinator.LoadSource(test.ProjectRoot, test.VoxPath);
        string outside = Path.Combine(test.CaseRoot, "outside.glb");
        File.Copy(test.GlbPath, outside);

        Ra2VoxelQualityPreviewResult outsideResult = coordinator.GenerateQualityCandidates(test.ProjectRoot, source, outside);
        Ra2VoxelSceneSnapshot conflictingSnapshot = new(
            source.Snapshot!.SceneId,
            source.Snapshot.Part,
            source.Snapshot.Palette,
            source.Snapshot.Cells,
            [new KeyValuePair<string, string>("mesh.glb", new string('A', 64))]);
        Ra2VoxelStyleSourceLoadResult conflictingSource = source with { Snapshot = conflictingSnapshot };
        Ra2VoxelQualityPreviewResult mismatch = coordinator.GenerateQualityCandidates(
            test.ProjectRoot,
            conflictingSource,
            test.GlbPath);

        Assert.Equal(Ra2VoxelQualityPreviewFailureKind.OutsideProject, outsideResult.FailureKind);
        Assert.Null(outsideResult.DirectCandidate);
        Assert.Equal(Ra2VoxelQualityPreviewFailureKind.SourceMismatch, mismatch.FailureKind);
        Assert.Equal(Ra2VoxelQualitySourceProvenance.Mismatch, mismatch.Provenance);
        Assert.Null(mismatch.DirectCandidate);
    }

    [Fact]
    public void GenerateQualityCandidates_MapsCancellationWithoutPartialPublication()
    {
        TestContext test = CreateContext();
        Ra2VoxelStylePreviewCoordinator coordinator = test.CreateCoordinator(new FakeClient(ProposalResponse()));
        Ra2VoxelStyleSourceLoadResult source = coordinator.LoadSource(test.ProjectRoot, test.VoxPath);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2VoxelQualityPreviewResult result = coordinator.GenerateQualityCandidates(
            test.ProjectRoot,
            source,
            test.GlbPath,
            cancellation.Token);

        Assert.Equal(Ra2VoxelQualityPreviewFailureKind.Cancelled, result.FailureKind);
        Assert.Null(result.DirectCandidate);
        Assert.Null(result.ReviewPackage);
    }

    [Fact]
    public async Task AnalyzeStructure_UsesAgentProposalReviewAndKeepsLocalCandidatesOnNoSafeChange()
    {
        TestContext test = CreateContext();
        Ra2VoxelStylePreviewCoordinator localCoordinator = test.CreateCoordinator(new FakeClient());
        Ra2VoxelStyleSourceLoadResult source = localCoordinator.LoadSource(test.ProjectRoot, test.VoxPath);
        Ra2VoxelQualityPreviewResult quality = localCoordinator.GenerateQualityCandidates(
            test.ProjectRoot,
            source,
            test.GlbPath);
        Assert.True(quality.IsSuccess, quality.Message);
        Assert.True(quality.SymmetryEvidenceResult?.IsSuccess == true, quality.SymmetryEvidenceResult?.Message);
        Assert.NotNull(quality.SymmetryEvidence);

        FakeClient semanticClient = new(
            SemanticToolResponse(quality.SymmetryEvidence!),
            SemanticToolResponse(quality.SymmetryEvidence!));
        Ra2VoxelStylePreviewCoordinator semanticCoordinator = test.CreateCoordinator(semanticClient);
        Ra2VoxelStructurePreviewResult result = await semanticCoordinator.AnalyzeStructureAsync(
            quality,
            DeepSeekRa2AiModel.V4Flash,
            CancellationToken.None);

        Assert.Equal(2, semanticClient.CallCount);
        Assert.NotNull(result.CompilerResult?.Partition);
        Assert.NotNull(quality.DirectCandidate);
        Assert.NotNull(quality.RefinedCandidate);
        Assert.True(result.IsSuccess || result.FailureKind == Ra2VoxelStructurePreviewFailureKind.NoSafeCandidate, result.Message);
        if (result.FailureKind == Ra2VoxelStructurePreviewFailureKind.NoSafeCandidate)
        {
            Assert.Equal(
                "Agent 的最终提案没有产生实际几何变化，因此未生成候选；本地直接候选和平滑候选保持不变。",
                result.Message);
        }
    }

    [Fact]
    public async Task AnalyzeStructure_ExplainsMissingStructuredToolCallWithoutDiscardingLocalPreview()
    {
        TestContext test = CreateContext();
        Ra2VoxelStylePreviewCoordinator localCoordinator = test.CreateCoordinator(new FakeClient());
        Ra2VoxelStyleSourceLoadResult source = localCoordinator.LoadSource(test.ProjectRoot, test.VoxPath);
        Ra2VoxelQualityPreviewResult quality = localCoordinator.GenerateQualityCandidates(
            test.ProjectRoot,
            source,
            test.GlbPath);
        Assert.True(quality.IsSuccess, quality.Message);

        FakeClient semanticClient = new(Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall("wrong-tool", "unrelated_tool", "{}")
        ]));
        Ra2VoxelStructurePreviewResult result = await test.CreateCoordinator(semanticClient).AnalyzeStructureAsync(
            quality,
            DeepSeekRa2AiModel.V4Flash,
            CancellationToken.None);

        Assert.Equal(Ra2VoxelStructurePreviewFailureKind.CompilerFailure, result.FailureKind);
        Assert.Contains("未返回唯一的结构工具调用", result.Message, StringComparison.Ordinal);
        Assert.NotNull(quality.RefinedCandidate);
        Assert.Equal(1, semanticClient.CallCount);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private TestContext CreateContext()
    {
        string caseRoot = Directory.CreateDirectory(Path.Combine(_root, Guid.NewGuid().ToString("N"))).FullName;
        string projectRoot = Directory.CreateDirectory(Path.Combine(caseRoot, "project")).FullName;
        string stylePath = Path.Combine(caseRoot, "VOXEL_STYLE.md");
        string instructionsPath = Path.Combine(caseRoot, "COMPILER.md");
        string cacheRoot = Path.Combine(caseRoot, "cache");
        File.WriteAllText(stylePath, "低饱和装甲载具风格。", new UTF8Encoding(false));
        File.WriteAllText(instructionsPath, "Compile a bounded voxel style plan.", new UTF8Encoding(false));
        string voxPath = Path.Combine(projectRoot, "sample.vox");
        File.WriteAllBytes(voxPath, Ra2MagicaVoxelCodec.Write(CreateSnapshot()));
        string vxlPath = Path.Combine(projectRoot, "sample.vxl");
        File.WriteAllBytes(vxlPath, CreateSyntheticVxl());
        string palettePath = Path.Combine(projectRoot, "unittem.pal");
        File.WriteAllBytes(palettePath, CreateWestwoodPalette());
        string glbPath = Path.Combine(projectRoot, "sample.glb");
        File.WriteAllBytes(glbPath, CreateCubeGlb());
        return new(caseRoot, projectRoot, voxPath, vxlPath, palettePath, glbPath, stylePath, instructionsPath, cacheRoot);
    }

    private static Ra2VoxelSceneSnapshot CreateSnapshot()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index))
            .ToArray();
        colours[0] = new Ra2Rgba32(0, 0, 0, 0);
        Ra2VoxelPaletteProfile palette = new(
            "ui-test",
            colours,
            [0],
            Enumerable.Range(16, 16).Select(value => (byte)value));
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "sample", 4, 4, 4);
        Ra2VoxelCell[] cells =
        [
            new(new Ra2VoxelCoordinate(1, 1, 1), 60),
            new(new Ra2VoxelCoordinate(2, 1, 1), 60),
            new(new Ra2VoxelCoordinate(1, 2, 1), 60),
            new(new Ra2VoxelCoordinate(2, 2, 1), 60),
            new(new Ra2VoxelCoordinate(1, 1, 2), 60),
            new(new Ra2VoxelCoordinate(2, 1, 2), 60),
            new(new Ra2VoxelCoordinate(1, 2, 2), 60),
            new(new Ra2VoxelCoordinate(2, 2, 2), 60)
        ];
        return new("sample", part, palette, cells);
    }

    private static Ra2AiResponse ProposalResponse() => Ra2AiResponse.CreateToolCalls(
    [
        new Ra2AiToolCall(
            "style-ui-1",
            Ra2VoxelStyleCompiler.ToolName,
            """
            {"outcome":"proposal","message":"","title":"Olive vehicle","summary":"Coarse deterministic shading","remap_policy":"none","interior_role_id":"body.dark","roles":[{"id":"body.base","category":"body_base","exact_palette_index":60,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.light","category":"body_light","exact_palette_index":80,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.dark","category":"body_dark","exact_palette_index":40,"target_rgb":null,"source_scope_ids":["built-in"]}],"rules":[{"region":"whole_part","role_id":"body.base","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"top_exposed","role_id":"body.light","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"interior","role_id":"body.dark","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]}],"unresolved_assumptions":[]}
            """)
    ]);

    private static Ra2AiResponse SemanticToolResponse(Ra2VoxelSymmetryEvidencePackage evidence)
    {
        string target = evidence.Regions.First().RegionId;
        string json = JsonSerializer.Serialize(new
        {
            outcome = "proposal",
            message = "",
            evidence_hash = evidence.PackageHash,
            reviewed_plane_twice_x = evidence.SelectedPlaneTwiceX,
            operations = new[]
            {
                new
                {
                target_id = target,
                action = "add_mirror",
                confidence = 0.96d,
                reason = "bounded test evidence"
                }
            },
            unresolved_assumptions = Array.Empty<string>()
        });
        return Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall("semantic-test", Ra2VoxelSemanticSymmetryCompiler.ToolName, json)
        ]);
    }

    private static Ra2AiResponse ContrastProposalResponse() => Ra2AiResponse.CreateToolCalls(
    [
        new Ra2AiToolCall(
            "style-ui-contrast",
            Ra2VoxelStyleCompiler.ToolName,
            """
            {"outcome":"proposal","message":"","title":"Soft olive vehicle","summary":"Ordinary candidate with optional deterministic contrast","remap_policy":"none","interior_role_id":"body.dark","roles":[{"id":"body.base","category":"body_base","exact_palette_index":-1,"target_rgb":[100,100,100],"source_scope_ids":["built-in"]},{"id":"body.light","category":"body_light","exact_palette_index":-1,"target_rgb":[102,102,102],"source_scope_ids":["built-in"]},{"id":"body.mid","category":"body_mid","exact_palette_index":-1,"target_rgb":[99,99,99],"source_scope_ids":["built-in"]},{"id":"body.dark","category":"body_dark","exact_palette_index":-1,"target_rgb":[97,97,97],"source_scope_ids":["built-in"]}],"rules":[{"region":"whole_part","role_id":"body.base","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"top_exposed","role_id":"body.light","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"side_exposed","role_id":"body.mid","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"interior","role_id":"body.dark","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]}],"unresolved_assumptions":[]}
            """)
    ]);

    private static Ra2AiResponse RemapProposalResponse() => Ra2AiResponse.CreateToolCalls(
    [
        new Ra2AiToolCall(
            "style-ui-remap",
            Ra2VoxelStyleCompiler.ToolName,
            """
            {"outcome":"proposal","message":"","title":"Olive vehicle","summary":"Coarse deterministic shading","remap_policy":"explicit_mask","interior_role_id":"body.dark","roles":[{"id":"body.base","category":"body_base","exact_palette_index":60,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.dark","category":"body_dark","exact_palette_index":40,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"team","category":"remap","exact_palette_index":-1,"target_rgb":[255,0,0],"source_scope_ids":["built-in"]}],"rules":[{"region":"whole_part","role_id":"body.base","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"interior","role_id":"body.dark","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"explicit_mask","role_id":"team","evidence":"explicit_user_mask","mask_id":"team-mask","source_scope_ids":["built-in"]}],"unresolved_assumptions":[]}
            """)
    ]);

    private static Ra2AiResponse MissingColourProposalResponse() => Ra2AiResponse.CreateToolCalls(
    [
        new Ra2AiToolCall(
            "style-ui-missing-colour",
            Ra2VoxelStyleCompiler.ToolName,
            """
            {"outcome":"proposal","message":"","title":"Olive vehicle","summary":"Missing colour source","remap_policy":"none","interior_role_id":"body.dark","roles":[{"id":"body.base","category":"body_base","exact_palette_index":-1,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.dark","category":"body_dark","exact_palette_index":40,"target_rgb":null,"source_scope_ids":["built-in"]}],"rules":[{"region":"whole_part","role_id":"body.base","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"interior","role_id":"body.dark","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]}],"unresolved_assumptions":[]}
            """)
    ]);

    private static byte[] CreateWestwoodPalette()
    {
        byte[] bytes = new byte[Ra2VxlseSliceImportContract.WestwoodPaletteByteLength];
        for (int index = 0; index < 256; index++)
        {
            bytes[index * 3] = (byte)(index & 63);
            bytes[index * 3 + 1] = (byte)((index * 3) & 63);
            bytes[index * 3 + 2] = (byte)((index * 5) & 63);
        }
        return bytes;
    }

    private static byte[] CreateSyntheticVxl()
    {
        const int xSize = 3;
        const int ySize = 2;
        const int zSize = 4;
        const int columnCount = xSize * ySize;
        int[] starts = Enumerable.Repeat(-1, columnCount).ToArray();
        int[] ends = Enumerable.Repeat(-1, columnCount).ToArray();
        using MemoryStream spanData = new();
        WriteColumn(column: 2, z: 3, colour: 17);
        WriteColumn(column: 3, z: 1, colour: 40);

        using MemoryStream body = new();
        using (BinaryWriter bodyWriter = new(body, Encoding.ASCII, leaveOpen: true))
        {
            foreach (int start in starts)
                bodyWriter.Write(start);
            foreach (int end in ends)
                bodyWriter.Write(end);
            spanData.Position = 0;
            spanData.CopyTo(body);
        }

        using MemoryStream output = new();
        using BinaryWriter writer = new(output, Encoding.ASCII, leaveOpen: true);
        WriteFixedAscii(writer, "Voxel Animation", 16);
        writer.Write((uint)1);
        writer.Write((uint)1);
        writer.Write((uint)1);
        writer.Write(checked((uint)body.Length));
        writer.Write((byte)16);
        writer.Write((byte)31);
        writer.Write(new byte[256 * 3]);
        WriteFixedAscii(writer, "Body", 16);
        writer.Write((uint)0);
        writer.Write((uint)1);
        writer.Write((uint)0);
        body.Position = 0;
        body.CopyTo(output);
        writer.Write((uint)0);
        writer.Write((uint)(columnCount * sizeof(int)));
        writer.Write((uint)(columnCount * sizeof(int) * 2));
        writer.Write(1f / 12f);
        for (int index = 0; index < 12; index++)
            writer.Write(index is 0 or 5 or 10 ? 1f : 0f);
        writer.Write(0f);
        writer.Write(0f);
        writer.Write(0f);
        writer.Write((float)xSize);
        writer.Write((float)ySize);
        writer.Write((float)zSize);
        writer.Write((byte)xSize);
        writer.Write((byte)ySize);
        writer.Write((byte)zSize);
        writer.Write((byte)4);
        writer.Flush();
        return output.ToArray();

        void WriteColumn(int column, int z, byte colour)
        {
            starts[column] = checked((int)spanData.Position);
            spanData.WriteByte((byte)z);
            spanData.WriteByte(1);
            spanData.WriteByte(colour);
            spanData.WriteByte(0);
            spanData.WriteByte(1);
            ends[column] = checked((int)spanData.Position - 1);
        }
    }

    private static byte[] CreateCubeGlb()
    {
        float[] positions =
        [
            0, 0, 0,
            1, 0, 0,
            1, 1, 0,
            0, 1, 0,
            0, 0, 1,
            1, 0, 1,
            1, 1, 1,
            0, 1, 1
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
        int paddedJsonLength = (jsonBytes.Length + 3) & ~3;
        Array.Resize(ref jsonBytes, paddedJsonLength);
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

    private static void WriteFixedAscii(BinaryWriter writer, string value, int length)
    {
        byte[] output = new byte[length];
        byte[] encoded = Encoding.ASCII.GetBytes(value);
        Array.Copy(encoded, output, Math.Min(encoded.Length, output.Length));
        writer.Write(output);
    }

    private sealed record TestContext(
        string CaseRoot,
        string ProjectRoot,
        string VoxPath,
        string VxlPath,
        string PalettePath,
        string GlbPath,
        string StylePath,
        string InstructionsPath,
        string CacheRoot)
    {
        internal Ra2VoxelStylePreviewCoordinator CreateCoordinator(IRa2AiClient client) => new(
            _ => client,
            new Ra2VoxelStylePlanCache(CacheRoot),
            StylePath,
            InstructionsPath);
    }

    private sealed class FakeClient(params Ra2AiResponse[] responses) : IRa2AiClient
    {
        private readonly Queue<Ra2AiResponse> _responses = new(responses);
        internal int CallCount { get; private set; }
        internal Ra2AiRequest? LastRequest { get; private set; }

        public Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            LastRequest = request;
            return Task.FromResult(_responses.Dequeue());
        }
    }

    private sealed class BlockingClient : IRa2AiClient
    {
        internal TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal int CallCount { get; private set; }

        public async Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
            Started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException();
        }
    }
}
