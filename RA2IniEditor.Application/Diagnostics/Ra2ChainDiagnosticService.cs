using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Application.Language;

namespace RA2IniEditor.Application.Diagnostics;

internal sealed class Ra2ChainDiagnosticService
{
    public const string MissingWeaponCode = "CHAIN_WEAPON_MISSING";
    public const string MissingProjectileCode = "CHAIN_PROJECTILE_MISSING";
    public const string MissingWarheadCode = "CHAIN_WARHEAD_MISSING";
    public const string SourceKind = "Chain";

    public IReadOnlyList<Ra2DiagnosticFact> AnalyzeCurrentDocument(
        Ra2DocumentSnapshot snapshot,
        Ra2DocumentSemanticModel semanticModel,
        Ra2ReferenceDiagnosticCatalog catalog,
        string scopeLabel = "当前文件",
        CancellationToken cancellationToken = default,
        int maximumResultItems = int.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(semanticModel);
        ArgumentNullException.ThrowIfNull(catalog);

        List<Ra2DiagnosticFact> issues = [];
        for (int index = 0; index < semanticModel.References.Count; index++)
        {
            if (index % Ra2DocumentDiagnosticService.CancellationCheckInterval == 0)
                cancellationToken.ThrowIfCancellationRequested();

            Ra2ValueReferenceSymbol reference = semanticModel.References[index];
            if (!TryResolveChainKind(reference, out string code, out string targetLabel))
                continue;

            Ra2KeyValueSymbol? sourceKeyValue = FindSourceKeyValue(semanticModel, reference);
            if (sourceKeyValue is null || sourceKeyValue.SectionKind == Ra2SectionKind.Unknown)
                continue;

            if (ShouldSkipTarget(reference.TargetSectionName))
                continue;

            if (catalog.ContainsSection(reference.TargetSectionName))
                continue;

            Ra2DiagnosticLimitGuard.ThrowIfAdditionExceeds(issues.Count, maximumResultItems);
            issues.Add(CreateIssue(snapshot, semanticModel, reference, code, targetLabel, scopeLabel));
        }

        cancellationToken.ThrowIfCancellationRequested();
        return issues;
    }

    private static bool TryResolveChainKind(
        Ra2ValueReferenceSymbol reference,
        out string code,
        out string targetLabel)
    {
        switch (reference.ReferenceKind)
        {
            case Ra2ValueReferenceKind.WeaponReference:
                code = MissingWeaponCode;
                targetLabel = "武器";
                return true;
            case Ra2ValueReferenceKind.ProjectileReference:
                code = MissingProjectileCode;
                targetLabel = "Projectile";
                return true;
            case Ra2ValueReferenceKind.WarheadReference:
                code = MissingWarheadCode;
                targetLabel = "Warhead";
                return true;
            default:
                code = string.Empty;
                targetLabel = string.Empty;
                return false;
        }
    }

    private static Ra2KeyValueSymbol? FindSourceKeyValue(
        Ra2DocumentSemanticModel semanticModel,
        Ra2ValueReferenceSymbol reference)
        => semanticModel.KeyValues.FirstOrDefault(keyValue =>
            keyValue.LineNumber == reference.LineNumber &&
            string.Equals(keyValue.SectionName, reference.SourceSectionName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(keyValue.Key, reference.SourceKey, StringComparison.OrdinalIgnoreCase));

    private static bool ShouldSkipTarget(string target)
    {
        string value = target.Trim();
        if (string.IsNullOrWhiteSpace(value))
            return true;

        if (value.Equals("none", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("<none>", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("empty", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("-1", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("0", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return value.StartsWith('%') ||
               value.StartsWith('$') ||
               (value.StartsWith('{') && value.EndsWith('}'));
    }

    private static Ra2DiagnosticFact CreateIssue(
        Ra2DocumentSnapshot snapshot,
        Ra2DocumentSemanticModel semanticModel,
        Ra2ValueReferenceSymbol reference,
        string code,
        string targetLabel,
        string scopeLabel)
    {
        int columnNumber = Math.Max(
            1,
            reference.ValueSpan.Start - ResolveLineStart(semanticModel.Snapshot.Text, reference.ValueSpan.Start) + 1);
        string message = code switch
        {
            MissingWeaponCode => $"武器引用可能不存在：{reference.TargetSectionName}。字段 {reference.SourceKey} 指向的武器未在{scopeLabel}中找到。",
            MissingProjectileCode => $"Projectile 引用可能不存在：{reference.TargetSectionName}。武器 {reference.SourceSectionName} 指向的 Projectile 未在{scopeLabel}中找到。",
            MissingWarheadCode => $"Warhead 引用可能不存在：{reference.TargetSectionName}。武器 {reference.SourceSectionName} 指向的 Warhead 未在{scopeLabel}中找到。",
            _ => $"{targetLabel} 引用可能不存在：{reference.TargetSectionName}。"
        };

        return new Ra2DiagnosticFact(
            code,
            SourceKind,
            IniIssueSeverity.Warning,
            message,
            snapshot.FilePath ?? string.Empty,
            reference.LineNumber,
            columnNumber,
            reference.SourceSectionName,
            reference.SourceKey,
            snapshot.Version);
    }

    private static int ResolveLineStart(string text, int offset)
    {
        int normalizedOffset = Math.Clamp(offset, 0, text.Length);
        int lineStart = text.LastIndexOf('\n', Math.Max(0, normalizedOffset - 1));
        return lineStart < 0 ? 0 : lineStart + 1;
    }
}
