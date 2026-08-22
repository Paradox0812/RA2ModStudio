using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EffectiveFieldCatalogTests
{
    [Fact]
    public void GetApplicableFields_DeduplicatesSameKeyAndPreservesAnnotation()
    {
        Ra2EffectiveFieldCatalog catalog = new(CreateResolver());

        IReadOnlyList<Ra2EffectiveFieldItem> fields = catalog.GetApplicableFields(Ra2SectionKind.Vehicle);

        Ra2EffectiveFieldItem armor = Assert.Single(fields, field => field.Key == "Armor");
        Assert.Equal("装甲类型", armor.DisplayInfo.DisplayName);
        Assert.Equal(Ra2FieldApplicabilityKind.Common, armor.Applicability);
    }

    [Fact]
    public void GetAllFields_DeduplicatesAcrossSectionKinds()
    {
        Ra2EffectiveFieldCatalog catalog = new(CreateResolver());

        IReadOnlyList<Ra2EffectiveFieldItem> fields = catalog.GetAllFields();

        Assert.Single(fields, field => field.Key == "Armor");
        Assert.Single(fields, field => field.Key == "Category");
        Assert.Single(fields, field => field.Key == "Cost");
    }

    private static Ra2FieldDisplayResolver CreateResolver()
    {
        return new Ra2FieldDisplayResolver(
            new StaticFieldProvider([
                new Ra2FieldDefinition("Armor", [Ra2SectionKind.Vehicle], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn, "Vehicle armor."),
                new Ra2FieldDefinition("Armor", [Ra2SectionKind.Vehicle], FieldEditorKind.Enum, Ra2FieldSourceKind.Custom, "Duplicate local armor."),
                new Ra2FieldDefinition("Armor", [Ra2SectionKind.Infantry], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn, "Infantry armor."),
                new Ra2FieldDefinition("Category", [Ra2SectionKind.Vehicle], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn, "Category."),
                new Ra2FieldDefinition("Category", [Ra2SectionKind.Infantry], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn, "Category."),
                new Ra2FieldDefinition("Cost", [Ra2SectionKind.Vehicle], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn, "Cost."),
                new Ra2FieldDefinition("Cost", [Ra2SectionKind.Building], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn, "Cost.")
            ]),
            new Ra2FieldAnnotationProvider(new Ra2FieldAnnotationPack(1, "zh-CN", [
                new Ra2FieldAnnotationEntry("Vehicle", "Armor", "装甲类型", ["护甲"], "单位使用的装甲类别。")
            ])));
    }

    private sealed class StaticFieldProvider : IRa2FieldDefinitionProvider
    {
        private readonly IReadOnlyList<Ra2FieldDefinition> _definitions;

        public StaticFieldProvider(IReadOnlyList<Ra2FieldDefinition> definitions)
        {
            _definitions = definitions;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = _definitions.FirstOrDefault(field =>
                field.AppliesTo.Contains(sectionKind) &&
                string.Equals(field.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definitions.Where(field => field.AppliesTo.Contains(sectionKind)).ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }
}
