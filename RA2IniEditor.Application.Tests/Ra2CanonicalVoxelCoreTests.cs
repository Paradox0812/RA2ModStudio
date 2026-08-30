using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2CanonicalVoxelCoreTests
{
    [Fact]
    public void Snapshot_CanonicalizesOrderCopiesInputsAndComputesGeometryFacts()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2VoxelPartDescriptor part = CreatePart();
        Ra2VoxelCell[] cells =
        [
            new(new Ra2VoxelCoordinate(2, 0, 1), 1),
            new(new Ra2VoxelCoordinate(0, 3, 4), 2),
            new(new Ra2VoxelCoordinate(1, 2, 3), 3)
        ];
        var sourceHashes = new[]
        {
            new KeyValuePair<string, string>("z.vox", new string('B', 64)),
            new KeyValuePair<string, string>("a.png", new string('A', 64))
        };

        Ra2VoxelSceneSnapshot first = new("SCENE", part, palette, cells, sourceHashes);
        Ra2VoxelSceneSnapshot second = new("SCENE", part, palette, cells.Reverse(), sourceHashes.Reverse());
        cells[0] = new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 4);

        Assert.Equal(first.CanonicalHash, second.CanonicalHash);
        Assert.NotEqual(
            first.CanonicalHash,
            new Ra2VoxelSceneSnapshot(
                "SCENE",
                part,
                palette,
                first.Cells.Select(cell => cell.Coordinate == new Ra2VoxelCoordinate(2, 0, 1)
                    ? cell with { PaletteIndex = 4 }
                    : cell),
                sourceHashes).CanonicalHash);
        Assert.Equal([1, 3, 2], first.Cells.Select(cell => (int)cell.PaletteIndex));
        Assert.Equal(["a.png", "z.vox"], first.SourceArtifactHashes.Select(pair => pair.Key));
        Assert.Equal(3, first.Connectivity.ComponentCount);
        Assert.Equal(2, first.Symmetry.UnmatchedCellCount);
        Assert.False(first.TryGetPaletteIndex(new Ra2VoxelCoordinate(0, 0, 0), out _));
    }

    [Fact]
    public void Snapshot_RejectsTransparentDuplicateAndOutOfBoundsCells()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2VoxelPartDescriptor part = CreatePart();

        Assert.Throws<ArgumentException>(() => new Ra2VoxelSceneSnapshot(
            "SCENE", part, palette, [new(new Ra2VoxelCoordinate(0, 0, 0), 0)]));
        Assert.Throws<ArgumentException>(() => new Ra2VoxelSceneSnapshot(
            "SCENE", part, palette,
            [new(new Ra2VoxelCoordinate(0, 0, 0), 1), new(new Ra2VoxelCoordinate(0, 0, 0), 2)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2VoxelSceneSnapshot(
            "SCENE", part, palette, [new(new Ra2VoxelCoordinate(3, 0, 0), 1)]));
    }

    [Fact]
    public void Palette_UsesStableHashAndExcludesTransparentEntriesFromQuantization()
    {
        Ra2Rgba32[] colours = CreateColours();
        Ra2VoxelPaletteProfile first = new("test", colours, [0, 9], [16, 17]);
        Ra2VoxelPaletteProfile second = new("test", colours.ToArray(), [9, 0], [17, 16]);

        Assert.Equal(first.ProfileHash, second.ProfileHash);
        Assert.NotEqual((byte)9, first.FindNearestOpaqueIndex(colours[9]));
        Assert.Equal((byte)16, first.FindNearestOpaqueIndex(colours[16]));
    }

    [Fact]
    public void MagicaVoxel_WritesDeterministicallyAndRoundTripsOneModel()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot();

        byte[] first = Ra2MagicaVoxelCodec.Write(source);
        byte[] second = Ra2MagicaVoxelCodec.Write(source);
        using MemoryStream stream = new(first, writable: false);
        Ra2VoxelSceneSnapshot decoded = Ra2MagicaVoxelCodec.Read(
            stream, "DECODED", "body", Ra2VoxelAssemblyPartRole.Body, "Body", "test");

        Assert.Equal(first, second);
        Assert.Equal(first, Ra2MagicaVoxelCodec.Write(decoded));
        Assert.Equal(source.Cells, decoded.Cells);
        Assert.Equal(source.Part.XSize, decoded.Part.XSize);
        Assert.Equal(source.Palette[1], decoded.Palette[1]);
        Assert.Single(decoded.SourceArtifactHashes);
    }

    [Fact]
    public void MagicaVoxel_RejectsDuplicateTruncatedAndOversizedModels()
    {
        byte[] valid = Ra2MagicaVoxelCodec.Write(CreateSnapshot());
        byte[] duplicate = valid.ToArray();
        duplicate[64] = duplicate[60];
        duplicate[65] = duplicate[61];
        duplicate[66] = duplicate[62];
        byte[] oversized = valid.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(oversized.AsSpan(32, 4), 256);

        Assert.Throws<InvalidDataException>(() => ReadVox(duplicate));
        Assert.Throws<InvalidDataException>(() => ReadVox(valid[..^1]));
        Assert.Throws<InvalidDataException>(() => ReadVox(oversized));
    }

    [Theory]
    [InlineData((int)Ra2VxlseSliceDirection.Downward)]
    [InlineData((int)Ra2VxlseSliceDirection.Rightward)]
    public void SliceStack_RgbaAndPngRoundTripPreserveAsymmetricCells(int directionValue)
    {
        Ra2VxlseSliceDirection direction = (Ra2VxlseSliceDirection)directionValue;
        Ra2VoxelSceneSnapshot source = CreateSnapshot();

        Ra2VoxelSliceStackRaster raster = Ra2VoxelSliceStackCodec.Export(source, direction);
        byte[] firstPng = Ra2PngRgbaCodec.Encode(raster);
        byte[] secondPng = Ra2PngRgbaCodec.Encode(raster);
        Ra2VoxelSliceStackRaster decodedRaster = Ra2VoxelSliceStackCodec.ImportPng(
            firstPng,
            direction,
            source.Part.XSize,
            source.Part.YSize,
            source.Part.ZSize,
            source.CanonicalHash);
        Ra2VoxelSceneSnapshot decoded = Ra2VoxelSliceStackCodec.Import(
            decodedRaster,
            "ROUNDTRIP",
            source.Part,
            source.Palette);

        Assert.Equal(firstPng, secondPng);
        Assert.Equal(raster.RgbaBytes.ToArray(), decodedRaster.RgbaBytes.ToArray());
        Assert.Equal(source.Cells, decoded.Cells);
    }

    [Fact]
    public void PngDecoder_AcceptsAllStandardFiltersAndRejectsCrcDamage()
    {
        byte[] rgba = [10, 20, 30, 255, 40, 50, 60, 255, 70, 80, 90, 255, 100, 110, 120, 255];
        for (byte filter = 0; filter <= 4; filter++)
        {
            byte[] png = CreateFilteredPng(2, 2, rgba, filter);
            (int width, int height, byte[] decoded) = Ra2PngRgbaCodec.Decode(png);
            Assert.Equal(2, width);
            Assert.Equal(2, height);
            Assert.Equal(rgba, decoded);
        }

        byte[] damaged = CreateFilteredPng(2, 2, rgba, 0);
        damaged[^5] ^= 0x20;
        Assert.Throws<InvalidDataException>(() => Ra2PngRgbaCodec.Decode(damaged));
    }

    [Fact]
    public void WestwoodVxlReader_DecodesVoxelNormalForgeCompatibleSpansAndYAxisOrder()
    {
        (byte[] bytes, _) = CreateSyntheticVxl();
        using MemoryStream stream = new(bytes, writable: false);

        Ra2VoxelSceneSnapshot snapshot = Assert.Single(Ra2WestwoodVxlReader.Read(
            stream, "SYNTHETIC", "synth", Ra2VoxelAssemblyPartRole.Body,
            new Ra2VoxelPaletteProfile("unittem", CreateColours(), [])));

        Assert.Equal(2, snapshot.OccupancyCount);
        Assert.True(snapshot.TryGetPaletteIndex(new Ra2VoxelCoordinate(2, 1, 3), out byte upper));
        Assert.Equal((byte)7, upper);
        Assert.True(snapshot.TryGetPaletteIndex(new Ra2VoxelCoordinate(0, 0, 1), out byte lower));
        Assert.Equal((byte)9, lower);
        Assert.Empty(snapshot.Palette.TransparentIndices);
        Assert.Equal(Enumerable.Range(16, 16).Select(value => (byte)value), snapshot.Palette.RemapIndices);
    }

    [Fact]
    public void WestwoodVxlReader_RejectsCorruptDuplicateCountAndTruncation()
    {
        (byte[] valid, int duplicateCountOffset) = CreateSyntheticVxl();
        byte[] corrupt = valid.ToArray();
        corrupt[duplicateCountOffset] = 2;

        Assert.Throws<InvalidDataException>(() => ReadVxl(corrupt));
        Assert.Throws<InvalidDataException>(() => ReadVxl(valid[..^1]));
    }

    private static Ra2VoxelSceneSnapshot CreateSnapshot() => new(
        "TEST_SCENE",
        CreatePart(),
        CreatePalette(),
        [
            new(new Ra2VoxelCoordinate(2, 0, 1), 1),
            new(new Ra2VoxelCoordinate(0, 3, 4), 2),
            new(new Ra2VoxelCoordinate(1, 2, 3), 3)
        ]);

    private static Ra2VoxelPartDescriptor CreatePart() => new(
        "body",
        Ra2VoxelAssemblyPartRole.Body,
        "Body",
        "test",
        3,
        4,
        5);

    private static Ra2VoxelPaletteProfile CreatePalette() => new("test", CreateColours(), [0], [16, 17]);

    private static Ra2Rgba32[] CreateColours()
    {
        Ra2Rgba32[] colours = new Ra2Rgba32[256];
        for (int index = 0; index < colours.Length; index++)
            colours[index] = new Ra2Rgba32((byte)index, (byte)(255 - index), (byte)((index * 17) & 255));
        colours[0] = new Ra2Rgba32(0, 0, 0, 0);
        return colours;
    }

    private static void ReadVox(byte[] bytes)
    {
        using MemoryStream stream = new(bytes, writable: false);
        _ = Ra2MagicaVoxelCodec.Read(stream, "TEST", "body", Ra2VoxelAssemblyPartRole.Body, "Body", "test");
    }

    private static void ReadVxl(byte[] bytes)
    {
        using MemoryStream stream = new(bytes, writable: false);
        _ = Ra2WestwoodVxlReader.Read(
            stream,
            "TEST",
            "test",
            Ra2VoxelAssemblyPartRole.Body,
            new Ra2VoxelPaletteProfile("unittem", CreateColours(), []));
    }

    private static (byte[] Bytes, int DuplicateCountOffset) CreateSyntheticVxl()
    {
        const int xSize = 3;
        const int ySize = 2;
        const int zSize = 4;
        const int columnCount = xSize * ySize;
        int[] starts = Enumerable.Repeat(-1, columnCount).ToArray();
        int[] ends = Enumerable.Repeat(-1, columnCount).ToArray();
        using MemoryStream spanData = new();
        int duplicateOffsetInSpan = -1;
        WriteColumn(column: 2, z: 3, colour: 7);
        WriteColumn(column: 3, z: 1, colour: 9);

        using MemoryStream body = new();
        using (BinaryWriter bodyWriter = new(body, Encoding.ASCII, leaveOpen: true))
        {
            foreach (int start in starts)
                bodyWriter.Write(start);
            foreach (int end in ends)
                bodyWriter.Write(end);
            spanData.Position = 0;
            spanData.CopyTo(body);
        }

        using MemoryStream output = new();
        using BinaryWriter writer = new(output, Encoding.ASCII, leaveOpen: true);
        WriteFixedAscii(writer, "Voxel Animation", 16);
        writer.Write((uint)1);
        writer.Write((uint)1);
        writer.Write((uint)1);
        writer.Write(checked((uint)body.Length));
        writer.Write((byte)16);
        writer.Write((byte)31);
        for (int index = 0; index < 256; index++)
        {
            writer.Write((byte)index);
            writer.Write((byte)(255 - index));
            writer.Write((byte)((index * 17) & 255));
        }
        WriteFixedAscii(writer, "Body", 16);
        writer.Write((uint)0);
        writer.Write((uint)1);
        writer.Write((uint)0);
        long bodyStart = output.Position;
        body.Position = 0;
        body.CopyTo(output);
        writer.Write((uint)0);
        writer.Write((uint)(columnCount * sizeof(int)));
        writer.Write((uint)(columnCount * sizeof(int) * 2));
        writer.Write(1f / 12f);
        for (int index = 0; index < 12; index++)
            writer.Write(index is 0 or 5 or 10 ? 1f : 0f);
        writer.Write(0f);
        writer.Write(0f);
        writer.Write(0f);
        writer.Write((float)xSize);
        writer.Write((float)ySize);
        writer.Write((float)zSize);
        writer.Write((byte)xSize);
        writer.Write((byte)ySize);
        writer.Write((byte)zSize);
        writer.Write((byte)4);
        writer.Flush();
        int duplicateOffset = checked((int)(bodyStart + (columnCount * sizeof(int) * 2) + duplicateOffsetInSpan));
        return (output.ToArray(), duplicateOffset);

        void WriteColumn(int column, int z, byte colour)
        {
            starts[column] = checked((int)spanData.Position);
            spanData.WriteByte((byte)z);
            spanData.WriteByte(1);
            spanData.WriteByte(colour);
            spanData.WriteByte(0);
            duplicateOffsetInSpan = checked((int)spanData.Position);
            spanData.WriteByte(1);
            ends[column] = checked((int)spanData.Position - 1);
        }
    }

    private static byte[] CreateFilteredPng(int width, int height, byte[] rgba, byte filter)
    {
        int rowBytes = width * 4;
        byte[] filtered = new byte[(rowBytes + 1) * height];
        for (int row = 0; row < height; row++)
        {
            int target = row * (rowBytes + 1);
            filtered[target] = filter;
            for (int column = 0; column < rowBytes; column++)
            {
                byte value = rgba[(row * rowBytes) + column];
                byte left = column >= 4 ? rgba[(row * rowBytes) + column - 4] : (byte)0;
                byte above = row > 0 ? rgba[((row - 1) * rowBytes) + column] : (byte)0;
                byte upperLeft = row > 0 && column >= 4 ? rgba[((row - 1) * rowBytes) + column - 4] : (byte)0;
                int predictor = filter switch
                {
                    0 => 0,
                    1 => left,
                    2 => above,
                    3 => (left + above) / 2,
                    4 => Paeth(left, above, upperLeft),
                    _ => throw new ArgumentOutOfRangeException(nameof(filter))
                };
                filtered[target + 1 + column] = unchecked((byte)(value - predictor));
            }
        }

        byte[] compressed;
        using (MemoryStream compressedStream = new())
        {
            using (ZLibStream zlib = new(compressedStream, CompressionLevel.NoCompression, leaveOpen: true))
                zlib.Write(filtered);
            compressed = compressedStream.ToArray();
        }

        using MemoryStream output = new();
        output.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        Span<byte> ihdr = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr, width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr[4..], height);
        ihdr[8] = 8;
        ihdr[9] = 6;
        WritePngChunk(output, "IHDR", ihdr);
        WritePngChunk(output, "IDAT", compressed);
        WritePngChunk(output, "IEND", []);
        return output.ToArray();
    }

    private static int Paeth(byte left, byte above, byte upperLeft)
    {
        int prediction = left + above - upperLeft;
        int leftDistance = Math.Abs(prediction - left);
        int aboveDistance = Math.Abs(prediction - above);
        int upperLeftDistance = Math.Abs(prediction - upperLeft);
        return leftDistance <= aboveDistance && leftDistance <= upperLeftDistance
            ? left
            : aboveDistance <= upperLeftDistance ? above : upperLeft;
    }

    private static void WritePngChunk(Stream output, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> header = stackalloc byte[8];
        BinaryPrimitives.WriteInt32BigEndian(header, data.Length);
        Encoding.ASCII.GetBytes(type, header[4..]);
        output.Write(header);
        output.Write(data);
        uint crc = uint.MaxValue;
        foreach (byte value in header[4..])
            crc = UpdateCrc(crc, value);
        foreach (byte value in data)
            crc = UpdateCrc(crc, value);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, ~crc);
        output.Write(crcBytes);
    }

    private static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (int bit = 0; bit < 8; bit++)
            crc = (crc & 1) != 0 ? 0xedb88320U ^ (crc >> 1) : crc >> 1;
        return crc;
    }

    private static void WriteFixedAscii(BinaryWriter writer, string value, int length)
    {
        byte[] output = new byte[length];
        byte[] encoded = Encoding.ASCII.GetBytes(value);
        Array.Copy(encoded, output, Math.Min(encoded.Length, output.Length));
        writer.Write(output);
    }
}
