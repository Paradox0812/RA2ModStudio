using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.ViewModels.FieldRegistry;

internal interface IFieldEditorDraftFactory
{
    FieldEditorDraft CreateDraft(FieldEditorViewModel viewModel, FieldEditorSaveTarget target);
}

internal sealed class FieldEditorDraftFactory : IFieldEditorDraftFactory
{
    private static readonly char[] AliasSeparators = [',', '，', ';', '；'];

    public FieldEditorDraft CreateDraft(FieldEditorViewModel viewModel, FieldEditorSaveTarget target)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        List<string> allowedValueInputErrors = [];
        IReadOnlyList<FieldEditorAllowedValueDraft> allowedValues = ParseAllowedValues(
            viewModel.AllowedValuesText,
            allowedValueInputErrors);

        return new FieldEditorDraft(
            viewModel.Key.Trim(),
            viewModel.SectionKind,
            viewModel.EditorKind,
            viewModel.ValueKind,
            viewModel.BooleanStyle,
            NormalizeOptional(viewModel.EnumName),
            allowedValues,
            NormalizeOptional(viewModel.DisplayName),
            ParseAliases(viewModel.AliasesText),
            NormalizeOptional(viewModel.Description),
            target,
            NormalizeSeparator(viewModel.Separator),
            allowedValueInputErrors);
    }

    private static IReadOnlyList<string> ParseAliases(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        List<string> aliases = [];
        foreach (string part in text.Split(AliasSeparators, StringSplitOptions.RemoveEmptyEntries))
        {
            string alias = part.Trim();
            if (alias.Length == 0 || !seen.Add(alias))
                continue;

            aliases.Add(alias);
        }

        return aliases;
    }

    private static IReadOnlyList<FieldEditorAllowedValueDraft> ParseAllowedValues(
        string text,
        List<string> inputErrors)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        List<FieldEditorAllowedValueDraft> values = [];
        string[] lines = text.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index];
            string trimmed = line.Trim();
            if (trimmed.Length == 0)
                continue;

            string[] parts = trimmed.Split('|', StringSplitOptions.None)
                .Select(part => part.Trim())
                .ToArray();
            if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
            {
                inputErrors.Add($"第 {index + 1} 行可选值缺少实际写入值。");
                continue;
            }

            values.Add(new FieldEditorAllowedValueDraft(
                parts[0],
                parts.Length > 1 ? parts[1] : null,
                parts.Length > 2 ? string.Join(" | ", parts.Skip(2)) : null));
        }

        return values;
    }

    private static string? NormalizeOptional(string text)
        => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static string NormalizeSeparator(string text)
        => text ?? ",";
}
