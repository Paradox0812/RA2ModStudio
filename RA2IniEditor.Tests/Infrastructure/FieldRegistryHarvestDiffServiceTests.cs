using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryHarvestDiffServiceTests
{
    [Fact]
    public void Compare_MissingExistingFieldReturnsAdded()
    {
        FieldRegistryHarvestDiffResult result = Compare(
            [Definition("MyNewKey", Ra2SectionKind.Infantry)],
            new FakeFieldDefinitionProvider([]));

        FieldRegistryHarvestDiffRow row = Assert.Single(result.Rows);
        Assert.Equal(FieldRegistryHarvestDiffKind.Added, row.Kind);
        Assert.Equal(1, result.AddedCount);
    }

    [Fact]
    public void Compare_MatchingExistingFieldReturnsSame()
    {
        Ra2FieldDefinition definition = Definition("Owner", Ra2SectionKind.Infantry, FieldEditorKind.Text, Ra2FieldSourceKind.External, "Owner countries");

        FieldRegistryHarvestDiffResult result = Compare(
            [definition],
            new FakeFieldDefinitionProvider([definition]));

        FieldRegistryHarvestDiffRow row = Assert.Single(result.Rows);
        Assert.Equal(FieldRegistryHarvestDiffKind.Same, row.Kind);
        Assert.Equal(1, result.SameCount);
    }

    [Fact]
    public void Compare_DifferentEditorKindReturnsChanged()
    {
        Ra2FieldDefinition preview = Definition("Owner", Ra2SectionKind.Infantry, FieldEditorKind.Text);
        Ra2FieldDefinition existing = Definition("Owner", Ra2SectionKind.Infantry, FieldEditorKind.MultiSelect);

        FieldRegistryHarvestDiffResult result = Compare(
            [preview],
            new FakeFieldDefinitionProvider([existing]));

        FieldRegistryHarvestDiffRow row = Assert.Single(result.Rows);
        Assert.Equal(FieldRegistryHarvestDiffKind.Changed, row.Kind);
        Assert.Contains("EditorKind differs", row.Message, StringComparison.Ordinal);
        Assert.Equal(1, result.ChangedCount);
    }

    [Fact]
    public void Compare_WithProvenanceIncludesExistingScopeAndSourceName()
    {
        Ra2FieldDefinition preview = Definition("Owner", Ra2SectionKind.Infantry, FieldEditorKind.Text);
        Ra2FieldDefinition existing = Definition("Owner", Ra2SectionKind.Infantry, FieldEditorKind.MultiSelect);
        FakeProvenanceProvider provenanceProvider = new(
            new FieldRegistryProvenanceLookupResultFactory().Create(
                FieldRegistryProvenanceScope.Global,
                "global.fields.json",
                "C:\\active\\global.fields.json",
                existing));

        FieldRegistryHarvestPreviewDraft draft = new([preview], []);
        FieldRegistryHarvestDiffResult result = new FieldRegistryHarvestDiffService().Compare(draft, provenanceProvider);

        FieldRegistryHarvestDiffRow row = Assert.Single(result.Rows);
        Assert.Equal(FieldRegistryProvenanceScope.Global, row.ExistingScope);
        Assert.Equal("global.fields.json", row.ExistingSourceName);
        Assert.Equal("C:\\active\\global.fields.json", row.ExistingSourcePath);
    }

    [Fact]
    public void Compare_NullAndEmptyDescriptionsAreEquivalent()
    {
        Ra2FieldDefinition preview = Definition("Owner", Ra2SectionKind.Infantry, description: string.Empty);
        Ra2FieldDefinition existing = Definition("Owner", Ra2SectionKind.Infantry, description: null);

        FieldRegistryHarvestDiffResult result = Compare(
            [preview],
            new FakeFieldDefinitionProvider([existing]));

        FieldRegistryHarvestDiffRow row = Assert.Single(result.Rows);
        Assert.Equal(FieldRegistryHarvestDiffKind.Same, row.Kind);
    }

    [Fact]
    public void Compare_EmptyAppliesToReturnsInvalid()
    {
        Ra2FieldDefinition preview = new("Owner", [], FieldEditorKind.Text, Ra2FieldSourceKind.External);

        FieldRegistryHarvestDiffResult result = Compare(
            [preview],
            new FakeFieldDefinitionProvider([]));

        FieldRegistryHarvestDiffRow row = Assert.Single(result.Rows);
        Assert.Equal(FieldRegistryHarvestDiffKind.Invalid, row.Kind);
        Assert.Equal(1, result.InvalidCount);
    }

    private static FieldRegistryHarvestDiffResult Compare(
        IReadOnlyList<Ra2FieldDefinition> definitions,
        IRa2FieldDefinitionProvider provider)
    {
        FieldRegistryHarvestPreviewDraft draft = new(definitions, []);
        return new FieldRegistryHarvestDiffService().Compare(draft, provider);
    }

    private static Ra2FieldDefinition Definition(
        string key,
        Ra2SectionKind appliesTo,
        FieldEditorKind editorKind = FieldEditorKind.Text,
        Ra2FieldSourceKind sourceKind = Ra2FieldSourceKind.External,
        string? description = null)
    {
        return new Ra2FieldDefinition(key, [appliesTo], editorKind, sourceKind, description);
    }

    private sealed class FakeFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        private readonly IReadOnlyList<Ra2FieldDefinition> _definitions;

        public FakeFieldDefinitionProvider(IReadOnlyList<Ra2FieldDefinition> definitions)
        {
            _definitions = definitions;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = _definitions.FirstOrDefault(candidate =>
                candidate.AppliesTo.Contains(sectionKind) &&
                string.Equals(candidate.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definitions.Where(definition => definition.AppliesTo.Contains(sectionKind)).ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }

    private sealed class FakeProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        private readonly FieldRegistryProvenanceLookupResult _result;

        public FakeProvenanceProvider(FieldRegistryProvenanceLookupResult result)
        {
            _result = result;
        }

        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(Ra2SectionKind sectionKind, string key)
            => _result;
    }

    private sealed class FieldRegistryProvenanceLookupResultFactory
    {
        public FieldRegistryProvenanceLookupResult Create(
            FieldRegistryProvenanceScope scope,
            string sourceName,
            string? sourcePath,
            Ra2FieldDefinition definition)
        {
            FieldRegistryProvenanceEntry entry = new(
                definition.Key,
                definition.AppliesTo.First(),
                scope,
                sourceName,
                sourcePath,
                definition);
            return FieldRegistryProvenanceLookupResult.FromEntry(entry);
        }
    }
}
