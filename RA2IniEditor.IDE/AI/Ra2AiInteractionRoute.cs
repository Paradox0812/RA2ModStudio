using System.Text;
using System.Text.RegularExpressions;

namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiEditAvailabilityKind
{
    Available = 0,
    MissingConfiguration,
    UnsupportedEndpoint,
    NoEditableDocument,
    SnapshotUnavailable,
    ResourceLimitExceeded
}

internal enum Ra2AiInteractionRouteKind
{
    Advisory = 0,
    EditExplicit,
    EditAmbiguous,
    EditUnavailable
}

internal readonly record struct Ra2AiInteractionRoute(
    Ra2AiInteractionRouteKind Kind,
    Ra2AiCapabilityMode CapabilityMode,
    Ra2AiEditAvailabilityKind EditAvailability);

/// <summary>仅根据用户可见提示词和本地可用性事实裁决编辑权限。</summary>
internal static partial class Ra2AiInteractionRouter
{
    private const int MaximumRoutedPromptCharacters = 32768;

    private static readonly string[] AdvisoryMarkers =
    [
        "不要修改", "不要改", "不修改", "无需修改", "只解释", "仅解释", "只分析", "仅分析",
        "只给代码", "仅给代码", "do not modify", "don't modify", "do not edit", "explain only",
        "analysis only", "advisory only"
    ];

    private static readonly string[] EditActionMarkers =
    [
        "修改", "更改", "改为", "设置", "设为", "替换", "新增", "添加", "写入", "修正", "优化",
        "update", "change", "set", "replace", "insert", "add", "write", "fix"
    ];

    private static readonly string[] CurrentDocumentMarkers =
    [
        "当前文件", "当前文档", "这个文件", "本文件", "this file", "current file", "current document"
    ];

    private static readonly string[] AssignmentMarkers =
    [
        "=", "为", "成", " to ", " with "
    ];

    internal static Ra2AiInteractionRoute Resolve(
        string userPrompt,
        Ra2AiEditAvailabilityKind editAvailability)
    {
        string prompt = Normalize(userPrompt);
        if (ContainsAny(prompt, AdvisoryMarkers))
            return Create(Ra2AiInteractionRouteKind.Advisory, editAvailability);

        bool hasEditAction = ContainsAny(prompt, EditActionMarkers);
        bool hasCurrentDocumentTarget = ContainsAny(prompt, CurrentDocumentMarkers);
        bool hasAssignment = ContainsAny(prompt, AssignmentMarkers);
        if (hasEditAction && hasCurrentDocumentTarget && hasAssignment)
        {
            return editAvailability == Ra2AiEditAvailabilityKind.Available
                ? Create(Ra2AiInteractionRouteKind.EditExplicit, editAvailability)
                : Create(Ra2AiInteractionRouteKind.EditUnavailable, editAvailability);
        }

        if (hasEditAction || LooksLikeBareKeyValue(prompt))
            return Create(Ra2AiInteractionRouteKind.EditAmbiguous, editAvailability);

        return Create(Ra2AiInteractionRouteKind.Advisory, editAvailability);
    }

    private static Ra2AiInteractionRoute Create(
        Ra2AiInteractionRouteKind kind,
        Ra2AiEditAvailabilityKind availability)
        => new(
            kind,
            kind == Ra2AiInteractionRouteKind.EditExplicit
                ? Ra2AiCapabilityMode.CurrentDocumentEditPreview
                : Ra2AiCapabilityMode.AdvisoryOnly,
            availability);

    private static bool LooksLikeBareKeyValue(string prompt)
        => BareKeyValuePattern().IsMatch(prompt);

    private static bool ContainsAny(string prompt, IReadOnlyList<string> markers)
    {
        foreach (string marker in markers)
        {
            if (prompt.Contains(marker, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        ReadOnlySpan<char> source = value.AsSpan(0, Math.Min(value.Length, MaximumRoutedPromptCharacters));
        StringBuilder builder = new(source.Length);
        bool previousWasWhitespace = false;
        foreach (char character in source)
        {
            bool isWhitespace = char.IsWhiteSpace(character);
            if (!isWhitespace || !previousWasWhitespace)
                builder.Append(isWhitespace ? ' ' : char.ToLowerInvariant(character));
            previousWasWhitespace = isWhitespace;
        }

        return builder.ToString().Trim();
    }

    [GeneratedRegex(@"^\s*[a-z_][a-z0-9_.-]*\s*(?:=|\s)\s*[^\s]+\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BareKeyValuePattern();
}
