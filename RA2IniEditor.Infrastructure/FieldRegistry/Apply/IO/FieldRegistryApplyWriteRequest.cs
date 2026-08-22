namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;

internal sealed class FieldRegistryApplyWriteRequest
{
    public const string DefaultTargetPackFileName = "user-import.fields.json";

    public FieldRegistryApplyWriteRequest(
        FieldRegistryApplyPlan plan,
        string? projectRootPath,
        string globalFieldRegistryRootPath,
        string? targetPackFileName = null,
        DateTimeOffset? timestamp = null)
    {
        Plan = plan ?? throw new ArgumentNullException(nameof(plan));
        ProjectRootPath = string.IsNullOrWhiteSpace(projectRootPath) ? null : projectRootPath;
        GlobalFieldRegistryRootPath = string.IsNullOrWhiteSpace(globalFieldRegistryRootPath)
            ? throw new ArgumentException("Global field registry root path cannot be empty.", nameof(globalFieldRegistryRootPath))
            : globalFieldRegistryRootPath;
        TargetPackFileName = string.IsNullOrWhiteSpace(targetPackFileName)
            ? DefaultTargetPackFileName
            : targetPackFileName;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    public FieldRegistryApplyPlan Plan { get; }

    public string? ProjectRootPath { get; }

    public string GlobalFieldRegistryRootPath { get; }

    public string TargetPackFileName { get; }

    public DateTimeOffset Timestamp { get; }
}
