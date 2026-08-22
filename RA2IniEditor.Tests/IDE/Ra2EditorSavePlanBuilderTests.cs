using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorSavePlanBuilderTests
{
    private readonly Ra2EditorSavePlanBuilder _builder = new();

    [Fact]
    public void Build_DirtySessionCreatesSavePreviewPlanFromCurrentText()
    {
        const string originalText = "[E1]\r\nStrength=100";
        const string currentText = "[E1]\r\nStrength=125";
        Ra2EditorTextEncodingMetadata metadata = new(
            Ra2EditorTextEncodingKind.Utf8Bom,
            "UTF-8 BOM",
            hasBom: true);
        Ra2EditableDocumentSession session = CreateSession(
            "rulesmd.ini",
            originalText,
            currentText,
            Ra2EditorDocumentState.EditableDirty,
            metadata);

        Ra2EditorSavePlan plan = _builder.Build(session);

        Assert.True(plan.CanSave);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.CanSave, plan.Status);
        Assert.Equal("rulesmd.ini", plan.FilePath);
        Assert.Equal(currentText, plan.Text);
        Assert.Equal(Ra2IniNewLineKind.CrLf, plan.NewLineKind);
        Assert.Equal(Ra2EditorNewLineSavePolicy.PreserveCurrentText, plan.NewLinePolicy);
        Assert.Same(metadata, plan.EncodingMetadata);
        Assert.False(string.IsNullOrWhiteSpace(plan.Message));
        Assert.Contains("backup", plan.BackupPlanPreview, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Save is not implemented yet", plan.Reason);
    }

    [Fact]
    public void Build_CleanSessionCannotSave()
    {
        Ra2EditableDocumentSession session = CreateSession(
            "rulesmd.ini",
            "[E1]\nStrength=100",
            "[E1]\nStrength=100",
            Ra2EditorDocumentState.EditableClean);

        Ra2EditorSavePlan plan = _builder.Build(session);

        Assert.False(plan.CanSave);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.NotDirty, plan.Status);
        Assert.Equal("No unsaved in-memory changes.", plan.Reason);
    }

    [Fact]
    public void Build_ReadOnlyPreviewSessionCannotSave()
    {
        Ra2EditableDocumentSession session = CreateSession(
            "rulesmd.ini",
            "[E1]\nStrength=100",
            "[E1]\nStrength=100",
            Ra2EditorDocumentState.ReadOnlyPreview);

        Ra2EditorSavePlan plan = _builder.Build(session);

        Assert.False(plan.CanSave);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.ReadOnlyPreview, plan.Status);
        Assert.Contains("read-only preview", plan.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_EmptyFilePathCannotSaveEvenWhenDirty()
    {
        Ra2EditableDocumentSession session = CreateSession(
            string.Empty,
            "[E1]\nStrength=100",
            "[E1]\nStrength=125",
            Ra2EditorDocumentState.EditableDirty);

        Ra2EditorSavePlan plan = _builder.Build(session);

        Assert.False(plan.CanSave);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.MissingFilePath, plan.Status);
        Assert.Contains("no file path", plan.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_UnknownEncodingDoesNotBlockDryRunPlan()
    {
        Ra2EditableDocumentSession session = CreateSession(
            "rulesmd.ini",
            "[E1]\nStrength=100",
            "[E1]\nStrength=125",
            Ra2EditorDocumentState.EditableDirty);

        Ra2EditorSavePlan plan = _builder.Build(session);

        Assert.True(plan.CanSave);
        Assert.Same(Ra2EditorTextEncodingMetadata.Unknown, plan.EncodingMetadata);
    }

    [Theory]
    [InlineData("[E1]\r\nStrength=100\r\n", (int)Ra2IniNewLineKind.CrLf)]
    [InlineData("[E1]\nStrength=100\n", (int)Ra2IniNewLineKind.Lf)]
    [InlineData("[E1]\rStrength=100\r", (int)Ra2IniNewLineKind.Cr)]
    [InlineData("[E1]\r\nStrength=100\n", (int)Ra2IniNewLineKind.Mixed)]
    [InlineData("[E1]", (int)Ra2IniNewLineKind.Unknown)]
    public void Build_CarriesTextDocumentNewLineKind(string currentText, int expected)
    {
        Ra2EditableDocumentSession session = CreateSession(
            "rulesmd.ini",
            currentText,
            currentText + "Name=Test",
            Ra2EditorDocumentState.EditableDirty);

        Ra2EditorSavePlan plan = _builder.Build(session);

        Assert.Equal((Ra2IniNewLineKind)expected, plan.NewLineKind);
        Assert.Equal(Ra2EditorNewLineSavePolicy.PreserveCurrentText, plan.NewLinePolicy);
    }

    private static Ra2EditableDocumentSession CreateSession(
        string filePath,
        string originalText,
        string currentText,
        Ra2EditorDocumentState state,
        Ra2EditorTextEncodingMetadata? encodingMetadata = null)
    {
        Ra2EditableDocumentState documentState = new(
            filePath,
            originalText,
            currentText,
            state,
            encodingMetadata);
        return new Ra2EditableDocumentSession(
            documentState,
            new Ra2IniTextDocumentParser().Parse(currentText));
    }
}
