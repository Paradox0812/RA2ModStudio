namespace RA2IniEditor.Core;

public enum IniIssueSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>INI 检查结果。</summary>
public sealed class IniIssue
{
    public IniIssue(IniIssueSeverity severity, int lineNumber, string message, string? filePath = null, string? sectionName = null, string? key = null, bool isNavigable = true)
    {
        Severity = severity;
        LineNumber = lineNumber;
        Message = message;
        FilePath = filePath;
        SectionName = sectionName;
        Key = key;
        IsNavigable = isNavigable;
    }

    public IniIssueSeverity Severity { get; }
    public int LineNumber { get; }
    public string Message { get; }
    public string? FilePath { get; }
    public string? SectionName { get; }
    public string? Key { get; }

    /// <summary>该问题是否允许从问题列表点击定位。资源缺失类提示通常不可定位，因为它们指向的是外部资源索引而不是 INI 源行。</summary>
    public bool IsNavigable { get; }
}
