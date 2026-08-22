using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AddPropertyUiBoundaryTests
{
    [Fact]
    public void ShellWindow_ExposesAddPropertyMenuAndAppliesOnlyInMemory()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string completionController = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Controllers",
            "Completion",
            "Ra2CompletionInteractionController.cs"));
        string fieldBrowserController = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Controllers",
            "FieldBrowser",
            "Ra2FieldBrowserController.cs"));
        string addPropertyWindowXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldBrowser", "Ra2AddPropertyWindow.xaml"));
        string combinedText = shellXaml + Environment.NewLine + shellCode + Environment.NewLine + completionController + Environment.NewLine + fieldBrowserController + Environment.NewLine + addPropertyWindowXaml;

        Assert.Contains("Shell.SourceEditor.AddPropertyMenuItem", shellXaml);
        Assert.Contains("AddProperty_OnClick", shellXaml);
        Assert.Contains("Ra2AddPropertyWindow", shellCode);
        Assert.Contains("Ra2AddPropertyViewModel", shellCode);
        Assert.Contains("Ra2AddPropertyInsertPlanner", combinedText);
        Assert.Contains("SelectedDuplicateAction", combinedText);
        Assert.Contains("Ra2DuplicateKeyAction.JumpExisting", combinedText);
        Assert.Contains("Ra2DuplicateKeyAction.ReplaceExisting", combinedText);
        Assert.Contains("ApplyAddPropertyReplaceExisting", shellCode);
        Assert.Contains("ApplyReplaceExisting", combinedText);
        Assert.Contains("ApplyInsertDuplicate", combinedText);
        Assert.Contains("Ra2RecentFieldUsageTracker", shellCode);
        Assert.Contains("Ra2FieldAnnotationStatusViewModel.FromLoadResult", combinedText);
        Assert.Contains("_recentFieldUsageTracker.Record", shellCode);
        Assert.Contains("_textChangeApplier.Apply", combinedText);
        Assert.Contains("SetEditorTextFromProgram", shellCode);
        Assert.Contains("RestoreSourceEditorFocusAtCaret", shellCode);
        Assert.Contains("Add property skipped: no editable file is currently open.", combinedText);
        Assert.Contains("Ra2CompletionDisplayEnhancer", combinedText);
        Assert.Contains("_displayEnhancer.Enhance", combinedText);
        Assert.Contains("AddProperty.AddSelectedButton", addPropertyWindowXaml);
        Assert.Contains("AddProperty.SearchModeComboBox", addPropertyWindowXaml);
        Assert.Contains("x:Name=\"SearchTextBox\"", addPropertyWindowXaml);
        Assert.Contains("PreviewKeyDown=\"SearchTextBox_OnPreviewKeyDown\"", addPropertyWindowXaml);
        Assert.Contains("AddProperty.EditAnnotationButton", addPropertyWindowXaml);
        Assert.Contains("编辑字段注释", addPropertyWindowXaml);
        Assert.Contains("AddProperty.FieldDetailsPanel", addPropertyWindowXaml);
        Assert.Contains("AddProperty.Inspector", addPropertyWindowXaml);
        Assert.Contains("AddProperty.ValueEntry", addPropertyWindowXaml);
        Assert.Contains("AddProperty.ActionFooter", addPropertyWindowXaml);
        Assert.Contains("Width=\"960\"", addPropertyWindowXaml);
        Assert.Contains("Height=\"680\"", addPropertyWindowXaml);
        Assert.Contains("IdeFieldRegistryR2DataGridStyle", addPropertyWindowXaml);
        Assert.Contains("SelectedItem.Details.Title", addPropertyWindowXaml);
        Assert.Contains("SelectedItem.Details.Examples", addPropertyWindowXaml);
        Assert.Contains("Text=\"示例\"", addPropertyWindowXaml);
        Assert.Contains("AddProperty.AnnotationStatusText", addPropertyWindowXaml);
        Assert.Contains("AddProperty.ValueHintText", addPropertyWindowXaml);
        Assert.Contains("AddProperty.InsertPreviewText", addPropertyWindowXaml);
        Assert.Contains("AddProperty.DuplicateWarningText", addPropertyWindowXaml);
        Assert.Contains("AddProperty.DuplicateActionWarningText", addPropertyWindowXaml);
        Assert.Contains("AddProperty.DuplicateActionComboBox", addPropertyWindowXaml);
        Assert.Contains("CanConfirm", addPropertyWindowXaml);
        Assert.Contains("AddProperty.ReadOnlyHintText", addPropertyWindowXaml);
        Assert.Contains("RecentDisplay", addPropertyWindowXaml);
        Assert.Contains("MatchSourceDisplay", addPropertyWindowXaml);
        Assert.Contains("AnnotationDisplay", addPropertyWindowXaml);
        Assert.Contains("MinWidth=\"130\"", addPropertyWindowXaml);
        Assert.Contains("MinWidth=\"220\"", addPropertyWindowXaml);
        Assert.Contains("Ra2AddPropertyWindow_Loaded", File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldBrowser", "Ra2AddPropertyWindow.xaml.cs")));
        Assert.Contains("DispatcherPriority.Input", File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldBrowser", "Ra2AddPropertyWindow.xaml.cs")));
        Assert.Contains("TryConfirmFromKeyboard", File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldBrowser", "Ra2AddPropertyWindow.xaml.cs")));
        Assert.Contains("ClearSearchForEscape", File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldBrowser", "Ra2AddPropertyWindow.xaml.cs")));
        Assert.Contains("EditAnnotationRequested", shellCode);
        Assert.Contains("Ra2FieldAnnotationEditorWindow", shellCode);

        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText(", combinedText, StringComparison.OrdinalIgnoreCase);
    }
}

