using System.Text.Json;

namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiToolChoiceMode
{
    None = 0,
    Auto,
    Required
}

/// <summary>描述一次 provider 请求可以使用的函数工具。</summary>
internal sealed class Ra2AiToolDefinition
{
    public Ra2AiToolDefinition(
        string name,
        string description,
        string parametersJsonSchema)
    {
        Name = ValidateToken(name, 128, nameof(name));
        Description = ValidateText(description, 2048, nameof(description));
        ParametersJsonSchema = ValidateSchema(parametersJsonSchema);
    }

    public string Name { get; }

    public string Description { get; }

    public string ParametersJsonSchema { get; }

    private static string ValidateToken(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tool token cannot be empty.", parameterName);

        string normalized = value.Trim();
        if (normalized.Length > maximumLength ||
            normalized.Any(character =>
                !(char.IsAsciiLetterOrDigit(character) || character is '_' or '-')))
        {
            throw new ArgumentException("Tool token contains unsupported characters.", parameterName);
        }

        return normalized;
    }

    private static string ValidateText(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tool text cannot be empty.", parameterName);

        string normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.Contains('\0'))
            throw new ArgumentException("Tool text is invalid.", parameterName);

        return normalized;
    }

    private static string ValidateSchema(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tool parameter schema cannot be empty.", nameof(value));

        using JsonDocument document = JsonDocument.Parse(value);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Tool parameter schema must be a JSON object.", nameof(value));

        return value;
    }
}

/// <summary>表示 provider 已完整返回、但尚未进行 authoring 语义验证的工具调用。</summary>
internal sealed class Ra2AiToolCall
{
    public const int MaximumIdLength = 256;
    public const int MaximumNameLength = 128;
    public const int MaximumArgumentsLength = 65536;

    public Ra2AiToolCall(string id, string name, string argumentsJson)
    {
        Id = ValidateRequired(id, MaximumIdLength, nameof(id));
        Name = ValidateRequired(name, MaximumNameLength, nameof(name));
        ArgumentsJson = ValidateRequired(
            argumentsJson,
            MaximumArgumentsLength,
            nameof(argumentsJson),
            trim: false);
    }

    public string Id { get; }

    public string Name { get; }

    public string ArgumentsJson { get; }

    private static string ValidateRequired(
        string value,
        int maximumLength,
        string parameterName,
        bool trim = true)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tool call value cannot be empty.", parameterName);

        string normalized = trim ? value.Trim() : value;
        if (normalized.Length > maximumLength || normalized.Contains('\0'))
            throw new ArgumentException("Tool call value is invalid.", parameterName);

        return normalized;
    }
}

/// <summary>表示 SSE 中一个可能跨事件分片的工具调用增量。</summary>
internal readonly record struct Ra2AiToolCallDelta
{
    public Ra2AiToolCallDelta(
        int index,
        string? idFragment,
        string? nameFragment,
        string? argumentsFragment)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (string.IsNullOrEmpty(idFragment) &&
            string.IsNullOrEmpty(nameFragment) &&
            string.IsNullOrEmpty(argumentsFragment))
        {
            throw new ArgumentException("A tool-call delta requires at least one fragment.");
        }

        Index = index;
        IdFragment = idFragment ?? string.Empty;
        NameFragment = nameFragment ?? string.Empty;
        ArgumentsFragment = argumentsFragment ?? string.Empty;
    }

    public int Index { get; }

    public string IdFragment { get; }

    public string NameFragment { get; }

    public string ArgumentsFragment { get; }
}
