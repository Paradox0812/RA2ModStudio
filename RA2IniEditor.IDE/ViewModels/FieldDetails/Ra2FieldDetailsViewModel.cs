using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldTrust;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.ViewModels.FieldDetails;

internal sealed class Ra2FieldDetailsViewModel
{
    private Ra2FieldDetailsViewModel(
        string title,
        string key,
        string displayName,
        string sectionKindDisplay,
        string sourceDisplay,
        string editorKindDisplay,
        string valueKindDisplay,
        string trustDisplay,
        string trustDetail,
        string description,
        IReadOnlyList<Ra2FieldExampleItemViewModel> examples,
        IReadOnlyList<Ra2AllowedValueItemViewModel> allowedValues,
        bool isNotFound,
        string notFoundMessage)
    {
        Title = title;
        Key = key;
        DisplayName = displayName;
        SectionKindDisplay = sectionKindDisplay;
        SourceDisplay = sourceDisplay;
        EditorKindDisplay = editorKindDisplay;
        ValueKindDisplay = valueKindDisplay;
        TrustDisplay = trustDisplay;
        TrustDetail = trustDetail;
        Description = description;
        Examples = examples;
        AllowedValues = allowedValues;
        IsNotFound = isNotFound;
        NotFoundMessage = notFoundMessage;
    }

    public string Title { get; }

    public string Key { get; }

    public string DisplayName { get; }

    public string SectionKindDisplay { get; }

    public string SourceDisplay { get; }

    public string EditorKindDisplay { get; }

    public string ValueKindDisplay { get; }

    public string TrustDisplay { get; }

    public string TrustDetail { get; }

    public bool HasTrustDetail => !string.IsNullOrWhiteSpace(TrustDetail);

    public string Description { get; }

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public IReadOnlyList<Ra2FieldExampleItemViewModel> Examples { get; }

    public bool HasExamples => Examples.Count > 0;

    public IReadOnlyList<Ra2AllowedValueItemViewModel> AllowedValues { get; }

    public bool HasAllowedValues => AllowedValues.Count > 0;

    public bool IsNotFound { get; }

    public string NotFoundMessage { get; }

    public static Ra2FieldDetailsViewModel FromDefinition(
        Ra2FieldDefinition definition,
        Ra2SectionKind sectionKind,
        string? sourceDisplay = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        Ra2FieldTrustInfo trustInfo = Ra2FieldTrustClassifier.Classify(definition);
        return new Ra2FieldDetailsViewModel(
            definition.Key,
            definition.Key,
            string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.Key : definition.DisplayName,
            FormatSectionKinds(definition.AppliesTo, sectionKind),
            string.IsNullOrWhiteSpace(sourceDisplay) ? FormatSource(definition.SourceKind) : sourceDisplay,
            definition.EditorKind.ToString(),
            FormatValueKind(definition.ValueMetadata),
            trustInfo.ShortLabel,
            trustInfo.DetailText,
            definition.Description ?? string.Empty,
            definition.Examples
                .Select(example => new Ra2FieldExampleItemViewModel(example.Value, example.Description))
                .ToArray(),
            definition.ValueMetadata.AllowedValues
                .Select(value => new Ra2AllowedValueItemViewModel(value.Value, value.DisplayName, value.Description))
                .ToArray(),
            isNotFound: false,
            notFoundMessage: string.Empty);
    }

    public static Ra2FieldDetailsViewModel FromProvenance(
        FieldRegistryProvenanceLookupResult provenance,
        Ra2SectionKind sectionKind)
    {
        if (provenance.Definition is null)
            return NotFound(string.Empty, sectionKind);

        return FromDefinition(provenance.Definition, sectionKind, FormatSource(provenance));
    }

    public static Ra2FieldDetailsViewModel NotFound(string key, Ra2SectionKind sectionKind)
    {
        string normalizedKey = string.IsNullOrWhiteSpace(key) ? "未知字段" : key.Trim();
        return new Ra2FieldDetailsViewModel(
            "未找到字段详情",
            normalizedKey,
            normalizedKey,
            sectionKind.ToString(),
            "未找到",
            "Unknown",
            "Unknown",
            "未分级",
            string.Empty,
            string.Empty,
            [],
            [],
            isNotFound: true,
            "当前字段未在项目 / 全局 / 内置字段库中找到。如这是自定义字段，可以通过字段库工具添加。");
    }

    public static string FormatSource(FieldRegistryProvenanceLookupResult provenance)
    {
        if (!provenance.Found || provenance.Definition is null)
            return "未找到";

        return provenance.Scope switch
        {
            FieldRegistryProvenanceScope.Project => $"项目字段库：{provenance.SourceName}",
            FieldRegistryProvenanceScope.Global => $"全局字段库：{provenance.SourceName}",
            FieldRegistryProvenanceScope.BuiltIn => FormatSource(provenance.Definition.SourceKind),
            _ => provenance.SourceName
        };
    }

    public static string FormatSource(Ra2FieldSourceKind sourceKind)
        => sourceKind switch
        {
            Ra2FieldSourceKind.Yuri or Ra2FieldSourceKind.Ra2 => "YR 内置参考",
            Ra2FieldSourceKind.Ares => "Ares 内置参考",
            Ra2FieldSourceKind.Phobos => "Phobos 内置参考",
            Ra2FieldSourceKind.BuiltIn => "内置参考",
            Ra2FieldSourceKind.User or Ra2FieldSourceKind.UserDictionary => "用户字段库",
            Ra2FieldSourceKind.Custom => "自定义字段库",
            Ra2FieldSourceKind.External or Ra2FieldSourceKind.ExternalDictionary => "外部字段库",
            _ => sourceKind.ToString()
        };

    private static string FormatSectionKinds(IReadOnlyCollection<Ra2SectionKind> appliesTo, Ra2SectionKind fallback)
        => appliesTo.Count == 0 ? fallback.ToString() : string.Join(", ", appliesTo);

    private static string FormatValueKind(Ra2FieldValueMetadata metadata)
        => metadata.HasSchema ? metadata.ValueKind.ToString() : "Unknown";
}

internal sealed class Ra2FieldExampleItemViewModel
{
    public Ra2FieldExampleItemViewModel(string value, string? description)
    {
        Value = value;
        Description = description ?? string.Empty;
    }

    public string Value { get; }

    public string Description { get; }

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
}

internal sealed class Ra2AllowedValueItemViewModel
{
    public Ra2AllowedValueItemViewModel(string value, string? displayName, string? description)
    {
        Value = value;
        DisplayName = displayName ?? string.Empty;
        Description = description ?? string.Empty;
    }

    public string Value { get; }

    public string DisplayName { get; }

    public string Description { get; }
}
