namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VxlseSliceDirection
{
    Downward = 0,
    Rightward
}

[Flags]
internal enum Ra2VxlseSliceImportIssue
{
    None = 0,
    RasterDimensionsMismatch = 1 << 0,
    DirectAlphaChannelRequired = 1 << 1,
    EmptyTargetSectionRequired = 1 << 2
}

internal readonly record struct Ra2VoxelCoordinate(int X, int Y, int Z);

internal readonly record struct Ra2SlicePixelCoordinate(int X, int Y);

internal readonly record struct Ra2Rgb24(byte Red, byte Green, byte Blue);

internal readonly record struct Ra2VxlseSliceImportValidation(
    Ra2VxlseSliceImportIssue Issues,
    bool RequiresNormalRegeneration)
{
    internal bool CanImport => Issues == Ra2VxlseSliceImportIssue.None;
}

/// <summary>
/// Reproduces the slice addressing contract used by the user-supplied VXLSE III
/// MagicalVoxel import build. This type does not read PNG files or mutate VXL data.
/// </summary>
internal sealed class Ra2VxlseSliceImportContract
{
    internal const int MaximumVoxelDimension = byte.MaxValue;
    internal const int WestwoodPaletteByteLength = 256 * 3;

    internal Ra2VxlseSliceImportContract(
        int xSize,
        int ySize,
        int zSize,
        Ra2VxlseSliceDirection direction)
    {
        if (!Enum.IsDefined(direction))
            throw new ArgumentOutOfRangeException(nameof(direction));

        ValidateVoxelDimension(xSize, nameof(xSize));
        ValidateVoxelDimension(ySize, nameof(ySize));
        ValidateVoxelDimension(zSize, nameof(zSize));

        XSize = xSize;
        YSize = ySize;
        ZSize = zSize;
        Direction = direction;

        if (direction == Ra2VxlseSliceDirection.Downward)
        {
            Offset = zSize;
            RasterWidth = xSize;
            RasterHeight = checked(ySize * zSize);
        }
        else
        {
            Offset = xSize;
            RasterWidth = checked(ySize * xSize);
            RasterHeight = zSize;
        }
    }

    internal int XSize { get; }
    internal int YSize { get; }
    internal int ZSize { get; }
    internal Ra2VxlseSliceDirection Direction { get; }
    internal int Offset { get; }
    internal int RasterWidth { get; }
    internal int RasterHeight { get; }

    internal Ra2SlicePixelCoordinate MapVoxelToPixel(Ra2VoxelCoordinate coordinate)
    {
        ValidateVoxelCoordinate(coordinate);
        int reversedYBlock = YSize - 1 - coordinate.Y;

        return Direction == Ra2VxlseSliceDirection.Downward
            ? new Ra2SlicePixelCoordinate(
                coordinate.X,
                coordinate.Z + (Offset * reversedYBlock))
            : new Ra2SlicePixelCoordinate(
                coordinate.X + (Offset * reversedYBlock),
                coordinate.Z);
    }

    internal bool TryMapPixelToVoxel(
        Ra2SlicePixelCoordinate pixel,
        out Ra2VoxelCoordinate coordinate)
    {
        coordinate = default;
        if (pixel.X < 0 || pixel.X >= RasterWidth ||
            pixel.Y < 0 || pixel.Y >= RasterHeight)
        {
            return false;
        }

        int reversedYBlock;
        int x;
        int z;
        if (Direction == Ra2VxlseSliceDirection.Downward)
        {
            reversedYBlock = pixel.Y / ZSize;
            x = pixel.X;
            z = pixel.Y % ZSize;
        }
        else
        {
            reversedYBlock = pixel.X / XSize;
            x = pixel.X % XSize;
            z = pixel.Y;
        }

        coordinate = new Ra2VoxelCoordinate(x, YSize - 1 - reversedYBlock, z);
        return true;
    }

    internal Ra2VxlseSliceImportValidation ValidateImport(
        int rasterWidth,
        int rasterHeight,
        bool hasDirectAlphaChannel,
        bool targetSectionIsEmpty)
    {
        Ra2VxlseSliceImportIssue issues = Ra2VxlseSliceImportIssue.None;
        if (rasterWidth != RasterWidth || rasterHeight != RasterHeight)
            issues |= Ra2VxlseSliceImportIssue.RasterDimensionsMismatch;
        if (!hasDirectAlphaChannel)
            issues |= Ra2VxlseSliceImportIssue.DirectAlphaChannelRequired;
        if (!targetSectionIsEmpty)
            issues |= Ra2VxlseSliceImportIssue.EmptyTargetSectionRequired;

        // VXLSE's slice importer writes Used/Colour but does not assign normals.
        return new Ra2VxlseSliceImportValidation(issues, RequiresNormalRegeneration: true);
    }

    internal static bool IsOccupied(byte alpha) => alpha != 0;

    internal static Ra2Rgb24[] DecodeWestwoodPalette(ReadOnlySpan<byte> paletteBytes)
    {
        if (paletteBytes.Length != WestwoodPaletteByteLength)
        {
            throw new ArgumentException(
                $"A Westwood PAL must contain exactly {WestwoodPaletteByteLength} bytes.",
                nameof(paletteBytes));
        }

        Ra2Rgb24[] palette = new Ra2Rgb24[256];
        for (int index = 0; index < palette.Length; index++)
        {
            int offset = index * 3;
            palette[index] = new Ra2Rgb24(
                ScaleWestwoodChannel(paletteBytes[offset]),
                ScaleWestwoodChannel(paletteBytes[offset + 1]),
                ScaleWestwoodChannel(paletteBytes[offset + 2]));
        }

        return palette;
    }

    internal static int FindNearestPaletteIndex(
        ReadOnlySpan<Ra2Rgb24> palette,
        Ra2Rgb24 colour)
    {
        if (palette.IsEmpty || palette.Length > 256)
            throw new ArgumentOutOfRangeException(nameof(palette));

        int selectedIndex = 0;
        long minimumColourDistance = long.MaxValue;
        double minimumStructureDistance = double.MaxValue;
        (double Red, double Green, double Blue) sourceStructure = NormalizeStructure(colour);

        for (int index = 0; index < palette.Length; index++)
        {
            Ra2Rgb24 candidate = palette[index];
            if (candidate == colour)
                return index;

            long colourDistance = SquaredColourDistance(colour, candidate);
            if (colourDistance < minimumColourDistance)
            {
                selectedIndex = index;
                minimumColourDistance = colourDistance;
                minimumStructureDistance = SquaredStructureDistance(
                    sourceStructure,
                    NormalizeStructure(candidate));
            }
            else if (colourDistance == minimumColourDistance)
            {
                double structureDistance = SquaredStructureDistance(
                    sourceStructure,
                    NormalizeStructure(candidate));
                if (structureDistance < minimumStructureDistance)
                {
                    selectedIndex = index;
                    minimumStructureDistance = structureDistance;
                }
            }
        }

        return selectedIndex;
    }

    private static byte ScaleWestwoodChannel(byte channel)
    {
        if (channel > 63)
            throw new ArgumentOutOfRangeException(nameof(channel), "Westwood PAL channels must be in the 0..63 range.");

        return checked((byte)(channel * 4));
    }

    private static long SquaredColourDistance(Ra2Rgb24 left, Ra2Rgb24 right)
    {
        long red = left.Red - right.Red;
        long green = left.Green - right.Green;
        long blue = left.Blue - right.Blue;
        return (red * red) + (green * green) + (blue * blue);
    }

    private static (double Red, double Green, double Blue) NormalizeStructure(Ra2Rgb24 colour)
    {
        int top = Math.Max(colour.Red, Math.Max(colour.Green, colour.Blue));
        return top == 0
            ? (1d, 1d, 1d)
            : ((double)colour.Red / top, (double)colour.Green / top, (double)colour.Blue / top);
    }

    private static double SquaredStructureDistance(
        (double Red, double Green, double Blue) left,
        (double Red, double Green, double Blue) right)
    {
        double red = left.Red - right.Red;
        double green = left.Green - right.Green;
        double blue = left.Blue - right.Blue;
        return (red * red) + (green * green) + (blue * blue);
    }

    private static void ValidateVoxelDimension(int value, string parameterName)
    {
        if (value is < 1 or > MaximumVoxelDimension)
            throw new ArgumentOutOfRangeException(parameterName);
    }

    private void ValidateVoxelCoordinate(Ra2VoxelCoordinate coordinate)
    {
        if (coordinate.X < 0 || coordinate.X >= XSize ||
            coordinate.Y < 0 || coordinate.Y >= YSize ||
            coordinate.Z < 0 || coordinate.Z >= ZSize)
        {
            throw new ArgumentOutOfRangeException(nameof(coordinate));
        }
    }
}
