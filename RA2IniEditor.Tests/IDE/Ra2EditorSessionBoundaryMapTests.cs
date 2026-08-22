using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorSessionBoundaryMapTests
{
    [Fact]
    public void BoundaryMap_DocumentsEditorSessionResponsibilitiesAndGuardrails()
    {
        string document = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "Docs",
            "RA2IniEditor_IDE_EditorSession_Boundary_Map_v0.4.54.md"));

        Assert.Contains("EnterEditMode_OnClick", document);
        Assert.Contains("RevertInMemoryChanges_OnClick", document);
        Assert.Contains("SourceTextEditor_OnTextChanged", document);
        Assert.Contains("SetEditorTextFromProgram", document);
        Assert.Contains("ResetEditableSessionToReadOnly", document);
        Assert.Contains("UpdateEditorStateControls", document);
        Assert.Contains("programmatic sync", document, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dirty", document, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Annotation sidecar edits", document);
        Assert.Contains("Completion commit", document);
        Assert.Contains("Add Property insert/replace", document);
        Assert.Contains("No Save / Save All", document);
        Assert.Contains("No full EditorSessionController extraction", document);
    }

    [Fact]
    public void OperationModels_DoNotDependOnWpfAvalonEditOrSaveChain()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Controllers",
            "EditorSession",
            "Ra2EditorSessionOperationModels.cs"));

        Assert.DoesNotContain("System.Windows", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AvalonEdit", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TextEditor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IniFileService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.Write", source, StringComparison.Ordinal);
    }
}

