using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldTrust;

namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2CompletionProvider : IRa2CompletionProvider
{
    private readonly IRa2FieldValueCompletionCatalog _valueCompletionCatalog;

    private static readonly HashSet<string> WeaponReferenceKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Primary",
        "Secondary",
        "ElitePrimary",
        "EliteSecondary",
        "DeathWeapon",
        "OpenToppedWeapon"
    };

    static Ra2CompletionProvider()
    {
        for (int index = 1; index <= 10; index++)
            WeaponReferenceKeys.Add($"Weapon{index}");
    }

    public Ra2CompletionProvider()
        : this(new CompositeRa2FieldValueCompletionCatalog([
            new FieldRegistryRa2FieldValueCompletionCatalog(),
            new BuiltInRa2FieldValueCompletionCatalog()
        ]))
    {
    }

    internal Ra2CompletionProvider(IRa2FieldValueCompletionCatalog valueCompletionCatalog)
    {
        _valueCompletionCatalog = valueCompletionCatalog ?? throw new ArgumentNullException(nameof(valueCompletionCatalog));
    }

    public Ra2CompletionResult GetCompletions(Ra2CompletionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (IsNoCompletionRegion(request))
            return Ra2CompletionResult.EmptyAt(request.CaretOffset);

        if (TryCreateLineKeyValueContext(
            request,
            out Ra2SectionKind lineSectionKind,
            out string lineKey,
            out Ra2TextSpan lineKeySpan,
            out Ra2TextSpan? lineValueSpan,
            out bool isValueContext))
        {
            return isValueContext
                ? GetValueCompletions(request, lineSectionKind, lineKey, lineValueSpan)
                : GetKeyCompletions(request, lineSectionKind, lineKeySpan);
        }

        if (TryCreateImplicitKeyContext(request, out Ra2SectionKind sectionKind, out Ra2TextSpan keyPrefixSpan))
            return GetKeyCompletions(request, sectionKind, keyPrefixSpan);

        return Ra2CompletionResult.EmptyAt(request.CaretOffset);
    }

    private static bool IsNoCompletionRegion(Ra2CompletionRequest request)
        => request.CaretContext.Region is Ra2CaretRegion.Comment or Ra2CaretRegion.SectionHeader;

    private static Ra2CompletionResult GetKeyCompletions(
        Ra2CompletionRequest request,
        Ra2SectionKind sectionKind,
        Ra2TextSpan keyPrefixSpan)
    {
        if (sectionKind == Ra2SectionKind.Unknown)
            return new Ra2CompletionResult([], keyPrefixSpan);

        string prefix = Slice(request.Snapshot.Text, keyPrefixSpan);
        bool appendEqualsSuffix = !IsSpanFollowedByEquals(request.Snapshot.Text, keyPrefixSpan);
        IReadOnlyList<Ra2CompletionItem> items = request.FieldProvider
            .GetFields(sectionKind)
            .Where(field => StartsWithPrefix(field.Key, prefix))
            .Where(ShouldOfferKeyCompletion)
            .Select(field => new Ra2CompletionItem(
                field.Key,
                Ra2CompletionItemKind.Key,
                $"Type: {field.EditorKind}",
                field.Description,
                appendEqualsSuffix ? $"{field.Key}=" : field.Key,
                priority: 100,
                sourceKind: Ra2CompletionItemSourceKind.FieldRegistry))
            .ToArray();

        return new Ra2CompletionResult(SortAndDeduplicate(items), keyPrefixSpan);
    }

    private static bool ShouldOfferKeyCompletion(Ra2FieldDefinition field)
    {
        Ra2FieldTrustLevel trustLevel = Ra2FieldTrustClassifier.Classify(field).Level;
        return trustLevel is not Ra2FieldTrustLevel.VerifiedGuardrail and
            not Ra2FieldTrustLevel.Obsolete and
            not Ra2FieldTrustLevel.NonExistent and
            not Ra2FieldTrustLevel.PseudoField;
    }

    private Ra2CompletionResult GetValueCompletions(
        Ra2CompletionRequest request,
        Ra2SectionKind sectionKind,
        string key,
        Ra2TextSpan? valueSpan)
    {
        if (TryGetTargetKind(sectionKind, key, out Ra2SectionKind targetKind))
            return GetValueReferenceCompletions(request, targetKind, valueSpan);

        if (!TryGetValueReplacementSpan(
                request,
                valueSpan,
                out Ra2TextSpan replacementSpan,
                out Ra2ValueCompletionContext context))
        {
            return Ra2CompletionResult.EmptyAt(request.CaretOffset);
        }

        if (!string.IsNullOrWhiteSpace(context.CurrentTokenPrefix) &&
            Ra2IniLineParser.IsNumericLiteral(context.CurrentTokenPrefix))
        {
            return new Ra2CompletionResult([], replacementSpan);
        }

        request.FieldProvider.TryGetField(sectionKind, key, out Ra2FieldDefinition? definition);
        IReadOnlyList<Ra2CompletionItem> items = _valueCompletionCatalog
            .GetCandidates(new Ra2FieldValueCompletionRequest(sectionKind, key, definition, context))
            .Select(ToCompletionItem)
            .ToArray();

        return new Ra2CompletionResult(SortAndDeduplicate(items), replacementSpan);
    }

    private static Ra2CompletionResult GetValueReferenceCompletions(
        Ra2CompletionRequest request,
        Ra2SectionKind targetKind,
        Ra2TextSpan? valueSpan)
    {
        if (!TryGetReferenceReplacementSpan(request, valueSpan, out Ra2TextSpan replacementSpan, out string prefix))
            return Ra2CompletionResult.EmptyAt(request.CaretOffset);

        if (IsExplicitNumericLiteralPrefix(prefix))
            return new Ra2CompletionResult([], replacementSpan);

        IReadOnlyList<Ra2CompletionItem> items = request.SemanticModel.Sections
            .Where(section => section.Kind == targetKind || section.Kind == Ra2SectionKind.Unknown)
            .Where(section => StartsWithPrefix(section.Name, prefix))
            .Select(section => new Ra2CompletionItem(
                section.Name,
                Ra2CompletionItemKind.Reference,
                section.Kind == Ra2SectionKind.Unknown
                    ? "Unclassified section fallback"
                    : $"{targetKind} section",
                CreateSectionCompletionDocumentation(section),
                section.Name,
                priority: section.Kind == Ra2SectionKind.Unknown ? 10 : 100,
                sourceKind: section.Kind == Ra2SectionKind.Unknown
                    ? Ra2CompletionItemSourceKind.CurrentDocumentUnknownFallback
                    : Ra2CompletionItemSourceKind.CurrentDocumentSection))
            .ToArray();

        return new Ra2CompletionResult(SortAndDeduplicate(items), replacementSpan);
    }

    private static Ra2CompletionItem ToCompletionItem(Ra2FieldValueCompletionCandidate candidate)
    {
        return new Ra2CompletionItem(
            candidate.Value,
            candidate.Kind,
            string.IsNullOrWhiteSpace(candidate.DisplayName) ? null : $"Type: {candidate.DisplayName}",
            candidate.Description,
            candidate.Value,
            candidate.Priority,
            ToCompletionSourceKind(candidate.SourceKind));
    }

    private static Ra2CompletionItemSourceKind ToCompletionSourceKind(
        Ra2FieldValueCompletionSourceKind sourceKind)
    {
        return sourceKind switch
        {
            Ra2FieldValueCompletionSourceKind.FieldRegistry => Ra2CompletionItemSourceKind.FieldRegistry,
            Ra2FieldValueCompletionSourceKind.User => Ra2CompletionItemSourceKind.UserValueCatalog,
            Ra2FieldValueCompletionSourceKind.Project => Ra2CompletionItemSourceKind.ProjectValueCatalog,
            Ra2FieldValueCompletionSourceKind.CurrentDocumentInference => Ra2CompletionItemSourceKind.CurrentDocumentInference,
            _ => Ra2CompletionItemSourceKind.BuiltInValueCatalog
        };
    }

    private static bool TryGetTargetKind(Ra2SectionKind sectionKind, string key, out Ra2SectionKind targetKind)
    {
        targetKind = Ra2SectionKind.Unknown;
        if (WeaponReferenceKeys.Contains(key))
        {
            targetKind = Ra2SectionKind.Weapon;
            return true;
        }

        if (sectionKind == Ra2SectionKind.Weapon &&
            string.Equals(key, "Projectile", StringComparison.OrdinalIgnoreCase))
        {
            targetKind = Ra2SectionKind.Projectile;
            return true;
        }

        if (sectionKind == Ra2SectionKind.Weapon &&
            string.Equals(key, "Warhead", StringComparison.OrdinalIgnoreCase))
        {
            targetKind = Ra2SectionKind.Warhead;
            return true;
        }

        return false;
    }

    private static bool TryGetReferenceReplacementSpan(
        Ra2CompletionRequest request,
        Ra2TextSpan? valueSpan,
        out Ra2TextSpan replacementSpan,
        out string prefix)
    {
        replacementSpan = default;
        prefix = string.Empty;
        if (valueSpan is not Ra2TextSpan span)
        {
            replacementSpan = new Ra2TextSpan(request.CaretOffset, 0);
            return true;
        }

        int valueRelativeOffset = request.CaretOffset - span.Start;
        if (valueRelativeOffset < 0 || valueRelativeOffset > span.Length)
            return false;

        string value = Slice(request.Snapshot.Text, span);
        int commaIndex = value.IndexOf(',');
        if (commaIndex >= 0 && valueRelativeOffset > commaIndex)
            return false;

        int tokenEnd = commaIndex >= 0 ? commaIndex : value.Length;
        int prefixLength = Math.Min(valueRelativeOffset, tokenEnd);
        string rawPrefix = value[..prefixLength];
        int firstNonWhite = 0;
        while (firstNonWhite < rawPrefix.Length && char.IsWhiteSpace(rawPrefix[firstNonWhite]))
            firstNonWhite++;

        int lastNonWhite = rawPrefix.Length;
        while (lastNonWhite > firstNonWhite && char.IsWhiteSpace(rawPrefix[lastNonWhite - 1]))
            lastNonWhite--;

        prefix = rawPrefix[firstNonWhite..lastNonWhite];
        replacementSpan = new Ra2TextSpan(span.Start + firstNonWhite, lastNonWhite - firstNonWhite);
        return true;
    }

    private static bool TryGetValueReplacementSpan(
        Ra2CompletionRequest request,
        Ra2TextSpan? valueSpan,
        out Ra2TextSpan replacementSpan,
        out Ra2ValueCompletionContext context)
    {
        replacementSpan = default;
        context = null!;
        if (valueSpan is not Ra2TextSpan span)
        {
            replacementSpan = new Ra2TextSpan(request.CaretOffset, 0);
            context = new Ra2ValueCompletionContext(string.Empty, string.Empty, false, []);
            return true;
        }

        int valueRelativeOffset = request.CaretOffset - span.Start;
        if (valueRelativeOffset < 0 || valueRelativeOffset > span.Length)
            return false;

        string value = Slice(request.Snapshot.Text, span);
        ResolveCurrentValueToken(
            value,
            valueRelativeOffset,
            out int tokenRawStart,
            out int tokenRawEnd);

        int tokenStart = FindFirstNonWhite(value, tokenRawStart, tokenRawEnd);
        int tokenEnd = FindLastNonWhiteExclusive(value, tokenStart, tokenRawEnd);
        int prefixEnd = Math.Clamp(valueRelativeOffset, tokenStart, tokenEnd);
        string prefix = tokenStart < prefixEnd
            ? value[tokenStart..prefixEnd]
            : string.Empty;

        replacementSpan = new Ra2TextSpan(span.Start + tokenStart, tokenEnd - tokenStart);
        context = new Ra2ValueCompletionContext(
            value,
            prefix,
            value.Contains(','),
            GetExistingListTokens(value, tokenRawStart, tokenRawEnd));
        return true;
    }

    private static void ResolveCurrentValueToken(
        string value,
        int valueRelativeOffset,
        out int tokenRawStart,
        out int tokenRawEnd)
    {
        tokenRawStart = 0;
        while (true)
        {
            int commaIndex = value.IndexOf(',', tokenRawStart);
            if (commaIndex < 0)
            {
                tokenRawEnd = value.Length;
                return;
            }

            if (valueRelativeOffset <= commaIndex)
            {
                tokenRawEnd = commaIndex;
                return;
            }

            tokenRawStart = commaIndex + 1;
        }
    }

    private static IReadOnlyList<string> GetExistingListTokens(
        string value,
        int currentTokenRawStart,
        int currentTokenRawEnd)
    {
        List<string> tokens = [];
        int tokenRawStart = 0;
        while (tokenRawStart <= value.Length)
        {
            int commaIndex = value.IndexOf(',', tokenRawStart);
            int tokenRawEnd = commaIndex < 0 ? value.Length : commaIndex;
            if (tokenRawStart != currentTokenRawStart || tokenRawEnd != currentTokenRawEnd)
            {
                int tokenStart = FindFirstNonWhite(value, tokenRawStart, tokenRawEnd);
                int tokenEnd = FindLastNonWhiteExclusive(value, tokenStart, tokenRawEnd);
                if (tokenStart < tokenEnd)
                    tokens.Add(value[tokenStart..tokenEnd]);
            }

            if (commaIndex < 0)
                break;

            tokenRawStart = commaIndex + 1;
        }

        return tokens;
    }

    private static bool TryCreateLineKeyValueContext(
        Ra2CompletionRequest request,
        out Ra2SectionKind sectionKind,
        out string key,
        out Ra2TextSpan keySpan,
        out Ra2TextSpan? valueSpan,
        out bool isValueContext)
    {
        sectionKind = Ra2SectionKind.Unknown;
        key = string.Empty;
        keySpan = default;
        valueSpan = null;
        isValueContext = false;

        Ra2SectionSymbol? section = ResolveSectionForCaret(request);
        if (section is null)
            return false;

        int lineStart = FindLineStart(request.Snapshot.Text, request.CaretOffset);
        int lineEnd = Ra2IniLineParser.FindLineEnd(request.Snapshot.Text, lineStart);
        if (!Ra2IniLineParser.TryParseKeyValue(
            request.Snapshot.Text,
            lineStart,
            lineEnd,
            out Ra2IniLineParser.ParsedKeyValueLine parsed))
        {
            return false;
        }

        int equalsIndex = IndexOf(request.Snapshot.Text, '=', lineStart, lineEnd);
        if (equalsIndex < 0)
            return false;

        sectionKind = section.Kind;
        key = parsed.Key;
        keySpan = parsed.KeySpan;
        valueSpan = parsed.ValueSpan;
        isValueContext = request.CaretOffset > equalsIndex;
        if (isValueContext && valueSpan is Ra2TextSpan span && request.CaretOffset > span.End)
            return false;

        return true;
    }

    private static bool TryCreateImplicitKeyContext(
        Ra2CompletionRequest request,
        out Ra2SectionKind sectionKind,
        out Ra2TextSpan keyPrefixSpan)
    {
        sectionKind = Ra2SectionKind.Unknown;
        keyPrefixSpan = new Ra2TextSpan(request.CaretOffset, 0);
        if (request.CaretContext.Region == Ra2CaretRegion.Comment ||
            request.CaretContext.Region == Ra2CaretRegion.SectionHeader)
        {
            return false;
        }

        Ra2SectionSymbol? section = ResolveSectionForCaret(request);
        if (section is null)
            return false;

        int lineStart = FindLineStart(request.Snapshot.Text, request.CaretOffset);
        int lineEnd = Ra2IniLineParser.FindLineEnd(request.Snapshot.Text, lineStart);
        if (Ra2IniLineParser.TryParseSectionHeader(
            request.Snapshot.Text,
            lineStart,
            lineEnd,
            out _))
        {
            return false;
        }

        int equalsIndex = IndexOf(request.Snapshot.Text, '=', lineStart, lineEnd);
        if (equalsIndex >= 0 && request.CaretOffset > equalsIndex)
            return false;

        int prefixStart = lineStart;
        while (prefixStart < request.CaretOffset && char.IsWhiteSpace(request.Snapshot.Text[prefixStart]))
            prefixStart++;

        if (prefixStart > request.CaretOffset)
            return false;

        keyPrefixSpan = new Ra2TextSpan(prefixStart, request.CaretOffset - prefixStart);
        sectionKind = section.Kind;
        return true;
    }

    private static Ra2SectionSymbol? ResolveSectionForCaret(Ra2CompletionRequest request)
    {
        return request.CaretContext.Section ??
               request.SemanticModel.FindSectionAtOffset(request.CaretOffset) ??
               request.SemanticModel.Sections
                   .Where(section => section.HeaderSpan.Start <= request.CaretOffset)
                   .OrderByDescending(section => section.HeaderSpan.Start)
                   .FirstOrDefault();
    }

    private static IReadOnlyList<Ra2CompletionItem> SortAndDeduplicate(IEnumerable<Ra2CompletionItem> items)
    {
        return items
            .GroupBy(item => (item.Kind, item.Label), new CompletionItemIdentityComparer())
            .Select(group => group.OrderByDescending(item => item.Priority).First())
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Kind)
            .ToArray();
    }

    private static bool StartsWithPrefix(string value, string prefix)
        => string.IsNullOrEmpty(prefix) || value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

    private static string CreateSectionCompletionDocumentation(Ra2SectionSymbol section)
    {
        if (string.IsNullOrWhiteSpace(section.DisplayNote))
            return $"Line {section.HeaderLineNumber}";

        string comment = section.DisplayNote.Trim();
        return comment.Length <= 80 ? comment : $"{comment[..80]}...";
    }

    private static bool IsExplicitNumericLiteralPrefix(string prefix)
    {
        return !string.IsNullOrWhiteSpace(prefix) &&
               (prefix[0] is '+' or '-' || prefix.Contains('.', StringComparison.Ordinal)) &&
               Ra2IniLineParser.IsNumericLiteral(prefix);
    }

    private static bool IsSpanFollowedByEquals(string text, Ra2TextSpan span)
    {
        int index = Math.Min(span.End, text.Length);
        int lineEnd = Ra2IniLineParser.FindLineEnd(text, index);
        while (index < lineEnd && char.IsWhiteSpace(text[index]))
            index++;

        return index < lineEnd && text[index] == '=';
    }

    private static string Slice(string text, Ra2TextSpan span)
        => text.Substring(span.Start, span.Length);

    private static int FindLineStart(string text, int offset)
    {
        int index = Math.Min(offset, text.Length);
        while (index > 0 && text[index - 1] is not ('\r' or '\n'))
            index--;

        return index;
    }

    private static int FindFirstNonWhite(string text, int start, int end)
    {
        int index = start;
        while (index < end && char.IsWhiteSpace(text[index]))
            index++;

        return index;
    }

    private static int FindLastNonWhiteExclusive(string text, int start, int end)
    {
        int index = end;
        while (index > start && char.IsWhiteSpace(text[index - 1]))
            index--;

        return index;
    }

    private static int IndexOf(string text, char value, int start, int end)
    {
        for (int index = start; index < end; index++)
        {
            if (text[index] == value)
                return index;
        }

        return -1;
    }

    private sealed class CompletionItemIdentityComparer : IEqualityComparer<(Ra2CompletionItemKind Kind, string Label)>
    {
        public bool Equals((Ra2CompletionItemKind Kind, string Label) x, (Ra2CompletionItemKind Kind, string Label) y)
            => x.Kind == y.Kind && string.Equals(x.Label, y.Label, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((Ra2CompletionItemKind Kind, string Label) obj)
            => HashCode.Combine(obj.Kind, StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Label));
    }
}
