using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelForwardDirection
{
    Unknown = 0,
    PositiveX,
    NegativeX,
    PositiveY,
    NegativeY
}

internal enum Ra2VoxelForwardDirectionFailureKind
{
    None = 0,
    InvalidDirection,
    InvalidCompositionIdentity
}

internal sealed class Ra2VoxelForwardDirectionSelection
{
    private Ra2VoxelForwardDirectionSelection(
        string sourceSnapshotHash,
        string compositionHash,
        Ra2VoxelForwardDirection direction,
        string selectionHash)
    {
        SourceSnapshotHash = sourceSnapshotHash;
        CompositionHash = compositionHash;
        Direction = direction;
        SelectionHash = selectionHash;
    }

    internal string SourceSnapshotHash { get; }
    internal string CompositionHash { get; }
    internal Ra2VoxelForwardDirection Direction { get; }
    internal string Source => "HumanManualSelection";
    internal string SelectionHash { get; }
    internal bool IsConfirmed => Direction != Ra2VoxelForwardDirection.Unknown;

    internal static Ra2VoxelForwardDirectionSelectionResult Create(
        Ra2VoxelSceneSnapshot snapshot,
        string compositionHash,
        Ra2VoxelForwardDirection direction)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!Enum.IsDefined(direction))
            return new(Ra2VoxelForwardDirectionFailureKind.InvalidDirection,
                "The selected forward direction is not supported.", null);
        string normalizedComposition;
        try
        {
            normalizedComposition = Ra2VoxelColourContractIdentity.RequireSha256(
                compositionHash, nameof(compositionHash));
        }
        catch (ArgumentException)
        {
            return new(Ra2VoxelForwardDirectionFailureKind.InvalidCompositionIdentity,
                "A canonical semantic composition identity is required.", null);
        }

        string sourceHash = snapshot.CanonicalHash.ToUpperInvariant();
        string selectionHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-forward-direction/1");
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, sourceHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, normalizedComposition);
            writer.Write((int)direction);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "HumanManualSelection");
        });
        return new(Ra2VoxelForwardDirectionFailureKind.None, string.Empty,
            new(sourceHash, normalizedComposition, direction, selectionHash));
    }
}

internal sealed record Ra2VoxelForwardDirectionSelectionResult(
    Ra2VoxelForwardDirectionFailureKind FailureKind,
    string Message,
    Ra2VoxelForwardDirectionSelection? Selection)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelForwardDirectionFailureKind.None && Selection is not null;
}

[Flags]
internal enum Ra2VoxelFormZone : ushort
{
    None = 0,
    Interior = 1 << 0,
    UpperPlane = 1 << 1,
    UpperBevel = 1 << 2,
    SideShoulder = 1 << 3,
    SideField = 1 << 4,
    LowerSkirt = 1 << 5,
    FrontEnd = 1 << 6,
    RearEnd = 1 << 7,
    LongitudinalEndUnknown = 1 << 8,
    Recess = 1 << 9,
    ContactShadow = 1 << 10,
    SilhouetteRidge = 1 << 11,
    UnclassifiedSurface = 1 << 12
}

internal enum Ra2VoxelFormZoneProjectionFailureKind
{
    None = 0,
    SnapshotMismatch,
    CompositionMismatch,
    ResourceLimitExceeded,
    Cancelled
}

internal sealed record Ra2VoxelFormZoneCount(Ra2VoxelFormZone Zone, int CellCount);

internal sealed class Ra2VoxelFormZoneProjection
{
    private readonly ushort[] _zones;
    private readonly Ra2VoxelFormZoneCount[] _counts;
    private readonly string[] _diagnostics;

    internal Ra2VoxelFormZoneProjection(
        string sourceSnapshotHash,
        string orientationSelectionHash,
        string adaptationPolicyHash,
        IEnumerable<ushort> zones,
        IEnumerable<Ra2VoxelFormZoneCount> counts,
        IEnumerable<string> diagnostics)
    {
        SourceSnapshotHash = Ra2VoxelColourContractIdentity.RequireSha256(
            sourceSnapshotHash, nameof(sourceSnapshotHash));
        OrientationSelectionHash = Ra2VoxelColourContractIdentity.RequireSha256(
            orientationSelectionHash, nameof(orientationSelectionHash));
        AdaptationPolicyHash = Ra2VoxelColourContractIdentity.RequireSha256(
            adaptationPolicyHash, nameof(adaptationPolicyHash));
        _zones = (zones ?? throw new ArgumentNullException(nameof(zones))).ToArray();
        _counts = (counts ?? throw new ArgumentNullException(nameof(counts)))
            .OrderBy(value => value.Zone)
            .ToArray();
        _diagnostics = (diagnostics ?? throw new ArgumentNullException(nameof(diagnostics)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        ProjectionHash = ComputeHash();
    }

    internal string SourceSnapshotHash { get; }
    internal string OrientationSelectionHash { get; }
    internal string AdaptationPolicyHash { get; }
    internal int CellCount => _zones.Length;
    internal IReadOnlyList<Ra2VoxelFormZoneCount> Counts => Array.AsReadOnly(_counts);
    internal IReadOnlyList<string> Diagnostics => Array.AsReadOnly(_diagnostics);
    internal string ProjectionHash { get; }
    internal Ra2VoxelFormZone this[int index] => (Ra2VoxelFormZone)_zones[index];

    internal bool Contains(int index, Ra2VoxelFormZone zone) =>
        (((Ra2VoxelFormZone)_zones[index]) & zone) != 0;

    private string ComputeHash() => Ra2VoxelColourContractIdentity.ComputeHash(writer =>
    {
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-form-zone-projection/1");
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, SourceSnapshotHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, OrientationSelectionHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, AdaptationPolicyHash);
        writer.Write(_zones.Length);
        foreach (ushort zone in _zones)
            writer.Write(zone);
        writer.Write(_diagnostics.Length);
        foreach (string diagnostic in _diagnostics)
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, diagnostic);
    });
}

internal sealed record Ra2VoxelFormZoneProjectionResult(
    Ra2VoxelFormZoneProjectionFailureKind FailureKind,
    string Message,
    Ra2VoxelFormZoneProjection? Projection)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelFormZoneProjectionFailureKind.None && Projection is not null;
}

internal static class Ra2VoxelFormZoneProjector
{
    internal const string Revision = "form-zone-projector/1";

    internal static Ra2VoxelFormZoneProjectionResult Project(
        Ra2VoxelSceneSnapshot snapshot,
        string compositionHash,
        Ra2VoxelForwardDirectionSelection orientation,
        Ra2VoxelUnitAdaptationPolicy adaptation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(orientation);
        ArgumentNullException.ThrowIfNull(adaptation);
        if (!string.Equals(snapshot.CanonicalHash, orientation.SourceSnapshotHash, StringComparison.Ordinal))
            return Failure(Ra2VoxelFormZoneProjectionFailureKind.SnapshotMismatch,
                "The forward direction belongs to another voxel snapshot.");
        if (!string.Equals(compositionHash, orientation.CompositionHash, StringComparison.OrdinalIgnoreCase))
            return Failure(Ra2VoxelFormZoneProjectionFailureKind.CompositionMismatch,
                "The forward direction belongs to another semantic composition.");
        if (snapshot.OccupancyCount > Ra2VoxelSceneSnapshot.MaximumOccupancyCount)
            return Failure(Ra2VoxelFormZoneProjectionFailureKind.ResourceLimitExceeded,
                "The voxel snapshot exceeds the form-zone resource limit.");

        try
        {
            ushort[] zones = new ushort[snapshot.OccupancyCount];
            int minimumZ = snapshot.Cells.Min(value => value.Coordinate.Z);
            int maximumZ = snapshot.Cells.Max(value => value.Coordinate.Z);
            int heightSpan = Math.Max(1, maximumZ - minimumZ);
            bool longitudinalAxisIsY = orientation.Direction switch
            {
                Ra2VoxelForwardDirection.PositiveX or Ra2VoxelForwardDirection.NegativeX => false,
                Ra2VoxelForwardDirection.PositiveY or Ra2VoxelForwardDirection.NegativeY => true,
                _ => snapshot.Part.YSize >= snapshot.Part.XSize
            };

            for (int index = 0; index < snapshot.Cells.Count; index++)
            {
                if ((index & 4095) == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                Ra2VoxelCoordinate coordinate = snapshot.Cells[index].Coordinate;
                bool positiveX = Exposed(Ra2VoxelFaceDirection.PositiveX);
                bool negativeX = Exposed(Ra2VoxelFaceDirection.NegativeX);
                bool positiveY = Exposed(Ra2VoxelFaceDirection.PositiveY);
                bool negativeY = Exposed(Ra2VoxelFaceDirection.NegativeY);
                bool top = Exposed(Ra2VoxelFaceDirection.PositiveZ);
                bool under = Exposed(Ra2VoxelFaceDirection.NegativeZ);
                bool xSurface = positiveX || negativeX;
                bool ySurface = positiveY || negativeY;
                bool horizontalSurface = xSurface || ySurface;
                bool surface = horizontalSurface || top || under;
                Ra2VoxelFormZone value = Ra2VoxelFormZone.None;
                if (!surface)
                {
                    value = Ra2VoxelFormZone.Interior;
                }
                else
                {
                    if (top)
                        value |= Ra2VoxelFormZone.UpperPlane;
                    if (top && horizontalSurface)
                        value |= Ra2VoxelFormZone.UpperBevel | Ra2VoxelFormZone.SilhouetteRidge;

                    bool lateralSurface = longitudinalAxisIsY ? xSurface : ySurface;
                    bool longitudinalSurface = longitudinalAxisIsY ? ySurface : xSurface;
                    if (lateralSurface)
                    {
                        double normalizedHeight = (coordinate.Z - minimumZ) / (double)heightSpan;
                        value |= normalizedHeight <= 0.34d
                            ? Ra2VoxelFormZone.LowerSkirt
                            : normalizedHeight >= 0.68d
                                ? Ra2VoxelFormZone.SideShoulder
                                : Ra2VoxelFormZone.SideField;
                    }
                    if (longitudinalSurface)
                        value |= DirectionalEnd(positiveX, negativeX, positiveY, negativeY,
                            orientation.Direction);
                    if ((value & ~(Ra2VoxelFormZone.UpperPlane | Ra2VoxelFormZone.SilhouetteRidge)) == 0 &&
                        !top)
                    {
                        value |= Ra2VoxelFormZone.UnclassifiedSurface;
                    }
                }
                zones[index] = (ushort)value;

                bool Exposed(Ra2VoxelFaceDirection direction) =>
                    Ra2VoxelNeighbourhood.IsFaceExposed(snapshot, coordinate, direction);
            }

            Ra2VoxelFormZone[] countable = Enum.GetValues<Ra2VoxelFormZone>()
                .Where(value => value != Ra2VoxelFormZone.None && IsSingleBit((ushort)value))
                .ToArray();
            Ra2VoxelFormZoneCount[] counts = countable
                .Select(zone => new Ra2VoxelFormZoneCount(zone,
                    zones.Count(value => (((Ra2VoxelFormZone)value) & zone) != 0)))
                .ToArray();
            List<string> diagnostics = [];
            if (!orientation.IsConfirmed)
                diagnostics.Add("ForwardDirectionNotConfirmed");
            return new(Ra2VoxelFormZoneProjectionFailureKind.None, string.Empty,
                new(snapshot.CanonicalHash, orientation.SelectionHash, adaptation.PolicyHash,
                    zones, counts, diagnostics));
        }
        catch (OperationCanceledException)
        {
            return Failure(Ra2VoxelFormZoneProjectionFailureKind.Cancelled,
                "Form-zone projection was cancelled.");
        }
    }

    private static Ra2VoxelFormZone DirectionalEnd(
        bool positiveX,
        bool negativeX,
        bool positiveY,
        bool negativeY,
        Ra2VoxelForwardDirection direction) => direction switch
    {
        Ra2VoxelForwardDirection.PositiveX =>
            (positiveX ? Ra2VoxelFormZone.FrontEnd : Ra2VoxelFormZone.None) |
            (negativeX ? Ra2VoxelFormZone.RearEnd : Ra2VoxelFormZone.None),
        Ra2VoxelForwardDirection.NegativeX =>
            (negativeX ? Ra2VoxelFormZone.FrontEnd : Ra2VoxelFormZone.None) |
            (positiveX ? Ra2VoxelFormZone.RearEnd : Ra2VoxelFormZone.None),
        Ra2VoxelForwardDirection.PositiveY =>
            (positiveY ? Ra2VoxelFormZone.FrontEnd : Ra2VoxelFormZone.None) |
            (negativeY ? Ra2VoxelFormZone.RearEnd : Ra2VoxelFormZone.None),
        Ra2VoxelForwardDirection.NegativeY =>
            (negativeY ? Ra2VoxelFormZone.FrontEnd : Ra2VoxelFormZone.None) |
            (positiveY ? Ra2VoxelFormZone.RearEnd : Ra2VoxelFormZone.None),
        _ => Ra2VoxelFormZone.LongitudinalEndUnknown
    };

    private static bool IsSingleBit(ushort value) => value != 0 && (value & (value - 1)) == 0;

    private static Ra2VoxelFormZoneProjectionResult Failure(
        Ra2VoxelFormZoneProjectionFailureKind kind,
        string message) => new(kind, message, null);
}
