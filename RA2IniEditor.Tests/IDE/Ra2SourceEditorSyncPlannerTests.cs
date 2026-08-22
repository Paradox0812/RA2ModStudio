using RA2IniEditor.IDE.Controllers.SourceEditor;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SourceEditorSyncPlannerTests
{
    private readonly Ra2SourceEditorSyncPlanner _planner = new();

    [Theory]
    [InlineData(-5, 10, 0)]
    [InlineData(0, 10, 0)]
    [InlineData(4, 10, 4)]
    [InlineData(12, 10, 10)]
    [InlineData(3, 0, 0)]
    public void ClampCaretOffset_ClampsToTextRange(int caretOffset, int textLength, int expected)
    {
        int actual = _planner.ClampCaretOffset(caretOffset, textLength);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ClampCaretOffset_RejectsNegativeTextLength()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _planner.ClampCaretOffset(0, -1));
    }

    [Fact]
    public void CreateTextSyncPlan_CanRepresentEveryOperationKind()
    {
        Ra2SourceEditorSyncOperationKind[] kinds =
        [
            Ra2SourceEditorSyncOperationKind.LoadFile,
            Ra2SourceEditorSyncOperationKind.Revert,
            Ra2SourceEditorSyncOperationKind.CompletionCommit,
            Ra2SourceEditorSyncOperationKind.AddPropertyInsert,
            Ra2SourceEditorSyncOperationKind.AddPropertyReplace,
            Ra2SourceEditorSyncOperationKind.ExternalReload
        ];

        foreach (Ra2SourceEditorSyncOperationKind kind in kinds)
        {
            Ra2SourceEditorSyncPlan plan = _planner.CreateTextSyncPlan(kind, "[E1]\n");

            Assert.Equal(kind, plan.Kind);
            Assert.Equal("[E1]\n", plan.Text);
            Assert.Null(plan.CaretOffset);
        }
    }

    [Fact]
    public void CreateTextSyncPlan_ClampsRequestedCaretOffsetToTextLength()
    {
        Ra2SourceEditorSyncPlan plan = _planner.CreateTextSyncPlan(
            Ra2SourceEditorSyncOperationKind.CompletionCommit,
            "[E1]\n",
            requestedCaretOffset: 99);

        Assert.Equal(5, plan.CaretOffset);
    }

    [Fact]
    public void CreateTextSyncPlan_KeepsNullCaretOffset()
    {
        Ra2SourceEditorSyncPlan plan = _planner.CreateTextSyncPlan(
            Ra2SourceEditorSyncOperationKind.AddPropertyInsert,
            "[E1]\n",
            requestedCaretOffset: null);

        Assert.Null(plan.CaretOffset);
    }

    [Fact]
    public void CreateTextSyncPlan_NormalizesNullTextBeforeCaretClamp()
    {
        Ra2SourceEditorSyncPlan plan = _planner.CreateTextSyncPlan(
            Ra2SourceEditorSyncOperationKind.ExternalReload,
            text: null!,
            requestedCaretOffset: 12);

        Assert.Equal(string.Empty, plan.Text);
        Assert.Equal(0, plan.CaretOffset);
    }

    [Fact]
    public void CreateTextSyncPlan_PreservesReadonlyAndEditableFlags()
    {
        Ra2SourceEditorSyncPlan plan = _planner.CreateTextSyncPlan(
            Ra2SourceEditorSyncOperationKind.LoadFile,
            "[E1]\n",
            shouldSetReadOnly: true);

        Assert.True(plan.ShouldSetReadOnly);
        Assert.False(plan.ShouldSetEditable);
    }
}
