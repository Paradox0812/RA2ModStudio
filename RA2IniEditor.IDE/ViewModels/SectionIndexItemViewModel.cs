using RA2IniEditor.IDE.Models;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Represents a readonly section item shown in the navigator.
/// </summary>
public sealed class SectionIndexItemViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SectionIndexItemViewModel"/> class.
    /// </summary>
    public SectionIndexItemViewModel(ReadonlySectionIndexItem item)
    {
        SectionId = item.SectionId;
        LineNumber = item.LineNumber;
        DisplayName = item.DisplayName;
    }

    /// <summary>
    /// Gets the section identifier.
    /// </summary>
    public string SectionId { get; }

    /// <summary>
    /// Gets the one-based line number.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the optional display name.
    /// </summary>
    public string? DisplayName { get; }

    /// <summary>
    /// Gets the primary display text.
    /// </summary>
    public string DisplayText => string.IsNullOrWhiteSpace(DisplayName)
        ? SectionId
        : $"{SectionId}  {DisplayName}";
}
