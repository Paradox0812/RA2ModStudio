using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;

namespace RA2IniEditor.IDE.ViewModels.FieldAnnotations;

internal sealed class Ra2FieldAnnotationEditorViewModel : INotifyPropertyChanged
{
    private readonly IRa2FieldAnnotationStore _store;
    private readonly IRa2FieldAnnotationEditingService _editingService;
    private readonly Ra2SectionKindDisplayNameProvider _sectionDisplayNameProvider;
    private Ra2FieldAnnotationPack _pack;
    private string _displayName;
    private string _aliasesText;
    private string _note;
    private string _statusText;
    private bool _isDirty;

    public Ra2FieldAnnotationEditorViewModel(
        Ra2SectionKind sectionKind,
        Ra2FieldDisplayInfo displayInfo,
        Ra2FieldAnnotationPack pack,
        string annotationPath,
        IRa2FieldAnnotationStore store,
        IRa2FieldAnnotationEditingService editingService,
        Ra2SectionKindDisplayNameProvider? sectionDisplayNameProvider = null)
    {
        DisplayInfo = displayInfo ?? throw new ArgumentNullException(nameof(displayInfo));
        _pack = pack ?? Ra2FieldAnnotationPack.Empty();
        AnnotationPath = annotationPath ?? string.Empty;
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _editingService = editingService ?? throw new ArgumentNullException(nameof(editingService));
        _sectionDisplayNameProvider = sectionDisplayNameProvider ?? new Ra2SectionKindDisplayNameProvider();
        SectionKind = sectionKind;
        SectionKindName = sectionKind.ToString();
        SectionKindDisplayName = _sectionDisplayNameProvider.GetDisplayName(sectionKind);
        Key = displayInfo.Key;

        Ra2FieldAnnotationEntry? annotation = FindExactAnnotation(_pack, SectionKindName, Key);
        _displayName = annotation?.DisplayName ?? string.Empty;
        _aliasesText = annotation is null ? string.Empty : string.Join(", ", annotation.Aliases);
        _note = annotation?.Note ?? string.Empty;
        _statusText = string.IsNullOrWhiteSpace(AnnotationPath)
            ? "未打开项目，无法保存字段注释。"
            : "字段注释会保存到项目 .ra2ide 目录，不会修改 INI。";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Ra2FieldDisplayInfo DisplayInfo { get; }

    public Ra2FieldAnnotationPack CurrentPack => _pack;

    public Ra2SectionKind SectionKind { get; }

    public string SectionKindName { get; }

    public string SectionKindDisplayName { get; }

    public string Key { get; }

    public string FieldTypeDisplay => DisplayInfo.TypeDisplay;

    public string FieldSourceDisplay => DisplayInfo.SourceDisplay;

    public string FieldDescription => DisplayInfo.Description ?? "暂无字段说明。";

    public string AnnotationPath { get; }

    public bool CanSave => !string.IsNullOrWhiteSpace(AnnotationPath);

    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (SetProperty(ref _displayName, value ?? string.Empty))
                MarkDirty();
        }
    }

    public string AliasesText
    {
        get => _aliasesText;
        set
        {
            if (SetProperty(ref _aliasesText, value ?? string.Empty))
                MarkDirty();
        }
    }

    public string Note
    {
        get => _note;
        set
        {
            if (SetProperty(ref _note, value ?? string.Empty))
                MarkDirty();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool IsDirty
    {
        get => _isDirty;
        private set => SetProperty(ref _isDirty, value);
    }

    public bool Save()
    {
        if (!CanSave)
        {
            StatusText = "未打开项目，无法保存字段注释。";
            return false;
        }

        Ra2FieldAnnotationPack updatedPack = _editingService.Upsert(
            _pack,
            SectionKindName,
            Key,
            DisplayName,
            ParseAliases(AliasesText),
            Note);
        Ra2FieldAnnotationSaveResult result = _store.Save(AnnotationPath, updatedPack);
        if (!result.Success)
        {
            StatusText = $"字段注释保存失败：{result.ErrorMessage ?? "未知错误"}";
            return false;
        }

        _pack = updatedPack;
        IsDirty = false;
        StatusText = "字段注释库已保存。";
        OnPropertyChanged(nameof(CurrentPack));
        return true;
    }

    public bool CreateLibrary()
    {
        if (!CanSave)
        {
            StatusText = "未打开项目，无法创建字段注释库。";
            return false;
        }

        Ra2FieldAnnotationSaveResult result = _store.Save(AnnotationPath, _pack);
        if (!result.Success)
        {
            StatusText = $"创建字段注释库失败：{result.ErrorMessage ?? "未知错误"}";
            return false;
        }

        IsDirty = false;
        StatusText = "字段注释库已创建。";
        return true;
    }

    public static IReadOnlyList<string> ParseAliases(string aliasesText)
    {
        if (string.IsNullOrWhiteSpace(aliasesText))
            return [];

        return aliasesText
            .Split([',', '，', ';', '；'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(alias => !string.IsNullOrWhiteSpace(alias))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static Ra2FieldAnnotationEntry? FindExactAnnotation(
        Ra2FieldAnnotationPack pack,
        string sectionKind,
        string key)
    {
        return pack.Entries.LastOrDefault(entry =>
            string.Equals(entry.SectionKind, sectionKind, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase));
    }

    private void MarkDirty()
    {
        if (!IsDirty)
            IsDirty = true;

        StatusText = "字段注释尚未保存。";
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
    {
        if (!string.IsNullOrWhiteSpace(propertyName))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
