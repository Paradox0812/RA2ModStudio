using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Tests.Core;

public sealed class Ra2FieldSchemaTests
{
    [Fact]
    public void Ra2FieldDefinition_StoresReadonlyFieldMetadata()
    {
        Ra2SectionKind[] appliesTo = [Ra2SectionKind.Infantry, Ra2SectionKind.Vehicle];

        Ra2FieldDefinition definition = new(
            "Owner",
            appliesTo,
            FieldEditorKind.MultiSelect,
            Ra2FieldSourceKind.BuiltIn,
            "Allowed owning houses.");

        appliesTo[0] = Ra2SectionKind.Building;

        Assert.Equal("Owner", definition.Key);
        Assert.Equal([Ra2SectionKind.Infantry, Ra2SectionKind.Vehicle], definition.AppliesTo);
        Assert.Equal(FieldEditorKind.MultiSelect, definition.EditorKind);
        Assert.Equal(Ra2FieldSourceKind.BuiltIn, definition.SourceKind);
        Assert.Equal("Allowed owning houses.", definition.Description);
        Assert.Null(definition.DisplayName);
        Assert.Empty(definition.Aliases);
    }

    [Fact]
    public void Ra2FieldDefinition_StoresDisplayNameAndAliases()
    {
        string[] aliases = [" HP ", "Health", "hp", ""];

        Ra2FieldDefinition definition = new(
            "Strength",
            [Ra2SectionKind.Vehicle],
            FieldEditorKind.Integer,
            Ra2FieldSourceKind.User,
            "Hit points.",
            displayName: " Health ",
            aliases: aliases);

        aliases[0] = "Changed";

        Assert.Equal("Health", definition.DisplayName);
        Assert.Equal(["HP", "Health"], definition.Aliases);
    }

    [Fact]
    public void Ra2FieldDefinition_StoresExamplesWithoutMutatingSourceCollection()
    {
        Ra2FieldExample[] examples =
        [
            new(" heavy ", " 重甲 ")
        ];

        Ra2FieldDefinition definition = new(
            "Armor",
            [Ra2SectionKind.Techno],
            FieldEditorKind.Enum,
            Ra2FieldSourceKind.BuiltIn,
            "Armor type.",
            valueMetadata: null,
            displayName: null,
            aliases: null,
            examples);

        examples[0] = new Ra2FieldExample("light");

        Ra2FieldExample example = Assert.Single(definition.Examples);
        Assert.Equal("heavy", example.Value);
        Assert.Equal("重甲", example.Description);
        var list = Assert.IsAssignableFrom<IList<Ra2FieldExample>>(definition.Examples);
        Assert.True(list.IsReadOnly);
    }

    [Fact]
    public void Ra2FieldDefinition_RejectsInvalidConstructionArguments()
    {
        Assert.Throws<ArgumentException>(() =>
            new Ra2FieldDefinition(string.Empty, [], FieldEditorKind.Text, Ra2FieldSourceKind.BuiltIn));

        Assert.Throws<ArgumentException>(() =>
            new Ra2FieldDefinition("Owner=", [], FieldEditorKind.Text, Ra2FieldSourceKind.BuiltIn));

        Assert.Throws<ArgumentNullException>(() =>
            new Ra2FieldDefinition("Owner", null!, FieldEditorKind.Text, Ra2FieldSourceKind.BuiltIn));
    }

    [Fact]
    public void BuiltInProvider_CanFindOwnerForInfantry()
    {
        BuiltInRa2FieldDefinitionProvider provider = new();

        bool found = provider.TryGetField(Ra2SectionKind.Infantry, "Owner", out Ra2FieldDefinition definition);

        Assert.True(found);
        Assert.Equal("Owner", definition.Key);
        Assert.Equal(FieldEditorKind.MultiSelect, definition.EditorKind);
        Assert.Equal(Ra2FieldSourceKind.BuiltIn, definition.SourceKind);
        Assert.Contains(Ra2SectionKind.Techno, definition.AppliesTo);
    }

    [Theory]
    [InlineData("Owner")]
    [InlineData("owner")]
    [InlineData("OWNER")]
    [InlineData(" Owner ")]
    public void BuiltInProvider_KeyLookupIsCaseInsensitive(string key)
    {
        BuiltInRa2FieldDefinitionProvider provider = new();

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, key, out Ra2FieldDefinition definition));
        Assert.Equal("Owner", definition.Key);
        Assert.True(provider.IsKnownField(Ra2SectionKind.Infantry, key));
    }

    [Fact]
    public void BuiltInProvider_UnknownKeyReturnsFalse()
    {
        BuiltInRa2FieldDefinitionProvider provider = new();

        Assert.False(provider.TryGetField(Ra2SectionKind.Infantry, "DefinitelyNotAField", out _));
        Assert.False(provider.IsKnownField(Ra2SectionKind.Infantry, "DefinitelyNotAField"));
    }

    [Fact]
    public void BuiltInProvider_UsesCommonFallbackDefinitions()
    {
        BuiltInRa2FieldDefinitionProvider provider = new();

        Assert.True(provider.TryGetField(Ra2SectionKind.Weapon, "Name", out Ra2FieldDefinition definition));
        Assert.Equal("Name", definition.Key);
        Assert.Empty(definition.AppliesTo);
    }

    [Fact]
    public void BuiltInProvider_GetFieldsReturnsRequestedKindWithDocumentedFallback()
    {
        BuiltInRa2FieldDefinitionProvider provider = new();

        IReadOnlyList<Ra2FieldDefinition> buildingFields = provider.GetFields(Ra2SectionKind.Building);
        string[] keys = buildingFields.Select(definition => definition.Key).ToArray();

        Assert.Contains("Name", keys);
        Assert.Contains("Owner", keys);
        Assert.DoesNotContain("Speed", keys);
        Assert.Equal(keys.Length, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Theory]
    [InlineData("Cameo", FieldEditorKind.Reference)]
    [InlineData("AltCameo", FieldEditorKind.Reference)]
    [InlineData("Voxel", FieldEditorKind.Boolean)]
    [InlineData("Remapable", FieldEditorKind.Boolean)]
    public void BuiltInProvider_ExposesMinimalArtObjectAuthoringGate(string key, FieldEditorKind editorKind)
    {
        BuiltInRa2FieldDefinitionProvider provider = new();

        Assert.True(provider.TryGetField(Ra2SectionKind.ArtObject, key, out Ra2FieldDefinition definition));
        Assert.Equal(editorKind, definition.EditorKind);
        Assert.Equal([Ra2SectionKind.ArtObject], definition.AppliesTo);
        Assert.False(provider.TryGetField(Ra2SectionKind.Techno, key, out _));
    }

    [Fact]
    public void BuiltInProvider_AbstractUnitAndTechnoDefinitionsApplyToConcreteKinds()
    {
        BuiltInRa2FieldDefinitionProvider provider = new();

        Assert.True(provider.TryGetField(Ra2SectionKind.Vehicle, "Speed", out Ra2FieldDefinition unitDefinition));
        Assert.Equal([Ra2SectionKind.Unit], unitDefinition.AppliesTo);

        Assert.True(provider.TryGetField(Ra2SectionKind.Building, "Armor", out Ra2FieldDefinition technoDefinition));
        Assert.Equal([Ra2SectionKind.Techno], technoDefinition.AppliesTo);
    }

    [Fact]
    public void BuiltInProvider_GetFieldsResultCannotBeExternallyMutated()
    {
        BuiltInRa2FieldDefinitionProvider provider = new();

        IReadOnlyList<Ra2FieldDefinition> fields = provider.GetFields(Ra2SectionKind.Infantry);

        Assert.NotEmpty(fields);
        var list = Assert.IsAssignableFrom<IList<Ra2FieldDefinition>>(fields);
        Assert.True(list.IsReadOnly);
    }

    [Fact]
    public void BuiltInProvider_DoesNotRequireFilesystemPath()
    {
        BuiltInRa2FieldDefinitionProvider provider = new();

        IReadOnlyList<Ra2FieldDefinition> fields = provider.GetFields(Ra2SectionKind.Vehicle);

        Assert.NotEmpty(fields);
    }

    [Fact]
    public void CompositeProvider_UsesProviderOrderForLookupPriority()
    {
        Ra2FieldDefinition local = new(
            "Owner",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            "Custom owner field.");
        Ra2FieldDefinition builtIn = new("Owner", [Ra2SectionKind.Infantry], FieldEditorKind.MultiSelect, Ra2FieldSourceKind.BuiltIn);
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(local),
            new StaticFieldDefinitionProvider(builtIn)
        ]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "owner", out Ra2FieldDefinition definition));
        Assert.Same(local, definition);
        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
    }

    [Fact]
    public void CompositeProvider_FallsBackToLowerPriorityProvider()
    {
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(),
            new BuiltInRa2FieldDefinitionProvider()
        ]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "Owner", out Ra2FieldDefinition definition));
        Assert.Equal(Ra2FieldSourceKind.BuiltIn, definition.SourceKind);
    }

    [Fact]
    public void CompositeProvider_TryGetField_DoesNotCallGetFieldsPerLookup()
    {
        CountingFieldDefinitionProvider highPriority = new(new Ra2FieldDefinition(
            "Primary",
            [Ra2SectionKind.Vehicle],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User));
        CountingFieldDefinitionProvider builtIn = new(new Ra2FieldDefinition(
            "Primary",
            [Ra2SectionKind.Techno],
            FieldEditorKind.Reference,
            Ra2FieldSourceKind.Yuri,
            "Primary weapon reference.",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference)));
        CompositeRa2FieldDefinitionProvider provider = new([highPriority, builtIn]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Vehicle, "Primary", out Ra2FieldDefinition definition));

        Assert.Equal(FieldEditorKind.Reference, definition.EditorKind);
        Assert.Equal(1, highPriority.TryGetFieldCallCount);
        Assert.Equal(1, builtIn.TryGetFieldCallCount);
        Assert.Equal(0, highPriority.GetFieldsCallCount);
        Assert.Equal(0, builtIn.GetFieldsCallCount);
    }

    [Fact]
    public void CompositeProvider_GetFields_CachesEffectiveFieldsPerSectionKind()
    {
        CountingFieldDefinitionProvider source = new(
            new Ra2FieldDefinition("Primary", [Ra2SectionKind.Vehicle], FieldEditorKind.Reference, Ra2FieldSourceKind.User),
            new Ra2FieldDefinition("Strength", [Ra2SectionKind.Techno], FieldEditorKind.Integer, Ra2FieldSourceKind.User));
        CompositeRa2FieldDefinitionProvider provider = new([source]);

        IReadOnlyList<Ra2FieldDefinition> first = provider.GetFields(Ra2SectionKind.Vehicle);
        IReadOnlyList<Ra2FieldDefinition> second = provider.GetFields(Ra2SectionKind.Vehicle);

        Assert.Same(first, second);
        Assert.Equal(1, source.GetFieldsCallCount);
    }

    [Fact]
    public void CompositeProvider_GetFieldsMergesWithoutDuplicateKeys()
    {
        Ra2FieldDefinition local = new("Owner", [Ra2SectionKind.Infantry], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(local),
            new BuiltInRa2FieldDefinitionProvider()
        ]);

        IReadOnlyList<Ra2FieldDefinition> fields = provider.GetFields(Ra2SectionKind.Infantry);

        Assert.Equal(1, fields.Count(field => string.Equals(field.Key, "Owner", StringComparison.OrdinalIgnoreCase)));
        Assert.Contains(fields, field => field.Key == "Owner" && field.SourceKind == Ra2FieldSourceKind.User);
    }

    [Fact]
    public void CompositeProvider_GetField_UsesProjectOverGlobalOverBuiltInWhenSpecificityMatches()
    {
        Ra2FieldDefinition project = new(
            "Armor",
            [Ra2SectionKind.Techno],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            "Custom armor field.");
        Ra2FieldDefinition global = new("Armor", [Ra2SectionKind.Techno], FieldEditorKind.Enum, Ra2FieldSourceKind.External);
        Ra2FieldDefinition builtIn = new("Armor", [Ra2SectionKind.Techno], FieldEditorKind.MultiSelect, Ra2FieldSourceKind.BuiltIn);
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(project),
            new StaticFieldDefinitionProvider(global),
            new StaticFieldDefinitionProvider(builtIn)
        ]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "armor", out Ra2FieldDefinition definition));
        Assert.Same(project, definition);
    }

    [Fact]
    public void CompositeProvider_GetField_EnrichesWeakUserFieldWithBuiltInDetails()
    {
        Ra2FieldDefinition project = new("Primary", [Ra2SectionKind.Techno], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        Ra2FieldDefinition builtIn = new(
            "Primary",
            [Ra2SectionKind.Techno],
            FieldEditorKind.Reference,
            Ra2FieldSourceKind.BuiltIn,
            "Primary weapon reference.",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference),
            "Primary Weapon",
            ["Weapon"],
            [new Ra2FieldExample("120mm", "Cannon weapon")]);
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(project),
            new StaticFieldDefinitionProvider(builtIn)
        ]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "Primary", out Ra2FieldDefinition definition));
        Assert.NotSame(project, definition);
        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
        Assert.Equal(FieldEditorKind.Reference, definition.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Reference, definition.ValueMetadata.ValueKind);
        Assert.Equal("Primary weapon reference.", definition.Description);
        Assert.Equal("Primary Weapon", definition.DisplayName);
        Assert.Contains("Weapon", definition.Aliases);
        Assert.Equal("120mm", Assert.Single(definition.Examples).Value);
    }

    [Fact]
    public void CompositeProvider_GetField_EnrichesWeakExactUserFieldWithAbstractYuriBuiltInField()
    {
        Ra2FieldDefinition project = new("Primary", [Ra2SectionKind.Vehicle], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        Ra2FieldDefinition builtIn = new(
            "Primary",
            [Ra2SectionKind.Techno],
            FieldEditorKind.Reference,
            Ra2FieldSourceKind.Yuri,
            "YR built-in reference field: Primary.",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference),
            displayName: null,
            aliases: null,
            examples: [new Ra2FieldExample("120mm")]);
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(project),
            new StaticFieldDefinitionProvider(builtIn)
        ]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Vehicle, "Primary", out Ra2FieldDefinition definition));
        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
        Assert.Equal(FieldEditorKind.Reference, definition.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Reference, definition.ValueMetadata.ValueKind);
        Assert.Equal("YR built-in reference field: Primary.", definition.Description);
        Assert.Equal("120mm", Assert.Single(definition.Examples).Value);
    }

    [Fact]
    public void CompositeProvider_GetField_DoesNotReplaceStrongUserFieldWithBuiltInDetails()
    {
        Ra2FieldDefinition project = new(
            "Primary",
            [Ra2SectionKind.Techno],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            "Custom primary text field.");
        Ra2FieldDefinition builtIn = new(
            "Primary",
            [Ra2SectionKind.Techno],
            FieldEditorKind.Reference,
            Ra2FieldSourceKind.BuiltIn,
            "Primary weapon reference.",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference));
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(project),
            new StaticFieldDefinitionProvider(builtIn)
        ]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "Primary", out Ra2FieldDefinition definition));
        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
        Assert.Equal(FieldEditorKind.Text, definition.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Unknown, definition.ValueMetadata.ValueKind);
        Assert.Equal("Custom primary text field.", definition.Description);
    }

    [Fact]
    public void CompositeProvider_DoesNotReplaceStrongExactUserFieldWithAbstractYuriBuiltInField()
    {
        Ra2FieldDefinition project = new(
            "Primary",
            [Ra2SectionKind.Vehicle],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            "User-defined Primary field.");
        Ra2FieldDefinition builtIn = new(
            "Primary",
            [Ra2SectionKind.Techno],
            FieldEditorKind.Reference,
            Ra2FieldSourceKind.Yuri,
            "YR built-in reference field: Primary.",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference));
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(project),
            new StaticFieldDefinitionProvider(builtIn)
        ]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Vehicle, "Primary", out Ra2FieldDefinition definition));
        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
        Assert.Equal(FieldEditorKind.Text, definition.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Unknown, definition.ValueMetadata.ValueKind);
        Assert.Equal("User-defined Primary field.", definition.Description);
    }

    [Fact]
    public void CompositeProvider_GetField_PrefersMoreSpecificBuiltInOverGenericProjectFallback()
    {
        Ra2FieldDefinition projectFallback = new("Armor", [Ra2SectionKind.Unknown], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        Ra2FieldDefinition builtInSpecific = new("Armor", [Ra2SectionKind.Techno], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn);
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(projectFallback),
            new StaticFieldDefinitionProvider(builtInSpecific)
        ]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "Armor", out Ra2FieldDefinition definition));
        Assert.Same(builtInSpecific, definition);
    }

    [Fact]
    public void CompositeProvider_GetFields_PreservesUnrelatedBuiltInFieldsWhenProjectProviderExists()
    {
        Ra2FieldDefinition project = new("CustomProjectKey", [Ra2SectionKind.Techno], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        Ra2FieldDefinition builtInPrimary = new("Primary", [Ra2SectionKind.Techno], FieldEditorKind.Reference, Ra2FieldSourceKind.BuiltIn);
        Ra2FieldDefinition builtInStrength = new("Strength", [Ra2SectionKind.Techno], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn);
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(project),
            new StaticFieldDefinitionProvider(builtInPrimary, builtInStrength)
        ]);

        IReadOnlyList<Ra2FieldDefinition> fields = provider.GetFields(Ra2SectionKind.Infantry);

        Assert.Contains(fields, field => field.Key == "CustomProjectKey");
        Assert.Contains(fields, field => field.Key == "Primary");
        Assert.Contains(fields, field => field.Key == "Strength");
    }

    [Fact]
    public void CompositeProvider_GetFields_PrefersMoreSpecificBuiltInOverGenericProjectFallback()
    {
        Ra2FieldDefinition projectFallback = new("Armor", [Ra2SectionKind.Unknown], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        Ra2FieldDefinition builtInSpecific = new("Armor", [Ra2SectionKind.Techno], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn);
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(projectFallback),
            new StaticFieldDefinitionProvider(builtInSpecific)
        ]);

        Ra2FieldDefinition definition = Assert.Single(provider.GetFields(Ra2SectionKind.Vehicle), field => field.Key == "Armor");

        Assert.Same(builtInSpecific, definition);
    }

    [Fact]
    public void CompositeProvider_GetFields_DeduplicatesCaseInsensitiveKeysWithPriority()
    {
        Ra2FieldDefinition project = new("armor", [Ra2SectionKind.Techno], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        Ra2FieldDefinition builtIn = new("Armor", [Ra2SectionKind.Techno], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn);
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(project),
            new StaticFieldDefinitionProvider(builtIn)
        ]);

        Ra2FieldDefinition definition = Assert.Single(provider.GetFields(Ra2SectionKind.Infantry), field =>
            string.Equals(field.Key, "Armor", StringComparison.OrdinalIgnoreCase));

        Assert.NotSame(project, definition);
        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
        Assert.Equal(FieldEditorKind.Enum, definition.EditorKind);
    }

    [Fact]
    public void CompositeProvider_GetFields_EnrichesWeakUserFieldWithBuiltInDetails()
    {
        Ra2FieldDefinition project = new("Primary", [Ra2SectionKind.Techno], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        Ra2FieldDefinition builtIn = new(
            "Primary",
            [Ra2SectionKind.Techno],
            FieldEditorKind.Reference,
            Ra2FieldSourceKind.BuiltIn,
            "Primary weapon reference.",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference));
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(project),
            new StaticFieldDefinitionProvider(builtIn)
        ]);

        Ra2FieldDefinition definition = Assert.Single(provider.GetFields(Ra2SectionKind.Vehicle), field =>
            string.Equals(field.Key, "Primary", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
        Assert.Equal(FieldEditorKind.Reference, definition.EditorKind);
        Assert.Equal("Primary weapon reference.", definition.Description);
    }

    [Fact]
    public void CompositeProvider_GetFields_EnrichesWeakExactUserFieldWithAbstractYuriBuiltInField()
    {
        Ra2FieldDefinition project = new("Primary", [Ra2SectionKind.Vehicle], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        Ra2FieldDefinition builtIn = new(
            "Primary",
            [Ra2SectionKind.Techno],
            FieldEditorKind.Reference,
            Ra2FieldSourceKind.Yuri,
            "YR built-in reference field: Primary.",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference),
            displayName: null,
            aliases: null,
            examples: [new Ra2FieldExample("120mm")]);
        CompositeRa2FieldDefinitionProvider provider = new([
            new StaticFieldDefinitionProvider(project),
            new StaticFieldDefinitionProvider(builtIn)
        ]);

        Ra2FieldDefinition definition = Assert.Single(provider.GetFields(Ra2SectionKind.Vehicle), field =>
            string.Equals(field.Key, "Primary", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
        Assert.Equal(FieldEditorKind.Reference, definition.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Reference, definition.ValueMetadata.ValueKind);
        Assert.Equal("YR built-in reference field: Primary.", definition.Description);
        Assert.Equal("120mm", Assert.Single(definition.Examples).Value);
    }

    [Fact]
    public void CompositeProvider_EmptyProviderListReturnsNoFields()
    {
        CompositeRa2FieldDefinitionProvider provider = new([]);

        Assert.False(provider.TryGetField(Ra2SectionKind.Infantry, "Owner", out _));
        Assert.Empty(provider.GetFields(Ra2SectionKind.Infantry));
        Assert.False(provider.IsKnownField(Ra2SectionKind.Infantry, "Owner"));
    }

    [Fact]
    public void CoreFieldSchema_DoesNotReferenceLegacyUiOrFieldDatabaseTypes()
    {
        string root = TestRepositoryRoot.Find();
        string schemaPath = Path.Combine(root, "RA2IniEditor.Core", "Schema", "Ra2FieldSchema.cs");
        string schemaText = File.ReadAllText(schemaPath);

        Assert.DoesNotContain("MainWindowViewModel", schemaText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2FieldDefinitionDatabase", schemaText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2EditorMetadata", schemaText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2FieldOptionProvider", schemaText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2SchemaProvider", schemaText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("System.Windows", schemaText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("System.IO", schemaText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.", schemaText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.", schemaText, StringComparison.OrdinalIgnoreCase);
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
            => Array.AsReadOnly(_definitions.Where(field => AppliesToSectionKind(field, sectionKind)).ToArray());

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);

        private static bool AppliesToSectionKind(Ra2FieldDefinition field, Ra2SectionKind sectionKind)
        {
            if (field.AppliesTo.Count == 0 || field.AppliesTo.Contains(sectionKind))
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

    private sealed class CountingFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        private readonly StaticFieldDefinitionProvider _inner;

        public CountingFieldDefinitionProvider(params Ra2FieldDefinition[] definitions)
        {
            _inner = new StaticFieldDefinitionProvider(definitions);
        }

        public int TryGetFieldCallCount { get; private set; }

        public int GetFieldsCallCount { get; private set; }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            TryGetFieldCallCount++;
            return _inner.TryGetField(sectionKind, key, out definition);
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
        {
            GetFieldsCallCount++;
            return _inner.GetFields(sectionKind);
        }

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }
}

