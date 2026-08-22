using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class LocalFieldRegistryLoaderTests
{
    [Fact]
    public void LoadDirectory_MissingDirectoryReturnsEmptyResult()
    {
        string missingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        LocalFieldRegistryLoader loader = new();

        LocalFieldRegistryLoadResult result = loader.LoadDirectory(missingDirectory);

        Assert.Empty(result.Definitions);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void LoadDirectory_ValidFieldsJsonLoadsDefinitions()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "test.fields.json", """
            {
              "name": "User Test",
              "fields": [
                {
                  "key": "MyCustomKey",
                  "appliesTo": ["Infantry", "Vehicle"],
                  "editorKind": "Boolean",
                  "sourceKind": "User",
                  "description": "Local test field",
                  "displayName": "My Custom Key",
                  "aliases": ["Custom", "Local Key", "custom"]
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Ra2FieldDefinition definition = Assert.Single(result.Definitions);
        Assert.Equal("MyCustomKey", definition.Key);
        Assert.Equal([Ra2SectionKind.Infantry, Ra2SectionKind.Vehicle], definition.AppliesTo);
        Assert.Equal(FieldEditorKind.Boolean, definition.EditorKind);
        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
        Assert.Equal("Local test field", definition.Description);
        Assert.Equal("My Custom Key", definition.DisplayName);
        Assert.Equal(["Custom", "Local Key"], definition.Aliases);
        LocalFieldRegistryLoadedDefinition loaded = Assert.Single(result.LoadedDefinitions);
        Assert.Same(definition, loaded.Definition);
        Assert.Equal("test.fields.json", loaded.SourceFileName);
        Assert.EndsWith("test.fields.json", loaded.SourceFilePath, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void LoadDirectory_PreservesQualityTag()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "quality.fields.json", """
            {
              "fields": [
                {
                  "key": "LooseKey",
                  "appliesTo": ["Techno"],
                  "editorKind": "Text",
                  "sourceKind": "BuiltIn",
                  "quality": "name-inferred-test"
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Ra2FieldDefinition definition = Assert.Single(result.Definitions);
        Assert.Equal("name-inferred-test", definition.RegistryQuality);
    }

    [Fact]
    public void LocalProvider_AbstractUnitDefinitionAppliesToConcreteKinds()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "unit.fields.json", """
            {
              "fields": [
                {
                  "key": "Armor",
                  "appliesTo": ["Unit"],
                  "editorKind": "Enum",
                  "sourceKind": "User"
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);
        LocalRa2FieldDefinitionProvider provider = new(result.Definitions);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, "Armor", out Ra2FieldDefinition definition));
        Assert.Equal([Ra2SectionKind.Unit], definition.AppliesTo);
        Assert.True(provider.TryGetField(Ra2SectionKind.Vehicle, "Armor", out _));
        Assert.True(provider.TryGetField(Ra2SectionKind.Aircraft, "Armor", out _));
        Assert.False(provider.TryGetField(Ra2SectionKind.Building, "Armor", out _));
    }

    [Fact]
    public void LoadDirectory_ValueSchemaLoadsAllowedValuesMetadata()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "values.fields.json", """
            {
              "fields": [
                {
                  "key": "Armor",
                  "appliesTo": ["Infantry"],
                  "editorKind": "Enum",
                  "sourceKind": "User",
                  "schema": {
                    "type": "Enum",
                    "enumName": "ArmorTypes",
                    "allowedValues": [
                      {
                        "value": "heavy",
                        "displayName": "Heavy armor",
                        "description": "Tank armor.",
                        "priority": 7
                      }
                    ]
                  }
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Ra2FieldDefinition definition = Assert.Single(result.Definitions);
        Assert.Equal(Ra2FieldValueKind.Enum, definition.ValueMetadata.ValueKind);
        Assert.Equal("ArmorTypes", definition.ValueMetadata.EnumName);
        Ra2FieldAllowedValue allowed = Assert.Single(definition.ValueMetadata.AllowedValues);
        Assert.Equal("heavy", allowed.Value);
        Assert.Equal("Heavy armor", allowed.DisplayName);
        Assert.Equal("Tank armor.", allowed.Description);
        Assert.Equal(7, allowed.Priority);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void LoadDirectory_LoadsFieldExamples()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "examples.fields.json", """
            {
              "fields": [
                {
                  "key": "Armor",
                  "appliesTo": ["Techno"],
                  "editorKind": "Enum",
                  "sourceKind": "Yuri",
                  "examples": [
                    { "value": " heavy ", "description": " 重甲 " }
                  ]
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Ra2FieldDefinition definition = Assert.Single(result.Definitions);
        Ra2FieldExample example = Assert.Single(definition.Examples);
        Assert.Equal("heavy", example.Value);
        Assert.Equal("重甲", example.Description);
        Assert.Empty(definition.ValueMetadata.AllowedValues);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void LoadDirectory_SkipsInvalidFieldExamplesAndRecordsWarnings()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "examples.fields.json", """
            {
              "fields": [
                {
                  "key": "Armor",
                  "editorKind": "Enum",
                  "examples": [
                    { "value": "heavy" },
                    { "description": "missing value" },
                    { "value": " " }
                  ]
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Ra2FieldDefinition definition = Assert.Single(result.Definitions);
        Assert.Equal("heavy", Assert.Single(definition.Examples).Value);
        Assert.Equal(2, result.Warnings.Count(warning => warning.Contains("examples entry", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void LoadDirectory_DeduplicatesFieldExamplesByValue()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "examples.fields.json", """
            {
              "fields": [
                {
                  "key": "Armor",
                  "editorKind": "Enum",
                  "examples": [
                    { "value": "Heavy", "description": "first" },
                    { "value": "heavy", "description": "second" },
                    { "value": "HEAVY", "description": "third" }
                  ]
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Ra2FieldExample example = Assert.Single(Assert.Single(result.Definitions).Examples);
        Assert.Equal("Heavy", example.Value);
        Assert.Equal("first", example.Description);
    }

    [Fact]
    public void LoadDirectory_ValueSchemaLoadsBooleanStyleAndListSeparator()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "values.fields.json", """
            {
              "fields": [
                {
                  "key": "CustomBool",
                  "editorKind": "Boolean",
                  "schema": {
                    "type": "Boolean",
                    "booleanStyle": "TrueFalse"
                  }
                },
                {
                  "key": "Owner",
                  "editorKind": "MultiSelect",
                  "schema": {
                    "type": "EnumList",
                    "separator": "|",
                    "allowedValues": [
                      { "value": "Americans" }
                    ]
                  }
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Ra2FieldDefinition boolDefinition = Assert.Single(result.Definitions, definition => definition.Key == "CustomBool");
        Assert.Equal(Ra2FieldValueKind.Boolean, boolDefinition.ValueMetadata.ValueKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.TrueFalse, boolDefinition.ValueMetadata.BooleanStyle);

        Ra2FieldDefinition listDefinition = Assert.Single(result.Definitions, definition => definition.Key == "Owner");
        Assert.Equal(Ra2FieldValueKind.EnumList, listDefinition.ValueMetadata.ValueKind);
        Assert.Equal("|", listDefinition.ValueMetadata.Separator);
        Assert.Equal("Americans", Assert.Single(listDefinition.ValueMetadata.AllowedValues).Value);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void LoadDirectory_MissingValueSchemaKeepsUnknownMetadataForCompatibility()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "legacy.fields.json", """
            {
              "fields": [
                {
                  "key": "LegacyKey",
                  "editorKind": "Text"
                }
              ]
            }
            """);

        Ra2FieldDefinition definition = Assert.Single(new LocalFieldRegistryLoader().LoadDirectory(temp.Path).Definitions);

        Assert.Same(Ra2FieldValueMetadata.Unknown, definition.ValueMetadata);
        Assert.False(definition.ValueMetadata.HasSchema);
    }

    [Fact]
    public void LoadDirectory_InvalidValueSchemaRecordsWarningsWithoutSkippingField()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "bad-schema.fields.json", """
            {
              "fields": [
                {
                  "key": "LooseKey",
                  "editorKind": "Boolean",
                  "schema": {
                    "type": "NotReal",
                    "booleanStyle": "AlsoNotReal",
                    "allowedValues": [
                      { "displayName": "Missing raw value" }
                    ]
                  }
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Ra2FieldDefinition definition = Assert.Single(result.Definitions);
        Assert.Equal(Ra2FieldValueKind.Unknown, definition.ValueMetadata.ValueKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.Unknown, definition.ValueMetadata.BooleanStyle);
        Assert.Empty(definition.ValueMetadata.AllowedValues);
        Assert.Contains(result.Warnings, warning => warning.Contains("unknown schema type", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, warning => warning.Contains("unknown booleanStyle", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, warning => warning.Contains("without value", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void LoadDirectory_RecordsSourceFileForEachLoadedDefinition()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "a.fields.json", """
            {
              "fields": [
                { "key": "AKey", "appliesTo": ["Infantry"], "editorKind": "Text" }
              ]
            }
            """);
        WritePack(temp.Path, "b.fields.json", """
            {
              "fields": [
                { "key": "BKey", "appliesTo": ["Building"], "editorKind": "Boolean" }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Assert.Equal(["AKey", "BKey"], result.Definitions.Select(definition => definition.Key).ToArray());
        Assert.Equal(["a.fields.json", "b.fields.json"], result.LoadedDefinitions.Select(loaded => loaded.SourceFileName).ToArray());
    }

    [Fact]
    public void LoadDirectory_InvalidJsonReturnsWarningAndKeepsOtherFilesLoaded()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "bad.fields.json", "{ not json");
        WritePack(temp.Path, "good.fields.json", """
            {
              "fields": [
                {
                  "key": "GoodKey",
                  "appliesTo": ["Building"],
                  "editorKind": "Text",
                  "sourceKind": "External"
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Assert.Single(result.Definitions);
        Assert.Equal("GoodKey", result.Definitions[0].Key);
        Assert.Single(result.Warnings);
        Assert.Contains("Failed to load field registry file", result.Warnings[0]);
    }

    [Fact]
    public void LoadDirectory_FieldWithoutKeyIsSkipped()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "test.fields.json", """
            {
              "fields": [
                {
                  "appliesTo": ["Infantry"],
                  "editorKind": "Text",
                  "sourceKind": "External"
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Assert.Empty(result.Definitions);
        Assert.Single(result.Warnings);
        Assert.Contains("key is missing", result.Warnings[0]);
    }

    [Fact]
    public void LoadDirectory_UnknownEditorAndSourceKindsUseDefaults()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "test.fields.json", """
            {
              "fields": [
                {
                  "key": "LooseKey",
                  "appliesTo": ["Unknown"],
                  "editorKind": "NotReal",
                  "sourceKind": "NotReal"
                }
              ]
            }
            """);

        Ra2FieldDefinition definition = Assert.Single(new LocalFieldRegistryLoader().LoadDirectory(temp.Path).Definitions);

        Assert.Equal(FieldEditorKind.Text, definition.EditorKind);
        Assert.Equal(Ra2FieldSourceKind.External, definition.SourceKind);
    }

    [Fact]
    public void LoadDirectory_UnknownAppliesToSkipsFieldWithWarning()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "test.fields.json", """
            {
              "fields": [
                {
                  "key": "LooseKey",
                  "appliesTo": ["NotASection"],
                  "editorKind": "Text",
                  "sourceKind": "External"
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Assert.Empty(result.Definitions);
        Assert.Equal(2, result.Warnings.Count);
        Assert.Contains(result.Warnings, warning => warning.Contains("unknown appliesTo value 'NotASection'", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, warning => warning.Contains("none of its appliesTo values are supported", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void LoadDirectory_NormalizesCompositeAppliesTo()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "test.fields.json", """
            {
              "fields": [
                {
                  "key": "DualKey",
                  "appliesTo": ["Building or Vehicle", "Techno or SW"],
                  "editorKind": "Text"
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Ra2FieldDefinition definition = Assert.Single(result.Definitions);
        Assert.Equal(
            [Ra2SectionKind.Building, Ra2SectionKind.Vehicle, Ra2SectionKind.Techno, Ra2SectionKind.SuperWeapon],
            definition.AppliesTo);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void LoadDirectory_LoadsArtAsArtObject()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "test.fields.json", """
            {
              "fields": [
                {
                  "key": "Foundation",
                  "appliesTo": ["Art", "UnitArt"],
                  "editorKind": "Text"
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Ra2FieldDefinition definition = Assert.Single(result.Definitions);
        Assert.Equal([Ra2SectionKind.ArtObject], definition.AppliesTo);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void LoadDirectory_OnlyReadsFieldsJsonFiles()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, "ignored.json", """
            {
              "fields": [
                {
                  "key": "IgnoredKey",
                  "appliesTo": ["Infantry"],
                  "editorKind": "Text",
                  "sourceKind": "External"
                }
              ]
            }
            """);

        LocalFieldRegistryLoadResult result = new LocalFieldRegistryLoader().LoadDirectory(temp.Path);

        Assert.Empty(result.Definitions);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void LocalProvider_TryGetFieldIsCaseInsensitiveAndRespectsSectionKind()
    {
        Ra2FieldDefinition definition = new(
            "MyCustomKey",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User);
        LocalRa2FieldDefinitionProvider provider = new([definition]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Infantry, " mycustomkey ", out Ra2FieldDefinition found));
        Assert.Same(definition, found);
        Assert.False(provider.TryGetField(Ra2SectionKind.Building, "MyCustomKey", out _));
    }

    [Fact]
    public void LocalProvider_GlobalAndUnknownFallbackWorks()
    {
        Ra2FieldDefinition global = new("GlobalKey", [Ra2SectionKind.Global], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        Ra2FieldDefinition unknown = new("LooseKey", [Ra2SectionKind.Unknown], FieldEditorKind.Text, Ra2FieldSourceKind.User);
        LocalRa2FieldDefinitionProvider provider = new([global, unknown]);

        Assert.True(provider.IsKnownField(Ra2SectionKind.Vehicle, "GlobalKey"));
        Assert.True(provider.IsKnownField(Ra2SectionKind.Vehicle, "LooseKey"));
    }

    [Fact]
    public void LocalProvider_GetFieldsReturnsReadonlyCollection()
    {
        LocalRa2FieldDefinitionProvider provider = new([
            new Ra2FieldDefinition("LocalKey", [Ra2SectionKind.Building], FieldEditorKind.Text, Ra2FieldSourceKind.User)
        ]);

        IReadOnlyList<Ra2FieldDefinition> fields = provider.GetFields(Ra2SectionKind.Building);

        var list = Assert.IsAssignableFrom<IList<Ra2FieldDefinition>>(fields);
        Assert.True(list.IsReadOnly);
    }

    [Fact]
    public void LocalProvider_DuplicateKeyKeepsDeterministicLastDefinition()
    {
        Ra2FieldDefinition first = new("LocalKey", [Ra2SectionKind.Building], FieldEditorKind.Text, Ra2FieldSourceKind.External);
        Ra2FieldDefinition second = new("LocalKey", [Ra2SectionKind.Building], FieldEditorKind.Boolean, Ra2FieldSourceKind.User);
        LocalRa2FieldDefinitionProvider provider = new([first, second]);

        Assert.True(provider.TryGetField(Ra2SectionKind.Building, "LocalKey", out Ra2FieldDefinition found));
        Assert.Same(second, found);
    }

    private static void WritePack(string directoryPath, string fileName, string text)
        => File.WriteAllText(Path.Combine(directoryPath, fileName), text);

    private sealed class TempDirectory : IDisposable
    {
        private TempDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

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
