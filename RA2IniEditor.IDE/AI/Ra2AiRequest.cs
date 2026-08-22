namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiRequest
{
    public Ra2AiRequest(
        Ra2AiIntent intent,
        string userPrompt,
        string promptText,
        Ra2AiRequestPreparationFlags preparationFlags = Ra2AiRequestPreparationFlags.None,
        IEnumerable<Ra2AiToolDefinition>? tools = null,
        Ra2AiToolChoiceMode toolChoice = Ra2AiToolChoiceMode.None,
        string? systemPromptText = null,
        string? userContentText = null)
    {
        if (!Enum.IsDefined(toolChoice))
            throw new ArgumentOutOfRangeException(nameof(toolChoice));

        Ra2AiToolDefinition[] toolArray = tools?.ToArray() ?? [];
        if (toolArray.Any(tool => tool is null))
            throw new ArgumentException("AI request tools cannot contain null entries.", nameof(tools));
        if (toolArray
            .GroupBy(tool => tool.Name, StringComparer.Ordinal)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException("AI request tool names must be unique.", nameof(tools));
        }
        if ((toolArray.Length == 0) != (toolChoice == Ra2AiToolChoiceMode.None))
        {
            throw new ArgumentException(
                "AI request tool choice must match the declared tools.",
                nameof(toolChoice));
        }
        if (string.IsNullOrWhiteSpace(systemPromptText) != string.IsNullOrWhiteSpace(userContentText))
        {
            throw new ArgumentException(
                "Separated AI request messages require both system and user content.");
        }
        if (!string.IsNullOrWhiteSpace(systemPromptText) && toolArray.Length == 0)
            throw new ArgumentException("Separated AI request messages require an authoring tool.");

        Intent = intent;
        UserPrompt = userPrompt ?? string.Empty;
        PromptText = promptText ?? string.Empty;
        PreparationFlags = preparationFlags;
        PromptCharacterCount = PromptText.Length;
        Tools = Array.AsReadOnly(toolArray);
        ToolChoice = toolChoice;
        SystemPromptText = systemPromptText ?? string.Empty;
        UserContentText = userContentText ?? string.Empty;
    }

    public Ra2AiIntent Intent { get; }

    public string UserPrompt { get; }

    public string PromptText { get; }

    public Ra2AiRequestPreparationFlags PreparationFlags { get; }

    public int PromptCharacterCount { get; }

    public IReadOnlyList<Ra2AiToolDefinition> Tools { get; }

    public Ra2AiToolChoiceMode ToolChoice { get; }

    public bool HasSeparatedMessages => SystemPromptText.Length > 0;

    public string SystemPromptText { get; }

    public string UserContentText { get; }
}
