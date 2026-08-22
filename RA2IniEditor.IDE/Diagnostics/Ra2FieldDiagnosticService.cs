using System.Globalization;
using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldTrust;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.Diagnostics;

/// <summary>
/// 基于当前文档语义模型和字段库定义生成字段级诊断。
/// </summary>
internal sealed class Ra2FieldDiagnosticService
{
    public const string UnknownKeyCode = "FIELD_UNKNOWN_KEY";
    public const string WrongContextKeyCode = "FIELD_WRONG_CONTEXT";
    public const string ObsoleteKeyCode = "FIELD_OBSOLETE_KEY";
    public const string NonExistentKeyCode = "FIELD_NON_EXISTENT_KEY";
    public const string PseudoFieldKeyCode = "FIELD_PSEUDO_FIELD";
    public const string InferredFallbackKeyCode = "FIELD_INFERRED_FALLBACK";
    public const string InvalidBooleanValueCode = "FIELD_BOOLEAN_INVALID";
    public const string InvalidEnumValueCode = "FIELD_ENUM_INVALID";
    public const string InvalidEnumListValueCode = "FIELD_ENUMLIST_INVALID";
    public const string InvalidNumberValueCode = "FIELD_NUMBER_INVALID";

    private const int MaxInvalidListItemsToDisplay = 5;
    private const string SourceKind = "Field";

    public IReadOnlyList<IdeDiagnosticIssueViewModel> AnalyzeCurrentDocument(
        CurrentSourceSnapshot snapshot,
        Ra2DocumentSemanticModel semanticModel,
        IRa2FieldDefinitionProvider fieldProvider)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(semanticModel);
        ArgumentNullException.ThrowIfNull(fieldProvider);

        HashSet<string> sectionsWithKnownFields = BuildKnownFieldSectionSet(semanticModel, fieldProvider);
        List<IdeDiagnosticIssueViewModel> issues = [];
        foreach (Ra2KeyValueSymbol keyValue in semanticModel.KeyValues)
        {
            if (ShouldSkipKeyValue(keyValue))
                continue;

            if (!TryResolveField(fieldProvider, keyValue.SectionKind, keyValue.Key, out Ra2FieldDefinition? definition))
            {
                if (!sectionsWithKnownFields.Contains(BuildSectionKey(keyValue)))
                    continue;

                issues.Add(CreateIssue(
                    snapshot,
                    keyValue,
                    UnknownKeyCode,
                    IniIssueSeverity.Warning,
                    $"未知字段：{keyValue.Key}。当前对象类型的字段库中没有找到该字段；如果这是自定义字段，可以把它加入字段库。",
                    useValueColumn: false));
                continue;
            }

            AddTrustDiagnostics(snapshot, keyValue, definition!, issues);
            AddValueDiagnostics(snapshot, keyValue, definition!, issues);
        }

        return issues;
    }

    private static HashSet<string> BuildKnownFieldSectionSet(
        Ra2DocumentSemanticModel semanticModel,
        IRa2FieldDefinitionProvider fieldProvider)
    {
        HashSet<string> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (Ra2KeyValueSymbol keyValue in semanticModel.KeyValues)
        {
            if (ShouldSkipKeyValue(keyValue))
                continue;

            if (TryResolveField(fieldProvider, keyValue.SectionKind, keyValue.Key, out _))
                result.Add(BuildSectionKey(keyValue));
        }

        return result;
    }

    private static bool ShouldSkipKeyValue(Ra2KeyValueSymbol keyValue)
        => keyValue.SectionKind is Ra2SectionKind.Unknown or Ra2SectionKind.Global ||
           string.IsNullOrWhiteSpace(keyValue.Key) ||
           IsNumericKey(keyValue.Key);

    private static bool TryResolveField(
        IRa2FieldDefinitionProvider fieldProvider,
        Ra2SectionKind sectionKind,
        string key,
        out Ra2FieldDefinition? definition)
    {
        if (fieldProvider.TryGetField(sectionKind, key, out Ra2FieldDefinition directDefinition))
        {
            definition = directDefinition;
            return true;
        }

        definition = fieldProvider.GetFields(sectionKind)
            .FirstOrDefault(candidate => candidate.Aliases.Any(alias => string.Equals(alias, key, StringComparison.OrdinalIgnoreCase)));
        return definition is not null;
    }

    private static void AddTrustDiagnostics(
        CurrentSourceSnapshot snapshot,
        Ra2KeyValueSymbol keyValue,
        Ra2FieldDefinition definition,
        List<IdeDiagnosticIssueViewModel> issues)
    {
        Ra2FieldTrustInfo trustInfo = Ra2FieldTrustClassifier.Classify(definition);
        if (IsGlobalOnlyDefinitionUsedOutsideGlobal(keyValue.SectionKind, definition))
        {
            issues.Add(CreateIssue(
                snapshot,
                keyValue,
                WrongContextKeyCode,
                IniIssueSeverity.Warning,
                $"疑似上下文错误：{keyValue.Key} 通常属于 [General] / Global 全局字段，不建议写在当前 {keyValue.SectionKind} 对象段中。",
                useValueColumn: false));
            return;
        }

        switch (trustInfo.Level)
        {
            case Ra2FieldTrustLevel.VerifiedGuardrail:
                issues.Add(CreateIssue(
                    snapshot,
                    keyValue,
                    WrongContextKeyCode,
                    IniIssueSeverity.Warning,
                    $"疑似上下文错误：{keyValue.Key} 是保护性字段定义，当前段中的用法需要复核。",
                    useValueColumn: false));
                break;
            case Ra2FieldTrustLevel.Obsolete:
                issues.Add(CreateIssue(
                    snapshot,
                    keyValue,
                    ObsoleteKeyCode,
                    IniIssueSeverity.Warning,
                    $"废弃字段：{keyValue.Key} 可能是旧版本或旧引擎残留字段，不建议继续作为正常字段使用。",
                    useValueColumn: false));
                break;
            case Ra2FieldTrustLevel.NonExistent:
                issues.Add(CreateIssue(
                    snapshot,
                    keyValue,
                    NonExistentKeyCode,
                    IniIssueSeverity.Warning,
                    $"未实现字段：{keyValue.Key} 可能是原始注释残留或引擎未读取字段，不建议作为正常字段使用。",
                    useValueColumn: false));
                break;
            case Ra2FieldTrustLevel.PseudoField:
                issues.Add(CreateIssue(
                    snapshot,
                    keyValue,
                    PseudoFieldKeyCode,
                    IniIssueSeverity.Info,
                    $"伪字段提示：{keyValue.Key} 更像注册项、列表项或旧导入残片，请确认是否应作为普通 key 写入。",
                    useValueColumn: false));
                break;
            case Ra2FieldTrustLevel.Inferred:
            case Ra2FieldTrustLevel.AutoExtracted:
                // 推断型字段只在 Hover / Quick Peek 中轻提示，不默认污染 Issues 面板。
                break;
        }
    }

    private static bool IsGlobalOnlyDefinitionUsedOutsideGlobal(Ra2SectionKind sectionKind, Ra2FieldDefinition definition)
        => sectionKind != Ra2SectionKind.Global &&
           definition.AppliesTo.Count > 0 &&
           definition.AppliesTo.All(kind => kind == Ra2SectionKind.Global);

    private static void AddValueDiagnostics(
        CurrentSourceSnapshot snapshot,
        Ra2KeyValueSymbol keyValue,
        Ra2FieldDefinition definition,
        List<IdeDiagnosticIssueViewModel> issues)
    {
        string value = Ra2IniLineParser.GetEffectiveValue(keyValue.Value);
        Ra2FieldValueMetadata metadata = definition.ValueMetadata;
        switch (metadata.ValueKind)
        {
            case Ra2FieldValueKind.Boolean:
                AddBooleanDiagnostic(snapshot, keyValue, metadata, value, issues);
                break;
            case Ra2FieldValueKind.Enum:
                AddEnumDiagnostic(snapshot, keyValue, metadata, value, issues);
                break;
            case Ra2FieldValueKind.EnumList:
                AddEnumListDiagnostic(snapshot, keyValue, metadata, value, issues);
                break;
            case Ra2FieldValueKind.Integer:
                AddIntegerDiagnostic(snapshot, keyValue, value, issues);
                break;
            case Ra2FieldValueKind.Float:
                AddFloatDiagnostic(snapshot, keyValue, value, issues);
                break;
        }
    }

    private static void AddBooleanDiagnostic(
        CurrentSourceSnapshot snapshot,
        Ra2KeyValueSymbol keyValue,
        Ra2FieldValueMetadata metadata,
        string value,
        List<IdeDiagnosticIssueViewModel> issues)
    {
        string[] allowedValues = ResolveBooleanAllowedValues(metadata).ToArray();
        if (IsAllowedValue(value, allowedValues))
            return;

        string suggestion = FormatAllowedValues(allowedValues);
        string message = string.IsNullOrWhiteSpace(value)
            ? $"布尔值可能为空。字段 {keyValue.Key} 建议使用：{suggestion}。"
            : $"布尔值可能无效：{value}。建议使用：{suggestion}。";
        issues.Add(CreateIssue(snapshot, keyValue, InvalidBooleanValueCode, IniIssueSeverity.Warning, message));
    }

    private static void AddEnumDiagnostic(
        CurrentSourceSnapshot snapshot,
        Ra2KeyValueSymbol keyValue,
        Ra2FieldValueMetadata metadata,
        string value,
        List<IdeDiagnosticIssueViewModel> issues)
    {
        if (metadata.AllowedValues.Count == 0 || IsAllowedValue(value, metadata.AllowedValues.Select(allowed => allowed.Value)))
            return;

        issues.Add(CreateIssue(
            snapshot,
            keyValue,
            InvalidEnumValueCode,
            IniIssueSeverity.Warning,
            $"枚举值可能无效：{value}。字段 {keyValue.Key} 的可选值中没有该项。"));
    }

    private static void AddEnumListDiagnostic(
        CurrentSourceSnapshot snapshot,
        Ra2KeyValueSymbol keyValue,
        Ra2FieldValueMetadata metadata,
        string value,
        List<IdeDiagnosticIssueViewModel> issues)
    {
        if (metadata.AllowedValues.Count == 0)
            return;

        string separator = string.IsNullOrEmpty(metadata.Separator) ? "," : metadata.Separator;
        HashSet<string> allowedValues = new(metadata.AllowedValues.Select(allowed => allowed.Value), StringComparer.OrdinalIgnoreCase);
        string[] invalidTokens = value.Split(separator, StringSplitOptions.None)
            .Select(token => token.Trim())
            .Where(token => string.IsNullOrWhiteSpace(token) || !allowedValues.Contains(token))
            .Select(token => string.IsNullOrWhiteSpace(token) ? "<empty>" : token)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (invalidTokens.Length == 0)
            return;

        issues.Add(CreateIssue(
            snapshot,
            keyValue,
            InvalidEnumListValueCode,
            IniIssueSeverity.Warning,
            $"列表中存在无效项：{FormatInvalidItems(invalidTokens)}。字段 {keyValue.Key} 的可选值中没有这些项。"));
    }

    private static void AddIntegerDiagnostic(
        CurrentSourceSnapshot snapshot,
        Ra2KeyValueSymbol keyValue,
        string value,
        List<IdeDiagnosticIssueViewModel> issues)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            return;

        issues.Add(CreateIssue(
            snapshot,
            keyValue,
            InvalidNumberValueCode,
            IniIssueSeverity.Warning,
            $"数字值可能无效：{value}。字段 {keyValue.Key} 需要整数。"));
    }

    private static void AddFloatDiagnostic(
        CurrentSourceSnapshot snapshot,
        Ra2KeyValueSymbol keyValue,
        string value,
        List<IdeDiagnosticIssueViewModel> issues)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            return;

        issues.Add(CreateIssue(
            snapshot,
            keyValue,
            InvalidNumberValueCode,
            IniIssueSeverity.Warning,
            $"数字值可能无效：{value}。字段 {keyValue.Key} 需要数值。"));
    }

    private static IEnumerable<string> ResolveBooleanAllowedValues(Ra2FieldValueMetadata metadata)
        => metadata.AllowedValues.Count > 0
            ? metadata.AllowedValues.Select(allowed => allowed.Value)
            : ["yes", "no", "true", "false"];

    private static string FormatAllowedValues(IReadOnlyList<string> allowedValues)
        => string.Join("、", allowedValues.Take(MaxInvalidListItemsToDisplay));

    private static string FormatInvalidItems(IReadOnlyList<string> invalidTokens)
    {
        string text = string.Join("、", invalidTokens.Take(MaxInvalidListItemsToDisplay));
        return invalidTokens.Count > MaxInvalidListItemsToDisplay
            ? $"{text} 等 {invalidTokens.Count} 项"
            : text;
    }

    private static bool IsAllowedValue(string value, IEnumerable<string> allowedValues)
        => allowedValues.Any(allowed => string.Equals(allowed, value, StringComparison.OrdinalIgnoreCase));

    private static IdeDiagnosticIssueViewModel CreateIssue(
        CurrentSourceSnapshot snapshot,
        Ra2KeyValueSymbol keyValue,
        string code,
        IniIssueSeverity severity,
        string message,
        bool useValueColumn = true)
    {
        Ra2TextSpan columnSpan = useValueColumn && keyValue.ValueSpan is { } valueSpan
            ? valueSpan
            : keyValue.KeySpan;
        return new IdeDiagnosticIssueViewModel(
            code,
            SourceKind,
            severity,
            message,
            snapshot.FilePath,
            keyValue.LineNumber,
            Math.Max(1, columnSpan.Start - keyValue.LineSpan.Start + 1),
            keyValue.SectionName,
            keyValue.Key,
            snapshot.Version);
    }

    private static string BuildSectionKey(Ra2KeyValueSymbol keyValue)
        => $"{keyValue.SectionKind}\u001f{keyValue.SectionName}";

    private static bool IsNumericKey(string key)
        => key.All(char.IsDigit);
}
