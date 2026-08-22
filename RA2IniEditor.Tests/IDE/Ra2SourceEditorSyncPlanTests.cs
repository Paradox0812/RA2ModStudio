using RA2IniEditor.IDE.Controllers.SourceEditor;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SourceEditorSyncPlanTests
{
    [Fact]
    public void RevertPlan_CarriesTextAndReadonlyRequest()
    {
        Ra2SourceEditorSyncPlan plan = new(
            Ra2SourceEditorSyncOperationKind.Revert,
            "[E1]\n",
            shouldSetReadOnly: true);

        Assert.Equal(Ra2SourceEditorSyncOperationKind.Revert, plan.Kind);
        Assert.Equal("[E1]\n", plan.Text);
        Assert.Null(plan.CaretOffset);
        Assert.True(plan.ShouldSetReadOnly);
        Assert.False(plan.ShouldSetEditable);
    }

    [Fact]
    public void CompletionCommitPlan_CarriesTextAndCaretOffset()
    {
        Ra2SourceEditorSyncPlan plan = new(
            Ra2SourceEditorSyncOperationKind.CompletionCommit,
            "[E1]\nArmor=steel\n",
            caretOffset: 16);

        Assert.Equal(Ra2SourceEditorSyncOperationKind.CompletionCommit, plan.Kind);
        Assert.Equal("[E1]\nArmor=steel\n", plan.Text);
        Assert.Equal(16, plan.CaretOffset);
        Assert.False(plan.ShouldSetReadOnly);
        Assert.False(plan.ShouldSetEditable);
    }

    [Fact]
    public void AddPropertyInsertPlan_CarriesTextAndCaretOffset()
    {
        Ra2SourceEditorSyncPlan plan = new(
            Ra2SourceEditorSyncOperationKind.AddPropertyInsert,
            "[E1]\nStrength=125\n",
            caretOffset: 18);

        Assert.Equal(Ra2SourceEditorSyncOperationKind.AddPropertyInsert, plan.Kind);
        Assert.Equal("[E1]\nStrength=125\n", plan.Text);
        Assert.Equal(18, plan.CaretOffset);
    }

    [Fact]
    public void Constructor_NormalizesNullTextToEmptyText()
    {
        Ra2SourceEditorSyncPlan plan = new(
            Ra2SourceEditorSyncOperationKind.LoadFile,
            text: null!);

        Assert.Equal(string.Empty, plan.Text);
    }

    [Fact]
    public void Constructor_RejectsConflictingReadonlyAndEditableRequests()
    {
        Assert.Throws<ArgumentException>(() => new Ra2SourceEditorSyncPlan(
            Ra2SourceEditorSyncOperationKind.ExternalReload,
            "[E1]\n",
            shouldSetReadOnly: true,
            shouldSetEditable: true));
    }
}
