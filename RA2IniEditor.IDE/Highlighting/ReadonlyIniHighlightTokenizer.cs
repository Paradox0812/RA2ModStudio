using System.Globalization;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Classification;

namespace RA2IniEditor.IDE.Highlighting;

/// <summary>
/// Represents a readonly INI highlighting token kind.
/// </summary>
public enum IniHighlightTokenKind
{
    SectionHeader,
    KnownKey,
    UnknownKey,
    Equals,
    Value,
    NumberValue,
    BooleanValue,
    ReferenceValue,
    EnumValue,
    NeutralValue,
    Comment
}

/// <summary>
/// Represents one readonly INI text range that can be highlighted.
/// </summary>
public sealed class IniHighlightToken
{
    public IniHighlightToken(IniHighlightTokenKind kind, int startOffset, int length)
    {
        if (startOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(startOffset));

        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        Kind = kind;
        StartOffset = startOffset;
        Length = length;
    }

    public IniHighlightTokenKind Kind { get; }

    public int StartOffset { get; }

    public int Length { get; }
}

/// <summary>
/// Tokenizes readonly INI text for the IDE source highlighter.
/// </summary>
public sealed class ReadonlyIniHighlightTokenizer
{
    private readonly IRa2FieldDefinitionProvider _fieldProvider;
    private readonly IRa2SectionClassifier _sectionClassifier;

    public ReadonlyIniHighlightTokenizer(IRa2FieldDefinitionProvider fieldProvider)
        : this(fieldProvider, new Ra2SectionClassifier())
    {
    }

    internal ReadonlyIniHighlightTokenizer(IRa2FieldDefinitionProvider fieldProvider, IRa2SectionClassifier sectionClassifier)
    {
        _fieldProvider = fieldProvider ?? throw new ArgumentNullException(nameof(fieldProvider));
        _sectionClassifier = sectionClassifier ?? throw new ArgumentNullException(nameof(sectionClassifier));
    }

    public IReadOnlyList<IniHighlightToken> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<IniHighlightToken>();

        IReadOnlyDictionary<string, Ra2SectionKind> sectionKindIndex = _sectionClassifier.Classify(text).SectionKindsByName;
        List<IniHighlightToken> tokens = new();
        Ra2SectionKind currentSectionKind = Ra2SectionKind.Unknown;
        int lineStart = 0;

        while (lineStart < text.Length)
        {
            int lineEnd = FindLineEnd(text, lineStart);
            TokenizeLine(text, lineStart, lineEnd, sectionKindIndex, ref currentSectionKind, tokens);
            lineStart = MoveToNextLine(text, lineEnd);
        }

        return tokens;
    }

    private void TokenizeLine(
        string text,
        int lineStart,
        int lineEnd,
        IReadOnlyDictionary<string, Ra2SectionKind> sectionKindIndex,
        ref Ra2SectionKind currentSectionKind,
        List<IniHighlightToken> tokens)
    {
        int firstNonWhite = FindFirstNonWhite(text, lineStart, lineEnd);
        if (firstNonWhite >= lineEnd)
            return;

        char first = text[firstNonWhite];
        if (first == ';' || first == '#')
        {
            AddToken(tokens, IniHighlightTokenKind.Comment, firstNonWhite, lineEnd - firstNonWhite);
            return;
        }

        if (first == '[')
        {
            TokenizeSectionLine(text, lineStart, lineEnd, firstNonWhite, sectionKindIndex, ref currentSectionKind, tokens);
            return;
        }

        TokenizeKeyValueLine(text, lineStart, lineEnd, currentSectionKind, tokens);
    }

    private void TokenizeSectionLine(
        string text,
        int lineStart,
        int lineEnd,
        int sectionStart,
        IReadOnlyDictionary<string, Ra2SectionKind> sectionKindIndex,
        ref Ra2SectionKind currentSectionKind,
        List<IniHighlightToken> tokens)
    {
        int closeBracket = IndexOf(text, ']', sectionStart + 1, lineEnd);
        if (closeBracket < 0)
            return;

        AddToken(tokens, IniHighlightTokenKind.SectionHeader, sectionStart, closeBracket - sectionStart + 1);
        string sectionId = text.Substring(sectionStart + 1, closeBracket - sectionStart - 1).Trim();
        currentSectionKind = ResolveSectionKind(sectionId, sectionKindIndex);

        int commentStart = FindInlineCommentStart(text, closeBracket + 1, lineEnd, includeHash: true);
        if (commentStart >= 0)
            AddToken(tokens, IniHighlightTokenKind.Comment, commentStart, lineEnd - commentStart);
    }

    private void TokenizeKeyValueLine(
        string text,
        int lineStart,
        int lineEnd,
        Ra2SectionKind currentSectionKind,
        List<IniHighlightToken> tokens)
    {
        int commentStart = FindInlineCommentStart(text, lineStart, lineEnd, includeHash: false);
        int parseEnd = commentStart >= 0 ? commentStart : lineEnd;
        int equalsIndex = IndexOf(text, '=', lineStart, parseEnd);
        if (equalsIndex < 0)
        {
            if (commentStart >= 0)
                AddToken(tokens, IniHighlightTokenKind.Comment, commentStart, lineEnd - commentStart);

            return;
        }

        int keyStart = FindFirstNonWhite(text, lineStart, equalsIndex);
        int keyEnd = FindLastNonWhiteExclusive(text, keyStart, equalsIndex);
        Ra2FieldDefinition? definition = null;
        if (keyStart < keyEnd)
        {
            string key = text.Substring(keyStart, keyEnd - keyStart);
            bool isKnownKey = TryResolveKnownField(currentSectionKind, key, out definition);
            IniHighlightTokenKind kind = isKnownKey
                ? IniHighlightTokenKind.KnownKey
                : IniHighlightTokenKind.UnknownKey;
            AddToken(tokens, kind, keyStart, keyEnd - keyStart);
        }

        AddToken(tokens, IniHighlightTokenKind.Equals, equalsIndex, 1);

        int valueStart = FindFirstNonWhite(text, equalsIndex + 1, parseEnd);
        int valueEnd = FindLastNonWhiteExclusive(text, valueStart, parseEnd);
        if (valueStart < valueEnd)
        {
            string value = text.Substring(valueStart, valueEnd - valueStart);
            AddToken(tokens, ResolveValueKind(value, definition), valueStart, valueEnd - valueStart);
        }

        if (commentStart >= 0)
            AddToken(tokens, IniHighlightTokenKind.Comment, commentStart, lineEnd - commentStart);
    }

    private bool TryResolveKnownField(
        Ra2SectionKind sectionKind,
        string key,
        out Ra2FieldDefinition? definition)
    {
        if (_fieldProvider.TryGetField(sectionKind, key, out Ra2FieldDefinition directDefinition))
        {
            definition = directDefinition;
            return true;
        }

        if (sectionKind == Ra2SectionKind.Unknown &&
            _fieldProvider.TryGetField(Ra2SectionKind.Unknown, key, out Ra2FieldDefinition fallbackDefinition))
        {
            definition = fallbackDefinition;
            return true;
        }

        definition = null;
        return false;
    }

    private static IniHighlightTokenKind ResolveValueKind(string value, Ra2FieldDefinition? definition)
    {
        string trimmed = value.Trim();
        if (IsNeutralValue(trimmed))
            return IniHighlightTokenKind.NeutralValue;

        if (definition is not null)
        {
            IniHighlightTokenKind? schemaKind = ResolveSchemaValueKind(definition);
            if (schemaKind is IniHighlightTokenKind kind)
                return kind;
        }

        if (IsBooleanLiteral(trimmed))
            return IniHighlightTokenKind.BooleanValue;

        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            return IniHighlightTokenKind.NumberValue;

        return IniHighlightTokenKind.Value;
    }

    private static IniHighlightTokenKind? ResolveSchemaValueKind(Ra2FieldDefinition definition)
    {
        return definition.ValueMetadata.ValueKind switch
        {
            Ra2FieldValueKind.Boolean => IniHighlightTokenKind.BooleanValue,
            Ra2FieldValueKind.Integer or Ra2FieldValueKind.Float => IniHighlightTokenKind.NumberValue,
            Ra2FieldValueKind.Enum or Ra2FieldValueKind.EnumList => IniHighlightTokenKind.EnumValue,
            Ra2FieldValueKind.Reference or Ra2FieldValueKind.ReferenceList => IniHighlightTokenKind.ReferenceValue,
            _ => ResolveEditorValueKind(definition.EditorKind, definition.ValueMetadata)
        };
    }

    private static IniHighlightTokenKind? ResolveEditorValueKind(
        FieldEditorKind editorKind,
        Ra2FieldValueMetadata metadata)
    {
        if (metadata.AllowedValues.Count > 0 || !string.IsNullOrWhiteSpace(metadata.EnumName))
            return IniHighlightTokenKind.EnumValue;

        return editorKind switch
        {
            FieldEditorKind.Boolean => IniHighlightTokenKind.BooleanValue,
            FieldEditorKind.Integer or FieldEditorKind.Float or FieldEditorKind.Percent => IniHighlightTokenKind.NumberValue,
            FieldEditorKind.Enum or FieldEditorKind.MultiSelect or FieldEditorKind.AbilityFlags => IniHighlightTokenKind.EnumValue,
            FieldEditorKind.Reference => IniHighlightTokenKind.ReferenceValue,
            _ => null
        };
    }

    private static bool IsBooleanLiteral(string value)
        => value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("false", StringComparison.OrdinalIgnoreCase);

    private static bool IsNeutralValue(string value)
        => value.Equals("none", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("<none>", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("empty", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("-1", StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyDictionary<string, Ra2SectionKind> BuildSectionKindIndex(string text)
    {
        Dictionary<string, Ra2SectionKind> result = new(StringComparer.OrdinalIgnoreCase);
        Ra2SectionKind? currentRegistryEntryKind = null;
        int lineStart = 0;

        while (lineStart < text.Length)
        {
            int lineEnd = FindLineEnd(text, lineStart);
            if (TryGetSectionId(text, lineStart, lineEnd, out string? sectionId))
            {
                currentRegistryEntryKind = TryGetRegistryEntryKind(sectionId, out Ra2SectionKind entryKind)
                    ? entryKind
                    : null;
            }
            else if (currentRegistryEntryKind is Ra2SectionKind registryEntryKind &&
                     TryGetKeyValueRanges(text, lineStart, lineEnd, out _, out _, out int valueStart, out int valueEnd))
            {
                string registryValue = text.Substring(valueStart, valueEnd - valueStart).Trim();
                if (!string.IsNullOrWhiteSpace(registryValue))
                    result.TryAdd(registryValue, registryEntryKind);
            }

            lineStart = MoveToNextLine(text, lineEnd);
        }

        return result;
    }

    private static Ra2SectionKind ResolveSectionKind(
        string sectionId,
        IReadOnlyDictionary<string, Ra2SectionKind> sectionKindIndex)
    {
        if (sectionKindIndex.TryGetValue(sectionId, out Ra2SectionKind inferredKind))
            return inferredKind;

        return InferDirectSectionKind(sectionId.AsSpan().Trim());
    }

    private static Ra2SectionKind InferDirectSectionKind(ReadOnlySpan<char> sectionId)
    {
        return sectionId switch
        {
            var value when value.Equals("General".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Global,
            var value when value.Equals("AudioVisual".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Global,
            var value when value.Equals("CombatDamage".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Global,
            var value when value.Equals("Countries".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Global,
            var value when value.Equals("Sides".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Global,
            var value when IsRegistrySection(value) => Ra2SectionKind.Global,
            _ => Ra2SectionKind.Unknown
        };
    }

    private static bool TryGetRegistryEntryKind(string sectionId, out Ra2SectionKind entryKind)
    {
        entryKind = sectionId.AsSpan().Trim() switch
        {
            var value when value.Equals("InfantryTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Infantry,
            var value when value.Equals("VehicleTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Vehicle,
            var value when value.Equals("AircraftTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Aircraft,
            var value when value.Equals("BuildingTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Building,
            var value when value.Equals("WeaponTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Weapon,
            var value when value.Equals("SuperWeaponTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.SuperWeapon,
            var value when value.Equals("Warheads".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Warhead,
            var value when value.Equals("WarheadTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Warhead,
            var value when value.Equals("Projectiles".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Projectile,
            var value when value.Equals("Animations".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Animation,
            var value when value.Equals("VoxelAnims".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.VoxelAnim,
            var value when value.Equals("Particles".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Particle,
            var value when value.Equals("ParticleSystems".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.ParticleSystem,
            var value when value.Equals("TerrainTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Terrain,
            _ => Ra2SectionKind.Unknown
        };

        return entryKind != Ra2SectionKind.Unknown;
    }

    private static bool IsRegistrySection(ReadOnlySpan<char> sectionId)
    {
        return sectionId.Equals("InfantryTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("VehicleTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("AircraftTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("BuildingTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("WeaponTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("SuperWeaponTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("Warheads".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("WarheadTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("Projectiles".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("Animations".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("VoxelAnims".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("Particles".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("ParticleSystems".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
               sectionId.Equals("TerrainTypes".AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetSectionId(string text, int lineStart, int lineEnd, out string sectionId)
    {
        sectionId = string.Empty;
        int firstNonWhite = FindFirstNonWhite(text, lineStart, lineEnd);
        if (firstNonWhite >= lineEnd || text[firstNonWhite] != '[')
            return false;

        int closeBracket = IndexOf(text, ']', firstNonWhite + 1, lineEnd);
        if (closeBracket < 0)
            return false;

        sectionId = text.Substring(firstNonWhite + 1, closeBracket - firstNonWhite - 1).Trim();
        return sectionId.Length > 0;
    }

    private static bool TryGetKeyValueRanges(
        string text,
        int lineStart,
        int lineEnd,
        out int keyStart,
        out int keyEnd,
        out int valueStart,
        out int valueEnd)
    {
        keyStart = -1;
        keyEnd = -1;
        valueStart = -1;
        valueEnd = -1;

        int firstNonWhite = FindFirstNonWhite(text, lineStart, lineEnd);
        if (firstNonWhite >= lineEnd || text[firstNonWhite] == ';' || text[firstNonWhite] == '#')
            return false;

        int commentStart = FindInlineCommentStart(text, lineStart, lineEnd, includeHash: false);
        int parseEnd = commentStart >= 0 ? commentStart : lineEnd;
        int equalsIndex = IndexOf(text, '=', lineStart, parseEnd);
        if (equalsIndex < 0)
            return false;

        keyStart = FindFirstNonWhite(text, lineStart, equalsIndex);
        keyEnd = FindLastNonWhiteExclusive(text, keyStart, equalsIndex);
        valueStart = FindFirstNonWhite(text, equalsIndex + 1, parseEnd);
        valueEnd = FindLastNonWhiteExclusive(text, valueStart, parseEnd);
        return keyStart < keyEnd && valueStart < valueEnd;
    }

    private static int FindLineEnd(string text, int start)
    {
        int index = start;
        while (index < text.Length && text[index] != '\r' && text[index] != '\n')
            index++;

        return index;
    }

    private static int MoveToNextLine(string text, int lineEnd)
    {
        if (lineEnd >= text.Length)
            return text.Length;

        if (text[lineEnd] == '\r' && lineEnd + 1 < text.Length && text[lineEnd + 1] == '\n')
            return lineEnd + 2;

        return lineEnd + 1;
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

    private static int FindInlineCommentStart(string text, int start, int end, bool includeHash)
    {
        for (int index = start; index < end; index++)
        {
            if (text[index] == ';' || includeHash && text[index] == '#')
                return index;
        }

        return -1;
    }

    private static void AddToken(List<IniHighlightToken> tokens, IniHighlightTokenKind kind, int startOffset, int length)
    {
        if (length > 0)
            tokens.Add(new IniHighlightToken(kind, startOffset, length));
    }
}
