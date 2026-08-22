using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SavePlanNewLineMetadataTests
{
    private readonly Ra2EditorSavePlanBuilder _savePlanBuilder = new();

    [Fact]
    public void CompletionCommit_SavePlanCarriesUpdatedNewLineKind()
    {
        Ra2EditableDocumentSession session = CreateSession("Str\r\n", Ra2EditorDocumentState.EditableClean);
        Ra2CompletionCommitCoordinator coordinator = new(
            new Ra2CompletionCommitPlanner(),
            new Ra2TextChangeApplier(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService()));
        Ra2CompletionResult completionResult = new(
            [new Ra2CompletionItem("Strength", Ra2CompletionItemKind.Key, insertText: "Strength=")],
            new Ra2TextSpan(0, 3));

        Ra2CompletionCommitApplyResult result = coordinator.TryCommit(
            session,
            completionResult,
            completionResult.Items[0]);
        Ra2EditorSavePlan plan = _savePlanBuilder.Build(result.Session!);

        Assert.Equal(Ra2IniNewLineKind.CrLf, plan.NewLineKind);
        Assert.Equal(Ra2EditorNewLineSavePolicy.PreserveCurrentText, plan.NewLinePolicy);
    }

    [Fact]
    public void TextChangeToMixedNewLines_SavePlanCarriesMixedNewLineKind()
    {
        Ra2EditableDocumentSession session = CreateSession("[E1]\r\nStrength=100\r\n", Ra2EditorDocumentState.EditableClean);
        Ra2TextChangeApplyResult applyResult = new Ra2TextChangeApplier(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService()).Apply(
                session.DocumentState,
                new Ra2TextChange(new Ra2TextSpan(session.DocumentState.CurrentText.Length, 0), "Armor=none\n", "test"));
        Ra2EditableDocumentSession updated = new(applyResult.DocumentState!, applyResult.TextDocument!);

        Ra2EditorSavePlan plan = _savePlanBuilder.Build(updated);

        Assert.Equal(Ra2IniNewLineKind.Mixed, plan.NewLineKind);
        Assert.Equal(Ra2EditorNewLineSavePolicy.PreserveCurrentText, plan.NewLinePolicy);
    }

    [Fact]
    public void Revert_SavePlanReturnsToOriginalNewLineKind()
    {
        Ra2EditableDocumentSessionService service = new(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService());
        Ra2EditableDocumentSession clean = service.StartEditing("rules.ini", "[E1]\r\nStrength=100\r\n");
        Ra2EditableDocumentSession dirty = service.UpdateText(clean, "[E1]\nStrength=125\n");

        Ra2EditableDocumentSession reverted = service.Revert(dirty);
        Ra2EditorSavePlan plan = _savePlanBuilder.Build(reverted);

        Assert.Equal(Ra2IniNewLineKind.CrLf, plan.NewLineKind);
        Assert.Equal(Ra2EditorNewLineSavePolicy.PreserveCurrentText, plan.NewLinePolicy);
    }

    private static Ra2EditableDocumentSession CreateSession(
        string currentText,
        Ra2EditorDocumentState state)
    {
        Ra2EditableDocumentState documentState = new("rulesmd.ini", currentText, currentText, state);
        return new Ra2EditableDocumentSession(
            documentState,
            new Ra2IniTextDocumentParser().Parse(currentText));
    }
}
