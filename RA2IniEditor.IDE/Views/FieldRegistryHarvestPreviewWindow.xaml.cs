using System.Windows;
using Microsoft.Win32;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.Views;

public partial class FieldRegistryHarvestPreviewWindow : Window
{
    private readonly Func<FieldRegistryCurrentIniSource?>? _currentIniSourceAccessor;

    public FieldRegistryHarvestPreviewWindow()
    {
        InitializeComponent();
        DataContext = new FieldRegistryHarvestPreviewViewModel();
        RefreshRemoteSources();
    }

    internal FieldRegistryHarvestPreviewWindow(Func<IFieldRegistryProvenanceProvider> provenanceProviderAccessor)
        : this(
            provenanceProviderAccessor,
            () => null,
            CreateDefaultGlobalFieldRegistryRootPath,
            null)
    {
    }

    internal FieldRegistryHarvestPreviewWindow(
        Func<IFieldRegistryProvenanceProvider> provenanceProviderAccessor,
        Func<string?> projectRootPathAccessor,
        Func<string> globalFieldRegistryRootPathAccessor,
        Action? reloadAfterApply,
        Func<FieldRegistryCurrentIniSource?>? currentIniSourceAccessor = null)
    {
        InitializeComponent();
        _currentIniSourceAccessor = currentIniSourceAccessor;
        DataContext = new FieldRegistryHarvestPreviewViewModel(
            new MarkdownFieldRegistryHarvestParser(),
            new FieldRegistryHarvestNormalizer(),
            new FieldRegistryHarvestPreviewBuilder(),
            new FieldRegistryHarvestDiffService(),
            provenanceProviderAccessor,
            new FieldRegistryApplyPlanBuilder(),
            new FieldRegistryApplyWriter(),
            projectRootPathAccessor,
            globalFieldRegistryRootPathAccessor,
            reloadAfterApply);
        RefreshRemoteSources();
    }

    private void InsertSample(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            viewModel.InsertSample();
    }

    private void ParseAndPreview(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            viewModel.ParseAndPreview();
    }

    private void UseCurrentIni(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldRegistryHarvestPreviewViewModel viewModel)
            return;

        FieldRegistryCurrentIniSource? source = _currentIniSourceAccessor?.Invoke();
        if (source is null)
        {
            viewModel.LoadCurrentIniHarvestPreview("current.ini", string.Empty);
            return;
        }

        viewModel.LoadCurrentIniHarvestPreview(source.SourceName, source.Text);
    }

    private void ClearPreview(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            viewModel.Clear();
    }

    private async void FetchRawText(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            await viewModel.FetchRawTextAsync();
    }

    private void CancelFetch(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            viewModel.CancelFetch();
    }

    private void RefreshRemoteHistory(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            viewModel.RefreshRemoteHistory();
    }

    private void UseCachedText(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            viewModel.UseCachedTextFromHistory();
    }

    private async void RefetchSelected(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            await viewModel.RefetchSelectedRemoteHistoryAsync();
    }

    private void ClearRemoteHistory(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldRegistryHarvestPreviewViewModel viewModel)
            return;

        MessageBoxResult result = MessageBox.Show(
            this,
            "Clear local remote source history? RawText and active field packs will not be changed.",
            "Clear Remote Source History",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        viewModel.ClearRemoteHistory(result == MessageBoxResult.Yes);
    }

    private void RefreshRemotePresets(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            viewModel.RefreshRemotePresets();
    }

    private void UsePresetUrl(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            viewModel.UsePresetUrl();
    }

    private async void FetchSelectedPreset(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            await viewModel.FetchSelectedPresetAsync();
    }

    private void AddPreset(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldRegistryHarvestPreviewViewModel viewModel)
            return;

        RemoteSourcePresetEditorWindow dialog = new(new FieldRegistryRemoteSourcePresetEditModel(
            null,
            string.IsNullOrWhiteSpace(viewModel.SourceName) ? "Remote source" : viewModel.SourceName,
            viewModel.FetchUrl,
            string.Empty,
            string.Empty,
            true))
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true && dialog.EditModel is not null)
            viewModel.AddPreset(dialog.EditModel);
    }

    private void EditPreset(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldRegistryHarvestPreviewViewModel { SelectedRemotePreset: not null } viewModel)
            return;

        FieldRegistryRemoteSourcePresetViewModel preset = viewModel.SelectedRemotePreset;
        RemoteSourcePresetEditorWindow dialog = new(new FieldRegistryRemoteSourcePresetEditModel(
            preset.Id,
            preset.Name,
            preset.Url,
            preset.Description,
            preset.TagsText,
            preset.IsEnabled))
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true && dialog.EditModel is not null)
            viewModel.EditSelectedPreset(dialog.EditModel);
    }

    private void RemovePreset(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldRegistryHarvestPreviewViewModel viewModel)
            return;

        MessageBoxResult result = MessageBox.Show(
            this,
            "Remove selected remote source preset? RawText, history, and active field packs will not be changed.",
            "Remove Remote Source Preset",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        viewModel.RemoveSelectedPreset(result == MessageBoxResult.Yes);
    }

    private void ImportPresets(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldRegistryHarvestPreviewViewModel viewModel)
            return;

        OpenFileDialog dialog = new()
        {
            Title = "Import Remote Source Presets",
            Filter = "Preset JSON (*.json)|*.json|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == true)
            viewModel.ImportPresets(dialog.FileName, replaceExisting: false);
    }

    private void ExportPresets(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldRegistryHarvestPreviewViewModel viewModel)
            return;

        SaveFileDialog dialog = new()
        {
            Title = "Export Remote Source Presets",
            Filter = "Preset JSON (*.json)|*.json|All files (*.*)|*.*",
            FileName = "remote-source-presets.json"
        };

        if (dialog.ShowDialog(this) == true)
            viewModel.ExportPresets(dialog.FileName);
    }

    private void BuildApplyPlan(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryHarvestPreviewViewModel viewModel)
            viewModel.BuildApplyPlan();
    }

    private void ApplyCurrentPlan(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldRegistryHarvestPreviewViewModel viewModel)
            return;

        FieldRegistryApplyConfirmationViewModel? confirmation = viewModel.CreateApplyConfirmation();
        if (confirmation is null)
            return;

        MessageBoxResult result = MessageBox.Show(
            this,
            confirmation.Message,
            confirmation.Title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes)
            return;

        viewModel.ApplyConfirmed();
    }

    private static string CreateDefaultGlobalFieldRegistryRootPath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return System.IO.Path.Combine(appData, "RA2IniEditor", "FieldRegistry");
    }

    private void RefreshRemoteSources()
    {
        if (DataContext is not FieldRegistryHarvestPreviewViewModel viewModel)
            return;

        viewModel.RefreshRemoteHistory();
        viewModel.RefreshRemotePresets();
    }
}

internal sealed class FieldRegistryCurrentIniSource
{
    public FieldRegistryCurrentIniSource(string sourceName, string text)
    {
        SourceName = string.IsNullOrWhiteSpace(sourceName) ? "current.ini" : sourceName.Trim();
        Text = text ?? string.Empty;
    }

    public string SourceName { get; }

    public string Text { get; }
}
