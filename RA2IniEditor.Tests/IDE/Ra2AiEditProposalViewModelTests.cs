using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AuthoringDiff;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiEditProposalViewModelTests
{
    [Fact]
    public void Constructor_UsesSnapshotValueAndReadyState()
    {
        Ra2AiEditProposalViewModel viewModel = new(CreateProposal(
            Ra2AiEditProposalApplyPolicy.Normal));

        Assert.Equal(Ra2AiEditProposalState.Ready, viewModel.State);
        Assert.Equal("可应用", viewModel.StatusText);
        Assert.True(viewModel.IsApplyEnabled);
        Assert.True(viewModel.IsDismissEnabled);
        Assert.Equal("应用到当前文件", viewModel.ApplyButtonText);
        Ra2AiEditProposalOperationViewModel operation = Assert.Single(viewModel.Operations);
        Assert.Equal("替换", operation.ActionText);
        Assert.Equal("[E1] Strength", operation.TargetText);
        Assert.Equal("100  →  125", operation.ChangeText);
    }

    [Fact]
    public void Caution_UsesExplicitReviewButtonText()
    {
        Ra2AiEditProposalViewModel viewModel = new(CreateProposal(
            Ra2AiEditProposalApplyPolicy.Caution));

        Assert.Equal("需要复核", viewModel.StatusText);
        Assert.Equal("仍要应用", viewModel.ApplyButtonText);
        Assert.True(viewModel.IsApplyEnabled);
    }

    [Fact]
    public void Blocked_DisablesApplyButAllowsDismiss()
    {
        Ra2AiEditProposalViewModel viewModel = new(CreateProposal(
            Ra2AiEditProposalApplyPolicy.Blocked));

        Assert.Equal(Ra2AiEditProposalState.Blocked, viewModel.State);
        Assert.False(viewModel.IsApplyEnabled);
        Assert.True(viewModel.IsDismissEnabled);
    }

    [Fact]
    public void StateTransitions_DisableActionsAndPreserveHistoricalAppliedMeaning()
    {
        Ra2AiEditProposalViewModel viewModel = new(CreateProposal(
            Ra2AiEditProposalApplyPolicy.Normal));

        viewModel.BeginApply();
        Assert.Equal(Ra2AiEditProposalState.Applying, viewModel.State);
        Assert.False(viewModel.IsApplyEnabled);
        Assert.False(viewModel.IsDismissEnabled);

        viewModel.MarkApplied("已应用到内存，尚未保存。");
        Assert.Equal(Ra2AiEditProposalState.Applied, viewModel.State);
        Assert.Equal("已应用过", viewModel.StatusText);
        Assert.Contains("Ctrl+Z", viewModel.ResultMessage);
    }

    [Fact]
    public void SupersedeDismissAndStaleStatesAreTerminalForActions()
    {
        Ra2AiEditProposalViewModel superseded = new(CreateProposal(
            Ra2AiEditProposalApplyPolicy.Normal));
        Ra2AiEditProposalViewModel dismissed = new(CreateProposal(
            Ra2AiEditProposalApplyPolicy.Normal));
        Ra2AiEditProposalViewModel stale = new(CreateProposal(
            Ra2AiEditProposalApplyPolicy.Normal));

        superseded.MarkSuperseded();
        dismissed.MarkDismissed();
        stale.MarkStale("changed");

        Assert.Equal(Ra2AiEditProposalState.Superseded, superseded.State);
        Assert.Equal(Ra2AiEditProposalState.Dismissed, dismissed.State);
        Assert.Equal(Ra2AiEditProposalState.Stale, stale.State);
        Assert.False(superseded.IsApplyEnabled);
        Assert.False(dismissed.IsDismissEnabled);
        Assert.False(stale.IsApplyEnabled);
    }

    [Fact]
    public void AuthoringDiff_ProjectsBlockedAndStaleProposalStateWithoutOwningAuthority()
    {
        Ra2AiEditProposalViewModel blockedProposal = new(CreateProposal(
            Ra2AiEditProposalApplyPolicy.Blocked));
        using TestDiffViewModel blocked = new(blockedProposal);
        Assert.True(blocked.ViewModel.IsBlocked);
        Assert.False(blocked.ViewModel.IsApplyEnabled);

        Ra2AiEditProposalViewModel currentProposal = new(CreateProposal(
            Ra2AiEditProposalApplyPolicy.Normal));
        using TestDiffViewModel current = new(currentProposal);
        currentProposal.MarkStale("changed");
        Assert.True(current.ViewModel.IsStale);
        Assert.False(current.ViewModel.IsApplyEnabled);
    }

    [Fact]
    public void ProposalView_UsesModernVirtualizedListAndRequiredAutomationIds()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "AI",
            "Ra2AiEditProposalView.xaml"));

        Assert.Contains("<ListBox", xaml);
        Assert.DoesNotContain("<DataGrid", xaml);
        Assert.Contains("VirtualizingPanel.IsVirtualizing=\"True\"", xaml);
        Assert.Contains("VirtualizingPanel.VirtualizationMode=\"Recycling\"", xaml);
        Assert.Contains("MaxHeight=\"240\"", xaml);
        foreach (string automationId in new[]
                 {
                     "AiAssistant.EditProposalCard",
                     "AiAssistant.EditProposalCard.Status",
                     "AiAssistant.EditProposalCard.Summary",
                     "AiAssistant.EditProposalCard.OperationList",
                     "AiAssistant.EditProposalCard.DiagnosticSummary",
                     "AiAssistant.EditProposalCard.OpenDiffButton",
                     "AiAssistant.EditProposalCard.ApplyButton",
                     "AiAssistant.EditProposalCard.DismissButton",
                     "AiAssistant.EditProposalCard.ResultMessage"
                 })
        {
            Assert.Contains(automationId, xaml);
        }
    }

    private static Ra2AiEditProposal CreateProposal(
        Ra2AiEditProposalApplyPolicy policy)
    {
        const string text =
            "[InfantryTypes]\n1=E1\n\n[E1]\nStrength=100\n";
        Ra2EditableDocumentSessionService sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        Ra2EditableDocumentSession session = sessionService.StartEditing(
            "rulesmd.ini",
            text);
        Ra2AuthoringSnapshot snapshot = Assert.IsType<Ra2AuthoringSnapshot>(
            Ra2AuthoringSnapshot.Capture(
                session,
                text,
                string.Empty,
                new Ra2FieldRegistryProviderSnapshot(
                    new BuiltInRa2FieldDefinitionProvider(),
                    1)).Snapshot);
        Ra2IniEditPlan plan = new(
            Guid.NewGuid(),
            snapshot.DocumentId,
            snapshot.EditRevision,
            snapshot.FieldRegistry.Revision,
            [
                new Ra2IniEditOperation(
                    Ra2IniEditOperationKind.ReplaceFieldValue,
                    "E1",
                    "Strength",
                    "125")
            ],
            "Update Strength",
            "Tests");
        Ra2IniEditPreview preview = new Ra2IniEditPreviewService(
            new Ra2IniLanguageAnalysisService(),
            new Ra2AddPropertyInsertPlanner()).Preview(snapshot, plan);
        Assert.True(preview.Succeeded);
        return new Ra2AiEditProposal(preview, policy, "risk summary");
    }

    private sealed class TestDiffViewModel : IDisposable
    {
        public TestDiffViewModel(Ra2AiEditProposalViewModel proposal)
            => ViewModel = new Ra2AuthoringDiffViewModel(proposal);

        public Ra2AuthoringDiffViewModel ViewModel { get; }

        public void Dispose() => ViewModel.Dispose();
    }
}
