using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryProvenanceSnapshotTests
{
    [Fact]
    public void TryGetFieldWithProvenance_GlobalSpecificBeatsProjectUnknownFallback()
    {
        FieldRegistryProvenanceSnapshot snapshot = BuildSnapshot(
            global: [Loaded("Owner", [Ra2SectionKind.Infantry], "global.fields.json", FieldEditorKind.Text)],
            project: [Loaded("Owner", [Ra2SectionKind.Unknown], "project.fields.json", FieldEditorKind.Boolean)]);

        FieldRegistryProvenanceLookupResult result = snapshot.TryGetFieldWithProvenance(Ra2SectionKind.Infantry, "Owner");

        Assert.True(result.Found);
        Assert.Equal(FieldRegistryProvenanceScope.Global, result.Scope);
        Assert.Equal("global.fields.json", result.SourceName);
        Assert.Equal(FieldEditorKind.MultiSelect, result.Definition?.EditorKind);
    }

    [Fact]
    public void TryGetFieldWithProvenance_BuiltInSpecificBeatsGlobalUnknownFallback()
    {
        FieldRegistryProvenanceSnapshot snapshot = BuildSnapshot(
            global: [Loaded("Owner", [Ra2SectionKind.Unknown], "global.fields.json", FieldEditorKind.Text)],
            project: []);

        FieldRegistryProvenanceLookupResult result = snapshot.TryGetFieldWithProvenance(Ra2SectionKind.Infantry, "Owner");

        Assert.True(result.Found);
        Assert.Equal(FieldRegistryProvenanceScope.BuiltIn, result.Scope);
        Assert.Equal("BuiltIn", result.SourceName);
    }

    [Fact]
    public void TryGetFieldWithProvenance_BuiltInFallbackWorks()
    {
        FieldRegistryProvenanceSnapshot snapshot = BuildSnapshot(global: [], project: []);

        FieldRegistryProvenanceLookupResult result = snapshot.TryGetFieldWithProvenance(Ra2SectionKind.Infantry, "Owner");

        Assert.True(result.Found);
        Assert.Equal(FieldRegistryProvenanceScope.BuiltIn, result.Scope);
        Assert.Equal("BuiltIn", result.SourceName);
        Assert.Equal("Owner", result.Definition?.Key);
    }

    [Fact]
    public void TryGetFieldWithProvenance_EnrichesWeakProjectFieldWithBuiltInDetails()
    {
        FieldRegistryProvenanceSnapshot snapshot = BuildSnapshot(
            global: [],
            project:
            [
                Loaded("Primary", [Ra2SectionKind.Techno], "project.fields.json", FieldEditorKind.Text)
            ]);

        FieldRegistryProvenanceLookupResult result = snapshot.TryGetFieldWithProvenance(Ra2SectionKind.Infantry, "Primary");

        Assert.True(result.Found);
        Assert.Equal(FieldRegistryProvenanceScope.Project, result.Scope);
        Assert.Equal("project.fields.json", result.SourceName);
        Assert.Equal(FieldEditorKind.Reference, result.Definition?.EditorKind);
        Assert.Contains("weapon", result.Definition?.Description ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetFieldWithProvenance_EnrichesWeakExactProjectFieldWithAbstractBuiltInDetails()
    {
        FieldRegistryProvenanceSnapshot snapshot = BuildSnapshot(
            global: [],
            project:
            [
                Loaded("Primary", [Ra2SectionKind.Vehicle], "project.fields.json", FieldEditorKind.Text)
            ],
            builtInProvider: new StaticFieldDefinitionProvider(new Ra2FieldDefinition(
                "Primary",
                [Ra2SectionKind.Techno],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.Yuri,
                "YR built-in reference field: Primary.",
                new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference),
                displayName: null,
                aliases: null,
                examples: [new Ra2FieldExample("120mm")])));

        FieldRegistryProvenanceLookupResult result = snapshot.TryGetFieldWithProvenance(Ra2SectionKind.Vehicle, "Primary");

        Assert.True(result.Found);
        Assert.Equal(FieldRegistryProvenanceScope.Project, result.Scope);
        Assert.Equal("project.fields.json", result.SourceName);
        Assert.Equal(FieldEditorKind.Reference, result.Definition?.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Reference, result.Definition?.ValueMetadata.ValueKind);
        Assert.Equal("YR built-in reference field: Primary.", result.Definition?.Description);
        Assert.Equal("120mm", Assert.Single(result.Definition?.Examples ?? []).Value);
    }

    [Fact]
    public void TryGetFieldWithProvenance_DoesNotEnrichStrongProjectField()
    {
        FieldRegistryProvenanceSnapshot snapshot = BuildSnapshot(
            global: [],
            project:
            [
                Loaded(
                    "Primary",
                    [Ra2SectionKind.Techno],
                    "project.fields.json",
                    new Ra2FieldDefinition(
                        "Primary",
                        [Ra2SectionKind.Techno],
                        FieldEditorKind.Text,
                        Ra2FieldSourceKind.User,
                        "Custom primary text field."))
            ]);

        FieldRegistryProvenanceLookupResult result = snapshot.TryGetFieldWithProvenance(Ra2SectionKind.Infantry, "Primary");

        Assert.True(result.Found);
        Assert.Equal(FieldRegistryProvenanceScope.Project, result.Scope);
        Assert.Equal(FieldEditorKind.Text, result.Definition?.EditorKind);
        Assert.Equal("Custom primary text field.", result.Definition?.Description);
    }

    [Fact]
    public void TryGetFieldWithProvenance_NotFoundReturnsNone()
    {
        FieldRegistryProvenanceSnapshot snapshot = BuildSnapshot(global: [], project: []);

        FieldRegistryProvenanceLookupResult result = snapshot.TryGetFieldWithProvenance(Ra2SectionKind.Infantry, "DefinitelyNotARealKey");

        Assert.False(result.Found);
        Assert.Equal(FieldRegistryProvenanceScope.None, result.Scope);
        Assert.Null(result.Definition);
    }

    private static FieldRegistryProvenanceSnapshot BuildSnapshot(
        IReadOnlyList<LocalFieldRegistryLoadedDefinition> global,
        IReadOnlyList<LocalFieldRegistryLoadedDefinition> project,
        IRa2FieldDefinitionProvider? builtInProvider = null)
    {
        return new FieldRegistryProvenanceSnapshotBuilder().Build(
            new LocalFieldRegistryLoadResult(
                global.Select(loaded => loaded.Definition).ToArray(),
                [],
                global),
            new LocalFieldRegistryLoadResult(
                project.Select(loaded => loaded.Definition).ToArray(),
                [],
                project),
            builtInProvider ?? new BuiltInRa2FieldDefinitionProvider());
    }

    private static LocalFieldRegistryLoadedDefinition Loaded(
        string key,
        IReadOnlyCollection<Ra2SectionKind> appliesTo,
        string sourceFileName,
        FieldEditorKind editorKind)
    {
        return Loaded(
            key,
            appliesTo,
            sourceFileName,
            new Ra2FieldDefinition(key, appliesTo, editorKind, Ra2FieldSourceKind.User),
            Path.Combine("active", sourceFileName));
    }

    private static LocalFieldRegistryLoadedDefinition Loaded(
        string key,
        IReadOnlyCollection<Ra2SectionKind> appliesTo,
        string sourceFileName,
        Ra2FieldDefinition definition)
    {
        return Loaded(key, appliesTo, sourceFileName, definition, Path.Combine("active", sourceFileName));
    }

    private static LocalFieldRegistryLoadedDefinition Loaded(
        string key,
        IReadOnlyCollection<Ra2SectionKind> appliesTo,
        string sourceFileName,
        Ra2FieldDefinition definition,
        string sourcePath)
    {
        return new LocalFieldRegistryLoadedDefinition(
            definition,
            sourceFileName,
            sourcePath);
    }

    private sealed class StaticFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        private readonly IReadOnlyList<Ra2FieldDefinition> _definitions;

        public StaticFieldDefinitionProvider(params Ra2FieldDefinition[] definitions)
        {
            _definitions = definitions;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = _definitions.FirstOrDefault(field =>
                AppliesToSectionKind(field, sectionKind) &&
                string.Equals(field.Key, key.Trim(), StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definitions.Where(field => AppliesToSectionKind(field, sectionKind)).ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);

        private static bool AppliesToSectionKind(Ra2FieldDefinition field, Ra2SectionKind sectionKind)
        {
            if (field.AppliesTo.Contains(sectionKind))
                return true;

            if (field.AppliesTo.Contains(Ra2SectionKind.Unit) &&
                sectionKind is Ra2SectionKind.Infantry or Ra2SectionKind.Vehicle or Ra2SectionKind.Aircraft)
            {
                return true;
            }

            if (field.AppliesTo.Contains(Ra2SectionKind.Techno) &&
                sectionKind is Ra2SectionKind.Infantry or
                    Ra2SectionKind.Vehicle or
                    Ra2SectionKind.Aircraft or
                    Ra2SectionKind.Building or
                    Ra2SectionKind.Unit)
            {
                return true;
            }

            return field.AppliesTo.Contains(Ra2SectionKind.Global) ||
                   field.AppliesTo.Contains(Ra2SectionKind.Unknown);
        }
    }
}
