using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2TextFirstSaveBoundaryGuardrailTests
{
    [Fact]
    public void SavePlanBuilder_DoesNotReferenceDiskIoLegacySaveOrProjectServices()
    {
        string root = TestRepositoryRoot.Find();
        string builderText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditorSavePlanBuilder.cs"));
        string planText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditorSavePlan.cs"));
        string combinedText = builderText + Environment.NewLine + planText;

        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IIniFileStore", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CreateBackup", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeProject_DoesNotChangeCoreOrInfrastructurePublicApiForSaveBoundary()
    {
        string root = TestRepositoryRoot.Find();
        string coreText = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(Path.Combine(root, "RA2IniEditor.Core"), "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));
        string infrastructureText = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(Path.Combine(root, "RA2IniEditor.Infrastructure"), "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("Ra2EditorSavePlan", coreText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2EditorSavePlan", infrastructureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2EditorStateViewModel", coreText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2EditorStateViewModel", infrastructureText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EncodingMetadataContract_DoesNotReferenceStorageAdaptersOrDiskSave()
    {
        string root = TestRepositoryRoot.Find();
        string[] files =
        [
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditorTextEncodingKind.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditorTextEncodingMetadata.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditorTextEncodingMetadataProvider.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditorNewLineSavePolicy.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditorNewLinePolicyProvider.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2SaveCurrentFilePlanStatus.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2SaveCurrentFilePlanRequest.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "IRa2SaveCurrentFilePlanBuilder.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2SaveCurrentFilePlanBuilder.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditableDocumentState.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditableDocumentSessionService.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2TextChangeApplier.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditorSavePlan.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2EditorSavePlanBuilder.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Services", "Ra2EditorEncodingMetadataAdapter.cs")
        ];
        string combinedText = string.Join(Environment.NewLine, files.Select(File.ReadAllText));

        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IIniFileStore", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BackupBeforeSaveFoundation_DoesNotCallSaveChainOrWriteCurrentText()
    {
        string root = TestRepositoryRoot.Find();
        string[] files =
        [
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2BackupPlanStatus.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2BackupPlan.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "IRa2BackupPlanBuilder.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2BackupPlanBuilder.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "IRa2BackupService.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2BackupResult.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Editing", "Ra2BackupService.cs")
        ];
        string combinedText = string.Join(Environment.NewLine, files.Select(File.ReadAllText));

        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IIniFileStore", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CurrentText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("File.Copy", combinedText, StringComparison.OrdinalIgnoreCase);
    }
}

