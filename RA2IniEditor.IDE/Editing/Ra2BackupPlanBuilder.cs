using System.Globalization;
using System.IO;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2BackupPlanBuilder : IRa2BackupPlanBuilder
{
    private const string BackupRootDirectoryName = "backup";

    public Ra2BackupPlan Build(Ra2EditorSavePlan savePlan, string? projectRoot, DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(savePlan);

        if (string.IsNullOrWhiteSpace(savePlan.FilePath))
        {
            return CannotBackup(
                Ra2BackupPlanStatus.InvalidSourcePath,
                "Cannot build a backup plan because the source file path is empty.");
        }

        string sourcePath;
        try
        {
            sourcePath = Path.GetFullPath(savePlan.FilePath);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return CannotBackup(
                Ra2BackupPlanStatus.InvalidSourcePath,
                $"Cannot build a backup plan because the source file path is invalid: {ex.Message}");
        }

        if (!savePlan.CanSave)
        {
            return CannotBackup(
                Ra2BackupPlanStatus.SavePlanCannotSave,
                "Cannot build a backup plan because the save plan cannot save.",
                sourcePath);
        }

        if (!File.Exists(sourcePath))
        {
            return CannotBackup(
                Ra2BackupPlanStatus.SourceFileMissing,
                "Cannot build a backup plan because the source file does not exist.",
                sourcePath);
        }

        string timestampDirectory = timestamp.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        string backupRoot = ResolveBackupRoot(sourcePath, projectRoot, timestampDirectory);
        string relativePath = ResolveSafeRelativeSourcePath(sourcePath, projectRoot);
        string backupPath = Path.GetFullPath(Path.Combine(backupRoot, relativePath));
        if (!IsPathUnderDirectory(backupPath, backupRoot))
        {
            return CannotBackup(
                Ra2BackupPlanStatus.InvalidBackupPath,
                "Cannot build a backup plan because the backup path escapes the backup directory.",
                sourcePath);
        }

        string backupDirectory = Path.GetDirectoryName(backupPath) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(backupDirectory))
        {
            return CannotBackup(
                Ra2BackupPlanStatus.BackupDirectoryUnavailable,
                "Cannot build a backup plan because the backup directory cannot be resolved.",
                sourcePath);
        }

        return new Ra2BackupPlan(
            sourcePath,
            backupPath,
            backupDirectory,
            Ra2BackupPlanStatus.CanBackup,
            "Backup plan is ready.");
    }

    private static Ra2BackupPlan CannotBackup(
        Ra2BackupPlanStatus status,
        string message,
        string sourceFilePath = "")
        => new(sourceFilePath, string.Empty, string.Empty, status, message);

    private static string ResolveBackupRoot(string sourcePath, string? projectRoot, string timestampDirectory)
    {
        if (!string.IsNullOrWhiteSpace(projectRoot))
        {
            string projectRootPath = Path.GetFullPath(projectRoot);
            if (IsPathUnderDirectory(sourcePath, projectRootPath))
            {
                return Path.GetFullPath(Path.Combine(
                    projectRootPath,
                    BackupRootDirectoryName,
                    timestampDirectory));
            }
        }

        string sourceDirectory = Path.GetDirectoryName(sourcePath) ?? Directory.GetCurrentDirectory();
        return Path.GetFullPath(Path.Combine(sourceDirectory, BackupRootDirectoryName, timestampDirectory));
    }

    private static string ResolveSafeRelativeSourcePath(string sourcePath, string? projectRoot)
    {
        if (!string.IsNullOrWhiteSpace(projectRoot))
        {
            string projectRootPath = Path.GetFullPath(projectRoot);
            if (IsPathUnderDirectory(sourcePath, projectRootPath))
            {
                string relativePath = Path.GetRelativePath(projectRootPath, sourcePath);
                if (!relativePath.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(relativePath))
                    return relativePath;
            }
        }

        return Path.GetFileName(sourcePath);
    }

    private static bool IsPathUnderDirectory(string path, string directoryPath)
    {
        string fullPath = EnsureTrailingSeparator(Path.GetFullPath(path));
        string fullDirectoryPath = EnsureTrailingSeparator(Path.GetFullPath(directoryPath));
        return fullPath.StartsWith(fullDirectoryPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}
