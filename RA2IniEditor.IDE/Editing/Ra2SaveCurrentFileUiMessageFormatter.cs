namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2SaveCurrentFileUiMessageFormatter
{
    public string Format(Ra2SaveCurrentFileResult result, bool hasLoadedFile)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.FailureKind switch
        {
            Ra2SaveCurrentFileFailureKind.None => FormatSuccess(result),
            Ra2SaveCurrentFileFailureKind.SavePlanCannotSave => FormatSavePlanCannotSave(result, hasLoadedFile),
            Ra2SaveCurrentFileFailureKind.BackupFailed => FormatBackupFailed(result),
            Ra2SaveCurrentFileFailureKind.WriteFailed => FormatWriteFailed(result),
            Ra2SaveCurrentFileFailureKind.RollbackFailed => FormatRollbackFailed(result),
            _ => $"保存当前文件失败：{result.Message}"
        };
    }

    private static string FormatSuccess(Ra2SaveCurrentFileResult result)
    {
        string filePath = result.SavePlan?.FilePath ?? string.Empty;
        string backupPath = ResolveBackupPath(result);
        return string.IsNullOrWhiteSpace(backupPath)
            ? $"保存当前文件成功：{filePath}"
            : $"保存当前文件成功：{filePath}{Environment.NewLine}备份：{backupPath}";
    }

    private static string FormatSavePlanCannotSave(
        Ra2SaveCurrentFileResult result,
        bool hasLoadedFile)
        => result.SavePlan?.Status switch
        {
            Ra2SaveCurrentFilePlanStatus.NoEditableSession when hasLoadedFile =>
                "当前没有可保存的编辑文件。",
            Ra2SaveCurrentFilePlanStatus.NoEditableSession =>
                "当前没有可保存的编辑文件。",
            Ra2SaveCurrentFilePlanStatus.ReadOnlyPreview =>
                "当前文件不可编辑，无法保存。",
            Ra2SaveCurrentFilePlanStatus.NotDirty =>
                "当前文件没有未保存的内存修改，未执行写入。",
            Ra2SaveCurrentFilePlanStatus.MissingFilePath =>
                "当前文件缺少文件路径，无法保存。",
            _ => $"当前文件暂不能保存：{result.Message}"
        };

    private static string FormatBackupFailed(Ra2SaveCurrentFileResult result)
        => $"保存当前文件失败：无法创建保存前备份。未保存修改仍保留在编辑器中。{Environment.NewLine}原因：{result.Message}";

    private static string FormatWriteFailed(Ra2SaveCurrentFileResult result)
    {
        string backupPath = ResolveBackupPath(result);
        string backupLine = string.IsNullOrWhiteSpace(backupPath)
            ? string.Empty
            : $"{Environment.NewLine}备份：{backupPath}";
        return $"保存当前文件失败，已从备份回滚。未保存修改仍保留在编辑器中。{backupLine}{Environment.NewLine}原因：{result.Message}";
    }

    private static string FormatRollbackFailed(Ra2SaveCurrentFileResult result)
    {
        string backupPath = ResolveBackupPath(result);
        string backupLine = string.IsNullOrWhiteSpace(backupPath)
            ? string.Empty
            : $"{Environment.NewLine}备份：{backupPath}";
        return $"保存当前文件失败，且回滚失败。原文件可能需要从备份手动恢复。{backupLine}{Environment.NewLine}原因：{result.Message}";
    }

    private static string ResolveBackupPath(Ra2SaveCurrentFileResult result)
        => result.BackupResult?.BackupFilePath
            ?? result.BackupPlan?.BackupFilePath
            ?? string.Empty;
}
