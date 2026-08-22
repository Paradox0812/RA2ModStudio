using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.FieldTrust;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2HoverProvider : IRa2HoverProvider
{
    public Ra2HoverInfo? GetHover(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        IRa2FieldDefinitionProvider fieldProvider,
        IFieldRegistryProvenanceProvider provenanceProvider)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fieldProvider);
        ArgumentNullException.ThrowIfNull(provenanceProvider);

        return GetHover(
            model,
            context,
            new Ra2FieldDisplayResolver(
                fieldProvider,
                new Ra2FieldAnnotationProvider(Ra2FieldAnnotationPack.Empty())),
            provenanceProvider);
    }

    public Ra2HoverInfo? GetHover(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        IRa2FieldDisplayResolver fieldDisplayResolver,
        IFieldRegistryProvenanceProvider provenanceProvider)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fieldDisplayResolver);
        ArgumentNullException.ThrowIfNull(provenanceProvider);

        return context.Region switch
        {
            Ra2CaretRegion.Key => GetKeyHover(context, fieldDisplayResolver, provenanceProvider),
            Ra2CaretRegion.Value => GetValueReferenceHover(model, context),
            Ra2CaretRegion.SectionHeader => GetSectionHover(context),
            _ => null
        };
    }

    private static Ra2HoverInfo? GetKeyHover(
        Ra2CaretContext context,
        IRa2FieldDisplayResolver fieldDisplayResolver,
        IFieldRegistryProvenanceProvider provenanceProvider)
    {
        if (context.KeyValue is null || context.TokenSpan is null)
            return null;

        Ra2FieldDisplayInfo displayInfo = fieldDisplayResolver.Resolve(
            context.KeyValue.SectionKind,
            context.KeyValue.Key);
        if (!displayInfo.HasUserAnnotation &&
            string.Equals(displayInfo.TypeDisplay, "Unknown", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        FieldRegistryProvenanceLookupResult provenance = provenanceProvider.TryGetFieldWithProvenance(
            context.KeyValue.SectionKind,
            context.KeyValue.Key);

        string source = provenance.Found ? provenance.Scope.ToString() : displayInfo.SourceDisplay;
        string aliasDetail = displayInfo.Aliases.Count == 0
            ? string.Empty
            : $"; Aliases: {string.Join(", ", displayInfo.Aliases)}";
        string detail = $"Key: {displayInfo.Key}; Type: {displayInfo.TypeDisplay}; Applies to: {displayInfo.AppliesToDisplay}{aliasDetail}";
        return new Ra2HoverInfo(
            displayInfo.DisplayName,
            "Field",
            detail,
            BuildKeyHoverDescription(displayInfo),
            source,
            context.TokenSpan.Value,
            displayInfo.Key,
            displayInfo.DisplayName,
            displayInfo.TypeDisplay,
            displayInfo.Aliases);
    }

    private static string? BuildKeyHoverDescription(Ra2FieldDisplayInfo displayInfo)
    {
        string? baseText = displayInfo.Note ?? displayInfo.Description;
        string? exampleText = TryFormatFirstExample(displayInfo);
        string? text = string.IsNullOrWhiteSpace(exampleText)
            ? baseText
            : string.IsNullOrWhiteSpace(baseText)
                ? exampleText
                : $"{baseText}；{exampleText}";

        Ra2FieldTrustInfo trustInfo = Ra2FieldTrustClassifier.Classify(displayInfo.Definition);
        if (!trustInfo.ShouldShowInHover || string.IsNullOrWhiteSpace(trustInfo.HoverFootnote))
            return text;

        return string.IsNullOrWhiteSpace(text)
            ? trustInfo.HoverFootnote
            : $"{text}{Environment.NewLine}{trustInfo.HoverFootnote}";
    }

    private static string? TryFormatFirstExample(Ra2FieldDisplayInfo displayInfo)
    {
        Ra2FieldExample? example = displayInfo.Definition?.Examples.FirstOrDefault();
        if (example is null)
            return null;

        string text = $"示例：{example.Value}";
        if (!string.IsNullOrWhiteSpace(example.Description) && example.Description.Length <= 24)
            text += $" - {example.Description}";

        return text;
    }

    private static Ra2HoverInfo? GetValueReferenceHover(Ra2DocumentSemanticModel model, Ra2CaretContext context)
    {
        if (context.KeyValue is null || context.TokenSpan is null)
            return null;

        Ra2ReferenceValueDetailService service = new();
        Ra2ReferenceValueDetailResult result = service.Resolve(
            new Ra2ReferenceValueDetailRequest(model, context.Offset));
        return result.Success
            ? service.CreateHoverInfo(result)
            : null;
    }

    private static Ra2HoverInfo? GetSectionHover(Ra2CaretContext context)
    {
        if (context.Section is null || context.TokenSpan is null)
            return null;

        return new Ra2HoverInfo(
            $"[{context.Section.Name}]",
            $"{context.Section.Kind} section",
            $"Line {context.Section.HeaderLineNumber}",
            string.IsNullOrWhiteSpace(context.Section.DisplayNote)
                ? null
                : $"\u5907\u6ce8: {context.Section.DisplayNote}",
            "Current document",
            context.TokenSpan.Value);
    }

}
