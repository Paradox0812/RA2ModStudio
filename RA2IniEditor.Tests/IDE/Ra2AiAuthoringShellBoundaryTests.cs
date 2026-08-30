using Xunit;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Views;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiAuthoringShellBoundaryTests
{
    [Fact]
    public void RetrievalSummary_UsesCompactHostFactsAndHidesTerminalQueryFailures()
    {
        Ra2AiContextQueryRequest query = new(
            Ra2AiContextQueryKind.GetSection,
            "rules",
            "HTNK",
            string.Empty,
            null,
            null,
            0);
        Ra2AiContextQueryResult fact = new(
            query,
            true,
            string.Empty,
            string.Empty,
            new Ra2AiContextSectionFact("HTNK", "Vehicle", 0, 1, [], false),
            null);
        Ra2AiSemanticRetrievalAttempt attempt = new(
            1,
            [query],
            [fact],
            1,
            Ra2AiResponseKind.ToolCalls,
            1200);
        Ra2AiResolvedEntityBinding binding = new(
            "techno",
            "rules",
            "HTNK",
            "Vehicle",
            "HTNK",
            "ExactSectionId",
            100);
        Ra2AiSemanticRetrievalResult ready = new(
            [fact],
            [binding],
            [attempt],
            Ra2AiSemanticRetrievalStopReason.EvidenceReady,
            "ready");

        Assert.Equal(
            "项目检索：2 轮 · 1 个实体 · 1 项事实 · 已就绪",
            ShellWindow.FormatAiAssistantRetrievalSummary(ready));
        Assert.EndsWith(
            "无新证据，使用现有事实",
            ShellWindow.FormatAiAssistantRetrievalSummary(
                ready with { StopReason = Ra2AiSemanticRetrievalStopReason.NoProgress }));
        Assert.EndsWith(
            "达到补查上限",
            ShellWindow.FormatAiAssistantRetrievalSummary(
                ready with { StopReason = Ra2AiSemanticRetrievalStopReason.RoundLimit }));
        Assert.Null(ShellWindow.FormatAiAssistantRetrievalSummary(
            ready with { StopReason = Ra2AiSemanticRetrievalStopReason.NeedsClarification }));
        Assert.Null(ShellWindow.FormatAiAssistantRetrievalSummary(
            ready with { StopReason = Ra2AiSemanticRetrievalStopReason.ProviderFailure }));
        Assert.Null(ShellWindow.FormatAiAssistantRetrievalSummary(
            new Ra2AiSemanticRetrievalResult([], [], [], Ra2AiSemanticRetrievalStopReason.NoRefinementRequired, "none")));
    }

    [Fact]
    public void RetrievalSummary_IsDynamicMetadataWithoutShellXamlOrRawProviderDisclosure()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string shellXaml = File.ReadAllText(
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("AiAssistant.RetrievalSummary", shellCode, StringComparison.Ordinal);
        Assert.Contains("IdeAiMetadataTextStyle", shellCode, StringComparison.Ordinal);
        Assert.Contains("TextTrimming = TextTrimming.CharacterEllipsis", shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AiAssistant.RetrievalSummary", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("retrieval.Message", shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("attempt.PromptCharacters", shellCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Shell_PassesAlreadyCapturedContextSourcesAndExistingGatewayWithoutUiChanges()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string shellXaml = File.ReadAllText(
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains(
            "new Ra2AiContextSourceSet(authoringRequestContext, projectAuthoringRequestContext)",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "DeepSeekRa2AiClientFactory.CreateClient(configurationSnapshot),\r\n            _automationCapabilityGateway",
            shellCode.Replace("\n", "\r\n", StringComparison.Ordinal).Replace("\r\r\n", "\r\n", StringComparison.Ordinal),
            StringComparison.Ordinal);
        Assert.DoesNotContain("Ra2AiContextSourceSet", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("context_queries", shellXaml, StringComparison.Ordinal);
    }

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
            "_aiUserMode == Ra2AiUserMode.Work &&",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "editAvailability is Ra2AiEditAvailabilityKind.MissingConfiguration or",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains("CaptureRulesArtProjectAuthoringContext", shellCode, StringComparison.Ordinal);
        Assert.Contains("ProjectEditAvailability = projectAvailability", shellCode, StringComparison.Ordinal);
        Assert.Contains("_projectDocumentSessionStore.MemberFilePaths", shellCode, StringComparison.Ordinal);
        Assert.Contains("viewModel.CurrentProjectFiles.ToArray()", shellCode, StringComparison.Ordinal);
        Assert.Contains("Work 项目：", shellCode, StringComparison.Ordinal);
        Assert.Contains("ResolveRulesWithOptionalArtTargets", shellCode, StringComparison.Ordinal);
        Assert.Contains("未找到唯一 rulesmd.ini 或 rules.ini", shellCode, StringComparison.Ordinal);
        Assert.Contains("描述当前文件或当前项目要完成的修改。", shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "interactionRoute.Kind == Ra2AiInteractionRouteKind.UnsupportedWorkCapability",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "Ra2AuthoringSnapshot.Capture(",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "new Ra2AiBoundedStructuredReplanRequest(",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "replanCoordinator.ExecuteAsync(",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "AttachPreparedAiEditProposal(",
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
            "proposal.ProjectPreview.Plan.Summary : proposal.Preview.Plan.Summary",
            shellCode,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Ra2AiStructuredRepairPolicy", shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("SendStructuredRepairAsync", shellCode, StringComparison.Ordinal);
        Assert.Contains("已自动修正 1 次。", shellCode, StringComparison.Ordinal);
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
        Assert.Contains(
            "AiAssistant.EditProposalCard.OpenDiffButton",
            proposalXaml,
            StringComparison.Ordinal);
        Assert.Contains("AiAssistant.EditProposalCard.ProjectSummary", proposalXaml, StringComparison.Ordinal);
        Assert.Contains("AiAssistant.EditProposalCard.AssetManifestSummary", proposalXaml, StringComparison.Ordinal);
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
        Assert.Contains(
            "or Ra2AiResponseKind.LocalRejection",
            shellCode,
            StringComparison.Ordinal);
        Assert.Contains("response.LocalRejectionMessage", shellCode, StringComparison.Ordinal);
    }
}
