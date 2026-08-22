namespace RA2IniEditor.IDE.ViewModels.Language;

internal static class Ra2CompletionDropdownPositioning
{
    public static int NormalizeCaretOffset(string? text, int caretOffset)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return Math.Clamp(caretOffset, 0, text.Length);
    }

    public static bool CanShowNearCaret(string? text, int caretOffset)
        => !string.IsNullOrEmpty(text) && caretOffset >= 0 && caretOffset <= text.Length;
}
