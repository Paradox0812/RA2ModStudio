using System.Security.Cryptography;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal sealed class Ra2VoxelSliceStackRaster
{
    private readonly byte[] _rgbaBytes;

    internal Ra2VoxelSliceStackRaster(
        int width,
        int height,
        Ra2VxlseSliceDirection direction,
        int xSize,
        int ySize,
        int zSize,
        string sourceSceneHash,
        ReadOnlySpan<byte> rgbaBytes)
    {
        if (width < 1 || height < 1)
            throw new ArgumentOutOfRangeException(nameof(width));
        int expectedLength = checked(width * height * 4);
        if (rgbaBytes.Length != expectedLength)
            throw new ArgumentException("RGBA raster length does not match its dimensions.", nameof(rgbaBytes));
        if (sourceSceneHash.Length != 64 || sourceSceneHash.Any(character => !Uri.IsHexDigit(character)))
            throw new ArgumentException("SliceStack source hash must be SHA-256 hex.", nameof(sourceSceneHash));

        _ = new Ra2VxlseSliceImportContract(xSize, ySize, zSize, direction);
        Width = width;
        Height = height;
        Direction = direction;
        XSize = xSize;
        YSize = ySize;
        ZSize = zSize;
        SourceSceneHash = sourceSceneHash.ToUpperInvariant();
        _rgbaBytes = rgbaBytes.ToArray();
        RgbaSha256 = Convert.ToHexString(SHA256.HashData(_rgbaBytes));
    }

    internal int Width { get; }
    internal int Height { get; }
    internal Ra2VxlseSliceDirection Direction { get; }
    internal int XSize { get; }
    internal int YSize { get; }
    internal int ZSize { get; }
    internal string SourceSceneHash { get; }
    internal string RgbaSha256 { get; }
    internal ReadOnlyMemory<byte> RgbaBytes => _rgbaBytes;
}

internal static class Ra2VoxelSliceStackCodec
{
    internal static Ra2VoxelSliceStackRaster Export(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VxlseSliceDirection direction)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var contract = new Ra2VxlseSliceImportContract(
            snapshot.Part.XSize,
            snapshot.Part.YSize,
            snapshot.Part.ZSize,
            direction);
        byte[] rgba = new byte[checked(contract.RasterWidth * contract.RasterHeight * 4)];
        foreach (Ra2VoxelCell cell in snapshot.Cells)
        {
            Ra2SlicePixelCoordinate pixel = contract.MapVoxelToPixel(cell.Coordinate);
            int offset = checked(((pixel.Y * contract.RasterWidth) + pixel.X) * 4);
            Ra2Rgba32 colour = snapshot.Palette[cell.PaletteIndex];
            rgba[offset] = colour.Red;
            rgba[offset + 1] = colour.Green;
            rgba[offset + 2] = colour.Blue;
            rgba[offset + 3] = byte.MaxValue;
        }

        return new Ra2VoxelSliceStackRaster(
            contract.RasterWidth,
            contract.RasterHeight,
            direction,
            snapshot.Part.XSize,
            snapshot.Part.YSize,
            snapshot.Part.ZSize,
            snapshot.CanonicalHash,
            rgba);
    }

    /// <summary>
    /// Exports the inverse axis mapping required by the user-supplied VXLSE III
    /// MagicalVoxel import build. Its Downward importer maps input (x,y,z) to
    /// VXL (z, inputXSize - 1 - x, y), so the raster volume must be Y,Z,X.
    /// </summary>
    internal static Ra2VoxelSliceStackRaster ExportForSuppliedVxlseDownward(
        Ra2VoxelSceneSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var contract = new Ra2VxlseSliceImportContract(
            snapshot.Part.YSize,
            snapshot.Part.ZSize,
            snapshot.Part.XSize,
            Ra2VxlseSliceDirection.Downward);
        byte[] rgba = new byte[checked(contract.RasterWidth * contract.RasterHeight * 4)];
        foreach (Ra2VoxelCell cell in snapshot.Cells)
        {
            var bridgeCoordinate = new Ra2VoxelCoordinate(
                snapshot.Part.YSize - 1 - cell.Coordinate.Y,
                cell.Coordinate.Z,
                cell.Coordinate.X);
            Ra2SlicePixelCoordinate pixel = contract.MapVoxelToPixel(bridgeCoordinate);
            int offset = checked(((pixel.Y * contract.RasterWidth) + pixel.X) * 4);
            Ra2Rgba32 colour = snapshot.Palette[cell.PaletteIndex];
            rgba[offset] = colour.Red;
            rgba[offset + 1] = colour.Green;
            rgba[offset + 2] = colour.Blue;
            rgba[offset + 3] = byte.MaxValue;
        }

        return new Ra2VoxelSliceStackRaster(
            contract.RasterWidth,
            contract.RasterHeight,
            Ra2VxlseSliceDirection.Downward,
            contract.XSize,
            contract.YSize,
            contract.ZSize,
            snapshot.CanonicalHash,
            rgba);
    }

    internal static Ra2VoxelSceneSnapshot Import(
        Ra2VoxelSliceStackRaster raster,
        string sceneId,
        Ra2VoxelPartDescriptor part,
        Ra2VoxelPaletteProfile palette,
        IEnumerable<KeyValuePair<string, string>>? sourceArtifactHashes = null)
    {
        ArgumentNullException.ThrowIfNull(raster);
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(palette);
        if (raster.XSize != part.XSize || raster.YSize != part.YSize || raster.ZSize != part.ZSize)
            throw new InvalidDataException("SliceStack volume dimensions do not match the target part.");

        var contract = new Ra2VxlseSliceImportContract(part.XSize, part.YSize, part.ZSize, raster.Direction);
        Ra2VxlseSliceImportValidation validation = contract.ValidateImport(
            raster.Width,
            raster.Height,
            hasDirectAlphaChannel: true,
            targetSectionIsEmpty: true);
        if (!validation.CanImport)
            throw new InvalidDataException("SliceStack raster does not satisfy the VXLSE import contract.");

        ReadOnlySpan<byte> rgba = raster.RgbaBytes.Span;
        List<Ra2VoxelCell> cells = new();
        for (int y = 0; y < raster.Height; y++)
        {
            for (int x = 0; x < raster.Width; x++)
            {
                int offset = checked(((y * raster.Width) + x) * 4);
                byte alpha = rgba[offset + 3];
                if (!Ra2VxlseSliceImportContract.IsOccupied(alpha))
                    continue;
                if (!contract.TryMapPixelToVoxel(new Ra2SlicePixelCoordinate(x, y), out Ra2VoxelCoordinate coordinate))
                    throw new InvalidDataException("SliceStack pixel cannot be mapped to a voxel coordinate.");
                byte paletteIndex = palette.FindNearestOpaqueIndex(
                    new Ra2Rgba32(rgba[offset], rgba[offset + 1], rgba[offset + 2], alpha));
                cells.Add(new Ra2VoxelCell(coordinate, paletteIndex));
                if (cells.Count > Ra2VoxelSceneSnapshot.MaximumOccupancyCount)
                    throw new InvalidDataException("SliceStack occupancy exceeds the supported limit.");
            }
        }

        return new Ra2VoxelSceneSnapshot(sceneId, part, palette, cells, sourceArtifactHashes);
    }

    internal static byte[] ExportPng(Ra2VoxelSceneSnapshot snapshot, Ra2VxlseSliceDirection direction) =>
        Ra2PngRgbaCodec.Encode(Export(snapshot, direction));

    internal static byte[] ExportPngForSuppliedVxlseDownward(Ra2VoxelSceneSnapshot snapshot) =>
        Ra2PngRgbaCodec.Encode(ExportForSuppliedVxlseDownward(snapshot));

    internal static Ra2VoxelSliceStackRaster ImportPng(
        ReadOnlySpan<byte> pngBytes,
        Ra2VxlseSliceDirection direction,
        int xSize,
        int ySize,
        int zSize,
        string sourceSceneHash)
    {
        (int width, int height, byte[] rgba) = Ra2PngRgbaCodec.Decode(pngBytes);
        return new Ra2VoxelSliceStackRaster(
            width,
            height,
            direction,
            xSize,
            ySize,
            zSize,
            sourceSceneHash,
            rgba);
    }
}
