using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Language;

internal enum Ra2ReferenceValueDetailStatus
{
    NotReferenceValue,
    Available,
    MissingTarget
}

internal sealed class Ra2ReferenceValueDetailRequest
{
    public Ra2ReferenceValueDetailRequest(
        Ra2DocumentSemanticModel model,
        int offset,
        Ra2TextSpan? selectionSpan = null)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Offset = offset;
        SelectionSpan = selectionSpan;
    }

    public Ra2DocumentSemanticModel Model { get; }

    public int Offset { get; }

    public Ra2TextSpan? SelectionSpan { get; }
}

internal sealed class Ra2ReferenceValueDetailResult
{
    private Ra2ReferenceValueDetailResult(
        Ra2ReferenceValueDetailStatus status,
        Ra2ValueReferenceSymbol? reference,
        Ra2SectionSymbol? targetSection,
        string? inlineComment,
        Ra2DefinitionTarget? target)
    {
        Status = status;
        Reference = reference;
        TargetSection = targetSection;
        InlineComment = inlineComment;
        Target = target;
    }

    public Ra2ReferenceValueDetailStatus Status { get; }

    public bool Success => Status is Ra2ReferenceValueDetailStatus.Available or Ra2ReferenceValueDetailStatus.MissingTarget;

    public Ra2ValueReferenceSymbol? Reference { get; }

    public Ra2SectionSymbol? TargetSection { get; }

    public string? InlineComment { get; }

    public Ra2DefinitionTarget? Target { get; }

    public static Ra2ReferenceValueDetailResult NotReferenceValue { get; } =
        new(Ra2ReferenceValueDetailStatus.NotReferenceValue, null, null, null, null);

    public static Ra2ReferenceValueDetailResult Create(
        Ra2ReferenceValueDetailStatus status,
        Ra2ValueReferenceSymbol reference,
        Ra2SectionSymbol? targetSection,
        string? inlineComment,
        Ra2DefinitionTarget target)
        => new(status, reference, targetSection, inlineComment, target);
}

internal sealed class Ra2ReferenceValueDetailService
{
    private static readonly IReadOnlyDictionary<Ra2SectionKind, string[]> PreferredSummaryKeys =
        new Dictionary<Ra2SectionKind, string[]>
        {
            [Ra2SectionKind.Weapon] = ["Damage", "ROF", "Range", "Projectile", "Warhead", "Report"],
            [Ra2SectionKind.Projectile] = ["AA", "AG", "ROT", "Image", "Shadow", "SubjectToCliffs", "SubjectToElevation", "SubjectToWalls"],
            [Ra2SectionKind.Warhead] = ["Verses", "InfDeath", "AnimList", "CellSpread", "PercentAtMax"]
        };

    public Ra2ReferenceValueDetailResult Resolve(Ra2ReferenceValueDetailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        int offset = Math.Clamp(request.Offset, 0, request.Model.Snapshot.Text.Length);
        Ra2ValueReferenceSymbol? reference = request.SelectionSpan is Ra2TextSpan selectionSpan
            ? FindReferenceFromSelection(request.Model, selectionSpan)
            : FindReferenceAtOffset(request.Model, offset);
        if (reference is null)
            return Ra2ReferenceValueDetailResult.NotReferenceValue;

        Ra2SectionSymbol? targetSection = request.Model.FindSectionByName(reference.TargetSectionName);
        string? inlineComment = reference.InlineComment;
        Ra2DefinitionTarget target = CreateTarget(request.Model, reference, targetSection, inlineComment);
        Ra2ReferenceValueDetailStatus status = targetSection is null
            ? Ra2ReferenceValueDetailStatus.MissingTarget
            : Ra2ReferenceValueDetailStatus.Available;
        return Ra2ReferenceValueDetailResult.Create(status, reference, targetSection, inlineComment, target);
    }

    public Ra2HoverInfo? CreateHoverInfo(Ra2ReferenceValueDetailResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (!result.Success || result.Reference is null || result.Target is null)
            return null;

        string typeDisplay = result.TargetSection?.Kind.ToString() ?? "\u5f15\u7528\u672a\u627e\u5230";
        return new Ra2HoverInfo(
            result.Reference.TargetSectionName,
            typeDisplay,
            result.Target.Detail,
            CreateCompactHoverDescription(result),
            result.Target.SourceName,
            result.Reference.ValueSpan,
            result.Reference.TargetSectionName,
            result.TargetSection?.DisplayNote,
            typeDisplay);
    }

    private static string? CreateCompactHoverDescription(Ra2ReferenceValueDetailResult result)
    {
        List<string> lines = [];
        if (result.Status == Ra2ReferenceValueDetailStatus.MissingTarget)
            lines.Add("\u5f53\u524d\u6587\u4ef6\u4e2d\u672a\u627e\u5230\u8be5\u5f15\u7528\u76ee\u6807\u3002");

        if (!string.IsNullOrWhiteSpace(result.InlineComment))
            lines.Add($"\u5f15\u7528\u5907\u6ce8: {result.InlineComment}");

        return lines.Count == 0 ? null : string.Join(Environment.NewLine, lines);
    }

    private static Ra2ValueReferenceSymbol? FindReferenceAtOffset(Ra2DocumentSemanticModel model, int offset)
        => model.References.FirstOrDefault(reference => reference.ValueSpan.Contains(offset));

    private static Ra2ValueReferenceSymbol? FindReferenceFromSelection(Ra2DocumentSemanticModel model, Ra2TextSpan selectionSpan)
    {
        if (selectionSpan.Length <= 0 || selectionSpan.End > model.Snapshot.Text.Length)
            return null;

        Ra2KeyValueSymbol? keyValue = model.KeyValues.FirstOrDefault(candidate =>
            candidate.LineSpan.Contains(selectionSpan.Start) &&
            candidate.LineSpan.Contains(Math.Max(selectionSpan.Start, selectionSpan.End - 1)));
        if (keyValue?.ValueSpan is not Ra2TextSpan valueSpan ||
            selectionSpan.Start < valueSpan.Start ||
            selectionSpan.End > valueSpan.End)
        {
            return null;
        }

        string selectedText = model.Snapshot.Text.Substring(selectionSpan.Start, selectionSpan.Length);
        string effectiveValue = GetFirstSelectedReferenceCandidate(selectedText);
        if (string.IsNullOrWhiteSpace(effectiveValue))
            return null;

        return model.References.FirstOrDefault(reference =>
            reference.LineNumber == keyValue.LineNumber &&
            string.Equals(reference.SourceSectionName, keyValue.SectionName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(reference.SourceKey, keyValue.Key, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(reference.TargetSectionName, effectiveValue, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetFirstSelectedReferenceCandidate(string selectedText)
    {
        string effective = Ra2IniLineParser.GetEffectiveValue(selectedText);
        int comma = effective.IndexOf(',');
        if (comma >= 0)
            effective = effective[..comma];

        return effective.Trim();
    }

    private static Ra2DefinitionTarget CreateTarget(
        Ra2DocumentSemanticModel model,
        Ra2ValueReferenceSymbol reference,
        Ra2SectionSymbol? targetSection,
        string? inlineComment)
    {
        string kindText = targetSection?.Kind.ToString() ?? reference.TargetSectionKind.ToString();
        string detail = targetSection is null
            ? $"{kindText} reference target was not found in the current document."
            : $"{kindText} reference target in current document.";
        string description = CreateDescription(model, reference, targetSection, inlineComment);
        return new Ra2DefinitionTarget(
            Ra2DefinitionTargetKind.ReferenceTarget,
            reference.TargetSectionName,
            detail,
            "Current document",
            model.Snapshot.FilePath,
            targetSection?.HeaderSpan,
            targetSection?.HeaderLineNumber,
            description);
    }

    private static string CreateDescription(
        Ra2DocumentSemanticModel model,
        Ra2ValueReferenceSymbol reference,
        Ra2SectionSymbol? targetSection,
        string? inlineComment)
    {
        List<string> lines = [];
        if (targetSection is null)
        {
            lines.Add("\u5f53\u524d\u6587\u4ef6\u4e2d\u672a\u627e\u5230\u8be5\u5f15\u7528\u76ee\u6807\u3002");
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(targetSection.DisplayNote))
                lines.Add($"\u76ee\u6807\u5907\u6ce8: {targetSection.DisplayNote}");

            lines.AddRange(GetSummaryLines(model, targetSection));
        }

        if (!string.IsNullOrWhiteSpace(inlineComment))
            lines.Add($"\u5f15\u7528\u5907\u6ce8: {inlineComment}");

        if (targetSection is not null)
            lines.Add($"\u4f4d\u7f6e: Line {targetSection.HeaderLineNumber}");

        return string.Join(Environment.NewLine, lines);
    }

    private static IEnumerable<string> GetSummaryLines(Ra2DocumentSemanticModel model, Ra2SectionSymbol targetSection)
    {
        List<Ra2KeyValueSymbol> keyValues = model.KeyValues
            .Where(keyValue => string.Equals(keyValue.SectionName, targetSection.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (keyValues.Count == 0)
            return [];

        if (!PreferredSummaryKeys.TryGetValue(targetSection.Kind, out string[]? preferredKeys))
            return keyValues.Take(6).Select(FormatSummaryLine);

        List<Ra2KeyValueSymbol> ordered = [];
        foreach (string key in preferredKeys)
        {
            Ra2KeyValueSymbol? match = keyValues.FirstOrDefault(value =>
                string.Equals(value.Key, key, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                ordered.Add(match);
        }

        foreach (Ra2KeyValueSymbol keyValue in keyValues)
        {
            if (ordered.Count >= 6)
                break;

            if (!ordered.Contains(keyValue))
                ordered.Add(keyValue);
        }

        return ordered.Select(FormatSummaryLine);
    }

    private static string FormatSummaryLine(Ra2KeyValueSymbol keyValue)
        => $"{keyValue.Key}={keyValue.Value}";
}
