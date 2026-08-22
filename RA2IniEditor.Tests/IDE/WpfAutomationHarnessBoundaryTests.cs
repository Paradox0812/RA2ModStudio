using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class WpfAutomationHarnessBoundaryTests
{
    [Fact]
    public void ShellAndFieldRegistryWindows_ExposeStableAutomationIds()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string managerXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml"));
        string completionXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml"));
        string addPropertyXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldBrowser", "Ra2AddPropertyWindow.xaml"));
        string issuesXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "IssuesToolWindow.xaml"));

        string[] requiredShellIds =
        [
            "Shell.Window",
            "Shell.Menu.OpenFolder",
            "Shell.Menu.FieldRegistryCenter",
            "Shell.SourceEditor",
            "Shell.SourceEditor.TextArea",
            "Shell.SourceEditor.EnterEditModeButton",
            "Visibility=\"Collapsed\"",
            "Shell.SourceEditor.UndoButton",
            "Shell.SourceEditor.RedoButton",
            "Shell.SourceEditor.SaveCurrentFileButton",
            "Shell.SourceEditor.RevertInMemoryChangesButton",
            "Shell.SourceEditor.EditorStateText",
            "Shell.SourceEditor.AddPropertyMenuItem",
            "Shell.SourceEditor.ShowCompletionPreviewMenuItem",
            "Shell.CompletionDropdownPopup",
            "Shell.ProjectExplorer",
            "RightToolWell.Root",
            "RightToolWell.SectionTab",
            "RightToolWell.AiTab",
            "RightToolWell.ActiveView",
            "AiAssistant.Panel",
            "AiAssistant.Header",
            "AiAssistant.ContextSummary",
            "AiAssistant.CurrentSubjectSummary",
            "AiAssistant.ConversationContextSummary",
            "AiAssistant.ChatHistory",
            "AiAssistant.Composer",
            "AiAssistant.AdvancedButton",
            "AiAssistant.AdvancedOptions",
            "AiAssistant.ChatHistoryActions",
            "AiAssistant.ModelSelector",
            "AiAssistant.ConfigurationStatus",
            "AiAssistant.RequestPreparationNotice",
            "AiAssistant.RequestDiagnostics",
            "AiAssistant.PromptBox",
            "AiAssistant.GenerateButton",
            "AiAssistant.CancelButton",
            "AiAssistant.ClearButton",
            "AiAssistant.AssistantMessageCopyButton",
            "AiAssistant.RestorePromptButton",
            "AiAssistant.RestorePromptStatus",
            "AiAssistant.CodeBlock",
            "AiAssistant.CodeBlockCopyButton",
            "AiAssistant.CodeBlockLanguage",
            "AiAssistant.MarkdownHeading",
            "AiAssistant.MarkdownParagraph",
            "AiAssistant.MarkdownListItem",
            "AiAssistant.MarkdownTable",
            "AiAssistant.MarkdownTableHeader",
            "AiAssistant.MarkdownTableRow",
            "AiAssistant.MarkdownTableCell",
            "AiAssistant.MarkdownInlineCode",
            "AiAssistant.MarkdownFallbackText",
            "AiAssistant.ResponseArea",
            "AiAssistant.DraftPreview",
            "AiAssistant.SafetyFooter",
            "AiAssistant.UserMessageList",
            "AiAssistant.AssistantMessageList",
            "AiAssistant.LatestAssistantMessage",
            "AiAssistant.EmptyStateMessage",
            "Shell.OutputTextBox"
        ];

        foreach (string id in requiredShellIds)
            Assert.Contains(id, shellXaml + shellCode, StringComparison.Ordinal);

        Assert.DoesNotContain("AiAssistant.ApiKeyTextBox", shellXaml + shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AiAssistant.SaveApiKeyButton", shellXaml + shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AiAssistant.ApplyButton", shellXaml + shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AiAssistant.InsertButton", shellXaml + shellCode, StringComparison.Ordinal);

        Assert.Contains("Ra2CompletionDropdown.ItemsList", completionXaml, StringComparison.Ordinal);
        Assert.Contains("AddProperty.Window", addPropertyXaml, StringComparison.Ordinal);
        Assert.Contains("AddProperty.SearchTextBox", addPropertyXaml, StringComparison.Ordinal);
        Assert.Contains("AddProperty.FieldsGrid", addPropertyXaml, StringComparison.Ordinal);
        Assert.Contains("AddProperty.ValueTextBox", addPropertyXaml, StringComparison.Ordinal);
        Assert.Contains("AddProperty.AddSelectedButton", addPropertyXaml, StringComparison.Ordinal);
        Assert.Contains("AddProperty.CancelButton", addPropertyXaml, StringComparison.Ordinal);
        Assert.Contains("Issues.Window", issuesXaml, StringComparison.Ordinal);
        Assert.Contains("Issues.Grid", issuesXaml, StringComparison.Ordinal);
        Assert.Contains("Issues.RefreshCurrentFileButton", issuesXaml, StringComparison.Ordinal);
        Assert.Contains("Issues.RunFullDiagnosticsButton", issuesXaml, StringComparison.Ordinal);
        Assert.Contains("Issues.ClearButton", issuesXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.Window", managerXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStyle=\"None\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("shell:WindowChrome", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CloseButton", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.StatusHubPanel", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.StatusChips", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.ActivePackChip", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.WarningChip", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.ProjectChip", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.GlobalChip", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.BuiltInChip", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.EntryActionsPanel", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.ActivePacksSection", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.ActivePacksScrollHost", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.ReadOnlyActionsGroup", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.WriteActionsGroup", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RollbackRiskSummary", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupWriteWarning", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupPreviewExpander", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupPreviewSummaryCard", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupPreviewEmptyState", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupPreviewScrollHost", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.WarningsSection", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.WarningsScrollHost", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.WarningsEmptyState", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.PacksGrid", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.BuildCleanupPlanButton", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.ApplyCleanupPlanButton", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RelearnCurrentIniButton", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupPlanGrid", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RepairPreviewTabs", managerXaml, StringComparison.Ordinal);
        Assert.Contains("IsExpanded=\"{Binding HasCleanupPreviewDetails, Mode=OneWay}\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("MaxHeight=\"260\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RepairPreviewSummary", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RepairPreviewAbstractFieldsGrid", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RepairPreviewRemovedConcreteGrid", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RepairPreviewSkippedGrid", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RepairPreviewWarningsList", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RollbackManifestsGrid", managerXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void MainPathChineseUiText_DoesNotContainMojibakeInEditorStateText()
    {
        string root = TestRepositoryRoot.Find();
        string viewModelSource = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "Editing", "Ra2EditorStateViewModel.cs"));
        string shellSource = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("未选择文件", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("已打开", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("内存中已修改", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("编辑状态：", shellSource, StringComparison.Ordinal);
        Assert.DoesNotContain("\uFFFD", viewModelSource + shellSource, StringComparison.Ordinal);
    }

    [Fact]
    public void UiAutomationSmokeProject_IsOptInAndDocumentsMainPathSmoke()
    {
        string root = TestRepositoryRoot.Find();
        string skipAttribute = File.ReadAllText(Path.Combine(root, "RA2IniEditor.UiAutomationTests", "UiAutomationFactAttribute.cs"));
        string mainPathSmoke = File.ReadAllText(Path.Combine(root, "RA2IniEditor.UiAutomationTests", "Ra2IdeMainPathSmokeTests.cs"));

        Assert.Contains("RA2INIEDITOR_RUN_UI_AUTOMATION", skipAttribute, StringComparison.Ordinal);
        Assert.Contains("Skip =", skipAttribute, StringComparison.Ordinal);
        Assert.Contains("IdeMainPath_OpenFolder_EditCompletionAddPropertyAndRevert_DoesNotWriteSourceIni", mainPathSmoke, StringComparison.Ordinal);
        Assert.Contains("--automation-open-folder", mainPathSmoke, StringComparison.Ordinal);
        Assert.DoesNotContain("Shell.SourceEditor.EnterEditModeButton", mainPathSmoke, StringComparison.Ordinal);
        Assert.Contains("Shell.SourceEditor.RevertInMemoryChangesButton", mainPathSmoke, StringComparison.Ordinal);
        Assert.Contains("Shell.SourceEditor.AddPropertyMenuItem", mainPathSmoke, StringComparison.Ordinal);
        Assert.Contains("Shell.SourceEditor.ShowCompletionPreviewMenuItem", mainPathSmoke, StringComparison.Ordinal);
        Assert.Contains("Assert.Equal(originalIniText", mainPathSmoke, StringComparison.Ordinal);
    }

    [Fact]
    public void DirtyNavigationUiAutomationSmoke_IsOptInAndUsesStableAutomationIds()
    {
        string root = TestRepositoryRoot.Find();
        string skipAttribute = File.ReadAllText(Path.Combine(root, "RA2IniEditor.UiAutomationTests", "UiAutomationFactAttribute.cs"));
        string smoke = File.ReadAllText(Path.Combine(root, "RA2IniEditor.UiAutomationTests", "Ra2IdeDirtyNavigationSmokeTests.cs"));

        Assert.Contains("RA2INIEDITOR_RUN_UI_AUTOMATION", skipAttribute, StringComparison.Ordinal);
        Assert.Contains("DirtyNavigation_CancelPreservesDirtyTextAndDiscardSwitchesWithoutWriting", smoke, StringComparison.Ordinal);
        Assert.Contains("DirtyNavigation_SaveWritesCurrentFileCreatesBackupAndSwitches", smoke, StringComparison.Ordinal);
        Assert.Contains("DirtyNavigation.Dialog", smoke, StringComparison.Ordinal);
        Assert.Contains("DirtyNavigation.CancelButton", smoke, StringComparison.Ordinal);
        Assert.Contains("DirtyNavigation.DiscardButton", smoke, StringComparison.Ordinal);
        Assert.Contains("DirtyNavigation.SaveButton", smoke, StringComparison.Ordinal);
        Assert.Contains("Shell.SourceEditor.TextArea", smoke, StringComparison.Ordinal);
        Assert.Contains("Shell.OutputTextBox", smoke, StringComparison.Ordinal);
        Assert.Contains("Assert.Equal(originalRulesText", smoke, StringComparison.Ordinal);
        Assert.Contains("WaitForSingleBackupFile", smoke, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldImportPreview_ExposesStableAutomationIds()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));

        string[] requiredIds =
        [
            "FieldImportPreview.Window",
            "FieldImportPreview.SourceNameTextBox",
            "FieldImportPreview.FetchUrlTextBox",
            "FieldImportPreview.FetchRawTextButton",
            "FieldImportPreview.RemoteHistoryGrid",
            "FieldImportPreview.RemotePresetsGrid",
            "FieldImportPreview.RawTextBox",
            "FieldImportPreview.ParsePreviewButton",
            "FieldImportPreview.BuildApplyPlanButton",
            "FieldImportPreview.ApplyButton",
            "FieldImportPreview.PreviewDiffGrid",
            "FieldImportPreview.ApplyPlanGrid",
            "FieldImportPreview.ParsedFieldsGrid",
            "FieldImportPreview.ValidationIssuesGrid"
        ];

        foreach (string id in requiredIds)
            Assert.Contains(id, xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistrySubWindows_UseResizableScrollableIdeLayoutWithoutMojibake()
    {
        string root = TestRepositoryRoot.Find();
        string appXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "App.xaml"));
        string centerXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryCenterWindow.xaml"));
        string editorXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldEditorWindow.xaml"));
        string learningXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldLearningWizardWindow.xaml"));
        string harvestXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));
        string managerXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml"));
        string quickPeekXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldQuickPeek", "Ra2FieldQuickPeekWindow.xaml"));
        string quickPeekCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldQuickPeek", "Ra2FieldQuickPeekWindow.xaml.cs"));
        string peekDefinitionXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2PeekDefinitionWindow.xaml"));
        string peekDefinitionCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2PeekDefinitionWindow.xaml.cs"));
        string completionDropdownXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml"));
        string findReferencesViewXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2FindReferencesView.xaml"));
        string findReferencesXaml = findReferencesViewXaml;
        string allowedValuesXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "AllowedValuesEditorWindow.xaml"));
        string remotePresetXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "RemoteSourcePresetEditorWindow.xaml"));
        string combined = centerXaml + editorXaml + learningXaml + harvestXaml + managerXaml + quickPeekXaml + peekDefinitionXaml + completionDropdownXaml + findReferencesXaml + allowedValuesXaml + remotePresetXaml;

        Assert.DoesNotContain("Resources/Styles/IdeSecondaryWindowStyles.xaml", appXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeSecondary", combined, StringComparison.Ordinal);

        Assert.Contains("ResizeMode=\"CanResize\"", centerXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeMode=\"CanResize\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeMode=\"CanResize\"", learningXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeMode=\"CanResize\"", harvestXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeMode=\"CanResize\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeMode=\"CanResize\"", allowedValuesXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStyle=\"None\"", centerXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStyle=\"None\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStyle=\"None\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStyle=\"None\"", learningXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStyle=\"None\"", allowedValuesXaml, StringComparison.Ordinal);
        Assert.Contains("shell:WindowChrome", centerXaml, StringComparison.Ordinal);
        Assert.Contains("shell:WindowChrome", managerXaml, StringComparison.Ordinal);
        Assert.Contains("shell:WindowChrome", editorXaml, StringComparison.Ordinal);
        Assert.Contains("shell:WindowChrome", learningXaml, StringComparison.Ordinal);
        Assert.Contains("shell:WindowChrome", allowedValuesXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeBorderThickness=\"6\"", centerXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeBorderThickness=\"6\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeBorderThickness=\"6\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeBorderThickness=\"6\"", learningXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeBorderThickness=\"6\"", allowedValuesXaml, StringComparison.Ordinal);
        Assert.Contains("UseAeroCaptionButtons=\"False\"", centerXaml, StringComparison.Ordinal);
        Assert.Contains("UseAeroCaptionButtons=\"False\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("UseAeroCaptionButtons=\"False\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("UseAeroCaptionButtons=\"False\"", learningXaml, StringComparison.Ordinal);
        Assert.Contains("UseAeroCaptionButtons=\"False\"", allowedValuesXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.CloseButton", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CloseButton", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldEditor.CloseButton", editorXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.CloseButton", learningXaml, StringComparison.Ordinal);
        Assert.Contains("AllowedValuesEditor.CloseButton", allowedValuesXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"CloseButton_OnClick\"", centerXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"CloseButton_OnClick\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"CloseButton_OnClick\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"CloseButton_OnClick\"", learningXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"CloseButton_OnClick\"", allowedValuesXaml, StringComparison.Ordinal);
        Assert.Contains("ResizeMode=\"NoResize\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStyle=\"None\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldQuickPeek.CloseButton\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"CloseButton_OnClick\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("ShowInTaskbar=\"False\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("AllowsTransparency=\"True\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("SizeToContent=\"WidthAndHeight\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStartupLocation=\"Manual\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStyle = WindowStyle.None;", quickPeekCode, StringComparison.Ordinal);
        Assert.Contains("ShowInTaskbar = false;", quickPeekCode, StringComparison.Ordinal);
        Assert.Contains("ResizeMode = ResizeMode.NoResize;", quickPeekCode, StringComparison.Ordinal);
        Assert.Contains("SizeToContent = SizeToContent.WidthAndHeight;", quickPeekCode, StringComparison.Ordinal);
        Assert.Contains("PlaceNearCaret", quickPeekCode, StringComparison.Ordinal);
        Assert.Contains("CloseButton_OnClick", quickPeekCode, StringComparison.Ordinal);
        Assert.Contains("WindowStyle=\"None\"", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Ra2PeekDefinition.CloseButton\"", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"CloseButton_OnClick\"", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.Contains("ShowInTaskbar=\"False\"", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.Contains("AllowsTransparency=\"True\"", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.Contains("SizeToContent=\"WidthAndHeight\"", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStartupLocation=\"Manual\"", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.Contains("WindowStyle = WindowStyle.None;", peekDefinitionCode, StringComparison.Ordinal);
        Assert.Contains("ShowInTaskbar = false;", peekDefinitionCode, StringComparison.Ordinal);
        Assert.Contains("ResizeMode = ResizeMode.NoResize;", peekDefinitionCode, StringComparison.Ordinal);
        Assert.Contains("SizeToContent = SizeToContent.WidthAndHeight;", peekDefinitionCode, StringComparison.Ordinal);
        Assert.Contains("PlaceNearCaret", peekDefinitionCode, StringComparison.Ordinal);
        Assert.Contains("CloseButton_OnClick", peekDefinitionCode, StringComparison.Ordinal);
        Assert.Contains("ResizeMode=\"CanResize\"", remotePresetXaml, StringComparison.Ordinal);

        // UI-MODERN-PROGRAM-R1 M4-A replaces the compatibility header with the scoped
        // Field Registry workspace vocabulary while preserving chrome and UIA anchors.
        Assert.Contains("IdeFieldRegistryRootStyle", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.Navigation", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.FieldList", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.Details", centerXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistTitleTextStyle", editorXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistTitleTextStyle", learningXaml, StringComparison.Ordinal);
        Assert.Contains("IdeFieldRegistryCommandButtonStyle", editorXaml, StringComparison.Ordinal);
        Assert.Contains("IdeFieldRegistryCommandButtonStyle", learningXaml, StringComparison.Ordinal);
        Assert.Contains("IdeFieldRegistryDataGridStyle", editorXaml, StringComparison.Ordinal);
        Assert.Contains("IdeFieldRegistryDataGridStyle", learningXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeSecondary", editorXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeSecondary", learningXaml, StringComparison.Ordinal);
        Assert.Contains("IdeFieldRegistryRootStyle", managerXaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"状态与来源\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"备份与回滚\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"概括清理\"", managerXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistPopupFrameStyle", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistPopupFrameStyle", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistToolWindowRootStyle", findReferencesXaml, StringComparison.Ordinal);
        Assert.Contains("IdeFieldRegistryRootStyle", allowedValuesXaml, StringComparison.Ordinal);
        Assert.Contains("IdeFieldRegistryRootStyle", remotePresetXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeSecondary", allowedValuesXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeSecondary", remotePresetXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistBadgeStyle", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistBadgeStyle", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistPopupFrameStyle", completionDropdownXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistCompletionListStyle", completionDropdownXaml, StringComparison.Ordinal);
        Assert.Contains("Ra2CompletionDropdown.ItemsList", completionDropdownXaml, StringComparison.Ordinal);
        Assert.Contains("IdeFieldRegistryCommandButtonStyle", harvestXaml, StringComparison.Ordinal);
        Assert.Contains("IdeFieldRegistryDataGridStyle", harvestXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistDataGridStyle", findReferencesXaml, StringComparison.Ordinal);

        Assert.Contains("FieldRegistryCenter.FieldsGrid", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.HeaderChips", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.PriorityStrip", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.PriorityChipProject", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.PriorityChipGlobal", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.PriorityChipBuiltIn", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.StatusSummaryPanel", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.ProjectStatusCard", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.GlobalStatusCard", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.BuiltInStatusCard", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.WarningSummary", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.ActionGroup", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.FieldCountChip", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.SearchSummaryRow", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.ActivePacksCompactList", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.Details.EmptyState", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.Details.Inspector", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.Details.Trust", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.Details.Definition", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.Details.Description", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.Details.Examples", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryCenter.Details.AllowedValues", centerXaml, StringComparison.Ordinal);
        Assert.Contains("项目", centerXaml, StringComparison.Ordinal);
        Assert.Contains("全局", centerXaml, StringComparison.Ordinal);
        Assert.Contains("内置", centerXaml, StringComparison.Ordinal);
        Assert.Contains("Project", centerXaml, StringComparison.Ordinal);
        Assert.Contains("Global", centerXaml, StringComparison.Ordinal);
        Assert.Contains("BuiltIn", centerXaml, StringComparison.Ordinal);
        Assert.Contains("Manager.BuiltInFallbackDisplayText", centerXaml, StringComparison.Ordinal);
        Assert.Contains("Manager.WarningSummaryText", centerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldEditor.PersistedPreviewTextBox", editorXaml, StringComparison.Ordinal);
        Assert.Contains("FieldEditor.CustomChrome", editorXaml, StringComparison.Ordinal);
        Assert.Contains("FieldEditor.ChromeTitle", editorXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.CustomChrome", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.ChromeTitle", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.WorkflowStepStrip", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.SourceScrollHost", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.SourceSummary", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.TargetModeSummary", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.ReviewScrollHost", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.EmptyReviewState", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.ApplyBoundaryPanel", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.MainTabs", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.RawTextBox", learningXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldLearningWizard.SourceSection\"", learningXaml, StringComparison.Ordinal);
        Assert.Contains("MaxHeight=\"130\"", learningXaml, StringComparison.Ordinal);
        Assert.Contains("MaxHeight=\"260\"", learningXaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.MainFlowTabs", harvestXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RepairPreviewTabs", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.ActivePacksScrollHost", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.WarningsScrollHost", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupPreviewExpander", managerXaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupPreviewScrollHost", managerXaml, StringComparison.Ordinal);
        Assert.Contains("SourcePriorityText", managerXaml, StringComparison.Ordinal);
        Assert.Contains("BuiltInFallbackDisplayText", managerXaml, StringComparison.Ordinal);
        Assert.Contains("WarningChipText", managerXaml, StringComparison.Ordinal);
        Assert.Contains("ShortDirectoryPath", centerXaml + managerXaml, StringComparison.Ordinal);
        Assert.Contains("DirectoryPathToolTip", centerXaml + managerXaml, StringComparison.Ordinal);
        Assert.Contains("CleanupWriteWarningText", managerXaml, StringComparison.Ordinal);
        Assert.Contains("AllowedValuesEditor.CustomChrome", allowedValuesXaml, StringComparison.Ordinal);
        Assert.Contains("AllowedValuesEditor.ChromeTitle", allowedValuesXaml, StringComparison.Ordinal);
        Assert.Contains("FieldQuickPeek.TitleText", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"520\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"500\"", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("MinHeight=", quickPeekXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("MinHeight=", peekDefinitionXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Height=\"360\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Height=\"320\"", peekDefinitionXaml, StringComparison.Ordinal);

        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", learningXaml, StringComparison.Ordinal);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", harvestXaml, StringComparison.Ordinal);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", quickPeekXaml, StringComparison.Ordinal);
        Assert.Contains("DisplayMemberPath=\"DisplayName\"", learningXaml, StringComparison.Ordinal);
        Assert.Contains("DisplayMemberPath=\"DisplayName\"", harvestXaml, StringComparison.Ordinal);
        Assert.Contains("Title=\"远程来源预设\"", remotePresetXaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"名称\"", remotePresetXaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"说明\"", remotePresetXaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"确定\"", remotePresetXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Remote Source Preset", remotePresetXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"OK\"", remotePresetXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"Cancel\"", remotePresetXaml, StringComparison.Ordinal);

        Assert.Contains("字段库", combined, StringComparison.Ordinal);
        Assert.Contains("预览", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("锟", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("瀛", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("鏉", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("\\\"", combined, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoteSourcePresetEditor_ExposesStableAutomationIds()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "RemoteSourcePresetEditorWindow.xaml"));

        string[] requiredIds =
        [
            "RemoteSourcePresetEditor.Window",
            "RemoteSourcePresetEditor.NameTextBox",
            "RemoteSourcePresetEditor.UrlTextBox",
            "RemoteSourcePresetEditor.DescriptionTextBox",
            "RemoteSourcePresetEditor.TagsTextBox",
            "RemoteSourcePresetEditor.EnabledCheckBox",
            "RemoteSourcePresetEditor.ValidationText",
            "RemoteSourcePresetEditor.OkButton",
            "RemoteSourcePresetEditor.CancelButton"
        ];

        foreach (string id in requiredIds)
            Assert.Contains(id, xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void IdeApp_ExposesAutomationOpenFolderArgumentWithoutLegacyProjectServices()
    {
        string root = TestRepositoryRoot.Find();
        string appCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "App.xaml.cs"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("--automation-open-folder", appCode, StringComparison.Ordinal);
        Assert.Contains("OpenProjectFolderForAutomationAsync", shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", appCode + shellCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", appCode + shellCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", appCode + shellCode, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AutomationHarness_DoesNotUseTextChangedAutoParseOrApply()
    {
        string root = TestRepositoryRoot.Find();
        string text = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml")) +
            File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml")) +
            File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));

        Assert.DoesNotContain("TextChanged=\"Parse", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextChanged=\"Apply", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RollbackCommand", text, StringComparison.OrdinalIgnoreCase);
    }
}
