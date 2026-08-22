using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Views;

public partial class AllowedValuesEditorWindow : Window
{
    private readonly AllowedValuesEditorViewModel _viewModel;

    internal AllowedValuesEditorWindow(
        string key,
        FieldEditorKind editorKind,
        Ra2FieldValueKind valueKind,
        string allowedValuesText,
        string scannedAllowedValuesText)
    {
        InitializeComponent();
        _viewModel = new AllowedValuesEditorViewModel(
            key,
            editorKind,
            valueKind,
            allowedValuesText,
            scannedAllowedValuesText);
        DataContext = _viewModel;
        TitleTextBlock.Text = $"编辑可选值：{key}";
    }

    public string ResultText { get; private set; } = string.Empty;

    private void AddRow(object sender, RoutedEventArgs e)
        => _viewModel.Rows.Add(new AllowedValueEditorRow());

    private void RemoveSelectedRow(object sender, RoutedEventArgs e)
    {
        if (AllowedValuesGrid.SelectedItem is AllowedValueEditorRow row)
            _viewModel.Rows.Remove(row);
    }

    private void DedupeRows(object sender, RoutedEventArgs e)
        => _viewModel.Dedupe();

    private void SortRows(object sender, RoutedEventArgs e)
        => _viewModel.Sort();

    private void AppendBuiltInValues(object sender, RoutedEventArgs e)
        => _viewModel.AppendMissingBuiltInValues();

    private void RestoreScannedValues(object sender, RoutedEventArgs e)
        => _viewModel.RestoreScannedValues();

    private void Accept(object sender, RoutedEventArgs e)
    {
        ResultText = _viewModel.ToAllowedValuesText();
        DialogResult = true;
    }

    private void Cancel(object sender, RoutedEventArgs e)
        => DialogResult = false;

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        => DialogResult = false;

    private sealed class AllowedValuesEditorViewModel
    {
        private readonly string _key;
        private readonly FieldEditorKind _editorKind;
        private readonly Ra2FieldValueKind _valueKind;
        private readonly string _scannedAllowedValuesText;
        private readonly BuiltInRa2FieldValueCompletionCatalog _builtInCatalog = new();

        public AllowedValuesEditorViewModel(
            string key,
            FieldEditorKind editorKind,
            Ra2FieldValueKind valueKind,
            string text,
            string scannedAllowedValuesText)
        {
            _key = key ?? string.Empty;
            _editorKind = editorKind;
            _valueKind = valueKind;
            _scannedAllowedValuesText = scannedAllowedValuesText ?? string.Empty;
            Reset(Parse(text));
        }

        public ObservableCollection<AllowedValueEditorRow> Rows { get; } = new();

        public void AppendMissingBuiltInValues()
        {
            Ra2FieldDefinition definition = new(
                _key,
                [Ra2SectionKind.Unknown],
                _editorKind,
                Ra2FieldSourceKind.BuiltIn,
                valueMetadata: new Ra2FieldValueMetadata(_valueKind));
            IReadOnlyList<string> existingValues = Rows
                .Select(row => row.Value.Trim())
                .Where(value => value.Length > 0)
                .ToArray();
            Ra2FieldValueCompletionRequest request = new(
                Ra2SectionKind.Unknown,
                _key,
                definition,
                new Ra2ValueCompletionContext(
                    string.Empty,
                    string.Empty,
                    _valueKind is Ra2FieldValueKind.EnumList or Ra2FieldValueKind.ReferenceList,
                    existingValues));
            HashSet<string> existing = new(existingValues, StringComparer.OrdinalIgnoreCase);

            foreach (Ra2FieldValueCompletionCandidate candidate in _builtInCatalog.GetCandidates(request))
            {
                if (!existing.Add(candidate.Value))
                    continue;

                Rows.Add(new AllowedValueEditorRow
                {
                    Value = candidate.Value
                });
            }

            Sort();
        }

        public void Dedupe()
        {
            AllowedValueEditorRow[] rows = Rows
                .Where(row => !string.IsNullOrWhiteSpace(row.Value))
                .GroupBy(row => row.Value.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToArray();
            Reset(rows);
        }

        public void RestoreScannedValues()
            => Reset(Parse(_scannedAllowedValuesText));

        public void Sort()
        {
            AllowedValueEditorRow[] rows = Rows
                .Where(row => !string.IsNullOrWhiteSpace(row.Value))
                .OrderBy(row => row.Value.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToArray();
            Reset(rows);
        }

        public string ToAllowedValuesText()
        {
            return string.Join(Environment.NewLine, Rows
                .Select(row => row.ToAllowedValueText())
                .Where(line => line.Length > 0));
        }

        private void Reset(IEnumerable<AllowedValueEditorRow> rows)
        {
            Rows.Clear();
            foreach (AllowedValueEditorRow row in rows)
                Rows.Add(row);
        }

        private static IEnumerable<AllowedValueEditorRow> Parse(string text)
        {
            char[] separators = ['\r', '\n', ';'];
            foreach (string rawEntry in (text ?? string.Empty).Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                string entry = rawEntry.Trim();
                if (entry.Length == 0)
                    continue;

                string[] parts = entry.Split('|', 3, StringSplitOptions.TrimEntries);
                yield return new AllowedValueEditorRow
                {
                    Value = parts.ElementAtOrDefault(0) ?? string.Empty,
                    DisplayName = parts.ElementAtOrDefault(1) ?? string.Empty,
                    Description = parts.ElementAtOrDefault(2) ?? string.Empty
                };
            }
        }
    }

    private sealed class AllowedValueEditorRow : INotifyPropertyChanged
    {
        private string _value = string.Empty;
        private string _displayName = string.Empty;
        private string _description = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string ToAllowedValueText()
        {
            string value = Value.Trim();
            if (value.Length == 0)
                return string.Empty;

            string displayName = DisplayName.Trim();
            string description = Description.Trim();
            if (description.Length > 0)
                return $"{value}|{displayName}|{description}";

            return displayName.Length > 0 ? $"{value}|{displayName}" : value;
        }

        private void SetProperty(ref string field, string value, [CallerMemberName] string? propertyName = null)
        {
            value ??= string.Empty;
            if (string.Equals(field, value, StringComparison.Ordinal))
                return;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
