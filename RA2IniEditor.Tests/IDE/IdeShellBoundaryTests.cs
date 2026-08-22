using System.Xml.Linq;
using System.Reflection;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Views;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class IdeShellBoundaryTests
{
    [Fact]
    public void AiMarkdownRenderBudget_FallsBackForBlockCodeRowAndCellOverflow()
    {
        MethodInfo method = Assert.IsAssignableFrom<MethodInfo>(typeof(ShellWindow).GetMethod(
            "RequiresAiAssistantPlainTextFallback",
            BindingFlags.Static | BindingFlags.NonPublic));

        bool Invoke(IReadOnlyList<Ra2AiMarkdownBlock> blocks)
            => Assert.IsType<bool>(method.Invoke(null, [blocks]));

        Assert.False(Invoke([new Ra2AiMarkdownBlock { Text = "safe" }]));
        Assert.True(Invoke(Enumerable.Range(0, 257)
            .Select(_ => new Ra2AiMarkdownBlock { Text = "block" })
            .ToArray()));
        Assert.True(Invoke(Enumerable.Range(0, 65)
            .Select(_ => new Ra2AiMarkdownBlock
            {
                Kind = Ra2AiMarkdownBlockKind.Code,
                Text = "code"
            })
            .ToArray()));
        Assert.True(Invoke([
            new Ra2AiMarkdownBlock
            {
                Kind = Ra2AiMarkdownBlockKind.Table,
                TableHeaders = ["A", "B"],
                TableRows = Enumerable.Range(0, 201)
                    .Select(_ => (IReadOnlyList<string>)["1", "2"])
                    .ToArray()
            }
        ]));
        Assert.True(Invoke([
            new Ra2AiMarkdownBlock
            {
                Kind = Ra2AiMarkdownBlockKind.Table,
                TableHeaders = Enumerable.Range(0, 7).Select(index => $"H{index}").ToArray(),
                TableRows = Enumerable.Range(0, 171)
                    .Select(_ => (IReadOnlyList<string>)Enumerable.Repeat("cell", 7).ToArray())
                    .ToArray()
            }
        ]));
    }

    [Fact]
    public void IdeProject_ReferencesCoreAndInfrastructureWithoutLegacyReference()
    {
        string root = TestRepositoryRoot.Find();
        string ideProjectPath = Path.Combine(root, "RA2IniEditor.IDE", "RA2IniEditor.IDE.csproj");
        string projectText = File.ReadAllText(ideProjectPath);
        XDocument project = XDocument.Load(ideProjectPath);

        Assert.Contains("<TargetFramework>net8.0-windows</TargetFramework>", projectText);
        Assert.Contains("<UseWPF>true</UseWPF>", projectText);
        Assert.Contains("<PackageReference Include=\"AvalonEdit\" Version=\"6.3.0.90\" />", projectText);
        Assert.Contains("..\\RA2IniEditor.Application\\RA2IniEditor.Application.csproj", projectText);
        Assert.Contains("..\\RA2IniEditor.Core\\RA2IniEditor.Core.csproj", projectText);
        Assert.Contains("..\\RA2IniEditor.Infrastructure\\RA2IniEditor.Infrastructure.csproj", projectText);
        Assert.DoesNotContain("..\\RA2IniEditor.csproj", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RA2IniEditor.Tests.csproj", projectText, StringComparison.OrdinalIgnoreCase);

        string[] projectReferences = project.Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value ?? string.Empty)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "..\\RA2IniEditor.Application\\RA2IniEditor.Application.csproj",
                "..\\RA2IniEditor.Core\\RA2IniEditor.Core.csproj",
                "..\\RA2IniEditor.Infrastructure\\RA2IniEditor.Infrastructure.csproj"
            },
            projectReferences);
    }

    [Fact]
    public void LegacyProject_ExcludesIdeShellSourcesAndXaml()
    {
        string root = TestRepositoryRoot.Find();
        string legacyProjectPath = Path.Combine(root, "RA2IniEditor.csproj");
        if (!File.Exists(legacyProjectPath))
        {
            Assert.True(File.Exists(Path.Combine(root, "RA2IniEditor.IDE.sln")));
            Assert.False(File.Exists(legacyProjectPath));
            return;
        }

        string projectText = File.ReadAllText(legacyProjectPath);

        Assert.Contains("Compile Remove=\"RA2IniEditor.IDE\\**\\*.cs\"", projectText);
        Assert.Contains("Page Remove=\"RA2IniEditor.IDE\\**\\*.xaml\"", projectText);
        Assert.Contains("ApplicationDefinition Remove=\"RA2IniEditor.IDE\\**\\App.xaml\"", projectText);
    }

    [Fact]
    public void IdeShellWindow_DefinesExpectedPlaceholderRegions()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellText = File.ReadAllText(shellWindowPath);

        Assert.Contains("SourceEditor.DocumentTitle", shellText);
        Assert.Contains("ContentId=\"Tool.SectionExplorer\"", shellText);
        Assert.Contains("ProjectExplorer.Items", shellText);
        Assert.Contains("ProjectExplorer.StatusText", shellText);
        Assert.Contains("Click=\"FocusIssuesToolTab\"", shellText);
        Assert.Contains("Click=\"ToggleProjectExplorer\"", shellText);
        Assert.Contains("Click=\"OpenAiAssistantInRightToolWell\"", shellText);
        Assert.Contains("Source=\"{Binding IconKey, Converter={StaticResource IconKeyToDrawingImageConverter}}\"", shellText);
        Assert.Contains("ToolTip=\"{Binding IconText}\"", shellText);
        Assert.Contains("RenderOptions.BitmapScalingMode=\"HighQuality\"", shellText);
        Assert.DoesNotContain("Text=\"{Binding IconGlyph}\"", shellText);
        Assert.Contains("Text=\"{Binding DisplayTextWithCount}\"", shellText);
        Assert.Contains("ToolTip=\"{Binding ToolTipText}\"", shellText);
        Assert.Contains("IsCurrentFile", shellText);
        Assert.Contains("IsCurrentSection", shellText);
        Assert.Contains("x:Name=\"ShellDockManager\"", shellText);
        Assert.Contains("WindowState=\"Maximized\"", shellText);
        Assert.Contains("x:Name=\"SectionExplorerAnchorable\"", shellText);
        Assert.Contains("x:Name=\"ProjectExplorerTreeView\"", shellText);
        Assert.Contains("RightToolWell.Root", shellText);
        Assert.Contains("RightToolWell.SectionTab", shellText);
        Assert.Contains("RightToolWell.AiTab", shellText);
        Assert.Contains("RightToolWell.ActiveView", shellText);
        Assert.Contains("AiAssistant.Panel", shellText);
        Assert.Contains("AiAssistant.ChatHistory", shellText);
        Assert.Contains("AiAssistant.Composer", shellText);
        Assert.Contains("AiAssistant.AdvancedButton", shellText);
        Assert.Contains("AiAssistant.ModelSelector", shellText);
        Assert.Contains("AiAssistant.SafetyFooter", shellText);
        Assert.DoesNotContain("AiAssistant.ProviderSelector", shellText);
        Assert.DoesNotContain("AiAssistant.ProviderStatus", shellText);
        Assert.DoesNotContain("AiAssistant.DeepSeekEnvironmentHint", shellText);
        Assert.DoesNotContain("AiAssistant.TaskKindSelector", shellText);
        Assert.DoesNotContain("AiAssistant.Apply", shellText);
        Assert.DoesNotContain("AiAssistant.ApiKeyTextBox", shellText);
        Assert.DoesNotContain("AiAssistant.SaveApiKeyButton", shellText);
        Assert.Contains("Text=\"{Binding OutputText, Mode=OneWay}\"", shellText);
        Assert.DoesNotContain("RelativeSource", ExtractShellOutputTextBoxBlock(shellText));
        Assert.DoesNotContain("NavigatorListBox", shellText);
        Assert.DoesNotContain("Navigator.Items", shellText);
        Assert.DoesNotContain("FileSwitcher.Files", shellText);
        Assert.Contains("Text=\"{Binding Issues.StatusText}\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusBar\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusCurrentFileText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusDirtyStateText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusEncodingText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusNewlineText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusCaretPositionText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusSelectionText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusOperationText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusOperationKindText\"", shellText);
        Assert.DoesNotContain("MainWindowViewModel", shellText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeShellStatusBar_BindsCurrentFileDirtyEncodingNewlineCaretAndSelection()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string shellViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ShellViewModel.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellWindowCodePath);
        string viewModelText = File.ReadAllText(shellViewModelPath);
        string combinedText = shellText + shellCodeText + viewModelText;

        Assert.Contains("AutomationProperties.AutomationId=\"StatusBar\"", shellText);
        Assert.Contains("Text=\"{Binding StatusCurrentFileText}\"", shellText);
        Assert.Contains("Text=\"{Binding StatusDirtyStateText}\"", shellText);
        Assert.Contains("Text=\"{Binding StatusEncodingText}\"", shellText);
        Assert.Contains("Text=\"{Binding StatusNewlineText}\"", shellText);
        Assert.Contains("Text=\"{Binding StatusCaretPositionText}\"", shellText);
        Assert.Contains("Text=\"{Binding StatusSelectionText}\"", shellText);
        Assert.Contains("Text=\"{Binding StatusOperationText}\"", shellText);
        Assert.Contains("Text=\"{Binding StatusOperationKindText}\"", shellText);
        Assert.Contains("UpdateEditorCaretStatus", viewModelText);
        Assert.Contains("UpdateEditorTextStatus", viewModelText);
        Assert.Contains("UpdateDirtyStatus", viewModelText);
        Assert.Contains("SetOperationStatus", viewModelText);
        Assert.Contains("SourceTextEditor.TextArea.Caret.PositionChanged += SourceTextEditorCaret_OnPositionChanged", shellCodeText);
        Assert.Contains("SourceTextEditor.TextArea.SelectionChanged += SourceTextEditorSelection_OnChanged", shellCodeText);
        Assert.Contains("UpdateShellStatusBar", shellCodeText);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeShellOperationFeedback_UsesShortStatusMessagesWithoutChangingCoreServices()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string shellViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ShellViewModel.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellWindowCodePath);
        string shellViewModelText = File.ReadAllText(shellViewModelPath);
        string combinedText = shellText + shellCodeText + shellViewModelText;

        Assert.Contains("AutomationProperties.AutomationId=\"StatusOperationText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusOperationKindText\"", shellText);
        Assert.Contains("SetOperationStatus", shellViewModelText);
        Assert.Contains("UpdateSaveOperationStatus", shellCodeText);
        Assert.Contains("UpdateSaveOperationStatus", shellCodeText);
        Assert.Contains("ShortenStatusReason", shellCodeText);
        Assert.Contains("SetOperationStatus", shellViewModelText);
        Assert.Contains("RunManualFullDiagnosticsFromShell", shellCodeText);
        Assert.Contains("RunManualFullDiagnosticsAsync", shellCodeText);
        Assert.Contains("ReloadLocalFieldRegistryForReadonlyHighlighting", shellCodeText);
        Assert.Contains("ReloadFieldRegistryFromShell", shellCodeText);
        Assert.Contains("ReloadLocalFieldRegistryForReadonlyHighlighting", shellCodeText);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeShellWindow_EnablesFieldLearningAndResizableCollapsibleWorkspacePanels()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellWindowCodePath);
        string combinedText = shellText + shellCodeText;

        Assert.Contains("Shell.FieldRegistry.LearnFromCurrentSectionMenuItem", shellText);
        Assert.Contains("Shell.FieldRegistry.LearnFromCurrentIniMenuItem", shellText);
        Assert.Contains("Click=\"OpenFieldLearningFromCurrentSection\"", shellText);
        Assert.Contains("Click=\"OpenFieldLearningFromCurrentIni\"", shellText);
        Assert.Contains("OpenFieldLearningFromCurrentSection", shellCodeText);
        Assert.Contains("OpenFieldLearningFromCurrentIni", shellCodeText);
        Assert.Contains("TryGetCurrentSectionSourceForFieldRegistryHarvest", shellCodeText);
        Assert.Contains("LoadFieldLearningSource", shellCodeText);
        Assert.Contains("FieldLearningWizardWindow", shellCodeText);
        Assert.Contains("LoadCurrentIniHarvestPreview", shellCodeText);

        Assert.Contains("AutomationProperties.AutomationId=\"Shell.DockManager\"", shellText);
        Assert.Contains("GridSplitterWidth=\"4\"", shellText);
        Assert.Contains("GridSplitterHeight=\"4\"", shellText);
        Assert.Contains("DockWidth=\"300\"", shellText);
        Assert.Contains("DockHeight=\"260\"", shellText);
        Assert.Contains("ContentId=\"Tool.SectionExplorer\"", shellText);
        Assert.Contains("ContentId=\"Tool.Problems\"", shellText);
        Assert.Contains("Shell.View.ToggleBottomToolPanelMenuItem", shellText);
        Assert.Contains("Click=\"ToggleBottomToolPanel\"", shellText);
        Assert.Contains("ApplyBottomToolPanelVisibility", shellCodeText);
        Assert.Contains("_dockLayoutCoordinator.ShowAndActivate(\"Tool.SectionExplorer\")", shellCodeText);
        Assert.Contains("CaptureAndHideBottomTools", shellCodeText);
        Assert.Contains("RestoreBottomToolVisibilitySnapshot", shellCodeText);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeShellBoundary_MainWorkspaceHasCompactDockMargins()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellText = File.ReadAllText(shellWindowPath);

        Assert.Contains("Style=\"{StaticResource IdeDockPanelStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeMainMenuStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeAiWorkspaceRootStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeWorkspaceCommandBarStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeMainToolbarStyle}\"", shellText);
        Assert.Contains("BasedOn=\"{StaticResource IdeDocumentTabStripStyle}\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.DockManager\"", shellText);
        Assert.DoesNotContain("Style=\"{StaticResource IdeDocumentHeaderStyle}\"", shellText);
        Assert.DoesNotContain("Shell.SourceEditor.DocumentToolbar", shellText);
        Assert.DoesNotContain("Style=\"{StaticResource ShellPanelStyle}\"", shellText);
        Assert.Contains("DockWidth=\"300\"", shellText);
        Assert.Contains("DockHeight=\"260\"", shellText);
    }

    [Fact]
    public void IdeShellBoundary_EditorAndProjectExplorerUseAvalonDockLayout()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("<avalondock:DockingManager", shellText);
        Assert.Contains("<avalondock:LayoutDocument", shellText);
        Assert.Contains("ContentId=\"Document.Source\"", shellText);
        Assert.Contains("ContentId=\"Tool.SectionExplorer\"", shellText);
        Assert.Contains("x:Name=\"SourceTextEditor\"", shellText);
        Assert.Contains("BorderThickness=\"0\"", shellText);
        Assert.Contains("x:Name=\"SectionExplorerAnchorable\"", shellText);
        Assert.Contains("x:Name=\"ProjectExplorerTreeView\"", shellText);
        Assert.Contains("ItemsSource=\"{Binding ProjectExplorer.Items}\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.ProjectExplorer\"", shellText);
        Assert.Contains("SelectedItemChanged=\"ProjectExplorerTreeView_OnSelectedItemChanged\"", shellText);
        Assert.Contains("BorderThickness=\"0\"", shellText);
    }

    [Fact]
    public void IdeShellBoundary_RightToolWellAddsEmptyAiPageWithoutReplacingProjectExplorer()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCodeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("AutomationProperties.AutomationId=\"RightToolWell.Root\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"RightToolWell.SectionTab\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"RightToolWell.AiTab\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"RightToolWell.ActiveView\"", shellText);
        Assert.Contains("x:Name=\"RightToolWellSectionView\"", shellText);
        Assert.Contains("x:Name=\"AiAssistantSkeletonView\"", shellText);
        Assert.Contains("ContentId=\"Tool.AiAssistant\"", shellText);
        Assert.Contains("SelectedContentIndex=\"0\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.ChatHistory\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.Composer\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.PromptBox\"", shellText);
        Assert.Contains("AcceptsReturn=\"True\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("PreviewKeyDown=\"AiAssistantPromptBox_OnPreviewKeyDown\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("TextWrapping=\"Wrap\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("MinLines=\"2\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("Style=\"{StaticResource IdeAiComposerInputStyle}\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("MinHeight=\"48\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("MaxHeight=\"112\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("BorderThickness=\"0\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("Background=\"Transparent\"", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.GenerateButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.CancelButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.AdvancedButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.AdvancedOptions\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.ModelSelector\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.RequestPreparationNotice\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.ConfigurationStatus\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.ChatHistoryActions\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.ModelSelector\"", ExtractAiAssistantModelSelectorBlock(shellText));
        Assert.Contains("DisplayMemberPath=\"DisplayName\"", ExtractAiAssistantModelSelectorBlock(shellText));
        Assert.Contains("SelectedValuePath=\"Value\"", ExtractAiAssistantModelSelectorBlock(shellText));
        Assert.DoesNotContain("ComboBoxItem", ExtractAiAssistantModelSelectorBlock(shellText));
        Assert.DoesNotContain("SelectedIndex", ExtractAiAssistantModelSelectorBlock(shellText));
        Assert.Contains("MaxDropDownHeight=\"80\"", ExtractAiAssistantModelSelectorBlock(shellText));
        Assert.Contains("Style=\"{StaticResource UiComboBoxStyle}\"", ExtractAiAssistantModelSelectorBlock(shellText));
        Assert.Contains("Height=\"28\"", ExtractAiAssistantModelSelectorBlock(shellText));
        Assert.Contains("Background=\"Transparent\"", ExtractAiAssistantModelSelectorBlock(shellText));
        Assert.Contains("x:Name=\"AiAssistantAdvancedToggle\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeAiComposerAdvancedButtonStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeAiComposerSendButtonStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeAiComposerCancelButtonStyle}\"", shellText);
        string aiAssistantPanelText = ExtractAiAssistantPanelBlock(shellText);
        Assert.Contains("Data=\"{StaticResource IconGeometry.Action.Clear}\"", aiAssistantPanelText);
        Assert.Contains("Data=\"{StaticResource IconGeometry.Action.Advanced}\"", aiAssistantPanelText);
        Assert.Contains("Data=\"{StaticResource IconGeometry.Action.Send}\"", aiAssistantPanelText);
        Assert.Contains("Data=\"{StaticResource IconGeometry.Action.Cancel}\"", aiAssistantPanelText);
        Assert.DoesNotContain("Icon.Action.Clear", aiAssistantPanelText, StringComparison.Ordinal);
        Assert.DoesNotContain("Icon.Action.Advanced", aiAssistantPanelText, StringComparison.Ordinal);
        Assert.DoesNotContain("Icon.Action.Send", aiAssistantPanelText, StringComparison.Ordinal);
        Assert.DoesNotContain("Icon.Action.Cancel", aiAssistantPanelText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"发送\"", shellText);
        Assert.Contains("AutomationProperties.Name=\"取消生成\"", shellText);
        Assert.Contains("Binding IsChecked, ElementName=AiAssistantAdvancedToggle", ExtractAiAssistantAdvancedOptionsBlock(shellText));
        Assert.DoesNotContain("<Expander", ExtractAiAssistantPanelBlock(shellText));
        Assert.DoesNotContain("Style=\"{StaticResource IdeAiHeaderStyle}\"", ExtractAiAssistantPanelBlock(shellText));
        Assert.Contains("MaxHeight=\"44\"", ExtractAiAssistantPanelBlock(shellText));
        Assert.Contains("Style=\"{StaticResource IdeAiR2CompactContextTextStyle}\"", ExtractAiAssistantPanelBlock(shellText));
        Assert.Contains("Visibility=\"Collapsed\"", ExtractAiAssistantPanelBlock(shellText));
        Assert.Contains("ToolTip=\"{Binding Text, ElementName=AiAssistantContextSummaryText}\"", ExtractAiAssistantPanelBlock(shellText));
        Assert.DoesNotContain("AiAssistant.ModelSelector", ExtractAiAssistantAdvancedOptionsBlock(shellText));
        Assert.Matches("<Grid Grid.Column=\"2\"[^>]*>\\s*<Button x:Name=\"AiAssistantGenerateButton\"[\\s\\S]*?</Button>\\s*<Button x:Name=\"AiAssistantCancelButton\"", shellText);
        Assert.DoesNotContain("AiAssistant.CancelButton", ExtractAiAssistantAdvancedOptionsBlock(shellText));
        Assert.DoesNotContain("AiAssistant.CopyButton", ExtractAiAssistantAdvancedOptionsBlock(shellText));
        Assert.DoesNotContain("AiAssistant.ClearButton", ExtractAiAssistantAdvancedOptionsBlock(shellText));
        Assert.DoesNotContain("AiAssistant.AssistantMessageCopyButton", ExtractAiAssistantAdvancedOptionsBlock(shellText));
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"AiAssistant.ProviderSelector\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"AiAssistant.ProviderStatus\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"AiAssistant.DeepSeekEnvironmentHint\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"AiAssistant.ProviderMockOption\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"AiAssistant.ProviderDeepSeekOption\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"AiAssistant.TaskKindSelector\"", shellText);
        Assert.DoesNotContain("API Key 通过 DEEPSEEK_API_KEY", shellText);
        Assert.DoesNotContain("BaseUrl", shellText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Timeout", shellText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.DraftPreview\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.SafetyFooter\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeAiR2SafetyTextStyle}\"", shellText);
        Assert.Contains("AI 输出仅供参考；发送会联网，但不会修改文件。", shellText);
        Assert.Contains("x:Name=\"AiAssistantContextSummaryText\"", shellText);
        Assert.Contains("上下文：发送时会将当前文档的受限上下文", shellText);
        Assert.Contains("传输给所选 DeepSeek 模型", shellText);
        Assert.Contains("不会读取整个项目或修改文件", shellText);
        Assert.DoesNotContain("当前阶段仅占位，不会收集或发送上下文", shellText);
        Assert.Contains("RefreshAiAssistantContextSummary();", shellCodeText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.CurrentSubjectSummary\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.ConversationContextSummary\"", shellText);
        Assert.Contains("x:Name=\"AiAssistantCurrentSubjectSummaryText\"", shellText);
        Assert.Contains("x:Name=\"AiAssistantConversationContextSummaryText\"", shellText);
        Assert.Contains("RefreshAiAssistantContextSummary(BuildCurrentAiContext", shellCodeText);
        Assert.Contains("UpdateAiAssistantContextSummary", shellCodeText);
        Assert.Contains("FormatFieldEvidenceSummary", shellCodeText);
        Assert.Contains("字段依据 0", shellCodeText);
        Assert.Contains("FieldEvidenceTopKeysText", shellCodeText);
        Assert.Contains("FormatDiagnosticsSummary", shellCodeText);
        Assert.Contains("DiagnosticCount", shellCodeText);
        Assert.Contains("FormatAiCurrentSubjectSummary", shellCodeText);
        Assert.Contains("当前主题：无", shellText + shellCodeText);
        Assert.Contains("上一轮 AI 草稿，仅作草稿/建议，未写入项目文件", shellCodeText);
        Assert.Contains("FormatAiConversationContextSummary", shellCodeText);
        Assert.Contains("对话上下文：最近 0 轮，未截断", shellText);
        Assert.Contains("BuildAiAssistantConversationContext", shellCodeText);
        Assert.Contains("Ra2AiConversationContextProvider", shellCodeText);
        Assert.Contains("Ra2AiCurrentSubjectExtractor", shellCodeText);
        Assert.Contains("diagnosticIssues: viewModel.Issues.Items.ToArray()", shellCodeText);
        Assert.DoesNotContain("当前项目对象", shellText + shellCodeText);
        Assert.DoesNotContain("已写入 rulesmd.ini", shellText + shellCodeText);
        Assert.DoesNotContain("raw prompt", ExtractAiAssistantPanelBlock(shellText), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("provider metadata", ExtractAiAssistantPanelBlock(shellText), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Authorization", ExtractAiAssistantPanelBlock(shellText), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AutomationProperties.AutomationId=\"AiAssistant.EmptyStateMessage\"", shellText);
        Assert.Contains("AiAssistant.UserMessageList", shellCodeText);
        Assert.Contains("AiAssistant.AssistantMessageList", shellCodeText);
        Assert.Contains("AiAssistant.LatestAssistantMessage", shellText + shellCodeText);
        Assert.Contains("AiAssistant.AssistantMessageCopyButton", shellCodeText);
        Assert.Contains("CopyAiAssistantMessage(turn.Text)", shellCodeText);
        Assert.Contains("Ra2AiMarkdownResponseParser.Parse(text)", shellCodeText);
        Assert.Contains("AiAssistantMaximumMarkdownBlocks = 256", shellCodeText);
        Assert.Contains("AiAssistantMaximumMarkdownCodeBlocks = 64", shellCodeText);
        Assert.Contains("AiAssistantMaximumMarkdownTableRows = 200", shellCodeText);
        Assert.Contains("AiAssistantMaximumMarkdownTableCells = 1200", shellCodeText);
        Assert.Contains("RequiresAiAssistantPlainTextFallback(blocks)", shellCodeText);
        Assert.Contains("AiAssistant.MarkdownFallbackText", shellCodeText);
        Assert.Contains("IsReadOnly = true", shellCodeText);
        Assert.Contains("AiAssistant.CodeBlock", shellCodeText);
        Assert.Contains("AiAssistant.CodeBlockCopyButton", shellCodeText);
        Assert.Contains("AiAssistant.CodeBlockLanguage", shellCodeText);
        Assert.Contains("AiAssistant.MarkdownHeading", shellCodeText);
        Assert.Contains("AiAssistant.MarkdownParagraph", shellCodeText);
        Assert.Contains("AiAssistant.MarkdownListItem", shellCodeText);
        Assert.Contains("AiAssistant.MarkdownTable", shellCodeText);
        Assert.Contains("AiAssistant.MarkdownTableHeader", shellCodeText);
        Assert.Contains("AiAssistant.MarkdownTableRow", shellCodeText);
        Assert.Contains("AiAssistant.MarkdownTableCell", shellCodeText);
        Assert.Contains("Ra2AiMarkdownBlockKind.Heading", shellCodeText);
        Assert.Contains("Ra2AiMarkdownBlockKind.Bullet", shellCodeText);
        Assert.Contains("Ra2AiMarkdownBlockKind.Numbered", shellCodeText);
        Assert.Contains("Ra2AiMarkdownBlockKind.Table", shellCodeText);
        Assert.Contains("AppendAiAssistantInlineText", shellCodeText);
        Assert.Contains("FontWeights.Bold", shellCodeText);
        Assert.Contains("AiAssistant.MarkdownInlineCode", shellCodeText);
        Assert.Contains("text.IndexOf('`'", shellCodeText);
        Assert.Contains("IdeAiInlineCodeStyle", shellCodeText);
        Assert.DoesNotContain("new SolidColorBrush(Color.FromRgb", shellCodeText);
        Assert.DoesNotContain("CreateFrozenBrush", shellCodeText);
        Assert.Contains("IdeAiUserMessageStyle", shellCodeText);
        Assert.Contains("IdeAiAssistantMessageStyle", shellText + shellCodeText);
        Assert.Contains("IdeAiErrorMessageStyle", shellCodeText);
        Assert.Contains("IdeAiMarkdownTableStyle", shellCodeText);
        Assert.Contains("IdeAiCodeBlockStyle", shellCodeText);
        Assert.Contains("IdeHoverCardStyle", shellCodeText);
        Assert.Contains("IdeHoverCodePillStyle", shellCodeText);
        Assert.Contains("FindRequiredVisualResource", shellCodeText);
        Assert.Contains("CopyAiAssistantCodeBlock(codeText)", shellCodeText);
        Assert.Contains("Clipboard.SetText(codeText)", shellCodeText);
        Assert.Contains("Click=\"GenerateAiAssistantResponse\"", shellText);
        Assert.Contains("Click=\"CancelAiAssistantResponse\"", shellText);
        Assert.Contains("Click=\"ClearAiAssistantMessages\"", shellText);
        Assert.Contains("Ra2AiAssistantPipeline", shellCodeText);
        Assert.Contains("new Ra2AiPromptBuilder()", shellCodeText);
        Assert.DoesNotContain("FakeRa2AiClient", shellText + shellCodeText);
        Assert.DoesNotContain("Ra2AiProviderMode", shellCodeText);
        Assert.Contains("DeepSeekRa2AiModelCatalog.Options", shellCodeText);
        Assert.Contains("DeepSeekRa2AiModelCatalog.Default", shellCodeText);
        Assert.Contains("AiAssistantModelSelector.SelectedValue is DeepSeekRa2AiModel", shellCodeText);
        Assert.Contains("CreateAiAssistantPipeline", shellCodeText);
        Assert.Contains("pipeline.SendStreamingAsync", shellCodeText);
        Assert.Contains("Ra2AiIncrementalTextBuffer", shellCodeText);
        Assert.Contains("AiAssistantStreamFlushIntervalMilliseconds = 50", shellCodeText);
        Assert.Contains("AiAssistantStreamImmediateFlushThresholdCharacters = 512", shellCodeText);
        Assert.Contains("x:Name=\"AiAssistantChatScrollViewer\"", shellText);
        Assert.Contains("IsAiAssistantChatNearBottom", shellCodeText);
        Assert.Contains("AiAssistantChatScrollViewer.ScrollToEnd()", shellCodeText);
        Assert.Contains("handle.Buffer.AccumulatedTextEquals(response.Text)", shellCodeText);
        Assert.Contains("流式响应一致性校验失败", shellCodeText);
        Assert.Contains("Ra2AiRequestLifecycle _aiAssistantRequestLifecycle = new()", shellCodeText);
        Assert.Contains("TryStart(out Ra2AiRequestSession? requestSession)", shellCodeText);
        Assert.Contains("requestSession.Token", shellCodeText);
        Assert.Contains("_aiAssistantRequestLifecycle.TryCancelCurrent()", shellCodeText);
        Assert.Contains("_aiAssistantRequestLifecycle.TryComplete(requestSession)", shellCodeText);
        Assert.Contains("requestSession.Dispose()", shellCodeText);
        Assert.DoesNotContain("_isAiAssistantSending", shellCodeText);
        Assert.DoesNotContain("_aiAssistantCancellationSource", shellCodeText);
        Assert.DoesNotContain("SetAiAssistantSendingState(false)", ExtractAiAssistantCancelMethodBlock(shellCodeText));
        Assert.Contains("AiAssistantModelSelector.IsEnabled = !isSending;", shellCodeText);
        Assert.Contains("AiAssistantMaximumTerminalMessageCards = 60", shellCodeText);
        Assert.Contains("TrimAiAssistantTerminalMessageHistory();", shellCodeText);
        Assert.Contains("State: not Ra2AiConversationTurnState.InProgress", shellCodeText);
        Assert.Contains("Children.RemoveAt(assistantIndex)", shellCodeText);
        Assert.Contains("Children.RemoveAt(userIndex)", shellCodeText);
        Assert.Contains("AiAssistantClearButton.IsEnabled = !isSending && HasAiAssistantMessages();", shellCodeText);
        Assert.Contains("Ra2AiConversationTurnState turnState = Ra2AiConversationTurnState.Completed", shellCodeText);
        Assert.Contains("State = turnState", shellCodeText);
        Assert.Contains("GetAiAssistantConversationTurnState(response.Kind)", shellCodeText);
        Assert.Contains("Ra2AiResponseKind.Incomplete or Ra2AiResponseKind.Cancelled or Ra2AiResponseKind.Timeout", shellCodeText);
        Assert.Contains("turnState: Ra2AiConversationTurnState.Incomplete", shellCodeText);
        Assert.Contains("turnState: Ra2AiConversationTurnState.Error", shellCodeText);
        Assert.Contains("DeepSeekRa2AiClientFactory.CreateConfigurationSnapshot(selectedModel)", shellCodeText);
        Assert.Contains("DeepSeekRa2AiClientFactory.CreateClient(configurationSnapshot)", shellCodeText);
        Assert.Contains("UpdateAiAssistantConfigurationStatus(configurationSnapshot)", shellCodeText);
        Assert.Contains("snapshot.UsesCustomEndpoint ? \"自定义端点\" : \"官方端点\"", shellCodeText);
        Assert.Contains("UpdateAiAssistantRequestPreparationNotice(result.Request)", shellCodeText);
        Assert.Contains("AiAssistant.RequestDiagnostics", shellCodeText);
        Assert.DoesNotContain("diagnostics.ToString()", shellCodeText);
        Assert.DoesNotContain("new DeepSeekRa2AiClient", shellCodeText);
        Assert.DoesNotContain("HttpClient", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FormatDeepSeekAiAssistantResponse", shellCodeText);
        Assert.Contains("DeepSeekRa2AiFailureUiMessageFormatter.FormatStandaloneMessage", shellCodeText);
        Assert.Contains("DeepSeekRa2AiFailureUiMessageFormatter.FormatPartialTerminalStatus", shellCodeText);
        Assert.Contains("response.FailureKind != Ra2AiFailureKind.None", shellCodeText);
        Assert.Contains("IsAiAssistantErrorMessage(response.Kind)", shellCodeText);
        Assert.DoesNotContain("IsTimeoutError", shellCodeText);
        Assert.DoesNotContain("response.ErrorMessage", ExtractDeepSeekAiAssistantResponseFormatterBlock(shellCodeText));
        Assert.Contains("if (string.IsNullOrWhiteSpace(prompt))", shellCodeText);
        Assert.Contains("AiAssistantMaximumUserPromptCharacters = 8000", shellCodeText);
        Assert.Contains("if (rawPrompt.Length > AiAssistantMaximumUserPromptCharacters)", shellCodeText);
        Assert.Contains("输入内容已保留，尚未发送", shellCodeText);
        Assert.DoesNotContain("MaxLength=", ExtractAiAssistantPromptBoxBlock(shellText));
        Assert.True(
            shellCodeText.IndexOf("rawPrompt.Length > AiAssistantMaximumUserPromptCharacters", StringComparison.Ordinal)
            < shellCodeText.IndexOf("_aiAssistantRequestLifecycle.TryStart", StringComparison.Ordinal));
        Assert.Contains("AiAssistantPromptBox_OnPreviewKeyDown", shellText + shellCodeText);
        Assert.Contains("GenerateAiAssistantResponse(AiAssistantGenerateButton, new RoutedEventArgs())", shellCodeText);
        Assert.Contains("e.Key != Key.Enter", shellCodeText);
        Assert.Contains("ModifierKeys.Shift", shellCodeText);
        Assert.Contains("Clipboard.SetText(text)", shellCodeText);
        Assert.Contains("Border? userMessageBorder = null;", shellCodeText);
        Assert.Contains("AddAiAssistantStreamingMessage(requestSession, userMessageBorder)", shellCodeText);
        Assert.Contains("public Border UserMessageBorder { get; }", shellCodeText);
        Assert.Contains("public StackPanel ActionPanel { get; }", shellCodeText);
        Assert.Contains("bool isContextEligible = turnState == Ra2AiConversationTurnState.Completed;", shellCodeText);
        Assert.Contains("SetAiAssistantMessageContextEligibility(handle.UserMessageBorder, isContextEligible)", shellCodeText);
        Assert.Contains("recoveryUserMessageBorder: userMessageBorder", shellCodeText);
        Assert.Contains("AiAssistant.RestorePromptButton", shellCodeText);
        Assert.Contains("AiAssistant.RestorePromptStatus", shellCodeText);
        Assert.Contains("仅恢复文本，不会自动发送；再次发送可能产生服务费用。", shellCodeText);
        Assert.Contains("提示词已恢复到输入框，尚未发送。", shellCodeText);
        Assert.Contains("输入框已有内容，未覆盖。", shellCodeText);
        string restoreMethodBlock = ExtractAiAssistantRestorePromptMethodBlock(shellCodeText);
        Assert.Contains("if (!string.IsNullOrWhiteSpace(AiAssistantPromptBox.Text))", restoreMethodBlock);
        Assert.Contains("AiAssistantPromptBox.Text = prompt;", restoreMethodBlock);
        Assert.Contains("AiAssistantPromptBox.Focus();", restoreMethodBlock);
        Assert.Contains("AiAssistantPromptBox.CaretIndex = AiAssistantPromptBox.Text.Length;", restoreMethodBlock);
        Assert.DoesNotContain("GenerateAiAssistantResponse", restoreMethodBlock);
        Assert.DoesNotContain("SendStreamingAsync", restoreMethodBlock);
        Assert.DoesNotContain("CreateAiAssistantPipeline", restoreMethodBlock);
        Assert.DoesNotContain("CopyLatestAiAssistantResponse", shellText + shellCodeText);
        Assert.DoesNotContain("_latestAiAssistantResponse", shellCodeText);
        Assert.DoesNotContain("意图：自动判断", shellText);
        Assert.Contains("x:Name=\"ProjectExplorerTreeView\"", shellText);
        Assert.Contains("ItemsSource=\"{Binding ProjectExplorer.Items}\"", shellText);
        Assert.Contains("ProjectExplorerTreeView.UpdateLayout();", shellCodeText);
        Assert.Contains("container.BringIntoView();", shellCodeText);
        Assert.Contains("SetRightToolWellAiViewVisible(false);", shellCodeText);
        Assert.Contains("OpenAiAssistantInRightToolWell", shellText + shellCodeText);
        Assert.Contains("CloseAiAssistantInRightToolWell", shellText + shellCodeText);
        Assert.DoesNotContain("MockRa2AiClient", shellText + shellCodeText);
        Assert.DoesNotContain("DeepSeekClient", shellText + shellCodeText);
        Assert.DoesNotContain("DeepSeekAdapter", shellText + shellCodeText);
        Assert.DoesNotContain("GetEnvironmentVariable", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DEEPSEEK_BASE_URL", shellText + shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DEEPSEEK_MODEL", shellText + shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DEEPSEEK_TIMEOUT_SECONDS", shellText + shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AiAssistant.ApiKeyTextBox", shellText + shellCodeText);
        Assert.DoesNotContain("AiAssistant.SaveApiKeyButton", shellText + shellCodeText);
        Assert.DoesNotContain("settings persistence", shellText + shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AiAssistant.Apply", shellText + shellCodeText);
        Assert.DoesNotContain("AiAssistant.InsertButton", shellText + shellCodeText);
        Assert.DoesNotContain("Apply / Insert", ExtractAiAssistantPanelBlock(shellText));
        Assert.DoesNotContain("WebView", shellText + shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Markdig", shellText + shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SourceEditor.Text =", ExtractAiAssistantCopyMethodsBlock(shellCodeText), StringComparison.Ordinal);
        Assert.DoesNotContain("MarkDirty", ExtractAiAssistantCopyMethodsBlock(shellCodeText), StringComparison.Ordinal);
        Assert.DoesNotContain("IsDirty", ExtractAiAssistantCopyMethodsBlock(shellCodeText), StringComparison.Ordinal);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"AiAssistant.CloseButton\"", shellText);
        Assert.DoesNotContain("Content=\"+\"", ExtractAiAssistantPanelBlock(shellText));
    }

    [Fact]
    public void IdeShellBoundary_BottomToolWindowAndFloatingSearchUseApprovedTopology()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("ContentId=\"Tool.Problems\"", shellText);
        Assert.Contains("ContentId=\"Tool.Output\"", shellText);
        Assert.Contains("ContentId=\"Tool.Search\"", shellText);
        Assert.Contains("ContentId=\"Tool.FindReferences\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.BottomToolTabs\"", shellText);
        Assert.Contains("Margin=\"0\"", shellText);
        Assert.Contains("DockHeight=\"260\"", shellText);
        Assert.Contains("Shell.BottomToolTabs.ErrorList", shellText);
        Assert.Contains("Shell.BottomToolTabs.Output", shellText);
        Assert.Contains("Shell.Tool.Search.Content", shellText);
        Assert.Contains("FloatingHeight=\"620\"", shellText);
        Assert.Contains("FloatingWidth=\"560\"", shellText);
    }

    [Fact]
    public void IdeShellBoundary_HasCompactVerticalAndHorizontalSplitters()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("GridSplitterWidth=\"4\"", shellText);
        Assert.Contains("GridSplitterHeight=\"4\"", shellText);
        Assert.Contains("DockWidth=\"300\"", shellText);
        Assert.Contains("DockHeight=\"260\"", shellText);
    }

    [Fact]
    public void IdeShellBoundary_DoesNotUseLegacyLargeCardMargins()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string[] forbiddenShellLayoutFragments =
        [
            "Margin=\"12,12,6,8\"",
            "Margin=\"6,12,12,8\"",
            "Margin=\"12,4,12,8\"",
            "Margin=\"12,0,12,0\"",
            "Margin=\"0,12,0,0\"",
            "Padding=\"10\"",
            "Width=\"5\"",
            "Height=\"5\""
        ];

        foreach (string fragment in forbiddenShellLayoutFragments)
            Assert.DoesNotContain(fragment, shellText, StringComparison.Ordinal);
    }

    [Fact]
    public void IdeShellBoundary_DockStylesAreScopedToShellTheme()
    {
        string root = TestRepositoryRoot.Find();
        string shellThemeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "ShellTheme.xaml"));
        string iconResourcesText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IconResources.xaml"));

        Assert.Contains("x:Key=\"IdeDockPanelStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"ShellTopChromeBrush\"", shellThemeText);
        Assert.Contains("x:Key=\"ShellMenuBarBrush\"", shellThemeText);
        Assert.Contains("x:Key=\"ShellToolbarBrush\"", shellThemeText);
        Assert.Contains("x:Key=\"ShellTopChromeInnerDividerBrush\"", shellThemeText);
        Assert.Contains("x:Key=\"ShellToolbarSeparatorBrush\"", shellThemeText);
        Assert.Contains("x:Key=\"ShellToolbarBottomBorderBrush\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeMainMenuStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeMainMenuItemStyle\"", shellThemeText);
        Assert.Contains("BasedOn=\"{StaticResource UiMenuStyle}\"", shellThemeText);
        Assert.Contains("BasedOn=\"{StaticResource UiMenuItemStyle}\"", shellThemeText);
        Assert.Contains("<Setter Property=\"Padding\" Value=\"5,3\" />", shellThemeText);
        Assert.DoesNotContain("<Setter Property=\"Padding\" Value=\"3,3\" />", shellThemeText);
        Assert.DoesNotContain("<Setter Property=\"Padding\" Value=\"9,3\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"VerticalContentAlignment\" Value=\"Center\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"MinHeight\" Value=\"26\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"Background\" Value=\"{StaticResource ShellTopChromeBrush}\" />", shellThemeText);
        Assert.Contains("x:Key=\"IdeDocumentHeaderStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeMainToolbarStyle\"", shellThemeText);
        Assert.Contains("<Setter Property=\"BorderBrush\" Value=\"{StaticResource ShellToolbarBottomBorderBrush}\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"MinHeight\" Value=\"30\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"Padding\" Value=\"4,2\" />", shellThemeText);
        Assert.Contains("x:Key=\"IdeDocumentTabStripStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeDocumentTabStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeDocumentTabTitleTextStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeDocumentDirtyMarkerStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeIconCommandButtonStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdePrimaryIconCommandButtonStyle\"", shellThemeText);
        Assert.Contains("BasedOn=\"{StaticResource UiIconButtonStyle}\"", shellThemeText);
        Assert.Contains("<Setter Property=\"Width\" Value=\"26\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"Height\" Value=\"26\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"Margin\" Value=\"0\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"Padding\" Value=\"5\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"Background\" Value=\"Transparent\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"BorderBrush\" Value=\"Transparent\" />", shellThemeText);
        Assert.Contains("<Trigger Property=\"IsMouseOver\" Value=\"True\">", shellThemeText);
        Assert.Contains("<Trigger Property=\"IsPressed\" Value=\"True\">", shellThemeText);
        Assert.Contains("<Trigger Property=\"IsEnabled\" Value=\"False\">", shellThemeText);
        Assert.Contains("x:Key=\"IdeCommandSeparatorStyle\"", shellThemeText);
        Assert.Contains("<Setter Property=\"Height\" Value=\"16\" />", shellThemeText);
        Assert.Contains("<Setter Property=\"Margin\" Value=\"3,0\" />", shellThemeText);
        Assert.Contains("x:Key=\"IconOpenFolder\"", iconResourcesText);
        Assert.Contains("x:Key=\"IconUndo\"", iconResourcesText);
        Assert.Contains("x:Key=\"IconRedo\"", iconResourcesText);
        Assert.Contains("x:Key=\"IconSave\"", iconResourcesText);
        Assert.Contains("x:Key=\"IconRevert\"", iconResourcesText);
        Assert.Contains("x:Key=\"IconSearch\"", iconResourcesText);
        Assert.Contains("x:Key=\"IconFieldRegistry\"", iconResourcesText);
        Assert.Contains("x:Key=\"IconIssues\"", iconResourcesText);
        Assert.Contains("x:Key=\"IdeToolWindowHeaderStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeToolWindowTitleStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeToolWindowStatusStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeBottomToolCommandBarStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeBottomToolDataGridStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeOutputTextSurfaceStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeToolFooterStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeShellStatusBarStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeSplitterStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeToolWindowTabControlStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeToolWindowDataGridStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeCompactTextBoxStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeCompactComboBoxStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeCompactButtonStyle\"", shellThemeText);
        Assert.Contains("x:Key=\"IdeBlueCompactButtonStyle\"", shellThemeText);
        Assert.Contains("BasedOn=\"{StaticResource UiAccentButtonStyle}\"", shellThemeText);
        Assert.DoesNotContain("#EAF4FF", shellThemeText);
        Assert.DoesNotContain("#155A9C", shellThemeText);
        Assert.Contains("x:Key=\"IdeEmptyStateTextStyle\"", shellThemeText);
        Assert.Contains("<Setter Property=\"Padding\" Value=\"0\" />", shellThemeText);
    }

    [Fact]
    public void IdeShellBoundary_DoesNotChangeEditorLifecycleBindings()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("PlacementTarget=\"{Binding ElementName=SourceTextEditor}\"", shellText);
        Assert.Contains("LostKeyboardFocus=\"SourceTextEditor_OnLostKeyboardFocus\"", shellText);
        Assert.Contains("PreviewKeyDown=\"SourceTextEditor_OnPreviewKeyDown\"", shellText);
        Assert.Contains("PreviewMouseRightButtonDown=\"SourceTextEditor_OnPreviewMouseRightButtonDown\"", shellText);
        Assert.Contains("TextChanged=\"SourceTextEditor_OnTextChanged\"", shellText);
        Assert.Contains("Click=\"SaveCurrentFile_OnClick\"", shellText);
        Assert.Contains("Click=\"UndoCurrentFile_OnClick\"", shellText);
        Assert.Contains("Click=\"RedoCurrentFile_OnClick\"", shellText);
        Assert.Contains("Click=\"FocusIssuesToolTab\"", shellText);
        Assert.Contains("ContentId=\"Document.Source\"", shellText);
        Assert.Contains("CanClose=\"False\"", shellText);
        Assert.Contains("CanFloat=\"False\"", shellText);
    }

    [Fact]
    public void IdeShellBoundary_UsesVsStyleMainToolbarAndDocumentTabStrip()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string toolbarText = ExtractShellMainToolbarBlock(shellText);
        string tabStripText = ExtractShellDocumentTabStripBlock(shellText);

        Assert.True(
            shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.MainMenu\"", StringComparison.Ordinal) <
            shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.MainToolbar\"", StringComparison.Ordinal));
        Assert.Contains("Style=\"{StaticResource IdeMainMenuStyle}\"", shellText);
        Assert.True(
            shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.MainToolbar\"", StringComparison.Ordinal) <
            shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.SourceEditor.DocumentTabStrip\"", StringComparison.Ordinal));
        Assert.True(
            shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.SourceEditor.DocumentTabStrip\"", StringComparison.Ordinal) <
            shellText.IndexOf("x:Name=\"SourceTextEditor\"", StringComparison.Ordinal));
        Assert.True(
            shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.EditorColumn\"", StringComparison.Ordinal) <
            shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.SourceEditor.DocumentTabStrip\"", StringComparison.Ordinal));
        Assert.True(
            shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.SourceEditor.DocumentTabStrip\"", StringComparison.Ordinal) <
            shellText.IndexOf("x:Name=\"SectionExplorerAnchorable\"", StringComparison.Ordinal));

        Assert.Contains("Shell.SourceEditor.DocumentTabStrip", tabStripText);
        Assert.Contains("Shell.SourceEditor.DocumentTab", tabStripText);
        Assert.Contains("Shell.SourceEditor.DocumentTabTitle", tabStripText);
        Assert.Contains("Shell.SourceEditor.DocumentDirtyMarker", tabStripText);
        Assert.Contains("Grid.Row=\"0\"", tabStripText);
        Assert.Contains("HorizontalAlignment=\"Stretch\"", tabStripText);
        Assert.Contains("BasedOn=\"{StaticResource IdeDocumentTabStripStyle}\"", tabStripText);
        Assert.Contains("Style=\"{StaticResource IdeDocumentTabStyle}\"", tabStripText);
        Assert.Contains("HorizontalAlignment=\"Left\"", tabStripText);
        Assert.Contains("Text=\"{Binding SourceEditor.DocumentTitle}\"", tabStripText);
        Assert.Contains("ToolTip=\"{Binding StatusCurrentFileText}\"", tabStripText);
        Assert.Contains("ToolTip=\"{Binding StatusDirtyStateText}\"", tabStripText);
        Assert.Contains("Value=\"未选择文件\"", tabStripText);
        Assert.Contains("Value=\"Collapsed\"", tabStripText);
        Assert.DoesNotContain("Grid.ColumnSpan", tabStripText);
        Assert.DoesNotContain("Grid.Column=\"2\"", tabStripText);
        Assert.DoesNotContain("ItemsSource", tabStripText);
        Assert.DoesNotContain("Close", tabStripText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ContextMenu", tabStripText);
        Assert.DoesNotContain("Drag", tabStripText, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("Shell.MainToolbar", toolbarText);
        Assert.Contains("Style=\"{StaticResource IdeMainToolbarStyle}\"", toolbarText);
        Assert.DoesNotContain("Text=\"{Binding SourceEditor.DocumentTitle}\"", toolbarText);
        Assert.DoesNotContain("SourceEditor.MetadataText", toolbarText);
        Assert.DoesNotContain("编辑状态：", toolbarText);
        Assert.DoesNotContain("没有未保存", toolbarText);
        Assert.DoesNotContain("Content=\"保存当前文件\"", toolbarText);
        Assert.DoesNotContain("Content=\"撤销内存修改\"", toolbarText);
        Assert.DoesNotContain("MinWidth=\"92\"", toolbarText);
        Assert.DoesNotContain("MinWidth=\"128\"", toolbarText);

        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.OpenFolderButton\"", toolbarText);
        Assert.Contains("AutomationProperties.Name=\"打开目录\"", toolbarText);
        Assert.Contains("ToolTip=\"打开项目目录\"", toolbarText);
        Assert.Contains("Content=\"{StaticResource IconOpenFolder}\"", toolbarText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.UndoButton\"", toolbarText);
        Assert.Contains("AutomationProperties.Name=\"撤销\"", toolbarText);
        Assert.Contains("ToolTip=\"撤销上一步编辑\"", toolbarText);
        Assert.Contains("Content=\"{StaticResource IconUndo}\"", toolbarText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.RedoButton\"", toolbarText);
        Assert.Contains("AutomationProperties.Name=\"重做\"", toolbarText);
        Assert.Contains("ToolTip=\"重做上一步撤销\"", toolbarText);
        Assert.Contains("Content=\"{StaticResource IconRedo}\"", toolbarText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.SaveCurrentFileButton\"", toolbarText);
        Assert.Contains("AutomationProperties.Name=\"保存\"", toolbarText);
        Assert.Contains("ToolTip=\"保存当前文件\"", toolbarText);
        Assert.Contains("Content=\"{StaticResource IconSave}\"", toolbarText);
        Assert.Contains("Style=\"{StaticResource IdeIconCommandButtonStyle}\"", toolbarText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.RevertInMemoryChangesButton\"", toolbarText);
        Assert.Contains("AutomationProperties.Name=\"放弃修改\"", toolbarText);
        Assert.Contains("ToolTip=\"放弃当前未保存修改\"", toolbarText);
        Assert.Contains("Content=\"{StaticResource IconRevert}\"", toolbarText);
        Assert.Contains("Style=\"{StaticResource IdeIconCommandButtonStyle}\"", toolbarText);
        Assert.Contains("Style=\"{StaticResource IdeCommandSeparatorStyle}\"", toolbarText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.SearchButton\"", toolbarText);
        Assert.Contains("ToolTip=\"搜索当前项目\"", toolbarText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.FieldRegistryButton\"", toolbarText);
        Assert.Contains("ToolTip=\"打开字段库中心\"", toolbarText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.IssuesButton\"", toolbarText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.WindowLayoutButton\"", toolbarText);
        Assert.Contains("<Grid Width=\"16\" Height=\"16\" SnapsToDevicePixels=\"True\">", toolbarText);
        Assert.Contains("Data=\"M 11,13 L 13,15 L 15,13\"", toolbarText);
        Assert.DoesNotContain("Data=\"M 11,14 L 13,16 L 15,14\"", toolbarText);
        Assert.Contains("ToolTip=\"显示问题面板\"", toolbarText);

        Assert.Contains("AutomationProperties.AutomationId=\"StatusCurrentFileText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusDirtyStateText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusEncodingText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusNewlineText\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"StatusCaretPositionText\"", shellText);
        Assert.DoesNotContain("Shell.SourceEditor.DocumentToolbar", shellText);
    }

    [Fact]
    public void IdeShellBoundary_MainToolbarStateCleanupKeepsEditorActionsContextual()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCodeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.OpenFolderButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.SearchButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.FieldRegistryButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.IssuesButton\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"Shell.FieldRegistryButton\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"Shell.MainToolbar.SaveButton\"", shellText);

        Assert.Contains("bool hasEditableDirtySession = editorState.HasSession && editorState.IsDirty;", shellCodeText);
        Assert.Contains("bool hasEditableSession = editorState.HasSession && editorState.IsEditing;", shellCodeText);
        Assert.Contains("bool canRevertDirtySession = editorState.CanRevertInMemoryChanges && editorState.IsDirty;", shellCodeText);
        Assert.Contains("SaveCurrentFileButton.IsEnabled = hasEditableDirtySession;", shellCodeText);
        Assert.Contains("RevertInMemoryChangesButton.IsEnabled = canRevertDirtySession;", shellCodeText);
        Assert.Contains("UndoCurrentFileButton.IsEnabled = hasEditableSession && CanUndoSourceEditor();", shellCodeText);
        Assert.Contains("RedoCurrentFileButton.IsEnabled = hasEditableSession && CanRedoSourceEditor();", shellCodeText);
    }

    [Fact]
    public void IdeShellBoundary_ProjectExplorerUsesCompactToolWindowLayout()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("ContentId=\"Tool.SectionExplorer\"", shellText);
        Assert.Contains("Title=\"项目浏览器\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeToolWindowStatusStyle}\"", shellText);
        Assert.Contains("BorderThickness=\"0\"", shellText);
        Assert.Contains("<Setter Property=\"MinHeight\"", shellText);
        Assert.Contains("<Setter Property=\"Padding\"", shellText);
        Assert.Contains("Width=\"18\"", shellText);
        Assert.Contains("Height=\"18\"", shellText);
        Assert.Contains("RenderOptions.BitmapScalingMode=\"HighQuality\"", shellText);
        Assert.DoesNotContain("MinWidth=\"34\"", shellText);
    }

    [Fact]
    public void IdeShellBoundary_BottomToolWindowsUseCompactControlsAndReadonlyOutput()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("ContentId=\"Tool.Problems\"", shellText);
        Assert.Contains("ContentId=\"Tool.Output\"", shellText);
        Assert.Contains("ContentId=\"Tool.Search\"", shellText);
        // The Copilot composer now owns the primary action through its scoped style.
        Assert.Contains("Style=\"{StaticResource IdeAiComposerSendButtonStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeIssueDataGridStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeOutputTextSurfaceStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeWorkspaceCommandButtonStyle}\"", shellText);
        Assert.DoesNotContain("Shell.BottomIssues.SeverityFilterComboBox", shellText);
        Assert.DoesNotContain("Shell.BottomIssues.SourceFilterTextBox", shellText);
        Assert.DoesNotContain("Shell.BottomIssues.SearchTextBox", shellText);
        Assert.DoesNotContain("Shell.BottomIssues.ClearFiltersButton", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.BottomIssues.RefreshCurrentFileButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.BottomIssues.RunFullDiagnosticsButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.BottomIssues.ClearButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.BottomIssues.Grid\"", shellText);
        Assert.Contains("Text=\"{Binding Issues.StatusText}\"", shellText);
        Assert.Contains("Shell.BottomIssues.Filter.Error", shellText);
        Assert.Contains("Shell.BottomIssues.Filter.Warning", shellText);
        Assert.Contains("Shell.BottomIssues.Filter.Info", shellText);
        Assert.Contains("Shell.BottomIssues.Count.Error", shellText);
        Assert.Contains("Shell.BottomIssues.Count.Warning", shellText);
        Assert.Contains("Shell.BottomIssues.Count.Info", shellText);
        Assert.Contains("CellTemplate=\"{StaticResource IdeIssueSeverityIconTemplate}\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.OutputTextBox\"", shellText);
        Assert.Contains("IsReadOnly=\"True\"", shellText);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", shellText);
        Assert.Contains("Text=\"{Binding OutputText, Mode=OneWay}\"", shellText);
        Assert.DoesNotContain("FindAncestor", shellText);
        Assert.Contains("<views:SearchToolView />", shellText);
    }

    [Fact]
    public void IdeToolWindows_UseCompactLayoutAndKeepAutomationIds()
    {
        string root = TestRepositoryRoot.Find();
        string issuesText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "IssuesToolWindow.xaml"));
        string searchViewText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "SearchToolView.xaml"));
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("AutomationProperties.AutomationId=\"Issues.Grid\"", issuesText);
        Assert.Contains("Issues.SeverityFilterComboBox", issuesText);
        Assert.Contains("Issues.SourceFilterTextBox", issuesText);
        Assert.Contains("Issues.SearchTextBox", issuesText);
        Assert.Contains("Issues.RefreshCurrentFileButton", issuesText);
        Assert.Contains("Issues.RunFullDiagnosticsButton", issuesText);
        Assert.Contains("Issues.ClearButton", issuesText);
        Assert.Contains("Style=\"{StaticResource IdeWorkspaceRootStyle}\"", issuesText);
        Assert.Contains("Style=\"{StaticResource IdeIssueDataGridStyle}\"", issuesText);
        Assert.Contains("Style=\"{StaticResource IdeWorkspaceCommandButtonStyle}\"", issuesText);
        Assert.Contains("CellTemplate=\"{StaticResource IdeIssueSeverityIconTemplate}\"", issuesText);
        Assert.DoesNotContain("<Grid Margin=\"14\">", issuesText);

        Assert.Contains("Title=\"查找\"", shellText);
        Assert.DoesNotContain("Text=\"查找\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.QueryTextBox\"", searchViewText);
        Assert.Contains("Content=\"区分大小写\"", searchViewText);
        Assert.Contains("Content=\"全字匹配\"", searchViewText);
        Assert.Contains("Content=\"正则表达式\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.ScopeComboBox\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.FilePatternComboBox\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.ResultsList\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.StatusText\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.ReplaceTextBox\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.PreviewReplaceAllButton\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.ApplyReplaceAllButton\"", searchViewText);
        Assert.Contains("Style=\"{StaticResource IdeWorkspaceCommandBarStyle}\"", searchViewText);
        Assert.Contains("Style=\"{StaticResource IdeWorkspaceCommandButtonStyle}\"", searchViewText);
        Assert.Contains("Style=\"{StaticResource UiAccentButtonStyle}\"", searchViewText);
        Assert.Contains("<ColumnDefinition Width=\"72\" />", searchViewText);
        Assert.Contains("SearchToolWindowViewModel", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.View\"", searchViewText);
        Assert.DoesNotContain("Search.UnavailableHint", searchViewText, StringComparison.Ordinal);
        Assert.DoesNotContain("<Grid Margin=\"14\">", searchViewText);
    }

    [Fact]
    public void IdeSourceEditorPlaceholder_IsReadonlyAvalonEditTextEditor()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string viewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ShellViewModel.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string viewModelText = File.ReadAllText(viewModelPath);

        Assert.Contains("xmlns:avalonedit=\"http://icsharpcode.net/sharpdevelop/avalonedit\"", shellText);
        Assert.Contains("<avalonedit:TextEditor", shellText);
        Assert.Contains("x:Name=\"SourceTextEditor\"", shellText);
        Assert.DoesNotContain("Text=\"{Binding SourceEditor.Text", shellText);
        Assert.Contains("IsReadOnly=\"True\"", shellText);
        Assert.Contains("ShowLineNumbers=\"True\"", shellText);
        Assert.Contains("WordWrap=\"False\"", shellText);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", shellText);
        Assert.Contains("HorizontalScrollBarVisibility=\"Auto\"", shellText);
        Assert.DoesNotContain("x:Name=\"SourceTextBox\"", shellText);
        Assert.DoesNotContain("SourceEditorTextBoxStyle", shellText);
        Assert.DoesNotContain("<TextBox x:Name=\"SourceTextBox\"", shellText);
        Assert.Contains("TextChanged=\"SourceTextEditor_OnTextChanged\"", shellText);
        Assert.Contains("Shell.SourceEditor.EnterEditModeButton", shellText);
        Assert.Contains("Visibility=\"Collapsed\"", shellText);
        Assert.Contains("Shell.SourceEditor.RevertInMemoryChangesButton", shellText);
        Assert.Contains("Shell.SourceEditor.EditorStateText", shellText);
        Assert.DoesNotContain("SourceEditorPlaceholderText", viewModelText);

        string shellCodeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        Assert.Contains("AttachSourceEditorTextBinding(DataContext as ShellViewModel)", shellCodeText);
        Assert.Contains("_boundSourceEditor.PropertyChanged += SourceEditor_OnPropertyChanged", shellCodeText);
        Assert.Contains("nameof(SourceEditorViewModel.Text)", shellCodeText);
        Assert.Contains("SourceTextEditor.Document.Text = text", shellCodeText);
        Assert.Contains("FieldRegistryRuntimeService", shellCodeText);
        Assert.Contains("ReloadReadonlySourceHighlighting", shellCodeText);
        Assert.Contains("ReplaceReadonlySourceHighlightingTransformer", shellCodeText);
        Assert.Contains("LineTransformers.Add", shellCodeText);
    }

    [Fact]
    public void IdeReadonlyHighlighter_UsesLocalCompositeProviderWithoutGitHubCompletionSaveOrLegacyFieldDatabase()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string tokenizerPath = Path.Combine(root, "RA2IniEditor.IDE", "Highlighting", "ReadonlyIniHighlightTokenizer.cs");
        string transformerPath = Path.Combine(root, "RA2IniEditor.IDE", "Highlighting", "Ra2KnownFieldHighlightingTransformer.cs");

        string shellCodeText = File.ReadAllText(shellWindowCodePath);
        string tokenizerText = File.ReadAllText(tokenizerPath);
        string transformerText = File.ReadAllText(transformerPath);
        string runtimeServiceText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Services", "FieldRegistryRuntimeService.cs"));
        string highlighterText = tokenizerText + transformerText;
        string combinedText = shellCodeText + highlighterText + runtimeServiceText;

        Assert.Contains("FieldRegistryRuntimeService", shellCodeText);
        Assert.Contains("_fieldRegistryRuntimeService.Reload", shellCodeText);
        Assert.Contains("LocalFieldRegistryLoader", runtimeServiceText);
        Assert.Contains("LocalRa2FieldDefinitionProvider", runtimeServiceText);
        Assert.Contains("CompositeRa2FieldDefinitionProvider", runtimeServiceText);
        Assert.Contains("BuiltInRa2FieldDefinitionProvider", runtimeServiceText);
        Assert.Contains("Environment.SpecialFolder.ApplicationData", runtimeServiceText);
        Assert.Contains("\".ra2inieditor\", \"field-registry\", \"active\"", runtimeServiceText);
        Assert.DoesNotContain("LocalFieldRegistryLoader", highlighterText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.", highlighterText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.", highlighterText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HttpClient", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GitHub", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2FieldDefinitionDatabase", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2FieldOptionProvider", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2SchemaProvider", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeSectionClassification_IsCurrentDocumentOnlyAndSharedByHighlighterAndProjectExplorer()
    {
        string root = TestRepositoryRoot.Find();
        string classifierDirectory = Path.Combine(root, "RA2IniEditor.Application", "Classification");
        string classifierText = string.Join(Environment.NewLine, Directory.GetFiles(classifierDirectory, "*.cs").Select(File.ReadAllText));
        string tokenizerText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Highlighting", "ReadonlyIniHighlightTokenizer.cs"));
        string explorerText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Services", "ReadonlyProjectExplorerGroupingService.cs"));
        string combinedText = classifierText + tokenizerText + explorerText;

        Assert.Contains("IRa2SectionClassifier", classifierText, StringComparison.Ordinal);
        Assert.Contains("Ra2SectionClassifier", tokenizerText, StringComparison.Ordinal);
        Assert.Contains("Ra2SectionClassifier", explorerText, StringComparison.Ordinal);
        Assert.Contains("Primary", classifierText, StringComparison.Ordinal);
        Assert.Contains("Projectile", classifierText, StringComparison.Ordinal);
        Assert.Contains("Warhead", classifierText, StringComparison.Ordinal);
        Assert.DoesNotContain("File.", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HttpClient", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeFieldRegistryManager_IsLocalStatusToolWindowWithoutNetworkCompletionSaveOrLegacyFieldDatabase()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string centerWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryCenterWindow.xaml");
        string centerCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryCenterWindow.xaml.cs");
        string managerWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml");
        string managerCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml.cs");
        string managerViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "FieldRegistryManagerViewModel.cs");
        string packViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "FieldRegistryPackStatusViewModel.cs");
        string runtimeServicePath = Path.Combine(root, "RA2IniEditor.IDE", "Services", "FieldRegistryRuntimeService.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellWindowCodePath);
        string centerText = File.ReadAllText(centerWindowPath);
        string centerCodeText = File.ReadAllText(centerCodePath);
        string managerText = File.ReadAllText(managerWindowPath);
        string managerCodeText = File.ReadAllText(managerCodePath);
        string managerViewModelText = File.ReadAllText(managerViewModelPath);
        string packViewModelText = File.ReadAllText(packViewModelPath);
        string runtimeServiceText = File.ReadAllText(runtimeServicePath);
        string combinedText = shellText + shellCodeText + centerText + centerCodeText + managerText + managerCodeText + managerViewModelText + packViewModelText + runtimeServiceText;

        Assert.Contains("Click=\"OpenFieldRegistryManagerWindow\"", shellText);
        Assert.Contains("FieldRegistryCenter", shellText);
        Assert.Contains("FieldRegistryCenterWindow", shellCodeText);
        Assert.Contains("FieldRegistryCenter.Window", centerText);
        Assert.Contains("FieldRegistryCenter.SearchTextBox", centerText);
        Assert.Contains("FieldRegistryCenter.FieldsGrid", centerText);
        Assert.Contains("FieldRegistryCenter.AdvancedToolsButton", centerText);
        Assert.Contains("ItemsSource=\"{Binding Manager.Packs}\"", centerText);
        Assert.Contains("ItemsSource=\"{Binding FieldRows}\"", centerText);
        Assert.Contains("AdvancedToolsRequested", centerCodeText);
        Assert.Contains("OpenAdvancedFieldRegistryToolsWindow", shellCodeText);
        Assert.Contains("private FieldRegistryManagerWindow? _fieldRegistryManagerWindow", shellCodeText);
        Assert.Contains("_fieldRegistryManagerWindow.Activate();", shellCodeText);
        Assert.Contains("_fieldRegistryManagerWindow.Show();", shellCodeText);
        Assert.DoesNotContain("_fieldRegistryManagerWindow.ShowDialog", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ReloadLocalFieldRegistryRequested", managerCodeText);
        Assert.Contains("OpenGlobalRegistryFolderRequested", managerCodeText);
        Assert.Contains("OpenProjectRegistryFolderRequested", managerCodeText);
        Assert.Contains("重新加载本地字段库", managerText);
        Assert.Contains("打开全局目录", managerText);
        Assert.Contains("打开项目目录", managerText);
        Assert.Contains("ItemsSource=\"{Binding Packs}\"", managerText);
        Assert.Contains("ItemsSource=\"{Binding Warnings}\"", managerText);
        Assert.Contains("SourceTextEditor.TextArea.TextView.Redraw();", shellCodeText);
        Assert.Contains("LineTransformers.RemoveAt(index)", shellCodeText);
        Assert.Contains("LineTransformers.Add", shellCodeText);
        Assert.Contains("Directory.CreateDirectory", shellCodeText);
        Assert.DoesNotContain("HttpClient", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Octokit", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "GitHub",
            shellCodeText + centerText + centerCodeText + managerText + managerCodeText + managerViewModelText + packViewModelText + runtimeServiceText,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2FieldDefinitionDatabase", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2FieldOptionProvider", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2SchemaProvider", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeProjectExplorer_UsesDescriptorNodesWithoutInlineSourceTextOrLegacyDependencies()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ShellViewModel.cs");
        string projectExplorerViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ProjectExplorerViewModel.cs");
        string projectExplorerItemViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ProjectExplorerItemViewModel.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellViewModelText = File.ReadAllText(shellViewModelPath);
        string projectExplorerViewModelText = File.ReadAllText(projectExplorerViewModelPath);
        string projectExplorerItemViewModelText = File.ReadAllText(projectExplorerItemViewModelPath);

        Assert.Contains("TreeView", shellText);
        Assert.Contains("ItemsSource=\"{Binding ProjectExplorer.Items}\"", shellText);
        Assert.Contains("HierarchicalDataTemplate", shellText);
        Assert.Contains("VirtualizingStackPanel.IsVirtualizing=\"True\"", shellText);
        Assert.Contains("VirtualizingStackPanel.VirtualizationMode=\"Recycling\"", shellText);
        Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", shellText);
        Assert.Contains("IsExpanded", shellText);
        Assert.Contains("ReadonlyIniFileDescriptor", projectExplorerViewModelText);
        Assert.Contains("FilePath", projectExplorerItemViewModelText);
        Assert.Contains("FileSizeBytes", projectExplorerItemViewModelText);
        Assert.Contains("CanNavigateToSource", projectExplorerItemViewModelText);
        Assert.Contains("IconText", projectExplorerItemViewModelText);
        Assert.Contains("IconGlyph", projectExplorerItemViewModelText);
        Assert.Contains("DisplayTextWithCount", projectExplorerItemViewModelText);
        Assert.Contains("ToolTipText", projectExplorerItemViewModelText);
        Assert.Contains("IsCurrentFile", projectExplorerItemViewModelText);
        Assert.Contains("IsCurrentSection", projectExplorerItemViewModelText);
        Assert.Contains("LoadProjectExplorerFileAsync", shellViewModelText);
        Assert.DoesNotContain("public string Text", projectExplorerItemViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ShowSelectedFile", shellViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SourceEditorViewModel", projectExplorerViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IIniFileStore", projectExplorerViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", projectExplorerViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", projectExplorerViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.GetFiles", projectExplorerViewModelText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeSearchToolContent_IsReusablePreviewWithoutMockResultsOrRealSearchDependencies()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string searchViewPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "SearchToolView.xaml");
        string searchViewCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "SearchToolView.xaml.cs");
        string searchViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "SearchToolWindowViewModel.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellWindowCodePath);
        string searchViewText = File.ReadAllText(searchViewPath);
        string searchCodeText = File.ReadAllText(searchViewCodePath);
        string searchViewModelText = File.ReadAllText(searchViewModelPath);

        Assert.Contains("Click=\"OpenSearchToolWindow\"", shellText);
        Assert.Contains("ContentId=\"Tool.Search\"", shellText);
        Assert.Contains("<views:SearchToolView />", shellText);
        Assert.Contains("ShowAndActivateSearchTool", shellCodeText);
        Assert.DoesNotContain("ShowAndActivateBottomTool(\"Tool.Search\"", shellCodeText, StringComparison.Ordinal);
        Assert.DoesNotContain("private SearchToolWindow? _searchToolWindow", shellCodeText);
        Assert.DoesNotContain("_searchToolWindow.ShowDialog", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SearchToolWindowViewModel", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.View\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.ScopeComboBox\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.FilePatternComboBox\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.FindPreviousButton\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.FindNextButton\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.FindAllButton\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.ResultsList\"", searchViewText);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.StatusText\"", searchViewText);
        Assert.Contains("SearchRequested", searchCodeText, StringComparison.Ordinal);
        Assert.Contains("ResultNavigateRequested", searchCodeText, StringComparison.Ordinal);
        Assert.Contains("ReplacePreviewRequested", searchCodeText, StringComparison.Ordinal);
        Assert.Contains("ReplaceApplyRequested", searchCodeText, StringComparison.Ordinal);
        Assert.Contains("Mode=TwoWay", searchViewText, StringComparison.Ordinal);
        Assert.DoesNotContain("mock", searchViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SourceEditorViewModel", searchViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IIniFileStore", searchViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", searchViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", searchViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.GetFiles", searchViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AvalonEdit", searchViewText + searchCodeText + searchViewModelText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeProjectOpenShell_UsesInfrastructureReadonlyFlowWithoutSaveOrLegacyLoader()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string shellViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ShellViewModel.cs");
        string projectOpenServicePath = Path.Combine(root, "RA2IniEditor.IDE", "Services", "ProjectOpenService.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellWindowCodePath);
        string shellViewModelText = File.ReadAllText(shellViewModelPath);
        string projectOpenServiceText = File.ReadAllText(projectOpenServicePath);
        string combinedText = shellText + shellCodeText + shellViewModelText + projectOpenServiceText;

        Assert.Contains("打开项目...", shellText);
        Assert.Contains("OpenFolderDialog", shellCodeText);
        Assert.Contains("OpenProjectFolderAsync", shellViewModelText);
        Assert.Contains("ReadonlyIniContentService", shellViewModelText);
        Assert.Contains("SearchOption.TopDirectoryOnly", projectOpenServiceText);
        Assert.DoesNotContain("IIniFileStore", projectOpenServiceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ReadText", projectOpenServiceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", projectOpenServiceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CreateBackup", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeProjectExplorer_UsesReadonlyCurrentFileGroupingWithoutParserDiagnosticsOrSave()
    {
        string root = TestRepositoryRoot.Find();
        string shellViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ShellViewModel.cs");
        string projectExplorerViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ProjectExplorerViewModel.cs");
        string groupingServicePath = Path.Combine(root, "RA2IniEditor.IDE", "Services", "ReadonlyProjectExplorerGroupingService.cs");

        string shellViewModelText = File.ReadAllText(shellViewModelPath);
        string projectExplorerViewModelText = File.ReadAllText(projectExplorerViewModelPath);
        string groupingServiceText = File.ReadAllText(groupingServicePath);
        string combinedText = shellViewModelText + projectExplorerViewModelText + groupingServiceText;

        Assert.Contains("_projectExplorerGroupingService.BuildGroups(result.Text)", shellViewModelText);
        Assert.Contains("ProjectExplorer.ShowGroupedSectionsForCurrentFile", shellViewModelText);
        Assert.Contains("ProjectExplorer.ShowPlaceholderForCurrentFile", shellViewModelText);
        Assert.Contains("ShowPlaceholderForCurrentFile", shellViewModelText);
        Assert.DoesNotContain("ProjectOpenService", projectExplorerViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ReadonlyIniContentService", projectExplorerViewModelText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniParser", groupingServiceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniDocument", groupingServiceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ReadText", groupingServiceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DiagnosticRuleRegistry", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AvalonEdit", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Docking", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeIssuesPanel_UsesStructuredIssuesAndCurrentFileJumpWithoutSearchOrSave()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string shellViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ShellViewModel.cs");
        string issuesToolWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "IssuesToolWindow.xaml");
        string issuesToolWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "IssuesToolWindow.xaml.cs");
        string workspaceStylePath = Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IdeWorkspaceStyles.xaml");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellWindowCodePath);
        string shellViewModelText = File.ReadAllText(shellViewModelPath);
        string issuesToolText = File.ReadAllText(issuesToolWindowPath);
        string issuesToolCodeText = File.ReadAllText(issuesToolWindowCodePath);
        string workspaceStyleText = File.ReadAllText(workspaceStylePath);
        string combinedText = shellText + shellCodeText + shellViewModelText + issuesToolText + issuesToolCodeText + workspaceStyleText;

        Assert.Contains("Click=\"FocusIssuesToolTab\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.BottomToolTabs.ErrorList\"", shellText);
        Assert.Contains("ItemsSource=\"{Binding Issues.Items}\"", shellText);
        Assert.Contains("Header=\"输出\"", shellText);
        Assert.Contains("Text=\"{Binding OutputText, Mode=OneWay}\"", shellText);
        Assert.Contains("DataGrid", shellText);
        Assert.Contains("MouseDoubleClick=\"BottomIssuesGrid_OnMouseDoubleClick\"", shellText);
        Assert.Contains("RefreshCurrentFileDiagnosticsFromShell", shellText);
        Assert.Contains("RunManualFullDiagnosticsFromShell", shellText);
        Assert.Contains("ClearIssuesFromShell", shellText);
        Assert.Contains("DataGrid", issuesToolText);
        Assert.Contains("ItemsSource=\"{Binding Issues.Items}\"", issuesToolText);
        Assert.Contains("Issues.SeverityFilterComboBox", issuesToolText);
        Assert.Contains("Issues.SourceFilterTextBox", issuesToolText);
        Assert.Contains("Issues.SearchTextBox", issuesToolText);
        Assert.Contains("Issues.RefreshCurrentFileButton", issuesToolText);
        Assert.Contains("Issues.RunFullDiagnosticsButton", issuesToolText);
        Assert.Contains("Issues.ClearButton", issuesToolText);
        Assert.Contains("SelectedItem=\"{Binding Issues.SelectedIssue, Mode=TwoWay}\"", issuesToolText);
        Assert.Contains("SelectionMode=\"Single\"", issuesToolText);
        Assert.Contains("SelectionUnit=\"FullRow\"", issuesToolText);
        Assert.Contains("IsReadOnly=\"True\"", issuesToolText);
        Assert.Contains("AutoGenerateColumns=\"False\"", issuesToolText);
        Assert.Contains("MouseDoubleClick=\"IssuesGrid_OnMouseDoubleClick\"", issuesToolText);
        Assert.Contains("Text=\"{Binding Issues.StatusText}\"", issuesToolText);
        Assert.Contains("Text=\"{Binding Issues.CountText}\"", issuesToolText);
        Assert.DoesNotContain("Text=\"{Binding OutputText", issuesToolText);
        Assert.Contains("CellTemplate=\"{StaticResource IdeIssueSeverityIconTemplate}\"", issuesToolText);
        Assert.Contains("AutomationProperties.Name=\"{Binding SeverityText}\"", workspaceStyleText);
        Assert.Contains("IconGeometry.Issue.Error", workspaceStyleText);
        Assert.Contains("IconGeometry.Issue.Warning", workspaceStyleText);
        Assert.Contains("IconGeometry.Issue.Info", workspaceStyleText);
        Assert.DoesNotContain("Text=\"{Binding SeverityMarker}\"", issuesToolText);
        Assert.Contains("Binding=\"{Binding LocationText}\"", issuesToolText);
        Assert.Contains("Text=\"{Binding Message}\"", issuesToolText);
        Assert.Contains("ToolTip=\"{Binding Message}\"", issuesToolText);
        Assert.Contains("Binding=\"{Binding SourceText}\"", issuesToolText);
        Assert.Contains("IssueNavigateRequested", issuesToolCodeText);
        Assert.Contains("RunManualFullDiagnosticsRequested", issuesToolCodeText);
        Assert.Contains("RefreshCurrentFileDiagnosticsRequested", issuesToolCodeText);
        Assert.Contains("ClearIssuesRequested", issuesToolCodeText);
        Assert.Contains("_issuesToolWindow.Activate();", shellCodeText);
        Assert.Contains("_issuesToolWindow.Show();", shellCodeText);
        Assert.Contains("RunManualFullDiagnosticsAsync(", shellCodeText);
        Assert.Contains("RefreshCurrentFileDiagnostics(", shellCodeText);
        Assert.Contains("_fieldRegistryRuntimeService.CurrentProvider", shellCodeText);
        Assert.DoesNotContain("_issuesToolWindow.ShowDialog", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("await TryNavigateToIssueAsync(viewModel, issue)", shellCodeText);
        Assert.Contains("TryNavigateToCurrentFileIssue", shellCodeText);
        Assert.Contains("TryNavigateToIssueLocation", shellCodeText);
        Assert.Contains("FindProjectExplorerFileItem(viewModel, issue.FilePath)", shellCodeText);
        Assert.Contains("TryResolveDirtyNavigationBeforeLeavingCurrentFile(viewModel)", shellCodeText);
        Assert.Contains("await viewModel.LoadProjectExplorerFileAsync(", shellCodeText);
        Assert.Contains("StartEditableSessionForCurrentSnapshot(viewModel)", shellCodeText);
        Assert.Contains("SelectProjectExplorerItem(fileItem)", shellCodeText);
        Assert.Contains("TryScrollSourceEditorToLine(viewModel, issue.LineNumber.Value, issue.ColumnNumber", shellCodeText);
        Assert.Contains("issue.Version != currentSnapshot.Version", shellCodeText);
        Assert.Contains("IssuesStatusMessages", shellViewModelText);
        Assert.Contains("TryReplaceIssuesForCurrentSnapshot", shellViewModelText);
        Assert.Contains("IsCurrentSnapshot", shellViewModelText);
        Assert.Contains("SkippedStaleResult", shellViewModelText);
        Assert.Contains("Cannot jump because this issue has no file path.", shellCodeText);
        Assert.Contains("Cannot jump because this issue has no line number.", shellCodeText);
        Assert.Contains("Cannot jump because the issue file is not in Project Explorer.", shellCodeText);
        Assert.Contains("Cannot jump because the issue file failed to load.", shellCodeText);
        Assert.Contains("Cannot jump because the target issue file is not loaded as source text.", shellCodeText);
        Assert.Contains("Cannot jump because the issue result is stale.", shellCodeText);
        Assert.Contains("Math.Min(oneBasedLineNumber, SourceTextEditor.Document.LineCount)", shellCodeText);
        Assert.Contains("Document.GetLineByNumber(currentTargetLineNumber)", shellCodeText);
        Assert.Contains("TextArea.Caret.Offset = targetOffset", shellCodeText);
        Assert.Contains("SourceTextEditor.ScrollTo(currentTargetLineNumber, Math.Max(1, columnOffset + 1))", shellCodeText);
        Assert.Contains("Jumped to issue at Line", shellCodeText);
        Assert.Contains("ShowOutputMessage", shellViewModelText);
        Assert.Contains("CurrentFileReadonlyDiagnosticService", shellViewModelText);
        Assert.Contains("CurrentSourceSnapshot", shellViewModelText);
        Assert.Contains("DIAGNOSTIC_EXCEPTION", File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Diagnostics", "CurrentFileReadonlyDiagnosticService.cs")));
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DiagnosticRuleRegistry", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeProjectExplorerSectionJump_UsesReadonlyAvalonEditOffsetNavigationWithoutReloadSaveOrEditorAbstraction()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string projectExplorerViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "ProjectExplorerViewModel.cs");
        string navigationResolverPath = Path.Combine(root, "RA2IniEditor.IDE", "Services", "ReadonlySourceSectionNavigationResolver.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellWindowCodePath);
        string projectExplorerViewModelText = File.ReadAllText(projectExplorerViewModelPath);
        string navigationResolverText = File.ReadAllText(navigationResolverPath);
        string combinedText = shellText + shellCodeText + projectExplorerViewModelText + navigationResolverText;

        Assert.Contains("x:Name=\"ProjectExplorerTreeView\"", shellText);
        Assert.Contains("SelectedItemChanged=\"ProjectExplorerTreeView_OnSelectedItemChanged\"", shellText);
        Assert.Contains("x:Name=\"SourceTextEditor\"", shellText);
        Assert.DoesNotContain("Text=\"{Binding SourceEditor.Text", shellText);
        Assert.Contains("IsReadOnly=\"True\"", shellText);
        Assert.Contains("ShowLineNumbers=\"True\"", shellText);
        Assert.Contains("AttachSourceEditorTextBinding", shellCodeText);
        Assert.Contains("SourceTextEditor.Document.Text = text", shellCodeText);
        Assert.Contains("TryNavigateToSection", shellCodeText);
        Assert.Contains("selectedItem.Kind == ProjectExplorerItemKind.Section", shellCodeText);
        Assert.Contains("Explorer navigation skipped: selected node is not a section.", shellCodeText);
        Assert.Contains("Explorer navigation skipped: section id is missing.", shellCodeText);
        Assert.Contains("_sectionNavigationResolver.Resolve", shellCodeText);
        Assert.Contains("SourceTextEditor.Text", shellCodeText);
        Assert.Contains("section.LineNumber", shellCodeText);
        Assert.Contains("header was not found in the current text", shellCodeText);
        Assert.Contains("TryScrollSourceEditorToCharacterIndex", shellCodeText);
        Assert.Contains("SourceTextEditor.Document.GetLocation(target.CharacterIndex)", shellCodeText);
        Assert.Contains("SourceTextEditor.TextArea.Caret.Offset = target.CharacterIndex", shellCodeText);
        Assert.Contains("SourceTextEditor.ScrollTo(location.Line, location.Column)", shellCodeText);
        Assert.Contains("ProjectExplorer.MarkCurrentSection", shellCodeText);
        Assert.Contains("CurrentSnapshot.FilePath", shellCodeText);
        Assert.Contains("Jumped to section [{target.SectionId}] at Line {target.OneBasedLineNumber}.", shellCodeText);
        Assert.Contains("Dispatcher.BeginInvoke", shellCodeText);
        Assert.Contains("TryReadSectionId", navigationResolverText);
        Assert.Contains("preferredOneBasedLineNumber", navigationResolverText);
        Assert.Contains("SelectedItem = null;", projectExplorerViewModelText);
        Assert.DoesNotContain("TryScrollSourceEditorToLine(viewModel, section.LineNumber.Value", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IsExpectedSectionHeaderLine", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GetRectFromCharacterIndex", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FindDescendant<ScrollViewer>", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("VisualTreeHelper", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ScrollToVerticalOffset", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ScrollToLine(0", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("LineNumber ?? 1", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Math.Max(0, oneBasedLineNumber", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ScrollToLine(anchorLineIndex)", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SourceTextBox", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectOpenService", shellCodeText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Ra2ProjectSearchService", shellCodeText, StringComparison.Ordinal);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ITextEditor", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ISourceEditorAdapter", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeSourceEditorFieldQuickPeek_IsMenuTriggeredAndDoesNotEditSaveOrDirty()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string quickPeekServicePath = Path.Combine(root, "RA2IniEditor.IDE", "Language", "FieldQuickPeek", "Ra2FieldQuickPeekService.cs");
        string quickPeekWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldQuickPeek", "Ra2FieldQuickPeekWindow.xaml");
        string detailsViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "FieldDetails", "Ra2FieldDetailsViewModel.cs");
        string addPropertyViewModelPath = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "FieldBrowser", "Ra2AddPropertyItemViewModel.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellWindowCodePath);
        string quickPeekText = File.ReadAllText(quickPeekServicePath);
        string quickPeekWindowText = File.ReadAllText(quickPeekWindowPath);
        string detailsViewModelText = File.ReadAllText(detailsViewModelPath);
        string addPropertyViewModelText = File.ReadAllText(addPropertyViewModelPath);
        string combinedText = shellText + shellCodeText + quickPeekText + quickPeekWindowText + detailsViewModelText + addPropertyViewModelText;

        Assert.Contains("Shell.SourceEditor.PeekFieldDetailsMenuItem", shellText);
        Assert.Contains("Header=\"速览字段详情\"", shellText);
        Assert.Contains("Click=\"PeekFieldDetails_OnClick\"", shellText);
        Assert.Contains("Opened=\"SourceEditorContextMenu_OnOpened\"", shellText);
        Assert.Contains("PreviewMouseRightButtonDown=\"SourceTextEditor_OnPreviewMouseRightButtonDown\"", shellText);
        Assert.Contains("Ra2FieldQuickPeekService", shellCodeText);
        Assert.Contains("Ra2FieldQuickPeekWindow", shellCodeText);
        Assert.Contains("CanResolveKeyValueLine", quickPeekText);
        Assert.Contains("Ra2FieldDetailsViewModel", quickPeekText);
        Assert.Contains("Ra2FieldDetailsViewModel", addPropertyViewModelText);
        Assert.Contains("ItemsSource=\"{Binding Examples}\"", quickPeekWindowText);
        Assert.Contains("AllowedValues", quickPeekWindowText);
        Assert.Contains("FromProvenance", detailsViewModelText);
        Assert.Contains("FromDefinition", detailsViewModelText);
        Assert.DoesNotContain("ShowFieldQuickPeekWindow", shellCodeText.Substring(
            shellCodeText.IndexOf("SourceTextEditor_OnPreviewMouseRightButtonDown", StringComparison.Ordinal),
            shellCodeText.IndexOf("SourceEditorContextMenu_OnOpened", StringComparison.Ordinal) -
            shellCodeText.IndexOf("SourceTextEditor_OnPreviewMouseRightButtonDown", StringComparison.Ordinal)),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SetDirty", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractShellOutputTextBoxBlock(string shellText)
    {
        int start = shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.OutputTextBox\"", StringComparison.Ordinal);
        Assert.True(start >= 0, "Shell output TextBox should keep its automation id.");

        int end = shellText.IndexOf("</avalondock:LayoutAnchorable>", start, StringComparison.Ordinal);
        Assert.True(end > start, "Shell output TextBox should remain inside the output dockable.");

        return shellText[start..end];
    }

    private static string ExtractShellMainToolbarBlock(string shellText)
    {
        int start = shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.MainToolbar\"", StringComparison.Ordinal);
        Assert.True(start >= 0, "Shell main toolbar should keep its automation id.");

        int end = shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.SourceEditor.DocumentTabStrip\"", start, StringComparison.Ordinal);
        Assert.True(end > start, "Shell main toolbar should stay between the menu and document tabs.");

        return shellText[start..end];
    }

    private static string ExtractAiAssistantPanelBlock(string shellText)
    {
        int start = shellText.IndexOf("AutomationProperties.AutomationId=\"AiAssistant.Panel\"", StringComparison.Ordinal);
        Assert.True(start >= 0, "AI assistant panel should expose its automation id.");

        int end = shellText.IndexOf("AutomationProperties.AutomationId=\"AiAssistant.SafetyFooter\"", start, StringComparison.Ordinal);
        Assert.True(end > start, "AI assistant panel should contain its safety footer.");

        return shellText[start..end];
    }

    private static string ExtractAiAssistantPromptBoxBlock(string shellText)
    {
        int start = shellText.IndexOf("AutomationProperties.AutomationId=\"AiAssistant.PromptBox\"", StringComparison.Ordinal);
        Assert.True(start >= 0, "AI assistant prompt box should expose its automation id.");

        int end = shellText.IndexOf("/>", start, StringComparison.Ordinal);
        Assert.True(end > start, "AI assistant prompt box should remain a self-closing TextBox in this skeleton.");

        return shellText[start..end];
    }

    private static string ExtractAiAssistantModelSelectorBlock(string shellText)
    {
        int start = shellText.IndexOf("AutomationProperties.AutomationId=\"AiAssistant.ModelSelector\"", StringComparison.Ordinal);
        Assert.True(start >= 0, "AI assistant model selector should keep its automation id.");

        int end = shellText.IndexOf("</ComboBox>", start, StringComparison.Ordinal);
        Assert.True(end > start, "AI assistant model selector should be a compact ComboBox.");

        return shellText[start..end];
    }

    private static string ExtractAiAssistantAdvancedOptionsBlock(string shellText)
    {
        int start = shellText.IndexOf("AutomationProperties.AutomationId=\"AiAssistant.AdvancedOptions\"", StringComparison.Ordinal);
        Assert.True(start >= 0, "AI assistant advanced options should expose its automation id.");

        int end = shellText.IndexOf("</Border>", start, StringComparison.Ordinal);
        Assert.True(end > start, "AI assistant advanced options should remain a compact status area.");

        return shellText[start..end];
    }

    private static string ExtractAiAssistantCopyMethodsBlock(string shellCodeText)
    {
        int start = shellCodeText.IndexOf("private static void CopyAiAssistantMessage", StringComparison.Ordinal);
        Assert.True(start >= 0, "AI assistant should keep a full-message copy method.");

        int end = shellCodeText.IndexOf("private Ra2AiAssistantPipeline CreateAiAssistantPipeline", start, StringComparison.Ordinal);
        Assert.True(end > start, "AI assistant copy methods should stay before provider pipeline creation.");

        return shellCodeText[start..end];
    }

    private static string ExtractAiAssistantRestorePromptMethodBlock(string shellCodeText)
    {
        int start = shellCodeText.IndexOf("private void AddAiAssistantRestorePromptAction", StringComparison.Ordinal);
        Assert.True(start >= 0, "AI assistant restore-prompt method was not found.");

        int end = shellCodeText.IndexOf("private static string GetAiAssistantMessageText", start, StringComparison.Ordinal);
        Assert.True(end > start, "AI assistant restore-prompt method boundary was not found.");

        return shellCodeText[start..end];
    }

    private static string ExtractDeepSeekAiAssistantResponseFormatterBlock(string shellCodeText)
    {
        int start = shellCodeText.IndexOf("private static string FormatDeepSeekAiAssistantResponse", StringComparison.Ordinal);
        Assert.True(start >= 0, "DeepSeek response formatter was not found.");

        int end = shellCodeText.IndexOf("private void SetAiAssistantSendingState", start, StringComparison.Ordinal);
        Assert.True(end > start, "DeepSeek response formatter boundary was not found.");

        return shellCodeText[start..end];
    }

    private static string ExtractAiAssistantCancelMethodBlock(string shellCodeText)
    {
        int start = shellCodeText.IndexOf("private void CancelAiAssistantResponse", StringComparison.Ordinal);
        Assert.True(start >= 0, "AI assistant cancel method was not found.");

        int end = shellCodeText.IndexOf("private void AiAssistantPromptBox_OnPreviewKeyDown", start, StringComparison.Ordinal);
        Assert.True(end > start, "AI assistant cancel method boundary was not found.");

        return shellCodeText[start..end];
    }

    private static string ExtractShellDocumentTabStripBlock(string shellText)
    {
        int start = shellText.IndexOf("AutomationProperties.AutomationId=\"Shell.SourceEditor.DocumentTabStrip\"", StringComparison.Ordinal);
        Assert.True(start >= 0, "Shell document tab strip should keep its automation id.");

        int end = shellText.IndexOf("x:Name=\"SourceTextEditor\"", start, StringComparison.Ordinal);
        Assert.True(end > start, "Shell document tab strip should stay inside the editor column before the source editor.");

        return shellText[start..end];
    }
}
