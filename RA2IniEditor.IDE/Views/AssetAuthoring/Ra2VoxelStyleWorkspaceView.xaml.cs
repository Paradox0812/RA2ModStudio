using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Automation;
using Microsoft.Win32;
using RA2IniEditor.IDE.AssetAuthoring;
using RA2IniEditor.IDE.ViewModels.AssetAuthoring;

namespace RA2IniEditor.IDE.Views.AssetAuthoring;

public partial class Ra2VoxelStyleWorkspaceView : UserControl
{
    private Ra2VoxelStyleWorkspaceViewModel? _subscribedViewModel;
    private string? _renderKey;
    private string? _cameraSourceDescriptor;
    private string? _cameraSourceHash;
    private long _cameraGroupGeneration;
    private bool _inspectorSizeWasAdjusted;
    private bool _detailsSizeWasAdjusted;
    private bool _detailsSelectionInitialized;
    private bool _isGameScaleReview;

    public Ra2VoxelStyleWorkspaceView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnWorkspaceSizeChanged;
        GotKeyboardFocus += (_, _) => ViewModel?.RefreshExternalModelContext();
        VoxelViewport.SceneBuildFailed += OnSceneBuildFailed;
        VoxelViewport.SemanticCellSelected += OnSemanticCellSelected;
        VoxelViewport.SemanticCellHitFailed += OnSemanticCellHitFailed;
        VoxelViewport.SemanticStrokeStarting += OnSemanticStrokeStarting;
        VoxelViewport.SemanticStrokeProgress += OnSemanticStrokeProgress;
        VoxelViewport.SemanticStrokeCompleted += OnSemanticStrokeCompleted;
        VoxelViewport.SemanticStrokeCanceled += OnSemanticStrokeCanceled;
    }

    private Ra2VoxelStyleWorkspaceViewModel? ViewModel => DataContext as Ra2VoxelStyleWorkspaceViewModel;

    private async void ChooseSource_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel)
            return;
        if (viewModel.ProjectRootPath is not string projectRoot)
        {
            viewModel.ReportNoActiveProject();
            return;
        }
        OpenFileDialog dialog = new()
        {
            Title = "选择项目内的体素模型",
            Filter = "体素模型 (*.vox;*.vxl)|*.vox;*.vxl|MagicaVoxel (*.vox)|*.vox|Westwood VXL (*.vxl)|*.vxl",
            CheckFileExists = true,
            Multiselect = false,
            InitialDirectory = projectRoot
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
        {
            string? palettePath = null;
            if (string.Equals(Path.GetExtension(dialog.FileName), ".vxl", StringComparison.OrdinalIgnoreCase))
            {
                OpenFileDialog paletteDialog = new()
                {
                    Title = "为该 VXL 选择对应的 Westwood PAL 色板",
                    Filter = "Westwood 色板 (*.pal)|*.pal",
                    CheckFileExists = true,
                    Multiselect = false,
                    InitialDirectory = projectRoot
                };
                if (paletteDialog.ShowDialog(Window.GetWindow(this)) != true)
                    return;
                palettePath = paletteDialog.FileName;
            }
            if (ConfirmDiscardSemanticChanges(viewModel))
                await viewModel.LoadSourceAsync(dialog.FileName, palettePath);
        }
    }

    private async void Compile_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.CompileAsync();
    }

    private void ConfirmUnitClass_OnClick(object sender, RoutedEventArgs e) =>
        ViewModel?.ConfirmUnitClass();

    private void SelectModelStage_OnClick(object sender, RoutedEventArgs e) =>
        ViewModel?.SelectWorkflowStage(Ra2VoxelWorkspaceStage.Model);

    private void SelectGeometryStage_OnClick(object sender, RoutedEventArgs e) =>
        ViewModel?.SelectWorkflowStage(Ra2VoxelWorkspaceStage.Geometry);

    private void SelectSemanticsStage_OnClick(object sender, RoutedEventArgs e) =>
        ViewModel?.SelectWorkflowStage(Ra2VoxelWorkspaceStage.Semantics);

    private void SelectColourStage_OnClick(object sender, RoutedEventArgs e) =>
        ViewModel?.SelectWorkflowStage(Ra2VoxelWorkspaceStage.Colour);

    private void SelectReviewStage_OnClick(object sender, RoutedEventArgs e) =>
        ViewModel?.SelectWorkflowStage(Ra2VoxelWorkspaceStage.Review);

    private void ChooseGenerationReference_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel)
            return;
        OpenFileDialog dialog = new()
        {
            Title = "选择发送给 Tencent Hunyuan 3D 的参考图",
            Filter = "参考图 (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
            viewModel.SelectGenerationReference(dialog.FileName);
    }

    private void ChooseGenerationPalette_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel)
            return;
        if (viewModel.ProjectRootPath is not string projectRoot)
        {
            viewModel.ReportNoActiveProject();
            return;
        }
        OpenFileDialog dialog = new()
        {
            Title = "选择项目内的 Westwood PAL 色板",
            Filter = "Westwood 色板 (*.pal)|*.pal",
            CheckFileExists = true,
            Multiselect = false,
            InitialDirectory = projectRoot
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
            viewModel.SelectGenerationPalette(dialog.FileName);
    }

    private async void GenerateModel_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel || !await viewModel.PrepareGenerationAsync())
            return;
        bool confirmed = ShowGenerationConsentDialog(viewModel);
        if (confirmed && !ConfirmDiscardSemanticChanges(viewModel))
            confirmed = false;
        await viewModel.GenerateConfirmedAsync(confirmed);
    }

    private static bool ShowGenerationConsentDialog(Ra2VoxelStyleWorkspaceViewModel viewModel)
    {
        bool accepted = false;
        Window dialog = new()
        {
            Title = "确认发送参考图",
            Owner = Application.Current?.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false
        };
        AutomationProperties.SetAutomationId(dialog, "VoxelStyle.Generation.ConfirmDialog");
        var confirm = new Button { Content = "确认并生成", MinWidth = 96, IsDefault = true, Margin = new Thickness(6, 0, 0, 0) };
        var cancel = new Button { Content = "取消", MinWidth = 72, IsCancel = true };
        AutomationProperties.SetAutomationId(confirm, "VoxelStyle.Generation.ConfirmSubmit");
        AutomationProperties.SetAutomationId(cancel, "VoxelStyle.Generation.ConfirmCancel");
        confirm.Click += (_, _) => { accepted = true; dialog.DialogResult = true; };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(cancel);
        buttons.Children.Add(confirm);
        var panel = new StackPanel { Margin = new Thickness(18), Width = 460 };
        panel.Children.Add(new TextBlock
        {
            Text = "将发送 1 张已选择的参考图，并创建 1 次固定 Tencent Hunyuan 3D 几何任务；不自动重试。免费余额无法由本程序确认，只有在你已设置免费包确认环境变量时才应继续。临时结果不会自动保存。",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        });
        panel.Children.Add(new TextBlock { Text = viewModel.GenerationProviderText, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 14) });
        panel.Children.Add(buttons);
        dialog.Content = panel;
        dialog.ShowDialog();
        return accepted;
    }

    private void ChooseQualitySource_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel)
            return;
        if (viewModel.ProjectRootPath is not string projectRoot)
        {
            viewModel.ReportNoActiveProject();
            return;
        }
        OpenFileDialog dialog = new()
        {
            Title = "选择项目内的 GLB 几何质量源",
            Filter = "glTF Binary (*.glb)|*.glb",
            CheckFileExists = true,
            Multiselect = false,
            InitialDirectory = projectRoot
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
            viewModel.SelectQualitySource(dialog.FileName);
    }

    private async void GenerateQuality_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.GenerateQualityCandidatesAsync();
    }

    private async void AnalyzeStructure_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.AnalyzeStructureAsync();
    }

    private async void AnalyzeSemantics_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.AnalyzeSemanticMasksAsync();
    }

    private async void PrepareSemantics_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.PrepareSemanticRegionsAsync();
    }

    private void AcceptSemantics_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AcceptSemanticSuggestions();
    private void DiscardSemantics_OnClick(object sender, RoutedEventArgs e) => ViewModel?.DiscardSemanticSuggestions();

    private void ClearSemanticOverride_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel && sender is FrameworkElement { DataContext: Ra2VoxelSemanticAssignmentRow row })
            viewModel.ClearSemanticOverride(row);
    }

    private async void SemanticBrowse_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.ActivateSemanticEditModeAsync(Ra2VoxelSemanticEditMode.Browse);
    }

    private async void SemanticPaint_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.ActivateSemanticEditModeAsync(Ra2VoxelSemanticEditMode.Paint);
    }

    private async void SemanticErase_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.ActivateSemanticEditModeAsync(Ra2VoxelSemanticEditMode.Erase);
    }

    private void UndoSemanticBrush_OnClick(object sender, RoutedEventArgs e) => ViewModel?.UndoSemanticBrush();
    private void RedoSemanticBrush_OnClick(object sender, RoutedEventArgs e) => ViewModel?.RedoSemanticBrush();
    private async void SaveSemanticSidecar_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { CanSaveSemanticSidecar: true } viewModel)
            return;
        SaveFileDialog dialog = new()
        {
            Title = "保存语义分划",
            Filter = "语义分划 (*.semantic.json)|*.semantic.json",
            AddExtension = true,
            DefaultExt = ".semantic.json",
            FileName = viewModel.SemanticSidecarSuggestedFileName,
            InitialDirectory = viewModel.SemanticSidecarInitialDirectory,
            OverwritePrompt = true
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
            await viewModel.SaveSemanticSidecarAsync(dialog.FileName);
    }

    private async void LoadSemanticSidecar_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { CanLoadSemanticSidecar: true } viewModel)
            return;
        OpenFileDialog dialog = new()
        {
            Title = "载入语义分划",
            Filter = "语义分划 (*.semantic.json)|*.semantic.json",
            CheckFileExists = true,
            Multiselect = false,
            InitialDirectory = viewModel.SemanticSidecarInitialDirectory
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true && ConfirmDiscardSemanticChanges(viewModel))
            await viewModel.LoadSemanticSidecarAsync(dialog.FileName);
    }
    private void SemanticPartReview_OnClick(object sender, RoutedEventArgs e) =>
        ViewModel?.SetSemanticReviewDimension(Ra2VoxelSemanticReviewDimension.Part);
    private void SemanticMaterialReview_OnClick(object sender, RoutedEventArgs e) =>
        ViewModel?.SetSemanticReviewDimension(Ra2VoxelSemanticReviewDimension.Material);

    private void Cancel_OnClick(object sender, RoutedEventArgs e) => ViewModel?.Cancel();
    private void AcceptSession_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AcceptCurrentSession();

    private async void ExportVox_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { CanExportVox: true } viewModel)
            return;
        SaveFileDialog dialog = new()
        {
            Title = "导出已固化的最终 VOX 候选",
            Filter = "MagicaVoxel (*.vox)|*.vox",
            AddExtension = true,
            DefaultExt = ".vox",
            FileName = viewModel.ExportSuggestedFileName,
            InitialDirectory = viewModel.ExportInitialDirectory,
            OverwritePrompt = true
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) != true)
            return;

        bool overwriteConfirmed = File.Exists(dialog.FileName);
        await viewModel.ExportAcceptedVoxAsync(dialog.FileName, overwriteConfirmed);
    }

    private void ShowOriginal_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.Original);
    private void ShowDirect_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.Direct);
    private void ShowRefined_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.Refined);
    private void ShowDifference_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.Difference);
    private void ShowStructureRegions_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.StructureRegions);
    private void ShowSymmetry_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.Symmetry);
    private void ShowSemantics_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.Semantics);
    private void UseQualityCandidate_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel && ConfirmDiscardSemanticChanges(viewModel))
            viewModel.UseCurrentQualityCandidateForSession();
    }
    private void ShowResult_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.Result);
    private void ShowContrast_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.Contrast);
    private void ShowRegionMask_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.RegionMask);
    private void ShowPalette_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.Palette);
    private void ShowFormZones_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.FormZones);
    private void ShowBoundaryIntent_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.BoundaryIntent);
    private void ShowRiskOverlay_OnClick(object sender, RoutedEventArgs e) => ViewModel?.SetPreviewMode(Ra2VoxelStylePreviewMode.RiskOverlay);
    private void ToggleGameScale_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.CanUseRev7DiagnosticPreviews != true)
            return;
        bool changed = _isGameScaleReview
            ? VoxelViewport.ExitGameScaleReview()
            : VoxelViewport.EnterGameScaleReview();
        if (!changed) return;
        _isGameScaleReview = !_isGameScaleReview;
        if (sender is Button button)
            button.Content = _isGameScaleReview ? "恢复视距" : "游戏尺寸";
    }

    private void ExitGameScaleReviewIfActive()
    {
        if (!_isGameScaleReview) return;
        VoxelViewport.ExitGameScaleReview();
        _isGameScaleReview = false;
        GameScalePreviewButton.Content = "游戏尺寸";
    }
    private void ResetCamera_OnClick(object sender, RoutedEventArgs e) => VoxelViewport.ResetCamera();
    private void ToggleSliceFallback_OnClick(object sender, RoutedEventArgs e) => ViewModel?.ToggleSliceFallback();

    private bool ConfirmDiscardSemanticChanges(Ra2VoxelStyleWorkspaceViewModel viewModel)
    {
        if (!viewModel.HasUnsavedSemanticSidecarChanges)
            return true;
        MessageBoxResult result = MessageBox.Show(
            Window.GetWindow(this),
            "当前语义分划有未保存修改。继续会丢弃这些修改，是否继续？",
            "未保存的语义分划",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        return result == MessageBoxResult.Yes;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_subscribedViewModel is not null)
            _subscribedViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _subscribedViewModel = e.NewValue as Ra2VoxelStyleWorkspaceViewModel;
        if (_subscribedViewModel is not null)
            _subscribedViewModel.PropertyChanged += OnViewModelPropertyChanged;
        _renderKey = null;
        _cameraSourceDescriptor = GetCameraSourceDescriptor(_subscribedViewModel);
        _cameraSourceHash = GetCurrentOriginalSourceHash(_subscribedViewModel);
        _cameraGroupGeneration++;
        if (_subscribedViewModel is not null)
            VoxelViewport.SetSemanticEditMode(_subscribedViewModel.SemanticEditMode);
        _ = RefreshViewportAsync();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!_detailsSelectionInitialized)
        {
            DetailsTabs.SelectedIndex = ViewModel?.ReviewIssues.Count > 0 ? 3 : 0;
            _detailsSelectionInitialized = true;
        }
        ApplyResponsiveLayout();
        _ = RefreshViewportAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ExitGameScaleReviewIfActive();
        _renderKey = null;
        VoxelViewport.ClearScene();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Ra2VoxelStyleWorkspaceViewModel.SourcePath))
        {
            ExitGameScaleReviewIfActive();
            UpdateCameraSourceIdentity();
        }
        if (e.PropertyName is nameof(Ra2VoxelStyleWorkspaceViewModel.CanUseRev7DiagnosticPreviews) &&
            ViewModel?.CanUseRev7DiagnosticPreviews != true)
            ExitGameScaleReviewIfActive();
        if (e.PropertyName is nameof(Ra2VoxelStyleWorkspaceViewModel.SemanticEditMode) && ViewModel is { } modeViewModel)
            VoxelViewport.SetSemanticEditMode(modeViewModel.SemanticEditMode);
        if (e.PropertyName is nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewSnapshot) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewRegionMask) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewComparisonSnapshot) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewProtectionMask) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewSemanticPartition) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewSemanticEvidence) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewSemanticAssignments) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewSemanticComposition) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewFormZones) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewBoundaryIntents) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewFeatureScale) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewRiskComposition) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewRiskCandidate) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.CurrentPreviewQuality) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.SemanticReviewDimension) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.IsThreeDimensionalPreview) or
            nameof(Ra2VoxelStyleWorkspaceViewModel.PreviewMode))
        {
            _ = RefreshViewportAsync();
        }
    }

    private async Task RefreshViewportAsync()
    {
        if (!IsLoaded)
        {
            return;
        }
        if (ViewModel is not { IsThreeDimensionalPreview: true } viewModel ||
            viewModel.CurrentPreviewSnapshot is not { } snapshot)
        {
            _renderKey = null;
            VoxelViewport.CancelSceneBuild();
            return;
        }

        Ra2VoxelViewportColourMode colourMode = viewModel.IsDifferenceMode || viewModel.IsSymmetryMode
            ? Ra2VoxelViewportColourMode.Difference
            : viewModel.IsStructureRegionsMode
                ? Ra2VoxelViewportColourMode.SemanticStructure
            : viewModel.IsRegionMaskMode
                ? Ra2VoxelViewportColourMode.GeometryRegion
            : viewModel.IsSemanticsMode
                ? Ra2VoxelViewportColourMode.SemanticMask
            : viewModel.IsFormZonesMode
                ? Ra2VoxelViewportColourMode.FormZone
            : viewModel.IsBoundaryIntentMode
                ? Ra2VoxelViewportColourMode.BoundaryIntent
            : viewModel.IsRiskOverlayMode
                ? Ra2VoxelViewportColourMode.RiskOverlay
                : Ra2VoxelViewportColourMode.Palette;
        string key = $"{snapshot.CanonicalHash}:{viewModel.CurrentPreviewRegionMask?.MaskHash}:" +
            $"{viewModel.CurrentPreviewComparisonSnapshot?.CanonicalHash}:{viewModel.CurrentPreviewProtectionMask?.MaskHash}:{colourMode}";
        key += $":{viewModel.CurrentPreviewSemanticPartition?.PartitionHash}";
        key += $":{viewModel.CurrentPreviewSemanticEvidence?.PackageHash}:" +
               string.Join(',', viewModel.CurrentPreviewSemanticAssignments?.Select(value => $"{value.RegionId}-{value.PartRole}-{value.MaterialRole}-{value.RemapIntent}") ?? []);
        key += $":{viewModel.CurrentPreviewSemanticComposition?.CompositionHash}";
        key += $":{viewModel.CurrentPreviewFormZones?.ProjectionHash}:{viewModel.CurrentPreviewBoundaryIntents?.ProjectionHash}:" +
               $"{viewModel.CurrentPreviewFeatureScale?.ProjectionHash}:{viewModel.CurrentPreviewRiskCandidate?.CanonicalHash}:" +
               $"{viewModel.CurrentPreviewQuality?.ReportHash}";
        key += $":{viewModel.SemanticReviewDimension}";
        if (string.Equals(_renderKey, key, StringComparison.Ordinal))
            return;
        _renderKey = key;
        await VoxelViewport.SetSceneAsync(
            snapshot,
            viewModel.CurrentPreviewRegionMask,
            colourMode,
            viewModel.CurrentPreviewComparisonSnapshot,
            viewModel.CurrentPreviewProtectionMask,
            viewModel.CurrentPreviewSemanticPartition,
            viewModel.CurrentPreviewSemanticEvidence,
            viewModel.CurrentPreviewSemanticAssignments,
            viewModel.CurrentPreviewSemanticComposition,
            viewModel.SemanticReviewDimension,
            viewModel.CurrentPreviewFormZones,
            viewModel.CurrentPreviewBoundaryIntents,
            viewModel.CurrentPreviewFeatureScale,
            viewModel.CurrentPreviewRiskCandidate,
            viewModel.CurrentPreviewRiskComposition,
            viewModel.CurrentPreviewQuality,
            $"{_cameraGroupGeneration}");
    }

    private void OnSceneBuildFailed(object? sender, string message)
    {
        _renderKey = null;
        ViewModel?.ReportViewportFallback(message);
    }

    private void OnSemanticCellSelected(object? sender, Ra2VoxelSemanticCellHit hit)
    {
        if (ViewModel?.HandleSemanticCellClick(hit.RegionId, hit.Coordinate) == true)
            DetailsTabs.SelectedIndex = 1;
    }

    private void OnSemanticCellHitFailed(object? sender, Ra2VoxelSemanticHitFailure failure) =>
        ViewModel?.ReportSemanticPointerFeedback(
            failure.Message,
            failure.Kind is Ra2VoxelSemanticHitFailureKind.SceneMismatch or Ra2VoxelSemanticHitFailureKind.RegionUnavailable);

    private void OnSemanticStrokeStarting(object? sender, Ra2VoxelSemanticStrokeStartingEventArgs e)
    {
        e.IsAccepted = ViewModel?.BeginSemanticStroke(e.Hit.RegionId, e.Hit.Coordinate) == true;
        if (e.IsAccepted)
            DetailsTabs.SelectedIndex = 1;
    }

    private void OnSemanticStrokeProgress(object? sender, Ra2VoxelSemanticStrokeProgressEventArgs e) =>
        ViewModel?.ReportSemanticStrokeProgress(e.SeedCount);

    private void OnSemanticStrokeCompleted(object? sender, Ra2VoxelSemanticStrokeCompletedEventArgs e) =>
        ViewModel?.CommitSemanticStroke(e.Seeds);

    private void OnSemanticStrokeCanceled(object? sender, Ra2VoxelSemanticStrokeCanceledEventArgs e) =>
        ViewModel?.CancelSemanticStroke(e.Message);

    private void OnWorkspaceSizeChanged(object sender, SizeChangedEventArgs e) => ApplyResponsiveLayout();

    private void InspectorSplitter_OnDragCompleted(object sender, DragCompletedEventArgs e) =>
        _inspectorSizeWasAdjusted = true;

    private void DetailsSplitter_OnDragCompleted(object sender, DragCompletedEventArgs e) =>
        _detailsSizeWasAdjusted = true;

    private void UpdateCameraSourceIdentity()
    {
        string? descriptor = GetCameraSourceDescriptor(ViewModel);
        string? sourceHash = GetCurrentOriginalSourceHash(ViewModel);
        bool descriptorChanged = !string.Equals(_cameraSourceDescriptor, descriptor, StringComparison.Ordinal);
        bool hashChanged = sourceHash is not null &&
            !string.Equals(_cameraSourceHash, sourceHash, StringComparison.Ordinal);
        if (!descriptorChanged && !hashChanged)
            return;
        _cameraSourceDescriptor = descriptor;
        _cameraSourceHash = sourceHash;
        _cameraGroupGeneration++;
    }

    private static string? GetCameraSourceDescriptor(Ra2VoxelStyleWorkspaceViewModel? viewModel) =>
        viewModel is null ? null : $"{viewModel.SourcePath}|{viewModel.SourceName}|{viewModel.SourceFacts}";

    private static string? GetCurrentOriginalSourceHash(Ra2VoxelStyleWorkspaceViewModel? viewModel) =>
        viewModel?.PreviewMode == Ra2VoxelStylePreviewMode.Original
            ? viewModel.CurrentPreviewSnapshot?.CanonicalHash
            : null;

    private void ApplyResponsiveLayout()
    {
        if (!_inspectorSizeWasAdjusted && ActualWidth > 0d)
        {
            InspectorColumn.Width = new GridLength(ActualWidth >= 1180d
                ? 312d
                : ActualWidth >= 900d
                    ? 280d
                    : 260d);
        }
        if (!_detailsSizeWasAdjusted && ActualHeight > 0d)
            DetailsRow.Height = new GridLength(ActualHeight >= 700d ? 240d : 160d);
    }
}
