using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.ViewModels.Editing;

internal sealed class Ra2EditorStateViewModel
{
    public Ra2EditorStateViewModel(
        Ra2EditorDocumentState state,
        string? filePath,
        bool hasSession)
    {
        State = state;
        FilePath = filePath ?? string.Empty;
        HasSession = hasSession;
    }

    public Ra2EditorDocumentState State { get; }

    public string FilePath { get; }

    public bool HasSession { get; }

    public bool IsReadOnlyPreview => State == Ra2EditorDocumentState.ReadOnlyPreview;

    public bool IsEditing => State is Ra2EditorDocumentState.EditableClean or Ra2EditorDocumentState.EditableDirty;

    public bool IsDirty => State == Ra2EditorDocumentState.EditableDirty;

    public bool CanEnterEditMode => false;

    public bool CanRevertInMemoryChanges => IsEditing;

    public bool CanSavePreview => IsDirty;

    public string StateText => State switch
    {
        Ra2EditorDocumentState.ReadOnlyPreview => "未选择文件",
        Ra2EditorDocumentState.EditableClean => "已打开",
        Ra2EditorDocumentState.EditableDirty => "内存中已修改",
        _ => "未知"
    };

    public string SaveHintText => State switch
    {
        Ra2EditorDocumentState.EditableDirty => "当前文件有未保存的内容修改。请保存或放弃内存修改。",
        Ra2EditorDocumentState.EditableClean => "没有未保存的内容修改。",
        _ => "请选择一个 INI 文件。"
    };
}
