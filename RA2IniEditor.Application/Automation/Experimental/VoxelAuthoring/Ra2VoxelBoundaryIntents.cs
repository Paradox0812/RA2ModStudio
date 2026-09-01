namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

[Flags]
internal enum Ra2VoxelBoundaryIntent : ushort
{
    None = 0,
    RaisedBevel = 1 << 0,
    StructuralSeam = 1 << 1,
    DeepOpening = 1 << 2,
    ContactShadow = 1 << 3,
    MaterialInterface = 1 << 4,
    PanelLine = 1 << 5,
    Silhouette = 1 << 6,
    DecorativeMark = 1 << 7
}

internal sealed record Ra2VoxelBoundaryIntentCount(
    Ra2VoxelBoundaryIntent Intent,
    int OpportunityCellCount,
    int OwnedCellCount);

internal sealed class Ra2VoxelBoundaryIntentProjection
{
    private readonly ushort[] _intents;
    private readonly int[] _owners;
    private readonly Ra2VoxelBoundaryIntentCount[] _counts;
    private readonly string[] _diagnostics;

    internal Ra2VoxelBoundaryIntentProjection(
        string sourceSnapshotHash,
        string compositionHash,
        string formZoneProjectionHash,
        string featureScaleProjectionHash,
        string techniquePolicyHash,
        IEnumerable<ushort> intents,
        IEnumerable<int> owners,
        IEnumerable<Ra2VoxelBoundaryIntentCount> counts,
        IEnumerable<string> diagnostics)
    {
        SourceSnapshotHash = Ra2VoxelColourContractIdentity.RequireSha256(
            sourceSnapshotHash, nameof(sourceSnapshotHash));
        CompositionHash = Ra2VoxelColourContractIdentity.RequireSha256(
            compositionHash, nameof(compositionHash));
        FormZoneProjectionHash = Ra2VoxelColourContractIdentity.RequireSha256(
            formZoneProjectionHash, nameof(formZoneProjectionHash));
        FeatureScaleProjectionHash = Ra2VoxelColourContractIdentity.RequireSha256(
            featureScaleProjectionHash, nameof(featureScaleProjectionHash));
        TechniquePolicyHash = Ra2VoxelColourContractIdentity.RequireSha256(
            techniquePolicyHash, nameof(techniquePolicyHash));
        _intents = (intents ?? throw new ArgumentNullException(nameof(intents))).ToArray();
        _owners = (owners ?? throw new ArgumentNullException(nameof(owners))).ToArray();
        if (_intents.Length != _owners.Length)
            throw new ArgumentException("Boundary intent and owner arrays must have the same length.");
        for (int index = 0; index < _owners.Length; index++)
        {
            if (_owners[index] is < -1 || _owners[index] >= _owners.Length)
                throw new ArgumentOutOfRangeException(nameof(owners));
            if (_owners[index] >= 0 && _owners[index] != index)
                throw new ArgumentException("A boundary cell must own its own one-cell paint operation.", nameof(owners));
        }
        _counts = (counts ?? throw new ArgumentNullException(nameof(counts)))
            .OrderBy(value => value.Intent)
            .ToArray();
        _diagnostics = (diagnostics ?? throw new ArgumentNullException(nameof(diagnostics)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        ProjectionHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-boundary-intent/1");
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, SourceSnapshotHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, CompositionHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, FormZoneProjectionHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, FeatureScaleProjectionHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, TechniquePolicyHash);
            writer.Write(_intents.Length);
            for (int index = 0; index < _intents.Length; index++)
            {
                writer.Write(_intents[index]);
                writer.Write(_owners[index]);
            }
            writer.Write(_diagnostics.Length);
            foreach (string diagnostic in _diagnostics)
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, diagnostic);
        });
    }

    internal string SourceSnapshotHash { get; }
    internal string CompositionHash { get; }
    internal string FormZoneProjectionHash { get; }
    internal string FeatureScaleProjectionHash { get; }
    internal string TechniquePolicyHash { get; }
    internal int CellCount => _intents.Length;
    internal IReadOnlyList<Ra2VoxelBoundaryIntentCount> Counts => Array.AsReadOnly(_counts);
    internal IReadOnlyList<string> Diagnostics => Array.AsReadOnly(_diagnostics);
    internal string ProjectionHash { get; }
    internal Ra2VoxelBoundaryIntent IntentAt(int index) => (Ra2VoxelBoundaryIntent)_intents[index];
    internal int OwnerAt(int index) => _owners[index];
    internal bool Contains(int index, Ra2VoxelBoundaryIntent intent) =>
        (((Ra2VoxelBoundaryIntent)_intents[index]) & intent) != 0;

    internal Ra2VoxelExplicitMask CreateOwnedMask(string maskId, Ra2VoxelBoundaryIntent intent)
    {
        byte[] selected = new byte[_intents.Length];
        for (int index = 0; index < selected.Length; index++)
        {
            if (_owners[index] == index && Contains(index, intent))
                selected[index] = 1;
        }
        return new(maskId, SourceSnapshotHash, selected);
    }
}

internal static class Ra2VoxelBoundaryIntentProjector
{
    internal const string Revision = "boundary-intent-projector/1";

    internal static Ra2VoxelBoundaryIntentProjection Project(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelSemanticMaskComposition composition,
        Ra2VoxelFormZoneProjection formZones,
        Ra2VoxelColourTechniquePolicy technique,
        Ra2VoxelFeatureScaleProjection? featureScale = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(composition);
        ArgumentNullException.ThrowIfNull(formZones);
        ArgumentNullException.ThrowIfNull(technique);
        featureScale ??= Ra2VoxelFeatureScaleProjector.Project(snapshot, composition, formZones);
        if (!string.Equals(snapshot.CanonicalHash, composition.SourceSnapshotHash, StringComparison.Ordinal) ||
            !string.Equals(snapshot.CanonicalHash, formZones.SourceSnapshotHash, StringComparison.Ordinal) ||
            snapshot.OccupancyCount != composition.CellCount || snapshot.OccupancyCount != formZones.CellCount ||
            snapshot.OccupancyCount != featureScale.CellCount ||
            !string.Equals(featureScale.SourceSnapshotHash, snapshot.CanonicalHash, StringComparison.Ordinal))
        {
            throw new ArgumentException("Boundary intent inputs do not match the current snapshot.");
        }

        Dictionary<Ra2VoxelCoordinate, int> indexByCoordinate = snapshot.Cells
            .Select((cell, index) => (cell.Coordinate, index))
            .ToDictionary(value => value.Coordinate, value => value.index);
        ushort[] intents = new ushort[snapshot.OccupancyCount];
        int[] owners = Enumerable.Repeat(-1, snapshot.OccupancyCount).ToArray();
        int protectedDirect = 0;

        for (int index = 0; index < snapshot.OccupancyCount; index++)
        {
            Ra2VoxelSemanticEffectiveAssignment current = composition[index];
            bool painted = IsPainted(current);
            Ra2VoxelBoundaryIntent intent = Ra2VoxelBoundaryIntent.None;

            if (formZones.Contains(index, Ra2VoxelFormZone.SilhouetteRidge))
                intent |= Ra2VoxelBoundaryIntent.Silhouette;
            if (painted && formZones.Contains(index, Ra2VoxelFormZone.UpperBevel) &&
                formZones.Contains(index, Ra2VoxelFormZone.SideShoulder) &&
                !formZones.Contains(index, Ra2VoxelFormZone.FrontEnd | Ra2VoxelFormZone.RearEnd))
            {
                intent |= Ra2VoxelBoundaryIntent.RaisedBevel;
                if (AllowsPaint(index, Ra2VoxelBoundaryIntent.RaisedBevel)) owners[index] = index;
            }
            if (current.MaterialRole == Ra2VoxelSemanticMaterialRole.DarkOpening)
                intent |= Ra2VoxelBoundaryIntent.DeepOpening;

            foreach (Ra2VoxelFaceDirection direction in Ra2VoxelNeighbourhood.OrderedDirections)
            {
                (int dx, int dy, int dz) = Ra2VoxelNeighbourhood.Offset(direction);
                Ra2VoxelCoordinate coordinate = snapshot.Cells[index].Coordinate;
                if (!indexByCoordinate.TryGetValue(
                        new(coordinate.X + dx, coordinate.Y + dy, coordinate.Z + dz), out int neighbourIndex))
                    continue;
                Ra2VoxelSemanticEffectiveAssignment neighbour = composition[neighbourIndex];
                bool materialBoundary = KnownMaterial(current.MaterialRole) && KnownMaterial(neighbour.MaterialRole) &&
                    current.MaterialRole != neighbour.MaterialRole;
                if (materialBoundary)
                {
                    intent |= Ra2VoxelBoundaryIntent.MaterialInterface;
                    if (!painted) protectedDirect++;
                }

                bool partBoundary = current.PartRole != Ra2VoxelSemanticPartRole.Unknown &&
                    neighbour.PartRole != Ra2VoxelSemanticPartRole.Unknown && current.PartRole != neighbour.PartRole;
                if (!partBoundary || !painted || !OwnsPartBoundary(current, neighbour, technique.AccentPolicy))
                    continue;
                bool contact = formZones.Contains(index, Ra2VoxelFormZone.LowerSkirt) ||
                    neighbour.PartRole is Ra2VoxelSemanticPartRole.Wheel or Ra2VoxelSemanticPartRole.Track;
                Ra2VoxelBoundaryIntent partIntent = contact
                    ? Ra2VoxelBoundaryIntent.ContactShadow
                    : Ra2VoxelBoundaryIntent.StructuralSeam;
                intent |= partIntent;
                if (AllowsPaint(index, partIntent)) owners[index] = index;
            }
            intents[index] = (ushort)intent;
        }

        RemoveIsolatedPaint(intents, owners, snapshot, indexByCoordinate, Ra2VoxelBoundaryIntent.RaisedBevel);
        RemoveIsolatedPaint(intents, owners, snapshot, indexByCoordinate, Ra2VoxelBoundaryIntent.StructuralSeam);
        RemoveIsolatedPaint(intents, owners, snapshot, indexByCoordinate, Ra2VoxelBoundaryIntent.ContactShadow);

        Ra2VoxelBoundaryIntent[] values = Enum.GetValues<Ra2VoxelBoundaryIntent>()
            .Where(value => value != Ra2VoxelBoundaryIntent.None && IsSingleBit((ushort)value))
            .ToArray();
        Ra2VoxelBoundaryIntentCount[] counts = values.Select(value => new Ra2VoxelBoundaryIntentCount(
            value,
            intents.Count(item => (((Ra2VoxelBoundaryIntent)item) & value) != 0),
            Enumerable.Range(0, intents.Length).Count(index => owners[index] == index &&
                (((Ra2VoxelBoundaryIntent)intents[index]) & value) != 0))).ToArray();
        List<string> diagnostics = [];
        if (protectedDirect > 0)
            diagnostics.Add($"ProtectedDirectMaterialInterfaces:{protectedDirect}");
        return new(snapshot.CanonicalHash, composition.CompositionHash, formZones.ProjectionHash,
            featureScale.ProjectionHash,
            technique.PolicyHash, intents, owners, counts, diagnostics);

        bool AllowsPaint(int index, Ra2VoxelBoundaryIntent intent)
        {
            if ((technique.AllowedBoundaryIntents & intent) == 0) return false;
            return !technique.CompressMicroDetails ||
                   featureScale[index] is Ra2VoxelFeatureScale.Macro or Ra2VoxelFeatureScale.Meso;
        }
    }

    private static void RemoveIsolatedPaint(
        ushort[] intents,
        int[] owners,
        Ra2VoxelSceneSnapshot snapshot,
        IReadOnlyDictionary<Ra2VoxelCoordinate, int> indexByCoordinate,
        Ra2VoxelBoundaryIntent intent)
    {
        int[] isolated = Enumerable.Range(0, intents.Length)
            .Where(index => owners[index] == index && (((Ra2VoxelBoundaryIntent)intents[index]) & intent) != 0)
            .Where(index => !Ra2VoxelNeighbourhood.OrderedDirections.Any(direction =>
            {
                (int dx, int dy, int dz) = Ra2VoxelNeighbourhood.Offset(direction);
                Ra2VoxelCoordinate coordinate = snapshot.Cells[index].Coordinate;
                return indexByCoordinate.TryGetValue(
                           new(coordinate.X + dx, coordinate.Y + dy, coordinate.Z + dz), out int neighbour) &&
                       (((Ra2VoxelBoundaryIntent)intents[neighbour]) & intent) != 0;
            }))
            .ToArray();
        foreach (int index in isolated)
        {
            intents[index] = (ushort)(((Ra2VoxelBoundaryIntent)intents[index]) & ~intent);
            if ((((Ra2VoxelBoundaryIntent)intents[index]) & PaintableIntents) == 0)
                owners[index] = -1;
        }
    }

    private const Ra2VoxelBoundaryIntent PaintableIntents =
        Ra2VoxelBoundaryIntent.RaisedBevel | Ra2VoxelBoundaryIntent.StructuralSeam |
        Ra2VoxelBoundaryIntent.ContactShadow | Ra2VoxelBoundaryIntent.PanelLine |
        Ra2VoxelBoundaryIntent.DecorativeMark;

    private static bool OwnsPartBoundary(
        Ra2VoxelSemanticEffectiveAssignment current,
        Ra2VoxelSemanticEffectiveAssignment neighbour,
        Ra2VoxelAccentPolicy accentPolicy)
    {
        if (!IsPainted(neighbour)) return true;
        int currentPriority = Priority(current.PartRole, accentPolicy);
        int neighbourPriority = Priority(neighbour.PartRole, accentPolicy);
        return currentPriority > neighbourPriority ||
               (currentPriority == neighbourPriority && current.PartRole > neighbour.PartRole);
    }

    private static int Priority(Ra2VoxelSemanticPartRole role, Ra2VoxelAccentPolicy accentPolicy)
    {
        int value = role switch
        {
            Ra2VoxelSemanticPartRole.BodyShell => 0,
            Ra2VoxelSemanticPartRole.Turret => 20,
            Ra2VoxelSemanticPartRole.Attachment => 30,
            Ra2VoxelSemanticPartRole.Wheel or Ra2VoxelSemanticPartRole.Track => 40,
            Ra2VoxelSemanticPartRole.Barrel => 50,
            Ra2VoxelSemanticPartRole.Antenna => 60,
            _ => -1
        };
        return accentPolicy == Ra2VoxelAccentPolicy.EmphasizeSmallMask &&
               role is Ra2VoxelSemanticPartRole.Barrel or Ra2VoxelSemanticPartRole.Antenna or
                   Ra2VoxelSemanticPartRole.Attachment
            ? value + 100
            : value;
    }

    private static bool IsPainted(Ra2VoxelSemanticEffectiveAssignment assignment) =>
        assignment.MaterialRole == Ra2VoxelSemanticMaterialRole.PaintedSurface &&
        assignment.RemapIntent != Ra2VoxelSemanticRemapIntent.ExplicitlyApproved;

    private static bool KnownMaterial(Ra2VoxelSemanticMaterialRole role) =>
        role != Ra2VoxelSemanticMaterialRole.Unknown;

    private static bool IsSingleBit(ushort value) => value != 0 && (value & (value - 1)) == 0;
}
