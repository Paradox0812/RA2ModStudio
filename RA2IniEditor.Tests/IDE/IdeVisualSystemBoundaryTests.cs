using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class IdeVisualSystemBoundaryTests
{
    private static readonly string[] FrozenBrushKeys =
    [
        "UiCanvasBrush",
        "UiSurfaceBrush",
        "UiSurfaceSubtleBrush",
        "UiSurfaceHoverBrush",
        "UiSurfacePressedBrush",
        "UiBorderBrush",
        "UiDividerBrush",
        "UiTextPrimaryBrush",
        "UiTextSecondaryBrush",
        "UiTextDisabledBrush",
        "UiAccentBrush",
        "UiAccentHoverBrush",
        "UiAccentPressedBrush",
        "UiAccentSoftBrush",
        "UiFocusBrush",
        "UiDangerBrush",
        "UiDangerSoftBrush",
        "UiWarningBrush",
        "UiWarningSoftBrush",
        "UiInfoBrush",
        "UiInfoSoftBrush",
        "UiSuccessBrush",
        "UiCodeSurfaceBrush",
        "UiSelectionBrush",
        "UiSelectionInactiveBrush"
    ];

    private static readonly string[] FrozenWorkspaceStyleKeys =
    [
        "IdeWorkspaceRootStyle",
        "IdeWorkspaceCommandBarStyle",
        "IdeWorkspaceCommandButtonStyle",
        "IdeWorkspaceIconButtonStyle",
        "IdeWorkspaceSectionTitleStyle",
        "IdeWorkspaceMetadataTextStyle",
        "IdeWorkspaceStatusBarStyle",
        "IdeWorkspaceDataGridStyle",
        "IdeWorkspaceLogTextBoxStyle",
        "IdeWorkspaceFlatListBoxStyle",
        "IdeWorkspaceHorizontalSelectorStyle",
        "IdeWorkspaceEmptyStateTextStyle",
        "IdeAiWorkspaceRootStyle",
        "IdeAiHeaderStyle",
        "IdeAiContextStripStyle",
        "IdeAiContextTextStyle",
        "IdeAiChatViewportStyle",
        "IdeAiAssistantMessageStyle",
        "IdeAiUserMessageStyle",
        "IdeAiErrorMessageStyle",
        "IdeAiMessageTextStyle",
        "IdeAiMetadataTextStyle",
        "IdeAiComposerStyle",
        "IdeAiComposerInputStyle",
        "IdeAiAdvancedOptionsStyle",
        "IdeAiMarkdownHeadingStyle",
        "IdeAiMarkdownParagraphStyle",
        "IdeAiMarkdownTableStyle",
        "IdeAiMarkdownTableCellStyle",
        "IdeAiCodeBlockStyle",
        "IdeAiReadOnlyCodeTextStyle",
        "IdeAiPlainTextFallbackStyle",
        "IdeAiInlineCodeStyle",
        "IdeHoverCardStyle",
        "IdeHoverCodePillStyle"
    ];

    private static readonly string[] ShellCompatibilityBrushKeys =
    [
        "ShellBackgroundBrush",
        "ShellTopBarBrush",
        "ShellPanelBrush",
        "ShellBorderBrush",
        "ShellPrimaryTextBrush",
        "ShellMutedTextBrush",
        "ShellTopChromeBrush",
        "ShellMenuBarBrush",
        "ShellToolbarBrush",
        "ShellTopChromeInnerDividerBrush",
        "ShellToolbarSeparatorBrush",
        "ShellToolbarBottomBorderBrush",
        "ShellAccentBrush",
        "ShellDockHeaderBrush",
        "ShellDockTabHoverBrush",
        "ShellDockTabSelectedBrush",
        "ShellDockSplitterBrush",
        "ShellDockSplitterHoverBrush"
    ];

    private static readonly string[] FrozenControlStyleKeys =
    [
        "UiButtonStyle",
        "UiAccentButtonStyle",
        "UiIconButtonStyle",
        "UiTextBoxStyle",
        "UiComboBoxStyle",
        "UiCheckBoxStyle",
        "UiRadioButtonStyle",
        "UiExpanderStyle",
        "UiToolTipStyle",
        "UiMenuStyle",
        "UiMenuItemStyle",
        "UiContextMenuStyle"
    ];

    private static readonly string[] FrozenCollectionStyleKeys =
    [
        "UiTabControlStyle",
        "UiTabItemStyle",
        "UiTreeViewStyle",
        "UiTreeViewItemStyle",
        "UiListViewStyle",
        "UiListBoxStyle",
        "UiDataGridStyle",
        "UiDataGridRowStyle",
        "UiDataGridCellStyle",
        "UiDataGridColumnHeaderStyle",
        "UiScrollBarStyle",
        "UiGridSplitterStyle",
        "UiProgressBarStyle"
    ];

    [Fact]
    public void VisualTokens_AreFirstInApplicationMergeOrderAndHaveNoShellThemeDuplicates()
    {
        string root = TestRepositoryRoot.Find();
        string appXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "App.xaml"));
        string tokenPath = Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IdeVisualTokens.xaml");
        string shellThemePath = Path.Combine(root, "RA2IniEditor.IDE", "Themes", "ShellTheme.xaml");

        Assert.Contains("<ResourceDictionary Source=\"Themes/IdeVisualTokens.xaml\" />", appXaml, StringComparison.Ordinal);
        Assert.Contains("<Style TargetType=\"{x:Type Window}\">", appXaml, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"FontFamily\" Value=\"{DynamicResource UiFontFamily}\" />", appXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("TextFormattingMode", appXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("TextRenderingMode", appXaml, StringComparison.Ordinal);
        Assert.True(
            appXaml.IndexOf("Themes/IdeVisualTokens.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/ShellTheme.xaml", StringComparison.Ordinal),
            "Semantic tokens must load before Shell compatibility styles.");

        HashSet<string> tokenKeys = ReadResourceKeys(tokenPath);
        HashSet<string> shellThemeKeys = ReadResourceKeys(shellThemePath);
        string[] duplicates = tokenKeys.Intersect(shellThemeKeys, StringComparer.Ordinal).Order().ToArray();

        Assert.Empty(duplicates);
        foreach (string key in FrozenBrushKeys)
            Assert.Contains(key, tokenKeys);
        foreach (string key in ShellCompatibilityBrushKeys)
            Assert.Contains(key, shellThemeKeys);
    }

    [Fact]
    public void CoreControlStyles_AreExplicitlyKeyedAndAdoptedOnlyByShellAndSearch()
    {
        string root = TestRepositoryRoot.Find();
        string ideRoot = Path.Combine(root, "RA2IniEditor.IDE");
        string appPath = Path.Combine(ideRoot, "App.xaml");
        string tokenPath = Path.Combine(ideRoot, "Themes", "IdeVisualTokens.xaml");
        string controlStylePath = Path.Combine(ideRoot, "Themes", "IdeControlStyles.xaml");
        string shellThemePath = Path.Combine(ideRoot, "Themes", "ShellTheme.xaml");
        string appXaml = File.ReadAllText(appPath);

        Assert.Contains("<ResourceDictionary Source=\"Themes/IdeControlStyles.xaml\" />", appXaml, StringComparison.Ordinal);
        Assert.True(
            appXaml.IndexOf("Themes/IdeVisualTokens.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/IdeControlStyles.xaml", StringComparison.Ordinal));
        Assert.True(
            appXaml.IndexOf("Themes/IdeControlStyles.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/ShellTheme.xaml", StringComparison.Ordinal));

        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        XDocument styleDocument = XDocument.Load(controlStylePath);
        // Application-level control styles remain keyed; scoped Style.Resources may use an
        // implicit child style so ContextMenu MenuItems do not style explicit Separators.
        XElement[] styles = styleDocument.Root!.Elements(presentation + "Style").ToArray();
        Assert.NotEmpty(styles);
        Assert.All(styles, style => Assert.NotNull(style.Attribute(xaml + "Key")));
        HashSet<string> tokenKeys = ReadResourceKeys(tokenPath);
        HashSet<string> controlStyleKeys = ReadResourceKeys(controlStylePath);
        HashSet<string> shellThemeKeys = ReadResourceKeys(shellThemePath);
        Assert.Empty(tokenKeys.Intersect(controlStyleKeys, StringComparer.Ordinal));
        Assert.Empty(controlStyleKeys.Intersect(shellThemeKeys, StringComparer.Ordinal));
        foreach (string key in FrozenControlStyleKeys)
            Assert.Contains(key, controlStyleKeys);

        string[] productionReferences = Directory
            .EnumerateFiles(ideRoot, "*.xaml", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !string.Equals(path, appPath, StringComparison.OrdinalIgnoreCase))
            .Where(path => !string.Equals(path, controlStylePath, StringComparison.OrdinalIgnoreCase))
            .Where(path => FrozenControlStyleKeys.Any(key => File.ReadAllText(path).Contains(key, StringComparison.Ordinal)))
            .ToArray();

        HashSet<string> allowedAdoptionFiles =
        [
            Path.Combine(ideRoot, "Themes", "IdeFieldRegistryStyles.xaml"),
            Path.Combine(ideRoot, "Themes", "IdeEditorAssistStyles.xaml"),
            Path.Combine(ideRoot, "Themes", "IdeWorkspaceStyles.xaml"),
            Path.Combine(ideRoot, "Themes", "ShellTheme.xaml"),
            Path.Combine(ideRoot, "Views", "FieldRegistryCenterWindow.xaml"),
            Path.Combine(ideRoot, "Views", "FieldRegistryHarvestPreviewWindow.xaml"),
            Path.Combine(ideRoot, "Views", "FieldLearningWizardWindow.xaml"),
            Path.Combine(ideRoot, "Views", "FieldRegistryManagerWindow.xaml"),
            Path.Combine(ideRoot, "Views", "FieldEditorWindow.xaml"),
            Path.Combine(ideRoot, "Views", "AllowedValuesEditorWindow.xaml"),
            Path.Combine(ideRoot, "Views", "RemoteSourcePresetEditorWindow.xaml"),
            Path.Combine(ideRoot, "Views", "IssuesToolWindow.xaml"),
            Path.Combine(ideRoot, "Views", "ShellWindow.xaml"),
            Path.Combine(ideRoot, "Views", "SearchToolView.xaml")
        ];
        Assert.NotEmpty(productionReferences);
        Assert.All(productionReferences, path => Assert.Contains(path, allowedAdoptionFiles));
    }

    [Fact]
    public void CollectionControlStyles_AreExplicitlyKeyedAndAdoptedOnlyByShellAndSearch()
    {
        string root = TestRepositoryRoot.Find();
        string ideRoot = Path.Combine(root, "RA2IniEditor.IDE");
        string appPath = Path.Combine(ideRoot, "App.xaml");
        string tokenPath = Path.Combine(ideRoot, "Themes", "IdeVisualTokens.xaml");
        string controlStylePath = Path.Combine(ideRoot, "Themes", "IdeControlStyles.xaml");
        string collectionStylePath = Path.Combine(ideRoot, "Themes", "IdeCollectionStyles.xaml");
        string shellThemePath = Path.Combine(ideRoot, "Themes", "ShellTheme.xaml");
        string appXaml = File.ReadAllText(appPath);

        Assert.Contains("<ResourceDictionary Source=\"Themes/IdeCollectionStyles.xaml\" />", appXaml, StringComparison.Ordinal);
        Assert.True(
            appXaml.IndexOf("Themes/IdeControlStyles.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/IdeCollectionStyles.xaml", StringComparison.Ordinal));
        Assert.True(
            appXaml.IndexOf("Themes/IdeCollectionStyles.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/ShellTheme.xaml", StringComparison.Ordinal));

        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        XDocument styleDocument = XDocument.Load(collectionStylePath);
        XElement[] styles = styleDocument.Descendants(presentation + "Style").ToArray();
        Assert.NotEmpty(styles);
        Assert.All(styles, style => Assert.NotNull(style.Attribute(xaml + "Key")));
        XElement dataGridStyle = Assert.Single(
            styles,
            style => string.Equals(style.Attribute(xaml + "Key")?.Value, "UiDataGridStyle", StringComparison.Ordinal));
        Assert.DoesNotContain(
            dataGridStyle.Elements(presentation + "Setter"),
            setter => string.Equals(setter.Attribute("Property")?.Value, "Template", StringComparison.Ordinal));

        HashSet<string> tokenKeys = ReadResourceKeys(tokenPath);
        HashSet<string> controlStyleKeys = ReadResourceKeys(controlStylePath);
        HashSet<string> collectionStyleKeys = ReadResourceKeys(collectionStylePath);
        HashSet<string> shellThemeKeys = ReadResourceKeys(shellThemePath);
        Assert.Empty(tokenKeys.Intersect(collectionStyleKeys, StringComparer.Ordinal));
        Assert.Empty(controlStyleKeys.Intersect(collectionStyleKeys, StringComparer.Ordinal));
        Assert.Empty(collectionStyleKeys.Intersect(shellThemeKeys, StringComparer.Ordinal));
        foreach (string key in FrozenCollectionStyleKeys)
            Assert.Contains(key, collectionStyleKeys);

        string[] productionReferences = Directory
            .EnumerateFiles(ideRoot, "*.xaml", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !string.Equals(path, appPath, StringComparison.OrdinalIgnoreCase))
            .Where(path => !string.Equals(path, collectionStylePath, StringComparison.OrdinalIgnoreCase))
            .Where(path => FrozenCollectionStyleKeys.Any(key => File.ReadAllText(path).Contains(key, StringComparison.Ordinal)))
            .ToArray();

        HashSet<string> allowedAdoptionFiles =
        [
            Path.Combine(ideRoot, "Themes", "IdeFieldRegistryStyles.xaml"),
            Path.Combine(ideRoot, "Themes", "IdeEditorAssistStyles.xaml"),
            Path.Combine(ideRoot, "Themes", "IdeWorkspaceStyles.xaml"),
            Path.Combine(ideRoot, "Themes", "ShellTheme.xaml"),
            Path.Combine(ideRoot, "Views", "FieldRegistryManagerWindow.xaml"),
            Path.Combine(ideRoot, "Views", "ShellWindow.xaml"),
            Path.Combine(ideRoot, "Views", "SearchToolView.xaml")
        ];
        Assert.NotEmpty(productionReferences);
        Assert.All(productionReferences, path => Assert.Contains(path, allowedAdoptionFiles));
    }

    [Fact]
    public void WorkspaceStyles_LoadAfterBaseCollectionsAndBeforeShellWithoutImplicitControlOverrides()
    {
        string root = TestRepositoryRoot.Find();
        string ideRoot = Path.Combine(root, "RA2IniEditor.IDE");
        string appXaml = File.ReadAllText(Path.Combine(ideRoot, "App.xaml"));
        string workspacePath = Path.Combine(ideRoot, "Themes", "IdeWorkspaceStyles.xaml");

        Assert.Contains("<ResourceDictionary Source=\"Themes/IdeWorkspaceStyles.xaml\" />", appXaml, StringComparison.Ordinal);
        Assert.True(
            appXaml.IndexOf("Themes/IdeCollectionStyles.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/IdeWorkspaceStyles.xaml", StringComparison.Ordinal));
        Assert.True(
            appXaml.IndexOf("Themes/IdeWorkspaceStyles.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/ShellTheme.xaml", StringComparison.Ordinal));

        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        XDocument document = XDocument.Load(workspacePath);
        XElement[] styles = document.Root!.Elements(presentation + "Style").ToArray();
        Assert.NotEmpty(styles);
        Assert.All(styles, style => Assert.NotNull(style.Attribute(xaml + "Key")));

        HashSet<string> workspaceKeys = ReadResourceKeys(workspacePath);
        foreach (string key in FrozenWorkspaceStyleKeys)
            Assert.Contains(key, workspaceKeys);

        Assert.Contains("EnableRowVirtualization", File.ReadAllText(workspacePath), StringComparison.Ordinal);
        Assert.Contains("VirtualizingStackPanel.VirtualizationMode", File.ReadAllText(workspacePath), StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryStyles_AreScopedKeyedAndPreserveCollectionVirtualization()
    {
        string root = TestRepositoryRoot.Find();
        string ideRoot = Path.Combine(root, "RA2IniEditor.IDE");
        string appXaml = File.ReadAllText(Path.Combine(ideRoot, "App.xaml"));
        string stylePath = Path.Combine(ideRoot, "Themes", "IdeFieldRegistryStyles.xaml");
        string styleText = File.ReadAllText(stylePath);

        Assert.Contains("<ResourceDictionary Source=\"Themes/IdeFieldRegistryStyles.xaml\" />", appXaml, StringComparison.Ordinal);
        Assert.True(
            appXaml.IndexOf("Themes/IdeWorkspaceStyles.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/IdeFieldRegistryStyles.xaml", StringComparison.Ordinal));
        Assert.True(
            appXaml.IndexOf("Themes/IdeFieldRegistryStyles.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/ShellTheme.xaml", StringComparison.Ordinal));

        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        XDocument document = XDocument.Load(stylePath);
        XElement[] styles = document.Root!.Elements(presentation + "Style").ToArray();
        Assert.NotEmpty(styles);
        Assert.All(styles, style => Assert.NotNull(style.Attribute(xaml + "Key")));

        Assert.DoesNotContain("Shell", styleText, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeSecondary", styleText, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource UiDataGridStyle}\"", styleText, StringComparison.Ordinal);
        Assert.Contains("EnableRowVirtualization", styleText, StringComparison.Ordinal);
        Assert.Contains("EnableColumnVirtualization", styleText, StringComparison.Ordinal);
        Assert.Contains("VirtualizingPanel.VirtualizationMode", styleText, StringComparison.Ordinal);
        Assert.Contains("ScrollViewer.CanContentScroll", styleText, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryR2Foundation_DefinesOnlyKeyedAdditiveStylesAndPreservesVirtualization()
    {
        string root = TestRepositoryRoot.Find();
        string stylePath = Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IdeFieldRegistryStyles.xaml");
        string styleText = File.ReadAllText(stylePath);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        XDocument document = XDocument.Load(stylePath);
        XElement[] r2Styles = document.Root!
            .Elements(presentation + "Style")
            .Where(style => style.Attribute(xaml + "Key")?.Value.StartsWith(
                "IdeFieldRegistryR2",
                StringComparison.Ordinal) == true)
            .ToArray();

        string[] requiredKeys =
        [
            "IdeFieldRegistryR2FlatCommandButtonStyle",
            "IdeFieldRegistryR2FlatAccentButtonStyle",
            "IdeFieldRegistryR2DataGridCellStyle",
            "IdeFieldRegistryR2DataGridColumnHeaderStyle",
            "IdeFieldRegistryR2DataGridStyle",
            "IdeFieldRegistryR2FlatSectionStyle",
            "IdeFieldRegistryR2InspectorSectionStyle",
            "IdeFieldRegistryR2EmptyStateTextStyle"
        ];

        Assert.Equal(requiredKeys.Length, r2Styles.Length);
        Assert.All(requiredKeys, key => Assert.Contains(
            r2Styles,
            style => string.Equals(style.Attribute(xaml + "Key")?.Value, key, StringComparison.Ordinal)));
        Assert.All(r2Styles, style => Assert.NotNull(style.Attribute(xaml + "Key")));
        Assert.Contains("BasedOn=\"{StaticResource IdeFieldRegistryDataGridStyle}\"", styleText, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource UiDataGridCellStyle}\"", styleText, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource UiDataGridColumnHeaderStyle}\"", styleText, StringComparison.Ordinal);
        Assert.Contains("EnableRowVirtualization", styleText, StringComparison.Ordinal);
        Assert.Contains("EnableColumnVirtualization", styleText, StringComparison.Ordinal);
        Assert.Contains("VirtualizingPanel.VirtualizationMode", styleText, StringComparison.Ordinal);
        Assert.Contains("ScrollViewer.CanContentScroll", styleText, StringComparison.Ordinal);
    }

    [Fact]
    public void EditorAssistStyles_AreScopedKeyedAndPreservePopupAndGridBoundaries()
    {
        string root = TestRepositoryRoot.Find();
        string ideRoot = Path.Combine(root, "RA2IniEditor.IDE");
        string appXaml = File.ReadAllText(Path.Combine(ideRoot, "App.xaml"));
        string stylePath = Path.Combine(ideRoot, "Themes", "IdeEditorAssistStyles.xaml");
        string styleText = File.ReadAllText(stylePath);

        Assert.Contains("<ResourceDictionary Source=\"Themes/IdeEditorAssistStyles.xaml\" />", appXaml, StringComparison.Ordinal);
        Assert.True(
            appXaml.IndexOf("Themes/IdeFieldRegistryStyles.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/IdeEditorAssistStyles.xaml", StringComparison.Ordinal));
        Assert.True(
            appXaml.IndexOf("Themes/IdeEditorAssistStyles.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/ShellTheme.xaml", StringComparison.Ordinal));

        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        XDocument document = XDocument.Load(stylePath);
        XElement[] styles = document.Root!.Elements(presentation + "Style").ToArray();
        Assert.NotEmpty(styles);
        Assert.All(styles, style => Assert.NotNull(style.Attribute(xaml + "Key")));

        Assert.DoesNotContain("Shell", styleText, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeSecondary", styleText, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeFieldRegistry", styleText, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource UiListBoxStyle}\"", styleText, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource UiDataGridStyle}\"", styleText, StringComparison.Ordinal);
        Assert.Contains("EnableRowVirtualization", styleText, StringComparison.Ordinal);
        Assert.Contains("EnableColumnVirtualization", styleText, StringComparison.Ordinal);
        Assert.Contains("VirtualizingPanel.VirtualizationMode", styleText, StringComparison.Ordinal);
        Assert.Contains("ScrollViewer.CanContentScroll", styleText, StringComparison.Ordinal);
        Assert.DoesNotMatch("#[0-9A-Fa-f]{6,8}", styleText);
    }

    [Fact]
    public void TransactionDialogs_UseAssistHierarchyWithoutChangingDecisionHandlers()
    {
        string root = TestRepositoryRoot.Find();
        string dirtyXaml = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "DirtyNavigation",
            "Ra2DirtyNavigationDialog.xaml"));
        string preflightXaml = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "SavePreflight",
            "SavePreflightConfirmationDialog.xaml"));

        Assert.Contains("AutomationProperties.AutomationId=\"DirtyNavigation.WarningBand\"", dirtyXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"SavePreflight.WarningBand\"", preflightXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistWarningBandStyle", dirtyXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistWarningBandStyle", preflightXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistAccentButtonStyle", dirtyXaml, StringComparison.Ordinal);
        Assert.Contains("IdeAssistAccentButtonStyle", preflightXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"SaveButton_OnClick\"", dirtyXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"DiscardButton_OnClick\"", dirtyXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"CancelButton_OnClick\"", dirtyXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"ContinueButton_OnClick\"", preflightXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"CancelButton_OnClick\"", preflightXaml, StringComparison.Ordinal);
        Assert.DoesNotMatch("#[0-9A-Fa-f]{6,8}", dirtyXaml + preflightXaml);
    }

    [Fact]
    public void FieldRegistryCenter_UsesApprovedThreePaneWorkspaceWithoutBehavioralRewiring()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryCenterWindow.xaml"));
        string code = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryCenterWindow.xaml.cs"));

        Assert.Contains("AutomationProperties.AutomationId=\"FieldRegistryCenter.Navigation\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldRegistryCenter.FieldList\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldRegistryCenter.Details\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldRegistryCenter.Details.EmptyState\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldRegistryCenter.Details.Inspector\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<ColumnDefinition Width=\"20*\" MinWidth=\"160\" MaxWidth=\"220\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("<ColumnDefinition Width=\"50*\" MinWidth=\"400\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("<ColumnDefinition Width=\"30*\" MinWidth=\"240\" MaxWidth=\"320\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"活跃字段包\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<DataGridTextColumn Width=\"80\" Binding=\"{Binding Scope}\" Header=\"范围\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("<DataGridTextColumn Width=\"56\" Binding=\"{Binding FieldCount}\" Header=\"字段\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryR2DataGridStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding FieldRows}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("MouseDoubleClick=\"FieldsGrid_OnMouseDoubleClick\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TextChanged=\"SearchTextBox_OnTextChanged\"", xaml, StringComparison.Ordinal);
        Assert.Contains("DataContext=\"{Binding SelectedItem.Details, ElementName=FieldsGrid}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding TrustDisplay}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding SourceDisplay}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding Examples}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding AllowedValues}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("public Ra2FieldDetailsViewModel Details { get; }", code, StringComparison.Ordinal);
        Assert.Contains("Ra2FieldDetailsViewModel.FromDefinition(definition, sectionKindValue)", code, StringComparison.Ordinal);
        Assert.Contains("条有效映射", code, StringComparison.Ordinal);
        Assert.Contains("WindowStyle=\"None\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"1280\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Height=\"720\"", xaml, StringComparison.Ordinal);
        Assert.Contains("CaptionHeight=\"52\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeSecondaryDataGridStyle", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("BorderThickness=\"1\" Background=\"{StaticResource ShellBackgroundBrush}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryManager_SeparatesStatusRollbackAndCleanupWithoutChangingWriteHandlers()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml"));

        Assert.Contains("Header=\"状态与来源\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"备份与回滚\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"概括清理\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryWriteBoundaryStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding RollbackManifests}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedItem=\"{Binding SelectedRollbackManifest, Mode=TwoWay}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"RollbackSelected\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"ApplyCleanupPlan\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsExpanded=\"{Binding HasCleanupPreviewDetails, Mode=OneWay}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldRegistryManager.RollbackDetails\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldRegistryManager.CleanupDetails\"", xaml, StringComparison.Ordinal);
        Assert.Contains("DataContext=\"{Binding SelectedRollbackManifest}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding StatusMessage}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding ManifestFilePath}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryR2DataGridStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<DataGridTextColumn Width=\"112\" Binding=\"{Binding TargetSectionKind}\" Header=\"目标 Section\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"1180\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Height=\"720\"", xaml, StringComparison.Ordinal);
        Assert.Contains("MinHeight=\"620\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeSecondaryDataGridStyle", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldImportPreview_UsesSourceReviewPlanHierarchyWithScopedAdvancedMigration()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));

        Assert.Contains("Text=\"来源 / Source\"", xaml, StringComparison.Ordinal);
        Assert.Contains("步骤 2：差异审阅；步骤 3：构建计划并确认应用", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryWriteBoundaryStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryR2FlatAccentButtonStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldImportPreview.MainFlowTabs\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldImportPreview.AdvancedDetailsExpander\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldImportPreview.WorkflowStepStrip\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldImportPreview.SourceArea\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldImportPreview.PlanArea\"", xaml, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource IdeFieldRegistryR2DataGridStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"1180\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryDataGridStyle}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("IdeSecondary", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"FetchRawText\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"CancelFetch\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"ApplyCurrentPlan\"", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedValue=\"{Binding SelectedTargetScope, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldLearningWizard_UsesModernSourceReviewAndExplicitWriteBoundary()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldLearningWizardWindow.xaml"));

        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryRootStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryCodeTextBoxStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryWriteBoundaryStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryAccentButtonStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemContainerStyle=\"{StaticResource IdeFieldRegistryTabItemStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding CurrentIniDraftRows}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"EditAllowedValues\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"BuildApplyPlan\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"ApplyCurrentPlan\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"1180\"", xaml, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource IdeFieldRegistryR2DataGridStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IconGeometry.Issue.Error", xaml, StringComparison.Ordinal);
        Assert.Contains("IconGeometry.Issue.Warning", xaml, StringComparison.Ordinal);
        Assert.Contains("<TextBlock Text=\"{Binding Severity}\" />", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldEditorAndAllowedValues_UseModernFormAndWriteBoundariesWithoutChangingDialogResults()
    {
        string root = TestRepositoryRoot.Find();
        string editorXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldEditorWindow.xaml"));
        string valuesXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "AllowedValuesEditorWindow.xaml"));

        Assert.Contains("AutomationProperties.AutomationId=\"FieldEditor.BasicSection\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MetadataColumn\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"DocumentationColumn\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldEditor.ActionFooter\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryR2FlatSectionStyle}\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource IdeFieldRegistryR2DataGridStyle}\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"960\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("Height=\"720\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("MinHeight=\"620\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"Width\" Value=\"30\" />", editorXaml, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"Height\" Value=\"28\" />", editorXaml, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"Padding\" Value=\"6\" />", editorXaml, StringComparison.Ordinal);
        Assert.Contains("<Path Width=\"14\" Height=\"14\" Data=\"{StaticResource IconGeometry.Action.Cancel}\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryCodeTextBoxStyle}\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryWriteBoundaryStyle}\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"BuildProjectPreview\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"ApplyProjectSave\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"ApplyGlobalSave\"", editorXaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryRootStyle}\"", valuesXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AllowedValuesEditor.Toolbar\"", valuesXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AllowedValuesEditor.RowCommands\"", valuesXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AllowedValuesEditor.NormalizationCommands\"", valuesXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AllowedValuesEditor.ValidationSummary\"", valuesXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AllowedValuesEditor.ActionFooter\"", valuesXaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryR2DataGridStyle}\"", valuesXaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"840\"", valuesXaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding Rows}\"", valuesXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"Accept\"", valuesXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"Cancel\"", valuesXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void AddPropertyAndAnnotationEditor_AdoptFieldRegistryVisualsAndPreserveCommands()
    {
        string root = TestRepositoryRoot.Find();
        string addXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldBrowser", "Ra2AddPropertyWindow.xaml"));
        string annotationXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldAnnotations", "Ra2FieldAnnotationEditorWindow.xaml"));

        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryR2DataGridStyle}\"", addXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AddProperty.Inspector\"", addXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AddProperty.ValueEntry\"", addXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AddProperty.ActionFooter\"", addXaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryR2FlatAccentButtonStyle}\"", addXaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"960\"", addXaml, StringComparison.Ordinal);
        Assert.Contains("Height=\"680\"", addXaml, StringComparison.Ordinal);
        Assert.Contains("PreviewKeyDown=\"SearchTextBox_OnPreviewKeyDown\"", addXaml, StringComparison.Ordinal);
        Assert.Contains("MouseDoubleClick=\"FieldsGrid_OnMouseDoubleClick\"", addXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldAnnotationEditor.Inspector\"", annotationXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldAnnotationEditor.Form\"", annotationXaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldAnnotationEditor.ActionFooter\"", annotationXaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryR2FlatSectionStyle}\"", annotationXaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryWriteBoundaryStyle}\"", annotationXaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"640\"", annotationXaml, StringComparison.Ordinal);
        Assert.Contains("Height=\"520\"", annotationXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"SaveAndCloseButton_OnClick\"", annotationXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"ApplyButton_OnClick\"", annotationXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void RemotePresetEditor_UsesCompactModernFormWithoutFetchOrApplyActions()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "RemoteSourcePresetEditorWindow.xaml"));

        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryRootStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryWriteBoundaryStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeFieldRegistryR2FlatAccentButtonStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"RemoteSourcePresetEditor.LocalPresetNotice\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"RemoteSourcePresetEditor.ActionFooter\"", xaml, StringComparison.Ordinal);
        Assert.Contains("只编辑本地远程来源预设", xaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"540\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Height=\"360\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding Url, UpdateSourceTrigger=PropertyChanged}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"Accept\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"Cancel\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("FetchRawText", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("ApplyCurrentPlan", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellAndSearch_AdoptModernStylesWithoutChangingBehaviorContracts()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string searchText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "SearchToolView.xaml"));
        string shellThemeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "ShellTheme.xaml"));
        string controlStylesText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IdeControlStyles.xaml"));
        string workspaceStylesText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IdeWorkspaceStyles.xaml"));

        Assert.Contains("SnapsToDevicePixels=\"True\"", shellText, StringComparison.Ordinal);
        Assert.Contains("UseLayoutRounding=\"True\"", shellText, StringComparison.Ordinal);
        Assert.Contains("SnapsToDevicePixels=\"True\"", searchText, StringComparison.Ordinal);
        Assert.Contains("UseLayoutRounding=\"True\"", searchText, StringComparison.Ordinal);
        Assert.DoesNotContain("#", shellText, StringComparison.Ordinal);
        Assert.DoesNotContain("#", searchText, StringComparison.Ordinal);

        string[] shellStyleKeys =
        [
            "IdeMainMenuStyle",
            "IdeIconCommandButtonStyle",
            "UiToolTipStyle",
            "UiContextMenuStyle",
            "UiTreeViewStyle",
            "UiTreeViewItemStyle",
            "IdeIssueDataGridStyle",
            "IdeWorkspaceCommandButtonStyle",
            "IdeAiWorkspaceRootStyle",
            "IdeAiAssistantMessageStyle",
            "IdeAiCopilotComposerSurfaceStyle",
            "IdeAiComposerInputStyle",
            "UiComboBoxStyle",
            "IdeAiComposerAdvancedButtonStyle",
            "IdeAiComposerSendButtonStyle",
            "IdeAiComposerCancelButtonStyle",
            "IdeAiCopilotAdvancedOptionsStyle",
            "IdeAiCopilotConfigurationStatusStyle",
            "IdeAiR2CompactContextTextStyle",
            "IdeAiR2SafetyTextStyle",
            "UiScrollBarStyle"
        ];
        foreach (string styleKey in shellStyleKeys)
            Assert.Contains(styleKey, shellText, StringComparison.Ordinal);

        string[] searchStyleKeys =
        [
            "IdeToolSearchTextBoxStyle",
            "IdeToolFilterChipStyle",
            "UiComboBoxStyle",
            "IdeWorkspaceCommandBarStyle",
            "IdeWorkspaceCommandButtonStyle",
            "UiAccentButtonStyle",
            "UiScrollBarStyle",
            "UiToolTipStyle"
        ];
        foreach (string styleKey in searchStyleKeys)
            Assert.Contains(styleKey, searchText, StringComparison.Ordinal);

        Assert.Contains("BasedOn=\"{StaticResource UiTabControlStyle}\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource UiTabItemStyle}\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource UiTextBoxStyle}\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource UiCheckBoxStyle}\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("BasedOn=\"{StaticResource UiListViewStyle}\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeMainMenuStyle}\"", shellText, StringComparison.Ordinal);
        Assert.DoesNotContain("ShellMainMenuItemStyle", shellText, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"IdeMainMenuItemStyle\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"Padding\" Value=\"5,3\" />", shellThemeText, StringComparison.Ordinal);
        Assert.DoesNotContain("<Setter Property=\"Padding\" Value=\"3,3\" />", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("ContentTemplateSelector=\"{TemplateBinding ItemTemplateSelector}\"", controlStylesText, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"IdeAiCopilotComposerSurfaceStyle\"", workspaceStylesText, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"IdeAiComposerSendButtonStyle\"", workspaceStylesText, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"IdeAiComposerCancelButtonStyle\"", workspaceStylesText, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"Visibility\" Value=\"Collapsed\" />", workspaceStylesText, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"IdeAiCopilotSafetyTextStyle\"", workspaceStylesText, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"TextWrapping\" Value=\"NoWrap\" />", workspaceStylesText, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"TextTrimming\" Value=\"CharacterEllipsis\" />", workspaceStylesText, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"IdeAiR2CompactContextTextStyle\"", workspaceStylesText, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"IdeAiR2SafetyTextStyle\"", workspaceStylesText, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"MinWidth\" Value=\"64\" />", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"Padding\" Value=\"9,0\" />", shellThemeText, StringComparison.Ordinal);
        Assert.DoesNotContain("DataContext.Title", shellText, StringComparison.Ordinal);
        Assert.DoesNotContain("MaxWidth=\"120\"", shellText, StringComparison.Ordinal);
        Assert.Contains("<Menu Grid.Column=\"1\"", shellText, StringComparison.Ordinal);
        Assert.Matches("x:Name=\"ShellTitleBarDragRegion\"\\s+Grid.Column=\"2\"", shellText);
        Assert.Contains("<StackPanel Grid.Column=\"3\" Orientation=\"Horizontal\">", shellText, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeWorkspaceCommandBarStyle}\"", shellText, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeIssueDataGridStyle}\"", shellText, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeOutputTextSurfaceStyle}\"", shellText, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeAiWorkspaceRootStyle}\"", shellText, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeAiAssistantMessageStyle}\"", shellText, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeAiCopilotComposerSurfaceStyle}\"", shellText, StringComparison.Ordinal);
        Assert.DoesNotContain("Style=\"{StaticResource IdeAiComposerStyle}\"", shellText, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource IdeShellStatusBarStyle}\"", shellText, StringComparison.Ordinal);
        Assert.Contains("ItemContainerStyle=\"{StaticResource ShellProjectExplorerTreeItemStyle}\"", shellText, StringComparison.Ordinal);
        Assert.Contains("Value=\"{Binding IsExpanded, Mode=TwoWay}\"", shellText, StringComparison.Ordinal);
        Assert.Contains("SelectedItemChanged=\"ProjectExplorerTreeView_OnSelectedItemChanged\"", shellText, StringComparison.Ordinal);
        Assert.Contains("MouseDoubleClick=\"BottomIssuesGrid_OnMouseDoubleClick\"", shellText, StringComparison.Ordinal);
        Assert.Contains("PreviewKeyDown=\"SourceTextEditor_OnPreviewKeyDown\"", shellText, StringComparison.Ordinal);
        Assert.Contains("TextChanged=\"SourceTextEditor_OnTextChanged\"", shellText, StringComparison.Ordinal);
        Assert.Contains("ContentId=\"Document.Source\"", shellText, StringComparison.Ordinal);
        Assert.Contains("DockWidth=\"300\"", shellText, StringComparison.Ordinal);
        Assert.Contains("DockHeight=\"260\"", shellText, StringComparison.Ordinal);
        Assert.Contains("WindowStyle=\"None\"", shellText, StringComparison.Ordinal);
        Assert.Contains("AllowsTransparency=\"False\"", shellText, StringComparison.Ordinal);
        Assert.Contains("CaptionHeight=\"30\"", shellText, StringComparison.Ordinal);
        Assert.Contains("<RowDefinition Height=\"30\" />", shellText, StringComparison.Ordinal);
        Assert.Contains("<RowDefinition Height=\"32\" />", shellText, StringComparison.Ordinal);
        Assert.Contains("<RowDefinition Height=\"24\" />", shellText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.TitleBar\"", shellText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.TitleBar.SystemMenuButton\"", shellText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.TitleBar.DragRegion\"", shellText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.TitleBar.MinimizeButton\"", shellText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.TitleBar.MaximizeRestoreButton\"", shellText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.TitleBar.CloseButton\"", shellText, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"IdeDockFloatingWindowStyle\"", shellThemeText, StringComparison.Ordinal);
        Assert.DoesNotContain("Property=\"AllowsTransparency\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PART_FloatingContentHost\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PART_FloatingMinimizeButton\"", shellThemeText, StringComparison.Ordinal);
        Assert.DoesNotContain("Command=\"{x:Static SystemCommands.MinimizeWindowCommand}\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.Dock.FloatingHost\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.Dock.FloatingHost.MinimizeButton\"", shellThemeText, StringComparison.Ordinal);
        Assert.DoesNotContain("AutomationProperties.AutomationId=\"Shell.Dock.FloatingHost.MaximizeRestoreButton\"", shellThemeText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Shell.Dock.FloatingHost.CloseButton\"", shellThemeText, StringComparison.Ordinal);

        Assert.Contains("AutomationProperties.AutomationId=\"Search.QueryTextBox\"", searchText, StringComparison.Ordinal);
        Assert.DoesNotContain("IsReadOnly=\"True\"", searchText, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding Query, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}\"", searchText, StringComparison.Ordinal);
        Assert.Contains("<WrapPanel Grid.Row=\"2\"", searchText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.ScopeComboBox\"", searchText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.FilePatternComboBox\"", searchText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.ResultsList\"", searchText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.StatusText\"", searchText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.ReplaceTextBox\"", searchText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.PreviewReplaceAllButton\"", searchText, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Search.ApplyReplaceAllButton\"", searchText, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"查找\"", searchText, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding Results}\"", searchText, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding StatusText}\"", searchText, StringComparison.Ordinal);
    }

    [Fact]
    public void VisualTokens_ResolveWithFrozenTypesAndValuesThroughStaResourceLoad()
    {
        RunInSta(() =>
        {
            Application application = Application.Current ?? new Application();
            ResourceDictionary previousResources = application.Resources;
            ResourceDictionary applicationResources = new();
            application.Resources = applicationResources;

            try
            {
                applicationResources.MergedDictionaries.Add(LoadDictionary("Themes/IdeVisualTokens.xaml"));
                applicationResources.MergedDictionaries.Add(LoadDictionary("Themes/IdeControlStyles.xaml"));
                applicationResources.MergedDictionaries.Add(LoadDictionary("Themes/IdeCollectionStyles.xaml"));
                applicationResources.MergedDictionaries.Add(LoadDictionary("Themes/IdeWorkspaceStyles.xaml"));
                applicationResources.MergedDictionaries.Add(LoadDictionary("Themes/ShellTheme.xaml"));

                ComboBox comboBox = new()
                {
                    DisplayMemberPath = "Value",
                    ItemsSource = new[] { new KeyValuePair<int, string>(1, "DeepSeek V4 Flash") },
                    SelectedIndex = 0,
                    Style = Assert.IsType<Style>(applicationResources["UiComboBoxStyle"])
                };

                Assert.True(comboBox.ApplyTemplate());
                ContentPresenter presenter = Assert.IsType<ContentPresenter>(
                    comboBox.Template.FindName("SelectionPresenter", comboBox));
                Assert.NotNull(comboBox.ItemTemplateSelector);
                Assert.Same(comboBox.ItemTemplateSelector, presenter.ContentTemplateSelector);

                Dictionary<string, string> expectedBrushColors = new(StringComparer.Ordinal)
                {
                ["UiCanvasBrush"] = "#F4F6F8",
                ["UiSurfaceBrush"] = "#FFFFFF",
                ["UiSurfaceSubtleBrush"] = "#F5F7FA",
                ["UiSurfaceHoverBrush"] = "#EAF2FB",
                ["UiSurfacePressedBrush"] = "#DCE9F7",
                ["UiBorderBrush"] = "#D7DCE2",
                ["UiDividerBrush"] = "#E3E8EF",
                ["UiTextPrimaryBrush"] = "#202733",
                ["UiTextSecondaryBrush"] = "#697386",
                ["UiTextDisabledBrush"] = "#98A2B3",
                ["UiAccentBrush"] = "#0F6CBD",
                ["UiAccentHoverBrush"] = "#115EA3",
                ["UiAccentPressedBrush"] = "#0C3B5E",
                ["UiAccentSoftBrush"] = "#EAF4FF",
                ["UiFocusBrush"] = "#0F6CBD",
                ["UiDangerBrush"] = "#B42318",
                ["UiWarningBrush"] = "#B54708",
                ["UiSuccessBrush"] = "#107C10",
                ["UiSelectionBrush"] = "#DCEEFF",
                ["UiSelectionInactiveBrush"] = "#EEF2F7",
                ["ShellBackgroundBrush"] = "#F4F6F8",
                ["ShellTopBarBrush"] = "#FFFFFF",
                ["ShellPanelBrush"] = "#FFFFFF",
                ["ShellBorderBrush"] = "#D7DCE2",
                ["ShellPrimaryTextBrush"] = "#202733",
                ["ShellMutedTextBrush"] = "#697386",
                ["ShellTopChromeBrush"] = "#F5F7FA",
                ["ShellMenuBarBrush"] = "#F5F7FA",
                ["ShellToolbarBrush"] = "#F5F7FA",
                ["ShellTopChromeInnerDividerBrush"] = "#EEF2F6",
                ["ShellToolbarSeparatorBrush"] = "#E3E8EF",
                ["ShellToolbarBottomBorderBrush"] = "#D5DCE5",
                ["ShellAccentBrush"] = "#0F6CBD",
                ["ShellDockHeaderBrush"] = "#F7F9FC",
                ["ShellDockTabHoverBrush"] = "#EAF2FB",
                ["ShellDockTabSelectedBrush"] = "#FFFFFF",
                ["ShellDockSplitterBrush"] = "#E2E7ED",
                ["ShellDockSplitterHoverBrush"] = "#A9C7E8"
                };

                foreach ((string key, string colorText) in expectedBrushColors)
                {
                    SolidColorBrush brush = Assert.IsType<SolidColorBrush>(applicationResources[key]);
                    Assert.Equal((Color)ColorConverter.ConvertFromString(colorText), brush.Color);
                }

                AssertFrozenDouble(applicationResources, "UiTitleMenuHeight", 30);
                AssertFrozenDouble(applicationResources, "UiControlHeightCompact", 28);
                AssertFrozenDouble(applicationResources, "UiControlHeightDefault", 32);
                AssertFrozenDouble(applicationResources, "UiCommandRowHeight", 32);
                AssertFrozenDouble(applicationResources, "UiDocumentTabHeight", 30);
                AssertFrozenDouble(applicationResources, "UiToolHeaderHeight", 30);
                AssertFrozenDouble(applicationResources, "UiStatusBarHeight", 24);
                AssertFrozenDouble(applicationResources, "UiSplitterThickness", 4);
                AssertFrozenDouble(applicationResources, "UiSpace1", 4);
                AssertFrozenDouble(applicationResources, "UiSpace2", 8);
                AssertFrozenDouble(applicationResources, "UiSpace3", 12);
                AssertFrozenDouble(applicationResources, "UiSpace4", 16);
                AssertFrozenDouble(applicationResources, "UiIconSmall", 16);
                AssertFrozenDouble(applicationResources, "UiIconMedium", 20);
                AssertFrozenDouble(applicationResources, "UiTreeRowHeight", 24);
                AssertFrozenDouble(applicationResources, "UiGridRowHeight", 26);

                Assert.Equal(new CornerRadius(3), Assert.IsType<CornerRadius>(applicationResources["UiCornerSmall"]));
                Assert.Equal(new CornerRadius(6), Assert.IsType<CornerRadius>(applicationResources["UiCornerMedium"]));
                Assert.Equal(new Thickness(1), Assert.IsType<Thickness>(applicationResources["UiBorderThickness"]));
                Assert.Equal(new Thickness(2), Assert.IsType<Thickness>(applicationResources["UiFocusThickness"]));
                Assert.Equal(
                    "Segoe UI Variable Text, Microsoft YaHei UI, Segoe UI",
                    Assert.IsType<FontFamily>(applicationResources["UiFontFamily"]).Source);
                Assert.Equal("Consolas", Assert.IsType<FontFamily>(applicationResources["UiCodeFontFamily"]).Source);
                AssertFrozenDouble(applicationResources, "UiFontSizeDefault", 12);
                AssertFrozenDouble(applicationResources, "UiFontSizeMetadata", 11);
                AssertFrozenDouble(applicationResources, "UiFontSizeToolTitle", 12);
                AssertFrozenDouble(applicationResources, "UiFontSizeSectionTitle", 13);

                AssertCoreControlStyles(applicationResources);
                AssertCollectionControlStyles(applicationResources);
                string[] runtimeWorkspaceStyleKeys =
                [
                    "IdeWorkspaceRootStyle",
                    "IdeWorkspaceCommandBarStyle",
                    "IdeWorkspaceStatusBarStyle",
                    "IdeAiWorkspaceRootStyle",
                    "IdeAiContextStripStyle",
                    "IdeAiR2CompactContextTextStyle",
                    "IdeAiChatViewportStyle",
                    "IdeAiAssistantMessageStyle",
                    "IdeAiComposerStyle",
                    "IdeAiCopilotComposerSurfaceStyle",
                    "IdeAiComposerAdvancedButtonStyle",
                    "IdeAiComposerSendButtonStyle",
                    "IdeAiComposerCancelButtonStyle",
                    "IdeAiCopilotAdvancedOptionsStyle",
                    "IdeAiCopilotConfigurationStatusStyle",
                    "IdeAiCopilotSafetyTextStyle",
                    "IdeAiR2SafetyTextStyle",
                    "IdeAiMarkdownTableStyle",
                    "IdeAiMarkdownTableCellStyle"
                ];
                foreach (string key in runtimeWorkspaceStyleKeys)
                {
                    try
                    {
                        Assert.IsType<Style>(applicationResources[key]);
                    }
                    catch (Exception exception)
                    {
                        Assert.Fail($"Workspace resource '{key}' failed to resolve: {exception}");
                    }
                }
            }
            finally
            {
                application.Resources = previousResources;
            }
        });
    }

    private static ResourceDictionary LoadDictionary(string relativePath)
    {
        return new ResourceDictionary
        {
            Source = new Uri($"/RA2IniEditor.IDE;component/{relativePath}", UriKind.Relative)
        };
    }

    private static HashSet<string> ReadResourceKeys(string path)
    {
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        return XDocument.Load(path)
            .Descendants()
            .Attributes(xaml + "Key")
            .Select(attribute => attribute.Value)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static void AssertFrozenDouble(ResourceDictionary resources, string key, double expected)
    {
        Assert.Equal(expected, Assert.IsType<double>(resources[key]));
    }

    private static void AssertCoreControlStyles(ResourceDictionary resources)
    {
        foreach (string key in FrozenControlStyleKeys)
            Assert.IsType<Style>(resources[key]);

        RoutedCommand command = new();
        int commandExecutions = 0;
        Window host = new()
        {
            Width = 640,
            Height = 480,
            Left = -10000,
            Top = -10000,
            Opacity = 0.01,
            ShowActivated = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            WindowStyle = WindowStyle.None
        };
        StackPanel panel = new();
        host.Content = panel;
        host.CommandBindings.Add(new CommandBinding(
            command,
            (_, _) => commandExecutions++,
            (_, args) => args.CanExecute = true));

        Button button = new()
        {
            Content = "_Run",
            Command = command,
            Style = Assert.IsType<Style>(resources["UiButtonStyle"])
        };
        ToggleButton toggleButton = new()
        {
            Content = "Toggle",
            IsThreeState = true,
            Style = Assert.IsType<Style>(resources["UiButtonStyle"])
        };
        TextBox textBox = new()
        {
            AcceptsReturn = true,
            Text = "Alpha",
            TextWrapping = TextWrapping.Wrap,
            Style = Assert.IsType<Style>(resources["UiTextBoxStyle"])
        };
        ComboBox comboBox = new()
        {
            IsEditable = true,
            Style = Assert.IsType<Style>(resources["UiComboBoxStyle"])
        };
        comboBox.Items.Add("One");
        comboBox.Items.Add("Two");
        comboBox.SelectedIndex = 0;
        ComboBox nonEditableComboBox = new()
        {
            Height = 28,
            Width = 240,
            Style = Assert.IsType<Style>(resources["UiComboBoxStyle"])
        };
        nonEditableComboBox.Items.Add("DeepSeek V4 Flash");
        nonEditableComboBox.Items.Add("DeepSeek V4 Pro");
        nonEditableComboBox.SelectedIndex = 0;

        CheckBox checkBox = new()
        {
            Content = "_Three state",
            IsThreeState = true,
            Style = Assert.IsType<Style>(resources["UiCheckBoxStyle"])
        };
        RadioButton firstRadio = new()
        {
            Content = "_First",
            GroupName = "M1BGroup",
            Style = Assert.IsType<Style>(resources["UiRadioButtonStyle"])
        };
        RadioButton secondRadio = new()
        {
            Content = "_Second",
            GroupName = "M1BGroup",
            Style = Assert.IsType<Style>(resources["UiRadioButtonStyle"])
        };
        Expander expander = new()
        {
            Header = "_Details",
            Content = new TextBlock { Text = "Expanded content" },
            Style = Assert.IsType<Style>(resources["UiExpanderStyle"])
        };
        MenuItem topLevelMenuItem = new()
        {
            Header = "_File",
            Style = Assert.IsType<Style>(resources["UiMenuItemStyle"])
        };
        MenuItem commandMenuItem = new()
        {
            Header = "_Run",
            IsCheckable = true,
            Command = command,
            Style = Assert.IsType<Style>(resources["UiMenuItemStyle"])
        };
        topLevelMenuItem.Items.Add(commandMenuItem);
        Menu menu = new()
        {
            Style = Assert.IsType<Style>(resources["UiMenuStyle"])
        };
            menu.Items.Add(topLevelMenuItem);

        panel.Children.Add(button);
        panel.Children.Add(toggleButton);
        panel.Children.Add(textBox);
        panel.Children.Add(comboBox);
        panel.Children.Add(nonEditableComboBox);
        panel.Children.Add(checkBox);
        panel.Children.Add(firstRadio);
        panel.Children.Add(secondRadio);
        panel.Children.Add(expander);
        panel.Children.Add(menu);

        ContextMenu contextMenu = new()
        {
            PlacementTarget = button,
            Style = Assert.IsType<Style>(resources["UiContextMenuStyle"])
        };
        MenuItem contextCommandMenuItem = new()
        {
            Header = "_Context command",
            Command = command
        };
        contextMenu.Items.Add(contextCommandMenuItem);
        contextMenu.Items.Add(new Separator());
        ToolTip toolTip = new()
        {
            Content = "Bounded tooltip",
            PlacementTarget = button,
            Style = Assert.IsType<Style>(resources["UiToolTipStyle"])
        };

        try
        {
            host.Show();
            host.UpdateLayout();
            CommandManager.InvalidateRequerySuggested();
            FlushDispatcher();

            topLevelMenuItem.ApplyTemplate();
            Assert.Equal(new GridLength(0), Assert.IsType<ColumnDefinition>(
                topLevelMenuItem.Template.FindName("MenuIconColumn", topLevelMenuItem)).Width);
            Assert.Equal(new GridLength(0), Assert.IsType<ColumnDefinition>(
                topLevelMenuItem.Template.FindName("MenuGestureColumn", topLevelMenuItem)).Width);
            Assert.Equal(new GridLength(0), Assert.IsType<ColumnDefinition>(
                topLevelMenuItem.Template.FindName("MenuArrowColumn", topLevelMenuItem)).Width);

            commandMenuItem.ApplyTemplate();
            Assert.Equal(new GridLength(18), Assert.IsType<ColumnDefinition>(
                commandMenuItem.Template.FindName("MenuIconColumn", commandMenuItem)).Width);
            Assert.Equal(new GridLength(14), Assert.IsType<ColumnDefinition>(
                commandMenuItem.Template.FindName("MenuArrowColumn", commandMenuItem)).Width);

            button.ApplyTemplate();
            Assert.NotNull(button.Template.FindName("ButtonContentPresenter", button));
            Assert.True(button.IsEnabled);
            IInvokeProvider buttonInvoke = Assert.IsAssignableFrom<IInvokeProvider>(
                new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke));
            buttonInvoke.Invoke();
            FlushDispatcher();
            Assert.Equal(1, commandExecutions);
            button.IsEnabled = false;
            Assert.False(button.IsEnabled);

            toggleButton.ApplyTemplate();
            IToggleProvider toggleProvider = Assert.IsAssignableFrom<IToggleProvider>(
                new ToggleButtonAutomationPeer(toggleButton).GetPattern(PatternInterface.Toggle));
            toggleProvider.Toggle();
            Assert.True(toggleButton.IsChecked);
            toggleProvider.Toggle();
            Assert.Null(toggleButton.IsChecked);
            toggleProvider.Toggle();
            Assert.False(toggleButton.IsChecked);

            textBox.ApplyTemplate();
            Assert.NotNull(textBox.Template.FindName("PART_ContentHost", textBox));
            Assert.True(InputMethod.GetIsInputMethodEnabled(textBox));
            textBox.Select(1, 3);
            Assert.Equal("lph", textBox.SelectedText);
            textBox.SelectedText = "eta";
            Assert.True(textBox.CanUndo);
            textBox.Undo();
            Assert.Equal("Alpha", textBox.Text);
            textBox.IsReadOnly = true;
            Assert.True(textBox.IsReadOnly);
            Assert.True(textBox.AcceptsReturn);
            Assert.Equal(TextWrapping.Wrap, textBox.TextWrapping);

            comboBox.ApplyTemplate();
            Popup comboPopup = Assert.IsType<Popup>(comboBox.Template.FindName("PART_Popup", comboBox));
            TextBox editableTextBox = Assert.IsType<TextBox>(comboBox.Template.FindName("PART_EditableTextBox", comboBox));
            ToggleButton editableDropDownToggle = Assert.IsType<ToggleButton>(
                comboBox.Template.FindName("DropDownToggle", comboBox));
            Assert.Equal(320, comboBox.MaxDropDownHeight);
            Assert.True(comboBox.IsEditable);
            Assert.Equal(Visibility.Visible, editableTextBox.Visibility);
            AssertInputHitWithin(comboBox, editableTextBox, new Point(20, comboBox.ActualHeight / 2));
            AssertInputHitWithin(
                comboBox,
                editableDropDownToggle,
                new Point(comboBox.ActualWidth - 2, comboBox.ActualHeight / 2));
            editableTextBox.Text = "Editable";
            Assert.Equal("Editable", editableTextBox.Text);
            comboBox.IsDropDownOpen = true;
            FlushDispatcher();
            Assert.True(comboPopup.IsOpen);
            comboBox.SelectedIndex = 1;
            comboBox.IsDropDownOpen = false;
            Assert.Equal("Two", comboBox.SelectedItem);

            nonEditableComboBox.ApplyTemplate();
            ToggleButton fullSurfaceDropDownToggle = Assert.IsType<ToggleButton>(
                nonEditableComboBox.Template.FindName("DropDownToggle", nonEditableComboBox));
            ContentPresenter nonEditableSelectionPresenter = Assert.IsType<ContentPresenter>(
                nonEditableComboBox.Template.FindName("SelectionPresenter", nonEditableComboBox));
            TextBox hiddenEditableTextBox = Assert.IsType<TextBox>(
                nonEditableComboBox.Template.FindName("PART_EditableTextBox", nonEditableComboBox));
            Assert.False(nonEditableSelectionPresenter.IsHitTestVisible);
            Assert.Equal(Visibility.Hidden, hiddenEditableTextBox.Visibility);
            Assert.Equal(nonEditableComboBox.ActualWidth, fullSurfaceDropDownToggle.ActualWidth, 3);
            Assert.Equal(nonEditableComboBox.ActualHeight, fullSurfaceDropDownToggle.ActualHeight, 3);
            AssertInputHitWithin(nonEditableComboBox, fullSurfaceDropDownToggle, new Point(2, 2));
            AssertInputHitWithin(nonEditableComboBox, fullSurfaceDropDownToggle, new Point(40, 14));
            AssertInputHitWithin(nonEditableComboBox, fullSurfaceDropDownToggle, new Point(238, 26));
            fullSurfaceDropDownToggle.IsChecked = true;
            FlushDispatcher();
            Assert.True(nonEditableComboBox.IsDropDownOpen);
            fullSurfaceDropDownToggle.IsChecked = false;
            FlushDispatcher();
            Assert.False(nonEditableComboBox.IsDropDownOpen);

            checkBox.ApplyTemplate();
            Assert.NotNull(checkBox.Template.FindName("CheckContentPresenter", checkBox));
            IToggleProvider checkProvider = Assert.IsAssignableFrom<IToggleProvider>(
                new CheckBoxAutomationPeer(checkBox).GetPattern(PatternInterface.Toggle));
            checkProvider.Toggle();
            Assert.True(checkBox.IsChecked);
            checkProvider.Toggle();
            Assert.Null(checkBox.IsChecked);
            checkProvider.Toggle();
            Assert.False(checkBox.IsChecked);

            firstRadio.ApplyTemplate();
            secondRadio.ApplyTemplate();
            Assert.NotNull(firstRadio.Template.FindName("RadioContentPresenter", firstRadio));
            firstRadio.IsChecked = true;
            secondRadio.IsChecked = true;
            Assert.False(firstRadio.IsChecked);
            Assert.True(secondRadio.IsChecked);

            expander.ApplyTemplate();
            ToggleButton headerSite = Assert.IsType<ToggleButton>(expander.Template.FindName("HeaderSite", expander));
            ContentPresenter expandSite = Assert.IsType<ContentPresenter>(expander.Template.FindName("ExpandSite", expander));
            Assert.Equal(Visibility.Collapsed, expandSite.Visibility);
            headerSite.IsChecked = true;
            FlushDispatcher();
            Assert.True(expander.IsExpanded);
            Assert.Equal(Visibility.Visible, expandSite.Visibility);

            menu.ApplyTemplate();
            Assert.NotNull(menu.Template.FindName("MenuItemsPresenter", menu));
            topLevelMenuItem.ApplyTemplate();
            commandMenuItem.ApplyTemplate();
            Assert.NotNull(topLevelMenuItem.Template.FindName("PART_Popup", topLevelMenuItem));
            ContentPresenter menuHeader = Assert.IsType<ContentPresenter>(
                topLevelMenuItem.Template.FindName("MenuHeaderPresenter", topLevelMenuItem));
            Assert.True(menuHeader.RecognizesAccessKey);
            commandMenuItem.IsChecked = true;
            Assert.True(commandMenuItem.IsChecked);
            IInvokeProvider menuInvoke = Assert.IsAssignableFrom<IInvokeProvider>(
                new MenuItemAutomationPeer(commandMenuItem).GetPattern(PatternInterface.Invoke));
            menuInvoke.Invoke();
            FlushDispatcher();
            Assert.Equal(2, commandExecutions);

            contextMenu.ApplyTemplate();
            Assert.NotNull(contextMenu.Template.FindName("ContextMenuItemsPresenter", contextMenu));
            contextMenu.IsOpen = true;
            FlushDispatcher();
            Assert.True(contextMenu.IsOpen);
            Assert.Same(resources["UiMenuItemStyle"], contextCommandMenuItem.Style.BasedOn);
            toolTip.ApplyTemplate();
            Assert.NotNull(toolTip.Template.FindName("ToolTipContentPresenter", toolTip));
            Assert.False(toolTip.Focusable);
        }
        finally
        {
            comboBox.IsDropDownOpen = false;
            contextMenu.IsOpen = false;
            toolTip.IsOpen = false;
            host.Close();
        }
    }

    private static void AssertCollectionControlStyles(ResourceDictionary resources)
    {
        foreach (string key in FrozenCollectionStyleKeys)
            Assert.IsType<Style>(resources[key]);

        Window host = new()
        {
            Width = 900,
            Height = 760,
            Left = -10000,
            Top = -10000,
            Opacity = 0.01,
            ShowActivated = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            WindowStyle = WindowStyle.None
        };
        StackPanel panel = new();
        host.Content = new ScrollViewer { Content = panel };

        TabControl tabs = new()
        {
            Height = 100,
            Style = Assert.IsType<Style>(resources["UiTabControlStyle"])
        };
        TabItem firstTab = new()
        {
            Header = "First",
            Content = "First content",
            Style = Assert.IsType<Style>(resources["UiTabItemStyle"])
        };
        TabItem secondTab = new()
        {
            Header = "Second",
            Content = "Second content",
            Style = Assert.IsType<Style>(resources["UiTabItemStyle"])
        };
        tabs.Items.Add(firstTab);
        tabs.Items.Add(secondTab);

        TreeView tree = new()
        {
            Height = 100,
            Style = Assert.IsType<Style>(resources["UiTreeViewStyle"])
        };
        TreeViewItem treeParent = new()
        {
            Header = "Parent",
            Style = Assert.IsType<Style>(resources["UiTreeViewItemStyle"])
        };
        treeParent.Items.Add(new TreeViewItem
        {
            Header = "Child",
            Style = Assert.IsType<Style>(resources["UiTreeViewItemStyle"])
        });
        tree.Items.Add(treeParent);

        ListBox listBox = new()
        {
            Height = 72,
            Style = Assert.IsType<Style>(resources["UiListBoxStyle"])
        };
        listBox.Items.Add("Alpha");
        listBox.Items.Add("Beta");

        ListView listView = new()
        {
            Height = 72,
            Style = Assert.IsType<Style>(resources["UiListViewStyle"])
        };
        listView.Items.Add("Alpha");
        listView.Items.Add("Beta");

        DataGrid dataGrid = new()
        {
            Height = 110,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            Background = Assert.IsType<SolidColorBrush>(resources["UiSurfaceBrush"]),
            BorderBrush = Assert.IsType<SolidColorBrush>(resources["UiBorderBrush"]),
            BorderThickness = Assert.IsType<Thickness>(resources["UiBorderThickness"]),
            Foreground = Assert.IsType<SolidColorBrush>(resources["UiTextPrimaryBrush"]),
            FontFamily = Assert.IsType<FontFamily>(resources["UiFontFamily"]),
            FontSize = Assert.IsType<double>(resources["UiFontSizeDefault"]),
            HorizontalGridLinesBrush = Assert.IsType<SolidColorBrush>(resources["UiDividerBrush"]),
            VerticalGridLinesBrush = Assert.IsType<SolidColorBrush>(resources["UiDividerBrush"]),
            Style = Assert.IsType<Style>(resources["UiDataGridStyle"])
        };
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Header",
            Binding = new System.Windows.Data.Binding("Key")
        });
        DataGridColumnHeader header = new()
        {
            Content = "Header",
            Style = Assert.IsType<Style>(resources["UiDataGridColumnHeaderStyle"])
        };

        ScrollBar verticalScrollBar = new()
        {
            Height = 90,
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            SmallChange = 5,
            Orientation = Orientation.Vertical,
            Style = Assert.IsType<Style>(resources["UiScrollBarStyle"])
        };
        ScrollBar horizontalScrollBar = new()
        {
            Width = 180,
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            Orientation = Orientation.Horizontal,
            Style = Assert.IsType<Style>(resources["UiScrollBarStyle"])
        };

        Grid splitterGrid = new() { Height = 40, Width = 300 };
        splitterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
        splitterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
        splitterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(156) });
        GridSplitter splitter = new()
        {
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
            ResizeDirection = GridResizeDirection.Columns,
            ShowsPreview = true,
            Style = Assert.IsType<Style>(resources["UiGridSplitterStyle"])
        };
        Grid.SetColumn(splitter, 1);
        splitterGrid.Children.Add(splitter);

        ProgressBar progress = new()
        {
            Width = 200,
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            Style = Assert.IsType<Style>(resources["UiProgressBarStyle"])
        };

        panel.Children.Add(tabs);
        panel.Children.Add(tree);
        panel.Children.Add(listBox);
        panel.Children.Add(listView);
        panel.Children.Add(dataGrid);
        panel.Children.Add(header);
        panel.Children.Add(verticalScrollBar);
        panel.Children.Add(horizontalScrollBar);
        panel.Children.Add(splitterGrid);
        panel.Children.Add(progress);

        try
        {
            host.Show();
            host.UpdateLayout();
            FlushDispatcher();

            firstTab.ApplyTemplate();
            tabs.ApplyTemplate();
            Assert.NotNull(tabs.Template.FindName("TabHeaderItemsPresenter", tabs));
            ContentPresenter selectedContentHost = Assert.IsType<ContentPresenter>(
                tabs.Template.FindName("PART_SelectedContentHost", tabs));
            tabs.SelectedIndex = 1;
            FlushDispatcher();
            Assert.Same(secondTab, tabs.SelectedItem);
            Assert.Equal("Second content", selectedContentHost.Content);

            tree.ApplyTemplate();
            treeParent.ApplyTemplate();
            Assert.NotNull(tree.Template.FindName("TreeScrollViewer", tree));
            Assert.NotNull(tree.Template.FindName("TreeItemsPresenter", tree));
            ItemsPresenter treeItemsHost = Assert.IsType<ItemsPresenter>(
                treeParent.Template.FindName("TreeItemsHost", treeParent));
            treeParent.IsExpanded = true;
            treeParent.IsSelected = true;
            FlushDispatcher();
            Assert.Equal(Visibility.Visible, treeItemsHost.Visibility);
            Assert.True(treeParent.IsSelected);
            Assert.True(VirtualizingPanel.GetIsVirtualizing(tree));
            Assert.Equal(VirtualizationMode.Recycling, VirtualizingPanel.GetVirtualizationMode(tree));
            Assert.True(ScrollViewer.GetCanContentScroll(tree));

            listBox.ApplyTemplate();
            Assert.NotNull(listBox.Template.FindName("ListBoxScrollViewer", listBox));
            Assert.NotNull(listBox.Template.FindName("ListBoxItemsPresenter", listBox));
            listBox.SelectedIndex = 1;
            Assert.Equal("Beta", listBox.SelectedItem);
            Assert.True(VirtualizingPanel.GetIsVirtualizing(listBox));
            Assert.Equal(VirtualizationMode.Recycling, VirtualizingPanel.GetVirtualizationMode(listBox));

            listView.ApplyTemplate();
            Assert.NotNull(listView.Template.FindName("ListViewScrollViewer", listView));
            Assert.NotNull(listView.Template.FindName("ListViewItemsPresenter", listView));
            listView.SelectedIndex = 1;
            Assert.Equal("Beta", listView.SelectedItem);
            Assert.True(VirtualizingPanel.GetIsVirtualizing(listView));
            Assert.Equal(VirtualizationMode.Recycling, VirtualizingPanel.GetVirtualizationMode(listView));

            dataGrid.ItemsSource = new[]
            {
                new KeyValuePair<string, string>("Alpha", "1"),
                new KeyValuePair<string, string>("Beta", "2")
            };
            dataGrid.ApplyTemplate();
            host.UpdateLayout();
            Assert.NotNull(FindVisualDescendant<ScrollViewer>(dataGrid));
            Assert.Equal(DataGridSelectionUnit.FullRow, dataGrid.SelectionUnit);
            Assert.Equal(DataGridSelectionMode.Single, dataGrid.SelectionMode);
            Assert.True(dataGrid.EnableRowVirtualization);
            Assert.True(dataGrid.EnableColumnVirtualization);
            Assert.True(VirtualizingPanel.GetIsVirtualizing(dataGrid));
            Assert.Same(resources["UiDataGridRowStyle"], dataGrid.RowStyle);
            Assert.Same(resources["UiDataGridCellStyle"], dataGrid.CellStyle);
            Assert.Same(resources["UiDataGridColumnHeaderStyle"], dataGrid.ColumnHeaderStyle);
            header.ApplyTemplate();
            Thumb leftGripper = Assert.IsType<Thumb>(header.Template.FindName("PART_LeftHeaderGripper", header));
            Thumb rightGripper = Assert.IsType<Thumb>(header.Template.FindName("PART_RightHeaderGripper", header));
            Assert.Equal(4, leftGripper.Width);
            Assert.Equal(4, rightGripper.Width);
            Assert.Equal(Cursors.SizeWE, leftGripper.Cursor);
            Assert.Equal(Cursors.SizeWE, rightGripper.Cursor);
            Assert.Same(resources["UiDataGridHeaderGripperTemplate"], leftGripper.Template);
            Assert.Same(resources["UiDataGridHeaderGripperTemplate"], rightGripper.Template);
            dataGrid.SelectedIndex = 1;
            Assert.Equal(1, dataGrid.SelectedIndex);

            verticalScrollBar.ApplyTemplate();
            horizontalScrollBar.ApplyTemplate();
            Track verticalTrack = Assert.IsType<Track>(
                verticalScrollBar.Template.FindName("PART_Track", verticalScrollBar));
            Track horizontalTrack = Assert.IsType<Track>(
                horizontalScrollBar.Template.FindName("PART_Track", horizontalScrollBar));
            Assert.NotNull(verticalTrack.Thumb);
            Assert.NotNull(verticalTrack.DecreaseRepeatButton);
            Assert.NotNull(verticalTrack.IncreaseRepeatButton);
            Assert.NotNull(horizontalTrack.Thumb);
            Assert.Equal(Orientation.Vertical, verticalTrack.Orientation);
            Assert.Equal(Orientation.Horizontal, horizontalTrack.Orientation);
            double previousValue = verticalScrollBar.Value;
            ScrollBar.LineDownCommand.Execute(null, verticalScrollBar);
            Assert.Equal(previousValue + verticalScrollBar.SmallChange, verticalScrollBar.Value);

            splitter.ApplyTemplate();
            Assert.NotNull(splitter.Template.FindName("SplitterBorder", splitter));
            Assert.Equal(Cursors.SizeWE, splitter.Cursor);
            Assert.Equal(GridResizeDirection.Columns, splitter.ResizeDirection);
            Assert.Equal(GridResizeBehavior.PreviousAndNext, splitter.ResizeBehavior);
            Assert.True(splitter.ShowsPreview);
            Assert.NotNull(splitter.PreviewStyle);

            progress.ApplyTemplate();
            Border progressTrack = Assert.IsType<Border>(progress.Template.FindName("PART_Track", progress));
            Border progressIndicator = Assert.IsType<Border>(progress.Template.FindName("PART_Indicator", progress));
            progress.Measure(new Size(200, 4));
            progress.Arrange(new Rect(0, 0, 200, 4));
            host.UpdateLayout();
            Assert.True(progressTrack.ActualWidth > 0);
            Assert.True(progressIndicator.ActualWidth > 0);
            progress.IsIndeterminate = true;
            Assert.True(progress.IsIndeterminate);
        }
        finally
        {
            host.Close();
        }
    }

    private static T? FindVisualDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                return match;

            T? descendant = FindVisualDescendant<T>(child);
            if (descendant is not null)
                return descendant;
        }

        return null;
    }

    private static void AssertInputHitWithin(UIElement surface, DependencyObject expectedAncestor, Point point)
    {
        DependencyObject hit = Assert.IsAssignableFrom<DependencyObject>(surface.InputHitTest(point));
        if (ReferenceEquals(hit, expectedAncestor))
            return;

        Visual hitVisual = Assert.IsAssignableFrom<Visual>(hit);
        Assert.True(hitVisual.IsDescendantOf(expectedAncestor));
    }

    private static void FlushDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
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
}
