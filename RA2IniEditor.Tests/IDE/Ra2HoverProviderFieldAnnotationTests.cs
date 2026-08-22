using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2HoverProviderFieldAnnotationTests
{
    [Fact]
    public void GetHover_AnnotatedKeyReturnsDisplayNameAliasesAndNote()
    {
        Ra2HoverProvider provider = new();
        Ra2DocumentSemanticModel model = BuildModel("[VehicleTypes]\n1=HTNK\n\n[HTNK]\nArmor=heavy\n");
        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, model.Snapshot.Text.IndexOf("Armor", StringComparison.Ordinal));

        Ra2HoverInfo? hover = provider.GetHover(
            model,
            context,
            CreateResolver(hasAnnotation: true),
            new EmptyProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Equal("Armor", hover.RawKey);
        Assert.Equal("装甲类型", hover.DisplayName);
        Assert.Equal("Enum", hover.TypeDisplay);
        Assert.Equal(["护甲"], hover.Aliases);
        Assert.Equal("单位使用的装甲类别。", hover.Description);
    }

    [Fact]
    public void GetHover_KnownKeyWithoutAnnotationFallsBackToFieldRegistry()
    {
        Ra2HoverProvider provider = new();
        Ra2DocumentSemanticModel model = BuildModel("[VehicleTypes]\n1=HTNK\n\n[HTNK]\nArmor=heavy\n");
        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, model.Snapshot.Text.IndexOf("Armor", StringComparison.Ordinal));

        Ra2HoverInfo? hover = provider.GetHover(
            model,
            context,
            CreateResolver(hasAnnotation: false),
            new EmptyProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Equal("Armor", hover.RawKey);
        Assert.Equal("Armor", hover.DisplayName);
        Assert.Equal("Enum", hover.TypeDisplay);
        Assert.Equal("Vehicle armor.", hover.Description);
    }

    private static Ra2DocumentSemanticModel BuildModel(string text)
    {
        Ra2DocumentSnapshot snapshot = new("rulesmd.ini", text, 1);
        return new Ra2DocumentSemanticModelBuilder().Build(snapshot, new StaticFieldProvider());
    }

    private static Ra2FieldDisplayResolver CreateResolver(bool hasAnnotation)
    {
        Ra2FieldAnnotationPack pack = hasAnnotation
            ? new Ra2FieldAnnotationPack(1, "zh-CN", [
                new Ra2FieldAnnotationEntry("Vehicle", "Armor", "装甲类型", ["护甲"], "单位使用的装甲类别。")
            ])
            : Ra2FieldAnnotationPack.Empty();
        return new Ra2FieldDisplayResolver(new StaticFieldProvider(), new Ra2FieldAnnotationProvider(pack));
    }

    private sealed class StaticFieldProvider : IRa2FieldDefinitionProvider
    {
        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = new Ra2FieldDefinition("Armor", [Ra2SectionKind.Vehicle], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn, "Vehicle armor.");
            return string.Equals(key, "Armor", StringComparison.OrdinalIgnoreCase);
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => [new Ra2FieldDefinition("Armor", [Ra2SectionKind.Vehicle], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn, "Vehicle armor.")];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => string.Equals(key, "Armor", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class EmptyProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(Ra2SectionKind sectionKind, string key)
            => FieldRegistryProvenanceLookupResult.NotFound;
    }
}
