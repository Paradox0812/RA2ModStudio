namespace RA2IniEditor.Core;

/// <summary>第一版基础校验：重复 Section、重复 Key、空 Key。</summary>
public static class IniValidator
{
    public static List<IniIssue> Validate(IniDocument document)
    {
        var issues = new List<IniIssue>();
        issues.AddRange(document.ParseIssues);
        AppendDuplicateIssues(document, issues);
        return issues.OrderBy(issue => issue.LineNumber).ToList();
    }

    public static void AppendBasicIssues(IniDocument document)
    {
        AppendDuplicateIssues(document, document.Issues);
    }

    private static void AppendDuplicateIssues(IniDocument document, ICollection<IniIssue> issues)
    {
        var sectionMap = new Dictionary<string, IniSection>(StringComparer.OrdinalIgnoreCase);

        foreach (IniSection section in document.Sections)
        {
            if (string.IsNullOrWhiteSpace(section.Name))
            {
                issues.Add(new IniIssue(IniIssueSeverity.Warning, section.HeaderLine.LineNumber, "Section 名称为空。", sectionName: section.Name));
                continue;
            }

            if (sectionMap.TryGetValue(section.Name, out IniSection? firstSection))
            {
                issues.Add(new IniIssue(IniIssueSeverity.Warning, section.HeaderLine.LineNumber, $"重复 Section '[{section.Name}]'，首次出现于第 {firstSection.HeaderLine.LineNumber} 行。", sectionName: section.Name));
            }
            else
            {
                sectionMap.Add(section.Name, section);
            }

            AppendDuplicateKeyIssues(section, issues);
        }
    }

    private static void AppendDuplicateKeyIssues(IniSection section, ICollection<IniIssue> issues)
    {
        var keyMap = new Dictionary<string, IniKeyValueLine>(StringComparer.OrdinalIgnoreCase);

        foreach (IniKeyValueLine keyValue in section.KeyValues)
        {
            if (string.IsNullOrWhiteSpace(keyValue.Key))
            {
                issues.Add(new IniIssue(IniIssueSeverity.Warning, keyValue.LineNumber, $"Section '[{section.Name}]' 中存在空 Key。", sectionName: section.Name, key: keyValue.Key));
                continue;
            }

            if (keyMap.TryGetValue(keyValue.Key, out IniKeyValueLine? firstLine))
            {
                issues.Add(new IniIssue(IniIssueSeverity.Warning, keyValue.LineNumber, $"Section '[{section.Name}]' 中重复 Key '{keyValue.Key}'，首次出现于第 {firstLine.LineNumber} 行。", sectionName: section.Name, key: keyValue.Key));
            }
            else
            {
                keyMap.Add(keyValue.Key, keyValue);
            }
        }
    }
}
