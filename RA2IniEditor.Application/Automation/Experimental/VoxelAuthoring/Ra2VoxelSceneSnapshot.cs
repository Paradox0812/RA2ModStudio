using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal readonly record struct Ra2Rgba32(byte Red, byte Green, byte Blue, byte Alpha = byte.MaxValue);

internal readonly record struct Ra2VoxelCell(
    Ra2VoxelCoordinate Coordinate,
    byte PaletteIndex);

internal readonly record struct Ra2VoxelVector3(double X, double Y, double Z)
{
    internal static Ra2VoxelVector3 Zero => new(0d, 0d, 0d);

    internal bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);
}

internal readonly record struct Ra2VoxelConnectivityFacts(
    int ComponentCount,
    int LargestComponentCellCount)
{
    internal bool IsSingleComponent => ComponentCount == 1;
}

internal readonly record struct Ra2VoxelSymmetryFacts(
    int MirroredCellPairCount,
    int UnmatchedCellCount)
{
    internal bool IsExactXSymmetric => UnmatchedCellCount == 0;
}

internal sealed class Ra2VoxelPaletteProfile
{
    internal const int ColourCount = 256;

    private readonly Ra2Rgba32[] _colours;
    private readonly byte[] _transparentIndices;
    private readonly byte[] _remapIndices;
    private readonly HashSet<byte> _transparentSet;
    private readonly HashSet<byte> _remapSet;

    internal Ra2VoxelPaletteProfile(
        string profileId,
        IEnumerable<Ra2Rgba32> colours,
        IEnumerable<byte>? transparentIndices = null,
        IEnumerable<byte>? remapIndices = null)
    {
        ProfileId = Ra2VoxelSceneSnapshot.ValidateIdentity(profileId, nameof(profileId));
        ArgumentNullException.ThrowIfNull(colours);
        _colours = colours.ToArray();
        if (_colours.Length != ColourCount)
            throw new ArgumentException("A voxel palette must contain exactly 256 colours.", nameof(colours));

        _transparentIndices = NormalizeIndices(transparentIndices ?? [0]);
        _remapIndices = NormalizeIndices(remapIndices ?? []);
        _transparentSet = _transparentIndices.ToHashSet();
        _remapSet = _remapIndices.ToHashSet();
        if (_remapIndices.Any(_transparentSet.Contains))
            throw new ArgumentException("Transparent and remap palette indices cannot overlap.", nameof(remapIndices));

        ProfileHash = ComputeProfileHash();
    }

    internal string ProfileId { get; }
    internal IReadOnlyList<Ra2Rgba32> Colours => Array.AsReadOnly(_colours);
    internal IReadOnlyList<byte> TransparentIndices => Array.AsReadOnly(_transparentIndices);
    internal IReadOnlyList<byte> RemapIndices => Array.AsReadOnly(_remapIndices);
    internal string ProfileHash { get; }

    internal Ra2Rgba32 this[byte index] => _colours[index];
    internal bool IsTransparent(byte index) => _transparentSet.Contains(index);
    internal bool IsRemap(byte index) => _remapSet.Contains(index);

    internal byte FindNearestOpaqueIndex(Ra2Rgba32 colour)
        => FindNearestIndex(colour, index => !_transparentSet.Contains(index));

    internal byte FindNearestOpaqueNonRemapIndex(Ra2Rgba32 colour)
        => FindNearestIndex(colour, index => !_transparentSet.Contains(index) && !_remapSet.Contains(index));

    internal byte FindNearestRemapIndex(Ra2Rgba32 colour)
        => FindNearestIndex(colour, _remapSet.Contains);

    private byte FindNearestIndex(Ra2Rgba32 colour, Func<byte, bool> predicate)
    {
        int selected = -1;
        long minimumDistance = long.MaxValue;
        for (int index = 0; index < _colours.Length; index++)
        {
            byte paletteIndex = checked((byte)index);
            if (!predicate(paletteIndex))
                continue;

            Ra2Rgba32 candidate = _colours[index];
            long red = colour.Red - candidate.Red;
            long green = colour.Green - candidate.Green;
            long blue = colour.Blue - candidate.Blue;
            long distance = (red * red) + (green * green) + (blue * blue);
            if (distance < minimumDistance)
            {
                minimumDistance = distance;
                selected = index;
            }
        }

        if (selected < 0)
            throw new InvalidOperationException("The palette does not contain an eligible colour entry.");

        return checked((byte)selected);
    }

    private string ComputeProfileHash()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)1);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, ProfileId);
        foreach (Ra2Rgba32 colour in _colours)
        {
            writer.Write(colour.Red);
            writer.Write(colour.Green);
            writer.Write(colour.Blue);
            writer.Write(colour.Alpha);
        }
        WriteIndices(writer, _transparentIndices);
        WriteIndices(writer, _remapIndices);
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    private static byte[] NormalizeIndices(IEnumerable<byte> indices) => indices
        .Distinct()
        .OrderBy(index => index)
        .ToArray();

    private static void WriteIndices(BinaryWriter writer, byte[] indices)
    {
        writer.Write(indices.Length);
        writer.Write(indices);
    }
}

internal sealed class Ra2VoxelPartDescriptor
{
    internal Ra2VoxelPartDescriptor(
        string partId,
        Ra2VoxelAssemblyPartRole role,
        string vxlSectionName,
        string stableFileStem,
        int xSize,
        int ySize,
        int zSize,
        double voxelUnitScale = 1d,
        Ra2VoxelVector3? origin = null,
        Ra2VoxelVector3? pivot = null,
        IEnumerable<double>? localTransform = null)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
        PartId = Ra2VoxelSceneSnapshot.ValidateIdentity(partId, nameof(partId));
        VxlSectionName = Ra2VoxelSceneSnapshot.ValidateIdentity(
            vxlSectionName,
            nameof(vxlSectionName),
            Ra2VoxelAssemblyPartSpec.MaximumSectionNameLength);
        StableFileStem = ValidateFileStem(stableFileStem);
        ValidateDimension(xSize, nameof(xSize));
        ValidateDimension(ySize, nameof(ySize));
        ValidateDimension(zSize, nameof(zSize));
        if (!double.IsFinite(voxelUnitScale) || voxelUnitScale <= 0d)
            throw new ArgumentOutOfRangeException(nameof(voxelUnitScale));

        Origin = origin ?? Ra2VoxelVector3.Zero;
        Pivot = pivot ?? Ra2VoxelVector3.Zero;
        if (!Origin.IsFinite || !Pivot.IsFinite)
            throw new ArgumentException("Voxel origin and pivot must be finite.");

        double[] transform = (localTransform ?? Identity4x3()).ToArray();
        if (transform.Length != 12 || transform.Any(value => !double.IsFinite(value)))
            throw new ArgumentException("A voxel local transform must contain 12 finite values.", nameof(localTransform));

        Role = role;
        XSize = xSize;
        YSize = ySize;
        ZSize = zSize;
        VoxelUnitScale = voxelUnitScale;
        LocalTransform = Array.AsReadOnly(transform);
    }

    internal string PartId { get; }
    internal Ra2VoxelAssemblyPartRole Role { get; }
    internal string VxlSectionName { get; }
    internal string StableFileStem { get; }
    internal int XSize { get; }
    internal int YSize { get; }
    internal int ZSize { get; }
    internal double VoxelUnitScale { get; }
    internal Ra2VoxelVector3 Origin { get; }
    internal Ra2VoxelVector3 Pivot { get; }
    internal IReadOnlyList<double> LocalTransform { get; }
    internal int MaximumCellCount => checked(XSize * YSize * ZSize);

    private static void ValidateDimension(int value, string parameterName)
    {
        if (value is < 1 or > Ra2VxlseSliceImportContract.MaximumVoxelDimension)
            throw new ArgumentOutOfRangeException(parameterName);
    }

    private static string ValidateFileStem(string value)
    {
        string normalized = Ra2VoxelSceneSnapshot.ValidateIdentity(value, nameof(value));
        if (normalized is "." or ".." || normalized.Contains('.') ||
            normalized.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Voxel file stem must be a simple extension-free file name.", nameof(value));
        }
        return normalized;
    }

    private static IEnumerable<double> Identity4x3()
    {
        double[] matrix = new double[12];
        matrix[0] = 1d;
        matrix[5] = 1d;
        matrix[10] = 1d;
        return matrix;
    }
}

internal sealed class Ra2VoxelSceneSnapshot
{
    internal const int CurrentSchemaVersion = 1;
    internal const int MaximumIdentityLength = 64;
    internal const int MaximumOccupancyCount = 1_000_000;

    private readonly Ra2VoxelCell[] _cells;
    private readonly KeyValuePair<string, string>[] _sourceArtifactHashes;
    private readonly Dictionary<Ra2VoxelCoordinate, byte> _cellLookup;

    internal Ra2VoxelSceneSnapshot(
        string sceneId,
        Ra2VoxelPartDescriptor part,
        Ra2VoxelPaletteProfile palette,
        IEnumerable<Ra2VoxelCell> cells,
        IEnumerable<KeyValuePair<string, string>>? sourceArtifactHashes = null)
    {
        SceneId = ValidateIdentity(sceneId, nameof(sceneId));
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(cells);

        int cellLimit = Math.Min(part.MaximumCellCount, MaximumOccupancyCount);
        _cells = cells
            .Take(cellLimit + 1)
            .OrderBy(cell => cell.Coordinate.Z)
            .ThenBy(cell => cell.Coordinate.Y)
            .ThenBy(cell => cell.Coordinate.X)
            .ToArray();
        if (_cells.Length > cellLimit)
            throw new ArgumentOutOfRangeException(nameof(cells));

        _cellLookup = new Dictionary<Ra2VoxelCoordinate, byte>(_cells.Length);
        foreach (Ra2VoxelCell cell in _cells)
        {
            ValidateCoordinate(cell.Coordinate, part);
            if (palette.IsTransparent(cell.PaletteIndex))
                throw new ArgumentException("Occupied cells cannot use a transparent palette index.", nameof(cells));
            if (!_cellLookup.TryAdd(cell.Coordinate, cell.PaletteIndex))
                throw new ArgumentException("Voxel cell coordinates must be unique.", nameof(cells));
        }

        _sourceArtifactHashes = NormalizeSourceHashes(sourceArtifactHashes ?? []);
        Part = part;
        Palette = palette;
        Connectivity = ComputeConnectivity(_cellLookup);
        Symmetry = ComputeXSymmetry(_cellLookup, part.XSize);
        CanonicalHash = ComputeCanonicalHash();
    }

    internal int SchemaVersion => CurrentSchemaVersion;
    internal string SceneId { get; }
    internal Ra2VoxelPartDescriptor Part { get; }
    internal Ra2VoxelPaletteProfile Palette { get; }
    internal IReadOnlyList<Ra2VoxelCell> Cells => Array.AsReadOnly(_cells);
    internal IReadOnlyList<KeyValuePair<string, string>> SourceArtifactHashes =>
        Array.AsReadOnly(_sourceArtifactHashes);
    internal int OccupancyCount => _cells.Length;
    internal Ra2VoxelConnectivityFacts Connectivity { get; }
    internal Ra2VoxelSymmetryFacts Symmetry { get; }
    internal string CanonicalHash { get; }

    internal bool TryGetPaletteIndex(Ra2VoxelCoordinate coordinate, out byte paletteIndex) =>
        _cellLookup.TryGetValue(coordinate, out paletteIndex);

    internal static string ValidateIdentity(
        string value,
        string parameterName,
        int maximumLength = MaximumIdentityLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Voxel identity cannot be empty.", parameterName);
        string normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("Voxel identity is invalid or exceeds its limit.", parameterName);
        return normalized;
    }

    internal static void WriteCanonicalString(BinaryWriter writer, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    private string ComputeCanonicalHash()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(CurrentSchemaVersion);
        WriteCanonicalString(writer, SceneId);
        WriteCanonicalString(writer, Part.PartId);
        writer.Write((int)Part.Role);
        WriteCanonicalString(writer, Part.VxlSectionName);
        WriteCanonicalString(writer, Part.StableFileStem);
        writer.Write(Part.XSize);
        writer.Write(Part.YSize);
        writer.Write(Part.ZSize);
        writer.Write(Part.VoxelUnitScale);
        WriteVector(writer, Part.Origin);
        WriteVector(writer, Part.Pivot);
        foreach (double value in Part.LocalTransform)
            writer.Write(value);
        WriteCanonicalString(writer, Palette.ProfileHash);
        writer.Write(_sourceArtifactHashes.Length);
        foreach ((string name, string hash) in _sourceArtifactHashes)
        {
            WriteCanonicalString(writer, name);
            WriteCanonicalString(writer, hash);
        }
        writer.Write(_cells.Length);
        foreach (Ra2VoxelCell cell in _cells)
        {
            writer.Write(cell.Coordinate.X);
            writer.Write(cell.Coordinate.Y);
            writer.Write(cell.Coordinate.Z);
            writer.Write(cell.PaletteIndex);
        }
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    private static void WriteVector(BinaryWriter writer, Ra2VoxelVector3 vector)
    {
        writer.Write(vector.X);
        writer.Write(vector.Y);
        writer.Write(vector.Z);
    }

    private static KeyValuePair<string, string>[] NormalizeSourceHashes(
        IEnumerable<KeyValuePair<string, string>> sourceArtifactHashes)
    {
        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach ((string rawName, string rawHash) in sourceArtifactHashes)
        {
            string name = ValidateIdentity(rawName, nameof(sourceArtifactHashes));
            string hash = rawHash?.Trim().ToUpperInvariant() ?? string.Empty;
            if (hash.Length != 64 || hash.Any(character => !Uri.IsHexDigit(character)))
                throw new ArgumentException("Source artifact hashes must be SHA-256 hex strings.", nameof(sourceArtifactHashes));
            if (!normalized.TryAdd(name, hash))
                throw new ArgumentException("Source artifact names must be unique.", nameof(sourceArtifactHashes));
        }

        return normalized
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .ToArray();
    }

    private static void ValidateCoordinate(Ra2VoxelCoordinate coordinate, Ra2VoxelPartDescriptor part)
    {
        if (coordinate.X < 0 || coordinate.X >= part.XSize ||
            coordinate.Y < 0 || coordinate.Y >= part.YSize ||
            coordinate.Z < 0 || coordinate.Z >= part.ZSize)
        {
            throw new ArgumentOutOfRangeException(nameof(coordinate));
        }
    }

    private static Ra2VoxelConnectivityFacts ComputeConnectivity(
        IReadOnlyDictionary<Ra2VoxelCoordinate, byte> cells)
    {
        if (cells.Count == 0)
            return new Ra2VoxelConnectivityFacts(0, 0);

        HashSet<Ra2VoxelCoordinate> remaining = cells.Keys.ToHashSet();
        int componentCount = 0;
        int largest = 0;
        Queue<Ra2VoxelCoordinate> queue = new();
        while (remaining.Count > 0)
        {
            Ra2VoxelCoordinate seed = remaining.First();
            remaining.Remove(seed);
            queue.Enqueue(seed);
            int size = 0;
            while (queue.Count > 0)
            {
                Ra2VoxelCoordinate current = queue.Dequeue();
                size++;
                foreach (Ra2VoxelCoordinate neighbor in EnumerateFaceNeighbors(current))
                {
                    if (remaining.Remove(neighbor))
                        queue.Enqueue(neighbor);
                }
            }
            componentCount++;
            largest = Math.Max(largest, size);
        }

        return new Ra2VoxelConnectivityFacts(componentCount, largest);
    }

    private static IEnumerable<Ra2VoxelCoordinate> EnumerateFaceNeighbors(Ra2VoxelCoordinate value)
    {
        yield return value with { X = value.X - 1 };
        yield return value with { X = value.X + 1 };
        yield return value with { Y = value.Y - 1 };
        yield return value with { Y = value.Y + 1 };
        yield return value with { Z = value.Z - 1 };
        yield return value with { Z = value.Z + 1 };
    }

    private static Ra2VoxelSymmetryFacts ComputeXSymmetry(
        IReadOnlyDictionary<Ra2VoxelCoordinate, byte> cells,
        int xSize)
    {
        int mirrored = 0;
        int unmatched = 0;
        foreach ((Ra2VoxelCoordinate coordinate, byte paletteIndex) in cells)
        {
            Ra2VoxelCoordinate counterpart = coordinate with { X = xSize - 1 - coordinate.X };
            if (cells.TryGetValue(counterpart, out byte counterpartPalette) && counterpartPalette == paletteIndex)
                mirrored++;
            else
                unmatched++;
        }
        return new Ra2VoxelSymmetryFacts(mirrored, unmatched);
    }
}
