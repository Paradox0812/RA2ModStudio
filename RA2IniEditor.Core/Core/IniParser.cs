using System.Text;
using System.Text.RegularExpressions;

namespace RA2IniEditor.Core;

/// <summary>保真 INI 解析器：尽量保留注释、空行、顺序和未知行。</summary>
public static partial class IniParser
{
    public static IniDocument Parse(string text, string? filePath = null, Encoding? encoding = null)
    {
        var document = new IniDocument
        {
            FilePath = filePath,
            Encoding = encoding ?? new UTF8Encoding(false),
            NewLine = DetectNewLine(text),
            OriginalText = text
        };

        IniSection? currentSection = null;
        string? currentSectionName = null;
        string[] lines = SplitLines(text);

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            string raw = lines[i];
            string trimmed = raw.Trim();

            if (string.IsNullOrWhiteSpace(raw))
            {
                document.Lines.Add(new IniBlankLine { LineNumber = lineNumber, RawText = raw });
                continue;
            }

            if (trimmed.StartsWith(';') || trimmed.StartsWith('#') || trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                IniCoveredKeyValueLine? coveredLine = TryParseCoveredKeyValueLine(raw, lineNumber, currentSectionName);
                if (coveredLine is not null)
                {
                    document.Lines.Add(coveredLine);
                    if (currentSection is not null)
                        currentSection.KeyValues.Add(coveredLine);
                    else
                        document.ParseIssues.Add(new IniIssue(IniIssueSeverity.Warning, lineNumber, $"被覆盖 Key '{coveredLine.Key}' 位于任何 Section 之前。"));
                    continue;
                }

                document.Lines.Add(new IniCommentLine { LineNumber = lineNumber, RawText = raw, Comment = trimmed });
                continue;
            }

            Match sectionMatch = SectionRegex().Match(raw);
            if (sectionMatch.Success)
            {
                string sectionName = sectionMatch.Groups["name"].Value.Trim();
                var sectionLine = new IniSectionLine { LineNumber = lineNumber, RawText = raw, SectionName = sectionName };
                var section = new IniSection(sectionName, sectionLine);

                document.Lines.Add(sectionLine);
                document.Sections.Add(section);
                currentSection = section;
                currentSectionName = sectionName;
                continue;
            }

            int equalsIndex = raw.IndexOf('=');
            if (equalsIndex >= 0)
            {
                var keyValueLine = ParseKeyValueLine(raw, lineNumber, currentSectionName);
                document.Lines.Add(keyValueLine);

                if (currentSection is null)
                {
                    document.ParseIssues.Add(new IniIssue(IniIssueSeverity.Warning, lineNumber, $"Key '{keyValueLine.Key}' 位于任何 Section 之前。"));
                }
                else
                {
                    currentSection.KeyValues.Add(keyValueLine);
                }

                continue;
            }

            document.Lines.Add(new IniUnknownLine { LineNumber = lineNumber, RawText = raw, Reason = "无法识别的非空行" });
            document.ParseIssues.Add(new IniIssue(IniIssueSeverity.Warning, lineNumber, "无法识别该行，保存时会原样保留。"));
        }

        document.Issues.AddRange(document.ParseIssues);
        IniValidator.AppendBasicIssues(document);
        return document;
    }

    private static IniKeyValueLine ParseKeyValueLine(string raw, int lineNumber, string? sectionName)
    {
        int firstNonWhitespace = raw.TakeWhile(char.IsWhiteSpace).Count();
        string leadingWhitespace = raw[..firstNonWhitespace];
        int equalsIndex = raw.IndexOf('=');
        string keyPart = raw[firstNonWhitespace..equalsIndex];
        string key = keyPart.Trim();
        int keyEndIndex = firstNonWhitespace + keyPart.IndexOf(key, StringComparison.Ordinal) + key.Length;
        string separator = raw[keyEndIndex..(equalsIndex + 1)];
        string valuePart = raw[(equalsIndex + 1)..];
        string value = valuePart;
        string inlineCommentSuffix = string.Empty;

        int commentIndex = FindInlineCommentIndex(valuePart);
        if (commentIndex >= 0)
        {
            string beforeComment = valuePart[..commentIndex];
            inlineCommentSuffix = valuePart[commentIndex..];
            value = beforeComment.TrimEnd();
            inlineCommentSuffix = beforeComment[value.Length..] + inlineCommentSuffix;
        }

        return new IniKeyValueLine
        {
            LineNumber = lineNumber,
            RawText = raw,
            SectionName = sectionName ?? string.Empty,
            LeadingWhitespace = leadingWhitespace,
            Key = key,
            Separator = string.IsNullOrEmpty(separator) ? "=" : separator,
            Value = value,
            InlineCommentSuffix = inlineCommentSuffix
        };
    }


    private static IniCoveredKeyValueLine? TryParseCoveredKeyValueLine(string raw, int lineNumber, string? sectionName)
    {
        Match match = CoveredKeyValueRegex().Match(raw);
        if (!match.Success)
            return null;

        string keyValueText = match.Groups["kv"].Value.Trim();
        if (!keyValueText.Contains('='))
            return null;

        IniKeyValueLine parsed = ParseKeyValueLine(keyValueText, lineNumber, sectionName);
        return new IniCoveredKeyValueLine
        {
            LineNumber = lineNumber,
            RawText = raw,
            SectionName = sectionName ?? string.Empty,
            LeadingWhitespace = match.Groups["indent"].Value,
            Key = parsed.Key,
            Separator = parsed.Separator,
            Value = parsed.Value,
            InlineCommentSuffix = parsed.InlineCommentSuffix,
            CoverReason = match.Groups["reason"].Value.Trim(),
            IsCovered = true
        };
    }

    private static int FindInlineCommentIndex(string valuePart)
    {
        // RA2/YR INI treats ';' as the start of an inline comment even when it is
        // written immediately after the value, for example:
        // OccupyWeapon=UCM1Carbine; The weapon I use while Occupying.
        // Keep the suffix for lossless save, but do not let validators treat it as part of the value.
        return valuePart.IndexOf(';');
    }

    private static string DetectNewLine(string text)
    {
        int crlfIndex = text.IndexOf("\r\n", StringComparison.Ordinal);
        int lfIndex = text.IndexOf('\n');
        int crIndex = text.IndexOf('\r');

        if (crlfIndex >= 0 && (lfIndex < 0 || crlfIndex <= lfIndex) && (crIndex < 0 || crlfIndex <= crIndex))
            return "\r\n";

        if (lfIndex >= 0 && (crIndex < 0 || lfIndex < crIndex))
            return "\n";

        return crIndex >= 0 ? "\r" : Environment.NewLine;
    }

    private static string[] SplitLines(string text)
    {
        return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    }

    [GeneratedRegex(@"^\s*\[(?<name>[^\]]+)\]\s*(?:(?:[;#].*)|(?://.*))?$")]
    private static partial Regex SectionRegex();

    [GeneratedRegex(@"^(?<indent>\s*)[;#]\s*RA2IniEditor:\s*(?<reason>[^:]+):\s*(?<kv>.+)$")]
    private static partial Regex CoveredKeyValueRegex();
}
