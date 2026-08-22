using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2DefinitionProvider : IRa2DefinitionProvider
{
    public Ra2DefinitionTarget? GetDefinition(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        IRa2FieldDefinitionProvider fieldProvider,
        IFieldRegistryProvenanceProvider provenanceProvider)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fieldProvider);
        ArgumentNullException.ThrowIfNull(provenanceProvider);

        return context.Region switch
        {
            Ra2CaretRegion.Key => GetFieldDefinition(context, fieldProvider, provenanceProvider),
            Ra2CaretRegion.Value => GetValueReferenceDefinition(model, context),
            Ra2CaretRegion.SectionHeader => GetCurrentSectionDefinition(context),
            _ => null
        };
    }

    private static Ra2DefinitionTarget? GetFieldDefinition(
        Ra2CaretContext context,
        IRa2FieldDefinitionProvider fieldProvider,
        IFieldRegistryProvenanceProvider provenanceProvider)
    {
        if (context.KeyValue is null)
            return null;

        FieldRegistryProvenanceLookupResult provenance = provenanceProvider.TryGetFieldWithProvenance(
            context.KeyValue.SectionKind,
            context.KeyValue.Key);
        Ra2FieldDefinition? definition = provenance.Definition;
        if (definition is null &&
            fieldProvider.TryGetField(context.KeyValue.SectionKind, context.KeyValue.Key, out Ra2FieldDefinition fallback))
        {
            definition = fallback;
        }

        if (definition is null)
            return null;

        string appliesTo = definition.AppliesTo.Count == 0
            ? "Common"
            : string.Join(", ", definition.AppliesTo);
        string detail = $"Type: {definition.EditorKind}; Applies to: {appliesTo}";
        return new Ra2DefinitionTarget(
            Ra2DefinitionTargetKind.FieldDefinition,
            definition.Key,
            detail,
            provenance.Found ? provenance.SourceName : definition.SourceKind.ToString(),
            provenance.SourcePath,
            null,
            null,
            definition.Description);
    }

    private static Ra2DefinitionTarget? GetValueReferenceDefinition(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context)
    {
        Ra2ValueReferenceSymbol? reference = FindReferenceAtContext(model, context);
        if (reference is null)
            return null;

        Ra2SectionSymbol? targetSection = model.FindSectionByName(reference.TargetSectionName);
        if (targetSection is null)
            return null;

        return CreateSectionTarget(targetSection);
    }

    private static Ra2DefinitionTarget? GetCurrentSectionDefinition(Ra2CaretContext context)
    {
        if (context.Section is null)
            return null;

        return CreateSectionTarget(context.Section);
    }

    private static Ra2DefinitionTarget CreateSectionTarget(Ra2SectionSymbol section)
    {
        return new Ra2DefinitionTarget(
            Ra2DefinitionTargetKind.SectionDefinition,
            $"[{section.Name}]",
            $"{section.Kind} section",
            "Current document",
            null,
            section.HeaderSpan,
            section.HeaderLineNumber,
            string.IsNullOrWhiteSpace(section.DisplayNote)
                ? null
                : $"\u5907\u6ce8: {section.DisplayNote}");
    }

    private static Ra2ValueReferenceSymbol? FindReferenceAtContext(Ra2DocumentSemanticModel model, Ra2CaretContext context)
    {
        if (context.KeyValue is null)
            return null;

        return model.References.FirstOrDefault(reference =>
            reference.LineNumber == context.KeyValue.LineNumber &&
            string.Equals(reference.SourceSectionName, context.KeyValue.SectionName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(reference.SourceKey, context.KeyValue.Key, StringComparison.OrdinalIgnoreCase) &&
            reference.ValueSpan.Contains(context.Offset));
    }
}
