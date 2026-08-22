using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Fetch;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldRegistryHarvestPreviewViewModelPresetTests
{
    [Fact]
    public void UsePresetUrl_DoesNotFetchOrParse()
    {
        string root = CreateTempRoot();
        FieldRegistryRemoteSourcePresetStore presetStore = CreateStoreWithPreset(root, CreatePreset("one", "Ares Docs"));
        CountingRawFetcher fetcher = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(root, fetcher, presetStore: presetStore);
        viewModel.RawText = "ExistingKey=1";
        viewModel.RefreshRemotePresets();
        viewModel.SelectedRemotePreset = viewModel.RemotePresets[0];

        viewModel.UsePresetUrl();

        Assert.Equal(0, fetcher.CallCount);
        Assert.Equal("ExistingKey=1", viewModel.RawText);
        Assert.Equal("https://github.com/owner/repo/blob/main/fields.md", viewModel.FetchUrl);
        Assert.Empty(viewModel.Candidates);
        Assert.Empty(viewModel.Definitions);
        Assert.Contains("未发起网络请求", viewModel.RemotePresetStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FetchSelectedPreset_CallsFetcherAndDoesNotAutoParseOrApply()
    {
        string root = CreateTempRoot();
        FieldRegistryRemoteSourcePresetStore presetStore = CreateStoreWithPreset(root, CreatePreset("one", "Ares Docs"));
        CountingRawFetcher fetcher = new("FetchedKey=1");
        RecordingHistoryStore historyStore = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(root, fetcher, historyStore, presetStore);
        viewModel.RefreshRemotePresets();
        viewModel.SelectedRemotePreset = viewModel.RemotePresets[0];

        await viewModel.FetchSelectedPresetAsync();

        Assert.Equal(1, fetcher.CallCount);
        Assert.Equal("FetchedKey=1", viewModel.RawText);
        Assert.Equal("Ares Docs", viewModel.SourceName);
        Assert.Single(historyStore.Entries);
        Assert.Empty(viewModel.Candidates);
        Assert.Empty(viewModel.Definitions);
        Assert.Empty(viewModel.ApplyPlanItems);
        Assert.Contains("解析并预览", viewModel.FetchStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DisabledPreset_CannotFetch()
    {
        string root = CreateTempRoot();
        FieldRegistryRemoteSourcePresetStore presetStore = CreateStoreWithPreset(root, CreatePreset("one", "Ares Docs", isEnabled: false));
        CountingRawFetcher fetcher = new("FetchedKey=1");
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(root, fetcher, presetStore: presetStore);
        viewModel.RefreshRemotePresets();
        viewModel.SelectedRemotePreset = viewModel.RemotePresets[0];

        Assert.False(viewModel.CanFetchSelectedPreset);
        await viewModel.FetchSelectedPresetAsync();

        Assert.Equal(0, fetcher.CallCount);
        Assert.Equal(string.Empty, viewModel.RawText);
        Assert.Contains("已禁用", viewModel.RemotePresetStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveSelectedPreset_RequiresConfirmation()
    {
        string root = CreateTempRoot();
        FieldRegistryRemoteSourcePresetStore presetStore = CreateStoreWithPreset(root, CreatePreset("one", "Ares Docs"));
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(root, new CountingRawFetcher(), presetStore: presetStore);
        viewModel.RefreshRemotePresets();
        viewModel.SelectedRemotePreset = viewModel.RemotePresets[0];

        viewModel.RemoveSelectedPreset(confirmed: false);

        Assert.Single(viewModel.RemotePresets);
        Assert.Contains("已取消", viewModel.RemotePresetStatusText, StringComparison.Ordinal);

        viewModel.RemoveSelectedPreset(confirmed: true);

        Assert.Empty(viewModel.RemotePresets);
        Assert.Contains("已移除", viewModel.RemotePresetStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void AddAndEditPreset_UpdateLocalPresetList()
    {
        string root = CreateTempRoot();
        FieldRegistryRemoteSourcePresetStore presetStore = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(root, new CountingRawFetcher(), presetStore: presetStore);

        viewModel.AddPreset(new FieldRegistryRemoteSourcePresetEditModel(
            null,
            "Ares Docs",
            "https://github.com/owner/repo/blob/main/fields.md",
            "Docs",
            "ares; docs",
            true));

        FieldRegistryRemoteSourcePresetViewModel added = Assert.Single(viewModel.RemotePresets);
        Assert.Equal("Ares Docs", added.Name);
        Assert.Equal("ares, docs", added.TagsText);

        viewModel.EditSelectedPreset(new FieldRegistryRemoteSourcePresetEditModel(
            added.Id,
            "Ares Updated",
            added.Url,
            "Updated",
            "updated",
            true));

        FieldRegistryRemoteSourcePresetViewModel edited = Assert.Single(viewModel.RemotePresets);
        Assert.Equal("Ares Updated", edited.Name);
        Assert.Equal("updated", edited.TagsText);
    }

    private static FieldRegistryHarvestPreviewViewModel CreateViewModel(
        string globalRoot,
        IFieldRegistryRawFetcher rawFetcher,
        IFieldRegistryRemoteSourceHistoryStore? historyStore = null,
        IFieldRegistryRemoteSourcePresetStore? presetStore = null)
        => new(
            new MarkdownFieldRegistryHarvestParser(),
            new FieldRegistryHarvestNormalizer(),
            new FieldRegistryHarvestPreviewBuilder(),
            new FieldRegistryHarvestDiffService(),
            () => new EmptyProvenanceProvider(),
            new FieldRegistryApplyPlanBuilder(),
            new FieldRegistryApplyWriter(),
            () => null,
            () => globalRoot,
            null,
            rawFetcher,
            historyStore,
            presetStore);

    private static FieldRegistryRemoteSourcePresetStore CreateStoreWithPreset(
        string root,
        FieldRegistryRemoteSourcePreset preset)
    {
        FieldRegistryRemoteSourcePresetStore store = new();
        store.AddOrUpdate(root, preset);
        return store;
    }

    private static FieldRegistryRemoteSourcePreset CreatePreset(
        string id,
        string name,
        bool isEnabled = true)
        => new(
            id,
            name,
            "https://github.com/owner/repo/blob/main/fields.md",
            "Docs",
            ["ares"],
            isEnabled,
            "2026-01-01T00:00:00.0000000+00:00",
            "2026-01-02T00:00:00.0000000+00:00");

    private static string CreateTempRoot()
    {
        string path = Path.Combine(Path.GetTempPath(), "RA2IniEditor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class EmptyProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(Ra2SectionKind sectionKind, string key)
            => FieldRegistryProvenanceLookupResult.NotFound;
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

    private sealed class RecordingHistoryStore : IFieldRegistryRemoteSourceHistoryStore
    {
        public List<FieldRegistryRemoteSourceHistoryEntry> Entries { get; } = [];

        public string? LastWarning => null;

        public FieldRegistryRemoteSourceHistory Load(string globalFieldRegistryRootPath)
            => new(Entries);

        public void Save(string globalFieldRegistryRootPath, FieldRegistryRemoteSourceHistory history)
        {
            Entries.Clear();
            Entries.AddRange(history.Entries);
        }

        public void AddOrUpdate(string globalFieldRegistryRootPath, FieldRegistryRemoteSourceHistoryEntry entry)
        {
            Entries.Clear();
            Entries.Add(entry);
        }

        public void Clear(string globalFieldRegistryRootPath)
            => Entries.Clear();
    }
}
