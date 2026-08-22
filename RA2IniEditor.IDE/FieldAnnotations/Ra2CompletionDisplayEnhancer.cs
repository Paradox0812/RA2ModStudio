using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2CompletionDisplayEnhancer
{
    public Ra2CompletionResult Enhance(
        Ra2CompletionResult result,
        Ra2SectionKind sectionKind,
        IRa2FieldDisplayResolver fieldDisplayResolver)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(fieldDisplayResolver);

        if (result.Items.Count == 0 || sectionKind == Ra2SectionKind.Unknown)
            return result;

        IReadOnlyList<Ra2CompletionItem> items = result.Items
            .Select(item => EnhanceItem(item, sectionKind, fieldDisplayResolver))
            .ToArray();
        return new Ra2CompletionResult(items, result.ReplacementSpan);
    }

    private static Ra2CompletionItem EnhanceItem(
        Ra2CompletionItem item,
        Ra2SectionKind sectionKind,
        IRa2FieldDisplayResolver fieldDisplayResolver)
    {
        if (item.Kind != Ra2CompletionItemKind.Key ||
            item.SourceKind != Ra2CompletionItemSourceKind.FieldRegistry)
        {
            return item;
        }

        Ra2FieldDisplayInfo displayInfo = fieldDisplayResolver.Resolve(sectionKind, item.Label);
        string detail = CreateDetail(item, displayInfo);
        string? documentation = displayInfo.Note ?? displayInfo.Description ?? item.Documentation;

        return new Ra2CompletionItem(
            item.Label,
            item.Kind,
            detail,
            documentation,
            item.InsertText,
            item.Priority,
            item.SourceKind);
    }

    private static string CreateDetail(Ra2CompletionItem item, Ra2FieldDisplayInfo displayInfo)
    {
        List<string> parts = new();
        if (!string.Equals(displayInfo.DisplayName, item.Label, StringComparison.Ordinal))
            parts.Add(displayInfo.DisplayName);

        parts.Add($"Type: {displayInfo.TypeDisplay}");
        if (displayInfo.Aliases.Count > 0)
            parts.Add($"Aliases: {string.Join(", ", displayInfo.Aliases)}");

        return string.Join(" | ", parts);
    }
}
