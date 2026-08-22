using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldDisplayResolverTests
{
    [Fact]
    public void Resolve_AnnotationDisplayNameWinsOverRawKey()
    {
        Ra2FieldDisplayResolver resolver = CreateResolver(
            [new Ra2FieldDefinition("Strength", [Ra2SectionKind.Vehicle], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn, "Object hit points.")],
            [new Ra2FieldAnnotationEntry("Vehicle", "Strength", "Health", ["HP"], "Maximum hit points.")]);

        Ra2FieldDisplayInfo info = resolver.Resolve(Ra2SectionKind.Vehicle, "Strength");

        Assert.Equal("Strength", info.Key);
        Assert.Equal("Health", info.DisplayName);
        Assert.Equal(["HP"], info.Aliases);
        Assert.Equal("Maximum hit points.", info.Note);
        Assert.Equal("Object hit points.", info.Description);
        Assert.Equal("Integer", info.TypeDisplay);
        Assert.True(info.HasUserAnnotation);
    }

    [Fact]
    public void Resolve_WithoutAnnotationFallsBackToRawKeyAndDefinition()
    {
        Ra2FieldDisplayResolver resolver = CreateResolver(
            [new Ra2FieldDefinition("Armor", [Ra2SectionKind.Vehicle], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn, "Armor type.")],
            []);

        Ra2FieldDisplayInfo info = resolver.Resolve(Ra2SectionKind.Vehicle, "Armor");

        Assert.Equal("Armor", info.DisplayName);
        Assert.Equal("Enum", info.TypeDisplay);
        Assert.Equal("Armor type.", info.Description);
        Assert.False(info.HasUserAnnotation);
    }

    [Fact]
    public void Resolve_WithoutDefinitionFallsBackToRawKey()
    {
        Ra2FieldDisplayResolver resolver = CreateResolver([], []);

        Ra2FieldDisplayInfo info = resolver.Resolve(Ra2SectionKind.Vehicle, "CustomKey");

        Assert.Equal("CustomKey", info.Key);
        Assert.Equal("CustomKey", info.DisplayName);
        Assert.Equal("Unknown", info.TypeDisplay);
        Assert.Equal("Unknown", info.SourceDisplay);
    }

    [Fact]
    public void GetFields_ReturnsResolvedFieldDefinitionsForSection()
    {
        Ra2FieldDisplayResolver resolver = CreateResolver(
            [
                new Ra2FieldDefinition("Strength", [Ra2SectionKind.Vehicle], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn),
                new Ra2FieldDefinition("Primary", [Ra2SectionKind.Vehicle], FieldEditorKind.Reference, Ra2FieldSourceKind.BuiltIn)
            ],
            [new Ra2FieldAnnotationEntry("Vehicle", "Primary", "Main Weapon")]);

        IReadOnlyList<Ra2FieldDisplayInfo> fields = resolver.GetFields(Ra2SectionKind.Vehicle);

        Assert.Equal(["Primary", "Strength"], fields.Select(field => field.Key).ToArray());
        Assert.Equal("Main Weapon", fields[0].DisplayName);
    }

    private static Ra2FieldDisplayResolver CreateResolver(
        IReadOnlyList<Ra2FieldDefinition> definitions,
        IReadOnlyList<Ra2FieldAnnotationEntry> annotations)
    {
        return new Ra2FieldDisplayResolver(
            new StaticFieldProvider(definitions),
            new Ra2FieldAnnotationProvider(new Ra2FieldAnnotationPack(1, "zh-CN", annotations)));
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
