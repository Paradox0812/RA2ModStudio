using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Controllers.FieldAnnotations;
using RA2IniEditor.IDE.FieldAnnotations;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldAnnotationCoordinatorTests
{
    [Fact]
    public void Refresh_WhenSidecarLoads_ReturnsProviderResolverAndLoadedStatus()
    {
        FakeAnnotationStore store = new(new Ra2FieldAnnotationLoadResult(
            new Ra2FieldAnnotationPack(1, "zh-CN", [
                new Ra2FieldAnnotationEntry("Vehicle", "Strength", "Health", ["HP"], "Maximum hit points.")
            ])));
        Ra2FieldAnnotationCoordinator coordinator = new(store, new Ra2FieldAnnotationPathService());

        Ra2FieldAnnotationRefreshResult result = coordinator.Refresh(new Ra2FieldAnnotationRefreshRequest(
            new TestFieldProvider(),
            @"C:\mods\sample"));

        Assert.EndsWith(@".ra2ide\field-annotations.zh-CN.json", result.AnnotationPath);
        Assert.True(result.LoadResult.Success);
        Assert.True(result.Status.IsLoaded);
        Assert.Empty(result.Warnings);
        Assert.NotNull(result.Provider.Find(Ra2SectionKind.Vehicle, "Strength"));
        Assert.Equal("Health", result.DisplayResolver.Resolve(Ra2SectionKind.Vehicle, "Strength").DisplayName);
    }

    [Fact]
    public void Refresh_WhenSidecarMissing_ReturnsFallbackResolverAndWarnings()
    {
        FakeAnnotationStore store = new(new Ra2FieldAnnotationLoadResult(
            Ra2FieldAnnotationPack.Empty(),
            ["Annotation sidecar was not found."]));
        Ra2FieldAnnotationCoordinator coordinator = new(store, new Ra2FieldAnnotationPathService());

        Ra2FieldAnnotationRefreshResult result = coordinator.Refresh(new Ra2FieldAnnotationRefreshRequest(
            new TestFieldProvider(),
            @"C:\mods\sample"));

        Assert.False(result.Status.IsLoaded);
        Assert.True(result.Status.HasWarnings);
        Assert.Contains("not found", Assert.Single(result.Warnings), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Strength", result.DisplayResolver.Resolve(Ra2SectionKind.Vehicle, "Strength").DisplayName);
    }

    [Fact]
    public void Refresh_WhenSidecarFails_ReturnsFailedStatusAndFallbackResolver()
    {
        FakeAnnotationStore store = new(new Ra2FieldAnnotationLoadResult(
            Ra2FieldAnnotationPack.Empty(),
            ["bad json"],
            success: false));
        Ra2FieldAnnotationCoordinator coordinator = new(store, new Ra2FieldAnnotationPathService());

        Ra2FieldAnnotationRefreshResult result = coordinator.Refresh(new Ra2FieldAnnotationRefreshRequest(
            new TestFieldProvider(),
            @"C:\mods\sample"));

        Assert.False(result.LoadResult.Success);
        Assert.False(result.Status.IsLoaded);
        Assert.True(result.Status.HasWarnings);
        Assert.Equal("bad json", Assert.Single(result.Warnings));
        Assert.Equal("Strength", result.DisplayResolver.Resolve(Ra2SectionKind.Vehicle, "Strength").DisplayName);
    }

    [Fact]
    public void GetProjectAnnotationPath_UsesRequestedLanguage()
    {
        Ra2FieldAnnotationCoordinator coordinator = new(
            new FakeAnnotationStore(new Ra2FieldAnnotationLoadResult(Ra2FieldAnnotationPack.Empty())),
            new Ra2FieldAnnotationPathService());

        string path = coordinator.GetProjectAnnotationPath(@"C:\mods\sample", "en-US");

        Assert.EndsWith(@".ra2ide\field-annotations.en-US.json", path);
    }

    private sealed class FakeAnnotationStore : IRa2FieldAnnotationStore
    {
        private readonly Ra2FieldAnnotationLoadResult _loadResult;

        public FakeAnnotationStore(Ra2FieldAnnotationLoadResult loadResult)
            => _loadResult = loadResult;

        public Ra2FieldAnnotationLoadResult Load(string path)
            => _loadResult;

        public Ra2FieldAnnotationSaveResult Save(string path, Ra2FieldAnnotationPack pack)
            => Ra2FieldAnnotationSaveResult.Succeeded();
    }

    private sealed class TestFieldProvider : IRa2FieldDefinitionProvider
    {
        private static readonly Ra2FieldDefinition Strength = new(
            "Strength",
            [Ra2SectionKind.Vehicle],
            FieldEditorKind.Integer,
            Ra2FieldSourceKind.BuiltIn,
            description: "Hit points.");

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            if (sectionKind == Ra2SectionKind.Vehicle &&
                string.Equals(key, "Strength", StringComparison.OrdinalIgnoreCase))
            {
                definition = Strength;
                return true;
            }

            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => sectionKind == Ra2SectionKind.Vehicle ? [Strength] : [];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }
}
