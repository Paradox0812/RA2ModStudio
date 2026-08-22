using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Services.FieldRegistry;

namespace RA2IniEditor.IDE.ViewModels.FieldRegistry;

internal sealed class FieldEditorViewModel : INotifyPropertyChanged
{
    private readonly IFieldEditorDraftFactory _draftFactory;
    private readonly IFieldEditorSavePreviewBuilder _previewBuilder;
    private readonly IFieldEditorSaveApplyService _applyService;
    private string _key = string.Empty;
    private Ra2SectionKind _sectionKind = Ra2SectionKind.Unknown;
    private FieldEditorKind _editorKind = FieldEditorKind.Text;
    private Ra2FieldValueKind _valueKind = Ra2FieldValueKind.String;
    private Ra2FieldBooleanValueStyle _booleanStyle = Ra2FieldBooleanValueStyle.Unknown;
    private string _enumName = string.Empty;
    private string _separator = ",";
    private string _allowedValuesText = string.Empty;
    private string _displayName = string.Empty;
    private string _aliasesText = string.Empty;
    private string _description = string.Empty;
    private string _sourceKind = Ra2FieldSourceKind.User.ToString();
    private string _statusText = "新建字段。请先生成保存预览，确认后再写入字段库。";
    private string _lastApplyTargetFilePath = string.Empty;
    private string _lastApplyManifestFilePath = string.Empty;
    private FieldEditorSavePreview? _savePreview;

    public FieldEditorViewModel()
        : this(new FieldEditorDraftFactory(), new FieldEditorSavePreviewBuilder(), new FieldEditorSaveApplyService())
    {
    }

    internal FieldEditorViewModel(
        IFieldEditorDraftFactory draftFactory,
        IFieldEditorSavePreviewBuilder previewBuilder,
        IFieldEditorSaveApplyService applyService)
    {
        _draftFactory = draftFactory ?? throw new ArgumentNullException(nameof(draftFactory));
        _previewBuilder = previewBuilder ?? throw new ArgumentNullException(nameof(previewBuilder));
        _applyService = applyService ?? throw new ArgumentNullException(nameof(applyService));
    }

    public FieldEditorViewModel(Ra2FieldDefinition definition, Ra2SectionKind sectionKind)
        : this()
    {
        ArgumentNullException.ThrowIfNull(definition);

        Key = definition.Key;
        SectionKind = sectionKind;
        EditorKind = definition.EditorKind;
        ValueKind = definition.ValueMetadata.ValueKind;
        BooleanStyle = definition.ValueMetadata.BooleanStyle;
        EnumName = definition.ValueMetadata.EnumName ?? string.Empty;
        Separator = definition.ValueMetadata.Separator;
        AllowedValuesText = FormatAllowedValues(definition.ValueMetadata.AllowedValues);
        DisplayName = definition.DisplayName ?? string.Empty;
        AliasesText = string.Join(", ", definition.Aliases);
        Description = definition.Description ?? string.Empty;
        SourceKind = definition.SourceKind.ToString();
        StatusText = definition.SourceKind == Ra2FieldSourceKind.BuiltIn
            ? "正在查看内置字段。保存预览会生成项目或全局覆盖项，不会修改内置字段库。"
            : "正在查看字段。请先生成保存预览，确认后再写入字段库。";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<Ra2SectionKind> SectionKindOptions { get; } =
        Array.AsReadOnly(Enum.GetValues<Ra2SectionKind>());

    public IReadOnlyList<FieldEditorKind> EditorKindOptions { get; } =
        Array.AsReadOnly(Enum.GetValues<FieldEditorKind>());

    public IReadOnlyList<Ra2FieldValueKind> ValueKindOptions { get; } =
        Array.AsReadOnly(Enum.GetValues<Ra2FieldValueKind>());

    public IReadOnlyList<Ra2FieldBooleanValueStyle> BooleanStyleOptions { get; } =
        Array.AsReadOnly(Enum.GetValues<Ra2FieldBooleanValueStyle>());

    public ObservableCollection<FieldEditorValidationIssue> PreviewIssues { get; } = [];

    public string Key
    {
        get => _key;
        set => SetEditableProperty(ref _key, value);
    }

    public Ra2SectionKind SectionKind
    {
        get => _sectionKind;
        set => SetEditableProperty(ref _sectionKind, value);
    }

    public FieldEditorKind EditorKind
    {
        get => _editorKind;
        set
        {
            if (!SetEditableProperty(ref _editorKind, value))
                return;

            OnPropertyChanged(nameof(IsBooleanStyleEditable));
        }
    }

    public Ra2FieldValueKind ValueKind
    {
        get => _valueKind;
        set
        {
            if (!SetEditableProperty(ref _valueKind, value))
                return;

            OnPropertyChanged(nameof(IsBooleanStyleEditable));
            OnPropertyChanged(nameof(IsSeparatorEditable));
        }
    }

    public Ra2FieldBooleanValueStyle BooleanStyle
    {
        get => _booleanStyle;
        set => SetEditableProperty(ref _booleanStyle, value);
    }

    public string EnumName
    {
        get => _enumName;
        set => SetEditableProperty(ref _enumName, value);
    }

    public string Separator
    {
        get => _separator;
        set => SetEditableProperty(ref _separator, value);
    }

    public bool IsBooleanStyleEditable =>
        ValueKind == Ra2FieldValueKind.Boolean ||
        EditorKind == FieldEditorKind.Boolean;

    public bool IsSeparatorEditable => ValueKind == Ra2FieldValueKind.EnumList;

    public string AllowedValuesText
    {
        get => _allowedValuesText;
        set => SetEditableProperty(ref _allowedValuesText, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetEditableProperty(ref _displayName, value);
    }

    public string AliasesText
    {
        get => _aliasesText;
        set => SetEditableProperty(ref _aliasesText, value);
    }

    public string Description
    {
        get => _description;
        set => SetEditableProperty(ref _description, value);
    }

    public string SourceKind
    {
        get => _sourceKind;
        private set => SetProperty(ref _sourceKind, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string LastApplyTargetFilePath
    {
        get => _lastApplyTargetFilePath;
        private set
        {
            if (!SetProperty(ref _lastApplyTargetFilePath, value))
                return;

            OnPropertyChanged(nameof(HasLastApplyTargetFilePath));
            OnPropertyChanged(nameof(HasLastApplyPaths));
        }
    }

    public string LastApplyManifestFilePath
    {
        get => _lastApplyManifestFilePath;
        private set
        {
            if (!SetProperty(ref _lastApplyManifestFilePath, value))
                return;

            OnPropertyChanged(nameof(HasLastApplyManifestFilePath));
            OnPropertyChanged(nameof(HasLastApplyPaths));
        }
    }

    public bool HasLastApplyTargetFilePath => !string.IsNullOrWhiteSpace(LastApplyTargetFilePath);

    public bool HasLastApplyManifestFilePath => !string.IsNullOrWhiteSpace(LastApplyManifestFilePath);

    public bool HasLastApplyPaths => HasLastApplyTargetFilePath || HasLastApplyManifestFilePath;

    public FieldEditorSavePreview? SavePreview
    {
        get => _savePreview;
        private set
        {
            if (_savePreview == value)
                return;

            _savePreview = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PreviewSummary));
            OnPropertyChanged(nameof(PersistedJsonPreview));
            OnPropertyChanged(nameof(HasPersistedJsonPreview));
            OnPropertyChanged(nameof(CanPreviewSave));
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(PreviewIssueCountText));
        }
    }

    public string PreviewSummary => SavePreview?.Summary ?? "尚未生成保存预览。";

    public string PersistedJsonPreview => SavePreview?.PersistedJsonPreview ?? "生成保存预览后，可在这里查看将写入字段库的 JSON 片段。";

    public bool HasPersistedJsonPreview => SavePreview is not null;

    public string PreviewIssueCountText => PreviewIssues.Count == 0
        ? "没有预览问题。"
        : $"{PreviewIssues.Count} 个预览问题。";

    public bool CanPreviewSave => SavePreview?.CanSave == true;

    public bool CanSave => SavePreview?.CanSave == true;

    public FieldEditorSavePreview BuildSavePreview(
        IRa2FieldDefinitionProvider effectiveProvider,
        FieldEditorSaveTarget target)
    {
        FieldEditorDraft draft = _draftFactory.CreateDraft(this, target);
        FieldEditorSavePreview preview = _previewBuilder.BuildPreview(draft, effectiveProvider);
        SavePreview = preview;
        RefreshPreviewIssues(preview);
        StatusText = preview.Summary;
        return preview;
    }

    public FieldEditorSaveApplyResult ApplySave(FieldEditorSaveContext context, FieldEditorSaveTarget target)
    {
        ArgumentNullException.ThrowIfNull(context);

        FieldEditorSavePreview preview = BuildSavePreview(context.EffectiveProvider, target);
        if (!preview.CanSave)
        {
            FieldEditorSaveApplyResult blocked = new(
                success: false,
                preview.Summary,
                null,
                preview.Issues);
            ShowApplyResult(blocked);
            ClearLastApplyPaths();
            return blocked;
        }

        FieldEditorDraft draft = _draftFactory.CreateDraft(this, target);
        FieldEditorSaveApplyResult result = _applyService.Apply(draft, context);
        ShowApplyResult(result);
        if (result.Success)
        {
            UpdateLastApplyPaths(result);
            ClearPreviewAfterSuccessfulSave();
        }
        else
        {
            ClearLastApplyPaths();
        }

        return result;
    }

    private static string FormatAllowedValues(IReadOnlyCollection<Ra2FieldAllowedValue> values)
    {
        if (values.Count == 0)
            return string.Empty;

        return string.Join(
            Environment.NewLine,
            values.Select(value =>
            {
                Collection<string> parts = [];
                parts.Add(value.Value);
                if (!string.IsNullOrWhiteSpace(value.DisplayName))
                    parts.Add(value.DisplayName);

                if (!string.IsNullOrWhiteSpace(value.Description))
                    parts.Add(value.Description);

                return string.Join(" | ", parts);
            }));
    }

    private void RefreshPreviewIssues(FieldEditorSavePreview preview)
    {
        PreviewIssues.Clear();
        foreach (FieldEditorValidationIssue issue in preview.Issues)
            PreviewIssues.Add(issue);

        OnPropertyChanged(nameof(PreviewIssueCountText));
    }

    private void ShowApplyResult(FieldEditorSaveApplyResult result)
    {
        PreviewIssues.Clear();
        foreach (FieldEditorValidationIssue issue in result.Issues)
            PreviewIssues.Add(issue);

        StatusText = result.Message;
        OnPropertyChanged(nameof(PreviewIssueCountText));
    }

    private void UpdateLastApplyPaths(FieldEditorSaveApplyResult result)
    {
        LastApplyTargetFilePath = result.WriteResult?.TargetFilePath ?? string.Empty;
        LastApplyManifestFilePath = result.WriteResult?.ManifestFilePath ?? string.Empty;
    }

    private void ClearLastApplyPaths()
    {
        LastApplyTargetFilePath = string.Empty;
        LastApplyManifestFilePath = string.Empty;
    }

    private void ClearPreviewAfterSuccessfulSave()
    {
        SavePreview = null;
        PreviewIssues.Clear();
        OnPropertyChanged(nameof(PreviewIssueCountText));
    }

    private void ClearStalePreview()
    {
        if (SavePreview is null && PreviewIssues.Count == 0 && !HasLastApplyPaths)
            return;

        SavePreview = null;
        PreviewIssues.Clear();
        ClearLastApplyPaths();
        StatusText = "字段已修改，请重新生成保存预览。";
        OnPropertyChanged(nameof(PreviewIssueCountText));
    }

    private bool SetEditableProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref field, value, propertyName))
            return false;

        ClearStalePreview();
        return true;
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
