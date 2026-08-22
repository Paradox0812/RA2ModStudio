namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2ValueCompletionContext
{
    public Ra2ValueCompletionContext(
        string currentValueText,
        string currentTokenPrefix,
        bool isListToken,
        IReadOnlyList<string> existingTokens)
    {
        CurrentValueText = currentValueText ?? string.Empty;
        CurrentTokenPrefix = currentTokenPrefix ?? string.Empty;
        IsListToken = isListToken;
        ExistingTokens = existingTokens ?? [];
    }

    public string CurrentValueText { get; }

    public string CurrentTokenPrefix { get; }

    public bool IsListToken { get; }

    public IReadOnlyList<string> ExistingTokens { get; }
}
