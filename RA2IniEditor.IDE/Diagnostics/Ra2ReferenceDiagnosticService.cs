using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.Diagnostics;

internal sealed class Ra2ReferenceDiagnosticService
{
    public const string MissingTargetCode = "REF_MISSING_TARGET";

    private const string SourceKind = "Reference";

    public IReadOnlyList<IdeDiagnosticIssueViewModel> AnalyzeCurrentDocument(
        CurrentSourceSnapshot snapshot,
        Ra2DocumentSemanticModel semanticModel,
        IRa2FieldDefinitionProvider? fieldProvider,
        Ra2ReferenceDiagnosticCatalog catalog,
        string scopeLabel = "当前文件")
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(semanticModel);
        ArgumentNullException.ThrowIfNull(catalog);

        List<IdeDiagnosticIssueViewModel> issues = [];
        foreach (Ra2ValueReferenceSymbol reference in semanticModel.References)
        {
            if (ShouldSkipReference(reference, semanticModel, fieldProvider, catalog))
                continue;

            issues.Add(new IdeDiagnosticIssueViewModel(
                MissingTargetCode,
                SourceKind,
                IniIssueSeverity.Warning,
                $"引用目标可能不存在：{reference.TargetSectionName}。字段 {reference.SourceKey} 指向的对象未在{scopeLabel}中找到。",
                snapshot.FilePath,
                reference.LineNumber,
                Math.Max(1, reference.ValueSpan.Start - ResolveLineStart(semanticModel.Snapshot.Text, reference.ValueSpan.Start) + 1),
                reference.SourceSectionName,
                reference.SourceKey,
                snapshot.Version));
        }

        return issues;
    }

    private static bool ShouldSkipReference(
        Ra2ValueReferenceSymbol reference,
        Ra2DocumentSemanticModel semanticModel,
        IRa2FieldDefinitionProvider? fieldProvider,
        Ra2ReferenceDiagnosticCatalog catalog)
    {
        if (reference.ReferenceKind == Ra2ValueReferenceKind.Unknown ||
            string.IsNullOrWhiteSpace(reference.TargetSectionName) ||
            IsNeutralOrComplexReferenceValue(reference.TargetSectionName) ||
            catalog.ContainsSection(reference.TargetSectionName))
        {
            return true;
        }

        Ra2KeyValueSymbol? keyValue = semanticModel.KeyValues.FirstOrDefault(candidate =>
            candidate.LineNumber == reference.LineNumber &&
            string.Equals(candidate.SectionName, reference.SourceSectionName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(candidate.Key, reference.SourceKey, StringComparison.OrdinalIgnoreCase));
        if (keyValue is null || keyValue.SectionKind == Ra2SectionKind.Unknown)
            return true;

        return fieldProvider is not null &&
               TryResolveAllowedValues(fieldProvider, keyValue.SectionKind, keyValue.Key, out IReadOnlyCollection<Ra2FieldAllowedValue> allowedValues) &&
               allowedValues.Any(value => string.Equals(value.Value, reference.TargetSectionName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryResolveAllowedValues(
        IRa2FieldDefinitionProvider fieldProvider,
        Ra2SectionKind sectionKind,
        string key,
        out IReadOnlyCollection<Ra2FieldAllowedValue> allowedValues)
    {
        allowedValues = [];
        if (!fieldProvider.TryGetField(sectionKind, key, out Ra2FieldDefinition definition))
            return false;

        allowedValues = definition.ValueMetadata.AllowedValues;
        return allowedValues.Count > 0;
    }

    private static bool IsNeutralOrComplexReferenceValue(string value)
    {
        string token = value.Trim();
        return token.Equals("empty", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("none", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("<none>", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("null", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("-1", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("0", StringComparison.OrdinalIgnoreCase) ||
               token.Contains('{', StringComparison.Ordinal) ||
               token.Contains('}', StringComparison.Ordinal) ||
               token.Contains('%', StringComparison.Ordinal) ||
               token.StartsWith("$", StringComparison.Ordinal);
    }

    private static int ResolveLineStart(string text, int offset)
    {
        if (string.IsNullOrEmpty(text) || offset <= 0)
            return 0;

        int index = Math.Min(offset, text.Length - 1);
        while (index > 0 && text[index - 1] is not '\n' and not '\r')
            index--;

        return index;
    }
}
