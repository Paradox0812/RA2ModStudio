using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Editing;

/// <summary>
/// 将 Host 快照投影到 Application 唯一语义预览引擎，并恢复 A3/A4 兼容展示形状。
/// </summary>
internal sealed class Ra2IniEditPreviewService : IRa2IniEditPreviewService
{
    private readonly IRa2AutomationCapabilityGateway _gateway;

    public Ra2IniEditPreviewService(
        IRa2IniLanguageAnalysisService languageAnalysisService,
        Ra2AddPropertyInsertPlanner insertPlanner)
        : this(new Ra2AutomationCapabilityGateway())
    {
        ArgumentNullException.ThrowIfNull(languageAnalysisService);
        ArgumentNullException.ThrowIfNull(insertPlanner);
    }

    internal Ra2IniEditPreviewService(IRa2AutomationCapabilityGateway gateway)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    }

    public Ra2IniEditPreview Preview(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);

        Ra2AutomationEditPreviewResult result = _gateway.Preview(
            snapshot.ToAutomationSnapshot(),
            plan,
            cancellationToken);
        return Ra2IniEditPreview.FromAutomation(snapshot, plan, result);
    }
}
