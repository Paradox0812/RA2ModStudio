using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.ViewModels.Language;

internal sealed class Ra2PeekDefinitionViewModel
{
    public Ra2PeekDefinitionViewModel(Ra2DefinitionTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        Title = target.Title;
        Kind = ToChineseKind(target.Kind);
        Detail = LocalizeDetail(target.Detail);
        SourceName = string.IsNullOrWhiteSpace(target.SourceName) ? "\u672a\u77e5\u6765\u6e90" : LocalizeSourceName(target.SourceName);
        SourcePath = string.IsNullOrWhiteSpace(target.SourcePath) ? string.Empty : target.SourcePath;
        LineText = target.TargetLineNumber is null ? "\u65e0\u6e90\u7801\u884c" : $"\u7b2c {target.TargetLineNumber.Value} \u884c";
        Description = string.IsNullOrWhiteSpace(target.Description)
            ? "\u6682\u65e0\u8bf4\u660e\u3002"
            : LocalizeDescription(target.Description);
    }

    public string Title { get; }

    public string Kind { get; }

    public string Detail { get; }

    public string SourceName { get; }

    public string SourcePath { get; }

    public string LineText { get; }

    public string Description { get; }

    private static string ToChineseKind(Ra2DefinitionTargetKind kind) => kind switch
    {
        Ra2DefinitionTargetKind.FieldDefinition => "\u5b57\u6bb5\u5b9a\u4e49",
        Ra2DefinitionTargetKind.SectionDefinition => "Section \u5b9a\u4e49",
        Ra2DefinitionTargetKind.ReferenceTarget => "\u5f15\u7528\u76ee\u6807",
        _ => kind.ToString()
    };

    private static string LocalizeSourceName(string sourceName)
        => sourceName.Equals("Current document", StringComparison.OrdinalIgnoreCase)
            ? "\u5f53\u524d\u6587\u4ef6"
            : sourceName;

    private static string LocalizeDetail(string detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
            return "\u6682\u65e0\u8be6\u7ec6\u4fe1\u606f\u3002";

        const string foundSuffix = " reference target in current document.";
        if (detail.EndsWith(foundSuffix, StringComparison.OrdinalIgnoreCase))
            return $"\u5f53\u524d\u6587\u4ef6\u4e2d\u7684 {detail[..^foundSuffix.Length]} \u5f15\u7528\u76ee\u6807\u3002";

        const string missingSuffix = " reference target was not found in the current document.";
        if (detail.EndsWith(missingSuffix, StringComparison.OrdinalIgnoreCase))
            return $"\u5f53\u524d\u6587\u4ef6\u4e2d\u672a\u627e\u5230 {detail[..^missingSuffix.Length]} \u5f15\u7528\u76ee\u6807\u3002";

        return detail
            .Replace("Type: ", "\u7c7b\u578b\uff1a", StringComparison.OrdinalIgnoreCase)
            .Replace("Type:", "\u7c7b\u578b\uff1a", StringComparison.OrdinalIgnoreCase)
            .Replace("No description available.", "\u6682\u65e0\u8bf4\u660e\u3002", StringComparison.OrdinalIgnoreCase);
    }

    private static string LocalizeDescription(string description)
    {
        string result = description
            .Replace("\u76ee\u6807\u5907\u6ce8:", "\u5907\u6ce8\uff1a", StringComparison.Ordinal)
            .Replace("\u5f15\u7528\u5907\u6ce8:", "\u5f15\u7528\u5907\u6ce8\uff1a", StringComparison.Ordinal)
            .Replace("\u4f4d\u7f6e:", "\u4f4d\u7f6e\uff1a", StringComparison.Ordinal)
            .Replace("\u5907\u6ce8\uff1a ", "\u5907\u6ce8\uff1a", StringComparison.Ordinal)
            .Replace("\u5f15\u7528\u5907\u6ce8\uff1a ", "\u5f15\u7528\u5907\u6ce8\uff1a", StringComparison.Ordinal)
            .Replace("\u4f4d\u7f6e\uff1a ", "\u4f4d\u7f6e\uff1a", StringComparison.Ordinal)
            .Replace("Current document", "\u5f53\u524d\u6587\u4ef6", StringComparison.OrdinalIgnoreCase);

        return System.Text.RegularExpressions.Regex.Replace(
            result,
            @"\bLine\s+(\d+)\b",
            "\u7b2c $1 \u884c",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
