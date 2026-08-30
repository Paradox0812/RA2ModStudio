using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

/// <summary>Deterministic, bounded PNG codec restricted to non-interlaced 8-bit RGBA rasters.</summary>
internal static class Ra2PngRgbaCodec
{
    internal const int MaximumEncodedByteLength = 256 * 1024 * 1024;
    internal const int MaximumDecodedByteLength = 128 * 1024 * 1024;
    internal const int MaximumChunkCount = 4096;

    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

    internal static byte[] Encode(Ra2VoxelSliceStackRaster raster)
    {
        ArgumentNullException.ThrowIfNull(raster);
        int rowBytes = checked(raster.Width * 4);
        int filteredLength = checked((rowBytes + 1) * raster.Height);
        if (filteredLength > MaximumDecodedByteLength)
            throw new InvalidDataException("PNG raster exceeds the decoded byte limit.");

        byte[] filtered = new byte[filteredLength];
        ReadOnlySpan<byte> rgba = raster.RgbaBytes.Span;
        for (int row = 0; row < raster.Height; row++)
        {
            int targetOffset = row * (rowBytes + 1);
            filtered[targetOffset] = 0;
            rgba.Slice(row * rowBytes, rowBytes).CopyTo(filtered.AsSpan(targetOffset + 1, rowBytes));
        }

        byte[] compressed;
        using (MemoryStream compressedStream = new())
        {
            using (ZLibStream zlib = new(compressedStream, CompressionLevel.NoCompression, leaveOpen: true))
                zlib.Write(filtered);
            compressed = compressedStream.ToArray();
        }

        using MemoryStream output = new();
        output.Write(Signature);
        Span<byte> ihdr = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr, raster.Width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr[4..], raster.Height);
        ihdr[8] = 8;
        ihdr[9] = 6;
        ihdr[10] = 0;
        ihdr[11] = 0;
        ihdr[12] = 0;
        WriteChunk(output, "IHDR", ihdr);
        WriteChunk(output, "IDAT", compressed);
        WriteChunk(output, "IEND", []);
        if (output.Length > MaximumEncodedByteLength)
            throw new InvalidDataException("PNG output exceeds the encoded byte limit.");
        return output.ToArray();
    }

    internal static (int Width, int Height, byte[] Rgba) Decode(ReadOnlySpan<byte> pngBytes)
    {
        if (pngBytes.Length > MaximumEncodedByteLength)
            throw new InvalidDataException("PNG input exceeds the encoded byte limit.");
        if (pngBytes.Length < Signature.Length || !pngBytes[..Signature.Length].SequenceEqual(Signature))
            throw new InvalidDataException("PNG signature is invalid.");

        int offset = Signature.Length;
        int width = 0;
        int height = 0;
        bool sawHeader = false;
        bool sawData = false;
        bool sawEnd = false;
        int chunkCount = 0;
        using MemoryStream compressed = new();
        while (offset < pngBytes.Length)
        {
            if (++chunkCount > MaximumChunkCount || pngBytes.Length - offset < 12)
                throw new InvalidDataException("PNG chunk structure is truncated or exceeds its limit.");
            int length = BinaryPrimitives.ReadInt32BigEndian(pngBytes[offset..]);
            if (length < 0 || length > MaximumEncodedByteLength)
                throw new InvalidDataException("PNG chunk length is invalid.");
            int chunkEnd = checked(offset + 12 + length);
            if (chunkEnd > pngBytes.Length)
                throw new InvalidDataException("PNG chunk extends beyond the input.");

            ReadOnlySpan<byte> type = pngBytes.Slice(offset + 4, 4);
            ReadOnlySpan<byte> data = pngBytes.Slice(offset + 8, length);
            uint expectedCrc = BinaryPrimitives.ReadUInt32BigEndian(pngBytes.Slice(offset + 8 + length, 4));
            if (ComputeCrc(type, data) != expectedCrc)
                throw new InvalidDataException("PNG chunk CRC is invalid.");
            string chunkType = Encoding.ASCII.GetString(type);

            switch (chunkType)
            {
                case "IHDR":
                    if (sawHeader || offset != Signature.Length || length != 13)
                        throw new InvalidDataException("PNG IHDR ordering or size is invalid.");
                    width = BinaryPrimitives.ReadInt32BigEndian(data);
                    height = BinaryPrimitives.ReadInt32BigEndian(data[4..]);
                    if (width < 1 || height < 1 || data[8] != 8 || data[9] != 6 ||
                        data[10] != 0 || data[11] != 0 || data[12] != 0)
                    {
                        throw new InvalidDataException("Only non-interlaced 8-bit RGBA PNG is supported.");
                    }
                    ValidateDecodedLength(width, height);
                    sawHeader = true;
                    break;
                case "IDAT":
                    if (!sawHeader || sawEnd)
                        throw new InvalidDataException("PNG IDAT ordering is invalid.");
                    if (compressed.Length + data.Length > MaximumEncodedByteLength)
                        throw new InvalidDataException("PNG compressed data exceeds its limit.");
                    compressed.Write(data);
                    sawData = true;
                    break;
                case "IEND":
                    if (!sawHeader || !sawData || sawEnd || length != 0)
                        throw new InvalidDataException("PNG IEND ordering or size is invalid.");
                    sawEnd = true;
                    if (chunkEnd != pngBytes.Length)
                        throw new InvalidDataException("PNG contains trailing bytes after IEND.");
                    break;
                default:
                    if (!sawHeader || sawEnd || !char.IsLower((char)type[0]))
                        throw new InvalidDataException($"Unsupported critical or misplaced PNG chunk '{chunkType}'.");
                    break;
            }

            offset = chunkEnd;
        }

        if (!sawHeader || !sawData || !sawEnd)
            throw new InvalidDataException("PNG is missing required chunks.");

        int rowBytes = checked(width * 4);
        int filteredLength = checked((rowBytes + 1) * height);
        byte[] filtered = new byte[filteredLength];
        compressed.Position = 0;
        using (ZLibStream zlib = new(compressed, CompressionMode.Decompress, leaveOpen: true))
        {
            int total = 0;
            while (total < filtered.Length)
            {
                int read = zlib.Read(filtered, total, filtered.Length - total);
                if (read == 0)
                    break;
                total += read;
            }
            if (total != filtered.Length || zlib.ReadByte() != -1)
                throw new InvalidDataException("PNG decompressed data length is invalid.");
        }

        byte[] rgba = new byte[checked(rowBytes * height)];
        for (int row = 0; row < height; row++)
        {
            int filteredOffset = row * (rowBytes + 1);
            byte filter = filtered[filteredOffset];
            if (filter > 4)
                throw new InvalidDataException("PNG uses an unsupported row filter.");
            for (int column = 0; column < rowBytes; column++)
            {
                byte raw = filtered[filteredOffset + 1 + column];
                byte left = column >= 4 ? rgba[(row * rowBytes) + column - 4] : (byte)0;
                byte above = row > 0 ? rgba[((row - 1) * rowBytes) + column] : (byte)0;
                byte upperLeft = row > 0 && column >= 4
                    ? rgba[((row - 1) * rowBytes) + column - 4]
                    : (byte)0;
                rgba[(row * rowBytes) + column] = filter switch
                {
                    0 => raw,
                    1 => unchecked((byte)(raw + left)),
                    2 => unchecked((byte)(raw + above)),
                    3 => unchecked((byte)(raw + ((left + above) / 2))),
                    4 => unchecked((byte)(raw + Paeth(left, above, upperLeft))),
                    _ => throw new InvalidDataException("PNG row filter is invalid.")
                };
            }
        }
        return (width, height, rgba);
    }

    private static void ValidateDecodedLength(int width, int height)
    {
        try
        {
            int rowBytes = checked(width * 4);
            int length = checked((rowBytes + 1) * height);
            if (length > MaximumDecodedByteLength)
                throw new InvalidDataException("PNG raster exceeds the decoded byte limit.");
        }
        catch (OverflowException exception)
        {
            throw new InvalidDataException("PNG dimensions exceed the decoded byte limit.", exception);
        }
    }

    private static byte Paeth(byte left, byte above, byte upperLeft)
    {
        int prediction = left + above - upperLeft;
        int leftDistance = Math.Abs(prediction - left);
        int aboveDistance = Math.Abs(prediction - above);
        int upperLeftDistance = Math.Abs(prediction - upperLeft);
        return leftDistance <= aboveDistance && leftDistance <= upperLeftDistance
            ? left
            : aboveDistance <= upperLeftDistance ? above : upperLeft;
    }

    private static void WriteChunk(Stream output, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> header = stackalloc byte[8];
        BinaryPrimitives.WriteInt32BigEndian(header, data.Length);
        Encoding.ASCII.GetBytes(type, header[4..]);
        output.Write(header);
        output.Write(data);
        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, ComputeCrc(header[4..], data));
        output.Write(crc);
    }

    private static uint ComputeCrc(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        uint crc = uint.MaxValue;
        foreach (byte value in type)
            crc = UpdateCrc(crc, value);
        foreach (byte value in data)
            crc = UpdateCrc(crc, value);
        return ~crc;
    }

    private static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (int bit = 0; bit < 8; bit++)
            crc = (crc & 1) != 0 ? 0xedb88320U ^ (crc >> 1) : crc >> 1;
        return crc;
    }
}
