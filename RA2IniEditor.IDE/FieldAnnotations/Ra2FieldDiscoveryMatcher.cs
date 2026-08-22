namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2FieldDiscoveryMatcher
{
    public bool IsMatch(Ra2FieldDisplayInfo displayInfo, string? query)
        => Match(displayInfo, query).IsMatch;

    public int GetPriority(Ra2FieldDisplayInfo displayInfo, string? query)
        => Match(displayInfo, query).Priority;

    public Ra2FieldBrowserMatchResult Match(Ra2FieldDisplayInfo displayInfo, string? query)
    {
        ArgumentNullException.ThrowIfNull(displayInfo);

        string normalizedQuery = query?.Trim() ?? string.Empty;
        if (normalizedQuery.Length == 0)
            return Ra2FieldBrowserMatchResult.EmptyQuery;

        if (StartsWith(displayInfo.Key, normalizedQuery))
            return Ra2FieldBrowserMatchResult.Match(Ra2FieldBrowserMatchSource.Key, 1, displayInfo.Key);

        if (StartsWith(displayInfo.DisplayName, normalizedQuery))
            return Ra2FieldBrowserMatchResult.Match(Ra2FieldBrowserMatchSource.DisplayName, 2, displayInfo.DisplayName);

        string? aliasPrefix = displayInfo.Aliases.FirstOrDefault(alias => StartsWith(alias, normalizedQuery));
        if (aliasPrefix is not null)
            return Ra2FieldBrowserMatchResult.Match(Ra2FieldBrowserMatchSource.Alias, 3, aliasPrefix);

        if (Contains(displayInfo.Key, normalizedQuery))
            return Ra2FieldBrowserMatchResult.Match(Ra2FieldBrowserMatchSource.Key, 4, displayInfo.Key);

        if (Contains(displayInfo.DisplayName, normalizedQuery))
            return Ra2FieldBrowserMatchResult.Match(Ra2FieldBrowserMatchSource.DisplayName, 5, displayInfo.DisplayName);

        string? aliasContains = displayInfo.Aliases.FirstOrDefault(alias => Contains(alias, normalizedQuery));
        if (aliasContains is not null)
            return Ra2FieldBrowserMatchResult.Match(Ra2FieldBrowserMatchSource.Alias, 6, aliasContains);

        if (Contains(displayInfo.Note, normalizedQuery))
            return Ra2FieldBrowserMatchResult.Match(Ra2FieldBrowserMatchSource.Note, 7, displayInfo.Note);

        if (Contains(displayInfo.Description, normalizedQuery))
            return Ra2FieldBrowserMatchResult.Match(Ra2FieldBrowserMatchSource.Description, 8, displayInfo.Description);

        return Ra2FieldBrowserMatchResult.NoMatch;
    }

    private static bool StartsWith(string? value, string query)
        => !string.IsNullOrEmpty(value) && value.StartsWith(query, StringComparison.OrdinalIgnoreCase);

    private static bool Contains(string? value, string query)
        => !string.IsNullOrEmpty(value) && value.Contains(query, StringComparison.OrdinalIgnoreCase);
}

internal enum Ra2FieldBrowserMatchSource
{
    None,
    Key,
    DisplayName,
    Alias,
    Note,
    Description,
    Recent,
}

internal sealed class Ra2FieldBrowserMatchResult
{
    public static Ra2FieldBrowserMatchResult EmptyQuery { get; } = new(true, Ra2FieldBrowserMatchSource.None, 0, null);

    public static Ra2FieldBrowserMatchResult NoMatch { get; } = new(false, Ra2FieldBrowserMatchSource.None, int.MaxValue, null);

    public static Ra2FieldBrowserMatchResult Recent { get; } = new(true, Ra2FieldBrowserMatchSource.Recent, 9, null);

    private Ra2FieldBrowserMatchResult(
        bool isMatch,
        Ra2FieldBrowserMatchSource source,
        int priority,
        string? matchedText)
    {
        IsMatch = isMatch;
        Source = source;
        Priority = priority;
        MatchedText = matchedText;
    }

    public bool IsMatch { get; }

    public Ra2FieldBrowserMatchSource Source { get; }

    public int Priority { get; }

    public string? MatchedText { get; }

    public static Ra2FieldBrowserMatchResult Match(
        Ra2FieldBrowserMatchSource source,
        int priority,
        string? matchedText)
        => new(true, source, priority, matchedText);
}
