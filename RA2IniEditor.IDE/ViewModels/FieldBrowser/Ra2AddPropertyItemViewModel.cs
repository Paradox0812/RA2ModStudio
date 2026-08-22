using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.ViewModels.FieldAnnotations;
using RA2IniEditor.IDE.ViewModels.FieldDetails;

namespace RA2IniEditor.IDE.ViewModels.FieldBrowser;

internal sealed class Ra2AddPropertyItemViewModel
{
    private static readonly Ra2SectionKindDisplayNameProvider SectionDisplayNameProvider = new();

    public Ra2AddPropertyItemViewModel(
        Ra2SectionKind sectionKind,
        Ra2FieldDisplayInfo displayInfo,
        bool isRecent = false,
        Ra2FieldApplicabilityKind applicability = Ra2FieldApplicabilityKind.Unknown,
        Ra2FieldBrowserMatchResult? matchResult = null)
    {
        SectionKind = sectionKind;
        DisplayInfo = displayInfo ?? throw new ArgumentNullException(nameof(displayInfo));
        IsRecent = isRecent;
        Applicability = applicability;
        MatchResult = matchResult ?? Ra2FieldBrowserMatchResult.EmptyQuery;
        Details = displayInfo.Definition is null
            ? Ra2FieldDetailsViewModel.NotFound(displayInfo.Key, sectionKind)
            : Ra2FieldDetailsViewModel.FromDefinition(displayInfo.Definition, sectionKind);
    }

    public Ra2FieldDisplayInfo DisplayInfo { get; }

    public Ra2SectionKind SectionKind { get; }

    public string Key => DisplayInfo.Key;

    public string DisplayName => DisplayInfo.DisplayName;

    public string Description => DisplayInfo.Description ?? string.Empty;

    public string TypeDisplay => DisplayInfo.TypeDisplay;

    public string SourceDisplay => DisplayInfo.SourceDisplay;

    public bool IsRecent { get; }

    public Ra2FieldApplicabilityKind Applicability { get; }

    public Ra2FieldBrowserMatchResult MatchResult { get; }

    public Ra2FieldDetailsViewModel Details { get; }

    public bool HasUserAnnotation => DisplayInfo.HasUserAnnotation;

    public string RecentDisplay => IsRecent ? "最近" : string.Empty;

    public string AnnotationDisplay => HasUserAnnotation ? "已注释" : string.Empty;

    public string ApplicabilityDisplay => Applicability switch
    {
        Ra2FieldApplicabilityKind.Common => "通用",
        Ra2FieldApplicabilityKind.SectionSpecific => SectionDisplayNameProvider.GetDisplayName(SectionKind),
        _ => "字段库"
    };

    public string MatchSourceDisplay => MatchResult.Source switch
    {
        Ra2FieldBrowserMatchSource.Key => "键名",
        Ra2FieldBrowserMatchSource.DisplayName => "显示名",
        Ra2FieldBrowserMatchSource.Alias => "别名",
        Ra2FieldBrowserMatchSource.Note => "备注",
        Ra2FieldBrowserMatchSource.Description => "字段说明",
        Ra2FieldBrowserMatchSource.Recent => "最近使用",
        _ => IsRecent ? "最近使用" : "-"
    };

    public string MatchText => MatchResult.MatchedText ?? string.Empty;

    public int MatchPriority => MatchResult.Priority;

    public string SuggestedValue => string.Empty;

    public string AliasesDisplay => string.Join(", ", DisplayInfo.Aliases);

    public string Note => DisplayInfo.Note ?? string.Empty;

    public string Tooltip
        => string.IsNullOrWhiteSpace(Description)
            ? Key
            : $"{Key}: {Description}";

    public string InsertKey => DisplayInfo.Key;

    public Ra2AddPropertyItemViewModel WithMatch(Ra2FieldBrowserMatchResult matchResult)
        => new(SectionKind, DisplayInfo, IsRecent, Applicability, matchResult);
}
