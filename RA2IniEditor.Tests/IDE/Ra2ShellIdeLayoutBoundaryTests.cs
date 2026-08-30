using Xunit;

using System.Threading;
using System.Windows;
using System.Windows.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using RA2IniEditor.IDE.Views;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ShellIdeLayoutBoundaryTests
{
    [Fact]
    public void FloatingChrome_UsesAvalonDockLifecycleAndHideRecoveryWithoutBusinessState()
    {
        string root = TestRepositoryRoot.Find();
        string controllerText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellDockFloatingChromeController.cs"));
        string chromeControllerText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindowChromeController.cs"));
        string shellCodeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string themeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "ShellTheme.xaml"));

        Assert.Contains("LayoutFloatingWindowControlCreated", controllerText, StringComparison.Ordinal);
        Assert.Contains("LayoutFloatingWindowControlClosed", controllerText, StringComparison.Ordinal);
        Assert.Contains("anchorable.Hide()", controllerText, StringComparison.Ordinal);
        Assert.DoesNotContain("anchorable.Close()", controllerText, StringComparison.Ordinal);
        Assert.Contains("Application.Current.Windows.OfType<LayoutFloatingWindowControl>()", controllerText, StringComparison.Ordinal);
        Assert.Contains("RegisterHost(host)", controllerText, StringComparison.Ordinal);
        Assert.Contains("host.Style = hostStyle", controllerText, StringComparison.Ordinal);
        Assert.DoesNotContain("HideSinglePaneHeader", controllerText, StringComparison.Ordinal);
        Assert.DoesNotContain("paneControl.Template", controllerText, StringComparison.Ordinal);
        Assert.DoesNotContain("Grid.SetRow(contentHost", controllerText, StringComparison.Ordinal);
        Assert.DoesNotContain("Grid.SetRowSpan(contentHost", controllerText, StringComparison.Ordinal);
        Assert.Matches("x:Name=\"PART_FloatingContentHost\"\\s+Grid.Row=\"1\"", themeText);
        Assert.Contains("<Setter TargetName=\"HeaderRow\" Property=\"Height\" Value=\"0\" />", themeText, StringComparison.Ordinal);
        Assert.Contains("<Setter TargetName=\"PaneHeader\" Property=\"Visibility\" Value=\"Collapsed\" />", themeText, StringComparison.Ordinal);
        Assert.Contains("Model.IsDirectlyHostedInFloatingWindow", themeText, StringComparison.Ordinal);
        Assert.Contains("Model.ChildrenCount", themeText, StringComparison.Ordinal);
        Assert.DoesNotContain("Model.IsSinglePane", themeText, StringComparison.Ordinal);
        Assert.Contains("SystemCommands.MinimizeWindow(host)", controllerText, StringComparison.Ordinal);
        Assert.Contains("RestoreAndActivateMinimizedHost", controllerText, StringComparison.Ordinal);
        Assert.Contains("SystemCommands.RestoreWindow(host)", controllerText, StringComparison.Ordinal);
        Assert.Contains("DispatcherPriority.Loaded", controllerText, StringComparison.Ordinal);
        Assert.Contains("host.Activate();", controllerText, StringComparison.Ordinal);
        Assert.True(
            controllerText.IndexOf("SystemCommands.RestoreWindow(host)", StringComparison.Ordinal) <
            controllerText.IndexOf("DispatcherPriority.Loaded", StringComparison.Ordinal));
        Assert.True(
            controllerText.IndexOf("DispatcherPriority.Loaded", StringComparison.Ordinal) <
            controllerText.IndexOf("host.Activate();", StringComparison.Ordinal));
        Assert.True(
            controllerText.IndexOf("host.Activate();", StringComparison.Ordinal) <
            controllerText.IndexOf("focusContent();", StringComparison.Ordinal));
        Assert.DoesNotContain("Task.Delay", controllerText, StringComparison.Ordinal);
        Assert.DoesNotContain("Thread.Sleep", controllerText, StringComparison.Ordinal);
        Assert.Contains("FrameworkElement? maximizeRegion", chromeControllerText, StringComparison.Ordinal);
        Assert.Contains("if (_maximizeRegion is not null)", chromeControllerText, StringComparison.Ordinal);
        Assert.Contains("maximizeRegion: null", controllerText, StringComparison.Ordinal);
        Assert.Contains("_floatingChromeController.RestoreAndActivateMinimizedHost", shellCodeText, StringComparison.Ordinal);
        Assert.DoesNotContain("PART_FloatingMaximizeRestoreButton", themeText, StringComparison.Ordinal);
        Assert.DoesNotContain("Shell.Dock.FloatingHost.MaximizeRestoreButton", themeText, StringComparison.Ordinal);
        Assert.Contains("_floatingChromeController.Attach();", shellCodeText, StringComparison.Ordinal);
        Assert.Contains("_floatingChromeController.RefreshExistingHosts", shellCodeText, StringComparison.Ordinal);
        Assert.Contains("_floatingChromeController.Dispose();", shellCodeText, StringComparison.Ordinal);
        Assert.Contains("WmGetMinMaxInfo", chromeControllerText, StringComparison.Ordinal);
        Assert.Contains("TryApplyMaximizedWorkArea", chromeControllerText, StringComparison.Ordinal);
        Assert.Contains("Marshal.StructureToPtr(minMaxInfo, minMaxInfoPointer, false)", chromeControllerText, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, 0, 1920, 1080, 0, 0, 1920, 1040, 0, 0, 1920, 1040)]
    [InlineData(0, 0, 1920, 1080, 40, 40, 1880, 1040, 40, 40, 1880, 1040)]
    [InlineData(-1920, 0, 1920, 1080, -1920, 0, 1920, 1040, 0, 0, 1920, 1040)]
    [InlineData(1920, -1080, 1920, 1080, 1920, -1080, 1920, 1080, 0, 0, 1920, 1080)]
    public void MainShellChrome_CalculatesMaximizedBoundsInNativeMonitorCoordinates(
        int monitorX,
        int monitorY,
        int monitorWidth,
        int monitorHeight,
        int workX,
        int workY,
        int workWidth,
        int workHeight,
        int expectedX,
        int expectedY,
        int expectedWidth,
        int expectedHeight)
    {
        bool calculated = ShellWindowChromeController.TryCalculateMaximizedBounds(
            new Int32Rect(monitorX, monitorY, monitorWidth, monitorHeight),
            new Int32Rect(workX, workY, workWidth, workHeight),
            out Int32Rect result);

        Assert.True(calculated);
        Assert.Equal(new Int32Rect(expectedX, expectedY, expectedWidth, expectedHeight), result);
    }

    [Theory]
    [InlineData(-1, 0, 1920, 1040)]
    [InlineData(0, -1, 1920, 1040)]
    [InlineData(0, 0, 1921, 1040)]
    [InlineData(0, 0, 1920, 1081)]
    public void MainShellChrome_RejectsWorkAreasOutsideTheMonitor(
        int workX,
        int workY,
        int workWidth,
        int workHeight)
    {
        bool calculated = ShellWindowChromeController.TryCalculateMaximizedBounds(
            new Int32Rect(0, 0, 1920, 1080),
            new Int32Rect(workX, workY, workWidth, workHeight),
            out Int32Rect result);

        Assert.False(calculated);
        Assert.Equal(Int32Rect.Empty, result);
    }

    [Fact]
    public void AvalonDockShell_UsesPinnedStablePackageAndApprovedLayoutContract()
    {
        string root = TestRepositoryRoot.Find();
        string projectText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "RA2IniEditor.IDE.csproj"));
        string shellText = ReadShellWindowXaml();
        string shellCodeText = ReadShellWindowCode();

        Assert.Contains("<PackageReference Include=\"Dirkster.AvalonDock\" Version=\"4.74.1\" />", projectText);
        Assert.Contains("xmlns:avalondock=\"https://github.com/Dirkster99/AvalonDock\"", shellText);
        Assert.DoesNotContain("Xceed.Wpf.AvalonDock", shellText + shellCodeText, StringComparison.Ordinal);
        Assert.Contains("WindowState=\"Maximized\"", shellText);
        Assert.Contains("DockWidth=\"300\"", shellText);
        Assert.Contains("DockHeight=\"260\"", shellText);

        foreach (string contentId in new[]
                 {
                     "Document.Source",
                     "Tool.SectionExplorer",
                     "Tool.AiAssistant",
                     "Tool.Problems",
                     "Tool.Output",
                     "Tool.Search",
                     "Tool.FindReferences"
                 })
        {
            Assert.Contains($"ContentId=\"{contentId}\"", shellText);
        }

        Assert.Contains("x:Name=\"SourceDocumentAnchorable\"", shellText);
        Assert.Contains("CanClose=\"False\"", shellText);
        Assert.Contains("CanFloat=\"False\"", shellText);
        Assert.Contains("CanMove=\"False\"", shellText);
        Assert.Contains("ShowAndActivateBottomTool(\"Tool.FindReferences\"", shellCodeText);
        Assert.Contains("ApplyCompiledDefaultTopology();", shellCodeText);
        Assert.Contains("FindTool(\"Tool.Output\")", shellCodeText);
        Assert.DoesNotContain("private SearchToolWindow?", shellCodeText, StringComparison.Ordinal);
        Assert.DoesNotContain("private Ra2FindReferencesWindow?", shellCodeText, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_DefinesIdeMenuAndBottomToolTabsWithoutLegacyHeaderToolbar()
    {
        string shellText = ReadShellWindowXaml();

        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainMenu\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.BottomToolTabs\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.BottomToolTabs.ErrorList\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.BottomToolTabs.Output\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.Tool.Search.Content\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.Menu.OpenFolder\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.Menu.Search\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.Menu.FieldRegistryCenter\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.Menu.Issues\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.Menu.ToggleProjectExplorer\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"Shell.Toolbar\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"Shell.OpenFolderButton\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"Shell.FieldRegistryButton\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"Shell.Toolbar.SaveCurrentFileButton\"", shellText);
    }

    [Fact]
    public void SourceEditorCommandStrip_ProvidesEditingEntryPointsWithoutLegacyHeaderToolbar()
    {
        string shellText = ReadShellWindowXaml();

        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.SaveCurrentFileButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.UndoButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.RedoButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.RevertInMemoryChangesButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.EditorColumn\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.OpenFolderButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.SearchButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.FieldRegistryButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.IssuesButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.DocumentTabStrip\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.DocumentTab\"", shellText);
        Assert.Contains("HorizontalAlignment=\"Left\"", shellText);
        Assert.Contains("Value=\"未选择文件\"", shellText);
        Assert.Contains("Value=\"Collapsed\"", shellText);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"Shell.SourceEditor.DocumentToolbar\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeIconCommandButtonStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdePrimaryIconCommandButtonStyle}\"", shellText);
        Assert.Contains("Style=\"{StaticResource IdeMainToolbarStyle}\"", shellText);
        Assert.Contains("Content=\"{StaticResource IconUndo}\"", shellText);
        Assert.Contains("Content=\"{StaticResource IconRedo}\"", shellText);
        Assert.Contains("Content=\"{StaticResource IconSave}\"", shellText);
        Assert.Contains("Content=\"{StaticResource IconRevert}\"", shellText);
        Assert.DoesNotContain("Content=\"保存当前文件\"", shellText);
        Assert.DoesNotContain("Content=\"撤销内存修改\"", shellText);
        Assert.DoesNotContain("Text=\"{Binding SourceEditor.MetadataText}\"", shellText);
        Assert.DoesNotContain("ItemsSource=\"{Binding Documents", shellText);
        Assert.DoesNotContain("CloseDocument", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.AddPropertyMenuItem\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.SourceEditor.ShowCompletionPreviewMenuItem\"", shellText);
        Assert.DoesNotContain("Text=\"{Binding Title}\"", shellText);
        Assert.DoesNotContain("Text=\"{Binding ProjectTitle}\"", shellText);
    }

    [Fact]
    public void BottomIssuesTab_ReusesStructuredIssuesViewModelAndCurrentNavigationHandlers()
    {
        string shellText = ReadShellWindowXaml();
        string shellCodeText = ReadShellWindowCode();

        Assert.Contains("ItemsSource=\"{Binding Issues.Items}\"", shellText);
        Assert.Contains("SelectedItem=\"{Binding Issues.SelectedIssue, Mode=TwoWay}\"", shellText);
        Assert.Contains("Text=\"{Binding Issues.StatusText}\"", shellText);
        Assert.Contains("Shell.BottomIssues.Count.All", shellText);
        Assert.Contains("Shell.BottomIssues.Count.Error", shellText);
        Assert.Contains("Shell.BottomIssues.Count.Warning", shellText);
        Assert.Contains("Shell.BottomIssues.Count.Info", shellText);
        Assert.Contains("MouseDoubleClick=\"BottomIssuesGrid_OnMouseDoubleClick\"", shellText);
        Assert.Contains("RefreshCurrentFileDiagnosticsFromShell", shellText);
        Assert.Contains("RunManualFullDiagnosticsFromShell", shellText);
        Assert.DoesNotContain("ClearIssueFiltersFromShell", shellText);
        Assert.Contains("ClearIssuesFromShell", shellText);
        Assert.Contains("await TryNavigateToIssueAsync(viewModel, viewModel.Issues.SelectedIssue)", shellCodeText);
        Assert.Contains("RefreshCurrentFileDiagnostics(", shellCodeText);
        Assert.Contains("RunManualFullDiagnosticsAsync(", shellCodeText);
        Assert.Contains("_fieldRegistryRuntimeService.CurrentProvider", shellCodeText);
    }

    [Fact]
    public void BottomOutputAndSearchTools_UseApprovedDockHostWithoutBusinessDependencies()
    {
        string shellText = ReadShellWindowXaml();
        string shellCodeText = ReadShellWindowCode();
        string combinedText = shellText + shellCodeText;

        Assert.Contains("AutomationProperties.AutomationId=\"Shell.OutputTextBox\"", shellText);
        Assert.Contains("Text=\"{Binding OutputText, Mode=OneWay}\"", shellText);
        Assert.Contains("ContentId=\"Tool.Output\"", shellText);
        Assert.Contains("ContentId=\"Tool.Search\"", shellText);
        Assert.Contains("<views:SearchToolView />", shellText);
        Assert.Contains("FocusSearchResultsToolTab", shellCodeText);
        Assert.Contains("ShowAndActivateSearchTool", shellCodeText);
        Assert.Contains("ShellDockHomeZone.Floating, 0, false, 560, 620", shellCodeText);
        Assert.Contains("GetDockViewportScreenBounds", shellCodeText);
        Assert.DoesNotContain("ShowAndActivateBottomTool(\"Tool.Search\"", shellCodeText, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DiagnosticRuleRegistry", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AvalonDockShell_UsesProjectOwnedModernThemeAndStableRenderedHeaderAutomationIds()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = ReadShellWindowXaml();
        string themeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "ShellTheme.xaml"));

        Assert.Contains("AnchorablePaneControlStyle=\"{StaticResource IdeDockAnchorablePaneControlStyle}\"", shellText);
        Assert.Contains("AnchorableHeaderTemplate=\"{StaticResource IdeDockAnchorableHeaderTemplate}\"", shellText);
        Assert.Contains("GridSplitterHorizontalStyle=\"{StaticResource IdeDockSplitterStyle}\"", shellText);
        Assert.Contains("GridSplitterVerticalStyle=\"{StaticResource IdeDockSplitterStyle}\"", shellText);
        Assert.Contains("x:Key=\"IdeDockAnchorablePaneControlStyle\"", themeText);
        Assert.Contains("x:Key=\"IdeDockAnchorableHeaderTemplate\"", themeText);
        Assert.Contains("StringFormat=Shell.Dock.Header.{0}", themeText);
        Assert.Contains("StringFormat=Shell.Dock.Tab.{0}", themeText);
        Assert.Contains("TabStripPlacement\" Value=\"Top\"", themeText);
        Assert.DoesNotContain("Dirkster.AvalonDock.Themes.", shellText + themeText, StringComparison.Ordinal);
        Assert.DoesNotContain("Xceed.Wpf.AvalonDock", shellText + themeText, StringComparison.Ordinal);
    }

    [Fact]
    public void AvalonDockShell_DefinesDeterministicHomeRecoveryAndWindowLayoutCommands()
    {
        string shellText = ReadShellWindowXaml();
        string shellCodeText = ReadShellWindowCode();
        string coordinatorText = ReadDockCoordinatorCode();
        string sessionText = ReadDockSessionCode();

        Assert.Contains("AnchorableHiding=\"ShellDockManager_OnAnchorableHiding\"", shellText);
        Assert.Contains("x:Name=\"BottomToolPaneGroup\"", shellText);
        Assert.Contains("x:Name=\"RightToolPaneGroup\"", shellText);
        Assert.Contains("FloatingWidth=\"800\"", shellText);
        Assert.Contains("FloatingHeight=\"420\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.MainToolbar.WindowLayoutButton\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.WindowLayout.ReturnFloatingToolsHome\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.WindowLayout.ResetDefaultLayout\"", shellText);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.Menu.WindowLayout\"", shellText);
        Assert.Contains("ShellDockManager_OnAnchorableHiding", shellCodeText);
        Assert.Contains("TryBeginFloatingHideRecovery", shellCodeText);
        Assert.Contains("BeginShellClose", shellCodeText);
        Assert.Contains("ReturnFloatingToolsHome", coordinatorText);
        Assert.Contains("ResetToCompiledDefault", sessionText);
        Assert.Contains("CollectGarbage", coordinatorText);
        Assert.DoesNotContain("XmlLayoutSerializer", coordinatorText, StringComparison.Ordinal);
        Assert.DoesNotContain("File.Write", coordinatorText, StringComparison.Ordinal);
    }

    [Fact]
    public void XmlLayoutSerializer_ReplacesLayoutModelsAndPreservesExistingContentInstances()
    {
        RunInSta(() =>
        {
            object documentContent = new();
            object toolContent = new();
            LayoutDocument originalDocument = new()
            {
                ContentId = "Document.Source",
                Content = documentContent
            };
            LayoutAnchorable originalTool = new()
            {
                ContentId = "Tool.Output",
                Content = toolContent
            };
            DockingManager manager = new()
            {
                Layout = CreateLayout(originalDocument, originalTool)
            };
            LayoutRoot originalRoot = manager.Layout;

            string serialized;
            using (StringWriter writer = new())
            {
                new XmlLayoutSerializer(manager).Serialize(writer);
                serialized = writer.ToString();
            }

            XmlLayoutSerializer restorer = new(manager);
            restorer.LayoutSerializationCallback += (_, args) =>
            {
                args.Content = args.Model.ContentId switch
                {
                    "Document.Source" => documentContent,
                    "Tool.Output" => toolContent,
                    _ => null
                };
            };

            using StringReader reader = new(serialized);
            restorer.Deserialize(reader);

            LayoutDocument restoredDocument = manager.Layout.Descendents()
                .OfType<LayoutDocument>()
                .Single(content => content.ContentId == "Document.Source");
            LayoutAnchorable restoredTool = manager.Layout.Descendents()
                .OfType<LayoutAnchorable>()
                .Single(content => content.ContentId == "Tool.Output");

            Assert.NotSame(originalRoot, manager.Layout);
            Assert.NotSame(originalDocument, restoredDocument);
            Assert.NotSame(originalTool, restoredTool);
            Assert.Same(documentContent, restoredDocument.Content);
            Assert.Same(toolContent, restoredTool.Content);
        });
    }

    [Fact]
    public void XmlLayoutSerializer_RoundTripsTheCompleteContractLayout()
    {
        RunInSta(() =>
        {
            string[] expectedContentIds =
            [
                "Document.Source",
                "Tool.Problems",
                "Tool.Output",
                "Tool.Search",
                "Tool.FindReferences",
                "Tool.SectionExplorer",
                "Tool.AiAssistant"
            ];
            Dictionary<string, object> contentById = expectedContentIds.ToDictionary(
                contentId => contentId,
                _ => new object(),
                StringComparer.Ordinal);
            DockingManager manager = new()
            {
                Layout = CreateContractLayout(contentById)
            };

            string serialized;
            using (StringWriter writer = new())
            {
                new XmlLayoutSerializer(manager).Serialize(writer);
                serialized = writer.ToString();
            }

            XmlLayoutSerializer restorer = new(manager);
            restorer.LayoutSerializationCallback += (_, args) =>
            {
                if (contentById.TryGetValue(args.Model.ContentId, out object? content))
                    args.Content = content;
                else
                    args.Cancel = true;
            };

            using StringReader reader = new(serialized);
            restorer.Deserialize(reader);

            LayoutContent[] restoredContents = manager.Layout.Descendents()
                .OfType<LayoutContent>()
                .OrderBy(content => content.ContentId, StringComparer.Ordinal)
                .ToArray();
            Assert.Equal(expectedContentIds.Order(StringComparer.Ordinal), restoredContents.Select(content => content.ContentId));
            Assert.Single(restoredContents.OfType<LayoutDocument>());
            Assert.Equal(6, restoredContents.OfType<LayoutAnchorable>().Count());
            foreach (LayoutContent restoredContent in restoredContents)
                Assert.Same(contentById[restoredContent.ContentId], restoredContent.Content);
        });
    }

    [Fact]
    public void XmlLayoutSerializer_CallbackCanRejectUnknownContentIdentity()
    {
        RunInSta(() =>
        {
            LayoutDocument sourceDocument = new()
            {
                ContentId = "Document.Source",
                Content = new object()
            };
            LayoutAnchorable knownTool = new()
            {
                ContentId = "Tool.Output",
                Content = new object()
            };
            LayoutAnchorable unknownTool = new()
            {
                ContentId = "Tool.Unknown",
                Content = new object()
            };
            DockingManager manager = new()
            {
                Layout = CreateLayout(sourceDocument, knownTool, unknownTool)
            };

            string serialized;
            using (StringWriter writer = new())
            {
                new XmlLayoutSerializer(manager).Serialize(writer);
                serialized = writer.ToString();
            }

            object currentDocumentContent = new();
            object currentToolContent = new();
            manager.Layout = CreateLayout(
                new LayoutDocument
                {
                    ContentId = "Document.Source",
                    Content = currentDocumentContent
                },
                new LayoutAnchorable
                {
                    ContentId = "Tool.Output",
                    Content = currentToolContent
                });

            XmlLayoutSerializer restorer = new(manager);
            restorer.LayoutSerializationCallback += (_, args) =>
            {
                switch (args.Model.ContentId)
                {
                    case "Document.Source":
                        args.Content = currentDocumentContent;
                        break;
                    case "Tool.Output":
                        args.Content = currentToolContent;
                        break;
                    default:
                        args.Cancel = true;
                        break;
                }
            };

            using StringReader reader = new(serialized);
            restorer.Deserialize(reader);

            LayoutContent[] restoredContents = manager.Layout.Descendents()
                .OfType<LayoutContent>()
                .ToArray();
            Assert.Equal(2, restoredContents.Length);
            Assert.DoesNotContain(restoredContents, content => content.ContentId == "Tool.Unknown");
            Assert.Same(
                currentDocumentContent,
                restoredContents.Single(content => content.ContentId == "Document.Source").Content);
            Assert.Same(
                currentToolContent,
                restoredContents.Single(content => content.ContentId == "Tool.Output").Content);
        });
    }

    [Fact]
    public void LayoutAnchorable_AddToLayoutProvidesBottomAndRightHomeFallbacks()
    {
        RunInSta(() =>
        {
            LayoutDocument sourceDocument = new()
            {
                ContentId = "Document.Source",
                Content = new object()
            };
            DockingManager manager = new()
            {
                Layout = CreateLayout(sourceDocument)
            };
            LayoutAnchorable bottomTool = new() { ContentId = "Tool.Output" };
            LayoutAnchorable rightTool = new() { ContentId = "Tool.SectionExplorer" };

            bottomTool.AddToLayout(manager, AnchorableShowStrategy.Bottom);
            rightTool.AddToLayout(manager, AnchorableShowStrategy.Right);

            Assert.IsType<LayoutAnchorablePane>(bottomTool.Parent);
            Assert.IsType<LayoutAnchorablePane>(rightTool.Parent);
            Assert.Contains(bottomTool, manager.Layout.Descendents().OfType<LayoutAnchorable>());
            Assert.Contains(rightTool, manager.Layout.Descendents().OfType<LayoutAnchorable>());
            Assert.NotSame(bottomTool.Parent, rightTool.Parent);
        });
    }

    [Fact]
    public void ShellDockLayoutSession_ResetIsIdempotentAndPreservesContentInstances()
    {
        RunInSta(() =>
        {
            object documentContent = new();
            object firstContent = new();
            object secondContent = new();
            object rightContent = new();
            LayoutDocument document = new() { ContentId = "Document.Source", Content = documentContent };
            LayoutAnchorable firstBottom = new() { ContentId = "Tool.FirstBottom", Content = firstContent };
            LayoutAnchorable secondBottom = new() { ContentId = "Tool.SecondBottom", Content = secondContent };
            LayoutAnchorable rightTool = new() { ContentId = "Tool.Right", Content = rightContent };
            LayoutAnchorablePane bottomPane = new(firstBottom);
            bottomPane.InsertChildAt(1, secondBottom);
            LayoutAnchorablePaneGroup bottomGroup = new(bottomPane) { DockHeight = new GridLength(260) };
            LayoutPanel editorPanel = new(new LayoutDocumentPaneGroup(new LayoutDocumentPane(document)))
            {
                Orientation = System.Windows.Controls.Orientation.Vertical
            };
            editorPanel.InsertChildAt(1, bottomGroup);
            LayoutAnchorablePane rightPane = new(rightTool);
            LayoutAnchorablePaneGroup rightGroup = new(rightPane) { DockWidth = new GridLength(300) };
            LayoutPanel rootPanel = new(editorPanel);
            rootPanel.InsertChildAt(1, rightGroup);
            DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = rootPanel } };
            ShellDockToolProfile[] profiles =
            [
                new("Tool.FirstBottom", ShellDockHomeZone.Bottom, 0, true, 880, 460),
                new("Tool.SecondBottom", ShellDockHomeZone.Bottom, 1, true, 800, 420),
                new("Tool.Right", ShellDockHomeZone.Right, 0, true, 360, 760)
            ];
            ShellDockLayoutSession session = new(manager, [document, firstBottom, secondBottom, rightTool], profiles);
            ShellDockLayoutCoordinator coordinator = new(manager, () => new Size(1280, 700), profiles);
            Assert.True(session.CaptureCompiledDefault().Succeeded);

            bottomPane.RemoveChild(secondBottom);
            rightPane.InsertChildAt(1, secondBottom);
            Assert.True(session.ResetToCompiledDefault().Succeeded);
            Assert.True(session.ResetToCompiledDefault().Succeeded);

            LayoutAnchorable restoredFirst = Assert.IsType<LayoutAnchorable>(session.FindContent("Tool.FirstBottom"));
            LayoutAnchorable restoredSecond = Assert.IsType<LayoutAnchorable>(session.FindContent("Tool.SecondBottom"));
            LayoutAnchorable restoredRight = Assert.IsType<LayoutAnchorable>(session.FindContent("Tool.Right"));
            LayoutDocument restoredDocument = Assert.IsType<LayoutDocument>(session.FindContent("Document.Source"));
            Assert.NotSame(firstBottom, restoredFirst);
            Assert.Same(restoredFirst, coordinator.FindTool("Tool.FirstBottom"));
            coordinator.ShowAndActivate("Tool.SecondBottom");
            Assert.True(restoredSecond.IsSelected);
            Assert.True(restoredSecond.IsActive);
            Assert.Same(firstContent, restoredFirst.Content);
            Assert.Same(secondContent, restoredSecond.Content);
            Assert.Same(rightContent, restoredRight.Content);
            Assert.Same(documentContent, restoredDocument.Content);
            Assert.Same(restoredFirst.Parent, restoredSecond.Parent);
            Assert.NotSame(restoredFirst.Parent, restoredRight.Parent);
            Assert.Equal(new[] { restoredFirst, restoredSecond }, ((LayoutAnchorablePane)restoredFirst.Parent).Children.ToArray());
        });
    }

    [Fact]
    public void ShellDockLayoutSession_DefaultSnapshotIncludesHiddenManagedTools()
    {
        RunInSta(() =>
        {
            object documentContent = new();
            object hiddenContent = new();
            LayoutDocument document = new() { ContentId = "Document.Source", Content = documentContent };
            LayoutAnchorable hiddenTool = new() { ContentId = "Tool.FindReferences", Content = hiddenContent };
            DockingManager manager = new() { Layout = CreateLayout(document, hiddenTool) };
            hiddenTool.Hide();
            ShellDockToolProfile[] profiles =
            [
                new("Tool.FindReferences", ShellDockHomeZone.Bottom, 0, false, 700, 460)
            ];
            ShellDockLayoutSession session = new(manager, [document, hiddenTool], profiles);

            Assert.True(session.CaptureCompiledDefault().Succeeded);
            Assert.True(session.ResetToCompiledDefault().Succeeded);
            LayoutAnchorable restored = Assert.IsType<LayoutAnchorable>(session.FindContent("Tool.FindReferences"));
            Assert.Same(hiddenContent, restored.Content);
            Assert.False(restored.IsVisible);
        });
    }

    [Fact]
    public void ShellDockLayoutSession_SerializedOutputIsAcceptedByVersionedStore()
    {
        RunInSta(() =>
        {
            LayoutDocument document = new() { ContentId = "Document.Source", Content = new object() };
            LayoutAnchorable tool = new() { ContentId = "Tool.Output", Content = new object() };
            DockingManager manager = new() { Layout = CreateLayout(document, tool) };
            ShellDockLayoutSession session = new(
                manager,
                [document, tool],
                [new ShellDockToolProfile("Tool.Output", ShellDockHomeZone.Bottom, 0, true, 800, 420)]);
            string directory = Path.Combine(Path.GetTempPath(), $"ra2-shell-session-store-{Guid.NewGuid():N}");
            Directory.CreateDirectory(directory);
            try
            {
                Assert.True(session.TrySerializeCurrent(out string? serialized).Succeeded);
                ShellDockLayoutStore store = new(directory);
                ShellDockLayoutOperationResult writeResult = store.TryWrite(serialized!);
                Assert.True(writeResult.Succeeded, writeResult.FailureKind.ToString());
                Assert.True(store.TryRead(out string? restored).Succeeded);
                Assert.Equal(serialized, restored);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        });
    }

    [Fact]
    public void ShellDockLayoutStore_V2IsAuthoritativeAndLegacyV1RemainsReadableForMigration()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"ra2-shell-layout-migration-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            const string v1Layout = "<?xml version=\"1.0\" encoding=\"utf-8\"?><LayoutRoot />";
            File.WriteAllText(
                Path.Combine(directory, ShellDockLayoutStore.LegacyLayoutFileName),
                v1Layout,
                new System.Text.UTF8Encoding(false));
            ShellDockLayoutStore store = new(directory);

            Assert.Equal(ShellDockLayoutFailureKind.NotFound, store.TryRead(out _).FailureKind);
            Assert.True(store.TryReadLegacy(out string? restored).Succeeded);
            Assert.Equal(v1Layout, restored);
            Assert.True(store.TryWrite(v1Layout).Succeeded);
            Assert.True(File.Exists(Path.Combine(directory, ShellDockLayoutStore.LayoutFileName)));
            Assert.True(File.Exists(Path.Combine(directory, ShellDockLayoutStore.LegacyLayoutFileName)));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ShellDockTopology_SourceDefinesFloatingHomeWithoutHardCodedSearchPaneAutomation()
    {
        string coordinatorText = ReadDockCoordinatorCode();
        string sessionText = ReadDockSessionCode();
        string storeText = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellDockLayoutStore.cs"));

        Assert.Contains("Floating", coordinatorText, StringComparison.Ordinal);
        Assert.Contains("ApplyCompiledDefaultTopology", coordinatorText, StringComparison.Ordinal);
        Assert.Contains("profile.HomeZone == ShellDockHomeZone.Floating", coordinatorText, StringComparison.Ordinal);
        Assert.Contains("RecoverFloatingHomeBoundsIfNeeded", coordinatorText, StringComparison.Ordinal);
        Assert.Contains("profile.HomeZone != ShellDockHomeZone.Floating", sessionText, StringComparison.Ordinal);
        Assert.DoesNotContain("[\"Tool.Problems\", \"Tool.Output\", \"Tool.Search\"", sessionText, StringComparison.Ordinal);
        Assert.Contains("shell-layout.v2.xml", storeText, StringComparison.Ordinal);
        Assert.Contains("shell-layout.v1.xml", storeText, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellStartup_SuppressesIntermediateFloatingHostsUntilLayoutRestoreCompletes()
    {
        string shellCode = ReadShellWindowCode();
        string shellXaml = ReadShellWindowXaml();
        string floatingChromeCode = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellDockFloatingChromeController.cs"));

        Assert.Contains("_floatingChromeController.BeginInitialLayoutVisibilitySuppression();", shellCode, StringComparison.Ordinal);
        Assert.Contains("await TryRestorePersistedDockLayoutAsync();", shellCode, StringComparison.Ordinal);
        Assert.Contains("_floatingChromeController.RefreshExistingHosts();", shellCode, StringComparison.Ordinal);
        Assert.Contains("finally", shellCode, StringComparison.Ordinal);
        Assert.Contains("_floatingChromeController.CompleteInitialLayoutVisibilitySuppression();", shellCode, StringComparison.Ordinal);
        Assert.Contains("_initialLayoutSuppressedHostOpacities", floatingChromeCode, StringComparison.Ordinal);
        Assert.Contains("host.SetCurrentValue(UIElement.OpacityProperty, 0.0);", floatingChromeCode, StringComparison.Ordinal);
        Assert.Contains("host.SetCurrentValue(UIElement.OpacityProperty, opacity);", floatingChromeCode, StringComparison.Ordinal);
        Assert.Matches(
            "x:Name=\"ShellDockManager\"[\\s\\S]*?IsHitTestVisible=\"False\"[\\s\\S]*?Opacity=\"0\"",
            shellXaml);
        int restore = shellCode.IndexOf("await TryRestorePersistedDockLayoutAsync();", StringComparison.Ordinal);
        int hideFindReferences = shellCode.IndexOf(
            "_dockLayoutCoordinator.ApplyToolCompiledDefaultVisibility(\"Tool.FindReferences\");",
            restore,
            StringComparison.Ordinal);
        int hideSearch = shellCode.IndexOf(
            "_dockLayoutCoordinator.ApplyToolCompiledDefaultVisibility(\"Tool.Search\");",
            hideFindReferences,
            StringComparison.Ordinal);
        int revealDock = shellCode.IndexOf(
            "ShellDockManager.SetCurrentValue(UIElement.OpacityProperty, 1.0);",
            hideSearch,
            StringComparison.Ordinal);
        int ready = shellCode.IndexOf("_shellReady.TrySetResult();", revealDock, StringComparison.Ordinal);
        Assert.True(restore >= 0);
        Assert.True(hideFindReferences > restore);
        Assert.True(hideSearch > hideFindReferences);
        Assert.True(revealDock > hideSearch);
        Assert.True(ready > revealDock);
        Assert.Contains(
            "ShellDockManager.SetCurrentValue(UIElement.IsHitTestVisibleProperty, true);",
            shellCode,
            StringComparison.Ordinal);
        int suppressionStart = floatingChromeCode.IndexOf(
            "private void SuppressInitialLayoutHost",
            StringComparison.Ordinal);
        int suppressionEnd = floatingChromeCode.IndexOf(
            "private sealed class HostRegistration",
            suppressionStart,
            StringComparison.Ordinal);
        Assert.True(suppressionStart >= 0 && suppressionEnd > suppressionStart);
        Assert.DoesNotContain(
            ".Hide()",
            floatingChromeCode[suppressionStart..suppressionEnd],
            StringComparison.Ordinal);
    }

    [Fact]
    public void ShellStartup_DefaultHiddenFloatingToolIsMaterializedOnlyWhenExplicitlyShown()
    {
        RunInSta(() =>
        {
            LayoutDocument document = new() { ContentId = "Document.Source", Content = new object() };
            LayoutAnchorable output = new() { ContentId = "Tool.Output", Content = new object() };
            LayoutAnchorable search = new() { ContentId = "Tool.Search", Content = new object() };
            DockingManager manager = new() { Layout = CreateLayout(document, output, search) };
            search.Hide();
            ShellDockToolProfile[] profiles =
            [
                new("Tool.Output", ShellDockHomeZone.Bottom, 0, true, 800, 420),
                new("Tool.Search", ShellDockHomeZone.Floating, 0, false, 560, 620)
            ];
            ShellDockLayoutCoordinator coordinator = new(manager, () => new Size(1280, 700), profiles);

            coordinator.ApplyCompiledDefaultTopology();
            coordinator.ApplyCompiledDefaultVisibility();

            Assert.False(search.IsVisible);
            Assert.False(search.IsFloating);
            Assert.Equal(560, search.FloatingWidth);
            Assert.Equal(620, search.FloatingHeight);

            coordinator.ShowAndActivate("Tool.Search");

            Assert.True(search.IsVisible);
            Assert.True(search.IsFloating);
            Assert.True(search.IsSelected);
            Assert.True(search.IsActive);
        });
    }

    [Fact]
    public void SearchCommand_LoadedHiddenBottomToolCreatesARealFloatingHostWithoutCrashing()
    {
        RunInSta(() =>
        {
            LayoutDocument document = new() { ContentId = "Document.Source", Content = new object() };
            LayoutAnchorable output = new() { ContentId = "Tool.Output", Content = new object() };
            LayoutAnchorable search = new() { ContentId = "Tool.Search", Content = new object() };
            DockingManager manager = new() { Layout = CreateLayout(document, output, search) };
            ShellDockToolProfile[] profiles =
            [
                new("Tool.Output", ShellDockHomeZone.Bottom, 0, true, 800, 420),
                new("Tool.Search", ShellDockHomeZone.Floating, 0, false, 560, 620)
            ];
            ShellDockLayoutCoordinator coordinator = new(manager, () => new Size(1280, 700), profiles);
            LayoutFloatingWindowControl? floatingHost = null;
            manager.LayoutFloatingWindowControlCreated += (_, args) =>
                floatingHost = args.LayoutFloatingWindowControl;
            Window host = new()
            {
                Width = 1280,
                Height = 700,
                Content = manager,
                ShowInTaskbar = false
            };

            try
            {
                host.Show();
                host.UpdateLayout();
                FlushDispatcher();

                search.Hide();
                FlushDispatcher();
                Assert.False(search.IsVisible);
                Assert.False(search.IsFloating);

                coordinator.ShowAndActivate("Tool.Search");
                FlushDispatcher();

                Assert.True(search.IsVisible);
                Assert.True(search.IsFloating);
                Assert.NotNull(floatingHost);
                Assert.NotSame(output.Parent, search.Parent);
                Assert.IsType<LayoutAnchorablePane>(search.Parent);
            }
            finally
            {
                if (floatingHost is { IsVisible: true })
                    floatingHost.Close();
                host.Close();
                FlushDispatcher();
            }
        });
    }

    [Fact]
    public void SearchCommand_OverridesAnIncorrectVisibleBottomPlacementWithFloatingHome()
    {
        RunInSta(() =>
        {
            LayoutDocument document = new() { ContentId = "Document.Source", Content = new object() };
            LayoutAnchorable output = new() { ContentId = "Tool.Output", Content = new object() };
            LayoutAnchorable search = new() { ContentId = "Tool.Search", Content = new object() };
            DockingManager manager = new() { Layout = CreateLayout(document, output, search) };
            ShellDockToolProfile[] profiles =
            [
                new("Tool.Output", ShellDockHomeZone.Bottom, 0, true, 800, 420),
                new("Tool.Search", ShellDockHomeZone.Floating, 0, false, 560, 620)
            ];
            ShellDockLayoutCoordinator coordinator = new(manager, () => new Size(1280, 700), profiles);

            Assert.True(search.IsVisible);
            Assert.False(search.IsFloating);

            coordinator.ShowAndActivate("Tool.Search");

            Assert.True(search.IsVisible);
            Assert.True(search.IsFloating);
            Assert.True(search.IsSelected);
            Assert.True(search.IsActive);
        });
    }

    [Fact]
    public void ShellStartup_HidesSearchAfterPersistedLayoutRestoreBeforeFloatingHostsAreRefreshed()
    {
        string shellCode = ReadShellWindowCode();
        int restore = shellCode.IndexOf("await TryRestorePersistedDockLayoutAsync();", StringComparison.Ordinal);
        int hideSearch = shellCode.IndexOf(
            "_dockLayoutCoordinator.ApplyToolCompiledDefaultVisibility(\"Tool.Search\");",
            restore,
            StringComparison.Ordinal);
        int refreshHosts = shellCode.IndexOf(
            "_floatingChromeController.RefreshExistingHosts();",
            hideSearch,
            StringComparison.Ordinal);

        Assert.True(restore >= 0);
        Assert.True(hideSearch > restore);
        Assert.True(refreshHosts > hideSearch);
    }

    [Fact]
    public void ShellDockLayoutSession_InvalidIdentityPreflightDoesNotReplaceLiveGraph()
    {
        RunInSta(() =>
        {
            LayoutDocument document = new() { ContentId = "Document.Source", Content = new object() };
            LayoutAnchorable tool = new() { ContentId = "Tool.Output", Content = new object() };
            DockingManager manager = new() { Layout = CreateLayout(document, tool) };
            LayoutRoot originalRoot = manager.Layout;
            ShellDockLayoutSession session = new(
                manager,
                [document, tool],
                [new ShellDockToolProfile("Tool.Output", ShellDockHomeZone.Bottom, 0, true, 800, 420)]);
            const string invalid = "<LayoutRoot><RootPanel><LayoutPanel><LayoutDocumentPane><LayoutDocument ContentId=\"Document.Source\" /></LayoutDocumentPane><LayoutAnchorablePane><LayoutAnchorable ContentId=\"Tool.Unknown\" /></LayoutAnchorablePane></LayoutPanel></RootPanel></LayoutRoot>";

            ShellDockLayoutOperationResult result = session.TryRestore(invalid);

            Assert.Equal(ShellDockLayoutFailureKind.InvalidContentIdentity, result.FailureKind);
            Assert.Same(originalRoot, manager.Layout);
            Assert.Same(document, session.FindContent("Document.Source"));
            Assert.Same(tool, session.FindContent("Tool.Output"));
        });
    }

    [Fact]
    public void ShellDockLifecycle_SourcePreservesApprovedRestoreAndCloseOrdering()
    {
        string shellCode = ReadShellWindowCode();
        Assert.Contains("ShellDockLayoutOperationResult captureResult = _dockLayoutSession.CaptureCompiledDefault();", shellCode);
        Assert.Contains("await TryRestorePersistedDockLayoutAsync();", shellCode);
        Assert.Contains("if (readResult.FailureKind != ShellDockLayoutFailureKind.NotFound)", shellCode);
        Assert.Contains("_dockLayoutStore.TryQuarantine();", shellCode);
        Assert.Contains("await TryMigrateLegacyDockLayoutAsync();", shellCode);
        Assert.Contains("_dockLayoutStore.TryReadLegacy", shellCode);
        Assert.Contains("PlaceToolAtCompiledDefaultHome(\"Tool.Search\")", shellCode);
        Assert.Contains("ApplyToolCompiledDefaultVisibility(\"Tool.Search\")", shellCode);
        Assert.Contains("ShellDockLayoutOperationResult fallbackResult = _dockLayoutSession.ResetToCompiledDefault();", shellCode);
        Assert.Contains("SynchronizeShellStateFromDockLayout();", shellCode);
        Assert.Contains("if (e.Cancel)", shellCode);
        Assert.Contains("_dockLayoutCoordinator.CancelShellClose();", shellCode);
        Assert.Contains("PersistCurrentDockLayout(\"关闭时无法保存窗口布局，已保留上一次有效布局。\"", shellCode);
        Assert.Contains("PersistCurrentDockLayout(\"默认窗口布局已恢复，但无法保存到本机。\"", shellCode);
    }

    private static string ReadShellWindowXaml()
    {
        string root = TestRepositoryRoot.Find();
        return File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
    }

    private static string ReadShellWindowCode()
    {
        string root = TestRepositoryRoot.Find();
        return File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
    }

    private static string ReadDockCoordinatorCode()
    {
        string root = TestRepositoryRoot.Find();
        return File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellDockLayoutCoordinator.cs"));
    }

    private static string ReadDockSessionCode()
    {
        string root = TestRepositoryRoot.Find();
        return File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellDockLayoutSession.cs"));
    }

    private static LayoutRoot CreateLayout(LayoutDocument document, params LayoutAnchorable[] tools)
    {
        LayoutDocumentPane documentPane = new(document);
        LayoutDocumentPaneGroup documentGroup = new(documentPane);
        LayoutPanel rootPanel = new(documentGroup);
        if (tools.Length > 0)
        {
            LayoutAnchorablePane toolPane = new(tools[0]);
            for (int index = 1; index < tools.Length; index++)
                toolPane.InsertChildAt(index, tools[index]);

            rootPanel.InsertChildAt(1, new LayoutAnchorablePaneGroup(toolPane));
        }

        return new LayoutRoot { RootPanel = rootPanel };
    }

    private static LayoutRoot CreateContractLayout(IReadOnlyDictionary<string, object> contentById)
    {
        LayoutDocument document = new()
        {
            ContentId = "Document.Source",
            Content = contentById["Document.Source"],
            CanClose = false,
            CanFloat = false,
            CanMove = false
        };
        LayoutDocumentPaneGroup documentGroup = new(new LayoutDocumentPane(document));
        LayoutAnchorable[] bottomTools =
        [
            CreateTool("Tool.Problems"),
            CreateTool("Tool.Output"),
            CreateTool("Tool.Search"),
            CreateTool("Tool.FindReferences")
        ];
        LayoutAnchorable[] rightTools =
        [
            CreateTool("Tool.SectionExplorer"),
            CreateTool("Tool.AiAssistant")
        ];
        LayoutAnchorablePane bottomPane = new(bottomTools[0]);
        for (int index = 1; index < bottomTools.Length; index++)
            bottomPane.InsertChildAt(index, bottomTools[index]);

        LayoutAnchorablePane rightPane = new(rightTools[0]);
        rightPane.InsertChildAt(1, rightTools[1]);
        LayoutPanel editorPanel = new(documentGroup) { Orientation = System.Windows.Controls.Orientation.Vertical };
        editorPanel.InsertChildAt(1, new LayoutAnchorablePaneGroup(bottomPane));
        LayoutPanel rootPanel = new(editorPanel) { Orientation = System.Windows.Controls.Orientation.Horizontal };
        rootPanel.InsertChildAt(1, new LayoutAnchorablePaneGroup(rightPane));
        return new LayoutRoot { RootPanel = rootPanel };

        LayoutAnchorable CreateTool(string contentId)
            => new()
            {
                ContentId = contentId,
                Content = contentById[contentId]
            };
    }

    private static void RunInSta(Action action)
    {
        Exception? threadFailure = null;
        Thread thread = new(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                threadFailure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(threadFailure);
    }

    private static void FlushDispatcher()
        => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
}

