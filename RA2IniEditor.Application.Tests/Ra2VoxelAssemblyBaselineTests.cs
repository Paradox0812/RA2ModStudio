using System.Text;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelAssemblyBaselineTests
{
    [Fact]
    public void AssemblySpec_AcceptsSeparatedBodyTurretAndBarrelGraph()
    {
        Ra2VoxelAssetAssemblySpec assembly = CreateAssembly();

        Assert.Equal("TEST_TANK", assembly.AssemblyId);
        Assert.Equal(3, assembly.Parts.Count);
        Assert.Equal("body", assembly.Parts[1].ParentPartId);
        Assert.Equal("turret", assembly.Parts[2].ParentPartId);
        Assert.Equal("testbarl.hva", assembly.Parts[2].HvaFileName);
    }

    [Fact]
    public void AssemblySpec_RejectsDisconnectedOrCyclicParts()
    {
        Assert.Throws<ArgumentException>(() => new Ra2VoxelAssetAssemblySpec(
            "BROKEN",
            [
                new("body", Ra2VoxelAssemblyPartRole.Body, "broken", "Body", null, true),
                new("turret", Ra2VoxelAssemblyPartRole.Turret, "brokentur", "Body", "barrel", true),
                new("barrel", Ra2VoxelAssemblyPartRole.Barrel, "brokenbarl", "Body", "turret", true)
            ]));
    }

    [Fact]
    public void BinaryProbe_ReadsBoundedVxlAndHvaMetadata()
    {
        using MemoryStream vxl = new(CreateVxl("Body", 9, 7, 5), writable: false);
        using MemoryStream hva = new(CreateHva("Body", frameCount: 2), writable: false);

        Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts> vxlResult = Ra2VoxelBinaryProbe.ProbeVxl(vxl);
        Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts> hvaResult = Ra2VoxelBinaryProbe.ProbeHva(hva);

        Assert.True(vxlResult.Succeeded);
        Assert.Equal("Body", Assert.Single(vxlResult.Facts!.Sections).Name);
        Assert.Equal((byte)9, vxlResult.Facts.Sections[0].XSize);
        Assert.Equal((byte)7, vxlResult.Facts.Sections[0].YSize);
        Assert.Equal((byte)5, vxlResult.Facts.Sections[0].ZSize);
        Assert.True(hvaResult.Succeeded);
        Assert.Equal((uint)2, hvaResult.Facts!.FrameCount);
        Assert.Equal(["Body"], hvaResult.Facts.SectionNames);
        Assert.False(hvaResult.Facts.HasUnnamedSection);
        Assert.True(hvaResult.Facts.AllTransformsFinite);
    }

    [Fact]
    public void AssemblyProbe_AcceptsSingleUnnamedLegacyHvaWhenVxlHasOneExpectedSection()
    {
        Ra2VoxelAssetAssemblySpec assembly = new(
            "LEGACY_BODY",
            [new("body", Ra2VoxelAssemblyPartRole.Body, "legacy", "Body", null, true)]);
        Dictionary<string, byte[]> artifacts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["legacy.vxl"] = CreateVxl("Body", 4, 4, 4),
            ["legacy.hva"] = CreateHva(string.Empty)
        };

        Ra2VoxelAssemblyProbeResult result = Ra2VoxelAssemblyProbe.Probe(assembly, artifacts);

        Assert.True(result.Succeeded);
        Assert.True(Assert.Single(result.Parts).Hva!.HasUnnamedSection);
    }

    [Fact]
    public void BinaryProbe_RejectsTruncatedAndNonFiniteInputsWithoutThrowing()
    {
        using MemoryStream truncated = new(new byte[31], writable: false);
        byte[] invalidHva = CreateHva("Body", frameCount: 1);
        WriteSingle(invalidHva, invalidHva.Length - sizeof(float), float.NaN);
        using MemoryStream nonFinite = new(invalidHva, writable: false);

        Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts> vxlResult = Ra2VoxelBinaryProbe.ProbeVxl(truncated);
        Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts> hvaResult = Ra2VoxelBinaryProbe.ProbeHva(nonFinite);

        Assert.False(vxlResult.Succeeded);
        Assert.Equal(Ra2VoxelBinaryProbeFailureKind.Truncated, vxlResult.FailureKind);
        Assert.False(hvaResult.Succeeded);
        Assert.Equal(Ra2VoxelBinaryProbeFailureKind.InvalidStructure, hvaResult.FailureKind);
    }

    [Fact]
    public void AssemblyProbe_ClosesSeparatedBodyTurretAndBarrelArtifacts()
    {
        Ra2VoxelAssetAssemblySpec assembly = CreateAssembly();
        Dictionary<string, byte[]> artifacts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["test.vxl"] = CreateVxl("Body", 12, 8, 5),
            ["test.hva"] = CreateHva("Body"),
            ["testtur.vxl"] = CreateVxl("Body", 6, 6, 3),
            ["testtur.hva"] = CreateHva("Body", frameCount: 8),
            ["testbarl.vxl"] = CreateVxl("Body", 8, 2, 2),
            ["testbarl.hva"] = CreateHva("Body", frameCount: 8)
        };

        Ra2VoxelAssemblyProbeResult result = Ra2VoxelAssemblyProbe.Probe(assembly, artifacts);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Parts.Count);
        Assert.Equal((uint)8, result.Parts.Single(part => part.Role == Ra2VoxelAssemblyPartRole.Turret).Hva!.FrameCount);
        Assert.Equal("turret", result.Parts.Single(part => part.Role == Ra2VoxelAssemblyPartRole.Barrel).ParentPartId);
    }

    [Fact]
    public void AssemblyProbe_RejectsMissingHvaWithoutPartialFacts()
    {
        Ra2VoxelAssetAssemblySpec assembly = CreateAssembly();
        Dictionary<string, byte[]> artifacts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["test.vxl"] = CreateVxl("Body", 12, 8, 5),
            ["test.hva"] = CreateHva("Body"),
            ["testtur.vxl"] = CreateVxl("Body", 6, 6, 3),
            ["testtur.hva"] = CreateHva("Body"),
            ["testbarl.vxl"] = CreateVxl("Body", 8, 2, 2)
        };

        Ra2VoxelAssemblyProbeResult result = Ra2VoxelAssemblyProbe.Probe(assembly, artifacts);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2VoxelAssemblyProbeFailureKind.MissingArtifact, result.FailureKind);
        Assert.Equal("barrel", result.FailedPartId);
        Assert.Empty(result.Parts);
    }

    [Fact]
    public void AssemblyProbe_RejectsHvaSectionMismatchWithoutPartialFacts()
    {
        Ra2VoxelAssetAssemblySpec assembly = new(
            "BODY_ONLY",
            [new("body", Ra2VoxelAssemblyPartRole.Body, "body", "Body", null, true)]);
        Dictionary<string, byte[]> artifacts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["body.vxl"] = CreateVxl("Body", 4, 4, 4),
            ["body.hva"] = CreateHva("Turret")
        };

        Ra2VoxelAssemblyProbeResult result = Ra2VoxelAssemblyProbe.Probe(assembly, artifacts);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2VoxelAssemblyProbeFailureKind.SectionMismatch, result.FailureKind);
        Assert.Empty(result.Parts);
    }

    [Fact]
    public void AssemblyProbe_RejectsUnexpectedAndCaseAmbiguousArtifacts()
    {
        Ra2VoxelAssetAssemblySpec assembly = new(
            "BODY_ONLY",
            [new("body", Ra2VoxelAssemblyPartRole.Body, "body", "Body", null, false)]);
        Dictionary<string, byte[]> unexpected = new(StringComparer.Ordinal)
        {
            ["body.vxl"] = CreateVxl("Body", 4, 4, 4),
            ["unused.vxl"] = CreateVxl("Body", 4, 4, 4)
        };
        Dictionary<string, byte[]> ambiguous = new(StringComparer.Ordinal)
        {
            ["body.vxl"] = CreateVxl("Body", 4, 4, 4),
            ["BODY.VXL"] = CreateVxl("Body", 4, 4, 4)
        };

        Ra2VoxelAssemblyProbeResult unexpectedResult = Ra2VoxelAssemblyProbe.Probe(assembly, unexpected);
        Ra2VoxelAssemblyProbeResult ambiguousResult = Ra2VoxelAssemblyProbe.Probe(assembly, ambiguous);

        Assert.Equal(Ra2VoxelAssemblyProbeFailureKind.UnexpectedArtifact, unexpectedResult.FailureKind);
        Assert.Empty(unexpectedResult.Parts);
        Assert.Equal(Ra2VoxelAssemblyProbeFailureKind.AmbiguousArtifactIdentity, ambiguousResult.FailureKind);
        Assert.Empty(ambiguousResult.Parts);
    }

    private static Ra2VoxelAssetAssemblySpec CreateAssembly() => new(
        "TEST_TANK",
        [
            new("body", Ra2VoxelAssemblyPartRole.Body, "test", "Body", null, true),
            new("turret", Ra2VoxelAssemblyPartRole.Turret, "testtur", "Body", "body", true),
            new("barrel", Ra2VoxelAssemblyPartRole.Barrel, "testbarl", "Body", "turret", true)
        ]);

    private static byte[] CreateVxl(string sectionName, byte xSize, byte ySize, byte zSize)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.ASCII, leaveOpen: true);
        WriteFixedAscii(writer, "Voxel Animation", 16);
        writer.Write((uint)1);
        writer.Write((uint)1);
        writer.Write((uint)1);
        writer.Write((uint)0);
        writer.Write((byte)16);
        writer.Write((byte)31);
        writer.Write(new byte[256 * 3]);
        WriteFixedAscii(writer, sectionName, 16);
        writer.Write((uint)0);
        writer.Write((uint)0);
        writer.Write((uint)0);
        writer.Write((uint)0);
        writer.Write((uint)0);
        writer.Write((uint)0);
        writer.Write(1f / 12f);
        for (int index = 0; index < 12; index++)
            writer.Write(index is 0 or 5 or 10 ? 1f : 0f);
        writer.Write(0f);
        writer.Write(0f);
        writer.Write(0f);
        writer.Write((float)xSize);
        writer.Write((float)ySize);
        writer.Write((float)zSize);
        writer.Write(xSize);
        writer.Write(ySize);
        writer.Write(zSize);
        writer.Write((byte)4);
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] CreateHva(string sectionName, uint frameCount = 1)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.ASCII, leaveOpen: true);
        WriteFixedAscii(writer, string.Empty, 16);
        writer.Write(frameCount);
        writer.Write((uint)1);
        WriteFixedAscii(writer, sectionName, 16);
        for (int frame = 0; frame < frameCount; frame++)
        {
            for (int index = 0; index < 12; index++)
                writer.Write(index is 0 or 5 or 10 ? 1f : 0f);
        }
        writer.Flush();
        return stream.ToArray();
    }

    private static void WriteFixedAscii(BinaryWriter writer, string value, int length)
    {
        byte[] output = new byte[length];
        byte[] encoded = Encoding.ASCII.GetBytes(value);
        Array.Copy(encoded, output, Math.Min(encoded.Length, output.Length));
        writer.Write(output);
    }

    private static void WriteSingle(byte[] buffer, int offset, float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Copy(bytes, 0, buffer, offset, bytes.Length);
    }
}
