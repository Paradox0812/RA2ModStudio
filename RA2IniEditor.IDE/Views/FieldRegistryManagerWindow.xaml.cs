using System.Windows;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;

namespace RA2IniEditor.IDE.Views;

public partial class FieldRegistryManagerWindow : Window
{
    public FieldRegistryManagerWindow()
    {
        InitializeComponent();
    }

    public event EventHandler? ReloadLocalFieldRegistryRequested;

    public event EventHandler? HarvestPreviewRequested;

    public event EventHandler? RelearnCurrentIniRequested;

    public event EventHandler? CleanupApplied;

    public event EventHandler? OpenGlobalRegistryFolderRequested;

    public event EventHandler? OpenProjectRegistryFolderRequested;

    public event EventHandler? RefreshRollbackManifestsRequested;

    internal event EventHandler<string>? OpenRollbackTargetFolderRequested;

    internal event EventHandler<string>? OpenRollbackManifestFolderRequested;

    internal event EventHandler<string>? OpenRollbackBackupFolderRequested;

    internal event EventHandler<FieldRegistryRollbackResult>? RollbackCompleted;

    private void ReloadLocalFieldRegistry(object sender, RoutedEventArgs e)
        => ReloadLocalFieldRegistryRequested?.Invoke(this, EventArgs.Empty);

    private void OpenHarvestPreview(object sender, RoutedEventArgs e)
        => HarvestPreviewRequested?.Invoke(this, EventArgs.Empty);

    private void BuildCleanupPlan(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryManagerViewModel viewModel)
            viewModel.BuildGeneralizationCleanupPlan();
    }

    private void ApplyCleanupPlan(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldRegistryManagerViewModel viewModel)
            return;

        MessageBoxResult result = MessageBox.Show(
            this,
            "这会更新默认 active 字段包并创建回滚备份。是否继续？",
            "应用字段库清理",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes)
            return;

        viewModel.ApplyGeneralizationCleanupPlan();
        CleanupApplied?.Invoke(this, EventArgs.Empty);
    }

    private void RelearnCurrentIni(object sender, RoutedEventArgs e)
        => RelearnCurrentIniRequested?.Invoke(this, EventArgs.Empty);

    private void OpenGlobalRegistryFolder(object sender, RoutedEventArgs e)
        => OpenGlobalRegistryFolderRequested?.Invoke(this, EventArgs.Empty);

    private void OpenProjectRegistryFolder(object sender, RoutedEventArgs e)
        => OpenProjectRegistryFolderRequested?.Invoke(this, EventArgs.Empty);

    private void RefreshRollbackManifests(object sender, RoutedEventArgs e)
        => RefreshRollbackManifestsRequested?.Invoke(this, EventArgs.Empty);

    private void OpenRollbackTargetFolder(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryManagerViewModel viewModel &&
            viewModel.TryGetSelectedTargetFolderPath() is { } directoryPath)
        {
            OpenRollbackTargetFolderRequested?.Invoke(this, directoryPath);
        }
    }

    private void OpenRollbackManifestFolder(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryManagerViewModel viewModel &&
            viewModel.TryGetSelectedManifestFolderPath() is { } directoryPath)
        {
            OpenRollbackManifestFolderRequested?.Invoke(this, directoryPath);
        }
    }

    private void OpenRollbackBackupFolder(object sender, RoutedEventArgs e)
    {
        if (DataContext is FieldRegistryManagerViewModel viewModel &&
            viewModel.TryGetSelectedBackupFolderPath() is { } directoryPath)
        {
            OpenRollbackBackupFolderRequested?.Invoke(this, directoryPath);
        }
    }

    private void RollbackSelected(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldRegistryManagerViewModel viewModel)
            return;

        FieldRegistryRollbackConfirmationViewModel? confirmation = viewModel.CreateRollbackConfirmation();
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

        FieldRegistryRollbackResult? rollbackResult = viewModel.RollbackSelectedConfirmed();
        if (rollbackResult is not null)
            RollbackCompleted?.Invoke(this, rollbackResult);
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        => Close();
}
