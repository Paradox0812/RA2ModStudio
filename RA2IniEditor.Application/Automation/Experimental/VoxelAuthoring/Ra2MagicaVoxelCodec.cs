using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

/// <summary>
/// Restricted MagicaVoxel VOX v150 exchange codec. It intentionally supports one model and
/// an explicit palette; scene graph, materials and animation remain outside Stage 1B.
/// </summary>
internal static class Ra2MagicaVoxelCodec
{
    internal const int MaximumEncodedByteLength = 256 * 1024 * 1024;
    internal const int MaximumChunkCount = 4096;
    private const int Version = 150;

    internal static Ra2VoxelSceneSnapshot Read(
        Stream source,
        string sceneId,
        string partId,
        Ra2VoxelAssemblyPartRole role,
        string vxlSectionName,
        string stableFileStem)
    {
        byte[] bytes = ReadBounded(source, MaximumEncodedByteLength);
        using MemoryStream stream = new(bytes, writable: false);
        using BinaryReader reader = new(stream, Encoding.ASCII, leaveOpen: true);

        if (ReadFourCc(reader) != "VOX ")
            throw new InvalidDataException("MagicaVoxel signature is invalid.");
        int version = ReadInt32(reader);
        if (version < Version)
            throw new InvalidDataException($"MagicaVoxel version {version} is not supported.");

        ChunkHeader main = ReadChunkHeader(reader, stream.Length);
        if (main.Id != "MAIN" || main.ContentLength != 0 || main.EndOffset != stream.Length)
            throw new InvalidDataException("MagicaVoxel MAIN chunk is invalid or is not the sole root chunk.");

        (int X, int Y, int Z)? dimensions = null;
        List<Ra2VoxelCell>? cells = null;
        Ra2Rgba32[]? colours = null;
        int chunkCount = 0;
        while (stream.Position < main.EndOffset)
        {
            if (++chunkCount > MaximumChunkCount)
                throw new InvalidDataException("MagicaVoxel chunk count exceeds the supported limit.");

            ChunkHeader chunk = ReadChunkHeader(reader, main.EndOffset);
            switch (chunk.Id)
            {
                case "PACK":
                    RequireChunkShape(chunk, sizeof(int), 0);
                    if (ReadInt32(reader) != 1)
                        throw new InvalidDataException("Only one MagicaVoxel model is supported.");
                    break;
                case "SIZE":
                    RequireChunkShape(chunk, 3 * sizeof(int), 0);
                    if (dimensions is not null || cells is not null)
                        throw new InvalidDataException("Only one SIZE/XYZI model pair is supported.");
                    dimensions = (ReadDimension(reader), ReadDimension(reader), ReadDimension(reader));
                    break;
                case "XYZI":
                    if (dimensions is null || cells is not null || chunk.ChildrenLength != 0)
                        throw new InvalidDataException("XYZI must follow exactly one SIZE chunk.");
                    int voxelCount = ReadInt32(reader);
                    int maximumCount = checked(dimensions.Value.X * dimensions.Value.Y * dimensions.Value.Z);
                    if (voxelCount < 0 || voxelCount > maximumCount ||
                        voxelCount > Ra2VoxelSceneSnapshot.MaximumOccupancyCount ||
                        chunk.ContentLength != sizeof(int) + checked(voxelCount * 4))
                    {
                        throw new InvalidDataException("MagicaVoxel voxel count or XYZI size is invalid.");
                    }
                    cells = ReadCells(reader, voxelCount, dimensions.Value);
                    break;
                case "RGBA":
                    RequireChunkShape(chunk, Ra2VoxelPaletteProfile.ColourCount * 4, 0);
                    if (colours is not null)
                        throw new InvalidDataException("MagicaVoxel contains more than one RGBA palette.");
                    colours = ReadPalette(reader);
                    break;
                default:
                    stream.Position = chunk.EndOffset;
                    continue;
            }

            if (stream.Position != chunk.ContentEndOffset)
                throw new InvalidDataException($"MagicaVoxel {chunk.Id} content length is inconsistent.");
            stream.Position = chunk.EndOffset;
        }

        if (dimensions is null || cells is null || colours is null)
            throw new InvalidDataException("MagicaVoxel requires one SIZE/XYZI model and an explicit RGBA palette.");

        var descriptor = new Ra2VoxelPartDescriptor(
            partId,
            role,
            vxlSectionName,
            stableFileStem,
            dimensions.Value.X,
            dimensions.Value.Y,
            dimensions.Value.Z);
        byte[] transparentIndices = colours
            .Select((colour, index) => (colour, index))
            .Where(item => item.index == 0 || item.colour.Alpha == 0)
            .Select(item => checked((byte)item.index))
            .ToArray();
        var palette = new Ra2VoxelPaletteProfile("magica-vox-rgba", colours, transparentIndices);
        return new Ra2VoxelSceneSnapshot(
            sceneId,
            descriptor,
            palette,
            cells,
            [new KeyValuePair<string, string>("source.vox", Convert.ToHexString(SHA256.HashData(bytes)))]);
    }

    internal static byte[] Write(Ra2VoxelSceneSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.Cells.Any(cell => cell.PaletteIndex == 0))
            throw new InvalidDataException("MagicaVoxel colour index 0 is reserved for empty space.");

        byte[] size = BuildContent(writer =>
        {
            writer.Write(snapshot.Part.XSize);
            writer.Write(snapshot.Part.YSize);
            writer.Write(snapshot.Part.ZSize);
        });
        byte[] xyzi = BuildContent(writer =>
        {
            writer.Write(snapshot.Cells.Count);
            foreach (Ra2VoxelCell cell in snapshot.Cells)
            {
                writer.Write(checked((byte)cell.Coordinate.X));
                writer.Write(checked((byte)cell.Coordinate.Y));
                writer.Write(checked((byte)cell.Coordinate.Z));
                writer.Write(cell.PaletteIndex);
            }
        });
        byte[] rgba = BuildContent(writer =>
        {
            for (int magicaIndex = 1; magicaIndex <= byte.MaxValue; magicaIndex++)
                WriteColour(writer, snapshot.Palette[checked((byte)magicaIndex)]);
            WriteColour(writer, snapshot.Palette[0]);
        });

        using MemoryStream children = new();
        using (BinaryWriter childWriter = new(children, Encoding.ASCII, leaveOpen: true))
        {
            WriteChunk(childWriter, "SIZE", size);
            WriteChunk(childWriter, "XYZI", xyzi);
            WriteChunk(childWriter, "RGBA", rgba);
        }

        using MemoryStream output = new();
        using BinaryWriter writer = new(output, Encoding.ASCII, leaveOpen: true);
        WriteFourCc(writer, "VOX ");
        writer.Write(Version);
        WriteChunk(writer, "MAIN", [], children.ToArray());
        writer.Flush();
        return output.ToArray();
    }

    private static List<Ra2VoxelCell> ReadCells(
        BinaryReader reader,
        int count,
        (int X, int Y, int Z) dimensions)
    {
        List<Ra2VoxelCell> cells = new(count);
        HashSet<Ra2VoxelCoordinate> coordinates = new();
        for (int index = 0; index < count; index++)
        {
            var coordinate = new Ra2VoxelCoordinate(
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadByte());
            byte paletteIndex = reader.ReadByte();
            if (coordinate.X >= dimensions.X || coordinate.Y >= dimensions.Y || coordinate.Z >= dimensions.Z)
                throw new InvalidDataException("MagicaVoxel cell lies outside SIZE bounds.");
            if (paletteIndex == 0)
                throw new InvalidDataException("MagicaVoxel occupied cells cannot use colour index 0.");
            if (!coordinates.Add(coordinate))
                throw new InvalidDataException("MagicaVoxel contains duplicate voxel coordinates.");
            cells.Add(new Ra2VoxelCell(coordinate, paletteIndex));
        }
        return cells;
    }

    private static Ra2Rgba32[] ReadPalette(BinaryReader reader)
    {
        Ra2Rgba32[] colours = new Ra2Rgba32[Ra2VoxelPaletteProfile.ColourCount];
        for (int magicaIndex = 1; magicaIndex <= byte.MaxValue; magicaIndex++)
            colours[magicaIndex] = ReadColour(reader);
        colours[0] = ReadColour(reader) with { Alpha = 0 };
        return colours;
    }

    private static Ra2Rgba32 ReadColour(BinaryReader reader) => new(
        reader.ReadByte(),
        reader.ReadByte(),
        reader.ReadByte(),
        reader.ReadByte());

    private static void WriteColour(BinaryWriter writer, Ra2Rgba32 colour)
    {
        writer.Write(colour.Red);
        writer.Write(colour.Green);
        writer.Write(colour.Blue);
        writer.Write(colour.Alpha);
    }

    private static int ReadDimension(BinaryReader reader)
    {
        int value = ReadInt32(reader);
        if (value is < 1 or > Ra2VxlseSliceImportContract.MaximumVoxelDimension)
            throw new InvalidDataException("MagicaVoxel dimensions must be in the 1..255 range.");
        return value;
    }

    private static int ReadInt32(BinaryReader reader)
    {
        try
        {
            return reader.ReadInt32();
        }
        catch (EndOfStreamException exception)
        {
            throw new InvalidDataException("MagicaVoxel input is truncated.", exception);
        }
    }

    private static ChunkHeader ReadChunkHeader(BinaryReader reader, long parentEndOffset)
    {
        Stream stream = reader.BaseStream;
        if (parentEndOffset - stream.Position < 12)
            throw new InvalidDataException("MagicaVoxel chunk header is truncated.");
        string id = ReadFourCc(reader);
        int contentLength = ReadInt32(reader);
        int childrenLength = ReadInt32(reader);
        if (contentLength < 0 || childrenLength < 0)
            throw new InvalidDataException("MagicaVoxel chunk lengths cannot be negative.");
        long contentEnd = checked(stream.Position + contentLength);
        long end = checked(contentEnd + childrenLength);
        if (end > parentEndOffset)
            throw new InvalidDataException("MagicaVoxel chunk exceeds its parent bounds.");
        return new ChunkHeader(id, contentLength, childrenLength, contentEnd, end);
    }

    private static void RequireChunkShape(ChunkHeader chunk, int contentLength, int childrenLength)
    {
        if (chunk.ContentLength != contentLength || chunk.ChildrenLength != childrenLength)
            throw new InvalidDataException($"MagicaVoxel {chunk.Id} chunk shape is invalid.");
    }

    private static string ReadFourCc(BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(4);
        if (bytes.Length != 4)
            throw new InvalidDataException("MagicaVoxel FourCC is truncated.");
        return Encoding.ASCII.GetString(bytes);
    }

    private static void WriteFourCc(BinaryWriter writer, string value)
    {
        if (value.Length != 4)
            throw new ArgumentException("A FourCC must contain four ASCII characters.", nameof(value));
        writer.Write(Encoding.ASCII.GetBytes(value));
    }

    private static byte[] BuildContent(Action<BinaryWriter> write)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.ASCII, leaveOpen: true);
        write(writer);
        writer.Flush();
        return stream.ToArray();
    }

    private static void WriteChunk(
        BinaryWriter writer,
        string id,
        ReadOnlySpan<byte> content,
        ReadOnlySpan<byte> children = default)
    {
        WriteFourCc(writer, id);
        writer.Write(content.Length);
        writer.Write(children.Length);
        writer.Write(content);
        writer.Write(children);
    }

    private static byte[] ReadBounded(Stream source, int limit)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead)
            throw new ArgumentException("Source stream must be readable.", nameof(source));
        if (source.CanSeek && source.Length - source.Position > limit)
            throw new InvalidDataException("MagicaVoxel input exceeds the supported size limit.");

        using MemoryStream destination = new();
        byte[] buffer = new byte[81920];
        int read;
        while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (destination.Length + read > limit)
                throw new InvalidDataException("MagicaVoxel input exceeds the supported size limit.");
            destination.Write(buffer, 0, read);
        }
        return destination.ToArray();
    }

    private readonly record struct ChunkHeader(
        string Id,
        int ContentLength,
        int ChildrenLength,
        long ContentEndOffset,
        long EndOffset);
}
