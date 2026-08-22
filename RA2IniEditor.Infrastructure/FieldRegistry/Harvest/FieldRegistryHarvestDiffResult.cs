namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestDiffResult
{
    public FieldRegistryHarvestDiffResult(IReadOnlyList<FieldRegistryHarvestDiffRow> rows)
    {
        Rows = rows ?? throw new ArgumentNullException(nameof(rows));
        AddedCount = rows.Count(row => row.Kind == FieldRegistryHarvestDiffKind.Added);
        SameCount = rows.Count(row => row.Kind == FieldRegistryHarvestDiffKind.Same);
        ChangedCount = rows.Count(row => row.Kind == FieldRegistryHarvestDiffKind.Changed);
        ConflictCount = rows.Count(row => row.Kind == FieldRegistryHarvestDiffKind.Conflict);
        InvalidCount = rows.Count(row => row.Kind == FieldRegistryHarvestDiffKind.Invalid);
    }

    public IReadOnlyList<FieldRegistryHarvestDiffRow> Rows { get; }

    public int AddedCount { get; }

    public int SameCount { get; }

    public int ChangedCount { get; }

    public int ConflictCount { get; }

    public int InvalidCount { get; }
}
