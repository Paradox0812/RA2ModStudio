namespace RA2IniEditor.IDE.Models;

/// <summary>
/// Represents a lightweight readonly section index item for the current INI source text.
/// </summary>
public sealed class ReadonlySectionIndexItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadonlySectionIndexItem"/> class.
    /// </summary>
    public ReadonlySectionIndexItem(string sectionId, int lineNumber, string? displayName)
    {
        SectionId = sectionId;
        LineNumber = lineNumber;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets the section identifier without brackets.
    /// </summary>
    public string SectionId { get; }

    /// <summary>
    /// Gets the one-based line number where the section begins.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the best-effort display name from Name, UIName, or Image.
    /// </summary>
    public string? DisplayName { get; }
}
