using System.Security.Cryptography;
using System.Text;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VxlseExecutableAcceptanceTests
{
    private const string ActionVariable = "RA2_VXLSE_ACCEPTANCE_ACTION";
    private const string DirectoryVariable = "RA2_VXLSE_ACCEPTANCE_DIR";
    private const string PaletteVariable = "RA2_VXLSE_PALETTE";
    private const string ResultVariable = "RA2_VXLSE_RESULT_VXL";

    [Fact]
    public void Fixture_IsDeterministicAndRoundTripsThroughTheCanonicalPngPath()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette(CreateSyntheticPaletteBytes());
        Ra2VoxelSceneSnapshot expected = CreateExpectedSnapshot(palette);

        byte[] first = Ra2VoxelSliceStackCodec.ExportPng(expected, Ra2VxlseSliceDirection.Downward);
        byte[] second = Ra2VoxelSliceStackCodec.ExportPng(expected, Ra2VxlseSliceDirection.Downward);
        Ra2VoxelSliceStackRaster raster = Ra2VoxelSliceStackCodec.ImportPng(
            first,
            Ra2VxlseSliceDirection.Downward,
            expected.Part.XSize,
            expected.Part.YSize,
            expected.Part.ZSize,
            expected.CanonicalHash);
        Ra2VoxelSceneSnapshot actual = Ra2VoxelSliceStackCodec.Import(
            raster,
            "VXLSE_ACCEPTANCE_ROUNDTRIP",
            expected.Part,
            palette);

        Assert.Equal(first, second);
        Assert.Equal(expected.Cells, actual.Cells);
    }

    [Fact]
    public void SuppliedVxlseBridge_InvertsTheExecutableAxisMapping()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette(CreateSyntheticPaletteBytes());
        Ra2VoxelSceneSnapshot expected = CreateExpectedSnapshot(palette);
        Ra2VoxelSliceStackRaster raster =
            Ra2VoxelSliceStackCodec.ExportForSuppliedVxlseDownward(expected);
        var bridgePart = new Ra2VoxelPartDescriptor(
            "bridge",
            Ra2VoxelAssemblyPartRole.Body,
            "Body",
            "bridge",
            raster.XSize,
            raster.YSize,
            raster.ZSize);
        Ra2VoxelSceneSnapshot bridge = Ra2VoxelSliceStackCodec.Import(
            raster,
            "BRIDGE",
            bridgePart,
            palette);
        Ra2VoxelCell[] simulatedExecutableResult = bridge.Cells
            .Select(cell => new Ra2VoxelCell(
                new Ra2VoxelCoordinate(
                    cell.Coordinate.Z,
                    bridgePart.XSize - 1 - cell.Coordinate.X,
                    cell.Coordinate.Y),
                cell.PaletteIndex))
            .OrderBy(cell => cell.Coordinate.Z)
            .ThenBy(cell => cell.Coordinate.Y)
            .ThenBy(cell => cell.Coordinate.X)
            .ToArray();

        Assert.Equal(4, raster.Width);
        Assert.Equal(15, raster.Height);
        Assert.Equal((4, 5, 3), (raster.XSize, raster.YSize, raster.ZSize));
        Assert.Equal(expected.Cells, simulatedExecutableResult);
    }

    [Fact]
    public void ExecutableAcceptance_ExportsOrVerifiesOnlyWhenExplicitlyRequested()
    {
        string? action = Environment.GetEnvironmentVariable(ActionVariable);
        if (string.IsNullOrWhiteSpace(action))
            return;

        string directory = RequireEnvironmentPath(DirectoryVariable);
        string palettePath = RequireEnvironmentPath(PaletteVariable);
        byte[] paletteBytes = File.ReadAllBytes(palettePath);
        Ra2VoxelPaletteProfile palette = CreatePalette(paletteBytes);
        Ra2VoxelSceneSnapshot expected = CreateExpectedSnapshot(palette);

        if (action.Equals("export", StringComparison.OrdinalIgnoreCase))
        {
            Directory.CreateDirectory(directory);
            byte[] png = Ra2VoxelSliceStackCodec.ExportPngForSuppliedVxlseDownward(expected);
            File.WriteAllBytes(Path.Combine(directory, "vxlse-acceptance-downward.png"), png);
            File.WriteAllBytes(Path.Combine(directory, "unittem.pal"), paletteBytes);
            File.WriteAllText(
                Path.Combine(directory, "expected.txt"),
                CreateManifest(expected, png, paletteBytes),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return;
        }

        if (!action.Equals("verify", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"{ActionVariable} must be 'export' or 'verify'.");

        string resultPath = RequireEnvironmentPath(ResultVariable);
        using FileStream stream = File.OpenRead(resultPath);
        Ra2VoxelSceneSnapshot actual = Assert.Single(Ra2WestwoodVxlReader.Read(
            stream,
            "VXLSE_EXECUTABLE_ACCEPTANCE",
            "vxlse-acceptance",
            Ra2VoxelAssemblyPartRole.Body,
            palette));

        File.WriteAllText(
            Path.Combine(directory, "actual.txt"),
            CreateActualManifest(actual),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        Assert.Equal(expected.Part.XSize, actual.Part.XSize);
        Assert.Equal(expected.Part.YSize, actual.Part.YSize);
        Assert.Equal(expected.Part.ZSize, actual.Part.ZSize);
        Assert.Equal(expected.OccupancyCount, actual.OccupancyCount);
        foreach (Ra2VoxelCell cell in expected.Cells)
        {
            Assert.True(
                actual.TryGetPaletteIndex(cell.Coordinate, out byte paletteIndex),
                $"VXLSE result is missing voxel {cell.Coordinate}.");
            Assert.Equal(cell.PaletteIndex, paletteIndex);
        }

        File.WriteAllText(
            Path.Combine(directory, "verified.txt"),
            $"result={Path.GetFullPath(resultPath)}{Environment.NewLine}" +
            $"section={actual.Part.VxlSectionName}{Environment.NewLine}" +
            $"dimensions={actual.Part.XSize}x{actual.Part.YSize}x{actual.Part.ZSize}{Environment.NewLine}" +
            $"occupancy={actual.OccupancyCount}{Environment.NewLine}" +
            $"canonicalHash={actual.CanonicalHash}{Environment.NewLine}",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static Ra2VoxelSceneSnapshot CreateExpectedSnapshot(Ra2VoxelPaletteProfile palette) => new(
        "VXLSE_EXECUTABLE_ACCEPTANCE",
        new Ra2VoxelPartDescriptor(
            "body",
            Ra2VoxelAssemblyPartRole.Body,
            "Body",
            "vxlse-acceptance",
            xSize: 3,
            ySize: 4,
            zSize: 5),
        palette,
        [
            new(new Ra2VoxelCoordinate(0, 0, 0), 193),
            new(new Ra2VoxelCoordinate(2, 3, 4), 168),
            new(new Ra2VoxelCoordinate(1, 1, 3), 145),
            new(new Ra2VoxelCoordinate(2, 2, 0), 40),
            new(new Ra2VoxelCoordinate(0, 3, 2), 102)
        ]);

    private static Ra2VoxelPaletteProfile CreatePalette(byte[] bytes)
    {
        Ra2Rgb24[] decoded = Ra2VxlseSliceImportContract.DecodeWestwoodPalette(bytes);
        return new Ra2VoxelPaletteProfile(
            "ra2-unittem-vxlse-acceptance",
            decoded.Select(colour => new Ra2Rgba32(colour.Red, colour.Green, colour.Blue)),
            transparentIndices: [],
            remapIndices: Enumerable.Range(16, 16).Select(index => checked((byte)index)));
    }

    private static string CreateManifest(
        Ra2VoxelSceneSnapshot expected,
        byte[] png,
        byte[] paletteBytes)
    {
        StringBuilder text = new();
        text.AppendLine("direction=Downward");
        text.AppendLine("offset=3");
        text.AppendLine("rasterVolume=4x5x3");
        text.AppendLine("targetDimensions=3x4x5");
        text.AppendLine($"occupancy={expected.OccupancyCount}");
        text.AppendLine($"sceneHash={expected.CanonicalHash}");
        text.AppendLine($"pngSha256={Convert.ToHexString(SHA256.HashData(png))}");
        text.AppendLine($"paletteSha256={Convert.ToHexString(SHA256.HashData(paletteBytes))}");
        foreach (Ra2VoxelCell cell in expected.Cells)
        {
            text.AppendLine(
                $"cell={cell.Coordinate.X},{cell.Coordinate.Y},{cell.Coordinate.Z},{cell.PaletteIndex}");
        }
        return text.ToString();
    }

    private static string CreateActualManifest(Ra2VoxelSceneSnapshot actual)
    {
        StringBuilder text = new();
        text.AppendLine($"section={actual.Part.VxlSectionName}");
        text.AppendLine($"dimensions={actual.Part.XSize}x{actual.Part.YSize}x{actual.Part.ZSize}");
        text.AppendLine($"occupancy={actual.OccupancyCount}");
        foreach (Ra2VoxelCell cell in actual.Cells)
        {
            text.AppendLine(
                $"cell={cell.Coordinate.X},{cell.Coordinate.Y},{cell.Coordinate.Z},{cell.PaletteIndex}");
        }
        return text.ToString();
    }

    private static string RequireEnvironmentPath(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Environment variable {name} is required.");
        return Path.GetFullPath(value);
    }

    private static byte[] CreateSyntheticPaletteBytes()
    {
        byte[] bytes = new byte[Ra2VxlseSliceImportContract.WestwoodPaletteByteLength];
        for (int index = 0; index < 256; index++)
        {
            bytes[index * 3] = checked((byte)(index % 64));
            bytes[(index * 3) + 1] = checked((byte)((index / 4) % 64));
            bytes[(index * 3) + 2] = checked((byte)((index * 7) % 64));
        }
        return bytes;
    }
}
