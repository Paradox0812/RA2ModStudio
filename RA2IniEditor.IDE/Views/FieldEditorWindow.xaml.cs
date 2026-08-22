using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels.FieldRegistry;

namespace RA2IniEditor.IDE.Views;

public partial class FieldEditorWindow : Window
{
    private readonly FieldEditorSaveContext _saveContext;

    public FieldEditorWindow()
        : this(new FieldEditorViewModel(), CreateStandaloneSaveContext())
    {
    }

    internal FieldEditorWindow(FieldEditorSaveContext saveContext)
        : this(new FieldEditorViewModel(), saveContext)
    {
    }

    internal FieldEditorWindow(
        Ra2FieldDefinition definition,
        Ra2SectionKind sectionKind,
        FieldEditorSaveContext saveContext)
        : this(new FieldEditorViewModel(definition, sectionKind), saveContext)
    {
    }

    internal event EventHandler<FieldEditorSaveApplyResult>? FieldRegistrySaveApplied;

    private FieldEditorWindow(FieldEditorViewModel viewModel, FieldEditorSaveContext saveContext)
    {
        _saveContext = saveContext ?? throw new ArgumentNullException(nameof(saveContext));
        InitializeComponent();
        DataContext = viewModel;
    }

    private void BuildProjectPreview(object sender, RoutedEventArgs e)
        => BuildPreview(FieldEditorSaveTarget.Project);

    private void BuildGlobalPreview(object sender, RoutedEventArgs e)
        => BuildPreview(FieldEditorSaveTarget.Global);

    private void BuildPreview(FieldEditorSaveTarget target)
    {
        if (DataContext is FieldEditorViewModel viewModel)
            viewModel.BuildSavePreview(_saveContext.EffectiveProvider, target);
    }

    private void ApplyProjectSave(object sender, RoutedEventArgs e)
        => ApplySave(FieldEditorSaveTarget.Project);

    private void ApplyGlobalSave(object sender, RoutedEventArgs e)
        => ApplySave(FieldEditorSaveTarget.Global);

    private void ApplySave(FieldEditorSaveTarget target)
    {
        if (DataContext is not FieldEditorViewModel viewModel)
            return;

        FieldEditorSaveApplyResult result = viewModel.ApplySave(_saveContext, target);
        if (result.Success)
            FieldRegistrySaveApplied?.Invoke(this, result);
    }

    private void CopyTargetPath(object sender, RoutedEventArgs e)
        => CopyPath(GetViewModel()?.LastApplyTargetFilePath);

    private void CopyManifestPath(object sender, RoutedEventArgs e)
        => CopyPath(GetViewModel()?.LastApplyManifestFilePath);

    private void CopyPersistedPreview(object sender, RoutedEventArgs e)
    {
        FieldEditorViewModel? viewModel = GetViewModel();
        if (viewModel?.SavePreview is null)
            return;

        CopyText(viewModel.PersistedJsonPreview, "字段 JSON 预览");
    }

    private void OpenTargetFolder(object sender, RoutedEventArgs e)
        => OpenContainingFolder(GetViewModel()?.LastApplyTargetFilePath);

    private void OpenManifestFolder(object sender, RoutedEventArgs e)
        => OpenContainingFolder(GetViewModel()?.LastApplyManifestFilePath);

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        => Close();

    private FieldEditorViewModel? GetViewModel()
        => DataContext as FieldEditorViewModel;

    private static void CopyPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        CopyText(path, "路径");
    }

    private static void CopyText(string? text, string label)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception ex) when (ex is ExternalException or InvalidOperationException)
        {
            MessageBox.Show($"复制{label}失败：{ex.Message}", "字段库", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static void OpenContainingFolder(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            string? directory = Directory.Exists(path)
                ? path
                : Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                MessageBox.Show("目录不存在，无法打开。", "字段库", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string arguments = File.Exists(path)
                ? $"/select,\"{path}\""
                : $"\"{directory}\"";
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = arguments,
                UseShellExecute = true
            });
        }
        catch (Exception ex) when (ex is Win32Exception or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"打开目录失败：{ex.Message}", "字段库", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static FieldEditorSaveContext CreateStandaloneSaveContext()
    {
        BuiltInRa2FieldDefinitionProvider provider = new();
        return new FieldEditorSaveContext(
            provider,
            new RA2IniEditor.Infrastructure.FieldRegistry.Provenance.FieldRegistryProvenanceSnapshot([], [], provider),
            null,
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RA2IniEditor",
                "FieldRegistry"));
    }
}
