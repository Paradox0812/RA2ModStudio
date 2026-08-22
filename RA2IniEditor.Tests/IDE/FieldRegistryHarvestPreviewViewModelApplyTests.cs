using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldRegistryHarvestPreviewViewModelApplyTests
{
    [Fact]
    public void TargetScopeOptionsUseChineseDisplayNames()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter());

        Assert.Collection(
            viewModel.TargetScopeOptions,
            option =>
            {
                Assert.Equal(FieldRegistryApplyTargetScope.Project, option.Value);
                Assert.Equal("项目 active 字段库", option.DisplayName);
                Assert.Equal("项目 active 字段库", option.ToString());
            },
            option =>
            {
                Assert.Equal(FieldRegistryApplyTargetScope.Global, option.Value);
                Assert.Equal("全局 active 字段库", option.DisplayName);
                Assert.Equal("全局 active 字段库", option.ToString());
            });
    }

    [Fact]
    public void ApplyModeOptionsUseChineseDisplayNames()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter());

        Assert.Collection(
            viewModel.ApplyModeOptions,
            option =>
            {
                Assert.Equal(FieldRegistryApplyMode.AppendOnly, option.Value);
                Assert.Equal("仅追加", option.DisplayName);
            },
            option =>
            {
                Assert.Equal(FieldRegistryApplyMode.AppendOrUpdate, option.Value);
                Assert.Equal("追加或更新", option.DisplayName);
            },
            option =>
            {
                Assert.Equal(FieldRegistryApplyMode.SkipExisting, option.Value);
                Assert.Equal("跳过已有字段", option.DisplayName);
            });
    }

    [Fact]
    public void SelectedTargetScopeStillMapsToEnumValue()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter());

        viewModel.SelectedTargetScope = FieldRegistryApplyTargetScope.Project;

        Assert.Equal(FieldRegistryApplyTargetScope.Project, viewModel.SelectedTargetScope);
    }

    [Fact]
    public void SelectedApplyModeStillMapsToEnumValue()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter());

        viewModel.SelectedApplyMode = FieldRegistryApplyMode.SkipExisting;

        Assert.Equal(FieldRegistryApplyMode.SkipExisting, viewModel.SelectedApplyMode);
    }

    [Fact]
    public void BuildApplyPlan_AfterPreviewCreatesApplyRows()
    {
        RecordingApplyWriter writer = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(writer);
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MyNewKey | Infantry | Text | New key |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();

        FieldRegistryApplyPlanItemViewModel row = Assert.Single(viewModel.ApplyPlanItems);
        Assert.Equal("Add", row.Operation);
        Assert.Equal("MyNewKey", row.Key);
        Assert.True(viewModel.CanApply);
        Assert.Equal("已准备好应用。", viewModel.ApplyDisabledReason);
        Assert.Equal(1, viewModel.PlanAddCount);
        Assert.Equal(0, writer.WriteCount);
    }

    [Fact]
    public void BuildApplyPlan_GeneralizesConcreteTechnoDraftRowsBeforePlanning()
    {
        RecordingApplyWriter writer = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(writer);
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | Armor | Infantry,Vehicle,Aircraft,Building | Enum | Imported armor |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();

        FieldRegistryApplyPlanItemViewModel item = Assert.Single(viewModel.ApplyPlanItems);
        Assert.Equal("Armor", item.Key);
        Assert.Equal("Techno", item.AppliesTo);
        Assert.Single(viewModel.Definitions);
        Assert.Single(viewModel.DiffRows);
        Assert.Equal(1, viewModel.GeneralizationNoticeCount);
        Assert.Equal(0, viewModel.GeneralizationWarningCount);
        Assert.Contains("已应用 1 项", viewModel.GeneralizationSummaryText, StringComparison.Ordinal);
        Assert.Equal(1, viewModel.GeneralizedFieldCount);
        Assert.Equal(1, viewModel.GeneralizedToTechnoCount);
        Assert.Equal(0, viewModel.GeneralizedToUnitCount);
        Assert.Contains("字段归纳摘要", viewModel.GeneralizationApplySummaryText, StringComparison.Ordinal);
        Assert.Contains("Techno：1 个", viewModel.GeneralizationApplySummaryText, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneralizationApplySummary_NoGeneralizationUsesReadableNoOpText()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter());
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MyNewKey | Infantry | Text | New key |
            """;

        viewModel.ParseAndPreview();

        Assert.Equal(0, viewModel.GeneralizedFieldCount);
        Assert.Contains("本次没有字段需要归纳", viewModel.GeneralizationApplySummaryText, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneralizationApplySummary_CountsUnitSeparately()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter());
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | Speed | Infantry,Vehicle,Aircraft | Integer | Imported speed |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();

        FieldRegistryApplyPlanItemViewModel item = Assert.Single(viewModel.ApplyPlanItems);
        Assert.Equal("Unit", item.AppliesTo);
        Assert.Equal(1, viewModel.GeneralizedFieldCount);
        Assert.Equal(0, viewModel.GeneralizedToTechnoCount);
        Assert.Equal(1, viewModel.GeneralizedToUnitCount);
        Assert.Contains("Unit：1 个", viewModel.GeneralizationApplySummaryText, StringComparison.Ordinal);
    }

    [Fact]
    public void ApplyDisabledReason_NoPreviewExplainsNextStep()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter());

        Assert.False(viewModel.CanApply);
        Assert.Equal("尚未生成预览。请先点击“解析并预览”。", viewModel.ApplyDisabledReason);
    }

    [Fact]
    public void ApplyDisabledReason_NoPlanExplainsNextStep()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter());
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MyNewKey | Infantry | Text | New key |
            """;

        viewModel.ParseAndPreview();

        Assert.False(viewModel.CanApply);
        Assert.Equal("尚未构建应用计划。请检查预览后点击“构建应用计划”。", viewModel.ApplyDisabledReason);
    }

    [Fact]
    public void BuildApplyPlan_ProjectTargetWithoutOpenProjectBlocksApply()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter(), projectRootPath: null);
        viewModel.SelectedTargetScope = FieldRegistryApplyTargetScope.Project;
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MyNewKey | Infantry | Text | New key |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();

        Assert.Null(viewModel.CurrentApplyPlan);
        Assert.Empty(viewModel.ApplyPlanItems);
        Assert.False(viewModel.CanApply);
        Assert.Contains("请先打开项目目录", viewModel.ApplyStatusText, StringComparison.Ordinal);
        Assert.Contains("请先打开项目目录", viewModel.StatusText, StringComparison.Ordinal);
        Assert.Equal("应用到 Project 范围前，请先打开项目目录。", viewModel.ApplyDisabledReason);
    }

    [Fact]
    public void ApplyConfirmed_WritesCurrentPlanAndReloadsRegistry()
    {
        RecordingApplyWriter writer = new();
        bool reloaded = false;
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(writer, reloadAfterApply: () => reloaded = true);
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MyNewKey | Infantry | Text | New key |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();
        FieldRegistryApplyWriteResult? result = viewModel.ApplyConfirmed();

        Assert.NotNull(result);
        Assert.Equal(1, writer.WriteCount);
        Assert.True(reloaded);
        Assert.Equal(FieldRegistryApplyTargetScope.Global, writer.LastRequest?.Plan.TargetScope);
        Assert.Contains("应用已完成", viewModel.ApplyStatusText, StringComparison.Ordinal);
        Assert.Equal(@"C:\ra2-global\active\user-import.fields.json", viewModel.LastApplyTargetFilePath);
        Assert.Equal(@"C:\ra2-global\backups\20260526-000000\manifest.json", viewModel.LastApplyBackupManifestPath);
        Assert.Contains("新增：1", viewModel.LastApplySummaryText, StringComparison.Ordinal);
        Assert.Contains("备份清单", viewModel.LastApplySummaryText, StringComparison.Ordinal);
    }

    [Fact]
    public void ApplyConfirmed_WhenWriterFailsDoesNotReload()
    {
        RecordingApplyWriter writer = new()
        {
            ExceptionToThrow = new IOException("write denied")
        };
        bool reloaded = false;
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(writer, reloadAfterApply: () => reloaded = true);
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MyNewKey | Infantry | Text | New key |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();
        FieldRegistryApplyWriteResult? result = viewModel.ApplyConfirmed();

        Assert.Null(result);
        Assert.Equal(1, writer.WriteCount);
        Assert.False(reloaded);
        Assert.Contains("应用失败", viewModel.ApplyStatusText, StringComparison.Ordinal);
        Assert.DoesNotContain("应用已完成", viewModel.ApplyStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void Clear_RemovesApplyPlanState()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter());
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MyNewKey | Infantry | Text | New key |
            """;
        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();
        Assert.NotNull(viewModel.ApplyConfirmed());
        Assert.NotEqual(string.Empty, viewModel.LastApplyTargetFilePath);

        viewModel.Clear();

        Assert.Null(viewModel.CurrentApplyPlan);
        Assert.Empty(viewModel.ApplyPlanItems);
        Assert.Equal(string.Empty, viewModel.LastApplyTargetFilePath);
        Assert.Equal(string.Empty, viewModel.LastApplyBackupManifestPath);
        Assert.False(viewModel.CanApply);
        Assert.False(viewModel.CanBuildApplyPlan);
    }

    [Fact]
    public void ApplyConfirmed_AllSkipPlanDoesNotCallWriter()
    {
        RecordingApplyWriter writer = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(
            writer,
            provenanceProvider: new SingleFieldProvenanceProvider(
                "MySameKey",
                Ra2SectionKind.Infantry,
                FieldRegistryProvenanceScope.Global,
                "global.fields.json",
                new Ra2FieldDefinition(
                    "MySameKey",
                    [Ra2SectionKind.Infantry],
                    FieldEditorKind.Text,
                    Ra2FieldSourceKind.External,
                    "Same description")));
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MySameKey | Infantry | Text | Same description |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();
        FieldRegistryApplyWriteResult? result = viewModel.ApplyConfirmed();

        Assert.Null(result);
        Assert.False(viewModel.CanApply);
        Assert.Equal(0, writer.WriteCount);
        Assert.Contains("没有可应用的新增或更新操作", viewModel.StatusText, StringComparison.Ordinal);
        Assert.Equal("没有可应用的新增或更新操作。", viewModel.ApplyDisabledReason);
    }

    [Fact]
    public void ApplyConfirmed_RejectPlanDoesNotCallWriter()
    {
        RecordingApplyWriter writer = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(
            writer,
            builder: RejectingApplyPlanBuilder.CreateRejectWithAdd());
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MyNewKey | Infantry | Text | New key |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();
        FieldRegistryApplyWriteResult? result = viewModel.ApplyConfirmed();

        Assert.Null(result);
        Assert.False(viewModel.CanApply);
        Assert.Equal(0, writer.WriteCount);
        Assert.Contains("被拒绝", viewModel.StatusText, StringComparison.Ordinal);
        Assert.Equal("计划包含被拒绝的条目。", viewModel.ApplyDisabledReason);
    }

    [Fact]
    public void ApplyConfirmed_ErrorPlanDoesNotCallWriter()
    {
        RecordingApplyWriter writer = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(
            writer,
            builder: RejectingApplyPlanBuilder.CreateErrorWithAdd());
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | MyNewKey | Infantry | Text | New key |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();
        FieldRegistryApplyWriteResult? result = viewModel.ApplyConfirmed();

        Assert.Null(result);
        Assert.False(viewModel.CanApply);
        Assert.Equal(0, writer.WriteCount);
        Assert.Contains("错误", viewModel.StatusText, StringComparison.Ordinal);
        Assert.Equal("计划包含错误。", viewModel.ApplyDisabledReason);
    }

    [Fact]
    public void Confirmation_IncludesWarningsForBuiltInOverride()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(
            new RecordingApplyWriter(),
            provenanceProvider: new BuiltInOwnerProvenanceProvider());
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | Owner | Infantry | Text | Imported owner description |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();
        FieldRegistryApplyConfirmationViewModel? confirmation = viewModel.CreateApplyConfirmation();

        Assert.NotNull(confirmation);
        Assert.Contains("BuiltIn", confirmation.Message, StringComparison.Ordinal);
        Assert.Contains("备份清单", confirmation.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Confirmation_IncludesGeneralizationSummary()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(new RecordingApplyWriter());
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | Armor | Infantry,Vehicle,Aircraft,Building | Enum | Imported armor |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();
        FieldRegistryApplyConfirmationViewModel? confirmation = viewModel.CreateApplyConfirmation();

        Assert.NotNull(confirmation);
        Assert.Contains("字段归纳摘要", confirmation.Message, StringComparison.Ordinal);
        Assert.Contains("Techno：1 个", confirmation.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ApplyConfirmed_StatusIncludesGeneralizationSummary()
    {
        RecordingApplyWriter writer = new();
        FieldRegistryHarvestPreviewViewModel viewModel = CreateViewModel(writer);
        viewModel.RawText = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | Armor | Infantry,Vehicle,Aircraft,Building | Enum | Imported armor |
            """;

        viewModel.ParseAndPreview();
        viewModel.BuildApplyPlan();
        FieldRegistryApplyWriteResult? result = viewModel.ApplyConfirmed();

        Assert.NotNull(result);
        Assert.Contains("字段归纳摘要", viewModel.ApplyStatusText, StringComparison.Ordinal);
        Assert.Contains("字段归纳摘要", viewModel.LastApplySummaryText, StringComparison.Ordinal);
        Assert.Contains("字段归纳摘要", viewModel.StatusText, StringComparison.Ordinal);
    }

    private static FieldRegistryHarvestPreviewViewModel CreateViewModel(
        IFieldRegistryApplyWriter writer,
        string? projectRootPath = @"C:\ra2-project",
        string globalRootPath = @"C:\ra2-global",
        IFieldRegistryProvenanceProvider? provenanceProvider = null,
        Action? reloadAfterApply = null,
        IFieldRegistryApplyPlanBuilder? builder = null)
    {
        return new FieldRegistryHarvestPreviewViewModel(
            new MarkdownFieldRegistryHarvestParser(),
            new FieldRegistryHarvestNormalizer(),
            new FieldRegistryHarvestPreviewBuilder(),
            new FieldRegistryHarvestDiffService(),
            () => provenanceProvider ?? new EmptyProvenanceProvider(),
            builder ?? new FieldRegistryApplyPlanBuilder(),
            writer,
            () => projectRootPath,
            () => globalRootPath,
            reloadAfterApply);
    }

    private sealed class RecordingApplyWriter : IFieldRegistryApplyWriter
    {
        public int WriteCount { get; private set; }

        public FieldRegistryApplyWriteRequest? LastRequest { get; private set; }

        public Exception? ExceptionToThrow { get; init; }

        public FieldRegistryApplyWriteResult Write(FieldRegistryApplyWriteRequest request)
        {
            WriteCount++;
            LastRequest = request;
            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return new FieldRegistryApplyWriteResult(
                @"C:\ra2-global\active\user-import.fields.json",
                @"C:\ra2-global\backups\20260526-000000",
                @"C:\ra2-global\backups\20260526-000000\manifest.json",
                request.Plan.AddCount,
                request.Plan.UpdateCount,
                request.Plan.SkipCount,
                []);
        }
    }

    private sealed class EmptyProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(Ra2SectionKind sectionKind, string key)
            => FieldRegistryProvenanceLookupResult.NotFound;
    }

    private sealed class BuiltInOwnerProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(Ra2SectionKind sectionKind, string key)
        {
            if (sectionKind != Ra2SectionKind.Infantry ||
                !string.Equals(key, "Owner", StringComparison.OrdinalIgnoreCase))
            {
                return FieldRegistryProvenanceLookupResult.NotFound;
            }

            Ra2FieldDefinition definition = new(
                "Owner",
                [Ra2SectionKind.Infantry],
                FieldEditorKind.MultiSelect,
                Ra2FieldSourceKind.BuiltIn,
                "BuiltIn owner field.");
            return FieldRegistryProvenanceLookupResult.FromEntry(new FieldRegistryProvenanceEntry(
                "Owner",
                Ra2SectionKind.Infantry,
                FieldRegistryProvenanceScope.BuiltIn,
                "BuiltIn",
                null,
                definition));
        }
    }

    private sealed class SingleFieldProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        private readonly string _key;
        private readonly Ra2SectionKind _appliesTo;
        private readonly FieldRegistryProvenanceScope _scope;
        private readonly string _sourceName;
        private readonly Ra2FieldDefinition _definition;

        public SingleFieldProvenanceProvider(
            string key,
            Ra2SectionKind appliesTo,
            FieldRegistryProvenanceScope scope,
            string sourceName,
            Ra2FieldDefinition definition)
        {
            _key = key;
            _appliesTo = appliesTo;
            _scope = scope;
            _sourceName = sourceName;
            _definition = definition;
        }

        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(Ra2SectionKind sectionKind, string key)
        {
            if (sectionKind != _appliesTo || !string.Equals(key, _key, StringComparison.OrdinalIgnoreCase))
                return FieldRegistryProvenanceLookupResult.NotFound;

            return FieldRegistryProvenanceLookupResult.FromEntry(new FieldRegistryProvenanceEntry(
                _key,
                _appliesTo,
                _scope,
                _sourceName,
                null,
                _definition));
        }
    }

    private sealed class RejectingApplyPlanBuilder : IFieldRegistryApplyPlanBuilder
    {
        private readonly bool _includeError;

        private RejectingApplyPlanBuilder(bool includeError)
        {
            _includeError = includeError;
        }

        public static RejectingApplyPlanBuilder CreateRejectWithAdd()
            => new(false);

        public static RejectingApplyPlanBuilder CreateErrorWithAdd()
            => new(true);

        public FieldRegistryApplyPlan BuildPlan(FieldRegistryApplyPlanRequest request)
        {
            Ra2FieldDefinition definition = new(
                "MyNewKey",
                [Ra2SectionKind.Infantry],
                FieldEditorKind.Text,
                Ra2FieldSourceKind.External,
                "New key");
            List<FieldRegistryApplyPlanItem> items =
            [
                new FieldRegistryApplyPlanItem(
                    "MyNewKey",
                    Ra2SectionKind.Infantry,
                    FieldRegistryApplyOperationKind.Add,
                    request.TargetScope,
                    FieldRegistryProvenanceScope.None,
                    "None",
                    definition,
                    "Add row."),
                new FieldRegistryApplyPlanItem(
                    "RejectedKey",
                    Ra2SectionKind.Infantry,
                    FieldRegistryApplyOperationKind.Reject,
                    request.TargetScope,
                    FieldRegistryProvenanceScope.None,
                    "None",
                    definition,
                    "Rejected row.")
            ];
            IReadOnlyList<FieldRegistryApplyPlanIssue> issues = _includeError
                ? [new FieldRegistryApplyPlanIssue(FieldRegistryApplyPlanSeverity.Error, "MyNewKey", Ra2SectionKind.Infantry, "Synthetic error.")]
                : [];
            return new FieldRegistryApplyPlan(
                request.TargetScope,
                request.Mode,
                items,
                issues);
        }
    }
}


