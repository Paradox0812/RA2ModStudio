using Xunit;
using System.Text.RegularExpressions;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelStyleWorkspaceUiContractTests
{
    private static readonly string RepositoryRoot = TestRepositoryRoot.Find();

    [Fact]
    public void WorkspaceXaml_ContainsTheApprovedAutomationSurfaceWithoutLegacyGrid()
    {
        string xaml = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "RA2IniEditor.IDE",
            "Views",
            "AssetAuthoring",
            "Ra2VoxelStyleWorkspaceView.xaml"));
        string[] automationIds =
        [
            "VoxelStyle.Document",
            "VoxelStyle.Generation.Card",
            "VoxelStyle.Generation.ReferencePicker",
            "VoxelStyle.Generation.ReferenceFacts",
            "VoxelStyle.Generation.Brief",
            "VoxelStyle.Generation.Advanced",
            "VoxelStyle.Generation.NegativeConstraints",
            "VoxelStyle.Generation.Resolution",
            "VoxelStyle.Generation.Timeout",
            "VoxelStyle.Generation.PalettePicker",
            "VoxelStyle.Generation.PaletteFacts",
            "VoxelStyle.Generation.Submit",
            "VoxelStyle.Generation.Progress",
            "VoxelStyle.Generation.CapabilityNotice",
            "VoxelStyle.Generation.ConfirmDialog",
            "VoxelStyle.Generation.ConfirmSubmit",
            "VoxelStyle.Generation.ConfirmCancel",
            "VoxelStyle.SourcePicker",
            "VoxelStyle.SourcePath",
            "VoxelStyle.StyleSources",
            "VoxelStyle.StyleOverride",
            "VoxelStyle.Semantics.Card",
            "VoxelStyle.Semantics.Instructions",
            "VoxelStyle.Semantics.Prepare",
            "VoxelStyle.Semantics.Analyze",
            "VoxelStyle.Semantics.Accept",
            "VoxelStyle.Semantics.Discard",
            "VoxelStyle.Semantics.Status",
            "VoxelStyle.UnitClass.Status",
            "VoxelStyle.UnitClass.Selector",
            "VoxelStyle.UnitClass.Confirm",
            "VoxelStyle.UnitClass.Skill",
            "VoxelStyle.Orientation.Selector",
            "VoxelStyle.Orientation.Status",
            "VoxelStyle.BaseColour.Selector",
            "VoxelStyle.BaseColour.Swatch",
            "VoxelStyle.BaseColour.Status",
            "VoxelStyle.Template.Selector",
            "VoxelStyle.Template.Description",
            "VoxelStyle.Semantics.PartRows",
            "VoxelStyle.Semantics.MaterialRows",
            "VoxelStyle.Semantics.RemapApproval",
            "VoxelStyle.Semantics.EditMode",
            "VoxelStyle.Semantics.BrushSize",
            "VoxelStyle.Semantics.MirrorBrush",
            "VoxelStyle.Semantics.Undo",
            "VoxelStyle.Semantics.Redo",
            "VoxelStyle.Semantics.SaveSidecar",
            "VoxelStyle.Semantics.LoadSidecar",
            "VoxelStyle.Semantics.PersistenceStatus",
            "VoxelStyle.Semantics.EditStatus",
            "VoxelStyle.Semantics.BrushPart",
            "VoxelStyle.Semantics.BrushMaterial",
            "VoxelStyle.Semantics.BrushRemap",
            "VoxelStyle.Semantics.ReviewDimension",
            "VoxelStyle.Semantics.ReviewPart",
            "VoxelStyle.Semantics.ReviewMaterial",
            "VoxelStyle.Semantics.ReviewLegend",
            "VoxelStyle.Preview.SemanticReviewControls",
            "VoxelStyle.Model",
            "VoxelStyle.Compile",
            "VoxelStyle.Cancel",
            "VoxelStyle.AcceptSession",
            "VoxelStyle.FinalCandidate.Status",
            "VoxelStyle.ExportVox",
            "VoxelStyle.Preview.Original",
            "VoxelStyle.Preview.Direct",
            "VoxelStyle.Preview.Refined",
            "VoxelStyle.Preview.Difference",
            "VoxelStyle.Preview.StructureRegions",
            "VoxelStyle.Preview.Symmetry",
            "VoxelStyle.Preview.Semantics",
            "VoxelStyle.Preview.Result",
            "VoxelStyle.Preview.Contrast",
            "VoxelStyle.Preview.RegionMask",
            "VoxelStyle.Preview.Palette",
            "VoxelStyle.Preview.FormZones",
            "VoxelStyle.Preview.BoundaryIntent",
            "VoxelStyle.Preview.RiskOverlay",
            "VoxelStyle.Preview.GameScale",
            "VoxelStyle.Preview.Image",
            "VoxelStyle.Preview.Viewport3D",
            "VoxelStyle.Preview.ResetCamera",
            "VoxelStyle.Preview.SliceFallback",
            "VoxelStyle.Preview.GeometryLightingNotice",
            "VoxelStyle.Layout.InspectorSplitter",
            "VoxelStyle.Layout.DetailsSplitter",
            "VoxelStyle.Workflow.Tabs",
            "VoxelStyle.Workflow.StageNavigator",
            "VoxelStyle.Workflow.Model",
            "VoxelStyle.Workflow.Geometry",
            "VoxelStyle.Workflow.Semantics",
            "VoxelStyle.Workflow.Colour",
            "VoxelStyle.Workflow.Review",
            "VoxelStyle.Workflow.NextAction",
            "VoxelStyle.Workflow.Output",
            "VoxelStyle.Preview.Toolbar",
            "VoxelStyle.Preview.ModeSelector",
            "VoxelStyle.Details.Tabs",
            "VoxelStyle.Details.Quality",
            "VoxelStyle.Details.Colour",
            "VoxelStyle.Details.Semantics",
            "VoxelStyle.Details.Review",
            "VoxelStyle.Plan.Roles",
            "VoxelStyle.Plan.Rules",
            "VoxelStyle.Review.Issues",
            "VoxelStyle.Quality.SourcePicker",
            "VoxelStyle.Quality.Generate",
            "VoxelStyle.Quality.AnalyzeStructure",
            "VoxelStyle.Quality.Status",
            "VoxelStyle.Quality.Provenance",
            "VoxelStyle.Quality.UseCandidate",
            "VoxelStyle.Quality.Metrics",
            "VoxelStyle.Quality.NormalComparison",
            "VoxelStyle.Quality.Candidates",
            "VoxelStyle.Quality.Admission",
            "VoxelStyle.Quality.StructureFacts",
            "VoxelStyle.Quality.DifferenceLegend",
            "VoxelStyle.Quality.SemanticRegions",
            "VoxelStyle.Quality.SemanticLegend",
            "VoxelStyle.Quality.SemanticReview",
            "VoxelStyle.Quality.PaletteContrast",
            "VoxelStyle.ColourQuality.Status",
            "VoxelStyle.ColourQuality.Metrics",
            "VoxelStyle.ColourQuality.FormZones",
            "VoxelStyle.ColourQuality.Boundaries",
            "VoxelStyle.ColourQuality.Accents",
            "VoxelStyle.ColourQuality.GameScale",
            "VoxelStyle.ColourQuality.Warnings",
            "VoxelStyle.ColourQuality.AcceptWarnings",
            "VoxelStyle.Status"
        ];

        string viewportXaml = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "RA2IniEditor.IDE",
            "Views",
            "AssetAuthoring",
            "Ra2VoxelViewport3D.xaml"));
        string code = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "RA2IniEditor.IDE",
            "Views",
            "AssetAuthoring",
            "Ra2VoxelStyleWorkspaceView.xaml.cs"));
        string automationSurface = xaml + viewportXaml + code;
        foreach (string automationId in automationIds)
            Assert.True(
                automationSurface.Contains($"AutomationProperties.AutomationId=\"{automationId}\"", StringComparison.Ordinal) ||
                automationSurface.Contains($"SetAutomationId(dialog, \"{automationId}\")", StringComparison.Ordinal) ||
                automationSurface.Contains($"SetAutomationId(confirm, \"{automationId}\")", StringComparison.Ordinal) ||
                automationSurface.Contains($"SetAutomationId(cancel, \"{automationId}\")", StringComparison.Ordinal),
                $"Missing AutomationId: {automationId}");
        Assert.DoesNotContain("<DataGrid", xaml, StringComparison.Ordinal);
        Assert.Empty(Regex.Matches(xaml, "<Run\\s+Text=\\\"\\{Binding\\s+[A-Za-z0-9_.]+\\}\\\""));
        Assert.Contains("<Run Text=\"{Binding SourceName, Mode=OneWay}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("MinWidth=\"786\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<ScrollViewer Grid.Row=\"3\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<Viewbox", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("ScaleTransform", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("LayoutTransform", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource UiTabControlStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemContainerStyle=\"{StaticResource UiTabItemStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ResizeDirection=\"Columns\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ResizeDirection=\"Rows\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<RowDefinition Height=\"*\" MinHeight=\"260\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("RenderOptions.BitmapScalingMode=\"NearestNeighbor\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<local:Ra2VoxelViewport3D", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource UiGridSplitterStyle}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Style=\"{StaticResource IdeDockSplitterStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"载入模型…\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"选择 VOX…\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Click=\"AnalyzeUnitClass_OnClick\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("VoxelStyle.UnitClass.Analyze", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("VoxelStyle.UnitClass.Evidence", xaml, StringComparison.Ordinal);
        Assert.Contains("不再调用 DeepSeek 判型", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"ConfirmUnitClass_OnClick\"", xaml, StringComparison.Ordinal);
        Assert.Matches(
            "VoxelStyle\\.UnitClass\\.Selector[^>]+DisplayMemberPath=\\\"Display\\\"",
            xaml);
        Assert.Contains("SelectedItem=\"{Binding SelectedBaseColour, Mode=TwoWay}\"", xaml, StringComparison.Ordinal);
        Assert.Matches(
            "VoxelStyle\\.BaseColour\\.Selector[^>]+DisplayMemberPath=\\\"Display\\\"",
            xaml);
        Assert.Contains("SelectedItem=\"{Binding SelectedTechnique, Mode=TwoWay}\"", xaml, StringComparison.Ordinal);
        Assert.Matches(
            "VoxelStyle\\.Template\\.Selector[^>]+DisplayMemberPath=\\\"DisplayName\\\"",
            xaml);
        Assert.Contains("IsEnabled=\"{Binding HasReviewableColourWarnings}\"", xaml, StringComparison.Ordinal);
        int classAnalyze = xaml.IndexOf("VoxelStyle.UnitClass.Selector", StringComparison.Ordinal);
        int styleSources = xaml.IndexOf("VoxelStyle.StyleSources", StringComparison.Ordinal);
        Assert.True(classAnalyze >= 0 && styleSources > classAnalyze);
        int previewToolbar = xaml.IndexOf("VoxelStyle.Preview.Toolbar", StringComparison.Ordinal);
        int semanticReviewControls = xaml.IndexOf("VoxelStyle.Preview.SemanticReviewControls", StringComparison.Ordinal);
        Assert.True(previewToolbar >= 0 && semanticReviewControls > previewToolbar);
        Assert.Contains("Text=\"分类预览\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Visibility=\"{Binding IsSemanticsMode, Converter={StaticResource BoolToVisibility}}\"", xaml, StringComparison.Ordinal);

        Assert.Contains("<Viewport3D", viewportXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"InputSurface\"", viewportXaml, StringComparison.Ordinal);
        Assert.Contains("MouseLeftButtonDown=\"InputSurface_OnMouseLeftButtonDown\"", viewportXaml, StringComparison.Ordinal);
        Assert.Contains("MouseRightButtonDown=\"InputSurface_OnMouseRightButtonDown\"", viewportXaml, StringComparison.Ordinal);
        Assert.Contains("MouseDown=\"InputSurface_OnMouseDown\"", viewportXaml, StringComparison.Ordinal);
        Assert.Contains("LostMouseCapture=\"InputSurface_OnLostMouseCapture\"", viewportXaml, StringComparison.Ordinal);
        Assert.Contains("左键选择/拖动绘制 · 右键拖动旋转 · Shift+右键/中键平移 · 滚轮缩放", viewportXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Helix", viewportXaml, StringComparison.OrdinalIgnoreCase);

        string viewportCode = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "RA2IniEditor.IDE",
            "Views",
            "AssetAuthoring",
            "Ra2VoxelViewport3D.xaml.cs"));
        Assert.Contains("_hitMap.TryResolve", viewportCode, StringComparison.Ordinal);
        Assert.Contains("InputSurface.CaptureMouse()", viewportCode, StringComparison.Ordinal);
        Assert.Contains("Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? DragMode.Pan : DragMode.Orbit", viewportCode, StringComparison.Ordinal);
        Assert.Contains("InterpolateStrokePoints", viewportCode, StringComparison.Ordinal);
        Assert.Contains("MaximumStrokeSeedCount = Ra2VoxelSemanticMaskEditor.MaximumStrokeSeedCount", viewportCode, StringComparison.Ordinal);
        Assert.Contains("StrokePreviewVisual.Content", viewportCode, StringComparison.Ordinal);
        Assert.Contains("SemanticStrokeCompleted", viewportCode, StringComparison.Ordinal);
        Assert.DoesNotContain("_leftPressPoint", viewportCode, StringComparison.Ordinal);
        Assert.DoesNotContain("bestDistance", viewportCode, StringComparison.Ordinal);
        Assert.DoesNotContain("Viewport_OnMouseButtonUp", viewportCode, StringComparison.Ordinal);

        Assert.Contains("*.vox;*.vxl", code, StringComparison.Ordinal);
        Assert.Contains("Westwood 色板 (*.pal)", code, StringComparison.Ordinal);
        Assert.Contains("LoadSourceAsync(dialog.FileName, palettePath)", code, StringComparison.Ordinal);
        Assert.Contains("glTF Binary (*.glb)|*.glb", code, StringComparison.Ordinal);
        Assert.Contains("SelectQualitySource(dialog.FileName)", code, StringComparison.Ordinal);
        Assert.Contains("GenerateQualityCandidatesAsync()", code, StringComparison.Ordinal);
        Assert.Contains("AnalyzeStructureAsync()", code, StringComparison.Ordinal);
        Assert.Contains("UseCurrentQualityCandidateForSession()", code, StringComparison.Ordinal);
        Assert.Contains("ApplyResponsiveLayout()", code, StringComparison.Ordinal);
        Assert.Contains("_cameraGroupGeneration", code, StringComparison.Ordinal);
        Assert.Contains("GetCurrentOriginalSourceHash", code, StringComparison.Ordinal);
        Assert.Contains("!descriptorChanged && !hashChanged", code, StringComparison.Ordinal);
        Assert.Contains("ExportAcceptedVoxAsync(dialog.FileName, overwriteConfirmed)", code, StringComparison.Ordinal);
        Assert.Contains("Content=\"固化最终候选\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"导出 VOX…\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<WrapPanel", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"VoxelStyle.Preview.Refined\" Content=\"平滑候选\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"VoxelStyle.Preview.Direct\" Content=\"基线候选\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsEnabled=\"{Binding HasRefinedCandidate}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsEnabled=\"{Binding HasQualityDifference}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding QualityCandidatesText}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AI 结构识别是独立的可选步骤", xaml, StringComparison.Ordinal);
        int difference = xaml.IndexOf("VoxelStyle.Preview.Difference", StringComparison.Ordinal);
        int structure = xaml.IndexOf("VoxelStyle.Preview.StructureRegions", StringComparison.Ordinal);
        int symmetry = xaml.IndexOf("VoxelStyle.Preview.Symmetry", StringComparison.Ordinal);
        Assert.True(difference >= 0 && structure > difference && symmetry > structure);
    }

    [Fact]
    public void ShellEntry_IsSingleDynamicNonFloatingDocumentAndNotADockProfile()
    {
        string shellXaml = File.ReadAllText(Path.Combine(RepositoryRoot, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCode = File.ReadAllText(Path.Combine(RepositoryRoot, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("Shell.Menu.VoxelStyleWorkspace", shellXaml, StringComparison.Ordinal);
        Assert.Contains("OpenVoxelStyleWorkspace_OnClick", shellXaml, StringComparison.Ordinal);
        Assert.Equal(1, Count(shellCode, "ContentId = \"Document.VoxelStyle\""));
        Assert.Contains("CanFloat = false", shellCode, StringComparison.Ordinal);
        Assert.Contains("CanMove = false", shellCode, StringComparison.Ordinal);
        Assert.Contains("CloseVoxelStyleWorkspace();", shellCode, StringComparison.Ordinal);

        int profileStart = shellCode.IndexOf("ShellDockToolProfile[] dockProfiles", StringComparison.Ordinal);
        int profileEnd = shellCode.IndexOf("_dockLayoutCoordinator =", profileStart, StringComparison.Ordinal);
        Assert.True(profileStart >= 0 && profileEnd > profileStart);
        Assert.DoesNotContain("Document.VoxelStyle", shellCode[profileStart..profileEnd], StringComparison.Ordinal);
    }

    [Fact]
    public void ProviderCreation_RemainsBehindExplicitCompilePath()
    {
        string coordinator = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "RA2IniEditor.IDE",
            "AssetAuthoring",
            "Ra2VoxelStylePreviewCoordinator.cs"));
        int loadStart = coordinator.IndexOf("internal Ra2VoxelStyleSourceLoadResult LoadSource", StringComparison.Ordinal);
        int compileStart = coordinator.IndexOf("internal async Task<Ra2VoxelStylePreviewResult> CompilePreviewAsync", StringComparison.Ordinal);
        int qualityStart = coordinator.IndexOf("internal Ra2VoxelQualityPreviewResult GenerateQualityCandidates", StringComparison.Ordinal);
        int qualityEnd = coordinator.IndexOf("internal async Task<Ra2VoxelStructurePreviewResult> AnalyzeStructureAsync", qualityStart, StringComparison.Ordinal);
        int structureStart = qualityEnd;
        int structureEnd = coordinator.IndexOf("internal bool IsStructureRecognitionConfigured", structureStart, StringComparison.Ordinal);
        Assert.True(loadStart >= 0 && compileStart > loadStart);
        Assert.True(qualityStart > compileStart && qualityEnd > qualityStart && structureEnd > structureStart);
        Assert.DoesNotContain("_clientFactory(", coordinator[loadStart..compileStart], StringComparison.Ordinal);
        Assert.DoesNotContain("_clientFactory(", coordinator[qualityStart..qualityEnd], StringComparison.Ordinal);
        Assert.Contains("_clientFactory(model)", coordinator[structureStart..structureEnd], StringComparison.Ordinal);
        Assert.Contains("_clientFactory(model)", coordinator[compileStart..], StringComparison.Ordinal);
        Assert.DoesNotContain("File.Write", coordinator, StringComparison.Ordinal);
    }

    private static int Count(string source, string value)
    {
        int count = 0;
        int offset = 0;
        while ((offset = source.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }
        return count;
    }
}
