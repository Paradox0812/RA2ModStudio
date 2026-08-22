using System.Text.Json;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels.FieldRegistry;

namespace RA2IniEditor.IDE.Services.FieldRegistry;

internal interface IFieldEditorSavePreviewBuilder
{
    FieldEditorSavePreview BuildPreview(FieldEditorDraft draft, IRa2FieldDefinitionProvider effectiveProvider);
}

internal sealed class FieldEditorSavePreviewBuilder : IFieldEditorSavePreviewBuilder
{
    public FieldEditorSavePreview BuildPreview(FieldEditorDraft draft, IRa2FieldDefinitionProvider effectiveProvider)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(effectiveProvider);

        List<FieldEditorValidationIssue> issues = [];
        ValidateDraft(draft, issues);

        if (HasErrors(issues))
        {
            return CreatePreview(
                FieldEditorSaveOperationKind.Blocked,
                draft,
                "无效：字段不能保存，请先修正预览问题。",
                issues,
                canSave: false);
        }

        bool exists = effectiveProvider.TryGetField(draft.SectionKind, draft.Key, out Ra2FieldDefinition existing);
        Ra2FieldDefinition? existingDefinition = exists ? existing : null;
        FieldEditorSaveOperationKind operationKind = ResolveOperationKind(draft, existingDefinition);
        if (operationKind == FieldEditorSaveOperationKind.OverrideBuiltIn)
        {
            issues.Add(Warning(
                "FE0006",
                "该字段来自内置字段库。保存会生成项目或全局覆盖项，不会修改内置字段库。"));
        }

        string summary = CreateSummary(operationKind, draft, existingDefinition);
        bool canSave = operationKind is not FieldEditorSaveOperationKind.NoChange and not FieldEditorSaveOperationKind.Blocked;

        return CreatePreview(operationKind, draft, summary, issues, canSave);
    }

    private static void ValidateDraft(FieldEditorDraft draft, List<FieldEditorValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(draft.Key))
        {
            issues.Add(Error("FE0001", "字段名不能为空。"));
        }
        else if (draft.Key.Contains('='))
        {
            issues.Add(Error("FE0002", "字段名不能包含等号。"));
        }

        if (draft.SectionKind == Ra2SectionKind.Unknown)
            issues.Add(Warning("FE0003", "适用对象类型为 Unknown；保存后会作为通用或未分类字段处理。"));

        if (draft.ValueKind == Ra2FieldValueKind.EnumList)
        {
            if (string.IsNullOrEmpty(draft.Separator))
                issues.Add(Error("FE0008", "列表分隔符不能为空。"));
            else if (draft.Separator.Length > 3)
                issues.Add(Error("FE0009", "列表分隔符建议限制为 1 到 3 个字符。"));
        }

        if (draft.ValueKind is Ra2FieldValueKind.Enum or Ra2FieldValueKind.EnumList &&
            draft.AllowedValues.Count == 0 &&
            string.IsNullOrWhiteSpace(draft.EnumName))
        {
            issues.Add(Warning("FE0010", "枚举字段建议填写枚举名称或至少一个可选值，方便补全复用。"));
        }

        foreach (string error in draft.AllowedValueInputErrors)
            issues.Add(Error("FE0007", error));

        foreach (string duplicate in FindDuplicateAllowedValues(draft))
            issues.Add(Error("FE0005", $"可选值 \"{duplicate}\" 重复出现，请删除或合并重复项。"));
    }

    private static FieldEditorSaveOperationKind ResolveOperationKind(FieldEditorDraft draft, Ra2FieldDefinition? existing)
    {
        if (existing is null)
            return FieldEditorSaveOperationKind.Add;

        if (!HasMaterialChange(draft, existing))
            return FieldEditorSaveOperationKind.NoChange;

        return existing.SourceKind == Ra2FieldSourceKind.BuiltIn
            ? FieldEditorSaveOperationKind.OverrideBuiltIn
            : FieldEditorSaveOperationKind.Update;
    }

    private static bool HasMaterialChange(FieldEditorDraft draft, Ra2FieldDefinition existing)
    {
        if (draft.EditorKind != existing.EditorKind)
            return true;

        if (draft.ValueKind != existing.ValueMetadata.ValueKind)
            return true;

        if (draft.BooleanStyle != existing.ValueMetadata.BooleanStyle)
            return true;

        if (!EqualsOptional(draft.EnumName, existing.ValueMetadata.EnumName))
            return true;

        if (!string.Equals(NormalizeSeparator(draft.Separator), NormalizeSeparator(existing.ValueMetadata.Separator), StringComparison.Ordinal))
            return true;

        if (!EqualsOptional(draft.Description, existing.Description))
            return true;

        if (!AllowedValuesEqual(draft.AllowedValues, existing.ValueMetadata.AllowedValues))
            return true;

        if (!EqualsOptional(draft.DisplayName, existing.DisplayName))
            return true;

        return !AliasesEqual(draft.Aliases, existing.Aliases);
    }

    private static bool AllowedValuesEqual(
        IReadOnlyList<FieldEditorAllowedValueDraft> draftValues,
        IReadOnlyCollection<Ra2FieldAllowedValue> existingValues)
    {
        if (draftValues.Count != existingValues.Count)
            return false;

        int index = 0;
        foreach (Ra2FieldAllowedValue existing in existingValues)
        {
            FieldEditorAllowedValueDraft draft = draftValues[index++];
            if (!string.Equals(draft.Value, existing.Value, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!EqualsOptional(draft.DisplayName, existing.DisplayName))
                return false;

            if (!EqualsOptional(draft.Description, existing.Description))
                return false;
        }

        return true;
    }

    private static IEnumerable<string> FindDuplicateAllowedValues(FieldEditorDraft draft)
        => draft.AllowedValues
            .GroupBy(value => value.Value, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.First().Value);

    private static bool AliasesEqual(IReadOnlyList<string> draftAliases, IReadOnlyList<string> existingAliases)
    {
        if (draftAliases.Count != existingAliases.Count)
            return false;

        for (int index = 0; index < draftAliases.Count; index++)
        {
            if (!string.Equals(draftAliases[index], existingAliases[index], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static string CreateSummary(
        FieldEditorSaveOperationKind operationKind,
        FieldEditorDraft draft,
        Ra2FieldDefinition? existing)
    {
        string target = draft.SaveTarget == FieldEditorSaveTarget.Project ? "项目字段库" : "全局字段库";
        string changeSummary = existing is null ? string.Empty : CreateChangeSummary(draft, existing);
        return operationKind switch
        {
            FieldEditorSaveOperationKind.Add => $"新增字段：将向{target}写入 {draft.Key}。",
            FieldEditorSaveOperationKind.Update => $"更新字段：将修改{target}中的 {draft.Key}。{changeSummary}",
            FieldEditorSaveOperationKind.OverrideBuiltIn => $"覆盖内置字段：将为 {draft.Key} 生成{target}覆盖项，不会修改内置字段库。{changeSummary}",
            FieldEditorSaveOperationKind.NoChange => $"无变化：字段 {draft.Key} 没有检测到可保存的变更。",
            _ => "无效：字段不能保存，请先修正预览问题。"
        };
    }

    private static string CreateChangeSummary(FieldEditorDraft draft, Ra2FieldDefinition existing)
    {
        List<string> changes = [];
        if (draft.EditorKind != existing.EditorKind)
            changes.Add("字段类型");

        if (draft.ValueKind != existing.ValueMetadata.ValueKind)
            changes.Add("值类型");

        if (draft.BooleanStyle != existing.ValueMetadata.BooleanStyle)
            changes.Add("布尔值风格");

        if (!EqualsOptional(draft.EnumName, existing.ValueMetadata.EnumName))
            changes.Add("枚举名称");

        if (!string.Equals(NormalizeSeparator(draft.Separator), NormalizeSeparator(existing.ValueMetadata.Separator), StringComparison.Ordinal))
            changes.Add("列表分隔符");

        if (!AllowedValuesEqual(draft.AllowedValues, existing.ValueMetadata.AllowedValues))
            changes.Add("可选值");

        if (!EqualsOptional(draft.DisplayName, existing.DisplayName))
            changes.Add("显示名称");

        if (!EqualsOptional(draft.Description, existing.Description))
            changes.Add("说明");

        if (!AliasesEqual(draft.Aliases, existing.Aliases))
            changes.Add("别名");

        return changes.Count == 0
            ? string.Empty
            : $"主要变化：{string.Join("、", changes)}。";
    }

    private static FieldEditorSavePreview CreatePreview(
        FieldEditorSaveOperationKind operationKind,
        FieldEditorDraft draft,
        string summary,
        IReadOnlyList<FieldEditorValidationIssue> issues,
        bool canSave)
        => new(
            operationKind,
            draft.SaveTarget,
            draft.Key,
            draft.SectionKind,
            summary,
            CreatePersistedJsonPreview(draft),
            issues,
            canSave);

    private static string CreatePersistedJsonPreview(FieldEditorDraft draft)
    {
        Dictionary<string, object?> field = new()
        {
            ["key"] = draft.Key,
            ["appliesTo"] = new[] { draft.SectionKind.ToString() },
            ["editorKind"] = draft.EditorKind.ToString(),
            ["sourceKind"] = Ra2FieldSourceKind.User.ToString()
        };

        if (!string.IsNullOrWhiteSpace(draft.DisplayName))
            field["displayName"] = draft.DisplayName;

        if (draft.Aliases.Count > 0)
            field["aliases"] = draft.Aliases.ToArray();

        if (!string.IsNullOrWhiteSpace(draft.Description))
            field["description"] = draft.Description;

        Dictionary<string, object?>? schema = CreateSchemaPreview(draft);
        if (schema is not null)
            field["schema"] = schema;

        return JsonSerializer.Serialize(field, new JsonSerializerOptions { WriteIndented = true });
    }

    private static Dictionary<string, object?>? CreateSchemaPreview(FieldEditorDraft draft)
    {
        Dictionary<string, object?> schema = [];
        if (draft.ValueKind != Ra2FieldValueKind.Unknown)
            schema["type"] = draft.ValueKind.ToString();

        if (draft.BooleanStyle != Ra2FieldBooleanValueStyle.Unknown)
            schema["booleanStyle"] = draft.BooleanStyle.ToString();

        if (!string.IsNullOrWhiteSpace(draft.EnumName))
            schema["enumName"] = draft.EnumName;

        if (draft.ValueKind == Ra2FieldValueKind.EnumList &&
            !string.Equals(NormalizeSeparator(draft.Separator), ",", StringComparison.Ordinal))
        {
            schema["separator"] = draft.Separator;
        }

        if (draft.AllowedValues.Count > 0)
        {
            schema["allowedValues"] = draft.AllowedValues
                .Select(value =>
                {
                    Dictionary<string, object?> preview = new()
                    {
                        ["value"] = value.Value
                    };

                    if (!string.IsNullOrWhiteSpace(value.DisplayName))
                        preview["displayName"] = value.DisplayName;

                    if (!string.IsNullOrWhiteSpace(value.Description))
                        preview["description"] = value.Description;

                    return preview;
                })
                .ToArray();
        }

        return schema.Count == 0 ? null : schema;
    }

    private static FieldEditorValidationIssue Error(string code, string message)
        => new(FieldEditorValidationSeverity.Error, code, message);

    private static FieldEditorValidationIssue Warning(string code, string message)
        => new(FieldEditorValidationSeverity.Warning, code, message);

    private static bool HasErrors(IEnumerable<FieldEditorValidationIssue> issues)
        => issues.Any(issue => issue.Severity == FieldEditorValidationSeverity.Error);

    private static bool EqualsOptional(string? left, string? right)
        => string.Equals(left ?? string.Empty, right ?? string.Empty, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSeparator(string? separator)
        => string.IsNullOrEmpty(separator) ? "," : separator;
}
