using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiAuthoringShellBoundaryTests
{
    [Fact]
    public void Shell_EnablesStructuredEditingOnlyForOfficialEndpointAndEditableSnapshot()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains(
            "configurationSnapshot.State == DeepSeekRa2AiConfigurationState.Ready",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "configurationSnapshot.EndpointKind == DeepSeekRa2AiEndpointKind.Official",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "Ra2AiInteractionRouter.Resolve(",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "Ra2AiInteractionRouteKind.EditUnavailable",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "Ra2AuthoringSnapshot.Capture(",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "PrepareAndAttachAiEditProposalAsync(",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "_aiProposalPreparationRunner.PrepareAsync(",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "requestGeneration != Volatile.Read(ref _aiAuthoringGeneration)",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "_authoringPreviewCurrencyEvaluator.Evaluate(",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "_activeAiEditProposalMessageBorder = handle.MessageBorder;",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "AiAssistantChatMessages.Children[assistantIndex],",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "_activeAiEditProposalMessageBorder))",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "$\"结构化修改建议：{proposal.Preview.Plan.Summary}\"",
            shellCode,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Shell_UsesCoordinatorForExplicitApplyAndLifecycleInvalidation()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains(
            "_aiAuthoringCoordinator.ApplyConfirmed(viewModel.Proposal)",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "_aiAuthoringCoordinator.Dismiss(viewModel.Proposal)",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "_aiAuthoringCoordinator.InvalidateActiveProposal()",
            shellCode,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "_authoringWorkspace.InvalidateActivePreview();",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "explicitConfirmationGranted: true",
            File.ReadAllText(
                Path.Combine(root, "RA2IniEditor.IDE", "AI", "Ra2AiAuthoringCoordinator.cs")),
            StringComparison.Ordinal);
    }

    [Fact]
    public void ProposalCard_IsIsolatedFromFrozenShellXamlAndExposesAutomationContract()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string proposalXaml = File.ReadAllText(
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "AI", "Ra2AiEditProposalView.xaml"));

        Assert.DoesNotContain(
            "Ra2AiEditProposalView",
            shellXaml,
            StringComparison.Ordinal);
        Assert.Contains(
            "AiAssistant.EditProposalCard",
            proposalXaml,
            StringComparison.Ordinal);
        Assert.Contains(
            "AiAssistant.EditProposalCard.OperationList",
            proposalXaml,
            StringComparison.Ordinal);
        Assert.Contains(
            "AiAssistant.EditProposalCard.ApplyButton",
            proposalXaml,
            StringComparison.Ordinal);
        Assert.Contains(
            "AiAssistant.EditProposalCard.DismissButton",
            proposalXaml,
            StringComparison.Ordinal);
        Assert.DoesNotContain("<DataGrid", proposalXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Shell_SeparatesProviderTextFromValidatedProposalAuthority()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains(
            "编辑权限仅来自本地校验后的结构化操作；以上文字不改变建议内容。",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "未调用所需编辑工具，本次内容不会进入后续对话上下文。",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "or Ra2AiResponseKind.AuthoringToolNotInvoked",
            shellCode,
            StringComparison.Ordinal);
    }
}
