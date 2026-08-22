using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.FieldTrust;

internal static class Ra2FieldTrustClassifier
{
    public static Ra2FieldTrustInfo Classify(Ra2FieldDefinition? definition)
        => Classify(definition?.RegistryQuality, definition?.SourceKind ?? Ra2FieldSourceKind.Unknown);

    public static Ra2FieldTrustInfo Classify(string? quality, Ra2FieldSourceKind sourceKind = Ra2FieldSourceKind.Unknown)
    {
        string normalized = Normalize(quality);
        if (string.IsNullOrWhiteSpace(normalized))
            return IsBuiltInLikeSource(sourceKind) ? Verified() : Unknown();

        if (ContainsAny(normalized, "non-existent", "nonexistent", "not-implemented", "notimplemented"))
            return new Ra2FieldTrustInfo(
                Ra2FieldTrustLevel.NonExistent,
                "未实现",
                "状态：未实现 / 不建议使用。",
                "该字段被标记为原始注释残留、未实现或不建议作为正常 INI 字段使用。",
                ShouldShowInHover: true,
                ShouldShowWarningStyle: true);

        if (ContainsAny(normalized, "obsolete", "deprecated"))
            return new Ra2FieldTrustInfo(
                Ra2FieldTrustLevel.Obsolete,
                "废弃字段",
                "状态：废弃字段，不建议继续使用。",
                "该字段被标记为废弃或旧版本残留，后续编辑时应优先使用替代字段。",
                ShouldShowInHover: true,
                ShouldShowWarningStyle: true);

        if (ContainsAny(normalized, "pseudo", "list-fragment", "registry-list"))
            return new Ra2FieldTrustInfo(
                Ra2FieldTrustLevel.PseudoField,
                "伪字段",
                "状态：伪字段或注册项残片。",
                "该条目更像注册列表、列表项或导入残片，不应直接当作普通对象字段使用。",
                ShouldShowInHover: true,
                ShouldShowWarningStyle: true);

        if (ContainsAny(normalized, "guardrail", "wrong-context", "wrongcontext"))
            return new Ra2FieldTrustInfo(
                Ra2FieldTrustLevel.VerifiedGuardrail,
                "上下文保护",
                "诊断：疑似上下文错误或保护性字段。",
                "该条目用于提示字段可能被写在错误上下文，或用于阻止宽松字段库把错误位置误认为合法字段。",
                ShouldShowInHover: true,
                ShouldShowWarningStyle: true);

        if (ContainsAny(normalized, "inferred", "source-assisted", "reference-inferred", "name-inferred"))
            return new Ra2FieldTrustInfo(
                Ra2FieldTrustLevel.Inferred,
                "推断说明",
                "可信度：推断说明，仅供参考。",
                "该字段说明来自字段名、所属上下文或社区资料线索推断，尚未完成官方逐条核验。",
                ShouldShowInHover: true,
                ShouldShowWarningStyle: false);

        if (normalized.StartsWith("manual-curated", StringComparison.OrdinalIgnoreCase))
            return new Ra2FieldTrustInfo(
                Ra2FieldTrustLevel.ManualCurated,
                "人工整理",
                null,
                "该字段由人工整理进入字段库，但未声明为官方逐条核验来源。",
                ShouldShowInHover: false,
                ShouldShowWarningStyle: false);

        if (normalized.StartsWith("auto-extracted", StringComparison.OrdinalIgnoreCase))
            return new Ra2FieldTrustInfo(
                Ra2FieldTrustLevel.AutoExtracted,
                "自动抽取",
                "可信度：自动抽取说明，建议复核。",
                "该字段来自自动抽取结果，说明和示例值可能需要人工复核。",
                ShouldShowInHover: true,
                ShouldShowWarningStyle: true);

        if (normalized.StartsWith("source-verified", StringComparison.OrdinalIgnoreCase))
            return Verified();

        return Unknown();
    }

    private static Ra2FieldTrustInfo Verified()
        => new(
            Ra2FieldTrustLevel.Verified,
            "来源核验",
            null,
            "该字段来自内置字段库或已核验来源。",
            ShouldShowInHover: false,
            ShouldShowWarningStyle: false);

    private static Ra2FieldTrustInfo Unknown()
        => new(
            Ra2FieldTrustLevel.Unknown,
            "未分级",
            null,
            "当前字段没有可识别的质量标签。",
            ShouldShowInHover: false,
            ShouldShowWarningStyle: false);

    private static bool IsBuiltInLikeSource(Ra2FieldSourceKind sourceKind)
        => sourceKind is Ra2FieldSourceKind.BuiltIn or
            Ra2FieldSourceKind.Ra2 or
            Ra2FieldSourceKind.Yuri or
            Ra2FieldSourceKind.Ares or
            Ra2FieldSourceKind.Phobos;

    private static string Normalize(string? quality)
        => string.IsNullOrWhiteSpace(quality) ? string.Empty : quality.Trim().ToLowerInvariant();

    private static bool ContainsAny(string value, params string[] tokens)
        => tokens.Any(token => value.Contains(token, StringComparison.OrdinalIgnoreCase));
}
