namespace RA2IniEditor.IDE.Search;

internal sealed class Ra2SearchExecutionResult
{
    private Ra2SearchExecutionResult(
        IReadOnlyList<Ra2SearchMatch> matches,
        Ra2SearchFailureKind failureKind,
        string statusText,
        int scannedFileCount,
        int skippedFileCount,
        bool isTruncated)
    {
        Matches = matches;
        FailureKind = failureKind;
        StatusText = statusText;
        ScannedFileCount = scannedFileCount;
        SkippedFileCount = skippedFileCount;
        IsTruncated = isTruncated;
    }

    public IReadOnlyList<Ra2SearchMatch> Matches { get; }

    public Ra2SearchFailureKind FailureKind { get; }

    public string StatusText { get; }

    public int ScannedFileCount { get; }

    public int SkippedFileCount { get; }

    public bool IsTruncated { get; }

    public static Ra2SearchExecutionResult Completed(
        IReadOnlyList<Ra2SearchMatch> matches,
        string statusText,
        int scannedFileCount,
        int skippedFileCount,
        bool isTruncated,
        Ra2SearchFailureKind warningKind = Ra2SearchFailureKind.None)
        => new(matches, warningKind, statusText, scannedFileCount, skippedFileCount, isTruncated);

    public static Ra2SearchExecutionResult Failed(Ra2SearchFailureKind failureKind, string statusText)
        => new([], failureKind, statusText, 0, 0, false);
}
