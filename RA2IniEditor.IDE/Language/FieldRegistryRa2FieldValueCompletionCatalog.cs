using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Language;

internal sealed class FieldRegistryRa2FieldValueCompletionCatalog : IRa2FieldValueCompletionCatalog
{
    private const int MetadataPriority = 200;

    public IReadOnlyList<Ra2FieldValueCompletionCandidate> GetCandidates(
        Ra2FieldValueCompletionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Ra2FieldValueMetadata? metadata = request.FieldDefinition?.ValueMetadata;
        if (metadata is null || !metadata.HasSchema)
            return [];

        return metadata.ValueKind switch
        {
            Ra2FieldValueKind.Boolean => GetBooleanCandidates(request, metadata),
            Ra2FieldValueKind.Enum => CreateAllowedValueCandidates(request, metadata, "Enum"),
            Ra2FieldValueKind.EnumList => CreateAllowedValueCandidates(request, metadata, "EnumList"),
            _ => metadata.AllowedValues.Count > 0
                ? CreateAllowedValueCandidates(request, metadata, metadata.ValueKind.ToString())
                : []
        };
    }

    private static IReadOnlyList<Ra2FieldValueCompletionCandidate> GetBooleanCandidates(
        Ra2FieldValueCompletionRequest request,
        Ra2FieldValueMetadata metadata)
    {
        if (metadata.AllowedValues.Count > 0)
            return CreateAllowedValueCandidates(request, metadata, "Boolean");

        return metadata.BooleanStyle switch
        {
            Ra2FieldBooleanValueStyle.YesNo => CreateRawValueCandidates(
                ["yes", "no"],
                "Boolean",
                "Boolean value from field registry metadata.",
                request.Context.CurrentTokenPrefix,
                request.Context.ExistingTokens,
                skipExistingTokens: false,
                Ra2FieldValueCompletionSourceKind.FieldRegistry),
            Ra2FieldBooleanValueStyle.TrueFalse => CreateRawValueCandidates(
                ["true", "false"],
                "Boolean",
                "Boolean value from field registry metadata.",
                request.Context.CurrentTokenPrefix,
                request.Context.ExistingTokens,
                skipExistingTokens: false,
                Ra2FieldValueCompletionSourceKind.FieldRegistry),
            _ => []
        };
    }

    private static IReadOnlyList<Ra2FieldValueCompletionCandidate> CreateAllowedValueCandidates(
        Ra2FieldValueCompletionRequest request,
        Ra2FieldValueMetadata metadata,
        string displayName)
    {
        bool skipExistingTokens = metadata.ValueKind is Ra2FieldValueKind.EnumList or Ra2FieldValueKind.ReferenceList ||
                                  request.Context.IsListToken;
        HashSet<string> existingTokens = new(request.Context.ExistingTokens, StringComparer.OrdinalIgnoreCase);
        return metadata.AllowedValues
            .Where(value => StartsWithPrefix(value.Value, request.Context.CurrentTokenPrefix))
            .Where(value => !skipExistingTokens || !existingTokens.Contains(value.Value))
            .Select(value => new Ra2FieldValueCompletionCandidate(
                value.Value,
                string.IsNullOrWhiteSpace(value.DisplayName) ? displayName : value.DisplayName,
                value.Description,
                Ra2CompletionItemKind.Value,
                MetadataPriority + value.Priority,
                Ra2FieldValueCompletionSourceKind.FieldRegistry))
            .ToArray();
    }

    private static IReadOnlyList<Ra2FieldValueCompletionCandidate> CreateRawValueCandidates(
        IEnumerable<string> values,
        string displayName,
        string description,
        string prefix,
        IReadOnlyList<string> existingTokens,
        bool skipExistingTokens,
        Ra2FieldValueCompletionSourceKind sourceKind)
    {
        HashSet<string> existing = new(existingTokens, StringComparer.OrdinalIgnoreCase);
        return values
            .Where(value => StartsWithPrefix(value, prefix))
            .Where(value => !skipExistingTokens || !existing.Contains(value))
            .Select(value => new Ra2FieldValueCompletionCandidate(
                value,
                displayName,
                description,
                Ra2CompletionItemKind.Value,
                MetadataPriority,
                sourceKind))
            .ToArray();
    }

    private static bool StartsWithPrefix(string value, string prefix)
        => string.IsNullOrEmpty(prefix) || value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
}
