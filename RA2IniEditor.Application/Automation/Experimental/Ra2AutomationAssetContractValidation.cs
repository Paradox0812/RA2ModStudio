namespace RA2IniEditor.Application.Automation.Experimental;

internal static class Ra2AutomationAssetContractValidation
{
    public static string ValidateText(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Asset contract text cannot be empty.", parameterName);

        string normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("Asset contract text is invalid or exceeds its limit.", parameterName);

        return normalized;
    }

    public static string ValidateFileName(string value, string parameterName)
    {
        string normalized = ValidateText(
            value,
            Ra2AutomationAssetRequirement.MaximumFileNameLength,
            parameterName);
        if (normalized is "." or ".." || normalized.IndexOfAny(['\\', '/', ':', '*', '?', '"', '<', '>', '|']) >= 0)
            throw new ArgumentException("Asset file names must not contain a path.", parameterName);

        return normalized;
    }

    public static string GetExpectedExtension(Ra2AutomationAssetKind kind)
        => kind switch
        {
            Ra2AutomationAssetKind.ShpAnimation or Ra2AutomationAssetKind.Cameo => ".shp",
            Ra2AutomationAssetKind.VxlModel => ".vxl",
            Ra2AutomationAssetKind.HvaAnimation => ".hva",
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    public static bool HasExpectedExtension(string fileName, Ra2AutomationAssetKind kind)
        => string.Equals(Path.GetExtension(fileName), GetExpectedExtension(kind), StringComparison.OrdinalIgnoreCase);
}
