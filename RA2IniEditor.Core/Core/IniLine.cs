namespace RA2IniEditor.Core;

/// <summary>INI 行基类，用于保留原始顺序和原始文本。</summary>
public abstract class IniLine
{
    public int LineNumber { get; set; }
    public string RawText { get; set; } = string.Empty;

    /// <summary>根据当前对象状态写回文本。</summary>
    public virtual string ToOutputText() => RawText;
}

/// <summary>空行。</summary>
public sealed class IniBlankLine : IniLine
{
}

/// <summary>注释行。</summary>
public sealed class IniCommentLine : IniLine
{
    public string Comment { get; init; } = string.Empty;
}

/// <summary>无法识别但需要保留的行。</summary>
public sealed class IniUnknownLine : IniLine
{
    public string Reason { get; init; } = string.Empty;
}

/// <summary>Section 行，例如 [E1]。</summary>
public sealed class IniSectionLine : IniLine
{
    public string SectionName { get; set; } = string.Empty;

    public override string ToOutputText() => $"[{SectionName}]";
}

/// <summary>Key=Value 行。</summary>
public class IniKeyValueLine : IniLine
{
    public string SectionName { get; set; } = string.Empty;
    public string LeadingWhitespace { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Separator { get; set; } = "=";
    public string Value { get; set; } = string.Empty;
    public string InlineCommentSuffix { get; set; } = string.Empty;

    public override string ToOutputText() => $"{LeadingWhitespace}{Key}{Separator}{Value}{InlineCommentSuffix}";
}


/// <summary>由拆分 INI 优先级整理产生的覆盖属性行。该行仍参与读取和编辑，但保存时按 IsCovered 决定是否输出为注释。</summary>
public sealed class IniCoveredKeyValueLine : IniKeyValueLine
{
    public bool IsCovered { get; set; } = true;
    public string CoverReason { get; set; } = "covered by higher priority split INI";

    public override string ToOutputText()
    {
        string keyValueText = base.ToOutputText();
        return IsCovered ? $"{LeadingWhitespace}; RA2IniEditor: {CoverReason}: {keyValueText.TrimStart()}" : keyValueText;
    }
}
