namespace RA2IniEditor.IDE.Editing;

internal interface IRa2TextFirstFileWriter
{
    Ra2TextFileWriteResult Write(Ra2EditorSavePlan plan);
}
