using System.IO;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

namespace RA2IniEditor.IDE.Services.FieldRegistry;

internal interface IFieldEditorSaveApplyService
{
    FieldEditorSaveApplyResult Apply(FieldEditorDraft draft, FieldEditorSaveContext context);
}

internal sealed class FieldEditorSaveApplyService : IFieldEditorSaveApplyService
{
    private readonly IFieldRegistryHarvestDiffService _diffService;
    private readonly IFieldRegistryApplyPlanBuilder _planBuilder;
    private readonly IFieldRegistryApplyWriter _writer;

    public FieldEditorSaveApplyService()
        : this(new FieldRegistryHarvestDiffService(), new FieldRegistryApplyPlanBuilder(), new FieldRegistryApplyWriter())
    {
    }

    internal FieldEditorSaveApplyService(
        IFieldRegistryHarvestDiffService diffService,
        IFieldRegistryApplyPlanBuilder planBuilder,
        IFieldRegistryApplyWriter writer)
    {
        _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
        _planBuilder = planBuilder ?? throw new ArgumentNullException(nameof(planBuilder));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public FieldEditorSaveApplyResult Apply(FieldEditorDraft draft, FieldEditorSaveContext context)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(context);

        FieldRegistryApplyTargetScope targetScope = ToTargetScope(draft.SaveTarget);
        if (targetScope == FieldRegistryApplyTargetScope.Project && string.IsNullOrWhiteSpace(context.ProjectRootPath))
        {
            return Blocked(
                "保存到项目字段库需要先打开项目目录。",
                Error("FEA001", "项目字段库保存被阻止：当前没有打开的项目目录。"));
        }

        FieldRegistryHarvestPreviewDraft previewDraft = CreatePreviewDraft(draft);
        FieldRegistryHarvestDiffResult diff = _diffService.Compare(previewDraft, context.ProvenanceProvider);
        FieldRegistryApplyPlan plan = _planBuilder.BuildPlan(new FieldRegistryApplyPlanRequest(
            previewDraft,
            diff,
            targetScope,
            FieldRegistryApplyMode.AppendOrUpdate));

        IReadOnlyList<FieldEditorValidationIssue> planIssues = ConvertPlanIssues(plan.Issues);
        if (!plan.CanApplyInFuture || plan.ErrorCount > 0 || plan.RejectCount > 0)
        {
            return new FieldEditorSaveApplyResult(
                success: false,
                CreateBlockedMessage(plan),
                null,
                planIssues);
        }

        if (plan.AddCount + plan.UpdateCount == 0)
        {
            return new FieldEditorSaveApplyResult(
                success: false,
                "没有可写入的字段变更。",
                null,
                planIssues);
        }

        try
        {
            FieldRegistryApplyWriteResult writeResult = _writer.Write(new FieldRegistryApplyWriteRequest(
                plan,
                context.ProjectRootPath,
                context.GlobalFieldRegistryRootPath));

            List<FieldEditorValidationIssue> issues = [.. planIssues];
            foreach (string warning in writeResult.Warnings)
                issues.Add(Warning("FEA010", warning));

            return new FieldEditorSaveApplyResult(
                success: true,
                CreateSuccessMessage(writeResult),
                writeResult,
                Array.AsReadOnly(issues.ToArray()));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException)
        {
            return Blocked(
                $"字段库保存失败：{ex.Message}",
                Error("FEA099", ex.Message));
        }
    }

    private static FieldRegistryHarvestPreviewDraft CreatePreviewDraft(FieldEditorDraft draft)
    {
        Ra2FieldDefinition definition = new(
            draft.Key,
            [draft.SectionKind],
            draft.EditorKind,
            Ra2FieldSourceKind.User,
            draft.Description,
            CreateValueMetadata(draft),
            draft.DisplayName,
            draft.Aliases);

        return new FieldRegistryHarvestPreviewDraft([definition], []);
    }

    private static Ra2FieldValueMetadata CreateValueMetadata(FieldEditorDraft draft)
        => new(
            draft.ValueKind,
            draft.BooleanStyle,
            draft.AllowedValues
                .Select(value => new Ra2FieldAllowedValue(value.Value, value.DisplayName, value.Description))
                .ToArray(),
            draft.EnumName,
            draft.Separator);

    private static FieldRegistryApplyTargetScope ToTargetScope(FieldEditorSaveTarget target)
        => target == FieldEditorSaveTarget.Project
            ? FieldRegistryApplyTargetScope.Project
            : FieldRegistryApplyTargetScope.Global;

    private static IReadOnlyList<FieldEditorValidationIssue> ConvertPlanIssues(
        IReadOnlyList<FieldRegistryApplyPlanIssue> issues)
        => Array.AsReadOnly(issues
            .Select(issue => new FieldEditorValidationIssue(
                ConvertSeverity(issue.Severity),
                "FEA020",
                issue.Message))
            .ToArray());

    private static FieldEditorValidationSeverity ConvertSeverity(FieldRegistryApplyPlanSeverity severity)
        => severity switch
        {
            FieldRegistryApplyPlanSeverity.Error => FieldEditorValidationSeverity.Error,
            FieldRegistryApplyPlanSeverity.Warning => FieldEditorValidationSeverity.Warning,
            _ => FieldEditorValidationSeverity.Info
        };

    private static string CreateBlockedMessage(FieldRegistryApplyPlan plan)
        => $"字段库保存被阻止：错误 {plan.ErrorCount}，拒绝 {plan.RejectCount}。";

    private static string CreateSuccessMessage(FieldRegistryApplyWriteResult writeResult)
    {
        string message = $"字段库保存完成：新增 {writeResult.AddedCount}，更新 {writeResult.UpdatedCount}，跳过 {writeResult.SkippedCount}。"
            + Environment.NewLine
            + $"目标文件：{writeResult.TargetFilePath}";

        if (!string.IsNullOrWhiteSpace(writeResult.ManifestFilePath))
            message += Environment.NewLine + $"备份清单：{writeResult.ManifestFilePath}";

        return message;
    }

    private static FieldEditorSaveApplyResult Blocked(string message, FieldEditorValidationIssue issue)
        => new(success: false, message, null, [issue]);

    private static FieldEditorValidationIssue Error(string code, string message)
        => new(FieldEditorValidationSeverity.Error, code, message);

    private static FieldEditorValidationIssue Warning(string code, string message)
        => new(FieldEditorValidationSeverity.Warning, code, message);
}
