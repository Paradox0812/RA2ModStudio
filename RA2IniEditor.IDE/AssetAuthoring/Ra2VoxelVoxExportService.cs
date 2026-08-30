extern alias Ra2Application;

using System.IO;
using System.Security.Cryptography;
using Ra2MagicaVoxelCodec = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2MagicaVoxelCodec;
using Ra2VoxelSceneSnapshot = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSceneSnapshot;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelVoxExportFailureKind
{
    None = 0,
    InvalidTarget,
    SourceOverwriteRejected,
    TargetExists,
    TargetRejected,
    EncodeFailed,
    TemporaryWriteFailed,
    RoundTripFailed,
    CommitFailed,
    Canceled
}

internal sealed record Ra2VoxelVoxExportResult(
    Ra2VoxelVoxExportFailureKind FailureKind,
    string Message,
    string? TargetPath,
    int ByteCount,
    string? ContentHash)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelVoxExportFailureKind.None &&
        TargetPath is not null && ByteCount > 0 && ContentHash is not null;
}

/// <summary>
/// Explicit Save-As materializer for one accepted session candidate. It does not participate in
/// project Apply/Save and publishes only after a same-directory round-trip verification succeeds.
/// </summary>
internal sealed class Ra2VoxelVoxExportService
{
    internal Ra2VoxelVoxExportResult Export(
        Ra2VoxelAcceptedCandidate candidate,
        string targetPath,
        string? currentSourcePath,
        bool overwriteExisting,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        string normalizedTarget;
        try
        {
            if (string.IsNullOrWhiteSpace(targetPath) ||
                !string.Equals(Path.GetExtension(targetPath), ".vox", StringComparison.OrdinalIgnoreCase))
            {
                return Failure(Ra2VoxelVoxExportFailureKind.InvalidTarget, "请选择以 .vox 结尾的导出文件。");
            }
            normalizedTarget = Path.GetFullPath(targetPath);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Failure(Ra2VoxelVoxExportFailureKind.InvalidTarget, "导出路径无效。");
        }

        if (!string.IsNullOrWhiteSpace(currentSourcePath) && PathsEqual(normalizedTarget, currentSourcePath))
        {
            return Failure(
                Ra2VoxelVoxExportFailureKind.SourceOverwriteRejected,
                "本阶段只支持导出副本，不能覆盖当前载入的源 VOX。",
                normalizedTarget);
        }

        string? directory = Path.GetDirectoryName(normalizedTarget);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory) || Directory.Exists(normalizedTarget))
            return Failure(Ra2VoxelVoxExportFailureKind.InvalidTarget, "请选择一个已存在目录中的 VOX 文件名。", normalizedTarget);
        if (File.Exists(normalizedTarget))
        {
            if (!overwriteExisting)
                return Failure(Ra2VoxelVoxExportFailureKind.TargetExists, "目标文件已存在，需要明确确认覆盖。", normalizedTarget);
            try
            {
                if ((File.GetAttributes(normalizedTarget) & FileAttributes.ReparsePoint) != 0)
                    return Failure(Ra2VoxelVoxExportFailureKind.TargetRejected, "不能覆盖重解析点形式的目标文件。", normalizedTarget);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return Failure(Ra2VoxelVoxExportFailureKind.TargetRejected, "无法安全检查现有目标文件。", normalizedTarget);
            }
        }

        byte[] encoded;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            encoded = Ra2MagicaVoxelCodec.Write(candidate.Snapshot);
        }
        catch (OperationCanceledException)
        {
            return Failure(Ra2VoxelVoxExportFailureKind.Canceled, "VOX 导出已取消。", normalizedTarget);
        }
        catch (Exception exception) when (exception is InvalidDataException or ArgumentException or OverflowException or InvalidOperationException)
        {
            return Failure(Ra2VoxelVoxExportFailureKind.EncodeFailed, "最终候选无法编码为受支持的 MagicaVoxel VOX。", normalizedTarget);
        }

        string temporaryPath = Path.Combine(directory, $".{Path.GetFileName(normalizedTarget)}.{Guid.NewGuid():N}.tmp");
        try
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                using FileStream stream = new(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    64 * 1024,
                    FileOptions.WriteThrough);
                stream.Write(encoded);
                stream.Flush(flushToDisk: true);
            }
            catch (OperationCanceledException)
            {
                return Failure(Ra2VoxelVoxExportFailureKind.Canceled, "VOX 导出已取消。", normalizedTarget);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                return Failure(Ra2VoxelVoxExportFailureKind.TemporaryWriteFailed, "无法在目标目录写入临时 VOX 文件。", normalizedTarget);
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                using FileStream stream = new(temporaryPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                Ra2VoxelSceneSnapshot decoded = Ra2MagicaVoxelCodec.Read(
                    stream,
                    candidate.Snapshot.SceneId,
                    candidate.Snapshot.Part.PartId,
                    candidate.Snapshot.Part.Role,
                    candidate.Snapshot.Part.VxlSectionName,
                    candidate.Snapshot.Part.StableFileStem);
                byte[] roundTrip = Ra2MagicaVoxelCodec.Write(decoded);
                if (!encoded.AsSpan().SequenceEqual(roundTrip))
                    return Failure(Ra2VoxelVoxExportFailureKind.RoundTripFailed, "临时 VOX 未通过确定性回读验证。", normalizedTarget);
            }
            catch (OperationCanceledException)
            {
                return Failure(Ra2VoxelVoxExportFailureKind.Canceled, "VOX 导出已取消。", normalizedTarget);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException or
                ArgumentException or OverflowException or InvalidOperationException)
            {
                return Failure(Ra2VoxelVoxExportFailureKind.RoundTripFailed, "临时 VOX 未通过确定性回读验证。", normalizedTarget);
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (File.Exists(normalizedTarget))
                {
                    if (!overwriteExisting)
                        return Failure(Ra2VoxelVoxExportFailureKind.TargetExists, "目标文件已存在，需要明确确认覆盖。", normalizedTarget);
                    if ((File.GetAttributes(normalizedTarget) & FileAttributes.ReparsePoint) != 0)
                        return Failure(Ra2VoxelVoxExportFailureKind.TargetRejected, "不能覆盖重解析点形式的目标文件。", normalizedTarget);
                    File.Replace(temporaryPath, normalizedTarget, null, ignoreMetadataErrors: true);
                }
                else
                {
                    File.Move(temporaryPath, normalizedTarget);
                }
            }
            catch (OperationCanceledException)
            {
                return Failure(Ra2VoxelVoxExportFailureKind.Canceled, "VOX 导出已取消。", normalizedTarget);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or PlatformNotSupportedException or NotSupportedException)
            {
                return Failure(Ra2VoxelVoxExportFailureKind.CommitFailed, "已验证的临时 VOX 无法原子发布到目标位置。", normalizedTarget);
            }

            return new(
                Ra2VoxelVoxExportFailureKind.None,
                "VOX 已导出并通过回读验证。",
                normalizedTarget,
                encoded.Length,
                Convert.ToHexString(SHA256.HashData(encoded)));
        }
        finally
        {
            TryDeleteTemporary(temporaryPath);
        }
    }

    private static Ra2VoxelVoxExportResult Failure(
        Ra2VoxelVoxExportFailureKind kind,
        string message,
        string? targetPath = null) =>
        new(kind, message, targetPath, 0, null);

    private static void TryDeleteTemporary(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Cleanup failure must not replace the authoritative export result.
        }
    }

    private static bool PathsEqual(string first, string second)
    {
        try
        {
            return string.Equals(Path.GetFullPath(first), Path.GetFullPath(second), PathComparison);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
