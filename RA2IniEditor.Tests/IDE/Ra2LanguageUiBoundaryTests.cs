using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2LanguageUiBoundaryTests
{
    [Fact]
    public void ShellSourceEditor_DefinesLanguageContextMenuForReadonlyPreview()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellCodePath);

        Assert.Contains("Shell.SourceEditor.ContextMenu", shellText);
        Assert.Contains("转到定义", shellText);
        Assert.Contains("查看定义", shellText);
        Assert.Contains("查找当前文件引用", shellText);
        Assert.Contains("GoToDefinition_OnClick", shellText);
        Assert.Contains("PeekDefinition_OnClick", shellText);
        Assert.Contains("FindAllReferences_OnClick", shellText);
        Assert.Contains("TryBuildLanguageContext", shellCodeText);
        Assert.Contains("Ra2DocumentSemanticModelBuilder", shellCodeText);
        Assert.Contains("Ra2CaretContextService", shellCodeText);
        Assert.Contains("Ra2DefinitionProvider", shellCodeText);
        Assert.Contains("Ra2ReferenceFinder", shellCodeText);
        Assert.Contains("SourceTextEditor.Document.Text", shellCodeText);
        Assert.Contains("SourceTextEditor.TextArea.Caret.Offset", shellCodeText);
    }

    [Fact]
    public void LanguagePreviewSurfaces_DefineAutomationIdsWithoutModalOrLegacyServices()
    {
        string root = TestRepositoryRoot.Find();
        string viewsRoot = Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language");
        string viewModelsRoot = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "Language");
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string combinedText = string.Join(Environment.NewLine,
            Directory.GetFiles(viewsRoot, "*.*").Concat(Directory.GetFiles(viewModelsRoot, "*.cs")).Select(File.ReadAllText));

        Assert.Contains("Ra2PeekDefinition.Window", combinedText);
        Assert.Contains("Ra2PeekDefinition.TitleText", combinedText);
        Assert.Contains("Ra2PeekDefinition.DetailText", combinedText);
        Assert.Contains("Ra2PeekDefinition.SourceText", combinedText);
        Assert.Contains("Key.Escape", combinedText);
        Assert.Contains("Ra2FindReferences.View", combinedText);
        Assert.Contains("Shell.Dock.Tool.FindReferences", shellText);
        Assert.Contains("ContentId=\"Tool.FindReferences\"", shellText);
        Assert.Contains("Ra2FindReferences.TargetText", combinedText);
        Assert.Contains("Ra2FindReferences.ReferencesGrid", combinedText);
        Assert.Contains("Ra2FindReferences.StatusText", combinedText);
        Assert.DoesNotContain("ShowDialog", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HttpClient", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GitHub", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Docking", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReferenceDockableAndPeekWindow_UseChineseLabelsAndScrollablePeekLayout()
    {
        string root = TestRepositoryRoot.Find();
        string findReferencesViewText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2FindReferencesView.xaml"));
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string findReferencesText = shellText + findReferencesViewText;
        string peekDefinitionText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2PeekDefinitionWindow.xaml"));
        string peekDefinitionCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2PeekDefinitionWindow.xaml.cs"));

        Assert.Contains("Title=\"查找引用\"", findReferencesText);
        Assert.Contains("Header=\"所在 Section\"", findReferencesText);
        Assert.Contains("Header=\"字段\"", findReferencesText);
        Assert.Contains("Header=\"值\"", findReferencesText);
        Assert.Contains("Header=\"行\"", findReferencesText);
        Assert.Contains("IdeAssistToolWindowRootStyle", findReferencesText);
        Assert.Contains("IdeAssistDataGridStyle", findReferencesText);
        Assert.Contains("AutomationProperties.AutomationId=\"Ra2FindReferences.Header\"", findReferencesText);
        Assert.Contains("ContentId=\"Tool.FindReferences\"", shellText);
        Assert.Contains("FloatingWidth=\"700\"", shellText);
        Assert.Contains("FloatingHeight=\"460\"", shellText);
        Assert.Contains("Ra2FindReferences.View", findReferencesViewText);
        Assert.Contains("languageViews:Ra2FindReferencesView", shellText);
        Assert.DoesNotContain("Title=\"Find References\"", findReferencesText);
        Assert.DoesNotContain("Header=\"Key\"", findReferencesText);
        Assert.DoesNotContain("Header=\"Value\"", findReferencesText);
        Assert.DoesNotContain("Header=\"Line\"", findReferencesText);

        Assert.Contains("Title=\"查看定义\"", peekDefinitionText);
        Assert.Contains("Text=\"来源\"", peekDefinitionText);
        Assert.Contains("Text=\"说明\"", peekDefinitionText);
        Assert.Contains("IdeAssistPopupFrameStyle", peekDefinitionText);
        Assert.Contains("IdeAssistInspectorRootStyle", peekDefinitionText);
        Assert.Contains("IdeAssistBadgeStyle", peekDefinitionText);
        Assert.Contains("IdeAssistPathTextStyle", peekDefinitionText);
        Assert.Contains("AutomationProperties.AutomationId=\"Ra2PeekDefinition.ContentScrollViewer\"", peekDefinitionText);
        Assert.Contains("AutomationProperties.AutomationId=\"Ra2PeekDefinition.CloseButton\"", peekDefinitionText);
        Assert.Contains("Click=\"CloseButton_OnClick\"", peekDefinitionText);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", peekDefinitionText);
        Assert.Contains("AllowsTransparency=\"True\"", peekDefinitionText);
        Assert.Contains("ResizeMode=\"NoResize\"", peekDefinitionText);
        Assert.Contains("ShowInTaskbar=\"False\"", peekDefinitionText);
        Assert.Contains("WindowStartupLocation=\"Manual\"", peekDefinitionText);
        Assert.Contains("WindowStyle=\"None\"", peekDefinitionText);
        Assert.Contains("SizeToContent=\"WidthAndHeight\"", peekDefinitionText);
        Assert.Contains("ApplyBorderlessFloatingHostOptions", peekDefinitionCode);
        Assert.Contains("WindowStyle = WindowStyle.None;", peekDefinitionCode);
        Assert.Contains("ShowInTaskbar = false;", peekDefinitionCode);
        Assert.Contains("ResizeMode = ResizeMode.NoResize;", peekDefinitionCode);
        Assert.Contains("SizeToContent = SizeToContent.WidthAndHeight;", peekDefinitionCode);
        Assert.Contains("PlaceNearCaret", peekDefinitionCode);
        Assert.Contains("CloseButton_OnClick", peekDefinitionCode);
        Assert.Contains("Width=\"500\"", peekDefinitionText);
        Assert.DoesNotContain("ResizeMode=\"CanResize\"", peekDefinitionText);
        Assert.DoesNotContain("MinHeight=\"", peekDefinitionText);
        Assert.DoesNotContain("Height=\"320\"", peekDefinitionText);
        Assert.DoesNotContain("Height=\"460\"", peekDefinitionText);
        Assert.DoesNotContain("Text=\"类型\"", peekDefinitionText);
        Assert.DoesNotContain("Text=\"行\"", peekDefinitionText);
        Assert.DoesNotContain("Title=\"Peek Definition\"", peekDefinitionText);
    }

    [Fact]
    public void DefinitionNavigation_SynchronizesProjectExplorerSelectionWithoutReentry()
    {
        string root = TestRepositoryRoot.Find();
        string shellCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string shellCodeText = File.ReadAllText(shellCodePath);

        string goToDefinitionMethod = ExtractPrivateMethod(shellCodeText, "GoToDefinition_OnClick");
        string languageTargetMethod = ExtractPrivateMethod(shellCodeText, "TryScrollSourceEditorToLanguageTarget");
        string selectExplorerItemMethod = ExtractPrivateMethod(shellCodeText, "SelectProjectExplorerItem");
        string selectedItemChangedMethod = ExtractPrivateMethod(shellCodeText, "ProjectExplorerTreeView_OnSelectedItemChanged");
        string findSectionItemMethod = ExtractPrivateMethod(shellCodeText, "FindProjectExplorerSectionItem");

        Assert.Contains("result.SectionName", goToDefinitionMethod);
        Assert.Contains("viewModel.ProjectExplorer.MarkCurrentSection(viewModel.CurrentSnapshot.FilePath, sectionName);", languageTargetMethod);
        Assert.Contains("FindProjectExplorerSectionItem(", languageTargetMethod);
        Assert.Contains("viewModel.CurrentSnapshot.FilePath", languageTargetMethod);
        Assert.Contains("SelectProjectExplorerItem(matchingSection);", languageTargetMethod);
        Assert.Contains("Navigation tree did not contain", languageTargetMethod);

        Assert.Contains("_isRestoringProjectExplorerSelection = true;", selectExplorerItemMethod);
        Assert.Contains("container.IsSelected = true;", selectExplorerItemMethod);
        Assert.Contains("container.BringIntoView();", selectExplorerItemMethod);
        Assert.Contains("container.Focus();", selectExplorerItemMethod);

        Assert.Contains("if (_isRestoringProjectExplorerSelection)", selectedItemChangedMethod);
        Assert.Contains("return;", selectedItemChangedMethod);

        Assert.Contains("string.Equals(descendant.FilePath, filePath, StringComparison.OrdinalIgnoreCase)", findSectionItemMethod);
        Assert.Contains("string.Equals(descendant.SectionId, sectionId, StringComparison.OrdinalIgnoreCase)", findSectionItemMethod);
        Assert.DoesNotContain(".Contains(", findSectionItemMethod, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".StartsWith(", findSectionItemMethod, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShellSourceEditor_FindReferencesMenuUsesCurrentFileReferenceContext()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellCodePath);
        string contextMenuMethod = ExtractPrivateMethod(shellCodeText, "SourceEditorContextMenu_OnOpened");
        string findReferencesMethod = ExtractPrivateMethod(shellCodeText, "FindAllReferences_OnClick");
        string requestMethod = ExtractPrivateMethod(shellCodeText, "TryBuildFindReferencesNavigationRequest");

        Assert.Contains("x:Name=\"FindReferencesMenuItem\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.FindReferencesMenuItem\"", shellText);
        Assert.Contains("Header=\"查找当前文件引用\"", shellText);
        Assert.Contains("IsEnabled=\"False\"", shellText);

        Assert.Contains("FindReferencesMenuItem.IsEnabled = false;", contextMenuMethod);
        Assert.Contains("GetContextMenuSelectionSpan(offset)", contextMenuMethod);
        Assert.Contains("CanFindCurrentFileReferences(model, context, selectionSpan)", contextMenuMethod);

        Assert.Contains("ReferenceEquals(sender, FindReferencesMenuItem)", findReferencesMethod);
        Assert.Contains("TryBuildFindReferencesNavigationRequest(", findReferencesMethod);
        Assert.Contains("GetContextMenuSelectionSpan(offset)", requestMethod);
        Assert.DoesNotContain("ProjectSaveService", findReferencesMethod, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractPrivateMethod(string source, string methodName)
    {
        int methodStart = -1;
        int methodNameIndex = -1;
        int searchIndex = 0;
        while (true)
        {
            methodNameIndex = source.IndexOf(methodName, searchIndex, StringComparison.Ordinal);
            if (methodNameIndex < 0)
                break;

            int lineStart = source.LastIndexOf('\n', methodNameIndex);
            int lineEnd = source.IndexOf('\n', methodNameIndex);
            if (lineEnd < 0)
                lineEnd = source.Length;

            string declarationLine = source[(lineStart + 1)..lineEnd].TrimStart();
            if (declarationLine.StartsWith("private ", StringComparison.Ordinal) &&
                declarationLine.Contains($"{methodName}(", StringComparison.Ordinal))
            {
                methodStart = lineStart;
                break;
            }

            searchIndex = methodNameIndex + methodName.Length;
        }

        Assert.True(methodNameIndex >= 0, $"Method '{methodName}' should exist.");
        Assert.True(methodStart >= 0, $"Method '{methodName}' should be private.");

        int nextMethodStart = source.IndexOf("\n    private ", methodNameIndex + methodName.Length, StringComparison.Ordinal);
        if (nextMethodStart < 0)
            nextMethodStart = source.Length;

        return source[methodStart..nextMethodStart];
    }
}


