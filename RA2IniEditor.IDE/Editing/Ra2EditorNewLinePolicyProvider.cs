using RA2IniEditor.IDE.TextModel;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2EditorNewLinePolicyProvider
{
    public Ra2EditorNewLineSavePolicy GetDefaultPolicy(Ra2IniTextDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return Ra2EditorNewLineSavePolicy.PreserveCurrentText;
    }
}
