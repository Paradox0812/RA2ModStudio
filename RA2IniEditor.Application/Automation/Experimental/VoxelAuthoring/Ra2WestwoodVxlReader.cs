using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

/// <summary>
/// Bounded Westwood VXL span decoder migrated from the user-authorized VoxelNormalForge core.
/// It deliberately exposes no VXL writer in Stage 1B.
/// </summary>
internal static class Ra2WestwoodVxlReader
{
    internal const int MaximumSectionCount = 16;
    private const int HeaderLength = 32;
    private const int PaletteLength = 770;
    private const int SectionHeaderLength = 28;
    private const int SectionInfoLength = 92;
    private const string Signature = "Voxel Animation";

    internal static IReadOnlyList<Ra2VoxelSceneSnapshot> Read(
        Stream source,
        string sceneId,
        string stableFileStem,
        Ra2VoxelAssemblyPartRole role,
        Ra2VoxelPaletteProfile palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        byte[] bytes = ReadBounded(source);
        using MemoryStream stream = new(bytes, writable: false);
        using BinaryReader reader = new(stream, Encoding.ASCII, leaveOpen: true);
        try
        {
            if (!string.Equals(ReadFixedAscii(reader, 16), Signature, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The VXL signature is not supported.");

            uint paletteCount = reader.ReadUInt32();
            uint sectionCount = reader.ReadUInt32();
            uint infoCount = reader.ReadUInt32();
            uint bodyLength = reader.ReadUInt32();
            if (paletteCount != 1)
                throw new InvalidDataException("Stage 1B supports exactly one embedded VXL palette.");
            if (sectionCount is < 1 or > MaximumSectionCount || infoCount != sectionCount)
                throw new InvalidDataException("The VXL Section count is outside the supported limit.");

            long sectionHeadersStart = checked(HeaderLength + (long)paletteCount * PaletteLength);
            long bodyStart = checked(sectionHeadersStart + sectionCount * SectionHeaderLength);
            long infoStart = checked(bodyStart + bodyLength);
            long requiredLength = checked(infoStart + infoCount * SectionInfoLength);
            if (requiredLength != stream.Length)
                throw new InvalidDataException("The VXL declared body and Section information do not match the input length.");

            byte remapStart = reader.ReadByte();
            byte remapEnd = reader.ReadByte();
            byte[] unusedPaletteBytes = reader.ReadBytes(Ra2VxlseSliceImportContract.WestwoodPaletteByteLength);
            if (unusedPaletteBytes.Length != Ra2VxlseSliceImportContract.WestwoodPaletteByteLength)
                throw new InvalidDataException("The VXL reserved palette block is truncated.");

            List<SectionHeader> headers = ReadHeaders(reader, checked((int)sectionCount));
            stream.Position = infoStart;
            SectionInfo[] infos = new SectionInfo[checked((int)infoCount)];
            for (int index = 0; index < infos.Length; index++)
                infos[index] = ReadInfo(reader, bodyLength);

            byte[] remapIndices = remapStart <= remapEnd
                ? Enumerable.Range(remapStart, remapEnd - remapStart + 1).Select(index => checked((byte)index)).ToArray()
                : [];
            var vxlPalette = new Ra2VoxelPaletteProfile(
                palette.ProfileId,
                palette.Colours,
                palette.TransparentIndices,
                remapIndices);
            string sourceHash = Convert.ToHexString(SHA256.HashData(bytes));

            List<Ra2VoxelSceneSnapshot> snapshots = new(headers.Count);
            foreach (SectionHeader header in headers)
            {
                if (header.InfoIndex >= infos.Length)
                    throw new InvalidDataException($"VXL Section '{header.Name}' has an invalid info index.");
                SectionInfo info = infos[header.InfoIndex];
                List<Ra2VoxelCell> cells = ReadSectionCells(reader, stream, bodyStart, bodyLength, header.Name, info);
                var descriptor = new Ra2VoxelPartDescriptor(
                    partId: headers.Count == 1 ? stableFileStem : ComposeIdentity(stableFileStem, header.Name),
                    role,
                    header.Name,
                    stableFileStem,
                    info.XSize,
                    info.YSize,
                    info.ZSize,
                    info.Scale,
                    localTransform: info.Transform.Select(value => (double)value));
                snapshots.Add(new Ra2VoxelSceneSnapshot(
                    headers.Count == 1 ? sceneId : ComposeIdentity(sceneId, header.Name),
                    descriptor,
                    vxlPalette,
                    cells,
                    [new KeyValuePair<string, string>("source.vxl", sourceHash)]));
            }

            return Array.AsReadOnly(snapshots.ToArray());
        }
        catch (EndOfStreamException exception)
        {
            throw new InvalidDataException("The VXL input is truncated.", exception);
        }
        catch (OverflowException exception)
        {
            throw new InvalidDataException("The VXL declared size exceeds the supported limits.", exception);
        }
    }

    private static List<SectionHeader> ReadHeaders(BinaryReader reader, int count)
    {
        List<SectionHeader> headers = new(count);
        HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < count; index++)
        {
            string name = ReadFixedAscii(reader, 16);
            uint infoIndex = reader.ReadUInt32();
            _ = reader.ReadUInt32();
            _ = reader.ReadUInt32();
            if (string.IsNullOrWhiteSpace(name) || !names.Add(name))
                throw new InvalidDataException("The VXL contains an empty or duplicate Section name.");
            headers.Add(new SectionHeader(name, infoIndex));
        }
        return headers;
    }

    private static SectionInfo ReadInfo(BinaryReader reader, uint bodyLength)
    {
        uint spanStartOffset = reader.ReadUInt32();
        uint spanEndOffset = reader.ReadUInt32();
        uint spanDataOffset = reader.ReadUInt32();
        float scale = reader.ReadSingle();
        float[] transform = new float[12];
        for (int index = 0; index < transform.Length; index++)
            transform[index] = reader.ReadSingle();
        float minX = reader.ReadSingle();
        float minY = reader.ReadSingle();
        float minZ = reader.ReadSingle();
        float maxX = reader.ReadSingle();
        float maxY = reader.ReadSingle();
        float maxZ = reader.ReadSingle();
        byte xSize = reader.ReadByte();
        byte ySize = reader.ReadByte();
        byte zSize = reader.ReadByte();
        byte normalType = reader.ReadByte();

        if (!float.IsFinite(scale) || scale <= 0f || transform.Any(value => !float.IsFinite(value)) ||
            !AllFinite(minX, minY, minZ, maxX, maxY, maxZ) ||
            minX > maxX || minY > maxY || minZ > maxZ || xSize == 0 || ySize == 0 || zSize == 0 ||
            normalType is not (2 or 4))
        {
            throw new InvalidDataException("The VXL Section information contains invalid geometry metadata.");
        }

        int columns = checked(xSize * ySize);
        long tableBytes = checked((long)columns * sizeof(int));
        ValidateBodyRange(spanStartOffset, tableBytes, bodyLength, "span-start table");
        ValidateBodyRange(spanEndOffset, tableBytes, bodyLength, "span-end table");
        ValidateBodyRange(spanDataOffset, 0, bodyLength, "span data");
        return new SectionInfo(
            spanStartOffset,
            spanEndOffset,
            spanDataOffset,
            scale,
            transform,
            xSize,
            ySize,
            zSize,
            normalType);
    }

    private static List<Ra2VoxelCell> ReadSectionCells(
        BinaryReader reader,
        Stream stream,
        long bodyStart,
        uint bodyLength,
        string sectionName,
        SectionInfo info)
    {
        int columnCount = checked(info.XSize * info.YSize);
        int[] starts = ReadInt32Table(reader, stream, bodyStart + info.SpanStartOffset, columnCount);
        int[] ends = ReadInt32Table(reader, stream, bodyStart + info.SpanEndOffset, columnCount);
        List<Ra2VoxelCell> cells = new();

        for (int column = 0; column < columnCount; column++)
        {
            int start = starts[column];
            int end = ends[column];
            if (start == -1 && end == -1)
                continue;
            if (start < 0 || end < start)
                throw new InvalidDataException($"VXL Section '{sectionName}' contains an invalid span range.");

            long relativeStart = checked((long)info.SpanDataOffset + start);
            long relativeEndInclusive = checked((long)info.SpanDataOffset + end);
            ValidateBodyRange(relativeStart, checked(relativeEndInclusive - relativeStart + 1), bodyLength, "column span");
            long absoluteEndInclusive = checked(bodyStart + relativeEndInclusive);
            stream.Position = checked(bodyStart + relativeStart);
            int x = column % info.XSize;
            int y = info.YSize - 1 - (column / info.XSize);
            int z = 0;

            while (stream.Position <= absoluteEndInclusive)
            {
                if (absoluteEndInclusive - stream.Position + 1 < 3)
                    throw new InvalidDataException($"VXL Section '{sectionName}' contains a truncated span packet.");
                int skipCount = reader.ReadByte();
                int voxelCount = reader.ReadByte();
                z = checked(z + skipCount);
                if (z > info.ZSize || absoluteEndInclusive - stream.Position + 1 < checked(voxelCount * 2 + 1))
                    throw new InvalidDataException($"VXL Section '{sectionName}' span exceeds its Z or byte bounds.");

                for (int index = 0; index < voxelCount; index++, z++)
                {
                    if (z >= info.ZSize)
                        throw new InvalidDataException($"VXL Section '{sectionName}' span exceeds its Z dimension.");
                    byte colourIndex = reader.ReadByte();
                    _ = reader.ReadByte(); // normalIndex is validated as a byte but is not canonical cell data in 1B.
                    cells.Add(new Ra2VoxelCell(new Ra2VoxelCoordinate(x, y, z), colourIndex));
                    if (cells.Count > Ra2VoxelSceneSnapshot.MaximumOccupancyCount)
                        throw new InvalidDataException($"VXL Section '{sectionName}' occupancy exceeds the supported limit.");
                }

                int duplicateCount = reader.ReadByte();
                if (duplicateCount != voxelCount)
                    throw new InvalidDataException($"VXL Section '{sectionName}' span duplicate count is inconsistent.");
            }

            if (stream.Position != absoluteEndInclusive + 1)
                throw new InvalidDataException($"VXL Section '{sectionName}' span length is inconsistent.");
        }

        return cells;
    }

    private static int[] ReadInt32Table(BinaryReader reader, Stream stream, long offset, int count)
    {
        stream.Position = offset;
        int[] values = new int[count];
        for (int index = 0; index < count; index++)
            values[index] = reader.ReadInt32();
        return values;
    }

    private static byte[] ReadBounded(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead)
            throw new ArgumentException("Source stream must be readable.", nameof(source));
        if (source.CanSeek && source.Length - source.Position > Ra2VoxelBinaryProbe.MaximumContentBytes)
            throw new InvalidDataException("The VXL input exceeds the existing asset content limit.");

        using MemoryStream destination = new();
        byte[] buffer = new byte[81920];
        int read;
        while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (destination.Length + read > Ra2VoxelBinaryProbe.MaximumContentBytes)
                throw new InvalidDataException("The VXL input exceeds the existing asset content limit.");
            destination.Write(buffer, 0, read);
        }
        if (destination.Length < HeaderLength)
            throw new InvalidDataException("The VXL fixed header is truncated.");
        return destination.ToArray();
    }

    private static string ReadFixedAscii(BinaryReader reader, int length)
    {
        byte[] bytes = reader.ReadBytes(length);
        if (bytes.Length != length)
            throw new EndOfStreamException();
        int terminator = Array.IndexOf(bytes, (byte)0);
        return Encoding.ASCII.GetString(bytes, 0, terminator >= 0 ? terminator : bytes.Length).Trim();
    }

    private static void ValidateBodyRange(long offset, long length, uint bodyLength, string label)
    {
        if (offset < 0 || length < 0 || offset > bodyLength || checked(offset + length) > bodyLength)
            throw new InvalidDataException($"The VXL {label} lies outside the declared body.");
    }

    private static bool AllFinite(params float[] values) => values.All(float.IsFinite);

    private static string ComposeIdentity(string prefix, string suffix)
    {
        string normalizedPrefix = Ra2VoxelSceneSnapshot.ValidateIdentity(prefix, nameof(prefix));
        string combined = $"{normalizedPrefix}:{suffix}";
        if (combined.Length <= Ra2VoxelSceneSnapshot.MaximumIdentityLength)
            return combined;
        int prefixLength = Ra2VoxelSceneSnapshot.MaximumIdentityLength - suffix.Length - 1;
        return $"{normalizedPrefix[..prefixLength]}:{suffix}";
    }

    private sealed record SectionHeader(string Name, uint InfoIndex);

    private sealed record SectionInfo(
        uint SpanStartOffset,
        uint SpanEndOffset,
        uint SpanDataOffset,
        float Scale,
        float[] Transform,
        byte XSize,
        byte YSize,
        byte ZSize,
        byte NormalType);
}
