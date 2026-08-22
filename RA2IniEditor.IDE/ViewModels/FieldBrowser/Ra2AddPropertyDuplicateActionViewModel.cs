using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.ViewModels.FieldBrowser;

internal sealed class Ra2AddPropertyDuplicateActionViewModel : INotifyPropertyChanged
{
    private Ra2DuplicateKeyAction _selectedAction;

    public Ra2AddPropertyDuplicateActionViewModel(
        Ra2DuplicateKeyMatch? match,
        Ra2EditorDocumentState editorState)
    {
        Match = match;
        HasDuplicate = match is not null;
        CanJumpExisting = HasDuplicate;
        CanReplaceExisting = HasDuplicate && editorState != Ra2EditorDocumentState.ReadOnlyPreview;
        CanInsertDuplicate = HasDuplicate && editorState != Ra2EditorDocumentState.ReadOnlyPreview;
        AvailableActions = HasDuplicate
            ? [
                Ra2DuplicateKeyAction.JumpExisting,
                Ra2DuplicateKeyAction.ReplaceExisting,
                Ra2DuplicateKeyAction.InsertDuplicate,
                Ra2DuplicateKeyAction.Cancel
              ]
            : [Ra2DuplicateKeyAction.InsertDuplicate, Ra2DuplicateKeyAction.Cancel];
        _selectedAction = HasDuplicate
            ? Ra2DuplicateKeyAction.JumpExisting
            : Ra2DuplicateKeyAction.InsertDuplicate;
        WarningText = match is null
            ? string.Empty
            : $"当前 Section 已包含字段：{match.Key}，第 {match.LineNumber} 行，当前值：{FormatValue(match.ExistingValue)}。";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool HasDuplicate { get; }

    public string WarningText { get; }

    public Ra2DuplicateKeyMatch? Match { get; }

    public bool CanJumpExisting { get; }

    public bool CanReplaceExisting { get; }

    public bool CanInsertDuplicate { get; }

    public IReadOnlyList<Ra2DuplicateKeyAction> AvailableActions { get; }

    public Ra2DuplicateKeyAction SelectedAction
    {
        get => _selectedAction;
        set
        {
            if (_selectedAction == value)
                return;

            _selectedAction = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanConfirmSelectedAction));
        }
    }

    public bool CanConfirmSelectedAction
        => SelectedAction switch
        {
            Ra2DuplicateKeyAction.Cancel => true,
            Ra2DuplicateKeyAction.JumpExisting => CanJumpExisting,
            Ra2DuplicateKeyAction.ReplaceExisting => CanReplaceExisting,
            Ra2DuplicateKeyAction.InsertDuplicate => !HasDuplicate || CanInsertDuplicate,
            _ => false
        };

    private static string FormatValue(string value)
        => string.IsNullOrEmpty(value) ? "<空>" : value;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (!string.IsNullOrWhiteSpace(propertyName))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
