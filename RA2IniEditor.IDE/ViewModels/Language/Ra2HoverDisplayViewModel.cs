using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.ViewModels.Language;

internal sealed class Ra2HoverDisplayViewModel
{
    public Ra2HoverDisplayViewModel(string title, string? noteText)
        : this(
            title,
            null,
            title,
            null,
            noteText,
            null,
            null,
            null,
            null,
            false)
    {
    }

    private Ra2HoverDisplayViewModel(
        string title,
        string? fieldTypeText,
        string fieldNameText,
        string? displayNameText,
        string? descriptionText,
        string? exampleValueText,
        string? exampleDescriptionText,
        string? sourceText,
        string? appliesToText,
        bool isReferenceValueHover)
    {
        Title = title;
        FieldTypeText = fieldTypeText;
        FieldNameText = fieldNameText;
        DisplayNameText = displayNameText;
        DescriptionText = descriptionText;
        ExampleValueText = exampleValueText;
        ExampleDescriptionText = exampleDescriptionText;
        SourceText = sourceText;
        AppliesToText = appliesToText;
        IsReferenceValueHover = isReferenceValueHover;
        NoteText = descriptionText;
    }

    public string Title { get; }

    public string? NoteText { get; }

    public string? FieldTypeText { get; }

    public string FieldNameText { get; }

    public string? DisplayNameText { get; }

    public string? DescriptionText { get; }

    public string? ExampleValueText { get; }

    public string? ExampleDescriptionText { get; }

    public string? SourceText { get; }

    public string? AppliesToText { get; }

    /// <summary>
    /// Single-line comment used by the Source Editor hover popup.
    /// </summary>
    public string? CompactCommentText => BuildCompactCommentText(DescriptionText, ExampleValueText);

    public bool IsReferenceValueHover { get; }

    public bool HasExample => !string.IsNullOrWhiteSpace(ExampleValueText);

    public bool HasMetadata =>
        !string.IsNullOrWhiteSpace(SourceText) ||
        !string.IsNullOrWhiteSpace(AppliesToText);

    public string ToToolTipText()
    {
        if (string.IsNullOrWhiteSpace(NoteText))
            return Title;

        return string.Join(Environment.NewLine, Title, NoteText);
    }

    public static Ra2HoverDisplayViewModel FromHoverInfo(Ra2HoverInfo hover)
    {
        ArgumentNullException.ThrowIfNull(hover);
        string key = hover.RawKey ?? hover.Title;
        string? displayName = hover.DisplayName ?? hover.Title;
        (string? description, string? exampleValue, string? exampleDescription) =
            SplitDescriptionAndExample(hover.Description);

        return new Ra2HoverDisplayViewModel(
            BuildTitle(hover.TypeDisplay, key, displayName),
            string.IsNullOrWhiteSpace(hover.TypeDisplay) ? null : hover.TypeDisplay.Trim(),
            key,
            ShouldShowDisplayName(key, displayName) ? displayName.Trim() : null,
            description,
            exampleValue,
            exampleDescription,
            NormalizeOptionalText(hover.Source),
            ExtractAppliesTo(hover.Detail),
            IsReferenceValueHoverInfo(hover));
    }

    internal static string BuildTitle(string? typeDisplay, string key, string? displayName)
    {
        List<string> parts = [];
        if (!string.IsNullOrWhiteSpace(typeDisplay))
            parts.Add(typeDisplay.Trim());

        parts.Add(key);
        if (!string.IsNullOrWhiteSpace(displayName) &&
            !string.Equals(key, displayName, StringComparison.OrdinalIgnoreCase))
        {
            parts.Add(displayName.Trim());
        }

        return string.Join(" ", parts);
    }

    private static bool ShouldShowDisplayName(string key, string? displayName)
        => !string.IsNullOrWhiteSpace(displayName) &&
           !string.Equals(key, displayName, StringComparison.OrdinalIgnoreCase);

    private static bool IsReferenceValueHoverInfo(Ra2HoverInfo hover)
        => hover.Detail.Contains("reference target", StringComparison.OrdinalIgnoreCase);

    private static (string? Description, string? ExampleValue, string? ExampleDescription) SplitDescriptionAndExample(string? text)
    {
        string? normalized = NormalizeOptionalText(text);
        if (normalized is null)
            return (null, null, null);

        int markerIndex = normalized.IndexOf("\u793a\u4f8b\uff1a", StringComparison.Ordinal);
        int markerLength = "\u793a\u4f8b\uff1a".Length;
        if (markerIndex < 0)
        {
            markerIndex = normalized.IndexOf("Example:", StringComparison.OrdinalIgnoreCase);
            markerLength = "Example:".Length;
        }

        if (markerIndex < 0)
            return (normalized, null, null);

        string? description = NormalizeOptionalText(normalized[..markerIndex].TrimEnd(';', '\uff1b', ' '));
        string exampleText = normalized[(markerIndex + markerLength)..].Trim();
        if (string.IsNullOrWhiteSpace(exampleText))
            return (description, null, null);

        string[] parts = exampleText.Split(" - ", 2, StringSplitOptions.TrimEntries);
        return parts.Length == 2
            ? (description, parts[0], parts[1])
            : (description, exampleText, null);
    }

    private static string? ExtractAppliesTo(string? detail)
    {
        string? normalized = NormalizeOptionalText(detail);
        if (normalized is null)
            return null;

        const string marker = "Applies to:";
        int start = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            return null;

        start += marker.Length;
        int end = normalized.IndexOf(';', start);
        string appliesTo = end < 0 ? normalized[start..] : normalized[start..end];
        return NormalizeOptionalText(appliesTo);
    }

    private static string? BuildCompactCommentText(string? description, string? exampleValue)
    {
        string? compactDescription = NormalizeSingleLine(description);
        if (string.IsNullOrWhiteSpace(exampleValue))
            return compactDescription;

        string exampleText = $"示例 {exampleValue.Trim()}";
        return string.IsNullOrWhiteSpace(compactDescription)
            ? exampleText
            : $"{compactDescription}；{exampleText}";
    }

    private static string? NormalizeSingleLine(string? text)
    {
        string? normalized = NormalizeOptionalText(text);
        if (normalized is null)
            return null;

        return normalized
            .Replace(Environment.NewLine, " · ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();
    }

    private static string? NormalizeOptionalText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text.Trim();
    }
}
