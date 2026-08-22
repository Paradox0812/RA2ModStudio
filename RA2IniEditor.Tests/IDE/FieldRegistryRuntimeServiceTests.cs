using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldRegistryRuntimeServiceTests
{
    [Fact]
    public void Reload_MissingDirectoriesReturnsBuiltInFallback()
    {
        using TempDirectory temp = TempDirectory.Create();
        string missingGlobal = Path.Combine(temp.Path, "missing-global");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), missingGlobal);

        IRa2FieldDefinitionProvider provider = service.Reload(null);

        Assert.Equal(0, service.CurrentState.Global.WarningCount);
        Assert.Equal(0, service.CurrentState.TotalLocalFieldCount);
        Assert.True(provider.IsKnownField(Ra2SectionKind.Infantry, "Owner"));
    }

    [Fact]
    public void Reload_UsesEmbeddedBuiltInV2InsteadOfStackingLegacyMinimalBuiltIn()
    {
        using TempDirectory temp = TempDirectory.Create();
        string missingGlobal = Path.Combine(temp.Path, "missing-global");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), missingGlobal);

        IRa2FieldDefinitionProvider provider = service.Reload(null);

        Assert.True(provider.TryGetField(Ra2SectionKind.Global, "AALimit", out Ra2FieldDefinition definition));
        Assert.Equal(Ra2FieldSourceKind.Yuri, definition.SourceKind);
        Assert.NotEmpty(definition.Examples);
        Assert.Equal(0, service.CurrentState.TotalLocalFieldCount);
    }

    [Fact]
    public void GetGlobalRootDirectoryPath_ReturnsParentOfActiveOverride()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        string rootPath = service.GetGlobalRootDirectoryPath();

        Assert.Equal(Path.GetDirectoryName(temp.GlobalActivePath), rootPath);
    }

    [Fact]
    public void Reload_ValidGlobalActivePackContributesFields()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.GlobalActivePath);
        WritePack(temp.GlobalActivePath, "global.fields.json", "GlobalCustomKey", "Infantry", "User");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        IRa2FieldDefinitionProvider provider = service.Reload(null);

        Assert.Equal(1, service.CurrentState.Global.FieldCount);
        Assert.True(provider.IsKnownField(Ra2SectionKind.Infantry, "GlobalCustomKey"));
        Assert.True(provider.IsKnownField(Ra2SectionKind.Infantry, "Owner"));
    }

    [Fact]
    public void Reload_ValidProjectActivePackContributesFields()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.ProjectActivePath);
        WritePack(temp.ProjectActivePath, "project.fields.json", "ProjectCustomKey", "Building", "User");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        IRa2FieldDefinitionProvider provider = service.Reload(temp.ProjectRootPath);

        Assert.NotNull(service.CurrentState.Project);
        Assert.Equal(1, service.CurrentState.Project!.FieldCount);
        Assert.True(provider.IsKnownField(Ra2SectionKind.Building, "ProjectCustomKey"));
    }

    [Fact]
    public void Reload_ProjectPackOverridesGlobalAndBuiltInFields()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.GlobalActivePath);
        Directory.CreateDirectory(temp.ProjectActivePath);
        WritePack(temp.GlobalActivePath, "global.fields.json", "Owner", "Infantry", "External");
        WritePack(temp.ProjectActivePath, "project.fields.json", "Owner", "Infantry", "User");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        IRa2FieldDefinitionProvider provider = service.Reload(temp.ProjectRootPath);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "Owner", out Ra2FieldDefinition definition));
        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
    }

    [Fact]
    public void Reload_GlobalSpecificFieldBeatsProjectUnknownFallbackField()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.GlobalActivePath);
        Directory.CreateDirectory(temp.ProjectActivePath);
        WritePack(temp.GlobalActivePath, "global.fields.json", "Owner", "Infantry", "External");
        WritePack(temp.ProjectActivePath, "project.fields.json", "Owner", "Unknown", "User");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        IRa2FieldDefinitionProvider provider = service.Reload(temp.ProjectRootPath);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "Owner", out Ra2FieldDefinition definition));
        Assert.Equal(Ra2FieldSourceKind.External, definition.SourceKind);
        Assert.Equal([Ra2SectionKind.Infantry], definition.AppliesTo);
    }

    [Fact]
    public void Reload_GlobalSpecificFieldBeatsProjectGlobalFallbackField()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.GlobalActivePath);
        Directory.CreateDirectory(temp.ProjectActivePath);
        WritePack(temp.GlobalActivePath, "global.fields.json", "CustomPriorityKey", "Infantry", "External");
        WritePack(temp.ProjectActivePath, "project.fields.json", "CustomPriorityKey", "Global", "User");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        IRa2FieldDefinitionProvider provider = service.Reload(temp.ProjectRootPath);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "CustomPriorityKey", out Ra2FieldDefinition definition));
        Assert.Equal(Ra2FieldSourceKind.External, definition.SourceKind);
        Assert.Equal([Ra2SectionKind.Infantry], definition.AppliesTo);
    }

    [Fact]
    public void Reload_UpdatesCurrentProvenanceProviderWithProjectAndGlobalSources()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.GlobalActivePath);
        Directory.CreateDirectory(temp.ProjectActivePath);
        WritePack(temp.GlobalActivePath, "global.fields.json", "Owner", "Infantry", "External");
        WritePack(temp.ProjectActivePath, "project.fields.json", "Owner", "Unknown", "User");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        service.Reload(temp.ProjectRootPath);

        FieldRegistryProvenanceLookupResult lookup = service.CurrentProvenanceProvider.TryGetFieldWithProvenance(
            Ra2SectionKind.Infantry,
            "Owner");
        Assert.True(lookup.Found);
        Assert.Equal(FieldRegistryProvenanceScope.Global, lookup.Scope);
        Assert.Equal("global.fields.json", lookup.SourceName);
        Assert.Equal(Path.Combine(temp.GlobalActivePath, "global.fields.json"), lookup.SourcePath);
    }

    [Fact]
    public void Reload_GlobalFieldOverridesBuiltInField()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.GlobalActivePath);
        WritePack(temp.GlobalActivePath, "global.fields.json", "Owner", "Techno", "External");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        IRa2FieldDefinitionProvider provider = service.Reload(null);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "Owner", out Ra2FieldDefinition definition));
        Assert.Equal(Ra2FieldSourceKind.External, definition.SourceKind);
    }

    [Fact]
    public void Reload_GenericGlobalFieldDoesNotHideMoreSpecificBuiltInField()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.GlobalActivePath);
        WritePack(temp.GlobalActivePath, "global.fields.json", "Armor", "Unknown", "External");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        IRa2FieldDefinitionProvider provider = service.Reload(null);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "Armor", out Ra2FieldDefinition definition));
        Assert.NotEqual(Ra2FieldSourceKind.External, definition.SourceKind);
        Assert.Contains(provider.GetFields(Ra2SectionKind.Infantry), field =>
            field.Key == "Primary" &&
            field.SourceKind != Ra2FieldSourceKind.External);
    }

    [Fact]
    public void Reload_ProjectFieldOverridesBuiltInOnlyWhenSpecificityMatches()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.ProjectActivePath);
        WritePack(temp.ProjectActivePath, "project.fields.json", "Armor", "Techno", "User");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        IRa2FieldDefinitionProvider provider = service.Reload(temp.ProjectRootPath);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "Armor", out Ra2FieldDefinition definition));
        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
    }

    [Fact]
    public void Reload_ProvenanceKeepsBuiltInVisibleWhenGlobalFallbackExists()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.GlobalActivePath);
        WritePack(temp.GlobalActivePath, "global.fields.json", "Armor", "Unknown", "External");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        service.Reload(null);

        FieldRegistryProvenanceLookupResult lookup = service.CurrentProvenanceProvider.TryGetFieldWithProvenance(
            Ra2SectionKind.Infantry,
            "Armor");
        Assert.True(lookup.Found);
        Assert.Equal(FieldRegistryProvenanceScope.BuiltIn, lookup.Scope);
    }

    [Fact]
    public void Reload_InvalidJsonCreatesWarningButProviderStillWorks()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.GlobalActivePath);
        File.WriteAllText(Path.Combine(temp.GlobalActivePath, "bad.fields.json"), "{ not json");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        IRa2FieldDefinitionProvider provider = service.Reload(null);

        Assert.Single(service.CurrentState.Warnings);
        Assert.True(provider.IsKnownField(Ra2SectionKind.Infantry, "Owner"));
    }

    [Fact]
    public void Reload_DoesNotThrowOnMissingProjectDirectory()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        Exception? exception = Record.Exception(() => service.Reload(temp.ProjectRootPath));

        Assert.Null(exception);
        Assert.NotNull(service.CurrentState.Project);
        Assert.False(service.CurrentState.Project!.DirectoryExists);
    }

    [Fact]
    public void CaptureProviderSnapshot_InitialSnapshotUsesRevisionOneAndCurrentProvider()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        Ra2FieldRegistryProviderSnapshot snapshot = service.CaptureProviderSnapshot();

        Assert.Equal(1, snapshot.Revision);
        Assert.Same(snapshot.Provider, service.CurrentProvider);
    }

    [Fact]
    public void Reload_PublishesExactlyOneNewRevisionAndKeepsOldSnapshotStable()
    {
        using TempDirectory temp = TempDirectory.Create();
        Directory.CreateDirectory(temp.GlobalActivePath);
        WritePack(temp.GlobalActivePath, "global.fields.json", "SnapshotKey", "Infantry", "External");
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);
        Ra2FieldRegistryProviderSnapshot beforeReload = service.CaptureProviderSnapshot();

        IRa2FieldDefinitionProvider reloadedProvider = service.Reload(null);
        Ra2FieldRegistryProviderSnapshot afterReload = service.CaptureProviderSnapshot();

        Assert.Equal(beforeReload.Revision + 1, afterReload.Revision);
        Assert.Same(reloadedProvider, afterReload.Provider);
        Assert.Same(afterReload.Provider, service.CurrentProvider);
        Assert.NotSame(beforeReload.Provider, afterReload.Provider);
        Assert.False(beforeReload.Provider.IsKnownField(Ra2SectionKind.Infantry, "SnapshotKey"));
        Assert.True(afterReload.Provider.IsKnownField(Ra2SectionKind.Infantry, "SnapshotKey"));
        Assert.Equal(1, beforeReload.Revision);
    }

    [Fact]
    public void Reload_RepeatedSuccessesIncrementRevisionOncePerPublication()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryRuntimeService service = new(new LocalFieldRegistryLoader(), temp.GlobalActivePath);

        service.Reload(null);
        long firstReloadRevision = service.CaptureProviderSnapshot().Revision;
        service.Reload(null);
        long secondReloadRevision = service.CaptureProviderSnapshot().Revision;

        Assert.Equal(2, firstReloadRevision);
        Assert.Equal(3, secondReloadRevision);
    }

    private static void WritePack(string directoryPath, string fileName, string key, string appliesTo, string sourceKind)
    {
        File.WriteAllText(Path.Combine(directoryPath, fileName), $$"""
            {
              "fields": [
                {
                  "key": "{{key}}",
                  "appliesTo": ["{{appliesTo}}"],
                  "editorKind": "Text",
                  "sourceKind": "{{sourceKind}}"
                }
              ]
            }
            """);
    }

    private sealed class TempDirectory : IDisposable
    {
        private TempDirectory(string path)
        {
            Path = path;
            GlobalActivePath = System.IO.Path.Combine(path, "global-active");
            ProjectRootPath = System.IO.Path.Combine(path, "project");
            ProjectActivePath = System.IO.Path.Combine(ProjectRootPath, ".ra2inieditor", "field-registry", "active");
            Directory.CreateDirectory(ProjectRootPath);
        }

        public string Path { get; }

        public string GlobalActivePath { get; }

        public string ProjectRootPath { get; }

        public string ProjectActivePath { get; }

        public static TempDirectory Create()
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TempDirectory(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
