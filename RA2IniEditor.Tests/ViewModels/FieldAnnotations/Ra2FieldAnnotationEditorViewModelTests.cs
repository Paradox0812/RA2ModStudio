using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.ViewModels.FieldAnnotations;
using Xunit;

namespace RA2IniEditor.Tests.ViewModels.FieldAnnotations;

public sealed class Ra2FieldAnnotationEditorViewModelTests
{
    [Fact]
    public void Constructor_LoadsExistingExactAnnotation()
    {
        Ra2FieldAnnotationPack pack = new(
            1,
            "zh-CN",
            [new Ra2FieldAnnotationEntry("Vehicle", "Cost", "造价", ["价格"], "建造消耗")]);

        Ra2FieldAnnotationEditorViewModel viewModel = CreateViewModel(pack);

        Assert.Equal("造价", viewModel.DisplayName);
        Assert.Equal("价格", viewModel.AliasesText);
        Assert.Equal("建造消耗", viewModel.Note);
        Assert.False(viewModel.IsDirty);
    }

    [Fact]
    public void Save_WritesSidecarPackAndClearsDirty()
    {
        FakeAnnotationStore store = new();
        Ra2FieldAnnotationEditorViewModel viewModel = CreateViewModel(Ra2FieldAnnotationPack.Empty(), store);

        viewModel.DisplayName = "造价";
        viewModel.AliasesText = "价格， 花费";
        viewModel.Note = "建造消耗";

        Assert.True(viewModel.IsDirty);
        Assert.True(viewModel.Save());

        Assert.False(viewModel.IsDirty);
        Assert.Equal(@"C:\Project\.ra2ide\field-annotations.zh-CN.json", store.LastPath);
        Ra2FieldAnnotationEntry entry = Assert.Single(store.LastPack!.Entries);
        Assert.Equal("Vehicle", entry.SectionKind);
        Assert.Equal("Cost", entry.Key);
        Assert.Equal("造价", entry.DisplayName);
        Assert.Equal(["价格", "花费"], entry.Aliases);
        Assert.Equal("建造消耗", entry.Note);
    }

    [Fact]
    public void Save_WhenStoreFails_KeepsDirtyAndShowsError()
    {
        FakeAnnotationStore store = new(saveResult: Ra2FieldAnnotationSaveResult.Failed("disk denied"));
        Ra2FieldAnnotationEditorViewModel viewModel = CreateViewModel(Ra2FieldAnnotationPack.Empty(), store);
        viewModel.DisplayName = "造价";

        Assert.False(viewModel.Save());

        Assert.True(viewModel.IsDirty);
        Assert.Contains("disk denied", viewModel.StatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateLibrary_WritesEmptyPackWithoutDirtyingIni()
    {
        FakeAnnotationStore store = new();
        Ra2FieldAnnotationEditorViewModel viewModel = CreateViewModel(Ra2FieldAnnotationPack.Empty(), store);

        Assert.True(viewModel.CreateLibrary());

        Assert.False(viewModel.IsDirty);
        Assert.Equal("字段注释库已创建。", viewModel.StatusText);
        Assert.Empty(store.LastPack!.Entries);
    }

    [Fact]
    public void Save_BlankAnnotation_RemovesExistingEntry()
    {
        FakeAnnotationStore store = new();
        Ra2FieldAnnotationPack pack = new(
            1,
            "zh-CN",
            [new Ra2FieldAnnotationEntry("Vehicle", "Cost", "造价")]);
        Ra2FieldAnnotationEditorViewModel viewModel = CreateViewModel(pack, store);

        viewModel.DisplayName = "";
        viewModel.Note = "";

        Assert.True(viewModel.Save());

        Assert.Empty(store.LastPack!.Entries);
    }

    [Fact]
    public void Save_WithoutAnnotationPath_ReturnsFalse()
    {
        FakeAnnotationStore store = new();
        Ra2FieldAnnotationEditorViewModel viewModel = CreateViewModel(
            Ra2FieldAnnotationPack.Empty(),
            store,
            annotationPath: "");

        viewModel.DisplayName = "造价";

        Assert.False(viewModel.Save());
        Assert.Null(store.LastPack);
    }

    [Fact]
    public void EditingProperties_SetsAnnotationDirtyAndChineseStatus()
    {
        Ra2FieldAnnotationEditorViewModel viewModel = CreateViewModel(Ra2FieldAnnotationPack.Empty());

        viewModel.AliasesText = "价格";

        Assert.True(viewModel.IsDirty);
        Assert.Equal("字段注释尚未保存。", viewModel.StatusText);
    }

    [Fact]
    public void ParseAliases_AcceptsChineseAndEnglishSeparators()
    {
        IReadOnlyList<string> aliases = Ra2FieldAnnotationEditorViewModel.ParseAliases("价格, 花费，Cost；cost");

        Assert.Equal(["价格", "花费", "Cost"], aliases);
    }

    private static Ra2FieldAnnotationEditorViewModel CreateViewModel(
        Ra2FieldAnnotationPack pack,
        FakeAnnotationStore? store = null,
        string annotationPath = @"C:\Project\.ra2ide\field-annotations.zh-CN.json")
    {
        Ra2FieldDisplayInfo displayInfo = new(
            "Cost",
            "Cost",
            [],
            null,
            "Build cost.",
            "Integer",
            "Vehicle",
            "BuiltIn",
            hasUserAnnotation: false);
        return new Ra2FieldAnnotationEditorViewModel(
            Ra2SectionKind.Vehicle,
            displayInfo,
            pack,
            annotationPath,
            store ?? new FakeAnnotationStore(),
            new Ra2FieldAnnotationEditingService());
    }

    private sealed class FakeAnnotationStore : IRa2FieldAnnotationStore
    {
        private readonly Ra2FieldAnnotationSaveResult _saveResult;

        public FakeAnnotationStore(Ra2FieldAnnotationSaveResult? saveResult = null)
            => _saveResult = saveResult ?? Ra2FieldAnnotationSaveResult.Succeeded();

        public string? LastPath { get; private set; }

        public Ra2FieldAnnotationPack? LastPack { get; private set; }

        public Ra2FieldAnnotationLoadResult Load(string path)
            => new(Ra2FieldAnnotationPack.Empty(), []);

        public Ra2FieldAnnotationSaveResult Save(string path, Ra2FieldAnnotationPack pack)
        {
            LastPath = path;
            LastPack = pack;
            return _saveResult;
        }
    }
}
