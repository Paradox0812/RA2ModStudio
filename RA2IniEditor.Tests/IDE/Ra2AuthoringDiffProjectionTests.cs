using RA2IniEditor.IDE.AuthoringDiff;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AuthoringDiffProjectionTests
{
    private readonly Ra2AuthoringDiffProjectionBuilder _builder = new();

    [Fact]
    public void Replacement_ProjectsWholeChangedLineAndPreservesMixedNewlines()
    {
        const string source = "A\r\nB=1\r\nC\n";
        int start = source.IndexOf('1');
        const string candidate = "A\r\nB=22\r\nC\n";

        Ra2AuthoringDiffProjection result = _builder.Build(
            source,
            candidate,
            [Change(start, 1, "22")]);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(1, result.HunkCount);
        Assert.Equal(1, result.RemovedLineCount);
        Assert.Equal(1, result.AddedLineCount);
        Assert.Contains(result.Rows, row => row.Kind == Ra2AuthoringDiffRowKind.Removed && row.Text == "B=1");
        Assert.Contains(result.Rows, row => row.Kind == Ra2AuthoringDiffRowKind.Added && row.Text == "B=22");
    }

    [Fact]
    public void InsertAtStartAndDeleteWholeLine_ProjectExpectedSides()
    {
        Ra2AuthoringDiffProjection inserted = _builder.Build(
            "[A]\nX=1",
            "; generated\n[A]\nX=1",
            [Change(0, 0, "; generated\n")]);
        Ra2AuthoringDiffProjection deleted = _builder.Build(
            "A\nB\nC",
            "A\nC",
            [Change(2, 2, string.Empty)]);

        Assert.True(inserted.Succeeded, inserted.Message);
        Assert.Equal(1, inserted.AddedLineCount);
        Assert.Equal(0, inserted.RemovedLineCount);
        Assert.Contains(inserted.Rows, row => row.Kind == Ra2AuthoringDiffRowKind.Added && row.Text == "; generated");
        Assert.True(deleted.Succeeded, deleted.Message);
        Assert.Equal(0, deleted.AddedLineCount);
        Assert.Equal(1, deleted.RemovedLineCount);
        Assert.Contains(deleted.Rows, row => row.Kind == Ra2AuthoringDiffRowKind.Removed && row.Text == "B");
    }

    [Fact]
    public void DistantChanges_ProduceStableOrderedHunks()
    {
        string source = string.Join('\n', Enumerable.Range(0, 40).Select(index => $"K{index}=0"));
        int first = source.IndexOf("K2=0", StringComparison.Ordinal) + 3;
        int second = source.IndexOf("K30=0", StringComparison.Ordinal) + 4;
        string candidate = source.Remove(second, 1).Insert(second, "2").Remove(first, 1).Insert(first, "1");

        Ra2AuthoringDiffProjection result = _builder.Build(
            source,
            candidate,
            [Change(second, 1, "2"), Change(first, 1, "1")]);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(2, result.HunkCount);
        Ra2AuthoringDiffRow[] headers = result.Rows.Where(row => row.Kind == Ra2AuthoringDiffRowKind.HunkHeader).ToArray();
        Assert.Equal(2, headers.Length);
        Assert.Contains("-1,", headers[0].Text, StringComparison.Ordinal);
        Assert.Contains("-28,", headers[1].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void CandidateMismatchAndOverlappingChanges_AreRejected()
    {
        Ra2AuthoringDiffProjection mismatch = _builder.Build(
            "abc",
            "axc",
            [Change(1, 1, "z")]);
        Ra2AuthoringDiffProjection overlap = _builder.Build(
            "abcd",
            "aXYd",
            [Change(1, 2, "X"), Change(2, 1, "Y")]);

        Assert.Equal(Ra2AuthoringDiffFailureKind.InvalidPreview, mismatch.FailureKind);
        Assert.Equal(Ra2AuthoringDiffFailureKind.InvalidPreview, overlap.FailureKind);
    }

    [Fact]
    public void CancellationAndResourceLimits_ReturnTypedFailures()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Ra2AuthoringDiffProjection canceled = _builder.Build(
            "a",
            "b",
            [Change(0, 1, "b")],
            cancellation.Token);
        string oversized = new('a', Ra2AuthoringDiffProjectionBuilder.MaximumInputCharacters + 1);
        Ra2AuthoringDiffProjection tooLarge = _builder.Build(
            oversized,
            oversized + "b",
            [Change(oversized.Length, 0, "b")]);

        Assert.Equal(Ra2AuthoringDiffFailureKind.Canceled, canceled.FailureKind);
        Assert.Equal(Ra2AuthoringDiffFailureKind.TooLarge, tooLarge.FailureKind);
    }

    [Fact]
    public void VisualRowLimit_ReturnsTypedFailureWithoutUnboundedProjection()
    {
        string candidate = string.Join('\n', Enumerable.Repeat("added", Ra2AuthoringDiffProjectionBuilder.MaximumVisualRows + 1));

        Ra2AuthoringDiffProjection result = _builder.Build(
            string.Empty,
            candidate,
            [Change(0, 0, candidate)]);

        Assert.Equal(Ra2AuthoringDiffFailureKind.ResultLimitExceeded, result.FailureKind);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void InputLineAndHunkLimits_ReturnTypedFailures()
    {
        string tooManyLines = string.Join(
            '\n',
            Enumerable.Repeat("x", Ra2AuthoringDiffProjectionBuilder.MaximumInputLines + 1));
        Ra2AuthoringDiffProjection lineLimited = _builder.Build(
            string.Empty,
            tooManyLines,
            [Change(0, 0, tooManyLines)]);

        const int hunkCount = Ra2AuthoringDiffProjectionBuilder.MaximumHunks + 1;
        string source = string.Concat(Enumerable.Repeat("0\n", hunkCount * 8));
        char[] candidateCharacters = source.ToCharArray();
        List<Ra2AutomationTextChange> changes = new(hunkCount);
        for (int index = 0; index < hunkCount; index++)
        {
            int offset = index * 16;
            candidateCharacters[offset] = '1';
            changes.Add(Change(offset, 1, "1"));
        }
        Ra2AuthoringDiffProjection hunkLimited = _builder.Build(
            source,
            new string(candidateCharacters),
            changes);

        Assert.Equal(Ra2AuthoringDiffFailureKind.TooLarge, lineLimited.FailureKind);
        Assert.Equal(Ra2AuthoringDiffFailureKind.ResultLimitExceeded, hunkLimited.FailureKind);
        Assert.Empty(hunkLimited.Rows);
    }

    private static Ra2AutomationTextChange Change(int start, int length, string newText)
        => new(new Ra2AutomationTextSpan(start, length), newText, "test");
}
