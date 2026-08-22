using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Fetch;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldRegistryHarvestPreviewViewModelTests
{
    [Fact]
    public void ParseAndPreview_MarkdownTablePopulatesPreview()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new()
        {
            SourceName = "sample.md",
            RawText = """
                | Key | AppliesTo | Type | Description |
                | --- | --- | --- | --- |
                | Owner | Infantry | Text | Owner countries |
                | Strength | Building | Float | Hit points |
                """
        };

        viewModel.ParseAndPreview();

        Assert.Equal(2, viewModel.Candidates.Count);
        Assert.Equal(2, viewModel.Definitions.Count);
        Assert.DoesNotContain(viewModel.Issues, issue => issue.Severity == "Error");
        Assert.True(viewModel.CanApplyInFuture);
        Assert.Contains("预览已生成", viewModel.StatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseAndPreview_EmptyInputClearsPreviewAndShowsStatus()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new()
        {
            RawText = string.Empty
        };

        viewModel.ParseAndPreview();

        Assert.Empty(viewModel.Candidates);
        Assert.Empty(viewModel.Definitions);
        Assert.Empty(viewModel.Issues);
        Assert.Empty(viewModel.RawWarnings);
        Assert.Contains("没有可解析的原始文本", viewModel.StatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void InitialStatusExplainsSupportedInputFormatsAndApplyBoundary()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new();

        Assert.Contains("INI 风格行", viewModel.StatusText, StringComparison.Ordinal);
        Assert.Contains("Markdown 表格", viewModel.StatusText, StringComparison.Ordinal);
        Assert.Contains("构建计划和确认", viewModel.StatusText, StringComparison.Ordinal);
        Assert.Equal("尚未生成预览。请先点击“解析并预览”。", viewModel.ApplyDisabledReason);
    }
    [Fact]
    public void ParseAndPreview_InvalidKeyCreatesRawWarningWithoutCrashing()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new()
        {
            RawText = "Bad Key=1"
        };

        viewModel.ParseAndPreview();

        Assert.Empty(viewModel.Candidates);
        Assert.Empty(viewModel.Definitions);
        Assert.Single(viewModel.RawWarnings);
        Assert.Contains("invalid", viewModel.RawWarnings[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseAndPreview_DuplicateCandidateKeepsOneDefinitionAndShowsRawWarning()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new()
        {
            RawText = """
                Owner=
                - Owner: owner countries
                """
        };

        viewModel.ParseAndPreview();

        Assert.Single(viewModel.Candidates);
        Assert.Single(viewModel.Definitions);
        Assert.Single(viewModel.RawWarnings);
        Assert.Contains("duplicate", viewModel.RawWarnings[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InsertSample_InsertsTextWithoutParsing()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithEmptyProvider();

        viewModel.InsertSample();

        Assert.Contains("# Basic sample", viewModel.RawText, StringComparison.Ordinal);
        Assert.Contains("# Table sample", viewModel.RawText, StringComparison.Ordinal);
        Assert.Contains("Owner=", viewModel.RawText, StringComparison.Ordinal);
        Assert.Contains("- MyCustomKey:", viewModel.RawText, StringComparison.Ordinal);
        Assert.Contains("| Key | AppliesTo | Type | Description |", viewModel.RawText, StringComparison.Ordinal);
        Assert.Empty(viewModel.Candidates);
        Assert.Empty(viewModel.Definitions);
        Assert.Empty(viewModel.DiffRows);
        Assert.Contains("解析并预览", viewModel.StatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FetchRawTextAsync_SuccessFillsRawTextAndDoesNotAutoParse()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithFetcher(new StubRawFetcher(
            new FieldRegistryRawFetchResult(
                "https://github.com/owner/repo/blob/main/fields.md",
                "https://raw.githubusercontent.com/owner/repo/main/fields.md",
                "fields.md",
                "FetchedKey=1",
                12)));
        viewModel.FetchUrl = "https://github.com/owner/repo/blob/main/fields.md";

        await viewModel.FetchRawTextAsync();

        Assert.Equal("FetchedKey=1", viewModel.RawText);
        Assert.Equal("fields.md", viewModel.SourceName);
        Assert.Empty(viewModel.Candidates);
        Assert.Empty(viewModel.Definitions);
        Assert.Empty(viewModel.ApplyPlanItems);
        Assert.Contains("获取 12 字节", viewModel.FetchStatusText, StringComparison.Ordinal);
        Assert.Contains("解析并预览", viewModel.FetchStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FetchRawTextAsync_FailurePreservesRawText()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithFetcher(new ThrowingRawFetcher(
            new InvalidOperationException("Only https URLs are supported.")));
        viewModel.FetchUrl = "http://raw.githubusercontent.com/owner/repo/main/fields.md";
        viewModel.RawText = "ExistingKey=1";
        viewModel.SourceName = "existing.md";

        await viewModel.FetchRawTextAsync();

        Assert.Equal("ExistingKey=1", viewModel.RawText);
        Assert.Equal("existing.md", viewModel.SourceName);
        Assert.Contains("获取失败", viewModel.FetchStatusText, StringComparison.Ordinal);
        Assert.Contains("Only https", viewModel.FetchStatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FetchRawTextAsync_EmptyUrlShowsErrorWithoutCallingFetcher()
    {
        CountingRawFetcher fetcher = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithFetcher(fetcher);
        viewModel.FetchUrl = " ";
        viewModel.RawText = "ExistingKey=1";

        await viewModel.FetchRawTextAsync();

        Assert.Equal(0, fetcher.CallCount);
        Assert.Equal("ExistingKey=1", viewModel.RawText);
        Assert.Contains("URL 不能为空", viewModel.FetchStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CancelFetch_DoesNotUpdateRawText()
    {
        BlockingRawFetcher fetcher = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithFetcher(fetcher);
        viewModel.FetchUrl = "https://raw.githubusercontent.com/owner/repo/main/fields.md";
        viewModel.RawText = "ExistingKey=1";

        Task fetchTask = viewModel.FetchRawTextAsync();
        await fetcher.WaitUntilStartedAsync();
        viewModel.CancelFetch();
        await fetchTask;

        Assert.Equal("ExistingKey=1", viewModel.RawText);
        Assert.True(
            viewModel.FetchStatusText.Contains("已取消", StringComparison.Ordinal) ||
            viewModel.FetchStatusText.Contains("正在取消获取", StringComparison.Ordinal));
    }

    [Fact]
    public async Task FetchRawTextAsync_SuccessSavesHistoryAndRefreshesList()
    {
        RecordingHistoryStore historyStore = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithFetcher(
            new StubRawFetcher(new FieldRegistryRawFetchResult(
                "https://github.com/owner/repo/blob/main/fields.md",
                "https://raw.githubusercontent.com/owner/repo/main/fields.md",
                "fields.md",
                "FetchedKey=1",
                12)),
            historyStore);
        viewModel.FetchUrl = "https://github.com/owner/repo/blob/main/fields.md";

        await viewModel.FetchRawTextAsync();

        FieldRegistryRemoteSourceHistoryEntry saved = Assert.Single(historyStore.Entries);
        Assert.Equal("https://raw.githubusercontent.com/owner/repo/main/fields.md", saved.ResolvedUrl);
        Assert.Single(viewModel.RemoteHistoryEntries);
        Assert.Contains("已保存到远程来源历史", viewModel.FetchStatusText, StringComparison.Ordinal);
        Assert.Empty(viewModel.Candidates);
    }

    [Fact]
    public void UseCachedTextFromHistory_DoesNotFetchAndDoesNotParse()
    {
        CountingRawFetcher fetcher = new();
        FieldRegistryRemoteSourceHistoryEntry entry = new(
            "https://github.com/owner/repo/blob/main/fields.md",
            "https://raw.githubusercontent.com/owner/repo/main/fields.md",
            "fields.md",
            DateTimeOffset.UtcNow,
            12,
            "CachedKey=1");
        RecordingHistoryStore historyStore = new([entry]);
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithFetcher(fetcher, historyStore);
        viewModel.RefreshRemoteHistory();
        viewModel.SelectedRemoteHistoryEntry = viewModel.RemoteHistoryEntries[0];

        viewModel.UseCachedTextFromHistory();

        Assert.Equal(0, fetcher.CallCount);
        Assert.Equal("CachedKey=1", viewModel.RawText);
        Assert.Equal("fields.md", viewModel.SourceName);
        Assert.Equal(entry.Url, viewModel.FetchUrl);
        Assert.Empty(viewModel.Candidates);
        Assert.Contains("载入缓存文本", viewModel.RemoteHistoryStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RefetchSelectedRemoteHistoryAsync_CallsFetcherAndUpdatesHistory()
    {
        CountingRawFetcher fetcher = new("NewKey=1");
        FieldRegistryRemoteSourceHistoryEntry entry = new(
            "https://github.com/owner/repo/blob/main/fields.md",
            "https://raw.githubusercontent.com/owner/repo/main/fields.md",
            "fields.md",
            DateTimeOffset.UtcNow,
            8,
            "OldKey=1");
        RecordingHistoryStore historyStore = new([entry]);
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithFetcher(fetcher, historyStore);
        viewModel.RefreshRemoteHistory();
        viewModel.SelectedRemoteHistoryEntry = viewModel.RemoteHistoryEntries[0];

        await viewModel.RefetchSelectedRemoteHistoryAsync();

        Assert.Equal(1, fetcher.CallCount);
        Assert.Equal("NewKey=1", viewModel.RawText);
        Assert.Single(historyStore.Entries);
        Assert.Equal("NewKey=1", historyStore.Entries[0].CachedText);
    }

    [Fact]
    public void ClearRemoteHistory_RequiresConfirmation()
    {
        FieldRegistryRemoteSourceHistoryEntry entry = new(
            "https://github.com/owner/repo/blob/main/fields.md",
            "https://raw.githubusercontent.com/owner/repo/main/fields.md",
            "fields.md",
            DateTimeOffset.UtcNow,
            8,
            "CachedKey=1");
        RecordingHistoryStore historyStore = new([entry]);
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithFetcher(new CountingRawFetcher(), historyStore);
        viewModel.RefreshRemoteHistory();

        viewModel.ClearRemoteHistory(confirmed: false);

        Assert.Single(historyStore.Entries);
        Assert.Single(viewModel.RemoteHistoryEntries);

        viewModel.ClearRemoteHistory(confirmed: true);

        Assert.Empty(historyStore.Entries);
        Assert.Empty(viewModel.RemoteHistoryEntries);
        Assert.Contains("已清空", viewModel.RemoteHistoryStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseAndPreview_GeneratesAddedDiffRowsAgainstCurrentProvider()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithEmptyProvider();
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MyNewKey | Infantry | Text | New key |
            """;

        viewModel.ParseAndPreview();

        FieldRegistryHarvestDiffRowViewModel row = Assert.Single(viewModel.DiffRows);
        Assert.Equal("Added", row.Kind);
        Assert.Equal("None", row.ExistingScope);
        Assert.Equal("None", row.ExistingSourceName);
        Assert.Equal(1, viewModel.AddedCount);
        Assert.Contains("新增：1", viewModel.StatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseAndPreview_DiffRowsIncludeExistingProvenance()
    {
        Ra2FieldDefinition existing = new(
            "Owner",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.MultiSelect,
            Ra2FieldSourceKind.External,
            "Existing owner source");
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithProvenance(
            new SingleFieldProvenanceProvider(
                new FieldRegistryProvenanceEntry(
                    "Owner",
                    Ra2SectionKind.Infantry,
                    FieldRegistryProvenanceScope.Global,
                    "global.fields.json",
                    @"C:\field-registry\active\global.fields.json",
                    existing)));
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | Owner | Infantry | Text | Owner countries |
            """;

        viewModel.ParseAndPreview();

        FieldRegistryHarvestDiffRowViewModel row = Assert.Single(viewModel.DiffRows);
        Assert.Equal("Changed", row.Kind);
        Assert.Equal("Global", row.ExistingScope);
        Assert.Equal("global.fields.json", row.ExistingSourceName);
        Assert.Equal(@"C:\field-registry\active\global.fields.json", row.ExistingSourcePath);
    }

    [Fact]
    public void Clear_RemovesRawTextAndPreviewState()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModelWithEmptyProvider();
        viewModel.InsertSample();
        viewModel.ParseAndPreview();

        viewModel.Clear();

        Assert.Equal(string.Empty, viewModel.RawText);
        Assert.Empty(viewModel.Candidates);
        Assert.Empty(viewModel.Definitions);
        Assert.Empty(viewModel.Issues);
        Assert.Empty(viewModel.RawWarnings);
        Assert.Empty(viewModel.DiffRows);
        Assert.Equal(0, viewModel.AddedCount);
        Assert.Equal(0, viewModel.SameCount);
        Assert.Equal(0, viewModel.ChangedCount);
        Assert.Equal(0, viewModel.ConflictCount);
        Assert.Equal(0, viewModel.InvalidCount);
        Assert.False(viewModel.CanApplyInFuture);
        Assert.Contains("预览已清空", viewModel.StatusText, StringComparison.Ordinal);
    }

    private static FieldRegistryHarvestPreviewViewModel CreateViewModelWithEmptyProvider()
        => CreateViewModelWithProvenance(new EmptyProvenanceProvider());

    private static FieldRegistryHarvestPreviewViewModel CreateViewModelWithFetcher(
        IFieldRegistryRawFetcher rawFetcher,
        IFieldRegistryRemoteSourceHistoryStore? historyStore = null)
    {
        return new FieldRegistryHarvestPreviewViewModel(
            new MarkdownFieldRegistryHarvestParser(),
            new FieldRegistryHarvestNormalizer(),
            new FieldRegistryHarvestPreviewBuilder(),
            new FieldRegistryHarvestDiffService(),
            () => new EmptyProvenanceProvider(),
            new FieldRegistryApplyPlanBuilder(),
            new FieldRegistryApplyWriter(),
            () => null,
            () => Path.Combine(Path.GetTempPath(), "RA2IniEditor.Tests", "FieldRegistry"),
            null,
            rawFetcher,
            historyStore);
    }

    private static FieldRegistryHarvestPreviewViewModel CreateViewModelWithProvenance(
        IFieldRegistryProvenanceProvider provenanceProvider)
    {
        return new FieldRegistryHarvestPreviewViewModel(
            new MarkdownFieldRegistryHarvestParser(),
            new FieldRegistryHarvestNormalizer(),
            new FieldRegistryHarvestPreviewBuilder(),
            new FieldRegistryHarvestDiffService(),
            () => provenanceProvider);
    }

    private sealed class EmptyProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(Ra2SectionKind sectionKind, string key)
            => FieldRegistryProvenanceLookupResult.NotFound;
    }

    private sealed class SingleFieldProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        private readonly FieldRegistryProvenanceEntry _entry;

        public SingleFieldProvenanceProvider(FieldRegistryProvenanceEntry entry)
        {
            _entry = entry;
        }

        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(Ra2SectionKind sectionKind, string key)
        {
            return _entry.AppliesTo == sectionKind &&
                string.Equals(_entry.Key, key, StringComparison.OrdinalIgnoreCase)
                ? FieldRegistryProvenanceLookupResult.FromEntry(_entry)
                : FieldRegistryProvenanceLookupResult.NotFound;
        }
    }

    private sealed class StubRawFetcher : IFieldRegistryRawFetcher
    {
        private readonly FieldRegistryRawFetchResult _result;

        public StubRawFetcher(FieldRegistryRawFetchResult result)
        {
            _result = result;
        }

        public Task<FieldRegistryRawFetchResult> FetchAsync(
            FieldRegistryRawFetchRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(_result);
    }

    private sealed class ThrowingRawFetcher : IFieldRegistryRawFetcher
    {
        private readonly Exception _exception;

        public ThrowingRawFetcher(Exception exception)
        {
            _exception = exception;
        }

        public Task<FieldRegistryRawFetchResult> FetchAsync(
            FieldRegistryRawFetchRequest request,
            CancellationToken cancellationToken)
            => Task.FromException<FieldRegistryRawFetchResult>(_exception);
    }

    private sealed class CountingRawFetcher : IFieldRegistryRawFetcher
    {
        private readonly string _text;

        public CountingRawFetcher(string text = "")
        {
            _text = text;
        }

        public int CallCount { get; private set; }

        public Task<FieldRegistryRawFetchResult> FetchAsync(
            FieldRegistryRawFetchRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new FieldRegistryRawFetchResult(
                request.Url,
                "https://raw.githubusercontent.com/owner/repo/main/fields.md",
                "fields.md",
                _text,
                _text.Length));
        }
    }

    private sealed class BlockingRawFetcher : IFieldRegistryRawFetcher
    {
        private readonly TaskCompletionSource _started = new();

        public Task WaitUntilStartedAsync()
            => _started.Task;

        public async Task<FieldRegistryRawFetchResult> FetchAsync(
            FieldRegistryRawFetchRequest request,
            CancellationToken cancellationToken)
        {
            _started.SetResult();
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            return new FieldRegistryRawFetchResult(request.Url, request.Url, "fields.md", "NewKey=1", 8);
        }
    }

    private sealed class RecordingHistoryStore : IFieldRegistryRemoteSourceHistoryStore
    {
        public RecordingHistoryStore()
            : this([])
        {
        }

        public RecordingHistoryStore(IReadOnlyList<FieldRegistryRemoteSourceHistoryEntry> entries)
        {
            Entries = entries.ToList();
        }

        public List<FieldRegistryRemoteSourceHistoryEntry> Entries { get; }

        public string? LastWarning { get; private set; }

        public FieldRegistryRemoteSourceHistory Load(string globalFieldRegistryRootPath)
            => new(Entries);

        public void Save(string globalFieldRegistryRootPath, FieldRegistryRemoteSourceHistory history)
        {
            Entries.Clear();
            Entries.AddRange(history.Entries);
        }

        public void AddOrUpdate(string globalFieldRegistryRootPath, FieldRegistryRemoteSourceHistoryEntry entry)
        {
            Entries.RemoveAll(existing => string.Equals(existing.ResolvedUrl, entry.ResolvedUrl, StringComparison.OrdinalIgnoreCase));
            Entries.Insert(0, entry);
        }

        public void Clear(string globalFieldRegistryRootPath)
        {
            LastWarning = null;
            Entries.Clear();
        }
    }
}
