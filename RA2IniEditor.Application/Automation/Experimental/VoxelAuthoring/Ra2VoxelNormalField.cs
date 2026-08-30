using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VxlNormalPaletteKind
{
    RedAlert2 = 0,
    TiberianSun
}

/// <summary>
/// Immutable VXL normal direction palette migrated from the user-authorized VoxelNormalForge source.
/// RGB triplets encode unit vectors as component = channel * 2 / 255 - 1.
/// </summary>
internal sealed class Ra2VxlNormalPalette
{
    private const string TiberianSunEncoded =
        "1ZgkocodeosANnMXaTYZrVgP5lNDjPdVVsogGapAKjRFpxk++5Jo1tZePelkBGhkXgdimwOL9WKmdPucNNizCa6LJSeV3Ch87K+tu+qic9LfDGuyZBK7vynEynHlt7nieor+M5vhPkzhiELv";
    private const string RedAlert2Encoded =
        "wlEdkrfwtN07ifRNV/ZoHM5xDLZ2AH6FAmtvC1FmLyNZVxg/khc4JCZ2v3ASue5pfnAAclUHxt9Td+g3WOU8KdVXHG/NCaJeJjlFNz0thghS4MhXf6YGdwFs0IUdwp4XodAjhdUhaNUkTNArObsnHYovKnAiOlYcTjweXicpfSYjpkIW1mYlYHAE5XwzUIsJVVUOajwVj1cHrFwNnnQEsYkKoKEJkrwRVbwXQ3EP514+Q6QV4rFBz7Ep3ZktvMcqsLUWc78RYaUJbosBFqVBzS9BiD0SnCokuzYpsSA540JN+pBh75VG0Mo/luU2luDOOtVAKKQpNIsZFm45JFQxlQ20dBQ6ow9P1Uky6z5r/HJn68Jy1dtsqO5QaPNQRuhTJb4+AodlD0qeFFFKPyc9ZwlVmANswRtT81lZ/W+E/ox+4NGImflmevxmR/KAFrxYDIlICW1TGjhgSBNYfgSgyhh02Cpf8K1b45jLwcDXstfLa/KzK8PCIKrIL3niqF/zOLPcGozLLSeqbyHUjBvOs0Li2Dm7+36c+Kd5x+iCqfiAaf1+Kt2GFsaOBI6cBVmHET9+OxRzagKIiQCE3ip/91V391uY4cWo0dydif6AefyaO+iaJtOkA5t9BG2dHTGURhSnTQqLsQqFyRmS7UGL8o2z+Z2XvuGymPmbte+ZT+uuO9q5D52zDH21Hz60PybAYQmlnAWY3S6e9HdM8mqx7auwz8y+pOy1iPK1W/eWNuZsjYwBWAdvMhqPWhfAdw+8sxGlxyKx6kmq53rI27TG77iRsgxoeOTNWuDIRs3SLpfeEF60NDzOUi3WhDHkqxzBvzDM5FjD04fezKXdsK/qnpr4gaD6crnwhc/iSEbjSGbvXlTzbmv8jGT7mEbuz0nS12nZonz6upPuo8ngF7qtZ9HeVLzmCquWhIL+IVLIMlreZznnfEz0oSzawVvmvnfuVZH2VZH2VZH2VZH2";

    private static readonly Ra2VxlNormalPalette RedAlert2Instance =
        new(Ra2VxlNormalPaletteKind.RedAlert2, RedAlert2Encoded, expectedCount: 244);
    private static readonly Ra2VxlNormalPalette TiberianSunInstance =
        new(Ra2VxlNormalPaletteKind.TiberianSun, TiberianSunEncoded, expectedCount: 36);

    private readonly Vector3[] _directions;

    private Ra2VxlNormalPalette(Ra2VxlNormalPaletteKind kind, string encoded, int expectedCount)
    {
        Kind = kind;
        byte[] bytes = Convert.FromBase64String(encoded);
        if (bytes.Length != expectedCount * 3)
            throw new InvalidOperationException("The VXL normal palette has an invalid encoded length.");

        _directions = new Vector3[expectedCount];
        for (int index = 0; index < expectedCount; index++)
        {
            Vector3 decoded = new(
                bytes[index * 3] * (2f / 255f) - 1f,
                bytes[index * 3 + 1] * (2f / 255f) - 1f,
                bytes[index * 3 + 2] * (2f / 255f) - 1f);
            _directions[index] = NormalizeOr(decoded, Vector3.UnitZ);
        }
    }

    internal Ra2VxlNormalPaletteKind Kind { get; }
    internal int Count => _directions.Length;
    internal IReadOnlyList<Vector3> Directions => Array.AsReadOnly(_directions);
    internal Vector3 this[int index] => _directions[index];

    internal static Ra2VxlNormalPalette For(Ra2VxlNormalPaletteKind kind) => kind switch
    {
        Ra2VxlNormalPaletteKind.RedAlert2 => RedAlert2Instance,
        Ra2VxlNormalPaletteKind.TiberianSun => TiberianSunInstance,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    internal byte FindClosestIndex(Vector3 direction)
    {
        Vector3 normalized = NormalizeOr(direction, Vector3.UnitZ);
        int bestIndex = 0;
        float bestDot = float.NegativeInfinity;
        for (int index = 0; index < _directions.Length; index++)
        {
            float dot = Vector3.Dot(normalized, _directions[index]);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestIndex = index;
            }
        }
        return checked((byte)bestIndex);
    }

    internal static Vector3 NormalizeOr(Vector3 value, Vector3 fallback)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) || !float.IsFinite(value.Z) ||
            value.LengthSquared() <= 1e-12f)
        {
            return fallback;
        }
        return Vector3.Normalize(value);
    }
}

internal sealed class Ra2VoxelNormalBakeOptions
{
    internal Ra2VoxelNormalBakeOptions(int radius = 1, int smoothingIterations = 1)
    {
        if (radius is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(radius));
        if (smoothingIterations is < 0 or > 4)
            throw new ArgumentOutOfRangeException(nameof(smoothingIterations));
        Radius = radius;
        SmoothingIterations = smoothingIterations;
    }

    internal int Radius { get; }
    internal int SmoothingIterations { get; }
}

internal readonly record struct Ra2VoxelNormalSample(
    Ra2VoxelCoordinate Coordinate,
    Vector3 Direction,
    byte NormalIndex);

internal readonly record struct Ra2VoxelNormalBakeFacts(
    int OccupiedCellCount,
    int SurfaceSampleCount,
    int Radius,
    int SmoothingIterations);

internal sealed class Ra2VoxelNormalField
{
    private readonly Ra2VoxelNormalSample[] _samples;
    private readonly Dictionary<Ra2VoxelCoordinate, Ra2VoxelNormalSample> _lookup;

    internal Ra2VoxelNormalField(
        string sourceSnapshotHash,
        Ra2VxlNormalPaletteKind paletteKind,
        Ra2VoxelNormalBakeOptions options,
        IEnumerable<Ra2VoxelNormalSample> samples)
    {
        SourceSnapshotHash = RequireSha256(sourceSnapshotHash);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(samples);
        PaletteKind = paletteKind;
        Radius = options.Radius;
        SmoothingIterations = options.SmoothingIterations;
        _samples = samples
            .OrderBy(sample => sample.Coordinate.Z)
            .ThenBy(sample => sample.Coordinate.Y)
            .ThenBy(sample => sample.Coordinate.X)
            .ToArray();
        _lookup = new(_samples.Length);
        foreach (Ra2VoxelNormalSample sample in _samples)
        {
            if (!float.IsFinite(sample.Direction.X) || !float.IsFinite(sample.Direction.Y) ||
                !float.IsFinite(sample.Direction.Z) || sample.Direction.LengthSquared() <= 0.99f ||
                sample.Direction.LengthSquared() >= 1.01f)
            {
                throw new ArgumentException("Normal samples must contain finite unit directions.", nameof(samples));
            }
            if (!_lookup.TryAdd(sample.Coordinate, sample))
                throw new ArgumentException("Normal sample coordinates must be unique.", nameof(samples));
        }
        FieldHash = ComputeHash();
    }

    internal string SourceSnapshotHash { get; }
    internal Ra2VxlNormalPaletteKind PaletteKind { get; }
    internal int Radius { get; }
    internal int SmoothingIterations { get; }
    internal IReadOnlyList<Ra2VoxelNormalSample> Samples => Array.AsReadOnly(_samples);
    internal string FieldHash { get; }

    internal bool TryGetSample(Ra2VoxelCoordinate coordinate, out Ra2VoxelNormalSample sample) =>
        _lookup.TryGetValue(coordinate, out sample);

    private string ComputeHash()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)1);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, SourceSnapshotHash);
        writer.Write((int)PaletteKind);
        writer.Write(Radius);
        writer.Write(SmoothingIterations);
        writer.Write(_samples.Length);
        foreach (Ra2VoxelNormalSample sample in _samples)
        {
            writer.Write(sample.Coordinate.X);
            writer.Write(sample.Coordinate.Y);
            writer.Write(sample.Coordinate.Z);
            writer.Write(sample.NormalIndex);
        }
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    private static string RequireSha256(string value)
    {
        if (value.Length != 64 || value.Any(character => !char.IsAsciiHexDigit(character)))
            throw new ArgumentException("A canonical source snapshot hash is required.", nameof(value));
        return value.ToUpperInvariant();
    }
}

internal enum Ra2VoxelNormalBakeFailureKind
{
    None = 0,
    ResourceLimitExceeded,
    Cancelled
}

internal sealed record Ra2VoxelNormalBakeResult(
    Ra2VoxelNormalBakeFailureKind FailureKind,
    string Message,
    Ra2VoxelNormalField? Field,
    Ra2VoxelNormalBakeFacts? Facts)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelNormalBakeFailureKind.None && Field is not null && Facts is not null;
}

/// <summary>
/// Computes a reviewable normal field from canonical occupancy without mutating or serializing the source snapshot.
/// It supports both VOX and VXL because format-specific input has already converged on the canonical snapshot.
/// </summary>
internal static class Ra2VoxelNormalBaker
{
    internal const int DefaultMaximumSampleCount = 250_000;
    internal const int MaximumSampleCount = 1_000_000;

    internal static Ra2VoxelNormalBakeResult Bake(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VxlNormalPaletteKind paletteKind = Ra2VxlNormalPaletteKind.RedAlert2,
        Ra2VoxelNormalBakeOptions? options = null,
        int maximumSampleCount = DefaultMaximumSampleCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (maximumSampleCount is < 1 or > MaximumSampleCount)
            throw new ArgumentOutOfRangeException(nameof(maximumSampleCount));
        options ??= new();

        try
        {
            Dictionary<Ra2VoxelCoordinate, Vector3> normals = new();
            for (int index = 0; index < snapshot.Cells.Count; index++)
            {
                if ((index & 4095) == 0)
                    cancellationToken.ThrowIfCancellationRequested();

                Ra2VoxelCoordinate coordinate = snapshot.Cells[index].Coordinate;
                if (!Ra2VoxelNeighbourhood.IsSurfaceCell(snapshot, coordinate))
                    continue;
                if (normals.Count == maximumSampleCount)
                {
                    return new(
                        Ra2VoxelNormalBakeFailureKind.ResourceLimitExceeded,
                        $"Voxel normal field exceeds the {maximumSampleCount:N0}-sample review limit.",
                        null,
                        null);
                }
                normals.Add(coordinate, EstimateNormal(snapshot, coordinate, options.Radius));
            }

            for (int iteration = 0; iteration < options.SmoothingIterations; iteration++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                normals = Smooth(normals, cancellationToken);
            }

            Ra2VxlNormalPalette palette = Ra2VxlNormalPalette.For(paletteKind);
            Ra2VoxelNormalSample[] samples = normals
                .OrderBy(pair => pair.Key.Z)
                .ThenBy(pair => pair.Key.Y)
                .ThenBy(pair => pair.Key.X)
                .Select(pair => new Ra2VoxelNormalSample(pair.Key, pair.Value, palette.FindClosestIndex(pair.Value)))
                .ToArray();
            Ra2VoxelNormalField field = new(snapshot.CanonicalHash, paletteKind, options, samples);
            return new(
                Ra2VoxelNormalBakeFailureKind.None,
                string.Empty,
                field,
                new(snapshot.OccupancyCount, samples.Length, options.Radius, options.SmoothingIterations));
        }
        catch (OperationCanceledException)
        {
            return new(Ra2VoxelNormalBakeFailureKind.Cancelled, "Voxel normal baking was cancelled.", null, null);
        }
    }

    private static Vector3 EstimateNormal(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelCoordinate coordinate,
        int radius)
    {
        Vector3 sum = Vector3.Zero;
        for (int dx = -radius; dx <= radius; dx++)
        for (int dy = -radius; dy <= radius; dy++)
        for (int dz = -radius; dz <= radius; dz++)
        {
            if (dx == 0 && dy == 0 && dz == 0)
                continue;
            int distanceSquared = checked(dx * dx + dy * dy + dz * dz);
            if (distanceSquared > radius * radius ||
                Ra2VoxelNeighbourhood.IsOccupied(
                    snapshot,
                    coordinate.X + dx,
                    coordinate.Y + dy,
                    coordinate.Z + dz))
            {
                continue;
            }

            Vector3 outward = Vector3.Normalize(new Vector3(dx, dy, dz));
            sum += outward / Math.Max(1f, distanceSquared);
        }
        return Ra2VxlNormalPalette.NormalizeOr(sum, FallbackAxisNormal(snapshot, coordinate));
    }

    private static Vector3 FallbackAxisNormal(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelCoordinate coordinate)
    {
        Vector3 sum = Vector3.Zero;
        foreach (Ra2VoxelFaceDirection direction in Ra2VoxelNeighbourhood.OrderedDirections)
        {
            if (!Ra2VoxelNeighbourhood.IsFaceExposed(snapshot, coordinate, direction))
                continue;
            (int x, int y, int z) = Ra2VoxelNeighbourhood.Offset(direction);
            sum += new Vector3(x, y, z);
        }
        return Ra2VxlNormalPalette.NormalizeOr(sum, Vector3.UnitZ);
    }

    private static Dictionary<Ra2VoxelCoordinate, Vector3> Smooth(
        IReadOnlyDictionary<Ra2VoxelCoordinate, Vector3> input,
        CancellationToken cancellationToken)
    {
        Dictionary<Ra2VoxelCoordinate, Vector3> output = new(input.Count);
        int index = 0;
        foreach ((Ra2VoxelCoordinate coordinate, Vector3 ownNormal) in input
                     .OrderBy(pair => pair.Key.Z)
                     .ThenBy(pair => pair.Key.Y)
                     .ThenBy(pair => pair.Key.X))
        {
            if ((index++ & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            Vector3 sum = ownNormal;
            int count = 1;
            foreach (Ra2VoxelFaceDirection direction in Ra2VoxelNeighbourhood.OrderedDirections)
            {
                (int dx, int dy, int dz) = Ra2VoxelNeighbourhood.Offset(direction);
                Ra2VoxelCoordinate neighbour = new(coordinate.X + dx, coordinate.Y + dy, coordinate.Z + dz);
                if (!input.TryGetValue(neighbour, out Vector3 neighbourNormal))
                    continue;
                sum += neighbourNormal;
                count++;
            }
            output.Add(coordinate, Ra2VxlNormalPalette.NormalizeOr(sum / count, ownNormal));
        }
        return output;
    }
}
