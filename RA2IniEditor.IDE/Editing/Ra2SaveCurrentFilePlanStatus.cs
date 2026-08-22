namespace RA2IniEditor.IDE.Editing;

internal enum Ra2SaveCurrentFilePlanStatus
{
    CanSave,
    NoEditableSession,
    ReadOnlyPreview,
    NotDirty,
    MissingFilePath,
    UnknownFailure
}
