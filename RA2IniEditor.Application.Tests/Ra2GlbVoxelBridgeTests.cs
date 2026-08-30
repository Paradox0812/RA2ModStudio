using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2GlbVoxelBridgeTests
{
    private static readonly ushort[] CubeIndices =
    [
        0, 2, 1, 0, 3, 2,
        4, 5, 6, 4, 6, 7,
        0, 1, 5, 0, 5, 4,
        3, 7, 6, 3, 6, 2,
        0, 4, 7, 0, 7, 3,
        1, 2, 6, 1, 6, 5
    ];

    [Fact]
    public void Reader_ParsesRestrictedWatertightGlbAndComputesTopology()
    {
        Ra2MeshSnapshot mesh = Ra2GlbMeshReader.Read(CreateCubeGlb());

        Assert.Equal(8, mesh.Positions.Count);
        Assert.Equal(12, mesh.Triangles.Count);
        Assert.True(mesh.Topology.IsWatertightSingleComponent);
        Assert.Equal(new Ra2MeshVector3(0, 0, 0), mesh.Bounds.Minimum);
        Assert.Equal(new Ra2MeshVector3(1, 1, 1), mesh.Bounds.Maximum);
        Assert.Equal(64, mesh.SourceHash.Length);
    }

    [Fact]
    public void Reader_AppliesNodeTrsBeforeReturningGeometry()
    {
        Ra2MeshSnapshot mesh = Ra2GlbMeshReader.Read(CreateCubeGlb(
            nodeProperties: ",\"translation\":[2,3,4],\"scale\":[2,3,4]"));

        Assert.Equal(new Ra2MeshVector3(2, 3, 4), mesh.Bounds.Minimum);
        Assert.Equal(new Ra2MeshVector3(4, 6, 8), mesh.Bounds.Maximum);
        Assert.True(mesh.Topology.IsWatertightSingleComponent);
    }

    [Fact]
    public void Reader_RejectsMalformedHeaderAndUnsupportedPrimitiveMode()
    {
        byte[] invalid = CreateCubeGlb();
        invalid[0] ^= 0x20;
        Ra2MeshVoxelizationException malformed = Assert.Throws<Ra2MeshVoxelizationException>(
            () => Ra2GlbMeshReader.Read(invalid));
        Assert.Equal(Ra2MeshVoxelizationFailureKind.MalformedContainer, malformed.FailureKind);

        Ra2MeshVoxelizationException unsupported = Assert.Throws<Ra2MeshVoxelizationException>(
            () => Ra2GlbMeshReader.Read(CreateCubeGlb(mode: 1)));
        Assert.Equal(Ra2MeshVoxelizationFailureKind.UnsupportedFeature, unsupported.FailureKind);
    }

    [Fact]
    public void Voxelizer_MapsAxesFillsSolidAndProducesCanonicalCandidate()
    {
        Ra2MeshVoxelizationResult result = Ra2MeshVoxelizer.ConvertGlb(
            CreateCubeGlb(nodeProperties: ",\"scale\":[2,3,4]"),
            CreateOptions(targetLongestDimension: 16));

        Assert.True(result.IsSuccess, result.Message);
        Ra2VoxelSceneSnapshot snapshot = Assert.IsType<Ra2VoxelSceneSnapshot>(result.Snapshot);
        Ra2MeshVoxelizationFacts facts = Assert.IsType<Ra2MeshVoxelizationFacts>(result.Facts);
        Assert.Equal(10, snapshot.Part.XSize);
        Assert.Equal(16, snapshot.Part.YSize);
        Assert.Equal(13, snapshot.Part.ZSize);
        Assert.Equal(Ra2MeshVoxelizer.AxisMapId, facts.AxisMapId);
        Assert.True(facts.SurfaceCellCount > 0);
        Assert.True(facts.InteriorCellCount > 0);
        Assert.Equal(snapshot.OccupancyCount, facts.TotalCellCount);
        Assert.True(snapshot.Connectivity.IsSingleComponent);
        Assert.Equal((byte)7, facts.PaletteIndex);
        Assert.True(facts.ReviewFlags.HasFlag(Ra2MeshVoxelReviewFlags.SemanticPartSplitNotAttempted));
        Assert.DoesNotContain(snapshot.Cells, cell =>
            cell.Coordinate.X == 0 || cell.Coordinate.X == snapshot.Part.XSize - 1 ||
            cell.Coordinate.Y == 0 || cell.Coordinate.Y == snapshot.Part.YSize - 1 ||
            cell.Coordinate.Z == 0 || cell.Coordinate.Z == snapshot.Part.ZSize - 1);
    }

    [Fact]
    public void Voxelizer_IsDeterministicAndExistingExchangeCodecsRoundTrip()
    {
        byte[] glb = CreateCubeGlb();
        Ra2MeshVoxelizationResult first = Ra2MeshVoxelizer.ConvertGlb(glb, CreateOptions(20));
        Ra2MeshVoxelizationResult second = Ra2MeshVoxelizer.ConvertGlb(glb, CreateOptions(20));
        Assert.True(first.IsSuccess, first.Message);
        Assert.True(second.IsSuccess, second.Message);
        Ra2VoxelSceneSnapshot source = first.Snapshot!;
        Assert.Equal(source.CanonicalHash, second.Snapshot!.CanonicalHash);
        Assert.Equal(first.Facts, second.Facts);

        byte[] vox = Ra2MagicaVoxelCodec.Write(source);
        using MemoryStream voxStream = new(vox, writable: false);
        Ra2VoxelSceneSnapshot voxRoundTrip = Ra2MagicaVoxelCodec.Read(
            voxStream, "VOX_ROUNDTRIP", "body", Ra2VoxelAssemblyPartRole.Body, "Body", "bridge");
        Assert.Equal(source.Cells, voxRoundTrip.Cells);
        Assert.Equal(vox, Ra2MagicaVoxelCodec.Write(voxRoundTrip));

        Ra2VoxelSliceStackRaster raster = Ra2VoxelSliceStackCodec.Export(source, Ra2VxlseSliceDirection.Downward);
        Ra2VoxelSceneSnapshot sliceRoundTrip = Ra2VoxelSliceStackCodec.Import(
            raster, "SLICE_ROUNDTRIP", source.Part, source.Palette);
        Assert.Equal(source.Cells, sliceRoundTrip.Cells);
    }

    [Fact]
    public void Voxelizer_RejectsOpenGeometryWithoutPartialSnapshot()
    {
        ushort[] openIndices = CubeIndices[..6];
        Ra2MeshVoxelizationResult result = Ra2MeshVoxelizer.ConvertGlb(
            CreateCubeGlb(indices: openIndices),
            CreateOptions(16));

        Assert.False(result.IsSuccess);
        Assert.Equal(Ra2MeshVoxelizationFailureKind.OpenSurface, result.FailureKind);
        Assert.Null(result.Snapshot);
        Assert.Null(result.Facts);
    }

    [Fact]
    public void Voxelizer_RejectsNonManifoldAndDegenerateGeometryWithTypedFailures()
    {
        Ra2MeshVoxelizationResult nonManifold = Ra2MeshVoxelizer.ConvertGlb(
            CreateCubeGlb(indices: CubeIndices.Concat(CubeIndices[..3]).ToArray()),
            CreateOptions(16));
        Assert.Equal(Ra2MeshVoxelizationFailureKind.NonManifoldSurface, nonManifold.FailureKind);
        Assert.Null(nonManifold.Snapshot);

        Ra2MeshVoxelizationResult degenerate = Ra2MeshVoxelizer.ConvertGlb(
            CreateCubeGlb(indices: [0, 0, 1]),
            CreateOptions(16));
        Assert.Equal(Ra2MeshVoxelizationFailureKind.DegenerateGeometry, degenerate.FailureKind);
        Assert.Null(degenerate.Snapshot);
    }

    [Fact]
    public void Voxelizer_RejectsOversizedContainerAndInvalidIndicesWithTypedFailures()
    {
        Ra2MeshVoxelizationResult oversized = Ra2MeshVoxelizer.ConvertGlb(
            new byte[Ra2GlbMeshReader.MaximumGlbBytes + 1],
            CreateOptions(16));
        Assert.Equal(Ra2MeshVoxelizationFailureKind.InputTooLarge, oversized.FailureKind);

        ushort[] indices = CubeIndices.ToArray();
        indices[0] = 9;
        Ra2MeshVoxelizationResult invalidIndex = Ra2MeshVoxelizer.ConvertGlb(
            CreateCubeGlb(indices: indices),
            CreateOptions(16));
        Assert.Equal(Ra2MeshVoxelizationFailureKind.InvalidIndex, invalidIndex.FailureKind);
        Assert.Null(invalidIndex.Snapshot);
    }

    [Fact]
    public void Voxelizer_ReturnsTypedCancellationWithoutPartialSnapshot()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2MeshVoxelizationResult result = Ra2MeshVoxelizer.ConvertGlb(
            CreateCubeGlb(),
            CreateOptions(16),
            cancellation.Token);

        Assert.Equal(Ra2MeshVoxelizationFailureKind.Cancelled, result.FailureKind);
        Assert.Null(result.Snapshot);
        Assert.Null(result.Facts);
    }

    [Fact]
    public void RealP2Glb_WhenExplicitlyEnabled_ProducesDeterministicBodyCandidate()
    {
        string? glbPath = Environment.GetEnvironmentVariable("RA2INI_VOX_1D_REAL_GLB");
        string? palettePath = Environment.GetEnvironmentVariable("RA2INI_VOX_1D_PALETTE");
        if (string.IsNullOrWhiteSpace(glbPath) || string.IsNullOrWhiteSpace(palettePath))
            return;

        byte[] glb = File.ReadAllBytes(glbPath);
        Ra2Rgb24[] decoded = Ra2VxlseSliceImportContract.DecodeWestwoodPalette(File.ReadAllBytes(palettePath));
        Ra2VoxelPaletteProfile palette = new(
            "unittem-real",
            decoded.Select(colour => new Ra2Rgba32(colour.Red, colour.Green, colour.Blue)),
            [0],
            Enumerable.Range(16, 16).Select(value => checked((byte)value)));
        Ra2MeshVoxelizationOptions options = new(
            "P2_BODY_CANDIDATE",
            "body",
            Ra2VoxelAssemblyPartRole.Body,
            "Body",
            "p2body",
            64,
            1,
            palette,
            targetColour: new Ra2Rgba32(92, 100, 68));

        Stopwatch stopwatch = Stopwatch.StartNew();
        Ra2MeshSnapshot mesh = Ra2GlbMeshReader.Read(glb);
        long parseMilliseconds = stopwatch.ElapsedMilliseconds;
        Assert.Equal(249_567, mesh.Topology.VertexCount);
        Assert.Equal(499_698, mesh.Topology.TriangleCount);
        Assert.True(mesh.Topology.IsWatertightSingleComponent);

        stopwatch.Restart();
        Ra2MeshVoxelizationResult first = Ra2MeshVoxelizer.Convert(mesh, options);
        long firstVoxelizationMilliseconds = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();
        Ra2MeshVoxelizationResult second = Ra2MeshVoxelizer.Convert(mesh, options);
        long secondVoxelizationMilliseconds = stopwatch.ElapsedMilliseconds;
        Assert.True(first.IsSuccess, first.Message);
        Assert.True(second.IsSuccess, second.Message);
        Assert.Equal(first.Snapshot!.CanonicalHash, second.Snapshot!.CanonicalHash);
        Assert.InRange(first.Snapshot.OccupancyCount, 1, Ra2VoxelSceneSnapshot.MaximumOccupancyCount);
        Assert.Equal(64, Math.Max(first.Snapshot.Part.XSize, Math.Max(first.Snapshot.Part.YSize, first.Snapshot.Part.ZSize)));

        byte[] vox = Ra2MagicaVoxelCodec.Write(first.Snapshot);
        using MemoryStream stream = new(vox, writable: false);
        Ra2VoxelSceneSnapshot decodedVox = Ra2MagicaVoxelCodec.Read(
            stream, "P2_VOX", "body", Ra2VoxelAssemblyPartRole.Body, "Body", "p2body");
        Assert.Equal(first.Snapshot.Cells, decodedVox.Cells);
        Ra2VoxelSliceStackRaster raster = Ra2VoxelSliceStackCodec.Export(first.Snapshot, Ra2VxlseSliceDirection.Downward);
        Ra2VoxelSceneSnapshot decodedSlice = Ra2VoxelSliceStackCodec.Import(
            raster, "P2_SLICE", first.Snapshot.Part, first.Snapshot.Palette);
        Assert.Equal(first.Snapshot.Cells, decodedSlice.Cells);

        string? reportDirectory = Environment.GetEnvironmentVariable("RA2INI_VOX_1D_REPORT_DIRECTORY");
        if (!string.IsNullOrWhiteSpace(reportDirectory))
        {
            Directory.CreateDirectory(reportDirectory);
            Ra2VoxelSliceStackRaster vxlseRaster =
                Ra2VoxelSliceStackCodec.ExportForSuppliedVxlseDownward(first.Snapshot);
            Assert.Equal(first.Snapshot.Part.YSize, vxlseRaster.XSize);
            Assert.Equal(first.Snapshot.Part.ZSize, vxlseRaster.YSize);
            Assert.Equal(first.Snapshot.Part.XSize, vxlseRaster.ZSize);
            byte[] png = Ra2PngRgbaCodec.Encode(vxlseRaster);
            File.WriteAllBytes(Path.Combine(reportDirectory, "body-candidate.vox"), vox);
            File.WriteAllBytes(Path.Combine(reportDirectory, "body-slicestack.png"), png);
            Ra2MeshVoxelizationFacts facts = first.Facts!.Value;
            string report = JsonSerializer.Serialize(new
            {
                schemaVersion = 1,
                sourceHash = mesh.SourceHash,
                canonicalHash = first.Snapshot.CanonicalHash,
                dimensions = new { x = first.Snapshot.Part.XSize, y = first.Snapshot.Part.YSize, z = first.Snapshot.Part.ZSize },
                occupancy = first.Snapshot.OccupancyCount,
                surfaceCells = facts.SurfaceCellCount,
                interiorCells = facts.InteriorCellCount,
                paletteIndex = facts.PaletteIndex,
                paletteHash = facts.PaletteHash,
                axisMap = facts.AxisMapId,
                reviewFlags = facts.ReviewFlags.ToString(),
                timingsMs = new { parse = parseMilliseconds, voxelizeFirst = firstVoxelizationMilliseconds, voxelizeSecond = secondVoxelizationMilliseconds },
                vox = new { bytes = vox.Length, sha256 = Convert.ToHexString(SHA256.HashData(vox)) },
                vxlseImport = new
                {
                    direction = "Downward",
                    rasterWidth = vxlseRaster.Width,
                    rasterHeight = vxlseRaster.Height,
                    createEmptySectionDimensions = new { x = vxlseRaster.XSize, y = vxlseRaster.YSize, z = vxlseRaster.ZSize },
                    expectedSavedVxlDimensions = new { x = first.Snapshot.Part.XSize, y = first.Snapshot.Part.YSize, z = first.Snapshot.Part.ZSize }
                },
                sliceStackPng = new { bytes = png.Length, sha256 = Convert.ToHexString(SHA256.HashData(png)) }
            }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(reportDirectory, "acceptance-report.json"), report, new UTF8Encoding(false));
        }
    }

    private static Ra2MeshVoxelizationOptions CreateOptions(int targetLongestDimension) => new(
        "BRIDGE_SCENE",
        "body",
        Ra2VoxelAssemblyPartRole.Body,
        "Body",
        "bridge",
        targetLongestDimension,
        1,
        CreatePalette(),
        paletteIndex: 7);

    private static Ra2VoxelPaletteProfile CreatePalette()
    {
        Ra2Rgba32[] colours = new Ra2Rgba32[256];
        for (int index = 0; index < colours.Length; index++)
            colours[index] = new Ra2Rgba32((byte)index, (byte)(255 - index), (byte)((index * 31) & 255));
        colours[0] = new Ra2Rgba32(0, 0, 0, 0);
        return new Ra2VoxelPaletteProfile("bridge", colours, [0], [16, 17]);
    }

    private static byte[] CreateCubeGlb(
        string nodeProperties = "",
        int mode = 4,
        ushort[]? indices = null)
    {
        float[] positions =
        [
            0, 0, 0,
            1, 0, 0,
            1, 1, 0,
            0, 1, 0,
            0, 0, 1,
            1, 0, 1,
            1, 1, 1,
            0, 1, 1
        ];
        ushort[] actualIndices = indices ?? CubeIndices;
        using MemoryStream binStream = new();
        using (BinaryWriter writer = new(binStream, Encoding.UTF8, leaveOpen: true))
        {
            foreach (float position in positions)
                writer.Write(position);
            foreach (ushort index in actualIndices)
                writer.Write(index);
        }
        while ((binStream.Length & 3) != 0)
            binStream.WriteByte(0);
        byte[] bin = binStream.ToArray();
        int positionBytes = positions.Length * sizeof(float);
        int indexBytes = actualIndices.Length * sizeof(ushort);
        string json = $$"""
        {"asset":{"version":"2.0"},"scene":0,"scenes":[{"nodes":[0]}],"nodes":[{"mesh":0{{nodeProperties}}}],"meshes":[{"primitives":[{"attributes":{"POSITION":0},"indices":1,"mode":{{mode}}}]}],"buffers":[{"byteLength":{{bin.Length}}}],"bufferViews":[{"buffer":0,"byteOffset":0,"byteLength":{{positionBytes}}},{"buffer":0,"byteOffset":{{positionBytes}},"byteLength":{{indexBytes}}}],"accessors":[{"bufferView":0,"componentType":5126,"count":8,"type":"VEC3"},{"bufferView":1,"componentType":5123,"count":{{actualIndices.Length}},"type":"SCALAR"}]}
        """;
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        int paddedJsonLength = (jsonBytes.Length + 3) & ~3;
        Array.Resize(ref jsonBytes, paddedJsonLength);
        for (int index = json.Length; index < jsonBytes.Length; index++)
            jsonBytes[index] = 0x20;

        using MemoryStream output = new();
        using (BinaryWriter writer = new(output, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(0x46546C67u);
            writer.Write(2u);
            writer.Write(checked((uint)(12 + 8 + jsonBytes.Length + 8 + bin.Length)));
            writer.Write(checked((uint)jsonBytes.Length));
            writer.Write(0x4E4F534Au);
            writer.Write(jsonBytes);
            writer.Write(checked((uint)bin.Length));
            writer.Write(0x004E4942u);
            writer.Write(bin);
        }
        return output.ToArray();
    }
}
