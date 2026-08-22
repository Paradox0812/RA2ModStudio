using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldLearningWizardBoundaryTests
{
    [Fact]
    public void FieldLearningWizard_IsSimpleCurrentIniAndPastedTextWorkflowWithoutAdvancedTools()
    {
        string root = TestRepositoryRoot.Find();
        string wizardText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldLearningWizardWindow.xaml"));
        string wizardCodeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldLearningWizardWindow.xaml.cs"));
        string combinedText = wizardText + wizardCodeText;

        Assert.Contains("FieldLearningWizard.Window", wizardText);
        Assert.Contains("FieldLearningWizard.HeaderArea", wizardText);
        Assert.Contains("FieldLearningWizard.SourceSection", wizardText);
        Assert.Contains("FieldLearningWizard.ApplyTargetSection", wizardText);
        Assert.Contains("FieldLearningWizard.MainTabs", wizardText);
        Assert.Contains("FieldLearningWizard.UseCurrentIniButton", wizardText);
        Assert.Contains("FieldLearningWizard.ParsePastedTextButton", wizardText);
        Assert.Contains("FieldLearningWizard.RawTextBox", wizardText);
        Assert.Contains("FieldLearningWizard.CurrentIniDraftsGrid", wizardText);
        Assert.Contains("FieldLearningWizard.PreviewDiffGrid", wizardText);
        Assert.Contains("FieldLearningWizard.ValidationIssuesGrid", wizardText);
        Assert.Contains("FieldLearningWizard.BuildApplyPlanButton", wizardText);
        Assert.Contains("FieldLearningWizard.ApplyButton", wizardText);
        Assert.Contains("FieldLearningWizard.CustomChrome", wizardText);
        Assert.Contains("FieldLearningWizard.ChromeTitle", wizardText);
        Assert.Contains("FieldLearningWizard.CloseButton", wizardText);
        Assert.Contains("FieldLearningWizard.WorkflowStepStrip", wizardText);
        Assert.Contains("Text=\"1  来源\"", wizardText);
        Assert.Contains("Text=\"2  审阅\"", wizardText);
        Assert.Contains("Text=\"3  计划\"", wizardText);
        Assert.Contains("Text=\"4  确认 / 结果\"", wizardText);
        Assert.Contains("FieldLearningWizard.SourceScrollHost", wizardText);
        Assert.Contains("FieldLearningWizard.SourceSummary", wizardText);
        Assert.Contains("FieldLearningWizard.TargetModeSummary", wizardText);
        Assert.Contains("FieldLearningWizard.ReviewScrollHost", wizardText);
        Assert.Contains("FieldLearningWizard.EmptyReviewState", wizardText);
        Assert.Contains("FieldLearningWizard.ApplyBoundaryPanel", wizardText);
        Assert.Contains("FieldLearningWizard.GeneralizationApplySummaryText", wizardText);
        Assert.Contains("FieldLearningWizard.GeneralizationWarningSummaryText", wizardText);
        Assert.Contains("FieldLearningWizard.EditAllowedValuesButton", wizardText);
        Assert.Contains("FieldLearningWizard.StatusText", wizardText);
        Assert.Contains("Title=\"{Binding LearningWindowTitle}\"", wizardText);
        Assert.Contains("WindowStyle=\"None\"", wizardText);
        Assert.Contains("ResizeMode=\"CanResize\"", wizardText);
        Assert.Contains("shell:WindowChrome", wizardText);
        Assert.Contains("ResizeBorderThickness=\"6\"", wizardText);
        Assert.Contains("UseAeroCaptionButtons=\"False\"", wizardText);
        Assert.Contains("IdeAssistTitleTextStyle", wizardText);
        Assert.Contains("IdeFieldRegistryCommandButtonStyle", wizardText);
        Assert.Contains("IdeFieldRegistryDataGridStyle", wizardText);
        Assert.Contains("IdeFieldRegistryR2DataGridStyle", wizardText);
        Assert.Contains("IconGeometry.Issue.Error", wizardText);
        Assert.Contains("IconGeometry.Issue.Warning", wizardText);
        Assert.Contains("Text=\"{Binding Severity}\"", wizardText);
        Assert.DoesNotContain("IdeSecondary", wizardText, StringComparison.Ordinal);
        Assert.Contains("FieldLearningWizard.LearningSourceText", wizardText);
        Assert.Contains("LearningSourceSummaryText", wizardText);
        Assert.Contains("Click=\"UseCurrentIni\"", wizardText);
        Assert.Contains("Click=\"ParsePastedText\"", wizardText);
        Assert.Contains("Click=\"BuildApplyPlan\"", wizardText);
        Assert.Contains("Click=\"ApplyCurrentPlan\"", wizardText);
        Assert.Contains("Click=\"CloseButton_OnClick\"", wizardText);
        Assert.Contains("Content=\"使用当前 INI\"", wizardText);
        Assert.Contains("Content=\"解析粘贴文本\"", wizardText);
        Assert.Contains("Header=\"字段草稿\"", wizardText);
        Assert.Contains("Header=\"预览差异\"", wizardText);
        Assert.Contains("Header=\"验证问题\"", wizardText);
        Assert.Contains("Header=\"应用计划\"", wizardText);
        Assert.Contains("Text=\"字段学习\"", wizardText);
        Assert.DoesNotContain("锟", wizardText, StringComparison.Ordinal);
        Assert.Contains("LoadCurrentIniHarvestPreview", wizardCodeText);
        Assert.Contains("ParseAndPreview", wizardCodeText);
        Assert.Contains("BuildApplyPlan", wizardCodeText);
        Assert.Contains("CloseButton_OnClick", wizardCodeText);
        Assert.Contains("CreateApplyConfirmation", wizardCodeText);
        Assert.Contains("ApplyConfirmed", wizardCodeText);
        Assert.DoesNotContain("FetchRawText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RemoteHistory", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RemotePresets", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Rollback", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Manifest", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Completion", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AllowedValuesEditorWindow_IsLocalDraftEditorWithoutRegistryApplyOrSaveCoupling()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "AllowedValuesEditorWindow.xaml"));
        string code = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "AllowedValuesEditorWindow.xaml.cs"));
        string combinedText = xaml + code;

        Assert.Contains("AllowedValuesEditor.Window", xaml);
        Assert.Contains("AllowedValuesEditor.Grid", xaml);
        Assert.Contains("AllowedValuesEditor.AddButton", xaml);
        Assert.Contains("AllowedValuesEditor.RemoveButton", xaml);
        Assert.Contains("AllowedValuesEditor.DedupeButton", xaml);
        Assert.Contains("AllowedValuesEditor.SortButton", xaml);
        Assert.Contains("AllowedValuesEditor.AppendBuiltInButton", xaml);
        Assert.Contains("AllowedValuesEditor.RestoreScannedButton", xaml);
        Assert.Contains("AllowedValuesEditor.OkButton", xaml);
        Assert.Contains("AllowedValuesEditor.CancelButton", xaml);
        Assert.Contains("AllowedValuesEditor.Toolbar", xaml);
        Assert.Contains("AllowedValuesEditor.RowCommands", xaml);
        Assert.Contains("AllowedValuesEditor.NormalizationCommands", xaml);
        Assert.Contains("AllowedValuesEditor.ValidationSummary", xaml);
        Assert.Contains("AllowedValuesEditor.ActionFooter", xaml);
        Assert.Contains("Title=\"编辑可选值\"", xaml);
        Assert.Contains("Width=\"840\"", xaml);
        Assert.Contains("IdeFieldRegistryRootStyle", xaml);
        Assert.Contains("IdeFieldRegistryCommandButtonStyle", xaml);
        Assert.DoesNotContain("IdeSecondary", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"添加可选值\"", xaml);
        Assert.Contains("AppendMissingBuiltInValues", code);
        Assert.Contains("RestoreScannedValues", code);
        Assert.Contains("ToAllowedValuesText", code);
        Assert.DoesNotContain("FieldRegistryApplyWriter", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dirty", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FieldLearningWizardViewModel_SourceNameDrivesLearningSourceTitleAndSummary()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new();
        List<string> changedProperties = [];

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is not null)
                changedProperties.Add(args.PropertyName);
        };

        viewModel.SourceName = "rulesmd.ini [GDI]";

        Assert.Equal("字段学习 - rulesmd.ini [GDI]", viewModel.LearningWindowTitle);
        Assert.Equal("学习来源：rulesmd.ini [GDI]", viewModel.LearningSourceSummaryText);
        Assert.Contains(nameof(FieldRegistryHarvestPreviewViewModel.LearningWindowTitle), changedProperties);
        Assert.Contains(nameof(FieldRegistryHarvestPreviewViewModel.LearningSourceSummaryText), changedProperties);
    }

    [Fact]
    public void Shell_FieldRegistryCenterLearningEntry_OpensFieldLearningWizardInsteadOfAdvancedPreview()
    {
        string root = TestRepositoryRoot.Find();
        string shellCodeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string centerCodeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryCenterWindow.xaml.cs"));
        string centerText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryCenterWindow.xaml"));

        Assert.Contains("FieldLearningRequested", centerCodeText);
        Assert.Contains("FieldRegistryCenter.LearnFieldsButton", centerText);
        Assert.Contains("private FieldLearningWizardWindow? _fieldLearningWizardWindow", shellCodeText);
        Assert.Contains("OpenFieldLearningWizardWindow", shellCodeText);
        Assert.Contains("CreateFieldRegistryHarvestPreviewViewModel", shellCodeText);
        Assert.Contains("new FieldLearningWizardWindow(viewModel, GetCurrentIniSourceForFieldRegistryHarvest)", shellCodeText);
        Assert.Contains("FieldRegistryCenterWindow_OnFieldLearningRequested", shellCodeText);
        Assert.DoesNotContain("FieldRegistryCenterWindow_OnFieldLearningRequested(object? sender, EventArgs e)\r\n        => FieldRegistryManagerWindow_OnHarvestPreviewRequested", shellCodeText, StringComparison.Ordinal);
    }
}

