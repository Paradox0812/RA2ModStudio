namespace RA2IniEditor.Infrastructure.FieldRegistry.Cleanup;

public sealed class FieldRegistryGeneralizationRepairPreview
{
    public FieldRegistryGeneralizationRepairPreview(
        string targetPackFileName,
        string targetPackPath,
        IReadOnlyList<FieldRegistryGeneralizationAbstractFieldPreview> abstractFields,
        IReadOnlyList<FieldRegistryGeneralizationRemovedConcreteFieldPreview> removedConcreteFields,
        IReadOnlyList<FieldRegistryGeneralizationSkippedFieldPreview> skippedFields,
        IReadOnlyList<string> warnings)
    {
        TargetPackFileName = targetPackFileName ?? throw new ArgumentNullException(nameof(targetPackFileName));
        TargetPackPath = targetPackPath ?? throw new ArgumentNullException(nameof(targetPackPath));
        AbstractFields = abstractFields ?? throw new ArgumentNullException(nameof(abstractFields));
        RemovedConcreteFields = removedConcreteFields ?? throw new ArgumentNullException(nameof(removedConcreteFields));
        SkippedFields = skippedFields ?? throw new ArgumentNullException(nameof(skippedFields));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    public string TargetPackFileName { get; }

    public string TargetPackPath { get; }

    public IReadOnlyList<FieldRegistryGeneralizationAbstractFieldPreview> AbstractFields { get; }

    public IReadOnlyList<FieldRegistryGeneralizationRemovedConcreteFieldPreview> RemovedConcreteFields { get; }

    public IReadOnlyList<FieldRegistryGeneralizationSkippedFieldPreview> SkippedFields { get; }

    public IReadOnlyList<string> Warnings { get; }

    public int AddedAbstractFieldCount => AbstractFields.Count(row => row.OperationText == "新增");

    public int UpdatedAbstractFieldCount => AbstractFields.Count(row => row.OperationText == "更新");

    public bool HasPlan => AbstractFields.Count > 0 || RemovedConcreteFields.Count > 0 || SkippedFields.Count > 0 || Warnings.Count > 0;

    public string SummaryText =>
        $"目标字段库：{TargetPackFileName}。本轮仅处理默认 active pack，其他 active pack 不会修改。" +
        $"新增抽象字段：{AddedAbstractFieldCount}；更新抽象字段：{UpdatedAbstractFieldCount}；" +
        $"将移除具体重复字段：{RemovedConcreteFields.Count}；跳过：{SkippedFields.Count}；警告：{Warnings.Count}。";

    public static FieldRegistryGeneralizationRepairPreview Empty(string targetPackPath)
    {
        return new FieldRegistryGeneralizationRepairPreview(
            Path.GetFileName(targetPackPath),
            targetPackPath,
            Array.Empty<FieldRegistryGeneralizationAbstractFieldPreview>(),
            Array.Empty<FieldRegistryGeneralizationRemovedConcreteFieldPreview>(),
            Array.Empty<FieldRegistryGeneralizationSkippedFieldPreview>(),
            Array.Empty<string>());
    }
}

public sealed class FieldRegistryGeneralizationAbstractFieldPreview
{
    public FieldRegistryGeneralizationAbstractFieldPreview(
        string operationText,
        string key,
        string targetSectionKind,
        IReadOnlyList<string> sourceSectionKinds,
        string valueKindText,
        IReadOnlyList<string> allowedValues,
        string description)
    {
        OperationText = operationText ?? throw new ArgumentNullException(nameof(operationText));
        Key = key ?? throw new ArgumentNullException(nameof(key));
        TargetSectionKind = targetSectionKind ?? throw new ArgumentNullException(nameof(targetSectionKind));
        SourceSectionKinds = sourceSectionKinds ?? throw new ArgumentNullException(nameof(sourceSectionKinds));
        ValueKindText = valueKindText ?? throw new ArgumentNullException(nameof(valueKindText));
        AllowedValues = allowedValues ?? throw new ArgumentNullException(nameof(allowedValues));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    public string OperationText { get; }

    public string Key { get; }

    public string TargetSectionKind { get; }

    public IReadOnlyList<string> SourceSectionKinds { get; }

    public string SourceSectionKindsText => string.Join(", ", SourceSectionKinds);

    public string ValueKindText { get; }

    public IReadOnlyList<string> AllowedValues { get; }

    public string AllowedValuesPreviewText => AllowedValues.Count == 0
        ? "-"
        : AllowedValues.Count <= 8
            ? string.Join(", ", AllowedValues)
            : $"{string.Join(", ", AllowedValues.Take(8))} ... 共 {AllowedValues.Count} 项";

    public string Description { get; }
}

public sealed class FieldRegistryGeneralizationRemovedConcreteFieldPreview
{
    public FieldRegistryGeneralizationRemovedConcreteFieldPreview(
        string key,
        string concreteSectionKind,
        string replacedBySectionKind,
        string reason)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        ConcreteSectionKind = concreteSectionKind ?? throw new ArgumentNullException(nameof(concreteSectionKind));
        ReplacedBySectionKind = replacedBySectionKind ?? throw new ArgumentNullException(nameof(replacedBySectionKind));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    public string Key { get; }

    public string ConcreteSectionKind { get; }

    public string ReplacedBySectionKind { get; }

    public string Reason { get; }
}

public sealed class FieldRegistryGeneralizationSkippedFieldPreview
{
    public FieldRegistryGeneralizationSkippedFieldPreview(string key, string sectionKindsText, string reason)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        SectionKindsText = sectionKindsText ?? throw new ArgumentNullException(nameof(sectionKindsText));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    public string Key { get; }

    public string SectionKindsText { get; }

    public string Reason { get; }
}
