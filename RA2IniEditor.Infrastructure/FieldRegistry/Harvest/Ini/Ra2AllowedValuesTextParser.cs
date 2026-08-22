using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;

internal sealed class Ra2AllowedValuesTextParser
{
    public Ra2AllowedValuesTextParseResult Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new Ra2AllowedValuesTextParseResult(
                Array.AsReadOnly(Array.Empty<Ra2FieldAllowedValue>()),
                Array.AsReadOnly(Array.Empty<string>()));
        }

        List<Ra2FieldAllowedValue> values = new();
        List<string> warnings = new();
        HashSet<string> seenValues = new(StringComparer.OrdinalIgnoreCase);

        string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        foreach (string line in normalized.Split('\n'))
        {
            ParseLine(line, values, warnings, seenValues);
        }

        return new Ra2AllowedValuesTextParseResult(
            Array.AsReadOnly(values.ToArray()),
            Array.AsReadOnly(warnings.ToArray()));
    }

    private static void ParseLine(
        string line,
        List<Ra2FieldAllowedValue> values,
        List<string> warnings,
        HashSet<string> seenValues)
    {
        if (line.Length == 0)
            return;

        string[] entries = line.Split(';');
        foreach (string entry in entries)
        {
            ParseEntry(entry, values, warnings, seenValues);
        }
    }

    private static void ParseEntry(
        string entry,
        List<Ra2FieldAllowedValue> values,
        List<string> warnings,
        HashSet<string> seenValues)
    {
        string trimmed = entry.Trim();
        if (trimmed.Length == 0)
        {
            warnings.Add("Empty allowed value entry was skipped.");
            return;
        }

        string[] parts = trimmed.Split('|');
        string value = parts[0].Trim();
        if (value.Length == 0)
        {
            warnings.Add("Allowed value entry has an empty value and was skipped.");
            return;
        }

        if (!seenValues.Add(value))
        {
            warnings.Add($"Duplicate allowed value '{value}' was skipped.");
            return;
        }

        string? displayName = parts.Length >= 2 ? NormalizeOptional(parts[1]) : null;
        string? description = parts.Length >= 3
            ? NormalizeOptional(string.Join("|", parts.Skip(2).Select(part => part.Trim())))
            : null;

        values.Add(new Ra2FieldAllowedValue(value, displayName, description));
    }

    private static string? NormalizeOptional(string value)
    {
        string trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}
