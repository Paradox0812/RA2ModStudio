using System.Text.RegularExpressions;

namespace RA2IniEditor.IDE.AI;

/// <summary>
/// 出站文本清理结果；只携带清理后的文本和是否发生替换的事实。
/// </summary>
internal readonly record struct Ra2AiOutboundTextSanitizationResult(
    string Text,
    bool WasRedacted);

/// <summary>
/// 统一清理可能泄露 provider 配置、凭据或原始载荷的出站文本。
/// </summary>
internal static class Ra2AiOutboundTextSanitizer
{
    internal const string RedactedText = "[redacted sensitive content]";

    private static readonly Regex ApiKeyLikeTokenPattern = new(
        @"\b(?:sk|ds)-[A-Za-z0-9_\-]{8,}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly string[] SensitiveLineMarkers =
    [
        "authorization",
        "bearer ",
        "api key",
        "api_key",
        "apikey",
        "deepseek_api_key",
        "deepseek_base_url",
        "deepseek_model",
        "deepseek_timeout_seconds",
        "raw request",
        "raw_request",
        "request payload",
        "raw response",
        "raw_response",
        "response payload",
        "provider metadata",
        "provider internal",
        "environment variable",
        "environment variables",
        "env:"
    ];

    public static Ra2AiOutboundTextSanitizationResult Sanitize(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return new Ra2AiOutboundTextSanitizationResult(string.Empty, WasRedacted: false);

        bool wasRedacted = false;
        string[] lines = text.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index];
            if (SensitiveLineMarkers.Any(marker =>
                line.Contains(marker, StringComparison.OrdinalIgnoreCase)))
            {
                lines[index] = RedactedText;
                wasRedacted = true;
                continue;
            }

            string sanitizedLine = ApiKeyLikeTokenPattern.Replace(line, RedactedText);
            wasRedacted |= !string.Equals(sanitizedLine, line, StringComparison.Ordinal);
            lines[index] = sanitizedLine;
        }

        return new Ra2AiOutboundTextSanitizationResult(
            string.Join(Environment.NewLine, lines),
            wasRedacted);
    }
}
