using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldAnnotationEditorUiBoundaryTests
{
    [Fact]
    public void AnnotationEditorWindow_ExposesSaveCloseApplyAndSidecarOnlyActions()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "FieldAnnotations",
            "Ra2FieldAnnotationEditorWindow.xaml"));
        string code = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "FieldAnnotations",
            "Ra2FieldAnnotationEditorWindow.xaml.cs"));
        string viewModel = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "ViewModels",
            "FieldAnnotations",
            "Ra2FieldAnnotationEditorViewModel.cs"));
        string combined = xaml + Environment.NewLine + code + Environment.NewLine + viewModel;

        Assert.Contains("字段注释编辑", xaml);
        Assert.Contains("中文显示名", xaml);
        Assert.Contains("别名", xaml);
        Assert.Contains("备注", xaml);
        Assert.Contains("FieldAnnotationEditor.DisplayNameTextBox", xaml);
        Assert.Contains("FieldAnnotationEditor.AliasesTextBox", xaml);
        Assert.Contains("FieldAnnotationEditor.NoteTextBox", xaml);
        Assert.Contains("FieldAnnotationEditor.CreateLibraryButton", xaml);
        Assert.Contains("FieldAnnotationEditor.ApplyButton", xaml);
        Assert.Contains("FieldAnnotationEditor.SaveAndCloseButton", xaml);
        Assert.Contains("FieldAnnotationEditor.Inspector", xaml);
        Assert.Contains("FieldAnnotationEditor.Form", xaml);
        Assert.Contains("FieldAnnotationEditor.ActionFooter", xaml);
        Assert.Contains("Width=\"640\"", xaml);
        Assert.Contains("Height=\"520\"", xaml);
        Assert.Contains("IdeFieldRegistryR2FlatSectionStyle", xaml);
        Assert.DoesNotContain("FieldAnnotationEditor.SaveButton", xaml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("创建注释库", xaml);
        Assert.Contains("应用", xaml);
        Assert.Contains("保存并关闭", xaml);
        Assert.Contains("TrySaveAnnotation()", code);
        Assert.Contains("Close();", ExtractMethod(code, "SaveAndCloseButton_OnClick"));
        Assert.DoesNotContain("Close();", ExtractMethod(code, "ApplyButton_OnClick"));
        Assert.Contains("IRa2FieldAnnotationStore", viewModel);
        Assert.Contains("Ra2FieldAnnotationEditingService", viewModel);

        Assert.DoesNotContain("ProjectSaveService", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText(", combined, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractMethod(string source, string methodName)
    {
        int start = source.IndexOf($"private void {methodName}", StringComparison.Ordinal);
        if (start < 0)
            throw new InvalidOperationException($"Method '{methodName}' was not found.");

        int nextMethod = source.IndexOf("\n    private ", start + methodName.Length, StringComparison.Ordinal);
        if (nextMethod < 0)
            nextMethod = source.Length;

        return source[start..nextMethod];
    }
}

