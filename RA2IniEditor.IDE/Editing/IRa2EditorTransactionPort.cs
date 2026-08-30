namespace RA2IniEditor.IDE.Editing;

/// <summary>
/// 表示唯一允许把结构化预览提交到当前实时编辑器状态的内部端口。
/// </summary>
internal interface IRa2EditorTransactionPort
{
    Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview);

    Ra2ProjectEditApplyResult ApplyProject(Ra2ProjectEditPreview preview)
        => Ra2ProjectEditApplyResult.Failed(
            Ra2ProjectEditApplyOutcomeKind.UnexpectedFailure,
            preview?.ProjectPreviewId ?? Guid.Empty,
            "Project editing is not connected to this transaction port.");
}
