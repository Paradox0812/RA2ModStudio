using System.Text;
using System.Text.RegularExpressions;

namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiEditAvailabilityKind
{
    Available = 0,
    MissingConfiguration,
    UnsupportedEndpoint,
    NoEditableDocument,
    SnapshotUnavailable,
    ResourceLimitExceeded
}

internal enum Ra2AiInteractionRouteKind
{
    Advisory = 0,
    EditExplicit,
    TemplateExplicit,
    CompleteTemplateExplicit,
    TechnoDualArmamentExplicit,
    ArcingProjectileExplicit,
    HomingProjectileExplicit,
    YrCoreWarheadExplicit,
    EditAmbiguous,
    EditUnavailable,
    UnsupportedWorkCapability
}

internal readonly record struct Ra2AiInteractionRoute(
    Ra2AiInteractionRouteKind Kind,
    Ra2AiCapabilityMode CapabilityMode,
    Ra2AiEditAvailabilityKind EditAvailability,
    Ra2AiUserMode UserMode = Ra2AiUserMode.Work,
    string DomainIntentId = "ra2-general");

/// <summary>仅根据用户可见提示词和本地可用性事实裁决编辑权限。</summary>
internal static partial class Ra2AiInteractionRouter
{
    private const int MaximumRoutedPromptCharacters = 32768;

    private static readonly string[] AdvisoryOnlyMarkers =
    [
        "只解释", "仅解释", "只分析", "仅分析", "只给代码", "仅给代码",
        "explain only", "analysis only", "advisory only"
    ];

    private static readonly string[] EditActionMarkers =
    [
        "修改", "更改", "改为", "设置", "设为", "替换", "新增", "添加", "创建", "搭建", "搭一个", "构建", "构筑", "建立",
        "生成", "制作", "组装", "装配", "加装", "写入", "修正", "优化", "配置", "调整", "调到", "改成",
        "update", "change", "set", "replace", "insert", "add", "create", "write", "fix"
    ];

    private static readonly string[] CurrentDocumentMarkers =
    [
        "当前文件", "当前文档", "这个文件", "本文件", "this file", "current file", "current document"
    ];

    internal static Ra2AiInteractionRoute Resolve(
        string userPrompt,
        Ra2AiEditAvailabilityKind editAvailability)
        => Resolve(userPrompt, editAvailability, Ra2AiUserMode.Work);

    internal static Ra2AiInteractionRoute Resolve(
        string userPrompt,
        Ra2AiEditAvailabilityKind editAvailability,
        Ra2AiUserMode userMode)
    {
        if (!Enum.IsDefined(userMode))
            throw new ArgumentOutOfRangeException(nameof(userMode));

        string prompt = Normalize(userPrompt);
        string domainIntentId = ResolveDomainIntentId(prompt);
        if (userMode == Ra2AiUserMode.Chat)
            return Create(Ra2AiInteractionRouteKind.Advisory, editAvailability, userMode, domainIntentId);

        string positiveIntentPrompt = NegatedEditActionPattern().Replace(prompt, string.Empty);
        bool hasEditAction = ContainsAny(positiveIntentPrompt, EditActionMarkers);
        if (ContainsAny(prompt, AdvisoryOnlyMarkers) ||
            (!hasEditAction && !string.Equals(positiveIntentPrompt, prompt, StringComparison.Ordinal)))
        {
            return Create(Ra2AiInteractionRouteKind.Advisory, editAvailability, userMode, domainIntentId);
        }

        // Work mode is itself the explicit current-document authoring context. Requiring the user
        // to repeat “当前文件” made otherwise explicit object requests silently fall back to Chat.
        bool hasCurrentDocumentTarget = userMode == Ra2AiUserMode.Work ||
                                        ContainsAny(prompt, CurrentDocumentMarkers);
        bool hasAssignment = LooksLikeAssignment(prompt);
        if (hasEditAction && LooksLikeUnsupportedCompleteObject(prompt))
            return Create(Ra2AiInteractionRouteKind.UnsupportedWorkCapability, editAvailability, userMode, domainIntentId);

        bool looksLikeProjectile = LooksLikeProjectileProfileRequest(prompt);
        bool looksLikeWarhead = LooksLikeWarheadProfileRequest(prompt);
        bool createsProfile = ContainsAny(prompt, ProfileCreationMarkers);
        if (hasEditAction && hasCurrentDocumentTarget && createsProfile && looksLikeProjectile && !looksLikeWarhead)
        {
            if (ContainsAny(prompt, UnsupportedProjectileProfileMarkers))
            {
                return Create(
                    Ra2AiInteractionRouteKind.UnsupportedWorkCapability,
                    editAvailability,
                    userMode,
                    domainIntentId);
            }

            bool requestsArcing = ContainsAny(prompt, ArcingProjectileMarkers);
            bool requestsHoming = ContainsAny(prompt, HomingProjectileMarkers);
            if (requestsArcing && requestsHoming)
            {
                return Create(
                    Ra2AiInteractionRouteKind.UnsupportedWorkCapability,
                    editAvailability,
                    userMode,
                    domainIntentId);
            }

            Ra2AiInteractionRouteKind? projectileKind = requestsArcing
                ? Ra2AiInteractionRouteKind.ArcingProjectileExplicit
                : requestsHoming
                    ? Ra2AiInteractionRouteKind.HomingProjectileExplicit
                    : null;
            if (projectileKind is not null)
            {
                return editAvailability == Ra2AiEditAvailabilityKind.Available
                    ? Create(projectileKind.Value, editAvailability, userMode, domainIntentId)
                    : Create(Ra2AiInteractionRouteKind.EditUnavailable, editAvailability, userMode, domainIntentId);
            }
        }

        if (hasEditAction && hasCurrentDocumentTarget && createsProfile && looksLikeWarhead && !looksLikeProjectile)
        {
            return editAvailability == Ra2AiEditAvailabilityKind.Available
                ? Create(Ra2AiInteractionRouteKind.YrCoreWarheadExplicit, editAvailability, userMode, domainIntentId)
                : Create(Ra2AiInteractionRouteKind.EditUnavailable, editAvailability, userMode, domainIntentId);
        }

        if (hasEditAction && hasCurrentDocumentTarget && LooksLikeTemplateRequest(prompt))
        {
            if (RequestsUnsupportedCyclicFire(prompt))
            {
                return Create(
                    Ra2AiInteractionRouteKind.UnsupportedWorkCapability,
                    editAvailability,
                    userMode,
                    domainIntentId);
            }

            Ra2AiInteractionRouteKind templateKind = ContainsAny(prompt, SkeletonMarkers)
                ? Ra2AiInteractionRouteKind.TemplateExplicit
                : ContainsAny(prompt, DualArmamentMarkers)
                    ? Ra2AiInteractionRouteKind.TechnoDualArmamentExplicit
                    : Ra2AiInteractionRouteKind.CompleteTemplateExplicit;
            return editAvailability == Ra2AiEditAvailabilityKind.Available
                ? Create(templateKind, editAvailability, userMode, domainIntentId)
                : Create(Ra2AiInteractionRouteKind.EditUnavailable, editAvailability, userMode, domainIntentId);
        }

        if (hasEditAction && hasCurrentDocumentTarget && hasAssignment)
        {
            return editAvailability == Ra2AiEditAvailabilityKind.Available
                ? Create(Ra2AiInteractionRouteKind.EditExplicit, editAvailability, userMode, domainIntentId)
                : Create(Ra2AiInteractionRouteKind.EditUnavailable, editAvailability, userMode, domainIntentId);
        }

        if (hasEditAction || LooksLikeBareKeyValue(prompt))
            return Create(Ra2AiInteractionRouteKind.EditAmbiguous, editAvailability, userMode, domainIntentId);

        return Create(Ra2AiInteractionRouteKind.Advisory, editAvailability, userMode, domainIntentId);
    }

    private static Ra2AiInteractionRoute Create(
        Ra2AiInteractionRouteKind kind,
        Ra2AiEditAvailabilityKind availability,
        Ra2AiUserMode userMode = Ra2AiUserMode.Work,
        string domainIntentId = "ra2-general")
        => new(
            kind,
            kind switch
            {
                Ra2AiInteractionRouteKind.EditExplicit => Ra2AiCapabilityMode.CurrentDocumentEditPreview,
                Ra2AiInteractionRouteKind.TemplateExplicit => Ra2AiCapabilityMode.CurrentDocumentTemplatePreview,
                Ra2AiInteractionRouteKind.CompleteTemplateExplicit => Ra2AiCapabilityMode.CurrentDocumentCompleteTemplatePreview,
                Ra2AiInteractionRouteKind.TechnoDualArmamentExplicit => Ra2AiCapabilityMode.CurrentDocumentDualArmamentPreview,
                Ra2AiInteractionRouteKind.ArcingProjectileExplicit => Ra2AiCapabilityMode.CurrentDocumentArcingProjectilePreview,
                Ra2AiInteractionRouteKind.HomingProjectileExplicit => Ra2AiCapabilityMode.CurrentDocumentHomingProjectilePreview,
                Ra2AiInteractionRouteKind.YrCoreWarheadExplicit => Ra2AiCapabilityMode.CurrentDocumentYrCoreWarheadPreview,
                _ => Ra2AiCapabilityMode.AdvisoryOnly
            },
            availability,
            userMode,
            domainIntentId);

    private static readonly string[] SkeletonMarkers =
    [
        "骨架", "框架", "占位", "空结构", "只建结构", "skeleton", "scaffold", "placeholder", "empty structure"
    ];

    private static readonly string[] DualArmamentMarkers =
    [
        "主副武器", "主武器和副武器", "主炮和副炮", "两套武器链", "双武器", "双武器链",
        "primary and secondary", "primary & secondary", "dual weapon", "dual armament"
    ];

    private static readonly string[] CyclicFireMarkers =
    [
        "循环开火", "交替开火", "轮流开火", "轮换开火", "循环射击", "交替射击",
        "cyclic fire", "alternate fire", "alternating fire", "cycle weapons"
    ];

    private static readonly string[] ProfileCreationMarkers =
    [
        "新增", "添加", "创建", "搭建", "构建", "构筑", "建立", "生成", "制作", "组装", "装配", "加装",
        "add", "create", "build", "generate", "assemble"
    ];

    private static readonly string[] ArcingProjectileMarkers =
    [
        "曲射", "抛物线", "弧线弹道", "arcing", "ballistic arc"
    ];

    private static readonly string[] HomingProjectileMarkers =
    [
        "追踪", "跟踪", "制导", "导弹弹体", "homing", "guided", "tracking projectile", "missile projectile"
    ];

    private static readonly string[] UnsupportedProjectileProfileMarkers =
    [
        "phobos trajectory", "trajectory.", "straight trajectory", "直线轨迹", "直线弹道",
        "bombard", "parabola trajectory", "vertical", "垂直弹道", "airburst", "splits", "子母弹"
    ];

    private static bool LooksLikeTemplateRequest(string prompt)
        => prompt.Contains("武器链", StringComparison.OrdinalIgnoreCase) ||
           prompt.Contains("weapon chain", StringComparison.OrdinalIgnoreCase) ||
           ((prompt.Contains("weapon", StringComparison.OrdinalIgnoreCase) || prompt.Contains("武器", StringComparison.OrdinalIgnoreCase)) &&
            (prompt.Contains("projectile", StringComparison.OrdinalIgnoreCase) || prompt.Contains("抛射体", StringComparison.OrdinalIgnoreCase)) &&
            (prompt.Contains("warhead", StringComparison.OrdinalIgnoreCase) || prompt.Contains("弹头", StringComparison.OrdinalIgnoreCase)));

    private static bool LooksLikeProjectileProfileRequest(string prompt)
        => ContainsAny(prompt, ["projectile", "抛射体", "弹体", "弹道", "轨迹"]);

    private static bool LooksLikeWarheadProfileRequest(string prompt)
        => ContainsAny(prompt, ["warhead", "弹头", "verses", "伤害倍率", "范围伤害", "扩散伤害"]);

    private static bool RequestsUnsupportedCyclicFire(string prompt)
        => ContainsAny(CyclicFireNegatedIntentPattern().Replace(prompt, string.Empty), CyclicFireMarkers);

    private static bool LooksLikeBareKeyValue(string prompt)
        => BareKeyValuePattern().IsMatch(prompt);

    private static bool LooksLikeAssignment(string prompt)
        => AssignmentIntentPattern().IsMatch(prompt) || LooksLikeBareKeyValue(prompt);

    private static bool LooksLikeUnsupportedCompleteObject(string prompt)
        => ContainsAny(prompt,
        [
            "完整单位", "完整建筑", "超级武器", "国家阵营", "ai触发", "ai trigger",
            "shp动画", "vxl", "vox", "图标", "cameo", "icon"
        ]);

    internal static string ResolveDomainIntentId(string prompt)
    {
        if (ContainsAny(prompt, ["武器链", "weapon chain"]) ||
            ((prompt.Contains("weapon", StringComparison.OrdinalIgnoreCase) || prompt.Contains("武器", StringComparison.OrdinalIgnoreCase)) &&
             (prompt.Contains("projectile", StringComparison.OrdinalIgnoreCase) || prompt.Contains("抛射体", StringComparison.OrdinalIgnoreCase)) &&
             (prompt.Contains("warhead", StringComparison.OrdinalIgnoreCase) || prompt.Contains("弹头", StringComparison.OrdinalIgnoreCase))))
            return "weapon-chain";
        if (ContainsAny(prompt, ["projectile", "trajectory", "抛射体", "弹道", "轨迹"]))
            return "projectile-trajectory";
        if (ContainsAny(prompt, ["warhead", "verses", "cellspread", "弹头", "伤害倍率", "扩散伤害"]))
            return "warhead-damage";
        if (ContainsAny(prompt, ["aitrigger", "ai trigger", "teamtype", "taskforce", "scripttype", "触发队伍", "作战小队", "脚本队伍"]))
            return "ai-programming";
        if (ContainsAny(prompt, ["superweapon", "超级武器", "超武"]))
            return "superweapon";
        if (ContainsAny(prompt, ["country", "side", "国家", "阵营", "派系"]))
            return "faction";
        if (ContainsAny(prompt, ["particle", "radiation", "粒子", "辐射", "烟雾系统"]))
            return "particle-radiation";
        if (ContainsAny(prompt, ["terrain", "overlay", "smudge", "tiberium", "地形", "覆盖物", "矿石", "泰伯利亚" ]))
            return "terrain-resource";
        if (ContainsAny(prompt, ["sound", "eva", "声音", "语音", "音效"]))
            return "sound-eva";
        if (ContainsAny(prompt, ["artmd", "animation", "动画", "美术", "shp", "vxl", "voxel", "cameo", "icon", "图标"]))
            return "art-animation";
        if (ContainsAny(prompt, ["unit", "infantry", "vehicle", "aircraft", "building", "单位", "步兵", "载具", "飞行器", "建筑"]))
            return "techno";
        if (ContainsAny(prompt, ["字段", "field", "schema", "可信", "trust"]))
            return "field-schema";
        if (ContainsAny(prompt, ["注册", "类型列表", "引用闭合", "registration", "type list", "reference closure"]))
            return "reference-registration";
        return "ini-document";
    }

    private static bool ContainsAny(string prompt, IReadOnlyList<string> markers)
    {
        foreach (string marker in markers)
        {
            if (prompt.Contains(marker, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        ReadOnlySpan<char> source = value.AsSpan(0, Math.Min(value.Length, MaximumRoutedPromptCharacters));
        StringBuilder builder = new(source.Length);
        bool previousWasWhitespace = false;
        foreach (char character in source)
        {
            bool isWhitespace = char.IsWhiteSpace(character);
            if (!isWhitespace || !previousWasWhitespace)
                builder.Append(isWhitespace ? ' ' : char.ToLowerInvariant(character));
            previousWasWhitespace = isWhitespace;
        }

        return builder.ToString().Trim();
    }

    [GeneratedRegex(@"^\s*[a-z_][a-z0-9_.-]*\s*(?:=|\s)\s*[^\s]+\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BareKeyValuePattern();

    [GeneratedRegex(
        @"(?:=|\b[a-z_][a-z0-9_.-]*\b\s*(?:设置|修改|调整)?\s*(?:为|成|到)\s*\S+|(?:修改|更改|改|设置|设|调整|调|替换)\s*(?:为|成|到)\s*\S+|(?:set|change|update|replace)\b[^\r\n]{0,256}?\b(?:to|with)\b\s*\S+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AssignmentIntentPattern();

    [GeneratedRegex(
        @"(?:不要|无需|不需要|不必|不得|禁止)\s*(?:再)?\s*(?:修改|更改|改动|编辑)|(?:do\s+not|don't|no\s+need\s+to)\s+(?:modify|edit|change)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NegatedEditActionPattern();

    [GeneratedRegex(
        @"(?:不要|无需|不需要|不必|不使用|不采用|避免|禁止|排除)(?:再)?(?:使用|采用)?\s*(?:(?:循环|交替|轮流|轮换)(?:开火|射击)?)(?:\s*(?:或|和|与|及|/|、)\s*(?:(?:循环|交替|轮流|轮换)(?:开火|射击)?))*|(?:without|do not(?: use)?|don't(?: use)?|not using|avoid|exclude|no)\s+(?:(?:cyclic|alternate|alternating)\s+(?:fire|firing)|cycle weapons)(?:\s*(?:or|and|/)\s*(?:(?:cyclic|alternate|alternating)\s+(?:fire|firing)|cycle weapons))*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CyclicFireNegatedIntentPattern();
}
