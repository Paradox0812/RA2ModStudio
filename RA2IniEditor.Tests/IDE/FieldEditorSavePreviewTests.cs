using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Services.FieldRegistry;
using RA2IniEditor.IDE.ViewModels.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldEditorSavePreviewTests
{
    [Fact]
    public void DraftFactory_ParsesAliasesAndAllowedValuesWithoutWritingFiles()
    {
        FieldEditorViewModel viewModel = new()
        {
            Key = " Armor ",
            SectionKind = Ra2SectionKind.Vehicle,
            EditorKind = FieldEditorKind.Enum,
            ValueKind = Ra2FieldValueKind.Enum,
            BooleanStyle = Ra2FieldBooleanValueStyle.TrueFalse,
            EnumName = " ArmorType ",
            Separator = ";",
            DisplayName = " Armor Type ",
            AliasesText = "Armor, Protection; ArmorClass",
            Description = " Armor description ",
            AllowedValuesText = "light | 杞荤敳 | Light armor\r\nheavy | 閲嶇敳"
        };

        FieldEditorDraft draft = new FieldEditorDraftFactory().CreateDraft(viewModel, FieldEditorSaveTarget.Project);

        Assert.Equal("Armor", draft.Key);
        Assert.Equal(Ra2SectionKind.Vehicle, draft.SectionKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.TrueFalse, draft.BooleanStyle);
        Assert.Equal("ArmorType", draft.EnumName);
        Assert.Equal(";", draft.Separator);
        Assert.Equal("Armor Type", draft.DisplayName);
        Assert.Equal(["Armor", "Protection", "ArmorClass"], draft.Aliases);
        Assert.Equal("Armor description", draft.Description);
        Assert.Collection(
            draft.AllowedValues,
            value =>
            {
                Assert.Equal("light", value.Value);
                Assert.Equal("杞荤敳", value.DisplayName);
                Assert.Equal("Light armor", value.Description);
            },
            value =>
            {
                Assert.Equal("heavy", value.Value);
                Assert.Equal("閲嶇敳", value.DisplayName);
                Assert.Null(value.Description);
            });
    }

    [Fact]
    public void FieldEditorViewModel_FromExistingDefinitionPreservesValueMetadata()
    {
        Ra2FieldDefinition booleanDefinition = new(
            "IsSelectable",
            [Ra2SectionKind.Unit],
            FieldEditorKind.Boolean,
            Ra2FieldSourceKind.User,
            valueMetadata: new Ra2FieldValueMetadata(
                Ra2FieldValueKind.Boolean,
                Ra2FieldBooleanValueStyle.YesNo));
        FieldEditorViewModel booleanViewModel = new(booleanDefinition, Ra2SectionKind.Unit);

        Assert.Equal(Ra2FieldValueKind.Boolean, booleanViewModel.ValueKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.YesNo, booleanViewModel.BooleanStyle);
        Assert.True(booleanViewModel.IsBooleanStyleEditable);

        Ra2FieldDefinition listDefinition = new(
            "Owner",
            [Ra2SectionKind.Unit],
            FieldEditorKind.MultiSelect,
            Ra2FieldSourceKind.User,
            valueMetadata: new Ra2FieldValueMetadata(
                Ra2FieldValueKind.EnumList,
                allowedValues: [new Ra2FieldAllowedValue("GDI")],
                separator: ";"));
        FieldEditorViewModel listViewModel = new(listDefinition, Ra2SectionKind.Unit);

        Assert.Equal(Ra2FieldValueKind.EnumList, listViewModel.ValueKind);
        Assert.Equal(";", listViewModel.Separator);
        Assert.True(listViewModel.IsSeparatorEditable);
    }

    [Fact]
    public void PreviewBuilder_BlocksEmptyKeyAndKeyWithEquals()
    {
        FieldEditorSavePreviewBuilder builder = new();
        StaticFieldProvider provider = new();

        FieldEditorSavePreview empty = builder.BuildPreview(
            Draft(string.Empty, Ra2SectionKind.Vehicle),
            provider);
        FieldEditorSavePreview withEquals = builder.BuildPreview(
            Draft("Speed=1", Ra2SectionKind.Vehicle),
            provider);

        Assert.Equal(FieldEditorSaveOperationKind.Blocked, empty.OperationKind);
        Assert.False(empty.CanSave);
        Assert.Contains(empty.Issues, issue => issue.Code == "FE0001");
        Assert.Equal(FieldEditorSaveOperationKind.Blocked, withEquals.OperationKind);
        Assert.False(withEquals.CanSave);
        Assert.Contains(withEquals.Issues, issue => issue.Code == "FE0002");
    }

    [Fact]
    public void PreviewBuilder_BlocksDuplicateAllowedValues()
    {
        FieldEditorDraft draft = Draft(
            "Armor",
            Ra2SectionKind.Unknown,
            allowedValues:
            [
                new FieldEditorAllowedValueDraft("light"),
                new FieldEditorAllowedValueDraft("LIGHT")
            ]);

        FieldEditorSavePreview preview = new FieldEditorSavePreviewBuilder().BuildPreview(draft, new StaticFieldProvider());

        Assert.Equal(FieldEditorSaveOperationKind.Blocked, preview.OperationKind);
        Assert.False(preview.CanSave);
        Assert.Contains(preview.Issues, issue => issue.Code == "FE0003");
        Assert.Contains(preview.Issues, issue => issue.Code == "FE0005" && issue.Severity == FieldEditorValidationSeverity.Error);
    }

    [Fact]
    public void PreviewBuilder_ExistingBuiltInFieldProducesOverrideWarningWhenChanged()
    {
        StaticFieldProvider provider = new(
            new Ra2FieldDefinition(
                "Armor",
                [Ra2SectionKind.Vehicle],
                FieldEditorKind.Enum,
                Ra2FieldSourceKind.BuiltIn,
                "Armor type.",
                new Ra2FieldValueMetadata(Ra2FieldValueKind.Enum)));

        FieldEditorDraft draft = Draft(
            "Armor",
            Ra2SectionKind.Vehicle,
            editorKind: FieldEditorKind.Enum,
            valueKind: Ra2FieldValueKind.Enum,
            description: "Custom armor type.");

        FieldEditorSavePreview preview = new FieldEditorSavePreviewBuilder().BuildPreview(draft, provider);

        Assert.Equal(FieldEditorSaveOperationKind.OverrideBuiltIn, preview.OperationKind);
        Assert.True(preview.CanSave);
        Assert.Contains(preview.Issues, issue => issue.Code == "FE0006");
        Assert.Contains("不会修改内置字段库", preview.Summary);
    }

    [Fact]
    public void PreviewBuilder_ExistingUserFieldProducesUpdateWhenChanged()
    {
        StaticFieldProvider provider = new(
            new Ra2FieldDefinition(
                "CustomKey",
                [Ra2SectionKind.Infantry],
                FieldEditorKind.Text,
                Ra2FieldSourceKind.User,
                "Old description."));

        FieldEditorDraft draft = Draft("CustomKey", Ra2SectionKind.Infantry, description: "New description.");

        FieldEditorSavePreview preview = new FieldEditorSavePreviewBuilder().BuildPreview(draft, provider);

        Assert.Equal(FieldEditorSaveOperationKind.Update, preview.OperationKind);
        Assert.True(preview.CanSave);
    }

    [Fact]
    public void FieldEditorViewModel_DisablesApply_WhenEnumListSeparatorIsEmpty()
    {
        FieldEditorViewModel viewModel = new()
        {
            Key = "Owner",
            SectionKind = Ra2SectionKind.Unit,
            EditorKind = FieldEditorKind.MultiSelect,
            ValueKind = Ra2FieldValueKind.EnumList,
            Separator = string.Empty,
            AllowedValuesText = "GDI"
        };

        FieldEditorSavePreview preview = viewModel.BuildSavePreview(new StaticFieldProvider(), FieldEditorSaveTarget.Global);

        Assert.Equal(FieldEditorSaveOperationKind.Blocked, preview.OperationKind);
        Assert.False(viewModel.CanSave);
        Assert.Contains(viewModel.PreviewIssues, issue => issue.Code == "FE0008" && issue.Message.Contains("列表分隔符"));
        Assert.Contains("无效", viewModel.StatusText);
    }

    [Fact]
    public void FieldEditorViewModel_DisablesApply_WhenAllowedValueRawValueIsEmpty()
    {
        FieldEditorViewModel viewModel = new()
        {
            Key = "Armor",
            SectionKind = Ra2SectionKind.Unit,
            EditorKind = FieldEditorKind.Enum,
            ValueKind = Ra2FieldValueKind.Enum,
            AllowedValuesText = "| Missing raw value"
        };

        FieldEditorSavePreview preview = viewModel.BuildSavePreview(new StaticFieldProvider(), FieldEditorSaveTarget.Global);

        Assert.Equal(FieldEditorSaveOperationKind.Blocked, preview.OperationKind);
        Assert.False(viewModel.CanSave);
        Assert.Contains(viewModel.PreviewIssues, issue => issue.Code == "FE0007" && issue.Message.Contains("实际写入值"));
    }

    [Fact]
    public void FieldEditorViewModel_DisablesApply_WhenSeparatorIsTooLong()
    {
        FieldEditorViewModel viewModel = new()
        {
            Key = "Owner",
            SectionKind = Ra2SectionKind.Unit,
            EditorKind = FieldEditorKind.MultiSelect,
            ValueKind = Ra2FieldValueKind.EnumList,
            Separator = "::::",
            AllowedValuesText = "GDI"
        };

        FieldEditorSavePreview preview = viewModel.BuildSavePreview(new StaticFieldProvider(), FieldEditorSaveTarget.Global);

        Assert.Equal(FieldEditorSaveOperationKind.Blocked, preview.OperationKind);
        Assert.False(viewModel.CanSave);
        Assert.Contains(viewModel.PreviewIssues, issue => issue.Code == "FE0009" && issue.Message.Contains("1 到 3"));
    }

    [Fact]
    public void PreviewBuilder_NoMaterialChangeCannotSave()
    {
        StaticFieldProvider provider = new(
            new Ra2FieldDefinition(
                "Armor",
                [Ra2SectionKind.Vehicle],
                FieldEditorKind.Enum,
                Ra2FieldSourceKind.User,
                "Armor type.",
                new Ra2FieldValueMetadata(
                    Ra2FieldValueKind.Enum,
                    allowedValues:
                    [
                        new Ra2FieldAllowedValue("light"),
                        new Ra2FieldAllowedValue("heavy")
                    ])));

        FieldEditorDraft draft = Draft(
            "Armor",
            Ra2SectionKind.Vehicle,
            editorKind: FieldEditorKind.Enum,
            valueKind: Ra2FieldValueKind.Enum,
            description: "Armor type.",
            allowedValues:
            [
                new FieldEditorAllowedValueDraft("light"),
                new FieldEditorAllowedValueDraft("heavy")
            ]);

        FieldEditorSavePreview preview = new FieldEditorSavePreviewBuilder().BuildPreview(draft, provider);

        Assert.Equal(FieldEditorSaveOperationKind.NoChange, preview.OperationKind);
        Assert.False(preview.CanSave);
    }

    [Fact]
    public void PreviewBuilder_AllowedValueDisplayMetadataChangesProduceUpdate()
    {
        StaticFieldProvider provider = new(
            new Ra2FieldDefinition(
                "Armor",
                [Ra2SectionKind.Vehicle],
                FieldEditorKind.Enum,
                Ra2FieldSourceKind.User,
                valueMetadata: new Ra2FieldValueMetadata(
                    Ra2FieldValueKind.Enum,
                    allowedValues:
                    [
                        new Ra2FieldAllowedValue("light", "Light", "Old description.")
                    ])));

        FieldEditorDraft draft = Draft(
            "Armor",
            Ra2SectionKind.Vehicle,
            editorKind: FieldEditorKind.Enum,
            valueKind: Ra2FieldValueKind.Enum,
            allowedValues:
            [
                new FieldEditorAllowedValueDraft("light", "Light Armor", "New description.")
            ]);

        FieldEditorSavePreview preview = new FieldEditorSavePreviewBuilder().BuildPreview(draft, provider);

        Assert.Equal(FieldEditorSaveOperationKind.Update, preview.OperationKind);
        Assert.True(preview.CanSave);
        Assert.Contains("可选值", preview.Summary);
    }

    [Fact]
    public void PreviewBuilder_ReportsUpdate_WhenBooleanStyleChanged()
    {
        StaticFieldProvider provider = new(
            new Ra2FieldDefinition(
                "IsSelectable",
                [Ra2SectionKind.Unit],
                FieldEditorKind.Boolean,
                Ra2FieldSourceKind.User,
                valueMetadata: new Ra2FieldValueMetadata(
                    Ra2FieldValueKind.Boolean,
                    Ra2FieldBooleanValueStyle.YesNo)));

        FieldEditorDraft draft = Draft(
            "IsSelectable",
            Ra2SectionKind.Unit,
            editorKind: FieldEditorKind.Boolean,
            valueKind: Ra2FieldValueKind.Boolean,
            booleanStyle: Ra2FieldBooleanValueStyle.TrueFalse);

        FieldEditorSavePreview preview = new FieldEditorSavePreviewBuilder().BuildPreview(draft, provider);

        Assert.Equal(FieldEditorSaveOperationKind.Update, preview.OperationKind);
        Assert.Contains("布尔值风格", preview.Summary);
    }

    [Fact]
    public void PreviewBuilder_ReportsUpdate_WhenSeparatorChanged()
    {
        StaticFieldProvider provider = new(
            new Ra2FieldDefinition(
                "Owner",
                [Ra2SectionKind.Unit],
                FieldEditorKind.MultiSelect,
                Ra2FieldSourceKind.User,
                valueMetadata: new Ra2FieldValueMetadata(
                    Ra2FieldValueKind.EnumList,
                    allowedValues: [new Ra2FieldAllowedValue("GDI")],
                    separator: ",")));

        FieldEditorDraft draft = Draft(
            "Owner",
            Ra2SectionKind.Unit,
            editorKind: FieldEditorKind.MultiSelect,
            valueKind: Ra2FieldValueKind.EnumList,
            allowedValues: [new FieldEditorAllowedValueDraft("GDI")],
            separator: ";");

        FieldEditorSavePreview preview = new FieldEditorSavePreviewBuilder().BuildPreview(draft, provider);

        Assert.Equal(FieldEditorSaveOperationKind.Update, preview.OperationKind);
        Assert.Contains("列表分隔符", preview.Summary);
    }

    [Fact]
    public void FieldEditorViewModel_BuildSavePreviewUpdatesReadonlyPreviewState()
    {
        FieldEditorViewModel viewModel = new()
        {
            Key = "NewField",
            SectionKind = Ra2SectionKind.Vehicle,
            EditorKind = FieldEditorKind.Text,
            ValueKind = Ra2FieldValueKind.String
        };

        FieldEditorSavePreview preview = viewModel.BuildSavePreview(new StaticFieldProvider(), FieldEditorSaveTarget.Global);

        Assert.Equal(FieldEditorSaveOperationKind.Add, preview.OperationKind);
        Assert.Same(preview, viewModel.SavePreview);
        Assert.Empty(viewModel.PreviewIssues);
        Assert.Equal("没有预览问题。", viewModel.PreviewIssueCountText);
        Assert.True(viewModel.HasPersistedJsonPreview);
        Assert.True(viewModel.CanPreviewSave);
        Assert.True(viewModel.CanSave);
        Assert.Contains("全局字段库", viewModel.StatusText);
    }

    [Fact]
    public void PreviewBuilder_IncludesPersistedJsonPreviewForSavedField()
    {
        FieldEditorDraft draft = new(
            "Armor",
            Ra2SectionKind.Vehicle,
            FieldEditorKind.Enum,
            Ra2FieldValueKind.Enum,
            Ra2FieldBooleanValueStyle.Unknown,
            enumName: "ArmorType",
            allowedValues:
            [
                new FieldEditorAllowedValueDraft("light", "Light Armor", "Low protection."),
                new FieldEditorAllowedValueDraft("heavy")
            ],
            displayName: "Armor Type",
            aliases: ["ArmorClass", "Protection"],
            description: "Armor description.",
            FieldEditorSaveTarget.Project);

        FieldEditorSavePreview preview = new FieldEditorSavePreviewBuilder().BuildPreview(draft, new StaticFieldProvider());

        Assert.Contains("\"key\": \"Armor\"", preview.PersistedJsonPreview);
        Assert.Contains("\"appliesTo\":", preview.PersistedJsonPreview);
        Assert.Contains("\"Vehicle\"", preview.PersistedJsonPreview);
        Assert.Contains("\"editorKind\": \"Enum\"", preview.PersistedJsonPreview);
        Assert.Contains("\"sourceKind\": \"User\"", preview.PersistedJsonPreview);
        Assert.Contains("\"displayName\": \"Armor Type\"", preview.PersistedJsonPreview);
        Assert.Contains("\"aliases\":", preview.PersistedJsonPreview);
        Assert.Contains("\"ArmorClass\"", preview.PersistedJsonPreview);
        Assert.Contains("\"description\": \"Armor description.\"", preview.PersistedJsonPreview);
        Assert.Contains("\"schema\":", preview.PersistedJsonPreview);
        Assert.Contains("\"type\": \"Enum\"", preview.PersistedJsonPreview);
        Assert.Contains("\"enumName\": \"ArmorType\"", preview.PersistedJsonPreview);
        Assert.Contains("\"allowedValues\":", preview.PersistedJsonPreview);
        Assert.Contains("\"value\": \"light\"", preview.PersistedJsonPreview);
        Assert.Contains("\"displayName\": \"Light Armor\"", preview.PersistedJsonPreview);
        Assert.Contains("\"description\": \"Low protection.\"", preview.PersistedJsonPreview);
    }

    [Fact]
    public void FieldEditorViewModel_BuildSavePreviewPublishesPersistedJsonPreview()
    {
        FieldEditorViewModel viewModel = new()
        {
            Key = "Armor",
            SectionKind = Ra2SectionKind.Vehicle,
            EditorKind = FieldEditorKind.Enum,
            ValueKind = Ra2FieldValueKind.Enum,
            EnumName = "ArmorType",
            DisplayName = "Armor Type",
            AliasesText = "ArmorClass, Protection",
            Description = "Armor description.",
            AllowedValuesText = "light | Light Armor | Low protection."
        };
        List<string?> changedProperties = [];
        viewModel.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        FieldEditorSavePreview preview = viewModel.BuildSavePreview(new StaticFieldProvider(), FieldEditorSaveTarget.Global);

        Assert.Same(preview.PersistedJsonPreview, viewModel.PersistedJsonPreview);
        Assert.Contains("\"key\": \"Armor\"", viewModel.PersistedJsonPreview);
        Assert.Contains("\"displayName\": \"Armor Type\"", viewModel.PersistedJsonPreview);
        Assert.Contains("\"ArmorClass\"", viewModel.PersistedJsonPreview);
        Assert.Contains(nameof(FieldEditorViewModel.PersistedJsonPreview), changedProperties);
        Assert.Contains(nameof(FieldEditorViewModel.HasPersistedJsonPreview), changedProperties);
    }

    [Fact]
    public void FieldEditorViewModel_ClearsStalePreviewAndApplyPathsWhenEditableFieldChanges()
    {
        using TemporaryDirectory temporaryDirectory = new();
        FieldEditorViewModel viewModel = new()
        {
            Key = "NewField",
            SectionKind = Ra2SectionKind.Vehicle,
            EditorKind = FieldEditorKind.Text,
            ValueKind = Ra2FieldValueKind.String
        };
        FieldEditorSaveContext context = new(
            new StaticFieldProvider(),
            new StaticProvenanceProvider(),
            projectRootPath: null,
            globalFieldRegistryRootPath: temporaryDirectory.Path);

        viewModel.ApplySave(context, FieldEditorSaveTarget.Global);

        Assert.True(viewModel.HasLastApplyPaths);

        viewModel.Description = "Changed description.";

        Assert.Null(viewModel.SavePreview);
        Assert.False(viewModel.CanSave);
        Assert.False(viewModel.HasPersistedJsonPreview);
        Assert.False(viewModel.HasLastApplyPaths);
        Assert.Equal("尚未生成保存预览。", viewModel.PreviewSummary);
        Assert.Equal("没有预览问题。", viewModel.PreviewIssueCountText);
        Assert.Contains("重新生成保存预览", viewModel.StatusText);
    }

    [Fact]
    public void SaveApply_ProjectWithoutProjectRootIsBlockedAndDoesNotWrite()
    {
        using TemporaryDirectory temporaryDirectory = new();
        FieldEditorDraft draft = Draft("ProjectOnlyField", Ra2SectionKind.Vehicle);
        FieldEditorSaveContext context = new(
            new StaticFieldProvider(),
            new StaticProvenanceProvider(),
            projectRootPath: null,
            globalFieldRegistryRootPath: temporaryDirectory.Path);

        FieldEditorSaveApplyResult result = new FieldEditorSaveApplyService().Apply(draft, context);

        Assert.False(result.Success);
        Assert.Null(result.WriteResult);
        Assert.Contains("项目目录", result.Message);
        Assert.False(File.Exists(Path.Combine(temporaryDirectory.Path, "active", "user-import.fields.json")));
    }

    [Fact]
    public void SaveApply_GlobalAddWritesUserImportPack()
    {
        using TemporaryDirectory temporaryDirectory = new();
        FieldEditorDraft draft = Draft(
            "GlobalNewField",
            Ra2SectionKind.Infantry,
            editorKind: FieldEditorKind.Boolean,
            valueKind: Ra2FieldValueKind.Boolean,
            description: "A custom global field.",
            displayName: "Global Field",
            aliases: ["GF", "Global Alias"],
            target: FieldEditorSaveTarget.Global);
        FieldEditorSaveContext context = new(
            new StaticFieldProvider(),
            new StaticProvenanceProvider(),
            projectRootPath: null,
            globalFieldRegistryRootPath: temporaryDirectory.Path);

        FieldEditorSaveApplyResult result = new FieldEditorSaveApplyService().Apply(draft, context);

        string targetFilePath = Path.Combine(temporaryDirectory.Path, "active", "user-import.fields.json");
        Assert.True(result.Success);
        Assert.NotNull(result.WriteResult);
        Assert.Equal(targetFilePath, result.WriteResult.TargetFilePath);
        Assert.Equal(1, result.WriteResult.AddedCount);
        Assert.Contains("保存", result.Message);
        Assert.Contains(targetFilePath, result.Message);
        Assert.Contains("备份", result.Message);
        Assert.Contains(result.WriteResult.ManifestFilePath!, result.Message);
        Assert.True(File.Exists(targetFilePath));
        string json = File.ReadAllText(targetFilePath);
        Assert.Contains("GlobalNewField", json);
        Assert.Contains("A custom global field.", json);
        Assert.Contains("Global Field", json);
        Assert.Contains("Global Alias", json);
        Ra2FieldDefinition loaded = Assert.Single(new RA2IniEditor.Infrastructure.FieldRegistry.LocalFieldRegistryLoader()
            .LoadDirectory(Path.GetDirectoryName(targetFilePath)!)
            .Definitions);
        Assert.Equal("Global Field", loaded.DisplayName);
        Assert.Equal(["GF", "Global Alias"], loaded.Aliases);
    }

    [Fact]
    public void SaveApply_PreservesBooleanStyleAndEnumListSeparator()
    {
        using TemporaryDirectory temporaryDirectory = new();
        FieldEditorSaveContext context = new(
            new StaticFieldProvider(),
            new StaticProvenanceProvider(),
            projectRootPath: null,
            globalFieldRegistryRootPath: temporaryDirectory.Path);

        FieldEditorSaveApplyResult booleanResult = new FieldEditorSaveApplyService().Apply(
            Draft(
                "IsSelectable",
                Ra2SectionKind.Unit,
                editorKind: FieldEditorKind.Boolean,
                valueKind: Ra2FieldValueKind.Boolean,
                target: FieldEditorSaveTarget.Global,
                booleanStyle: Ra2FieldBooleanValueStyle.YesNo),
            context);
        FieldEditorSaveApplyResult listResult = new FieldEditorSaveApplyService().Apply(
            Draft(
                "Owner",
                Ra2SectionKind.Unit,
                editorKind: FieldEditorKind.MultiSelect,
                valueKind: Ra2FieldValueKind.EnumList,
                allowedValues: [new FieldEditorAllowedValueDraft("GDI")],
                target: FieldEditorSaveTarget.Global,
                separator: ";"),
            context);

        Assert.True(booleanResult.Success);
        Assert.True(listResult.Success);
        string targetFilePath = Path.Combine(temporaryDirectory.Path, "active", "user-import.fields.json");
        IReadOnlyList<Ra2FieldDefinition> loadedDefinitions = new RA2IniEditor.Infrastructure.FieldRegistry.LocalFieldRegistryLoader()
            .LoadDirectory(Path.GetDirectoryName(targetFilePath)!)
            .Definitions;
        Ra2FieldDefinition booleanDefinition = Assert.Single(loadedDefinitions, definition => definition.Key == "IsSelectable");
        Ra2FieldDefinition listDefinition = Assert.Single(loadedDefinitions, definition => definition.Key == "Owner");
        Assert.Equal(Ra2FieldBooleanValueStyle.YesNo, booleanDefinition.ValueMetadata.BooleanStyle);
        Assert.Equal(";", listDefinition.ValueMetadata.Separator);
    }

    [Fact]
    public void FieldEditorViewModel_ApplySuccessPublishesLastApplyPaths()
    {
        using TemporaryDirectory temporaryDirectory = new();
        FieldEditorViewModel viewModel = new()
        {
            Key = "GlobalNewField",
            SectionKind = Ra2SectionKind.Infantry,
            EditorKind = FieldEditorKind.Boolean,
            ValueKind = Ra2FieldValueKind.Boolean,
            Description = "A custom global field."
        };
        FieldEditorSaveContext context = new(
            new StaticFieldProvider(),
            new StaticProvenanceProvider(),
            projectRootPath: null,
            globalFieldRegistryRootPath: temporaryDirectory.Path);

        FieldEditorSaveApplyResult result = viewModel.ApplySave(context, FieldEditorSaveTarget.Global);

        Assert.True(result.Success);
        Assert.True(viewModel.HasLastApplyPaths);
        Assert.True(viewModel.HasLastApplyTargetFilePath);
        Assert.True(viewModel.HasLastApplyManifestFilePath);
        Assert.Equal(result.WriteResult!.TargetFilePath, viewModel.LastApplyTargetFilePath);
        Assert.Equal(result.WriteResult.ManifestFilePath, viewModel.LastApplyManifestFilePath);
    }

    [Fact]
    public void SaveApply_BuiltInChangeWritesOverrideWithoutMutatingEffectiveProvider()
    {
        using TemporaryDirectory temporaryDirectory = new();
        Ra2FieldDefinition builtIn = new(
            "Armor",
            [Ra2SectionKind.Vehicle],
            FieldEditorKind.Enum,
            Ra2FieldSourceKind.BuiltIn,
            "BuiltIn armor.");
        StaticFieldProvider provider = new(builtIn);
        FieldEditorDraft draft = Draft(
            "Armor",
            Ra2SectionKind.Vehicle,
            editorKind: FieldEditorKind.Enum,
            valueKind: Ra2FieldValueKind.Enum,
            description: "Custom armor description.",
            target: FieldEditorSaveTarget.Global);
        FieldEditorSaveContext context = new(
            provider,
            new StaticProvenanceProvider(builtIn, FieldRegistryProvenanceScope.BuiltIn),
            projectRootPath: null,
            globalFieldRegistryRootPath: temporaryDirectory.Path);

        FieldEditorSaveApplyResult result = new FieldEditorSaveApplyService().Apply(draft, context);

        Assert.True(result.Success);
        Assert.NotNull(result.WriteResult);
        Assert.Equal(1, result.WriteResult.AddedCount);
        Assert.Equal("BuiltIn armor.", builtIn.Description);
        string json = File.ReadAllText(result.WriteResult.TargetFilePath);
        Assert.Contains("Custom armor description.", json);
    }

    [Fact]
    public void FieldEditorViewModel_BuildSavePreviewCopiesIssuesForUiBinding()
    {
        FieldEditorViewModel viewModel = new()
        {
            Key = "Armor",
            SectionKind = Ra2SectionKind.Unknown,
            AllowedValuesText = "light\r\nLIGHT"
        };

        FieldEditorSavePreview preview = viewModel.BuildSavePreview(new StaticFieldProvider(), FieldEditorSaveTarget.Project);

        Assert.Equal(2, preview.Issues.Count);
        Assert.Equal(2, viewModel.PreviewIssues.Count);
        Assert.Equal("2 个预览问题。", viewModel.PreviewIssueCountText);
        Assert.Contains(viewModel.PreviewIssues, issue => issue.Code == "FE0003");
        Assert.Contains(viewModel.PreviewIssues, issue => issue.Code == "FE0005");
    }

    private static FieldEditorDraft Draft(
        string key,
        Ra2SectionKind sectionKind,
        FieldEditorKind editorKind = FieldEditorKind.Text,
        Ra2FieldValueKind valueKind = Ra2FieldValueKind.String,
        string? description = null,
        IReadOnlyList<FieldEditorAllowedValueDraft>? allowedValues = null,
        string? displayName = null,
        IReadOnlyList<string>? aliases = null,
        FieldEditorSaveTarget target = FieldEditorSaveTarget.Project,
        Ra2FieldBooleanValueStyle booleanStyle = Ra2FieldBooleanValueStyle.Unknown,
        string separator = ",")
        => new(
            key,
            sectionKind,
            editorKind,
            valueKind,
            booleanStyle,
            enumName: null,
            allowedValues ?? [],
            displayName,
            aliases ?? [],
            description,
            target,
            separator);

    private sealed class StaticFieldProvider : IRa2FieldDefinitionProvider
    {
        private readonly List<Ra2FieldDefinition> _definitions;

        public StaticFieldProvider(params Ra2FieldDefinition[] definitions)
        {
            _definitions = [.. definitions];
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = _definitions.FirstOrDefault(candidate =>
                candidate.AppliesTo.Contains(sectionKind) &&
                string.Equals(candidate.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definitions
                .Where(candidate => candidate.AppliesTo.Contains(sectionKind))
                .ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }

    private sealed class StaticProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        private readonly Ra2FieldDefinition? _definition;
        private readonly FieldRegistryProvenanceScope _scope;

        public StaticProvenanceProvider(
            Ra2FieldDefinition? definition = null,
            FieldRegistryProvenanceScope scope = FieldRegistryProvenanceScope.None)
        {
            _definition = definition;
            _scope = scope;
        }

        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(
            Ra2SectionKind sectionKind,
            string key)
        {
            if (_definition is null ||
                !_definition.AppliesTo.Contains(sectionKind) ||
                !string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return FieldRegistryProvenanceLookupResult.NotFound;
            }

            return _scope == FieldRegistryProvenanceScope.BuiltIn
                ? FieldRegistryProvenanceLookupResult.BuiltIn(_definition)
                : FieldRegistryProvenanceLookupResult.FromEntry(new FieldRegistryProvenanceEntry(
                    _definition.Key,
                    sectionKind,
                    _scope,
                    _scope.ToString(),
                    null,
                    _definition));
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RA2IniEditor.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
