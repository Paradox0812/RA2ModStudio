using System.Text;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Search;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.Infrastructure.IO;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ProjectSearchServiceTests
{
    [Fact]
    public void Search_ProjectScopeReturnsStableFileAndCharacterOrder()
    {
        RecordingIniFileStore store = new(
            ("rulesmd.ini", "[E1]\nPrimary=Gun\nSecondary=Gun"),
            ("artmd.ini", "[E1]\nImage=Gun"));
        Ra2ProjectSearchService service = CreateService(store);

        Ra2SearchExecutionResult result = service.Search(
            Options("Gun"),
            Files("rulesmd.ini", "artmd.ini"),
            null,
            null);

        Assert.Equal(Ra2SearchFailureKind.None, result.FailureKind);
        Assert.Collection(
            result.Matches,
            match =>
            {
                Assert.Equal("rulesmd.ini", match.FileName);
                Assert.Equal(2, match.LineNumber);
                Assert.Equal("E1", match.SectionName);
            },
            match =>
            {
                Assert.Equal("rulesmd.ini", match.FileName);
                Assert.Equal(3, match.LineNumber);
            },
            match => Assert.Equal("artmd.ini", match.FileName));
    }

    [Fact]
    public void Search_CurrentMemoryTextOverridesDiskText()
    {
        RecordingIniFileStore store = new(("rulesmd.ini", "[E1]\nPrimary=OldGun"));
        Ra2ProjectSearchService service = CreateService(store);

        Ra2SearchExecutionResult result = service.Search(
            Options("NewGun", Ra2SearchScope.CurrentFile),
            Files("rulesmd.ini"),
            Path.GetFullPath("rulesmd.ini"),
            "[E1]\nPrimary=NewGun");

        Ra2SearchMatch match = Assert.Single(result.Matches);
        Assert.Equal(2, match.LineNumber);
        Assert.Equal(9, match.ColumnNumber);
        Assert.Empty(store.ReadPaths);
    }

    [Fact]
    public void Search_WholeWordDoesNotMatchIdentifierSuffix()
    {
        RecordingIniFileStore store = new(("rulesmd.ini", "Gun=1\nSuperGun=1\nGun_Elite=1"));
        Ra2ProjectSearchService service = CreateService(store);

        Ra2SearchExecutionResult result = service.Search(
            Options("Gun", isWholeWord: true),
            Files("rulesmd.ini"),
            null,
            null);

        Ra2SearchMatch match = Assert.Single(result.Matches);
        Assert.Equal(1, match.LineNumber);
    }

    [Fact]
    public void Search_RegexHonorsCaseOption()
    {
        RecordingIniFileStore store = new(("rulesmd.ini", "Primary=Gun\nSecondary=gun"));
        Ra2ProjectSearchService service = CreateService(store);

        Ra2SearchExecutionResult result = service.Search(
            Options(@"=G\w+", useRegex: true, isCaseSensitive: true),
            Files("rulesmd.ini"),
            null,
            null);

        Assert.Single(result.Matches);
    }

    [Fact]
    public void Search_InvalidRegexReturnsTypedFailure()
    {
        Ra2ProjectSearchService service = CreateService(new RecordingIniFileStore());

        Ra2SearchExecutionResult result = service.Search(
            Options("[", useRegex: true),
            Files("rulesmd.ini"),
            null,
            null);

        Assert.Equal(Ra2SearchFailureKind.InvalidRegex, result.FailureKind);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public void Search_PathLikePatternIsRejected()
    {
        Ra2ProjectSearchService service = CreateService(new RecordingIniFileStore());

        Ra2SearchExecutionResult result = service.Search(
            Options("Gun") with { FilePattern = @"sub\*.ini" },
            Files("rulesmd.ini"),
            null,
            null);

        Assert.Equal(Ra2SearchFailureKind.InvalidPattern, result.FailureKind);
    }

    [Fact]
    public void Search_ReadFailureAndDeferredLargeFileAreSkipped()
    {
        RecordingIniFileStore store = new(("rulesmd.ini", "Primary=Gun"))
        {
            ThrowForPath = Path.GetFullPath("artmd.ini")
        };
        Ra2ProjectSearchService service = CreateService(store);
        IReadOnlyList<ReadonlyIniFileDescriptor> files =
        [
            Descriptor("rulesmd.ini", 20),
            Descriptor("artmd.ini", 20),
            Descriptor("aimd.ini", ReadonlyIniContentService.VeryLargeFilePreviewThresholdBytes + 1)
        ];

        Ra2SearchExecutionResult result = service.Search(Options("Gun"), files, null, null);

        Assert.Single(result.Matches);
        Assert.Equal(1, result.ScannedFileCount);
        Assert.Equal(2, result.SkippedFileCount);
    }

    [Fact]
    public void Search_FilePatternOnlyFiltersCanonicalDescriptors()
    {
        RecordingIniFileStore store = new(
            ("rulesmd.ini", "Primary=Gun"),
            ("notes.txt", "Primary=Gun"));
        Ra2ProjectSearchService service = CreateService(store);

        Ra2SearchExecutionResult result = service.Search(
            Options("Gun") with { FilePattern = "*.ini" },
            Files("rulesmd.ini", "notes.txt"),
            null,
            null);

        Assert.Single(result.Matches);
        Assert.Equal(["rulesmd.ini"], store.ReadPaths.Select(Path.GetFileName));
    }

    private static Ra2SearchOptions Options(
        string query,
        Ra2SearchScope scope = Ra2SearchScope.Project,
        bool isCaseSensitive = false,
        bool isWholeWord = false,
        bool useRegex = false)
        => new(query, scope, isCaseSensitive, isWholeWord, useRegex, "*.ini");

    private static Ra2ProjectSearchService CreateService(RecordingIniFileStore store)
        => new(new ReadonlyIniContentService(store));

    private static IReadOnlyList<ReadonlyIniFileDescriptor> Files(params string[] names)
        => names.Select(name => Descriptor(name, 100)).ToArray();

    private static ReadonlyIniFileDescriptor Descriptor(string name, long size)
        => new(name, Path.GetFullPath(name), size);

    private sealed class RecordingIniFileStore : IIniFileStore
    {
        private readonly Dictionary<string, string> _textByPath;

        public RecordingIniFileStore(params (string Path, string Text)[] files)
        {
            _textByPath = files.ToDictionary(
                file => Path.GetFullPath(file.Path),
                file => file.Text,
                StringComparer.OrdinalIgnoreCase);
        }

        public List<string> ReadPaths { get; } = [];

        public string? ThrowForPath { get; init; }

        public IniTextReadResult ReadText(string path)
        {
            string fullPath = Path.GetFullPath(path);
            ReadPaths.Add(fullPath);
            if (string.Equals(fullPath, ThrowForPath, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Read failed.");

            return new IniTextReadResult(fullPath, _textByPath[fullPath], Encoding.UTF8, "\n");
        }

        public IniTextWriteResult WriteText(string path, string text, Encoding encoding)
            => throw new NotSupportedException();
    }
}
