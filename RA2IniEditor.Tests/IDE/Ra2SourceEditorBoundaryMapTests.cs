using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SourceEditorBoundaryMapTests
{
    [Fact]
    public void BoundaryMap_DocumentsSourceEditorResponsibilitiesAndGuardrails()
    {
        string document = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "Docs",
            "RA2IniEditor_IDE_SourceEditor_Boundary_Map_v0.4.57.md"));

        Assert.Contains("SetEditorTextFromProgram", document);
        Assert.Contains("_isSynchronizingEditorText", document);
        Assert.Contains("AvalonEdit", document);
        Assert.Contains("TextChanged", document);
        Assert.Contains("Completion commit", document);
        Assert.Contains("Add Property insert / replace", document);
        Assert.Contains("Revert", document);
        Assert.Contains("caret", document, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("focus", document, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ShellWindow", document);
        Assert.Contains("No Save / Save All", document);
        Assert.Contains("does not extract a runtime `Ra2SourceEditorController`", document);
    }

    [Fact]
    public void SyncPlanModel_DoesNotDependOnWpfAvalonEditOrSaveChain()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Controllers",
            "SourceEditor",
            "Ra2SourceEditorSyncPlan.cs"));

        Assert.DoesNotContain("System.Windows", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AvalonEdit", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TextEditor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IniFileService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.Write", source, StringComparison.Ordinal);
    }
}

