using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.IDE.ViewModels.FieldDetails;
using RA2IniEditor.IDE.ViewModels.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.Views;

public partial class FieldRegistryCenterWindow : Window, INotifyPropertyChanged
{
    private readonly List<FieldRegistryCenterFieldRow> _allFieldRows = [];
    private FieldEditorSaveContext _fieldEditorSaveContext;
    private string _fieldCountText = "0 条有效映射";
    private FieldEditorWindow? _fieldEditorWindow;

    internal FieldRegistryCenterWindow(
        FieldRegistryManagerViewModel manager,
        IRa2FieldDefinitionProvider fieldProvider,
        IFieldRegistryProvenanceProvider provenanceProvider,
        string? projectRootPath,
        string globalFieldRegistryRootPath)
    {
        Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _fieldEditorSaveContext = new FieldEditorSaveContext(
            fieldProvider ?? throw new ArgumentNullException(nameof(fieldProvider)),
            provenanceProvider ?? throw new ArgumentNullException(nameof(provenanceProvider)),
            projectRootPath,
            globalFieldRegistryRootPath);
        InitializeComponent();
        DataContext = this;
        RefreshFieldRows(fieldProvider);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? ReloadLocalFieldRegistryRequested;

    public event EventHandler? FieldLearningRequested;

    public event EventHandler? AdvancedToolsRequested;

    public object Manager { get; }

    public ObservableCollection<object> FieldRows { get; } = [];

    public string FieldCountText
    {
        get => _fieldCountText;
        private set
        {
            if (_fieldCountText == value)
                return;

            _fieldCountText = value;
            OnPropertyChanged();
        }
    }

    public void RefreshFieldRows(IRa2FieldDefinitionProvider fieldProvider)
    {
        ArgumentNullException.ThrowIfNull(fieldProvider);
        _allFieldRows.Clear();
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

        foreach (Ra2SectionKind sectionKind in Enum.GetValues<Ra2SectionKind>())
        {
            foreach (Ra2FieldDefinition definition in fieldProvider.GetFields(sectionKind))
            {
                string identity = $"{sectionKind}\u001f{definition.Key}";
                if (!seen.Add(identity))
                    continue;

                _allFieldRows.Add(FieldRegistryCenterFieldRow.FromDefinition(sectionKind, definition));
            }
        }

        ApplyFilter(string.Empty);
    }

    internal void RefreshFieldRegistryContext(
        IRa2FieldDefinitionProvider fieldProvider,
        IFieldRegistryProvenanceProvider provenanceProvider,
        string? projectRootPath,
        string globalFieldRegistryRootPath)
    {
        _fieldEditorSaveContext = new FieldEditorSaveContext(
            fieldProvider,
            provenanceProvider,
            projectRootPath,
            globalFieldRegistryRootPath);
        RefreshFieldRows(fieldProvider);
    }

    private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        => ApplyFilter((sender as TextBox)?.Text ?? string.Empty);

    private void ApplyFilter(string filterText)
    {
        FieldRows.Clear();
        IEnumerable<FieldRegistryCenterFieldRow> rows = _allFieldRows;
        if (!string.IsNullOrWhiteSpace(filterText))
            rows = rows.Where(row => row.Matches(filterText));

        foreach (FieldRegistryCenterFieldRow row in rows
                     .OrderBy(row => row.Key, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(row => row.SectionKind, StringComparer.OrdinalIgnoreCase))
        {
            FieldRows.Add(row);
        }

        FieldCountText = FieldRows.Count == _allFieldRows.Count
            ? $"{FieldRows.Count} 条有效映射"
            : $"显示 {FieldRows.Count} / {_allFieldRows.Count} 条有效映射";
    }

    private void ReloadLocalFieldRegistry(object sender, RoutedEventArgs e)
        => ReloadLocalFieldRegistryRequested?.Invoke(this, EventArgs.Empty);

    private void OpenFieldLearning(object sender, RoutedEventArgs e)
        => FieldLearningRequested?.Invoke(this, EventArgs.Empty);

    private void CreateNewField(object sender, RoutedEventArgs e)
        => OpenFieldEditor(null);

    private void EditSelectedField(object sender, RoutedEventArgs e)
        => OpenFieldEditor(FieldsGrid.SelectedItem as FieldRegistryCenterFieldRow);

    private void FieldsGrid_OnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => OpenFieldEditor(FieldsGrid.SelectedItem as FieldRegistryCenterFieldRow);

    private void OpenFieldEditor(FieldRegistryCenterFieldRow? row)
    {
        if (_fieldEditorWindow is { IsVisible: true })
        {
            _fieldEditorWindow.Activate();
            return;
        }

        _fieldEditorWindow = row is null
            ? new FieldEditorWindow(_fieldEditorSaveContext)
            : new FieldEditorWindow(row.Definition, row.SectionKindValue, _fieldEditorSaveContext);
        _fieldEditorWindow.Owner = this;
        _fieldEditorWindow.FieldRegistrySaveApplied += FieldEditorWindow_OnFieldRegistrySaveApplied;
        _fieldEditorWindow.Closed += FieldEditorWindow_OnClosed;
        _fieldEditorWindow.Show();
    }

    private void FieldEditorWindow_OnFieldRegistrySaveApplied(object? sender, FieldEditorSaveApplyResult e)
        => ReloadLocalFieldRegistryRequested?.Invoke(this, EventArgs.Empty);

    private void FieldEditorWindow_OnClosed(object? sender, EventArgs e)
    {
        if (_fieldEditorWindow is not null)
        {
            _fieldEditorWindow.FieldRegistrySaveApplied -= FieldEditorWindow_OnFieldRegistrySaveApplied;
            _fieldEditorWindow.Closed -= FieldEditorWindow_OnClosed;
        }

        _fieldEditorWindow = null;
    }

    private void OpenAdvancedTools(object sender, RoutedEventArgs e)
        => AdvancedToolsRequested?.Invoke(this, EventArgs.Empty);

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        => Close();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

internal sealed class FieldRegistryCenterFieldRow
{
    private FieldRegistryCenterFieldRow(
        Ra2FieldDefinition definition,
        Ra2SectionKind sectionKindValue,
        string key,
        string sectionKind,
        string editorKind,
        string valueKind,
        string sourceKind,
        string description)
    {
        Definition = definition;
        SectionKindValue = sectionKindValue;
        Details = Ra2FieldDetailsViewModel.FromDefinition(definition, sectionKindValue);
        Key = key;
        SectionKind = sectionKind;
        EditorKind = editorKind;
        ValueKind = valueKind;
        SourceKind = sourceKind;
        Description = description;
    }

    public string Key { get; }

    public Ra2FieldDefinition Definition { get; }

    public Ra2SectionKind SectionKindValue { get; }

    public Ra2FieldDetailsViewModel Details { get; }

    public string SectionKind { get; }

    public string EditorKind { get; }

    public string ValueKind { get; }

    public string SourceKind { get; }

    public string Description { get; }

    public static FieldRegistryCenterFieldRow FromDefinition(
        Ra2SectionKind sectionKind,
        Ra2FieldDefinition definition)
        => new(
            definition,
            sectionKind,
            definition.Key,
            sectionKind.ToString(),
            definition.EditorKind.ToString(),
            definition.ValueMetadata.ValueKind.ToString(),
            definition.SourceKind.ToString(),
            definition.Description ?? string.Empty);

    public bool Matches(string filterText)
        => Contains(Key, filterText) ||
           Contains(SectionKind, filterText) ||
           Contains(EditorKind, filterText) ||
           Contains(ValueKind, filterText) ||
           Contains(SourceKind, filterText) ||
           Contains(Description, filterText);

    private static bool Contains(string text, string filterText)
        => text.Contains(filterText, StringComparison.OrdinalIgnoreCase);
}
