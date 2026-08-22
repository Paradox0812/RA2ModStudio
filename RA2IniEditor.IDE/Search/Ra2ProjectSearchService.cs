using System.IO;
using System.IO.Enumeration;
using System.Text.RegularExpressions;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;

namespace RA2IniEditor.IDE.Search;

internal sealed class Ra2ProjectSearchService
{
    internal const int MaximumResultCount = 10_000;
    private const int MaximumPreviewLength = 240;
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(500);
    private readonly ReadonlyIniContentService _contentService;

    public Ra2ProjectSearchService(ReadonlyIniContentService contentService)
    {
        _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
    }

    public Ra2SearchExecutionResult Search(
        Ra2SearchOptions options,
        IReadOnlyList<ReadonlyIniFileDescriptor> projectFiles,
        string? currentFilePath,
        string? currentEditorText,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(projectFiles);

        if (string.IsNullOrEmpty(options.Query))
            return Ra2SearchExecutionResult.Failed(Ra2SearchFailureKind.EmptyQuery, "请输入查找内容。");

        if (!TryParsePatterns(options.FilePattern, out IReadOnlyList<string> patterns))
            return Ra2SearchExecutionResult.Failed(Ra2SearchFailureKind.InvalidPattern, "文件类型包含无效路径或模式。");

        Regex? regex = null;
        if (options.UseRegex)
        {
            try
            {
                regex = CreateRegex(options);
            }
            catch (ArgumentException ex)
            {
                return Ra2SearchExecutionResult.Failed(
                    Ra2SearchFailureKind.InvalidRegex,
                    $"正则表达式无效：{ex.Message}");
            }
        }

        IReadOnlyList<ReadonlyIniFileDescriptor> files = SelectFiles(
            options.Scope,
            projectFiles,
            currentFilePath,
            patterns);
        if (files.Count == 0)
            return Ra2SearchExecutionResult.Failed(Ra2SearchFailureKind.NoFiles, "当前范围内没有可搜索的项目文件。");

        List<Ra2SearchMatch> matches = [];
        int scannedFileCount = 0;
        int skippedFileCount = 0;
        bool isTruncated = false;
        bool hadRegexTimeout = false;

        foreach (ReadonlyIniFileDescriptor file in files)
        {
            if (cancellationToken.IsCancellationRequested)
                return Ra2SearchExecutionResult.Failed(Ra2SearchFailureKind.Canceled, "查找已取消。");

            if (!TryGetSourceText(file, currentFilePath, currentEditorText, out string text))
            {
                skippedFileCount++;
                continue;
            }

            scannedFileCount++;
            try
            {
                FindMatches(file, text, options, regex, matches, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return Ra2SearchExecutionResult.Failed(Ra2SearchFailureKind.Canceled, "查找已取消。");
            }
            catch (RegexMatchTimeoutException)
            {
                skippedFileCount++;
                hadRegexTimeout = true;
            }

            if (matches.Count < MaximumResultCount)
                continue;

            isTruncated = true;
            if (matches.Count > MaximumResultCount)
                matches.RemoveRange(MaximumResultCount, matches.Count - MaximumResultCount);
            break;
        }

        string statusText = BuildStatusText(matches.Count, scannedFileCount, skippedFileCount, isTruncated, hadRegexTimeout);
        return Ra2SearchExecutionResult.Completed(
            matches,
            statusText,
            scannedFileCount,
            skippedFileCount,
            isTruncated,
            hadRegexTimeout ? Ra2SearchFailureKind.RegexTimeout : Ra2SearchFailureKind.None);
    }

    private bool TryGetSourceText(
        ReadonlyIniFileDescriptor file,
        string? currentFilePath,
        string? currentEditorText,
        out string text)
    {
        if (!string.IsNullOrWhiteSpace(currentFilePath) &&
            string.Equals(file.FilePath, currentFilePath, StringComparison.OrdinalIgnoreCase) &&
            currentEditorText is not null)
        {
            text = currentEditorText;
            return true;
        }

        ReadonlyIniContentResult result = _contentService.ReadFileReadonly(file);
        if (result.ErrorMessage is not null || result.IsLargeFileDeferred)
        {
            text = string.Empty;
            return false;
        }

        text = result.Text;
        return true;
    }

    private static IReadOnlyList<ReadonlyIniFileDescriptor> SelectFiles(
        Ra2SearchScope scope,
        IReadOnlyList<ReadonlyIniFileDescriptor> projectFiles,
        string? currentFilePath,
        IReadOnlyList<string> patterns)
    {
        IEnumerable<ReadonlyIniFileDescriptor> files = projectFiles.Where(
            file => patterns.Any(pattern => FileSystemName.MatchesSimpleExpression(
                pattern,
                file.FileName,
                ignoreCase: true)));

        if (scope == Ra2SearchScope.CurrentFile)
        {
            if (string.IsNullOrWhiteSpace(currentFilePath))
                return [];

            files = files.Where(file => string.Equals(
                file.FilePath,
                currentFilePath,
                StringComparison.OrdinalIgnoreCase));
        }

        return files.ToArray();
    }

    private static bool TryParsePatterns(string? patternText, out IReadOnlyList<string> patterns)
    {
        string normalized = string.IsNullOrWhiteSpace(patternText) ? "*.ini" : patternText;
        string[] split = normalized
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length == 0 ||
            split.Any(pattern =>
                Path.IsPathRooted(pattern) ||
                pattern.Contains(Path.DirectorySeparatorChar) ||
                pattern.Contains(Path.AltDirectorySeparatorChar)))
        {
            patterns = [];
            return false;
        }

        patterns = split;
        return true;
    }

    private static void FindMatches(
        ReadonlyIniFileDescriptor file,
        string text,
        Ra2SearchOptions options,
        Regex? regex,
        List<Ra2SearchMatch> destination,
        CancellationToken cancellationToken)
    {
        LineIndex lineIndex = new(text);
        IReadOnlyList<(int Index, int Length)> textMatches = FindTextMatches(
            text,
            options,
            MaximumResultCount - destination.Count,
            cancellationToken,
            regex);
        foreach ((int index, int length) in textMatches)
        {
            destination.Add(CreateMatch(file, text, lineIndex, index, length));
            if (destination.Count >= MaximumResultCount)
                return;
        }
    }

    internal static IReadOnlyList<(int Index, int Length)> FindTextMatches(
        string text,
        Ra2SearchOptions options,
        int maximumCount,
        CancellationToken cancellationToken = default,
        Regex? preparedRegex = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(options);
        if (maximumCount <= 0 || string.IsNullOrEmpty(options.Query))
            return [];

        List<(int Index, int Length)> matches = [];
        Regex? regex = preparedRegex ?? (options.UseRegex ? CreateRegex(options) : null);
        if (regex is not null)
        {
            foreach (Match match in regex.Matches(text))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (options.IsWholeWord && !IsWholeWord(text, match.Index, match.Length))
                    continue;

                matches.Add((match.Index, match.Length));
                if (matches.Count >= maximumCount)
                    break;
            }

            return matches;
        }

        StringComparison comparison = options.IsCaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        int searchIndex = 0;
        while (searchIndex <= text.Length - options.Query.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int index = text.IndexOf(options.Query, searchIndex, comparison);
            if (index < 0)
                return matches;

            if (!options.IsWholeWord || IsWholeWord(text, index, options.Query.Length))
            {
                matches.Add((index, options.Query.Length));
                if (matches.Count >= maximumCount)
                    break;
            }

            searchIndex = index + Math.Max(1, options.Query.Length);
        }

        return matches;
    }

    internal static Regex CreateRegex(Ra2SearchOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        RegexOptions regexOptions = RegexOptions.CultureInvariant | RegexOptions.Multiline;
        if (!options.IsCaseSensitive)
            regexOptions |= RegexOptions.IgnoreCase;
        return new Regex(options.Query, regexOptions, RegexTimeout);
    }

    private static Ra2SearchMatch CreateMatch(
        ReadonlyIniFileDescriptor file,
        string text,
        LineIndex lineIndex,
        int characterIndex,
        int length)
    {
        (int lineNumber, int columnNumber, int lineStart, int lineLength, string sectionName) =
            lineIndex.Resolve(characterIndex);
        string preview = text.Substring(lineStart, lineLength).Trim();
        if (preview.Length > MaximumPreviewLength)
            preview = $"{preview[..MaximumPreviewLength]}…";

        return new Ra2SearchMatch(
            file.FileName,
            file.FilePath,
            lineNumber,
            columnNumber,
            sectionName,
            preview,
            text.Substring(characterIndex, length),
            characterIndex,
            length);
    }

    private static bool IsWholeWord(string text, int start, int length)
    {
        bool leftBoundary = start == 0 || !IsWordCharacter(text[start - 1]);
        int end = start + length;
        bool rightBoundary = end >= text.Length || !IsWordCharacter(text[end]);
        return leftBoundary && rightBoundary;
    }

    private static bool IsWordCharacter(char value) => char.IsLetterOrDigit(value) || value == '_';

    private static string BuildStatusText(
        int matchCount,
        int scannedFileCount,
        int skippedFileCount,
        bool isTruncated,
        bool hadRegexTimeout)
    {
        string status = $"找到 {matchCount} 项，已扫描 {scannedFileCount} 个文件";
        if (skippedFileCount > 0)
            status += $"，跳过 {skippedFileCount} 个文件";
        if (hadRegexTimeout)
            status += "（部分正则匹配超时）";
        if (isTruncated)
            status += $"（结果已限制为 {MaximumResultCount} 项）";
        return $"{status}。";
    }

    private sealed class LineIndex
    {
        private readonly string _text;
        private readonly List<int> _lineStarts = [0];
        private readonly List<string> _sections = [string.Empty];

        public LineIndex(string text)
        {
            _text = text;
            string currentSection = string.Empty;
            int lineStart = 0;
            for (int index = 0; index <= text.Length; index++)
            {
                bool isEnd = index == text.Length;
                bool isNewLine = !isEnd && text[index] is '\r' or '\n';
                if (!isEnd && !isNewLine)
                    continue;

                string line = text[lineStart..index].Trim();
                if (line.Length >= 2 && line[0] == '[' && line[^1] == ']')
                    currentSection = line[1..^1].Trim();
                _sections[^1] = currentSection;

                if (isEnd)
                    break;

                if (text[index] == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                    index++;
                lineStart = index + 1;
                _lineStarts.Add(lineStart);
                _sections.Add(currentSection);
            }
        }

        public (int LineNumber, int ColumnNumber, int LineStart, int LineLength, string SectionName) Resolve(int offset)
        {
            int index = _lineStarts.BinarySearch(offset);
            if (index < 0)
                index = ~index - 1;
            index = Math.Max(0, index);

            int lineStart = _lineStarts[index];
            int lineEnd = index + 1 < _lineStarts.Count ? _lineStarts[index + 1] : _text.Length;
            while (lineEnd > lineStart && _text[lineEnd - 1] is '\r' or '\n')
                lineEnd--;

            return (index + 1, offset - lineStart + 1, lineStart, lineEnd - lineStart, _sections[index]);
        }
    }
}
