using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelBinaryProbeFailureKind
{
    None = 0,
    InvalidInput,
    UnsupportedSignature,
    Truncated,
    ResourceLimitExceeded,
    InvalidStructure
}

internal sealed record Ra2VxlSectionFacts(
    string Name,
    uint InfoIndex,
    float Scale,
    IReadOnlyList<float> Transform,
    float MinX,
    float MinY,
    float MinZ,
    float MaxX,
    float MaxY,
    float MaxZ,
    byte XSize,
    byte YSize,
    byte ZSize,
    byte NormalType);

internal sealed record Ra2VxlFileFacts(
    long ContentLength,
    uint PaletteCount,
    uint BodyLength,
    IReadOnlyList<Ra2VxlSectionFacts> Sections);

internal sealed record Ra2HvaFileFacts(
    long ContentLength,
    uint FrameCount,
    IReadOnlyList<string> SectionNames,
    bool HasUnnamedSection,
    bool AllTransformsFinite);

internal sealed class Ra2VoxelBinaryProbeResult<TFacts>
    where TFacts : class
{
    private Ra2VoxelBinaryProbeResult(
        Ra2VoxelBinaryProbeFailureKind failureKind,
        string message,
        TFacts? facts)
    {
        bool succeeded = failureKind == Ra2VoxelBinaryProbeFailureKind.None;
        if (succeeded != (facts is not null))
            throw new ArgumentException("Voxel probe payload does not match its failure state.");

        Succeeded = succeeded;
        FailureKind = failureKind;
        Message = string.IsNullOrWhiteSpace(message) ? "Voxel binary probe failed." : message.Trim();
        Facts = facts;
    }

    internal bool Succeeded { get; }
    internal Ra2VoxelBinaryProbeFailureKind FailureKind { get; }
    internal string Message { get; }
    internal TFacts? Facts { get; }

    internal static Ra2VoxelBinaryProbeResult<TFacts> Success(string message, TFacts facts)
        => new(Ra2VoxelBinaryProbeFailureKind.None, message, facts ?? throw new ArgumentNullException(nameof(facts)));

    internal static Ra2VoxelBinaryProbeResult<TFacts> Failure(
        Ra2VoxelBinaryProbeFailureKind failureKind,
        string message)
    {
        if (failureKind == Ra2VoxelBinaryProbeFailureKind.None)
            throw new ArgumentException("A failed voxel probe requires a failure kind.", nameof(failureKind));
        return new(failureKind, message, null);
    }
}

internal static class Ra2VoxelBinaryProbe
{
    internal const long MaximumContentBytes = Ra2AutomationAssetSource.MaximumContentBytes;
    internal const uint MaximumPaletteCount = 16;
    internal const uint MaximumSectionCount = 256;
    internal const uint MaximumFrameCount = 4096;
    internal const long MaximumTransformCount = 65_536;

    private const int VxlHeaderLength = 32;
    private const int VxlPaletteLength = 770;
    private const int VxlSectionHeaderLength = 28;
    private const int VxlSectionInfoLength = 92;
    private const int HvaHeaderLength = 24;
    private const int HvaSectionNameLength = 16;
    private const int HvaTransformLength = 12 * sizeof(float);
    private const string VxlSignature = "Voxel Animation";

    internal static Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts> ProbeVxl(Stream stream)
    {
        if (!TryValidateStream(stream, VxlHeaderLength, out Ra2VoxelBinaryProbeFailureKind failureKind, out string failure))
            return Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts>.Failure(failureKind, failure);

        try
        {
            using BinaryReader reader = new(stream, Encoding.ASCII, leaveOpen: true);
            stream.Position = 0;
            string signature = ReadFixedAscii(reader, 16);
            if (!string.Equals(signature, VxlSignature, StringComparison.OrdinalIgnoreCase))
            {
                return Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts>.Failure(
                    Ra2VoxelBinaryProbeFailureKind.UnsupportedSignature,
                    "The VXL signature is not supported.");
            }

            uint paletteCount = reader.ReadUInt32();
            uint sectionCount = reader.ReadUInt32();
            uint sectionInfoCount = reader.ReadUInt32();
            uint bodyLength = reader.ReadUInt32();
            if (paletteCount is < 1 or > MaximumPaletteCount ||
                sectionCount is < 1 or > MaximumSectionCount ||
                sectionInfoCount is < 1 or > MaximumSectionCount ||
                sectionCount != sectionInfoCount)
            {
                return Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts>.Failure(
                    Ra2VoxelBinaryProbeFailureKind.ResourceLimitExceeded,
                    "The VXL palette or section count is outside the probe limits.");
            }

            long sectionHeadersStart = checked(VxlHeaderLength + (long)paletteCount * VxlPaletteLength);
            long bodyStart = checked(sectionHeadersStart + (long)sectionCount * VxlSectionHeaderLength);
            long sectionInfoStart = checked(bodyStart + bodyLength);
            long requiredLength = checked(sectionInfoStart + (long)sectionInfoCount * VxlSectionInfoLength);
            if (requiredLength > stream.Length)
            {
                return Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts>.Failure(
                    Ra2VoxelBinaryProbeFailureKind.Truncated,
                    "The VXL body or section information extends beyond the input.");
            }

            stream.Position = sectionHeadersStart;
            List<(string Name, uint InfoIndex)> headers = new((int)sectionCount);
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < sectionCount; index++)
            {
                string name = ReadFixedAscii(reader, 16);
                uint infoIndex = reader.ReadUInt32();
                _ = reader.ReadUInt32();
                _ = reader.ReadUInt32();
                if (string.IsNullOrWhiteSpace(name) || infoIndex >= sectionInfoCount || !names.Add(name))
                {
                    return Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts>.Failure(
                        Ra2VoxelBinaryProbeFailureKind.InvalidStructure,
                        "The VXL contains an empty, duplicate, or invalid section header.");
                }
                headers.Add((name, infoIndex));
            }

            stream.Position = sectionInfoStart;
            Ra2VxlSectionFacts?[] infos = new Ra2VxlSectionFacts[sectionInfoCount];
            for (int index = 0; index < sectionInfoCount; index++)
            {
                _ = reader.ReadUInt32();
                _ = reader.ReadUInt32();
                _ = reader.ReadUInt32();
                float scale = reader.ReadSingle();
                float[] transform = new float[12];
                for (int transformIndex = 0; transformIndex < transform.Length; transformIndex++)
                    transform[transformIndex] = reader.ReadSingle();

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
                if (!float.IsFinite(scale) || scale <= 0 ||
                    transform.Any(value => !float.IsFinite(value)) ||
                    !AllFinite(minX, minY, minZ, maxX, maxY, maxZ) ||
                    minX > maxX || minY > maxY || minZ > maxZ ||
                    xSize == 0 || ySize == 0 || zSize == 0)
                {
                    return Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts>.Failure(
                        Ra2VoxelBinaryProbeFailureKind.InvalidStructure,
                        "The VXL section information contains invalid bounds, dimensions, scale, or transform values.");
                }

                infos[index] = new Ra2VxlSectionFacts(
                    string.Empty,
                    (uint)index,
                    scale,
                    Array.AsReadOnly(transform),
                    minX,
                    minY,
                    minZ,
                    maxX,
                    maxY,
                    maxZ,
                    xSize,
                    ySize,
                    zSize,
                    normalType);
            }

            Ra2VxlSectionFacts[] sections = headers.Select(header =>
            {
                Ra2VxlSectionFacts info = infos[header.InfoIndex]!;
                return info with { Name = header.Name, InfoIndex = header.InfoIndex };
            }).ToArray();

            return Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts>.Success(
                "VXL metadata probe completed.",
                new Ra2VxlFileFacts(stream.Length, paletteCount, bodyLength, Array.AsReadOnly(sections)));
        }
        catch (EndOfStreamException)
        {
            return Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts>.Failure(
                Ra2VoxelBinaryProbeFailureKind.Truncated,
                "The VXL input ended before its declared metadata was complete.");
        }
        catch (OverflowException)
        {
            return Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts>.Failure(
                Ra2VoxelBinaryProbeFailureKind.ResourceLimitExceeded,
                "The VXL declared size exceeds the probe limits.");
        }
    }

    internal static Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts> ProbeHva(Stream stream)
    {
        if (!TryValidateStream(stream, HvaHeaderLength, out Ra2VoxelBinaryProbeFailureKind failureKind, out string failure))
            return Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts>.Failure(failureKind, failure);

        try
        {
            using BinaryReader reader = new(stream, Encoding.ASCII, leaveOpen: true);
            stream.Position = 0;
            _ = ReadFixedAscii(reader, 16);
            uint frameCount = reader.ReadUInt32();
            uint sectionCount = reader.ReadUInt32();
            long transformCount = checked((long)frameCount * sectionCount);
            if (frameCount is < 1 or > MaximumFrameCount ||
                sectionCount is < 1 or > MaximumSectionCount ||
                transformCount > MaximumTransformCount)
            {
                return Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts>.Failure(
                    Ra2VoxelBinaryProbeFailureKind.ResourceLimitExceeded,
                    "The HVA frame or section count is outside the probe limits.");
            }

            long transformsStart = checked(HvaHeaderLength + (long)sectionCount * HvaSectionNameLength);
            long requiredLength = checked(transformsStart + transformCount * HvaTransformLength);
            if (requiredLength > stream.Length)
            {
                return Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts>.Failure(
                    Ra2VoxelBinaryProbeFailureKind.Truncated,
                    "The HVA transform data extends beyond the input.");
            }

            List<string> sectionNames = new((int)sectionCount);
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < sectionCount; index++)
            {
                string name = ReadFixedAscii(reader, HvaSectionNameLength);
                if ((string.IsNullOrWhiteSpace(name) && sectionCount != 1) ||
                    (!string.IsNullOrWhiteSpace(name) && !names.Add(name)))
                {
                    return Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts>.Failure(
                        Ra2VoxelBinaryProbeFailureKind.InvalidStructure,
                        "The HVA contains ambiguous unnamed or duplicate section names.");
                }
                sectionNames.Add(name);
            }

            bool allFinite = true;
            for (long transform = 0; transform < transformCount; transform++)
            {
                for (int component = 0; component < 12; component++)
                    allFinite &= float.IsFinite(reader.ReadSingle());
            }
            if (!allFinite)
            {
                return Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts>.Failure(
                    Ra2VoxelBinaryProbeFailureKind.InvalidStructure,
                    "The HVA contains a non-finite transform value.");
            }

            return Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts>.Success(
                "HVA metadata probe completed.",
                new Ra2HvaFileFacts(
                    stream.Length,
                    frameCount,
                    Array.AsReadOnly(sectionNames.ToArray()),
                    HasUnnamedSection: sectionNames.Any(string.IsNullOrWhiteSpace),
                    AllTransformsFinite: true));
        }
        catch (EndOfStreamException)
        {
            return Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts>.Failure(
                Ra2VoxelBinaryProbeFailureKind.Truncated,
                "The HVA input ended before its declared metadata was complete.");
        }
        catch (OverflowException)
        {
            return Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts>.Failure(
                Ra2VoxelBinaryProbeFailureKind.ResourceLimitExceeded,
                "The HVA declared size exceeds the probe limits.");
        }
    }

    private static bool TryValidateStream(
        Stream? stream,
        int minimumLength,
        out Ra2VoxelBinaryProbeFailureKind failureKind,
        out string failure)
    {
        if (stream is null || !stream.CanRead || !stream.CanSeek)
        {
            failureKind = Ra2VoxelBinaryProbeFailureKind.InvalidInput;
            failure = "The voxel probe requires a readable, seekable stream.";
            return false;
        }
        if (stream.Length < minimumLength)
        {
            failureKind = Ra2VoxelBinaryProbeFailureKind.Truncated;
            failure = "The voxel input is shorter than its fixed header.";
            return false;
        }
        if (stream.Length > MaximumContentBytes)
        {
            failureKind = Ra2VoxelBinaryProbeFailureKind.ResourceLimitExceeded;
            failure = "The voxel input exceeds the existing asset content limit.";
            return false;
        }

        failureKind = Ra2VoxelBinaryProbeFailureKind.None;
        failure = string.Empty;
        return true;
    }

    private static string ReadFixedAscii(BinaryReader reader, int length)
    {
        byte[] bytes = reader.ReadBytes(length);
        if (bytes.Length != length)
            throw new EndOfStreamException();
        int terminator = Array.IndexOf(bytes, (byte)0);
        return Encoding.ASCII.GetString(bytes, 0, terminator >= 0 ? terminator : bytes.Length).Trim();
    }

    private static bool AllFinite(params float[] values) => values.All(float.IsFinite);
}
