using System.Text;
using System.IO;
using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.AuthoringDiff;

internal enum Ra2AuthoringDiffFailureKind
{
    None = 0,
    TooLarge,
    ResultLimitExceeded,
    Canceled,
    InvalidPreview
}

internal enum Ra2AuthoringDiffRowKind
{
    Context = 0,
    Added,
    Removed,
    HunkHeader,
    FileHeader
}

internal sealed record Ra2AuthoringDiffRow(
    Ra2AuthoringDiffRowKind Kind,
    int? OldLineNumber,
    int? NewLineNumber,
    string Marker,
    string Text)
{
    public bool IsFileHeader => Kind == Ra2AuthoringDiffRowKind.FileHeader;

    public string FileHeaderName
        => IsFileHeader ? Text.Split(" — ", 2, StringSplitOptions.None)[0] : string.Empty;

    public string FileHeaderPath
    {
        get
        {
            if (!IsFileHeader)
                return string.Empty;
            string[] parts = Text.Split(" — ", 2, StringSplitOptions.None);
            return parts.Length == 2 ? parts[1] : string.Empty;
        }
    }
}

internal sealed record Ra2AuthoringMappedChange(
    Ra2AutomationTextSpan SourceSpan,
    Ra2AutomationTextSpan CandidateSpan,
    int SourceStartLine,
    int SourceEndLine,
    int CandidateStartLine,
    int CandidateEndLine,
    int RemovedLineCount);

internal sealed class Ra2AuthoringDiffProjection
{
    private Ra2AuthoringDiffProjection(
        Ra2AuthoringDiffFailureKind failureKind,
        string message,
        IReadOnlyList<Ra2AuthoringDiffRow> rows,
        int addedLineCount,
        int removedLineCount,
        int hunkCount)
    {
        Succeeded = failureKind == Ra2AuthoringDiffFailureKind.None;
        FailureKind = failureKind;
        Message = message;
        Rows = rows;
        AddedLineCount = addedLineCount;
        RemovedLineCount = removedLineCount;
        HunkCount = hunkCount;
    }

    public bool Succeeded { get; }
    public Ra2AuthoringDiffFailureKind FailureKind { get; }
    public string Message { get; }
    public IReadOnlyList<Ra2AuthoringDiffRow> Rows { get; }
    public int AddedLineCount { get; }
    public int RemovedLineCount { get; }
    public int HunkCount { get; }

    public static Ra2AuthoringDiffProjection Success(
        IReadOnlyList<Ra2AuthoringDiffRow> rows,
        int added,
        int removed,
        int hunks)
        => new(Ra2AuthoringDiffFailureKind.None, "差异预览已生成。", rows, added, removed, hunks);

    public static Ra2AuthoringDiffProjection Failure(Ra2AuthoringDiffFailureKind kind, string message)
        => new(kind, message, [], 0, 0, 0);
}

internal sealed class Ra2AuthoringDiffProjectionBuilder
{
    public const int MaximumInputCharacters = 8 * 1024 * 1024;
    public const int MaximumInputLines = 200_000;
    public const int MaximumVisualRows = 20_000;
    public const int MaximumHunks = 2_000;
    private const int ContextLineCount = 3;

    public Ra2AuthoringDiffProjection Build(
        Ra2IniEditPreview preview,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preview);
        if (!preview.Succeeded || preview.CandidateText is null || preview.ChangeSet is null ||
            preview.AutomationResult.Changes.Count == 0)
        {
            return Ra2AuthoringDiffProjection.Failure(
                Ra2AuthoringDiffFailureKind.InvalidPreview,
                "当前提案没有可投影的差异。");
        }

        return Build(
            preview.Snapshot.Text,
            preview.CandidateText,
            preview.AutomationResult.Changes,
            cancellationToken);
    }

    public Ra2AuthoringDiffProjection Build(
        Ra2ProjectEditPreview preview,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preview);
        if (!preview.Succeeded || preview.DocumentPreviews.Count == 0)
        {
            return Ra2AuthoringDiffProjection.Failure(
                Ra2AuthoringDiffFailureKind.InvalidPreview,
                "当前项目提案没有可投影的差异。");
        }

        try
        {
            List<Ra2AuthoringDiffRow> rows = [];
            int added = 0;
            int removed = 0;
            int hunks = 0;
            foreach (Ra2AutomationEditPreviewResult documentPreview in preview.DocumentPreviews)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Ra2AutomationDocumentSnapshot sourceDocument = preview.Snapshot.Documents.Single(
                    document => document.DocumentId == documentPreview.DocumentId);
                if (documentPreview.CandidateText is null)
                    return Ra2AuthoringDiffProjection.Failure(Ra2AuthoringDiffFailureKind.InvalidPreview, "项目提案包含无效文档差异。");

                Ra2AuthoringDiffProjection documentProjection = Build(
                    sourceDocument.Text,
                    documentPreview.CandidateText,
                    documentPreview.Changes,
                    cancellationToken);
                if (!documentProjection.Succeeded)
                    return Ra2AuthoringDiffProjection.Failure(documentProjection.FailureKind, documentProjection.Message);

                if (hunks + documentProjection.HunkCount > MaximumHunks ||
                    rows.Count + 1 + documentProjection.Rows.Count > MaximumVisualRows)
                {
                    return ResultLimit("项目差异超过全局可视行或差异块上限。");
                }

                string relativePath = Path.GetRelativePath(preview.Snapshot.ProjectRootPath, documentPreview.FilePath);
                rows.Add(new Ra2AuthoringDiffRow(
                    Ra2AuthoringDiffRowKind.FileHeader,
                    null,
                    null,
                    string.Empty,
                    $"{Path.GetFileName(documentPreview.FilePath)} — {relativePath}"));
                rows.AddRange(documentProjection.Rows);
                added = checked(added + documentProjection.AddedLineCount);
                removed = checked(removed + documentProjection.RemovedLineCount);
                hunks = checked(hunks + documentProjection.HunkCount);
            }

            return Ra2AuthoringDiffProjection.Success(Array.AsReadOnly(rows.ToArray()), added, removed, hunks);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Ra2AuthoringDiffProjection.Failure(Ra2AuthoringDiffFailureKind.Canceled, "项目差异预览已取消。");
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        {
            return Ra2AuthoringDiffProjection.Failure(Ra2AuthoringDiffFailureKind.InvalidPreview, "无法生成项目差异预览。");
        }
    }

    internal Ra2AuthoringDiffProjection Build(
        string source,
        string candidate,
        IReadOnlyList<Ra2AutomationTextChange> changes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(changes);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (changes.Count == 0)
            {
                return Ra2AuthoringDiffProjection.Failure(
                    Ra2AuthoringDiffFailureKind.InvalidPreview,
                    "当前提案没有可投影的差异。");
            }

            if (!TryMapChanges(source, candidate, changes, cancellationToken, out IReadOnlyList<Ra2AuthoringMappedChange> mappedChanges, out Ra2AuthoringDiffProjection? mappingFailure))
                return mappingFailure!;

            LineMap oldMap = LineMap.Create(source, cancellationToken);
            LineMap newMap = LineMap.Create(candidate, cancellationToken);
            List<Region> regions = mappedChanges.Select(change => new Region(
                change.SourceStartLine,
                change.SourceEndLine,
                change.CandidateStartLine,
                change.CandidateEndLine)).ToList();

            List<RegionGroup> groups = Group(regions);
            if (groups.Count > MaximumHunks)
                return ResultLimit("差异块超过 2,000 个上限。");

            List<Ra2AuthoringDiffRow> rows = [];
            int added = 0;
            int removed = 0;
            foreach ((RegionGroup group, int groupIndex) in groups.Select((value, index) => (value, index)))
            {
                if ((groupIndex & 31) == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                int before = Math.Min(ContextLineCount, Math.Min(group.First.OldStart, group.First.NewStart));
                int oldStart = group.First.OldStart - before;
                int newStart = group.First.NewStart - before;
                int after = Math.Min(
                    ContextLineCount,
                    Math.Min(oldMap.Count - group.Last.OldEnd, newMap.Count - group.Last.NewEnd));
                int oldCount = group.Last.OldEnd - oldStart + after;
                int newCount = group.Last.NewEnd - newStart + after;
                Add(rows, new Ra2AuthoringDiffRow(
                    Ra2AuthoringDiffRowKind.HunkHeader,
                    null,
                    null,
                    string.Empty,
                    $"@@ -{oldStart + 1},{oldCount} +{newStart + 1},{newCount} @@"));

                AddPairedContext(rows, oldMap, newMap, oldStart, newStart, before);
                for (int regionIndex = 0; regionIndex < group.Regions.Count; regionIndex++)
                {
                    Region region = group.Regions[regionIndex];
                    for (int line = region.OldStart; line < region.OldEnd; line++)
                    {
                        Add(rows, new Ra2AuthoringDiffRow(Ra2AuthoringDiffRowKind.Removed, line + 1, null, "−", oldMap[line]));
                        removed++;
                    }
                    for (int line = region.NewStart; line < region.NewEnd; line++)
                    {
                        Add(rows, new Ra2AuthoringDiffRow(Ra2AuthoringDiffRowKind.Added, null, line + 1, "+", newMap[line]));
                        added++;
                    }

                    if (regionIndex + 1 < group.Regions.Count)
                    {
                        Region next = group.Regions[regionIndex + 1];
                        int gap = Math.Min(next.OldStart - region.OldEnd, next.NewStart - region.NewEnd);
                        AddPairedContext(rows, oldMap, newMap, region.OldEnd, region.NewEnd, Math.Max(0, gap));
                    }
                }
                AddPairedContext(rows, oldMap, newMap, group.Last.OldEnd, group.Last.NewEnd, after);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return Ra2AuthoringDiffProjection.Success(Array.AsReadOnly(rows.ToArray()), added, removed, groups.Count);
        }
        catch (ResultLimitException)
        {
            return ResultLimit("差异可视行超过 20,000 行上限。");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Ra2AuthoringDiffProjection.Failure(Ra2AuthoringDiffFailureKind.Canceled, "差异预览已取消。");
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        {
            return Ra2AuthoringDiffProjection.Failure(Ra2AuthoringDiffFailureKind.InvalidPreview, "无法生成差异预览。");
        }
    }

    private static int GetStartLine(LineMap map, int offset) => map.LineAt(offset);

    internal static bool TryMapChanges(
        string source,
        string candidate,
        IReadOnlyList<Ra2AutomationTextChange> changes,
        CancellationToken cancellationToken,
        out IReadOnlyList<Ra2AuthoringMappedChange> mappedChanges,
        out Ra2AuthoringDiffProjection? failure)
    {
        mappedChanges = [];
        failure = null;
        if (source.Length > MaximumInputCharacters || candidate.Length > MaximumInputCharacters)
        {
            failure = TooLarge("差异输入超过 8 MiB 上限。");
            return false;
        }

        Ra2AutomationTextChange[] orderedChanges = changes.OrderBy(change => change.Span.Start).ToArray();
        if (!ChangesProduceCandidate(source, candidate, orderedChanges, cancellationToken))
        {
            failure = Ra2AuthoringDiffProjection.Failure(
                Ra2AuthoringDiffFailureKind.InvalidPreview,
                "差异变更与候选文本不一致。");
            return false;
        }

        LineMap oldMap = LineMap.Create(source, cancellationToken);
        LineMap newMap = LineMap.Create(candidate, cancellationToken);
        if (oldMap.Count > MaximumInputLines || newMap.Count > MaximumInputLines)
        {
            failure = TooLarge("差异输入超过 200,000 行上限。");
            return false;
        }

        List<Ra2AuthoringMappedChange> result = new(orderedChanges.Length);
        int delta = 0;
        int previousEnd = 0;
        foreach ((Ra2AutomationTextChange change, int index) in orderedChanges.Select((value, index) => (value, index)))
        {
            if ((index & 31) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            if (change.Span.Start < previousEnd || change.Span.End > source.Length)
            {
                failure = Ra2AuthoringDiffProjection.Failure(
                    Ra2AuthoringDiffFailureKind.InvalidPreview,
                    "差异变更范围重叠或越界。");
                return false;
            }

            int candidateStart = checked(change.Span.Start + delta);
            int oldStartLine = GetStartLine(oldMap, change.Span.Start);
            int oldEndLine = GetEndLine(oldMap, change.Span.Start, change.Span.Length);
            int newStartLine = GetStartLine(newMap, candidateStart);
            int newEndLine = GetEndLine(newMap, candidateStart, change.NewText.Length);
            result.Add(new Ra2AuthoringMappedChange(
                change.Span,
                new Ra2AutomationTextSpan(candidateStart, change.NewText.Length),
                oldStartLine,
                oldEndLine,
                newStartLine,
                newEndLine,
                Math.Max(0, oldEndLine - oldStartLine)));
            previousEnd = change.Span.End;
            delta = checked(delta + change.NewText.Length - change.Span.Length);
        }

        mappedChanges = Array.AsReadOnly(result.ToArray());
        return true;
    }

    private static bool ChangesProduceCandidate(
        string source,
        string candidate,
        IReadOnlyList<Ra2AutomationTextChange> changes,
        CancellationToken cancellationToken)
    {
        StringBuilder builder = new(candidate.Length);
        int cursor = 0;
        for (int index = 0; index < changes.Count; index++)
        {
            if ((index & 31) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            Ra2AutomationTextChange change = changes[index];
            if (change.Span.Start < cursor || change.Span.End > source.Length)
                return false;
            builder.Append(source, cursor, change.Span.Start - cursor);
            builder.Append(change.NewText);
            cursor = change.Span.End;
            if (builder.Length > candidate.Length)
                return false;
        }
        builder.Append(source, cursor, source.Length - cursor);
        return builder.Length == candidate.Length &&
               builder.ToString().Equals(candidate, StringComparison.Ordinal);
    }

    private static int GetEndLine(LineMap map, int start, int length)
    {
        int startLine = map.LineAt(start);
        if (length == 0)
            return map.IsLineStart(start) ? startLine : Math.Min(map.Count, startLine + 1);
        int end = start + length;
        int endLine = map.LineAt(end);
        return map.IsLineStart(end) ? endLine : Math.Min(map.Count, endLine + 1);
    }

    private static List<RegionGroup> Group(IReadOnlyList<Region> regions)
    {
        List<RegionGroup> groups = [];
        foreach (Region region in regions)
        {
            if (groups.Count == 0 ||
                (region.OldStart - groups[^1].Last.OldEnd > ContextLineCount * 2 &&
                 region.NewStart - groups[^1].Last.NewEnd > ContextLineCount * 2))
            {
                groups.Add(new RegionGroup(region));
            }
            else
            {
                groups[^1].Regions.Add(region);
            }
        }
        return groups;
    }

    private static void AddPairedContext(
        List<Ra2AuthoringDiffRow> rows,
        LineMap oldMap,
        LineMap newMap,
        int oldStart,
        int newStart,
        int count)
    {
        for (int index = 0; index < count; index++)
        {
            Add(rows, new Ra2AuthoringDiffRow(
                Ra2AuthoringDiffRowKind.Context,
                oldStart + index + 1,
                newStart + index + 1,
                string.Empty,
                newMap[newStart + index]));
        }
    }

    private static void Add(List<Ra2AuthoringDiffRow> rows, Ra2AuthoringDiffRow row)
    {
        if (rows.Count >= MaximumVisualRows)
            throw new ResultLimitException();
        rows.Add(row);
    }

    private static Ra2AuthoringDiffProjection TooLarge(string message)
        => Ra2AuthoringDiffProjection.Failure(Ra2AuthoringDiffFailureKind.TooLarge, message);
    private static Ra2AuthoringDiffProjection ResultLimit(string message)
        => Ra2AuthoringDiffProjection.Failure(Ra2AuthoringDiffFailureKind.ResultLimitExceeded, message);

    private readonly record struct Region(int OldStart, int OldEnd, int NewStart, int NewEnd);

    private sealed class RegionGroup
    {
        public RegionGroup(Region first) => Regions = [first];
        public List<Region> Regions { get; }
        public Region First => Regions[0];
        public Region Last => Regions[^1];
    }

    private sealed class LineMap
    {
        private readonly string[] _lines;
        private readonly int[] _starts;
        private readonly int _textLength;

        private LineMap(string[] lines, int[] starts, int textLength)
        {
            _lines = lines;
            _starts = starts;
            _textLength = textLength;
        }

        public int Count => _lines.Length;
        public string this[int index] => _lines[index];

        public static LineMap Create(string text, CancellationToken cancellationToken)
        {
            List<string> lines = [];
            List<int> starts = [0];
            int lineStart = 0;
            for (int index = 0; index < text.Length; index++)
            {
                if ((index & 8191) == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                if (text[index] is not ('\r' or '\n'))
                    continue;
                lines.Add(text[lineStart..index]);
                if (text[index] == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                    index++;
                lineStart = index + 1;
                starts.Add(lineStart);
                if (lines.Count > MaximumInputLines)
                    break;
            }
            lines.Add(text[lineStart..]);
            return new LineMap(lines.ToArray(), starts.Take(lines.Count).ToArray(), text.Length);
        }

        public int LineAt(int offset)
        {
            int bounded = Math.Clamp(offset, 0, _textLength);
            int index = Array.BinarySearch(_starts, bounded);
            return index >= 0 ? index : ~index - 1;
        }

        public bool IsLineStart(int offset)
            => Array.BinarySearch(_starts, Math.Clamp(offset, 0, _textLength)) >= 0;
    }

    private sealed class ResultLimitException : Exception;
}
