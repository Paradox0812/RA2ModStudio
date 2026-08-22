using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Cleanup;

/// <summary>
/// Builds a readonly plan for consolidating repeated concrete fields into abstract Unit or Techno fields.
/// </summary>
public sealed class FieldRegistryGeneralizationCleanupPlanner
{
    private static readonly Ra2SectionKind[] UnitKinds =
    [
        Ra2SectionKind.Infantry,
        Ra2SectionKind.Vehicle,
        Ra2SectionKind.Aircraft
    ];

    private static readonly Ra2SectionKind[] TechnoKinds =
    [
        Ra2SectionKind.Infantry,
        Ra2SectionKind.Vehicle,
        Ra2SectionKind.Aircraft,
        Ra2SectionKind.Building
    ];

    private readonly LocalFieldRegistryLoader _loader;

    public FieldRegistryGeneralizationCleanupPlanner()
        : this(new LocalFieldRegistryLoader())
    {
    }

    public FieldRegistryGeneralizationCleanupPlanner(LocalFieldRegistryLoader loader)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
    }

    public FieldRegistryGeneralizationCleanupPlan BuildPlan(FieldRegistryGeneralizationCleanupRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        List<FieldRegistryGeneralizationCleanupRow> rows = new();
        List<string> warnings = new();
        AnalyzeScope("Global", request.GlobalActiveDirectoryPath, rows, warnings);
        if (!string.IsNullOrWhiteSpace(request.ProjectActiveDirectoryPath))
            AnalyzeScope("Project", request.ProjectActiveDirectoryPath, rows, warnings);

        return new FieldRegistryGeneralizationCleanupPlan(
            Array.AsReadOnly(rows
                .OrderBy(row => row.Scope, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.TargetSectionKind)
                .ThenBy(row => row.Key, StringComparer.OrdinalIgnoreCase)
                .ToArray()),
            Array.AsReadOnly(warnings.ToArray()));
    }

    private void AnalyzeScope(
        string scope,
        string activeDirectoryPath,
        List<FieldRegistryGeneralizationCleanupRow> rows,
        List<string> warnings)
    {
        LocalFieldRegistryLoadResult result = _loader.LoadDirectory(activeDirectoryPath);
        warnings.AddRange(result.Warnings.Select(warning => $"{scope}: {warning}"));
        if (result.LoadedDefinitions.Count == 0)
            return;

        foreach (IGrouping<string, LocalFieldRegistryLoadedDefinition> group in result.LoadedDefinitions.GroupBy(
            loaded => loaded.Definition.Key,
            StringComparer.OrdinalIgnoreCase))
        {
            if (TryBuildRow(scope, group, TechnoKinds, Ra2SectionKind.Techno, out FieldRegistryGeneralizationCleanupRow? technoRow) &&
                technoRow is not null)
            {
                rows.Add(technoRow);
                continue;
            }

            if (TryBuildRow(scope, group, UnitKinds, Ra2SectionKind.Unit, out FieldRegistryGeneralizationCleanupRow? unitRow) &&
                unitRow is not null)
                rows.Add(unitRow);
        }
    }

    private static bool TryBuildRow(
        string scope,
        IEnumerable<LocalFieldRegistryLoadedDefinition> loadedDefinitions,
        IReadOnlyList<Ra2SectionKind> requiredKinds,
        Ra2SectionKind targetKind,
        out FieldRegistryGeneralizationCleanupRow? row)
    {
        row = null;
        List<LocalFieldRegistryLoadedDefinition> candidates = loadedDefinitions
            .Where(loaded => loaded.Definition.AppliesTo.Count == 1 &&
                             requiredKinds.Contains(loaded.Definition.AppliesTo.Single()))
            .ToList();

        List<LocalFieldRegistryLoadedDefinition> matches = requiredKinds
            .Select(kind => candidates.LastOrDefault(loaded => loaded.Definition.AppliesTo.Single() == kind))
            .Where(loaded => loaded is not null)
            .Cast<LocalFieldRegistryLoadedDefinition>()
            .ToList();
        if (matches.Count != requiredKinds.Count)
            return false;

        Ra2FieldDefinition first = matches[0].Definition;
        if (!matches.All(match => IsCompatible(first, match.Definition)))
            return false;

        int allowedValueCount = matches
            .SelectMany(match => match.Definition.ValueMetadata.AllowedValues)
            .Select(value => value.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        string actionText = $"Preview only: add/update '{first.Key}' as {targetKind}, then remove {matches.Count} concrete duplicate definition(s).";

        row = new FieldRegistryGeneralizationCleanupRow(
            scope,
            first.Key,
            targetKind,
            Array.AsReadOnly(requiredKinds.ToArray()),
            Array.AsReadOnly(matches
                .Select(match => match.SourceFileName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray()),
            first.EditorKind,
            first.ValueMetadata.ValueKind,
            matches.Count,
            allowedValueCount,
            actionText);
        return true;
    }

    private static bool IsCompatible(Ra2FieldDefinition left, Ra2FieldDefinition right)
    {
        return left.EditorKind == right.EditorKind &&
               left.ValueMetadata.ValueKind == right.ValueMetadata.ValueKind &&
               left.ValueMetadata.BooleanStyle == right.ValueMetadata.BooleanStyle &&
               string.Equals(left.ValueMetadata.Separator, right.ValueMetadata.Separator, StringComparison.Ordinal);
    }
}
