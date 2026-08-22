using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryApplyPathResolverTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 5, 25, 12, 34, 56, TimeSpan.Zero);

    [Fact]
    public void ResolveProjectTargetPaths()
    {
        string projectRoot = Path.Combine(Path.GetTempPath(), "project");
        FieldRegistryApplyPathResolver resolver = new();

        string targetPath = resolver.ResolveTargetPackPath(
            FieldRegistryApplyTargetScope.Project,
            projectRoot,
            "unused-global",
            "user-import.fields.json");
        string backupPath = resolver.ResolveBackupDirectory(
            FieldRegistryApplyTargetScope.Project,
            projectRoot,
            "unused-global",
            Timestamp);

        Assert.Equal(Path.Combine(projectRoot, ".ra2inieditor", "field-registry", "active", "user-import.fields.json"), targetPath);
        Assert.Equal(Path.Combine(projectRoot, ".ra2inieditor", "field-registry", "backups", "20260525-123456"), backupPath);
    }

    [Fact]
    public void ResolveGlobalTargetPaths()
    {
        string globalRoot = Path.Combine(Path.GetTempPath(), "global", "FieldRegistry");
        FieldRegistryApplyPathResolver resolver = new();

        string targetPath = resolver.ResolveTargetPackPath(
            FieldRegistryApplyTargetScope.Global,
            null,
            globalRoot,
            "user-import.fields.json");
        string backupPath = resolver.ResolveBackupDirectory(
            FieldRegistryApplyTargetScope.Global,
            null,
            globalRoot,
            Timestamp);

        Assert.Equal(Path.Combine(globalRoot, "active", "user-import.fields.json"), targetPath);
        Assert.Equal(Path.Combine(globalRoot, "backups", "20260525-123456"), backupPath);
    }
}
