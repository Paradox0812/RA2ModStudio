using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels.FieldBrowser;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.BuiltIn;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AddPropertyViewModelTests
{
    [Fact]
    public void Constructor_DefaultsToCurrentSectionKindFields()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle);

        Assert.Contains(viewModel.FilteredItems, item => item.Key == "Strength");
        Assert.DoesNotContain(viewModel.FilteredItems, item => item.Key == "Projectile");
    }

    [Theory]
    [InlineData("Str", "Strength")]
    [InlineData("Health", "Strength")]
    [InlineData("HP", "Strength")]
    [InlineData("hit points", "Strength")]
    [InlineData("Weapon", "Primary")]
    public void Search_FiltersByKeyDisplayAliasNoteAndDescription(string query, string expectedKey)
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle);

        viewModel.SearchText = query;

        Assert.Contains(viewModel.FilteredItems, item => item.Key == expectedKey);
    }

    [Theory]
    [InlineData("Str", "Strength", "键名")]
    [InlineData("Health", "Strength", "显示名")]
    [InlineData("HP", "Strength", "别名")]
    [InlineData("hit points", "Strength", "备注")]
    [InlineData("weapon reference", "Primary", "字段说明")]
    public void Search_ExposesMatchSourceDisplay(string query, string expectedKey, string expectedMatchSource)
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle);

        viewModel.SearchText = query;

        Ra2AddPropertyItemViewModel item = viewModel.FilteredItems.Single(field => field.Key == expectedKey);
        Assert.Equal(expectedMatchSource, item.MatchSourceDisplay);
    }

    [Fact]
    public void SelectingFieldSetsOptionTextToRawKey()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle);
        Ra2AddPropertyItemViewModel item = viewModel.FilteredItems.Single(field => field.Key == "Strength");

        viewModel.SelectedItem = item;

        Assert.Equal("Strength", viewModel.OptionText);
        Assert.NotEqual(item.DisplayName, viewModel.OptionText);
        Assert.Equal("整数。示例：400", viewModel.ValueHintText);
        Assert.Equal("预览：Strength=", viewModel.InsertPreviewText);
    }

    [Fact]
    public void ReadOnlyPreviewDisablesInsert()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle, Ra2EditorDocumentState.ReadOnlyPreview);

        viewModel.SelectedItem = viewModel.FilteredItems.Single(field => field.Key == "Strength");

        Assert.False(viewModel.CanInsert);
        Assert.Equal("当前没有可编辑文件。", viewModel.ReadOnlyHintText);
    }

    [Fact]
    public void EditModeAllowsInsertWhenOptionIsRawKey()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle, Ra2EditorDocumentState.EditableClean);

        viewModel.SelectedItem = viewModel.FilteredItems.Single(field => field.Key == "Strength");
        viewModel.ValueText = "400";

        Assert.True(viewModel.CanInsert);
        Assert.Equal("400", viewModel.ValueText);
        Assert.Equal("预览：Strength=400", viewModel.InsertPreviewText);
    }

    [Fact]
    public void AllSectionFilterShowsFieldsFromMultipleKinds()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(null);

        Assert.Contains(viewModel.FilteredItems, item => item.Key == "Strength");
        Assert.Contains(viewModel.FilteredItems, item => item.Key == "Projectile");
    }

    [Fact]
    public void Constructor_IncludesSpecificBuiltInFieldWhenProjectFallbackFieldExists()
    {
        Ra2FieldDisplayResolver resolver = new(
            new CompositeRa2FieldDefinitionProvider([
                new StaticFieldProvider([
                    new Ra2FieldDefinition("Armor", [Ra2SectionKind.Unknown], FieldEditorKind.Text, Ra2FieldSourceKind.User)
                ]),
                new BuiltInRa2FieldDefinitionProvider()
            ]),
            new Ra2FieldAnnotationProvider(Ra2FieldAnnotationPack.Empty()));
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle, resolver: resolver);

        Ra2AddPropertyItemViewModel item = viewModel.FilteredItems.Single(field => field.Key == "Armor");

        Assert.Equal("Enum", item.TypeDisplay);
        Assert.Equal("BuiltIn", item.SourceDisplay);
    }

    [Fact]
    public void Constructor_UsesBuiltInDetailsWhenExactUserFieldIsWeakAndBuiltInIsAbstractYuriField()
    {
        Ra2FieldDisplayResolver resolver = new(
            new CompositeRa2FieldDefinitionProvider([
                new StaticFieldProvider([
                    new Ra2FieldDefinition("Primary", [Ra2SectionKind.Vehicle], FieldEditorKind.Text, Ra2FieldSourceKind.User)
                ]),
                new StaticFieldProvider([
                    new Ra2FieldDefinition(
                        "Primary",
                        [Ra2SectionKind.Techno],
                        FieldEditorKind.Reference,
                        Ra2FieldSourceKind.Yuri,
                        "Primary weapon reference.",
                        new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference))
                ])
            ]),
            new Ra2FieldAnnotationProvider(Ra2FieldAnnotationPack.Empty()));
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle, resolver: resolver);

        Ra2AddPropertyItemViewModel item = viewModel.FilteredItems.Single(field => field.Key == "Primary");

        Assert.Equal("Reference", item.TypeDisplay);
        Assert.Contains("Primary weapon reference.", item.Description);
    }

    [Fact]
    public void Constructor_ShowsV3DescriptionAndExamplesForProjectileAA()
    {
        Ra2FieldDisplayResolver resolver = new(
            new LocalRa2FieldDefinitionProvider(new BuiltInFieldRegistryPackLoader().Load().Definitions),
            new Ra2FieldAnnotationProvider(Ra2FieldAnnotationPack.Empty()));
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Projectile, resolver: resolver);

        viewModel.SearchText = "AA";

        Ra2AddPropertyItemViewModel item = viewModel.FilteredItems.Single(field => field.Key == "AA");
        Assert.Equal("Boolean", item.TypeDisplay);
        Assert.Contains("攻击空中目标", item.Description);
        Assert.Contains(item.Details.Examples, example => example.Value == "yes");
        Assert.Contains(item.Details.Examples, example => example.Value == "no");
    }

    [Fact]
    public void SearchMode_CommonShowsOnlyCommonFields()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle);

        viewModel.SelectedSearchMode = Ra2FieldBrowserSearchMode.Common;

        Assert.Contains(viewModel.FilteredItems, item => item.Key == "Strength");
        Assert.DoesNotContain(viewModel.FilteredItems, item => item.Key == "Primary");
    }

    [Fact]
    public void SearchMode_SpecificShowsOnlyCurrentKindSpecificFields()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle);

        viewModel.SelectedSearchMode = Ra2FieldBrowserSearchMode.Specific;

        Assert.Contains(viewModel.FilteredItems, item => item.Key == "Primary");
        Assert.DoesNotContain(viewModel.FilteredItems, item => item.Key == "Strength");
        Assert.DoesNotContain(viewModel.FilteredItems, item => item.Key == "Projectile");
    }

    [Fact]
    public void SearchMode_RecentShowsOnlyRecentFields()
    {
        Ra2RecentFieldUsageTracker tracker = new();
        tracker.Record(Ra2SectionKind.Vehicle, "Primary");

        Ra2AddPropertyViewModel viewModel = CreateViewModel(
            Ra2SectionKind.Vehicle,
            recentFieldUsageTracker: tracker);

        viewModel.SelectedSearchMode = Ra2FieldBrowserSearchMode.Recent;

        Ra2AddPropertyItemViewModel item = Assert.Single(viewModel.FilteredItems);
        Assert.Equal("Primary", item.Key);
        Assert.Equal("最近使用", item.MatchSourceDisplay);
    }

    [Fact]
    public void SearchMode_AllDeduplicatesFieldsAcrossKinds()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(null);

        viewModel.SelectedSearchMode = Ra2FieldBrowserSearchMode.All;

        Assert.Single(viewModel.FilteredItems, item => item.Key == "Strength");
        Assert.Contains(viewModel.FilteredItems, item => item.Key == "Projectile");
    }

    [Fact]
    public void EmptyResultStatusExplainsSearchScope()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle);

        viewModel.SearchText = "DefinitelyMissingField";

        Assert.Empty(viewModel.FilteredItems);
        Assert.Contains("未找到匹配字段", viewModel.StatusText);
        Assert.Contains("当前未加载注释库", viewModel.StatusText);
    }

    [Fact]
    public void SpecificModeEmptyResultStatusSuggestsSwitchingScope()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle);

        viewModel.SelectedSearchMode = Ra2FieldBrowserSearchMode.Specific;
        viewModel.SearchText = "Strength";

        Assert.Empty(viewModel.FilteredItems);
        Assert.Contains("当前类型独有字段中没有匹配项", viewModel.StatusText);
        Assert.Contains("当前可用字段", viewModel.StatusText);
        Assert.Contains("全部字段", viewModel.StatusText);
    }

    [Fact]
    public void ClearSearchForEscape_ClearsNonEmptySearchOnly()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle);
        viewModel.SearchText = "Str";

        Assert.True(viewModel.ClearSearchForEscape());
        Assert.Equal(string.Empty, viewModel.SearchText);
        Assert.False(viewModel.ClearSearchForEscape());
    }

    [Fact]
    public void TryConfirmFromKeyboard_EditModeRequiresSelectedInsertableField()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(Ra2SectionKind.Vehicle);

        Assert.False(viewModel.TryConfirmFromKeyboard());
        Assert.Contains("请先选择", viewModel.StatusText);

        viewModel.SelectedItem = viewModel.FilteredItems.Single(field => field.Key == "Strength");

        Assert.True(viewModel.TryConfirmFromKeyboard());
        Assert.Equal("Strength", viewModel.OptionText);
    }

    [Fact]
    public void TryConfirmFromKeyboard_ReadOnlyDoesNotConfirm()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel(
            Ra2SectionKind.Vehicle,
            Ra2EditorDocumentState.ReadOnlyPreview);
        viewModel.SelectedItem = viewModel.FilteredItems.Single(field => field.Key == "Strength");

        Assert.False(viewModel.TryConfirmFromKeyboard());
        Assert.Equal("当前没有可编辑文件。", viewModel.StatusText);
    }

    [Fact]
    public void Constructor_ExposesAnnotationStatusText()
    {
        Ra2FieldAnnotationStatusViewModel status = new(
            "字段注释：已加载 .ra2ide/field-annotations.zh-CN.json",
            isLoaded: true,
            hasWarnings: false);

        Ra2AddPropertyViewModel viewModel = CreateViewModel(
            Ra2SectionKind.Vehicle,
            annotationStatus: status);

        Assert.Equal("字段注释：已加载 .ra2ide/field-annotations.zh-CN.json", viewModel.AnnotationStatusText);
        Assert.False(viewModel.HasAnnotationWarnings);
    }

    [Fact]
    public void RefreshFilteredItems_PutsRecentFieldsBeforeOtherMatches()
    {
        Ra2RecentFieldUsageTracker tracker = new();
        tracker.Record(Ra2SectionKind.Vehicle, "Primary");

        Ra2AddPropertyViewModel viewModel = CreateViewModel(
            Ra2SectionKind.Vehicle,
            recentFieldUsageTracker: tracker);

        Assert.Equal("Primary", viewModel.FilteredItems.First().Key);
        Assert.True(viewModel.FilteredItems.First().IsRecent);
    }

    [Fact]
    public void DuplicateKeyWarning_IsVisibleButDoesNotBlockInsert()
    {
        const string text = "[HTNK]\nStrength=400\nName=Heavy\n";
        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);

        Ra2AddPropertyViewModel viewModel = CreateViewModel(
            Ra2SectionKind.Vehicle,
            document: document,
            caretOffset: text.IndexOf("Name", StringComparison.Ordinal));
        viewModel.SelectedItem = viewModel.FilteredItems.Single(field => field.Key == "Strength");

        Assert.Equal("当前 Section 可能已包含字段：Strength。", viewModel.DuplicateWarningText);
        Assert.True(viewModel.HasDuplicate);
        Assert.Contains("第 2 行", viewModel.DuplicateActionWarningText);
        Assert.Equal(Ra2DuplicateKeyAction.JumpExisting, viewModel.SelectedDuplicateAction);
        Assert.True(viewModel.CanInsert);
        Assert.True(viewModel.CanConfirm);
        Assert.Equal("执行操作", viewModel.ConfirmButtonText);
    }

    [Fact]
    public void DuplicateAction_ReadOnlyAllowsJumpButNotReplace()
    {
        const string text = "[HTNK]\nStrength=400\nName=Heavy\n";
        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);

        Ra2AddPropertyViewModel viewModel = CreateViewModel(
            Ra2SectionKind.Vehicle,
            state: Ra2EditorDocumentState.ReadOnlyPreview,
            document: document,
            caretOffset: text.IndexOf("Name", StringComparison.Ordinal));
        viewModel.SelectedItem = viewModel.FilteredItems.Single(field => field.Key == "Strength");

        Assert.True(viewModel.CanConfirm);
        viewModel.SelectedDuplicateAction = Ra2DuplicateKeyAction.ReplaceExisting;
        Assert.False(viewModel.CanConfirm);
    }

    private static Ra2AddPropertyViewModel CreateViewModel(
        Ra2SectionKind? initialSectionKind,
        Ra2EditorDocumentState state = Ra2EditorDocumentState.EditableClean,
        Ra2FieldAnnotationStatusViewModel? annotationStatus = null,
        Ra2RecentFieldUsageTracker? recentFieldUsageTracker = null,
        Ra2IniTextDocument? document = null,
        int caretOffset = 0,
        Ra2FieldDisplayResolver? resolver = null)
    {
        return new Ra2AddPropertyViewModel(
            resolver ?? CreateResolver(),
            initialSectionKind,
            state,
            annotationStatus: annotationStatus,
            recentFieldUsageTracker: recentFieldUsageTracker,
            document: document,
            caretOffset: caretOffset);
    }

    private static Ra2FieldDisplayResolver CreateResolver()
    {
        return new Ra2FieldDisplayResolver(
            new StaticFieldProvider([
                new Ra2FieldDefinition("Strength", [Ra2SectionKind.Vehicle], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn, "Object hit points."),
                new Ra2FieldDefinition("Strength", [Ra2SectionKind.Infantry], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn, "Infantry hit points."),
                new Ra2FieldDefinition("Primary", [Ra2SectionKind.Vehicle], FieldEditorKind.Reference, Ra2FieldSourceKind.BuiltIn, "Primary weapon reference."),
                new Ra2FieldDefinition("Projectile", [Ra2SectionKind.Weapon], FieldEditorKind.Reference, Ra2FieldSourceKind.BuiltIn, "Projectile reference.")
            ]),
            new Ra2FieldAnnotationProvider(new Ra2FieldAnnotationPack(1, "zh-CN", [
                new Ra2FieldAnnotationEntry("Vehicle", "Strength", "Health", ["HP"], "Maximum hit points."),
                new Ra2FieldAnnotationEntry("Vehicle", "Primary", "Main Weapon", [], "Weapon field.")
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
                AppliesTo(field, sectionKind) &&
                string.Equals(field.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definitions.Where(field => AppliesTo(field, sectionKind)).ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);

        private static bool AppliesTo(Ra2FieldDefinition field, Ra2SectionKind sectionKind)
        {
            if (field.AppliesTo.Contains(sectionKind) ||
                field.AppliesTo.Contains(Ra2SectionKind.Global) ||
                field.AppliesTo.Contains(Ra2SectionKind.Unknown))
            {
                return true;
            }

            if (field.AppliesTo.Contains(Ra2SectionKind.Unit) &&
                sectionKind is Ra2SectionKind.Infantry or Ra2SectionKind.Vehicle or Ra2SectionKind.Aircraft)
            {
                return true;
            }

            return field.AppliesTo.Contains(Ra2SectionKind.Techno) &&
                   sectionKind is Ra2SectionKind.Infantry or
                       Ra2SectionKind.Vehicle or
                       Ra2SectionKind.Aircraft or
                       Ra2SectionKind.Building or
                       Ra2SectionKind.Unit;
        }
    }
}
