using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Application.Diagnostics;
using RA2IniEditor.Core;

namespace RA2IniEditor.Application.Editing;

internal static class Ra2AutomationDiagnosticDeltaCalculator
{
    public static Ra2AutomationDiagnosticDelta Compare(
        IReadOnlyList<Ra2AutomationDiagnosticFact> current,
        IReadOnlyList<Ra2AutomationDiagnosticFact> candidate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(candidate);

        Dictionary<DiagnosticFingerprint, int> currentCounts = BuildCounts(current, cancellationToken);
        List<Ra2AutomationDiagnosticFact> added = [];
        for (int index = 0; index < candidate.Count; index++)
        {
            CheckCancellation(index, cancellationToken);
            Ra2AutomationDiagnosticFact diagnostic = candidate[index];
            if (!TryConsume(currentCounts, DiagnosticFingerprint.From(diagnostic)))
                added.Add(diagnostic);
        }

        Dictionary<DiagnosticFingerprint, int> candidateCounts = BuildCounts(candidate, cancellationToken);
        List<Ra2AutomationDiagnosticFact> removed = [];
        for (int index = 0; index < current.Count; index++)
        {
            CheckCancellation(index, cancellationToken);
            Ra2AutomationDiagnosticFact diagnostic = current[index];
            if (!TryConsume(candidateCounts, DiagnosticFingerprint.From(diagnostic)))
                removed.Add(diagnostic);
        }

        return new Ra2AutomationDiagnosticDelta(
            Array.AsReadOnly(added.ToArray()),
            Array.AsReadOnly(removed.ToArray()));
    }

    private static Dictionary<DiagnosticFingerprint, int> BuildCounts(
        IReadOnlyList<Ra2AutomationDiagnosticFact> diagnostics,
        CancellationToken cancellationToken)
    {
        Dictionary<DiagnosticFingerprint, int> counts = [];
        for (int index = 0; index < diagnostics.Count; index++)
        {
            CheckCancellation(index, cancellationToken);
            DiagnosticFingerprint fingerprint = DiagnosticFingerprint.From(diagnostics[index]);
            counts.TryGetValue(fingerprint, out int count);
            counts[fingerprint] = count + 1;
        }

        return counts;
    }

    private static bool TryConsume(
        IDictionary<DiagnosticFingerprint, int> counts,
        DiagnosticFingerprint fingerprint)
    {
        if (!counts.TryGetValue(fingerprint, out int count) || count == 0)
            return false;

        counts[fingerprint] = count - 1;
        return true;
    }

    private static void CheckCancellation(int index, CancellationToken cancellationToken)
    {
        if (index % Ra2DocumentDiagnosticService.CancellationCheckInterval == 0)
            cancellationToken.ThrowIfCancellationRequested();
    }

    private readonly record struct DiagnosticFingerprint(
        string Code,
        string SourceKind,
        IniIssueSeverity Severity,
        string Message,
        string? SectionId,
        string? Key)
    {
        public static DiagnosticFingerprint From(Ra2AutomationDiagnosticFact fact)
            => new(fact.Code, fact.SourceKind, fact.Severity, fact.Message, fact.SectionId, fact.Key);
    }
}

internal readonly record struct Ra2AutomationDiagnosticDelta(
    IReadOnlyList<Ra2AutomationDiagnosticFact> Added,
    IReadOnlyList<Ra2AutomationDiagnosticFact> Removed);
