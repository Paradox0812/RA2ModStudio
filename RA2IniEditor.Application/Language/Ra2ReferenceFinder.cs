using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Language;

internal sealed class Ra2ReferenceFinder : IRa2ReferenceFinder
{
    public Ra2ReferenceResult FindReferences(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        Ra2TextSpan? selectionSpan = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(context);

        if (!TryResolveTarget(model, context, selectionSpan, out string targetName, out Ra2SectionKind targetKind))
            return new Ra2ReferenceResult(string.Empty, Ra2SectionKind.Unknown, []);

        List<Ra2ReferenceItem> items = [];
        foreach (Ra2ValueReferenceSymbol reference in model.References)
        {
            if (!string.Equals(reference.TargetSectionName, targetName, StringComparison.OrdinalIgnoreCase))
                continue;

            Ra2KeyValueSymbol? keyValue = FindSourceKeyValue(model, reference);
            if (keyValue is null)
                continue;

            items.Add(new Ra2ReferenceItem(
                reference.SourceSectionName,
                reference.SourceKey,
                reference.TargetSectionName,
                reference.LineNumber,
                keyValue.LineSpan,
                reference.ValueSpan));
        }

        return new Ra2ReferenceResult(targetName, targetKind, items);
    }

    private static bool TryResolveTarget(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        Ra2TextSpan? selectionSpan,
        out string targetName,
        out Ra2SectionKind targetKind)
    {
        targetName = string.Empty;
        targetKind = Ra2SectionKind.Unknown;

        if (selectionSpan is Ra2TextSpan selectedSpan &&
            TryResolveTargetFromSelection(model, selectedSpan, out targetName, out targetKind))
        {
            return true;
        }

        if (context.Region == Ra2CaretRegion.SectionHeader && context.Section is not null)
        {
            targetName = context.Section.Name;
            targetKind = context.Section.Kind;
            return true;
        }

        if (context.Region != Ra2CaretRegion.Value || context.KeyValue is null)
            return false;

        Ra2ValueReferenceSymbol? reference = model.References.FirstOrDefault(candidate =>
            candidate.LineNumber == context.KeyValue.LineNumber &&
            string.Equals(candidate.SourceSectionName, context.KeyValue.SectionName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(candidate.SourceKey, context.KeyValue.Key, StringComparison.OrdinalIgnoreCase) &&
            candidate.ValueSpan.Contains(context.Offset));
        if (reference is null)
            return false;

        targetName = reference.TargetSectionName;
        targetKind = reference.TargetSectionKind;
        return true;
    }

    private static bool TryResolveTargetFromSelection(
        Ra2DocumentSemanticModel model,
        Ra2TextSpan selectionSpan,
        out string targetName,
        out Ra2SectionKind targetKind)
    {
        targetName = string.Empty;
        targetKind = Ra2SectionKind.Unknown;
        if (selectionSpan.Length <= 0 || selectionSpan.End > model.Snapshot.Text.Length)
            return false;

        Ra2KeyValueSymbol? keyValue = model.KeyValues.FirstOrDefault(candidate =>
            candidate.LineSpan.Contains(selectionSpan.Start) &&
            candidate.LineSpan.Contains(Math.Max(selectionSpan.Start, selectionSpan.End - 1)));
        if (keyValue?.ValueSpan is not Ra2TextSpan valueSpan ||
            selectionSpan.Start < valueSpan.Start ||
            selectionSpan.End > valueSpan.End)
        {
            return false;
        }

        string selectedText = model.Snapshot.Text.Substring(selectionSpan.Start, selectionSpan.Length);
        string effectiveValue = GetFirstSelectedReferenceCandidate(selectedText);
        if (string.IsNullOrWhiteSpace(effectiveValue))
            return false;

        Ra2ValueReferenceSymbol? reference = model.References.FirstOrDefault(candidate =>
            candidate.LineNumber == keyValue.LineNumber &&
            string.Equals(candidate.SourceSectionName, keyValue.SectionName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(candidate.SourceKey, keyValue.Key, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(candidate.TargetSectionName, effectiveValue, StringComparison.OrdinalIgnoreCase));
        if (reference is null)
            return false;

        targetName = reference.TargetSectionName;
        targetKind = reference.TargetSectionKind;
        return true;
    }

    private static string GetFirstSelectedReferenceCandidate(string selectedText)
    {
        string effective = Ra2IniLineParser.GetEffectiveValue(selectedText);
        int comma = effective.IndexOf(',');
        if (comma >= 0)
            effective = effective[..comma];

        return effective.Trim();
    }

    private static Ra2KeyValueSymbol? FindSourceKeyValue(
        Ra2DocumentSemanticModel model,
        Ra2ValueReferenceSymbol reference)
    {
        return model.KeyValues.FirstOrDefault(keyValue =>
            keyValue.LineNumber == reference.LineNumber &&
            string.Equals(keyValue.SectionName, reference.SourceSectionName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(keyValue.Key, reference.SourceKey, StringComparison.OrdinalIgnoreCase));
    }
}
