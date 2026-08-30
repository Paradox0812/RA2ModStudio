using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VxlseSliceImportContractTests
{
    [Theory]
    [InlineData((int)Ra2VxlseSliceDirection.Downward, 3, 20, 5)]
    [InlineData((int)Ra2VxlseSliceDirection.Rightward, 12, 5, 3)]
    public void Layout_RoundTripsEveryAsymmetricSyntheticBarrelVoxel(
        int directionValue,
        int expectedWidth,
        int expectedHeight,
        int expectedOffset)
    {
        Ra2VxlseSliceDirection direction = (Ra2VxlseSliceDirection)directionValue;
        Ra2VxlseSliceImportContract contract = new(3, 4, 5, direction);

        Assert.Equal(expectedWidth, contract.RasterWidth);
        Assert.Equal(expectedHeight, contract.RasterHeight);
        Assert.Equal(expectedOffset, contract.Offset);

        for (int y = 0; y < contract.YSize; y++)
        {
            for (int z = 0; z < contract.ZSize; z++)
            {
                for (int x = 0; x < contract.XSize; x++)
                {
                    Ra2VoxelCoordinate source = new(x, y, z);
                    Ra2SlicePixelCoordinate pixel = contract.MapVoxelToPixel(source);

                    Assert.True(contract.TryMapPixelToVoxel(pixel, out Ra2VoxelCoordinate roundTrip));
                    Assert.Equal(source, roundTrip);
                }
            }
        }
    }

    [Fact]
    public void Layout_PlacesHighestYFirstInBothVxlseDirections()
    {
        Ra2VxlseSliceImportContract downward = new(3, 4, 5, Ra2VxlseSliceDirection.Downward);
        Ra2VxlseSliceImportContract rightward = new(3, 4, 5, Ra2VxlseSliceDirection.Rightward);

        Assert.Equal(new Ra2SlicePixelCoordinate(2, 4), downward.MapVoxelToPixel(new(2, 3, 4)));
        Assert.Equal(new Ra2SlicePixelCoordinate(2, 19), downward.MapVoxelToPixel(new(2, 0, 4)));
        Assert.Equal(new Ra2SlicePixelCoordinate(2, 4), rightward.MapVoxelToPixel(new(2, 3, 4)));
        Assert.Equal(new Ra2SlicePixelCoordinate(11, 4), rightward.MapVoxelToPixel(new(2, 0, 4)));
    }

    [Fact]
    public void ImportPreflight_RequiresExactRgbaRasterAndEmptyTargetThenRegeneratedNormals()
    {
        Ra2VxlseSliceImportContract contract = new(3, 4, 5, Ra2VxlseSliceDirection.Downward);

        Ra2VxlseSliceImportValidation valid = contract.ValidateImport(3, 20, true, true);
        Ra2VxlseSliceImportValidation invalid = contract.ValidateImport(4, 20, false, false);

        Assert.True(valid.CanImport);
        Assert.True(valid.RequiresNormalRegeneration);
        Assert.False(invalid.CanImport);
        Assert.Equal(
            Ra2VxlseSliceImportIssue.RasterDimensionsMismatch |
            Ra2VxlseSliceImportIssue.DirectAlphaChannelRequired |
            Ra2VxlseSliceImportIssue.EmptyTargetSectionRequired,
            invalid.Issues);
        Assert.True(invalid.RequiresNormalRegeneration);
    }

    [Fact]
    public void Occupancy_UsesZeroVersusAnyNonZeroAlpha()
    {
        Assert.False(Ra2VxlseSliceImportContract.IsOccupied(0));
        Assert.True(Ra2VxlseSliceImportContract.IsOccupied(1));
        Assert.True(Ra2VxlseSliceImportContract.IsOccupied(byte.MaxValue));
    }

    [Fact]
    public void Palette_DecodesSixBitChannelsAndSelectsVxlseCompatibleNearestColour()
    {
        byte[] bytes = new byte[Ra2VxlseSliceImportContract.WestwoodPaletteByteLength];
        bytes[3] = 63;
        bytes[7] = 63;
        Ra2Rgb24[] palette = Ra2VxlseSliceImportContract.DecodeWestwoodPalette(bytes);

        Assert.Equal(new Ra2Rgb24(252, 0, 0), palette[1]);
        Assert.Equal(new Ra2Rgb24(0, 252, 0), palette[2]);
        Assert.Equal(1, Ra2VxlseSliceImportContract.FindNearestPaletteIndex(
            palette,
            new Ra2Rgb24(250, 4, 0)));
        Assert.Equal(2, Ra2VxlseSliceImportContract.FindNearestPaletteIndex(
            palette,
            new Ra2Rgb24(0, 250, 4)));
    }

    [Fact]
    public void Palette_RejectsMalformedOrOutOfRangeWestwoodData()
    {
        Assert.Throws<ArgumentException>(() =>
            Ra2VxlseSliceImportContract.DecodeWestwoodPalette(new byte[767]));

        byte[] invalid = new byte[Ra2VxlseSliceImportContract.WestwoodPaletteByteLength];
        invalid[0] = 64;
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Ra2VxlseSliceImportContract.DecodeWestwoodPalette(invalid));
    }

    [Fact]
    public void Layout_RejectsDimensionsOutsideVxlByteRangeAndOutOfBoundsPixels()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ra2VxlseSliceImportContract(0, 4, 5, Ra2VxlseSliceDirection.Downward));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ra2VxlseSliceImportContract(3, 256, 5, Ra2VxlseSliceDirection.Downward));

        Ra2VxlseSliceImportContract contract = new(3, 4, 5, Ra2VxlseSliceDirection.Rightward);
        Assert.False(contract.TryMapPixelToVoxel(new(-1, 0), out _));
        Assert.False(contract.TryMapPixelToVoxel(new(contract.RasterWidth, 0), out _));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            contract.MapVoxelToPixel(new Ra2VoxelCoordinate(3, 0, 0)));
    }
}
