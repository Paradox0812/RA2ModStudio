using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SaveCurrentFilePlanBuilderTests
{
    private readonly Ra2SaveCurrentFilePlanBuilder _builder = new();

    [Fact]
    public void BuildDryRun_DirtyEditableSessionCanSave()
    {
        Ra2EditorTextEncodingMetadata metadata = new(Ra2EditorTextEncodingKind.Utf8Bom, "UTF-8 BOM", true);
        Ra2EditableDocumentSession session = CreateSession(
            "rules.ini",
            "[E1]\r\nStrength=100\r\n",
            "[E1]\r\nStrength=125\r\n",
            Ra2EditorDocumentState.EditableDirty,
            metadata);

        Ra2EditorSavePlan plan = _builder.BuildDryRun(new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false));

        Assert.True(plan.CanSave);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.CanSave, plan.Status);
        Assert.Equal("rules.ini", plan.FilePath);
        Assert.Equal("[E1]\r\nStrength=125\r\n", plan.Text);
        Assert.Same(metadata, plan.EncodingMetadata);
        Assert.Equal(Ra2IniNewLineKind.CrLf, plan.NewLineKind);
        Assert.Equal(Ra2EditorNewLineSavePolicy.PreserveCurrentText, plan.NewLinePolicy);
        Assert.False(string.IsNullOrWhiteSpace(plan.Message));
        Assert.Contains("backup", plan.BackupPlanPreview, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildDryRun_NoSessionCannotSave()
    {
        Ra2EditorSavePlan plan = _builder.BuildDryRun(new Ra2SaveCurrentFilePlanRequest(null, isReadOnlyPreview: false));

        Assert.False(plan.CanSave);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.NoEditableSession, plan.Status);
        Assert.Equal(string.Empty, plan.Text);
        Assert.Equal(Ra2IniNewLineKind.Unknown, plan.NewLineKind);
        Assert.Same(Ra2EditorTextEncodingMetadata.Unknown, plan.EncodingMetadata);
        Assert.Contains("no editable session", plan.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildDryRun_ReadOnlyPreviewCannotSave()
    {
        Ra2EditableDocumentSession session = CreateSession(
            "rules.ini",
            "[E1]\n",
            "[E1]\n",
            Ra2EditorDocumentState.ReadOnlyPreview);

        Ra2EditorSavePlan plan = _builder.BuildDryRun(new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: true));

        Assert.False(plan.CanSave);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.ReadOnlyPreview, plan.Status);
        Assert.Contains("read-only", plan.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildDryRun_CleanSessionCannotSave()
    {
        Ra2EditableDocumentSession session = CreateSession(
            "rules.ini",
            "[E1]\n",
            "[E1]\n",
            Ra2EditorDocumentState.EditableClean);

        Ra2EditorSavePlan plan = _builder.BuildDryRun(new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false));

        Assert.False(plan.CanSave);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.NotDirty, plan.Status);
        Assert.Contains("no unsaved", plan.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildDryRun_MissingFilePathCannotSave()
    {
        Ra2EditableDocumentSession session = CreateSession(
            string.Empty,
            "[E1]\n",
            "[E1]\nStrength=125\n",
            Ra2EditorDocumentState.EditableDirty);

        Ra2EditorSavePlan plan = _builder.BuildDryRun(new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false));

        Assert.False(plan.CanSave);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.MissingFilePath, plan.Status);
        Assert.Contains("path", plan.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildDryRun_UsesCurrentTextWithoutRegeneratingDictionaryModel()
    {
        const string originalText = """
            [E1]
            Strength=100
            """;
        const string currentText = """
            [E1]
            ; keep comment
            Strength=125
            Strength=130

            [E1]
            Name=Duplicate Section
            """;
        Ra2EditableDocumentSession session = CreateSession(
            "rules.ini",
            originalText,
            currentText,
            Ra2EditorDocumentState.EditableDirty);

        Ra2EditorSavePlan plan = _builder.BuildDryRun(new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false));

        Assert.Equal(currentText, plan.Text);
        Assert.Contains("; keep comment", plan.Text);
        Assert.Contains("Strength=125", plan.Text);
        Assert.Contains("Strength=130", plan.Text);
        Assert.Equal(2, CountOccurrences(plan.Text, "[E1]"));
    }

    private static Ra2EditableDocumentSession CreateSession(
        string filePath,
        string originalText,
        string currentText,
        Ra2EditorDocumentState state,
        Ra2EditorTextEncodingMetadata? metadata = null)
    {
        Ra2EditableDocumentState documentState = new(filePath, originalText, currentText, state, metadata);
        return new Ra2EditableDocumentSession(
            documentState,
            new Ra2IniTextDocumentParser().Parse(currentText));
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
