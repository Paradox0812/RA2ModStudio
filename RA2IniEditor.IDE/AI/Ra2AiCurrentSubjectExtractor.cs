using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiCurrentSubjectExtractor : IRa2AiCurrentSubjectExtractor
{
    private static readonly Regex SectionHeaderPattern = new(
        @"^\s*\[(?<id>[A-Za-z0-9_.:\-]+)\]\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex KeyValuePattern = new(
        @"^\s*(?<key>[A-Za-z0-9_.:\-]+)\s*=",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> UnitKeys = CreateKeySet(
        "Strength",
        "Armor",
        "Primary",
        "Secondary",
        "Speed",
        "TechLevel",
        "Owner",
        "Cost",
        "Prerequisite");

    private static readonly HashSet<string> WeaponKeys = CreateKeySet(
        "Damage",
        "ROF",
        "Range",
        "Projectile",
        "Warhead");

    private static readonly HashSet<string> WarheadKeys = CreateKeySet(
        "Verses",
        "CellSpread",
        "PercentAtMax",
        "InfDeath",
        "Wall",
        "Wood");

    private static readonly HashSet<string> ProjectileKeys = CreateKeySet(
        "AA",
        "AG",
        "Arm",
        "Shadow",
        "Proximity",
        "Ranged",
        "Rotates",
        "SubjectToCliffs");

    private static readonly HashSet<string> ArtKeys = CreateKeySet(
        "Voxel",
        "Remapable",
        "Cameo",
        "TurretOffset",
        "PrimaryFireFLH");

    public Ra2AiCurrentSubject Extract(Ra2AiConversationContext conversationContext)
    {
        ArgumentNullException.ThrowIfNull(conversationContext);

        IReadOnlyList<SubjectCandidate> candidates = ExtractCandidates(conversationContext);
        if (candidates.Count == 0)
            return CreateUnknown();

        SubjectCandidate? mainUnit = candidates.FirstOrDefault(static candidate => candidate.Kind == Ra2AiSubjectKind.Unit);
        if (mainUnit is not null)
            return CreateSubject(mainUnit, Ra2AiSubjectSource.LastAssistantDraft);

        string? userMention = FindRecentUserMention(conversationContext, candidates);
        if (!string.IsNullOrWhiteSpace(userMention))
        {
            SubjectCandidate? mentionedCandidate = candidates.FirstOrDefault(
                candidate => string.Equals(candidate.SubjectId, userMention, StringComparison.OrdinalIgnoreCase));
            if (mentionedCandidate is not null)
                return CreateSubject(mentionedCandidate, Ra2AiSubjectSource.UserMention);
        }

        SubjectCandidate? supportSection = candidates.FirstOrDefault(static candidate =>
            candidate.Kind is not Ra2AiSubjectKind.Weapon
                and not Ra2AiSubjectKind.Warhead
                and not Ra2AiSubjectKind.Projectile);
        if (supportSection is not null)
            return CreateSubject(supportSection, Ra2AiSubjectSource.LastAssistantDraft);

        SubjectCandidate highestConfidence = candidates
            .OrderByDescending(static candidate => candidate.Confidence)
            .ThenBy(static candidate => candidate.Order)
            .First();
        return CreateSubject(highestConfidence, Ra2AiSubjectSource.LastAssistantDraft);
    }

    private static IReadOnlyList<SubjectCandidate> ExtractCandidates(Ra2AiConversationContext conversationContext)
    {
        List<SubjectCandidate> candidates = [];
        int order = 0;

        foreach (Ra2AiConversationTurn turn in conversationContext.Turns.Where(static turn =>
            turn.Role == Ra2AiConversationRole.Assistant && turn.IsDraftResponse))
        {
            foreach (DraftSection section in ExtractDraftSections(turn.Text))
            {
                Ra2AiSubjectKind kind = InferKind(section);
                double confidence = CalculateConfidence(kind, section);
                candidates.Add(new SubjectCandidate(
                    section.SectionId,
                    kind,
                    confidence,
                    order++));
            }
        }

        return candidates;
    }

    private static IReadOnlyList<DraftSection> ExtractDraftSections(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        List<DraftSection> sections = [];
        DraftSectionBuilder? current = null;
        bool artContext = false;

        string normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        foreach (string line in normalized.Split('\n'))
        {
            if (line.Contains("artmd.ini", StringComparison.OrdinalIgnoreCase))
                artContext = true;

            Match sectionMatch = SectionHeaderPattern.Match(line);
            if (sectionMatch.Success)
            {
                if (current is not null)
                    sections.Add(current.ToDraftSection());

                current = new DraftSectionBuilder(sectionMatch.Groups["id"].Value, artContext);
                continue;
            }

            if (current is null)
                continue;

            Match keyMatch = KeyValuePattern.Match(line);
            if (keyMatch.Success)
                current.Keys.Add(keyMatch.Groups["key"].Value);
        }

        if (current is not null)
            sections.Add(current.ToDraftSection());

        return sections;
    }

    private static Ra2AiSubjectKind InferKind(DraftSection section)
    {
        int unitScore = ScoreKeys(section.Keys, UnitKeys);
        int weaponScore = ScoreKeys(section.Keys, WeaponKeys);
        int warheadScore = ScoreKeys(section.Keys, WarheadKeys);
        int projectileScore = ScoreKeys(section.Keys, ProjectileKeys);
        int artScore = ScoreKeys(section.Keys, ArtKeys);

        if (section.IsArtContext && artScore > 0)
            return Ra2AiSubjectKind.Art;

        int highestScore = new[] { unitScore, weaponScore, warheadScore, projectileScore, artScore }.Max();
        if (highestScore <= 0)
            return Ra2AiSubjectKind.Section;

        if (unitScore >= 2 && unitScore == highestScore)
            return Ra2AiSubjectKind.Unit;

        if (weaponScore >= 2 && weaponScore == highestScore)
            return Ra2AiSubjectKind.Weapon;

        if (warheadScore >= 2 && warheadScore == highestScore)
            return Ra2AiSubjectKind.Warhead;

        if (projectileScore >= 2 && projectileScore == highestScore)
            return Ra2AiSubjectKind.Projectile;

        if (artScore >= 2 && artScore == highestScore)
            return Ra2AiSubjectKind.Art;

        return Ra2AiSubjectKind.Section;
    }

    private static double CalculateConfidence(Ra2AiSubjectKind kind, DraftSection section)
    {
        int score = kind switch
        {
            Ra2AiSubjectKind.Unit => ScoreKeys(section.Keys, UnitKeys),
            Ra2AiSubjectKind.Weapon => ScoreKeys(section.Keys, WeaponKeys),
            Ra2AiSubjectKind.Warhead => ScoreKeys(section.Keys, WarheadKeys),
            Ra2AiSubjectKind.Projectile => ScoreKeys(section.Keys, ProjectileKeys),
            Ra2AiSubjectKind.Art => ScoreKeys(section.Keys, ArtKeys),
            Ra2AiSubjectKind.Section => 1,
            _ => 0
        };

        return kind switch
        {
            Ra2AiSubjectKind.Unknown => 0,
            Ra2AiSubjectKind.Section => 0.35,
            _ => Math.Min(0.95, 0.55 + (score * 0.08))
        };
    }

    private static string? FindRecentUserMention(
        Ra2AiConversationContext conversationContext,
        IReadOnlyList<SubjectCandidate> candidates)
    {
        HashSet<string> candidateIds = candidates
            .Select(static candidate => candidate.SubjectId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (Ra2AiConversationTurn turn in conversationContext.Turns
            .Where(static turn => turn.Role == Ra2AiConversationRole.User)
            .Reverse())
        {
            foreach (string candidateId in candidateIds)
            {
                if (turn.Text.Contains($"[{candidateId}]", StringComparison.OrdinalIgnoreCase)
                    || ContainsIdentifierToken(turn.Text, candidateId))
                {
                    return candidateId;
                }
            }
        }

        return null;
    }

    private static bool ContainsIdentifierToken(string text, string identifier)
    {
        int index = text.IndexOf(identifier, StringComparison.OrdinalIgnoreCase);
        while (index >= 0)
        {
            bool hasValidPrefix = index == 0 || !IsIdentifierCharacter(text[index - 1]);
            int endIndex = index + identifier.Length;
            bool hasValidSuffix = endIndex >= text.Length || !IsIdentifierCharacter(text[endIndex]);
            if (hasValidPrefix && hasValidSuffix)
                return true;

            index = text.IndexOf(identifier, index + identifier.Length, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsIdentifierCharacter(char character)
        => char.IsLetterOrDigit(character) || character is '_' or '-' or '.' or ':';

    private static int ScoreKeys(IReadOnlyCollection<string> keys, HashSet<string> knownKeys)
        => keys.Count(key => knownKeys.Contains(key));

    private static HashSet<string> CreateKeySet(params string[] keys)
        => new(keys, StringComparer.OrdinalIgnoreCase);

    private static Ra2AiCurrentSubject CreateSubject(
        SubjectCandidate candidate,
        Ra2AiSubjectSource source)
        => new()
        {
            Kind = candidate.Kind,
            SubjectId = candidate.SubjectId,
            Source = source,
            Summary = $"上一轮 AI 草稿中的 {GetKindDisplayName(candidate.Kind)} [{candidate.SubjectId}]；仅来自对话草稿，尚未确认存在于项目文件中。",
            Confidence = candidate.Confidence,
            IsDraft = source == Ra2AiSubjectSource.LastAssistantDraft || source == Ra2AiSubjectSource.UserMention
        };

    private static Ra2AiCurrentSubject CreateUnknown()
        => new()
        {
            Kind = Ra2AiSubjectKind.Unknown,
            Source = Ra2AiSubjectSource.Unknown,
            Summary = "未能从当前会话的可见 AI 草稿中确定当前主题；没有假设任何项目文件状态。",
            Confidence = 0,
            IsDraft = false
        };

    private static string GetKindDisplayName(Ra2AiSubjectKind kind)
        => kind switch
        {
            Ra2AiSubjectKind.Unit => "单位",
            Ra2AiSubjectKind.Weapon => "武器",
            Ra2AiSubjectKind.Warhead => "弹头",
            Ra2AiSubjectKind.Projectile => "抛射体",
            Ra2AiSubjectKind.Art => "美术定义",
            Ra2AiSubjectKind.Section => "Section",
            _ => "未知对象"
        };

    private sealed record SubjectCandidate(
        string SubjectId,
        Ra2AiSubjectKind Kind,
        double Confidence,
        int Order);

    private sealed record DraftSection(
        string SectionId,
        IReadOnlyCollection<string> Keys,
        bool IsArtContext);

    private sealed class DraftSectionBuilder
    {
        public DraftSectionBuilder(string sectionId, bool isArtContext)
        {
            SectionId = sectionId;
            IsArtContext = isArtContext;
        }

        public string SectionId { get; }

        public bool IsArtContext { get; }

        public HashSet<string> Keys { get; } = new(StringComparer.OrdinalIgnoreCase);

        public DraftSection ToDraftSection()
            => new(SectionId, Keys.ToArray(), IsArtContext);
    }
}
