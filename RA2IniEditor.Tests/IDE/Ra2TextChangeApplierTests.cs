using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2TextChangeApplierTests
{
    [Fact]
    public void Apply_InsertChangeProducesNewTextAndDirtyState()
    {
        Ra2TextChangeApplyResult result = CreateApplier().Apply(
            CreateState("Primary=", Ra2EditorDocumentState.EditableClean),
            new Ra2TextChange(new Ra2TextSpan(8, 0), "120mm", "test"));

        Assert.True(result.Success);
        Assert.Equal("Primary=120mm", result.DocumentState!.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.DocumentState.State);
    }

    [Fact]
    public void Apply_ReplaceChangeProducesNewText()
    {
        Ra2TextChangeApplyResult result = CreateApplier().Apply(
            CreateState("Str", Ra2EditorDocumentState.EditableClean),
            new Ra2TextChange(new Ra2TextSpan(0, 3), "Strength", "test"));

        Assert.True(result.Success);
        Assert.Equal("Strength", result.DocumentState!.CurrentText);
    }

    [Fact]
    public void Apply_DeleteChangeProducesNewText()
    {
        Ra2TextChangeApplyResult result = CreateApplier().Apply(
            CreateState("Primary=120mm", Ra2EditorDocumentState.EditableDirty),
            new Ra2TextChange(new Ra2TextSpan(8, 5), string.Empty, "test"));

        Assert.True(result.Success);
        Assert.Equal("Primary=", result.DocumentState!.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.DocumentState.State);
    }

    [Fact]
    public void Apply_SpanAtStartAndEndCanBeApplied()
    {
        IRa2TextChangeApplier applier = CreateApplier();

        Ra2TextChangeApplyResult atStart = applier.Apply(
            CreateState("World", Ra2EditorDocumentState.EditableClean),
            new Ra2TextChange(new Ra2TextSpan(0, 0), "Hello ", "test"));
        Ra2TextChangeApplyResult atEnd = applier.Apply(
            CreateState("Hello", Ra2EditorDocumentState.EditableClean),
            new Ra2TextChange(new Ra2TextSpan(5, 0), " World", "test"));

        Assert.Equal("Hello World", atStart.DocumentState!.CurrentText);
        Assert.Equal("Hello World", atEnd.DocumentState!.CurrentText);
    }

    [Fact]
    public void Apply_CanInsertIntoEmptyText()
    {
        Ra2TextChangeApplyResult result = CreateApplier().Apply(
            CreateState(string.Empty, Ra2EditorDocumentState.EditableClean),
            new Ra2TextChange(new Ra2TextSpan(0, 0), "[NEWINF]", "test"));

        Assert.True(result.Success);
        Assert.Equal("[NEWINF]", result.DocumentState!.CurrentText);
        Assert.Single(result.TextDocument!.Lines);
    }

    [Fact]
    public void Apply_ReadOnlyPreviewReturnsFailure()
    {
        Ra2TextChangeApplyResult result = CreateApplier().Apply(
            CreateState("Text", Ra2EditorDocumentState.ReadOnlyPreview),
            new Ra2TextChange(new Ra2TextSpan(0, 0), "X", "test"));

        Assert.False(result.Success);
        Assert.Null(result.DocumentState);
        Assert.Null(result.TextDocument);
        Assert.Contains("read-only preview", result.ErrorMessage);
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(2, 5)]
    public void Apply_OutOfRangeSpanReturnsFailureWithoutState(int start, int length)
    {
        Ra2TextChangeApplyResult result = CreateApplier().Apply(
            CreateState("Text", Ra2EditorDocumentState.EditableClean),
            new Ra2TextChange(new Ra2TextSpan(start, length), "X", "test"));

        Assert.False(result.Success);
        Assert.Null(result.DocumentState);
        Assert.Null(result.TextDocument);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    public void TextSpan_RejectsNegativeSpanBeforeApply(int start, int length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2TextSpan(start, length));
    }

    [Fact]
    public void Apply_NoOpDoesNotDirtyCleanDocument()
    {
        Ra2TextChangeApplyResult result = CreateApplier().Apply(
            CreateState("Strength", Ra2EditorDocumentState.EditableClean),
            new Ra2TextChange(new Ra2TextSpan(0, 8), "Strength", "test"));

        Assert.True(result.Success);
        Assert.Equal(Ra2EditorDocumentState.EditableClean, result.DocumentState!.State);
    }

    [Fact]
    public void Apply_NoOpKeepsDirtyDocumentDirty()
    {
        Ra2TextChangeApplyResult result = CreateApplier().Apply(
            CreateState("Strength", Ra2EditorDocumentState.EditableDirty),
            new Ra2TextChange(new Ra2TextSpan(0, 8), "Strength", "test"));

        Assert.True(result.Success);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.DocumentState!.State);
    }

    [Fact]
    public void Apply_PreservesEncodingMetadata()
    {
        Ra2EditorTextEncodingMetadata metadata = new(
            Ra2EditorTextEncodingKind.Utf8Bom,
            "UTF-8 BOM",
            hasBom: true);

        Ra2TextChangeApplyResult result = CreateApplier().Apply(
            CreateState("Primary=", Ra2EditorDocumentState.EditableClean, metadata),
            new Ra2TextChange(new Ra2TextSpan(8, 0), "120mm", "test"));

        Assert.True(result.Success);
        Assert.Same(metadata, result.DocumentState!.EncodingMetadata);
    }

    [Fact]
    public void Apply_RebuildsTextModelAndPreservesDuplicateSectionsKeysCommentsAndBlankLines()
    {
        const string text = """
            [120mm]
            Damage=90
            Damage=100

            ; comment
            [120mm]
            ROF=60
            """;

        Ra2TextChangeApplyResult result = CreateApplier().Apply(
            CreateState(text, Ra2EditorDocumentState.EditableClean),
            new Ra2TextChange(new Ra2TextSpan(text.Length, 0), "\nRange=5", "test"));

        Assert.True(result.Success);
        Assert.Equal(2, result.TextDocument!.SectionHeaders.Count(line => line.SectionName == "120mm"));
        Assert.Equal(2, result.TextDocument.KeyValues.Count(line => line.Key == "Damage"));
        Assert.Contains(result.TextDocument.Lines, line => line.Kind == Ra2IniDocumentLineKind.Comment);
        Assert.Contains(result.TextDocument.Lines, line => line.Kind == Ra2IniDocumentLineKind.Blank);
        Assert.Equal(result.DocumentState!.CurrentText, string.Concat(result.TextDocument.Lines.Select(line => line.Text + line.LineBreak)));
    }

    private static IRa2TextChangeApplier CreateApplier()
        => new Ra2TextChangeApplier(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService());

    private static Ra2EditableDocumentState CreateState(
        string currentText,
        Ra2EditorDocumentState state,
        Ra2EditorTextEncodingMetadata? encodingMetadata = null)
        => new("rulesmd.ini", currentText, currentText, state, encodingMetadata);
}
