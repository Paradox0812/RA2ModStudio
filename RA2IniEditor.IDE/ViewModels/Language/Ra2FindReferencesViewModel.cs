using System.Collections.ObjectModel;
using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.ViewModels.Language;

internal sealed class Ra2FindReferencesViewModel
{
    public Ra2FindReferencesViewModel(Ra2ReferenceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        Target = string.IsNullOrWhiteSpace(result.TargetName)
            ? "No reference target"
            : $"[{result.TargetName}]\uff08{result.TargetKind}\uff09";
        References = new ObservableCollection<Ra2ReferenceItemViewModel>(
            result.Items.Select(item => new Ra2ReferenceItemViewModel(item)));
        StatusText = References.Count == 0
            ? "\u5f53\u524d\u6587\u4ef6\u4e2d\u672a\u627e\u5230\u5f15\u7528\u3002"
            : $"\u5f53\u524d\u6587\u4ef6\u4e2d\u627e\u5230 {References.Count} \u5904\u5f15\u7528\u3002";
    }

    public string Target { get; }

    public ObservableCollection<Ra2ReferenceItemViewModel> References { get; }

    public string StatusText { get; }
}
