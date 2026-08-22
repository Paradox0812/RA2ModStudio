using System.Windows;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.Views;

public partial class FieldLearningWizardWindow : Window
{
    private readonly FieldRegistryHarvestPreviewViewModel _viewModel;
    private readonly Func<FieldRegistryCurrentIniSource?>? _currentIniSourceAccessor;

    internal FieldLearningWizardWindow(
        FieldRegistryHarvestPreviewViewModel viewModel,
        Func<FieldRegistryCurrentIniSource?>? currentIniSourceAccessor = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _currentIniSourceAccessor = currentIniSourceAccessor;
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void UseCurrentIni(object sender, RoutedEventArgs e)
    {
        FieldRegistryCurrentIniSource? source = _currentIniSourceAccessor?.Invoke();
        if (source is null)
        {
            _viewModel.LoadCurrentIniHarvestPreview("current.ini", string.Empty);
            return;
        }

        _viewModel.RawText = source.Text;
        _viewModel.LoadCurrentIniHarvestPreview(source.SourceName, source.Text);
    }

    private void ParsePastedText(object sender, RoutedEventArgs e)
        => _viewModel.ParseAndPreview();

    private void BuildApplyPlan(object sender, RoutedEventArgs e)
        => _viewModel.BuildApplyPlan();

    private void EditAllowedValues(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not FieldRegistryIniDraftRowViewModel row)
            return;

        if (row.ValueKindValue is not (Ra2FieldValueKind.Enum or Ra2FieldValueKind.EnumList or Ra2FieldValueKind.Boolean))
        {
            MessageBox.Show(
                this,
                "当前行不是 enum/list/boolean 类型，不需要编辑允许值。",
                "编辑允许值",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        AllowedValuesEditorWindow window = new(
            row.Key,
            row.EditorKindValue,
            row.ValueKindValue,
            row.AllowedValuesText,
            row.ScannedAllowedValuesText)
        {
            Owner = this
        };
        if (window.ShowDialog() == true)
        {
            row.AllowedValuesText = window.ResultText;
            _viewModel.BuildApplyPlan();
        }
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        => Close();

    private void ApplyCurrentPlan(object sender, RoutedEventArgs e)
    {
        FieldRegistryApplyConfirmationViewModel? confirmation = _viewModel.CreateApplyConfirmation();
        if (confirmation is null)
            return;

        MessageBoxResult result = MessageBox.Show(
            this,
            confirmation.Message,
            confirmation.Title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
            _viewModel.ApplyConfirmed();
    }
}
