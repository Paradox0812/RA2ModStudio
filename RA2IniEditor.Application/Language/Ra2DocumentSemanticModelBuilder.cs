using RA2IniEditor.Core.Schema;
using RA2IniEditor.Application.Classification;

namespace RA2IniEditor.Application.Language;

internal sealed class Ra2DocumentSemanticModelBuilder : IRa2DocumentSemanticModelBuilder
{
    private static readonly HashSet<string> WeaponReferenceKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Primary",
        "Secondary",
        "ElitePrimary",
        "EliteSecondary",
        "DeathWeapon",
        "OpenToppedWeapon"
    };

    private static readonly HashSet<string> IgnoredReferenceValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "empty",
        "none",
        "<none>",
        "null",
        "true",
        "false",
        "yes",
        "no"
    };

    private readonly IRa2SectionClassifier _sectionClassifier;

    static Ra2DocumentSemanticModelBuilder()
    {
        for (int index = 1; index <= 10; index++)
            WeaponReferenceKeys.Add($"Weapon{index}");
    }

    public Ra2DocumentSemanticModelBuilder()
        : this(new Ra2SectionClassifier())
    {
    }

    public Ra2DocumentSemanticModelBuilder(IRa2SectionClassifier sectionClassifier)
    {
        _sectionClassifier = sectionClassifier ?? throw new ArgumentNullException(nameof(sectionClassifier));
    }

    public Ra2DocumentSemanticModel Build(Ra2DocumentSnapshot snapshot, IRa2FieldDefinitionProvider fieldProvider)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(fieldProvider);

        Ra2SectionClassificationResult classification = _sectionClassifier.Classify(
            Ra2IniLineParser.NormalizeSectionHeadersForClassification(snapshot.Text));
        List<ParsedSection> parsedSections = ParseSections(snapshot.Text);
        List<Ra2SectionSymbol> sections = BuildSectionSymbols(snapshot.Text, parsedSections, classification);
        List<Ra2KeyValueSymbol> keyValues = BuildKeyValues(parsedSections, classification, fieldProvider);
        List<Ra2ValueReferenceSymbol> references = BuildReferences(keyValues);
        return new Ra2DocumentSemanticModel(snapshot, classification, sections, keyValues, references);
    }

    private static List<Ra2SectionSymbol> BuildSectionSymbols(
        string text,
        IReadOnlyList<ParsedSection> parsedSections,
        Ra2SectionClassificationResult classification)
    {
        List<Ra2SectionSymbol> sections = [];
        for (int index = 0; index < parsedSections.Count; index++)
        {
            ParsedSection section = parsedSections[index];
            int bodyEnd = index + 1 < parsedSections.Count
                ? parsedSections[index + 1].LineSpan.Start
                : text.Length;
            Ra2SectionKind kind = ResolveSectionKind(classification, section.Name);
            Ra2TextSpan bodySpan = new(section.HeaderSpan.End, Math.Max(0, bodyEnd - section.HeaderSpan.End));
            sections.Add(new Ra2SectionSymbol(
                section.Name,
                kind,
                section.HeaderLineNumber,
                section.HeaderSpan,
                bodySpan,
                section.InlineComment,
                IsPrecedingCommentSupported(kind) ? section.PrecedingComment : null));
        }

        return sections;
    }

    private static List<Ra2KeyValueSymbol> BuildKeyValues(
        IEnumerable<ParsedSection> parsedSections,
        Ra2SectionClassificationResult classification,
        IRa2FieldDefinitionProvider fieldProvider)
    {
        List<Ra2KeyValueSymbol> keyValues = [];
        foreach (ParsedSection section in parsedSections)
        {
            Ra2SectionKind sectionKind = ResolveSectionKind(classification, section.Name);
            foreach (ParsedKeyValue keyValue in section.KeyValues)
            {
                bool isKnownKey = fieldProvider.IsKnownField(sectionKind, keyValue.Key) ||
                                  sectionKind == Ra2SectionKind.Unknown &&
                                  fieldProvider.IsKnownField(Ra2SectionKind.Unknown, keyValue.Key);
                keyValues.Add(new Ra2KeyValueSymbol(
                    section.Name,
                    sectionKind,
                    keyValue.Key,
                    keyValue.Value,
                    keyValue.RawValue,
                    keyValue.InlineComment,
                    keyValue.LineNumber,
                    keyValue.LineSpan,
                    keyValue.KeySpan,
                    keyValue.ValueSpan,
                    isKnownKey));
            }
        }

        return keyValues;
    }

    private static List<Ra2ValueReferenceSymbol> BuildReferences(IEnumerable<Ra2KeyValueSymbol> keyValues)
    {
        List<Ra2ValueReferenceSymbol> references = [];
        foreach (Ra2KeyValueSymbol keyValue in keyValues)
        {
            if (keyValue.ValueSpan is not Ra2TextSpan valueSpan ||
                !TryGetReferenceTarget(keyValue, valueSpan, out string? targetSectionName, out Ra2SectionKind targetKind, out Ra2ValueReferenceKind referenceKind, out Ra2TextSpan referenceTokenSpan))
            {
                continue;
            }

            references.Add(new Ra2ValueReferenceSymbol(
                keyValue.SectionName,
                keyValue.Key,
                targetSectionName,
                targetKind,
                referenceKind,
                keyValue.LineNumber,
                referenceTokenSpan,
                keyValue.InlineComment));
        }

        return references;
    }

    private static bool TryGetReferenceTarget(
        Ra2KeyValueSymbol keyValue,
        Ra2TextSpan valueSpan,
        out string targetSectionName,
        out Ra2SectionKind targetKind,
        out Ra2ValueReferenceKind referenceKind,
        out Ra2TextSpan referenceTokenSpan)
    {
        targetSectionName = string.Empty;
        targetKind = Ra2SectionKind.Unknown;
        referenceKind = Ra2ValueReferenceKind.Unknown;
        referenceTokenSpan = default;

        if (!TryGetReferenceToken(keyValue.Value, valueSpan, out string referenceToken, out referenceTokenSpan))
            return false;

        if (WeaponReferenceKeys.Contains(keyValue.Key))
        {
            targetSectionName = referenceToken;
            targetKind = Ra2SectionKind.Weapon;
            referenceKind = Ra2ValueReferenceKind.WeaponReference;
            return true;
        }

        if (keyValue.SectionKind == Ra2SectionKind.Weapon &&
            string.Equals(keyValue.Key, "Projectile", StringComparison.OrdinalIgnoreCase))
        {
            targetSectionName = referenceToken;
            targetKind = Ra2SectionKind.Projectile;
            referenceKind = Ra2ValueReferenceKind.ProjectileReference;
            return true;
        }

        if (keyValue.SectionKind == Ra2SectionKind.Weapon &&
            string.Equals(keyValue.Key, "Warhead", StringComparison.OrdinalIgnoreCase))
        {
            targetSectionName = referenceToken;
            targetKind = Ra2SectionKind.Warhead;
            referenceKind = Ra2ValueReferenceKind.WarheadReference;
            return true;
        }

        return false;
    }

    private static bool TryGetReferenceToken(
        string? rawValue,
        Ra2TextSpan valueSpan,
        out string referenceToken,
        out Ra2TextSpan referenceTokenSpan)
    {
        referenceToken = string.Empty;
        referenceTokenSpan = default;
        if (!Ra2IniLineParser.TryGetFirstValueToken(rawValue, valueSpan, out string token, out Ra2TextSpan tokenSpan))
            return false;

        if (string.IsNullOrWhiteSpace(token) ||
            IgnoredReferenceValues.Contains(token) ||
            Ra2IniLineParser.IsNumericLiteral(token))
        {
            return false;
        }

        referenceToken = token;
        referenceTokenSpan = tokenSpan;
        return true;
    }

    private static Ra2SectionKind ResolveSectionKind(Ra2SectionClassificationResult classification, string sectionName)
        => classification.SectionKindsByName.TryGetValue(sectionName, out Ra2SectionKind kind)
            ? kind
            : Ra2SectionKind.Unknown;

    private static List<ParsedSection> ParseSections(string text)
    {
        List<ParsedSection> sections = [];
        ParsedSection? currentSection = null;
        List<string> precedingCommentLines = [];
        bool precedingCommentBlockTooLong = false;
        int lineStart = 0;
        int lineNumber = 1;

        while (lineStart < text.Length)
        {
            int lineEnd = Ra2IniLineParser.FindLineEnd(text, lineStart);
            Ra2TextSpan lineSpan = new(lineStart, lineEnd - lineStart);
            ParsedSection? section = TryParseSectionHeader(text, lineStart, lineEnd, lineNumber, lineSpan);
            if (section is not null)
            {
                if (!precedingCommentBlockTooLong)
                    section.PrecedingComment = CreatePrecedingComment(precedingCommentLines);

                currentSection = section;
                sections.Add(section);
                precedingCommentLines.Clear();
                precedingCommentBlockTooLong = false;
            }
            else if (TryParsePrecedingCommentLine(text, lineStart, lineEnd, out string? commentText))
            {
                if (precedingCommentLines.Count >= 2)
                    precedingCommentBlockTooLong = true;
                else
                    precedingCommentLines.Add(commentText);
            }
            else if (currentSection is not null)
            {
                ParsedKeyValue? keyValue = TryParseKeyValue(text, lineStart, lineEnd, lineNumber, lineSpan);
                if (keyValue is not null)
                    currentSection.KeyValues.Add(keyValue);

                precedingCommentLines.Clear();
                precedingCommentBlockTooLong = false;
            }
            else
            {
                precedingCommentLines.Clear();
                precedingCommentBlockTooLong = false;
            }

            lineStart = Ra2IniLineParser.MoveToNextLine(text, lineEnd);
            lineNumber++;
        }

        return sections;
    }

    private static ParsedSection? TryParseSectionHeader(
        string text,
        int lineStart,
        int lineEnd,
        int lineNumber,
        Ra2TextSpan lineSpan)
    {
        if (!Ra2IniLineParser.TryParseSectionHeader(
            text,
            lineStart,
            lineEnd,
            out Ra2IniLineParser.ParsedSectionHeader header))
        {
            return null;
        }

        return new ParsedSection(
            header.Name,
            lineNumber,
            header.HeaderSpan,
            lineSpan,
            ExtractInlineComment(text, header.HeaderSpan.End, lineEnd));
    }

    private static ParsedKeyValue? TryParseKeyValue(
        string text,
        int lineStart,
        int lineEnd,
        int lineNumber,
        Ra2TextSpan lineSpan)
    {
        if (!Ra2IniLineParser.TryParseKeyValue(
            text,
            lineStart,
            lineEnd,
            out Ra2IniLineParser.ParsedKeyValueLine parsedKeyValue))
        {
            return null;
        }

        return new ParsedKeyValue(
            parsedKeyValue.Key,
            parsedKeyValue.Value,
            ExtractRawValue(text, lineStart, lineEnd),
            ExtractInlineComment(text, parsedKeyValue.ValueSpan?.End ?? FindValueStart(text, lineStart, lineEnd), lineEnd),
            lineNumber,
            lineSpan,
            parsedKeyValue.KeySpan,
            parsedKeyValue.ValueSpan);
    }

    private static string ExtractRawValue(string text, int lineStart, int lineEnd)
    {
        int equalsIndex = IndexOf(text, '=', lineStart, lineEnd);
        if (equalsIndex < 0)
            return string.Empty;

        return text[(equalsIndex + 1)..lineEnd].Trim();
    }

    private static int FindValueStart(string text, int lineStart, int lineEnd)
    {
        int equalsIndex = IndexOf(text, '=', lineStart, lineEnd);
        return equalsIndex < 0 ? lineStart : equalsIndex + 1;
    }

    private static string? ExtractInlineComment(string text, int searchStart, int lineEnd)
    {
        int commentStart = Ra2IniLineParser.FindInlineCommentStart(text, searchStart, lineEnd, includeHash: true);
        return Ra2IniLineParser.ExtractInlineCommentText(text, commentStart, lineEnd);
    }

    private static bool TryParsePrecedingCommentLine(
        string text,
        int lineStart,
        int lineEnd,
        out string commentText)
    {
        commentText = string.Empty;
        int firstNonWhite = FindFirstNonWhite(text, lineStart, lineEnd);
        if (firstNonWhite >= lineEnd || text[firstNonWhite] != ';')
            return false;

        string candidate = text[(firstNonWhite + 1)..lineEnd].Trim();
        if (string.IsNullOrWhiteSpace(candidate))
            return false;

        commentText = candidate;
        return true;
    }

    private static string? CreatePrecedingComment(IReadOnlyList<string> commentLines)
    {
        if (commentLines.Count is < 1 or > 2)
            return null;

        foreach (string line in commentLines)
        {
            if (!IsAcceptablePrecedingCommentLine(line))
                return null;
        }

        return string.Join(" / ", commentLines);
    }

    private static bool IsAcceptablePrecedingCommentLine(string comment)
    {
        if (comment.Length > 100)
            return false;

        if (ContainsSeparatorRun(comment))
            return false;

        if (LooksLikeSectionHeadingOrParagraph(comment))
            return false;

        return true;
    }

    private static bool ContainsSeparatorRun(string comment)
    {
        int runLength = 0;
        char runChar = '\0';
        foreach (char ch in comment)
        {
            if (ch is '*' or '-' or '=' or '_')
            {
                runLength = ch == runChar ? runLength + 1 : 1;
                runChar = ch;
                if (runLength >= 3)
                    return true;
            }
            else
            {
                runLength = 0;
                runChar = '\0';
            }
        }

        return false;
    }

    private static bool LooksLikeSectionHeadingOrParagraph(string comment)
    {
        string lower = comment.ToLowerInvariant();
        if (lower.Contains(" rules", StringComparison.Ordinal) ||
            lower.Contains(" controls", StringComparison.Ordinal) ||
            lower.Contains(" section", StringComparison.Ordinal) ||
            lower.Contains(" list", StringComparison.Ordinal) ||
            lower.Contains(" types", StringComparison.Ordinal))
        {
            return true;
        }

        int wordCount = comment.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
        return wordCount > 12;
    }

    private static bool IsPrecedingCommentSupported(Ra2SectionKind kind)
        => kind is Ra2SectionKind.Infantry or
            Ra2SectionKind.Vehicle or
            Ra2SectionKind.Aircraft or
            Ra2SectionKind.Building or
            Ra2SectionKind.Weapon or
            Ra2SectionKind.Projectile or
            Ra2SectionKind.Warhead or
            Ra2SectionKind.Animation or
            Ra2SectionKind.VoxelAnimation or
            Ra2SectionKind.Particle or
            Ra2SectionKind.ParticleSystem or
            Ra2SectionKind.SuperWeapon or
            Ra2SectionKind.Terrain or
            Ra2SectionKind.Overlay or
            Ra2SectionKind.ArtObject or
            Ra2SectionKind.Sound or
            Ra2SectionKind.Shield or
            Ra2SectionKind.AttachEffect or
            Ra2SectionKind.LaserTrail or
            Ra2SectionKind.DigitalDisplay or
            Ra2SectionKind.Banner or
            Ra2SectionKind.Insignia or
            Ra2SectionKind.Tiberium or
            Ra2SectionKind.Radiation;

    private static int IndexOf(string text, char value, int start, int end)
    {
        for (int index = start; index < end; index++)
        {
            if (text[index] == value)
                return index;
        }

        return -1;
    }

    private static int FindFirstNonWhite(string text, int start, int end)
    {
        int index = start;
        while (index < end && char.IsWhiteSpace(text[index]))
            index++;

        return index;
    }

    private sealed class ParsedSection
    {
        public ParsedSection(
            string name,
            int headerLineNumber,
            Ra2TextSpan headerSpan,
            Ra2TextSpan lineSpan,
            string? inlineComment)
        {
            Name = name;
            HeaderLineNumber = headerLineNumber;
            HeaderSpan = headerSpan;
            LineSpan = lineSpan;
            InlineComment = inlineComment;
        }

        public string Name { get; }

        public int HeaderLineNumber { get; }

        public Ra2TextSpan HeaderSpan { get; }

        public Ra2TextSpan LineSpan { get; }

        public string? InlineComment { get; }

        public string? PrecedingComment { get; set; }

        public List<ParsedKeyValue> KeyValues { get; } = [];
    }

    private sealed class ParsedKeyValue
    {
        public ParsedKeyValue(
            string key,
            string value,
            string rawValue,
            string? inlineComment,
            int lineNumber,
            Ra2TextSpan lineSpan,
            Ra2TextSpan keySpan,
            Ra2TextSpan? valueSpan)
        {
            Key = key;
            Value = value;
            RawValue = rawValue;
            InlineComment = inlineComment;
            LineNumber = lineNumber;
            LineSpan = lineSpan;
            KeySpan = keySpan;
            ValueSpan = valueSpan;
        }

        public string Key { get; }

        public string Value { get; }

        public string RawValue { get; }

        public string? InlineComment { get; }

        public int LineNumber { get; }

        public Ra2TextSpan LineSpan { get; }

        public Ra2TextSpan KeySpan { get; }

        public Ra2TextSpan? ValueSpan { get; }
    }
}
