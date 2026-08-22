using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldRegistryHarvestPreviewWindowApplyGuardrailTests
{
    [Fact]
    public void HarvestPreviewWindow_ExposesExplicitPlanThenApplyFlow()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));
        string codeBehind = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml.cs"));

        Assert.Contains("构建应用计划", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"BuildApplyPlan\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"ApplyCurrentPlan\"", xaml, StringComparison.Ordinal);
        Assert.Contains("LastApplyTargetFilePath", xaml, StringComparison.Ordinal);
        Assert.Contains("LastApplyBackupManifestPath", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.GeneralizationSummaryText", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.GeneralizationApplySummaryText", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.GeneralizationWarningSummaryText", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.GeneralizationGrid", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.WorkflowStepStrip", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.PlanArea", xaml, StringComparison.Ordinal);
        Assert.Contains("MessageBox.Show", codeBehind, StringComparison.Ordinal);
        Assert.Contains("CreateApplyConfirmation", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ApplyConfirmed", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void HarvestPreviewWindow_DoesNotExposeRollbackOrAutomaticApply()
    {
        string root = TestRepositoryRoot.Find();
        string text = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml")) +
            File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml.cs"));

        Assert.DoesNotContain("Rollback", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GitHub", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HttpClient", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ApplyAsync", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextChanged=\"Apply", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextChanged=\"Parse", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", text, StringComparison.OrdinalIgnoreCase);
    }
}

