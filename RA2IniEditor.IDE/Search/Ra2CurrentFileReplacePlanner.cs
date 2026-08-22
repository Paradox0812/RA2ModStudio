using System.Text.RegularExpressions;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Search;

internal sealed class Ra2CurrentFileReplacePlanner
{
    private const int MaximumReplacementCount = 10_000;

    public Ra2CurrentFileReplacePlan Plan(
        Ra2EditableDocumentSession? session,
        Ra2SearchOptions options,
        string replacementText)
    {
        ArgumentNullException.ThrowIfNull(options);
        replacementText ??= string.Empty;

        if (session is null ||
            session.DocumentState.State == Ra2EditorDocumentState.ReadOnlyPreview)
        {
            return Ra2CurrentFileReplacePlan.Failed(
                Ra2ReplaceFailureKind.NotEditable,
                "当前文件不可编辑，无法预览替换。");
        }

        if (options.Scope != Ra2SearchScope.CurrentFile)
        {
            return Ra2CurrentFileReplacePlan.Failed(
                Ra2ReplaceFailureKind.ProjectScopeNotSupported,
                "替换仅支持当前文件。");
        }

        if (string.IsNullOrEmpty(options.Query))
        {
            return Ra2CurrentFileReplacePlan.Failed(
                Ra2ReplaceFailureKind.EmptyQuery,
                "请输入查找内容。");
        }

        string originalText = session.DocumentState.CurrentText;
        Regex? regex = null;
        try
        {
            regex = options.UseRegex ? Ra2ProjectSearchService.CreateRegex(options) : null;
            IReadOnlyList<(int Index, int Length)> matches = Ra2ProjectSearchService.FindTextMatches(
                originalText,
                options,
                MaximumReplacementCount + 1,
                preparedRegex: regex);
            if (matches.Count == 0)
            {
                return Ra2CurrentFileReplacePlan.Failed(
                    Ra2ReplaceFailureKind.NoMatches,
                    "当前文件中没有可替换的匹配项。");
            }

            if (matches.Count > MaximumReplacementCount)
            {
                return Ra2CurrentFileReplacePlan.Failed(
                    Ra2ReplaceFailureKind.TooManyMatches,
                    $"替换项超过 {MaximumReplacementCount} 条安全上限。");
            }

            if (matches.Any(match => match.Length == 0))
            {
                return Ra2CurrentFileReplacePlan.Failed(
                    Ra2ReplaceFailureKind.ZeroLengthMatch,
                    "正则表达式产生了零长度匹配，已拒绝 Replace All。");
            }

            List<Ra2TextChange> changes = [];
            foreach ((int index, int length) in matches)
            {
                string newText = regex is null
                    ? replacementText
                    : ResolveRegexReplacement(regex, originalText, index, length, replacementText);
                string matchedText = originalText.Substring(index, length);
                if (string.Equals(matchedText, newText, StringComparison.Ordinal))
                    continue;

                changes.Add(new Ra2TextChange(
                    new Ra2TextSpan(index, length),
                    newText,
                    "Replace all in current file"));
            }

            if (changes.Count == 0)
            {
                return Ra2CurrentFileReplacePlan.Failed(
                    Ra2ReplaceFailureKind.NoChanges,
                    "替换文本与所有匹配内容相同，没有需要应用的更改。");
            }

            Ra2TextChangeSet changeSet = new(changes);
            string updatedText = changeSet.Apply(originalText);
            return Ra2CurrentFileReplacePlan.Succeeded(session, updatedText, changeSet, changes.Count);
        }
        catch (RegexMatchTimeoutException)
        {
            return Ra2CurrentFileReplacePlan.Failed(
                Ra2ReplaceFailureKind.RegexTimeout,
                "正则替换预览超时，请缩小表达式范围。");
        }
        catch (ArgumentException ex) when (options.UseRegex)
        {
            return Ra2CurrentFileReplacePlan.Failed(
                Ra2ReplaceFailureKind.InvalidRegex,
                $"正则表达式或替换文本无效：{ex.Message}");
        }
        catch (Exception ex)
        {
            return Ra2CurrentFileReplacePlan.Failed(
                Ra2ReplaceFailureKind.Unexpected,
                $"替换预览失败：{ex.Message}");
        }
    }

    private static string ResolveRegexReplacement(
        Regex regex,
        string originalText,
        int index,
        int length,
        string replacementText)
    {
        Match match = regex.Match(originalText, index);
        if (!match.Success || match.Index != index || match.Length != length)
            throw new InvalidOperationException("Regex match changed while building the replacement plan.");
        return match.Result(replacementText);
    }
}
