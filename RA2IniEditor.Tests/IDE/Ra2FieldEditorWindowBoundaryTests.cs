using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldEditorWindowBoundaryTests
{
    [Fact]
    public void FieldEditorWindow_ExposesSingleFieldEditorShellPreviewAndApplyResultUi()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldEditorWindow.xaml"));
        string code = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldEditorWindow.xaml.cs"));
        string viewModel = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "FieldRegistry", "FieldEditorViewModel.cs"));
        string combined = xaml + code + viewModel;

        Assert.Contains("FieldEditor.Window", xaml);
        Assert.Contains("WindowStyle=\"None\"", xaml);
        Assert.Contains("ResizeMode=\"CanResize\"", xaml);
        Assert.Contains("shell:WindowChrome", xaml);
        Assert.Contains("FieldEditor.CustomChrome", xaml);
        Assert.Contains("FieldEditor.ChromeTitle", xaml);
        Assert.Contains("FieldEditor.CloseButton", xaml);
        Assert.Contains("FieldEditor.HeaderArea", xaml);
        Assert.Contains("FieldEditor.BasicSection", xaml);
        Assert.Contains("FieldEditor.DescriptionSection", xaml);
        Assert.Contains("x:Name=\"MetadataColumn\"", xaml);
        Assert.Contains("x:Name=\"DocumentationColumn\"", xaml);
        Assert.Contains("FieldEditor.ActionFooter", xaml);
        Assert.Contains("FieldEditor.SavePreviewSection", xaml);
        Assert.Contains("FieldEditor.KeyTextBox", xaml);
        Assert.Contains("FieldEditor.SectionKindComboBox", xaml);
        Assert.Contains("FieldEditor.EditorKindComboBox", xaml);
        Assert.Contains("FieldEditor.ValueKindComboBox", xaml);
        Assert.Contains("FieldEditor.BooleanStyleComboBox", xaml);
        Assert.Contains("FieldEditor.SeparatorTextBox", xaml);
        Assert.Contains("FieldEditor.EnumNameTextBox", xaml);
        Assert.Contains("FieldEditor.AllowedValuesTextBox", xaml);
        Assert.Contains("FieldEditor.DisplayNameTextBox", xaml);
        Assert.Contains("FieldEditor.AliasesTextBox", xaml);
        Assert.Contains("FieldEditor.DescriptionTextBox", xaml);
        Assert.Contains("FieldEditor.ProjectPreviewButton", xaml);
        Assert.Contains("FieldEditor.GlobalPreviewButton", xaml);
        Assert.Contains("FieldEditor.CopyPersistedPreviewButton", xaml);
        Assert.Contains("FieldEditor.PreviewSummaryText", xaml);
        Assert.Contains("FieldEditor.PreviewIssuesGrid", xaml);
        Assert.Contains("FieldEditor.PersistedPreviewTextBox", xaml);
        Assert.Contains("FieldEditor.ProjectSaveButton", xaml);
        Assert.Contains("FieldEditor.GlobalSaveButton", xaml);
        Assert.Contains("FieldEditor.CancelButton", xaml);
        Assert.Contains("FieldEditor.StatusText", xaml);
        Assert.Contains("FieldEditor.ApplyResultPanel", xaml);
        Assert.Contains("FieldEditor.TargetPathTextBox", xaml);
        Assert.Contains("FieldEditor.ManifestPathTextBox", xaml);
        Assert.Contains("FieldEditor.CopyTargetPathButton", xaml);
        Assert.Contains("FieldEditor.OpenTargetFolderButton", xaml);
        Assert.Contains("FieldEditor.CopyManifestPathButton", xaml);
        Assert.Contains("FieldEditor.OpenManifestFolderButton", xaml);
        Assert.Contains("Text=\"字段名\"", xaml);
        Assert.Contains("Text=\"适用对象类型\"", xaml);
        Assert.Contains("Text=\"布尔值风格\"", xaml);
        Assert.Contains("Text=\"列表分隔符\"", xaml);
        Assert.Contains("Text=\"可选值\"", xaml);
        Assert.Contains("Text=\"保存预览\"", xaml);
        Assert.Contains("Content=\"保存到项目字段库\"", xaml);
        Assert.Contains("Content=\"保存到全局字段库\"", xaml);
        Assert.Contains("Width=\"960\"", xaml);
        Assert.Contains("Height=\"720\"", xaml);
        Assert.Contains("MinHeight=\"620\"", xaml);
        Assert.Contains("IdeFieldRegistryR2DataGridStyle", xaml);
        Assert.Contains("用于多个枚举值之间的分隔", xaml);
        Assert.Contains("每一行表示一个可补全值", xaml);
        Assert.Contains("public bool CanSave => SavePreview?.CanSave == true", viewModel);
        Assert.Contains("public string PersistedJsonPreview", viewModel);
        Assert.Contains("public bool HasPersistedJsonPreview", viewModel);
        Assert.Contains("Text=\"{Binding PersistedJsonPreview, Mode=OneWay}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding HasPersistedJsonPreview}\"", xaml);
        Assert.Contains("IsReadOnly=\"True\"", xaml);
        Assert.Contains("public bool CanPreviewSave", viewModel);
        Assert.Contains("public bool HasLastApplyPaths", viewModel);
        Assert.Contains("LastApplyTargetFilePath", viewModel);
        Assert.Contains("LastApplyManifestFilePath", viewModel);
        Assert.Contains("ObservableCollection<FieldEditorValidationIssue> PreviewIssues", viewModel);
        Assert.Contains("BooleanStyleOptions", viewModel);
        Assert.Contains("IsBooleanStyleEditable", viewModel);
        Assert.Contains("IsSeparatorEditable", viewModel);
        Assert.Contains("ApplySave", viewModel);
        Assert.Contains("BuildSavePreview", viewModel);
        Assert.Contains("BuildProjectPreview", code);
        Assert.Contains("BuildGlobalPreview", code);
        Assert.Contains("ApplyProjectSave", code);
        Assert.Contains("ApplyGlobalSave", code);
        Assert.Contains("CopyTargetPath", code);
        Assert.Contains("CopyManifestPath", code);
        Assert.Contains("CopyPersistedPreview", code);
        Assert.Contains("OpenTargetFolder", code);
        Assert.Contains("OpenManifestFolder", code);
        Assert.Contains("CloseButton_OnClick", code);
        Assert.Contains("=> Close();", code);
        Assert.Contains("Clipboard.SetText", code);
        Assert.Contains("explorer.exe", code);
        Assert.Contains("FieldEditorViewModel(Ra2FieldDefinition definition, Ra2SectionKind sectionKind)", viewModel);
        Assert.Contains("new FieldEditorViewModel(definition, sectionKind)", code);
        Assert.DoesNotContain("ProjectSaveService", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SaveCurrentFile", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Completion", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Rollback", combined, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FieldEditorWindow_PersistedJsonPreviewBinding_IsOneWayAndReadOnly()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldEditorWindow.xaml"));

        Assert.Contains("FieldEditor.PersistedPreviewTextBox", xaml);
        Assert.Contains("Text=\"{Binding PersistedJsonPreview, Mode=OneWay}\"", xaml);
        Assert.Contains("IsReadOnly=\"True\"", xaml);
        Assert.DoesNotContain("Text=\"{Binding PersistedJsonPreview}\"", xaml);
        Assert.DoesNotContain("Text=\"{Binding PersistedJsonPreview, Mode=TwoWay", xaml);
        Assert.DoesNotContain("Text=\"{Binding PersistedJsonPreview, Mode=OneWayToSource", xaml);
    }

    [Fact]
    public void AllowedValuesEditor_UsesPlainChineseColumnsAndDisabledRemoveButton()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "AllowedValuesEditorWindow.xaml"));
        string code = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "AllowedValuesEditorWindow.xaml.cs"));

        Assert.Contains("WindowStyle=\"None\"", xaml);
        Assert.Contains("ResizeMode=\"CanResize\"", xaml);
        Assert.Contains("shell:WindowChrome", xaml);
        Assert.Contains("AllowedValuesEditor.CustomChrome", xaml);
        Assert.Contains("AllowedValuesEditor.ChromeTitle", xaml);
        Assert.Contains("AllowedValuesEditor.CloseButton", xaml);
        Assert.Contains("Click=\"CloseButton_OnClick\"", xaml);
        Assert.Contains("Header=\"值\"", xaml);
        Assert.Contains("Header=\"显示名\"", xaml);
        Assert.Contains("Header=\"说明\"", xaml);
        Assert.Contains("Content=\"添加可选值\"", xaml);
        Assert.Contains("Content=\"删除选中项\"", xaml);
        Assert.Contains("Content=\"恢复扫描值\"", xaml);
        Assert.Contains("IdeFieldRegistryRootStyle", xaml);
        Assert.Contains("IdeFieldRegistryDataGridStyle", xaml);
        Assert.Contains("IdeFieldRegistryCommandButtonStyle", xaml);
        Assert.DoesNotContain("IdeSecondary", xaml, StringComparison.Ordinal);
        Assert.Contains("IsEnabled=\"{Binding SelectedItem, ElementName=AllowedValuesGrid}\"", xaml);
        Assert.Contains("ResultText = _viewModel.ToAllowedValuesText();", code);
        Assert.Contains("DialogResult = true;", code);
        Assert.Contains("DialogResult = false;", code);
        Assert.Contains("CloseButton_OnClick", code);
    }

    [Fact]
    public void FieldRegistryCenter_OpensFieldEditorWithEffectiveProvider()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryCenterWindow.xaml"));
        string code = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryCenterWindow.xaml.cs"));

        Assert.Contains("FieldRegistryCenter.HeaderArea", xaml);
        Assert.Contains("FieldRegistryCenter.Toolbar", xaml);
        Assert.Contains("FieldRegistryCenter.SearchArea", xaml);
        Assert.Contains("FieldRegistryCenter.ActivePacksPanel", xaml);
        Assert.Contains("FieldRegistryCenter.MainFieldsPanel", xaml);
        Assert.Contains("FieldRegistryCenter.NewFieldButton", xaml);
        Assert.Contains("FieldRegistryCenter.EditFieldButton", xaml);
        Assert.Contains("MouseDoubleClick=\"FieldsGrid_OnMouseDoubleClick\"", xaml);
        Assert.Contains("Content=\"重新加载\"", xaml);
        Assert.Contains("Content=\"学习字段\"", xaml);
        Assert.Contains("Content=\"高级工具\"", xaml);
        Assert.Contains("MinHeight=\"260\"", xaml);
        Assert.Contains("private FieldEditorSaveContext _fieldEditorSaveContext", code);
        Assert.Contains("private FieldEditorWindow? _fieldEditorWindow", code);
        Assert.Contains("OpenFieldEditor(null)", code);
        Assert.Contains("OpenFieldEditor(FieldsGrid.SelectedItem as FieldRegistryCenterFieldRow)", code);
        Assert.Contains("new FieldEditorWindow(_fieldEditorSaveContext)", code);
        Assert.Contains("new FieldEditorWindow(row.Definition, row.SectionKindValue, _fieldEditorSaveContext)", code);
        Assert.Contains("FieldRegistrySaveApplied", code);
        Assert.Contains("RefreshFieldRegistryContext", code);
        Assert.Contains("public Ra2FieldDefinition Definition", code);
        Assert.Contains("public Ra2SectionKind SectionKindValue", code);
    }
}

