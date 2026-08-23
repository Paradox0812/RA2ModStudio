using System.Globalization;
using RA2IniEditor.Application.Automation;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation.Experimental;

public sealed class Ra2AutomationTemplateService : IRa2AutomationTemplateService
{
    internal const string WeaponChainTemplateId = "weapon-projectile-warhead-skeleton";
    internal const int WeaponChainTemplateVersion = 1;
    internal const string CompleteWeaponChainTemplateId = "weapon-projectile-warhead-direct-fire-complete";
    internal const int CompleteWeaponChainTemplateVersion = 1;
    internal const string DualArmamentTemplateId = "techno-primary-secondary-direct-fire-complete";
    internal const int DualArmamentTemplateVersion = 1;
    internal const string ArcingProjectileTemplateId = "weapon-projectile-arcing-complete";
    internal const int ArcingProjectileTemplateVersion = 1;
    internal const string HomingProjectileTemplateId = "weapon-projectile-homing-complete";
    internal const int HomingProjectileTemplateVersion = 1;
    internal const string YrCoreWarheadTemplateId = "weapon-warhead-yr-core-complete";
    internal const int YrCoreWarheadTemplateVersion = 1;

    private static readonly Ra2ContentTemplateDefinition WeaponChainDefinition = new(
        WeaponChainTemplateId,
        WeaponChainTemplateVersion,
        "Weapon / Projectile / Warhead skeleton",
        [
            new Ra2ContentTemplateParameter("weaponId", Ra2ContentTemplateParameterKind.Identifier, required: true),
            new Ra2ContentTemplateParameter("projectileId", Ra2ContentTemplateParameterKind.Identifier, required: true),
            new Ra2ContentTemplateParameter("warheadId", Ra2ContentTemplateParameterKind.Identifier, required: true)
        ],
        [
            new Ra2ContentTemplateSectionSpec(
                Ra2ContentTemplateValueSource.Parameter("weaponId"),
                Ra2SectionKind.Weapon,
                [
                    new Ra2ContentTemplateFieldSpec("Projectile", Ra2ContentTemplateValueSource.Parameter("projectileId")),
                    new Ra2ContentTemplateFieldSpec("Warhead", Ra2ContentTemplateValueSource.Parameter("warheadId"))
                ]),
            new Ra2ContentTemplateSectionSpec(
                Ra2ContentTemplateValueSource.Parameter("projectileId"),
                Ra2SectionKind.Projectile,
                []),
            new Ra2ContentTemplateSectionSpec(
                Ra2ContentTemplateValueSource.Parameter("warheadId"),
                Ra2SectionKind.Warhead,
                [])
        ]);

    private static readonly IReadOnlyList<Ra2AutomationTemplateDescriptor> Templates =
        Array.AsReadOnly<Ra2AutomationTemplateDescriptor>(
        [
            new(
                WeaponChainTemplateId,
                WeaponChainTemplateVersion,
                "Weapon / Projectile / Warhead 骨架",
                "创建三个相互关联的当前文档 Section；不生成玩法默认值、注册列表或素材。",
                Ra2AutomationTemplateOutputKind.Skeleton,
                [
                    new("weaponId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
                    new("projectileId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
                    new("warheadId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null)
                ]),
            new(
                CompleteWeaponChainTemplateId,
                CompleteWeaponChainTemplateVersion,
                "Direct-fire 武器链",
                "把现有 TechnoType 武器槽绑定到一组字段完整、引用闭合的 Weapon / Projectile / Warhead Section。",
                Ra2AutomationTemplateOutputKind.CompleteObject,
                [
                    new("ownerSectionId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
                    new("ownerWeaponSlot", Ra2AutomationTemplateParameterKind.String, required: true, defaultValue: null),
                    new("weaponId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
                    new("projectileId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
                    new("warheadId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
                    new("damage", Ra2AutomationTemplateParameterKind.Integer, required: true, defaultValue: null),
                    new("rof", Ra2AutomationTemplateParameterKind.Integer, required: true, defaultValue: null),
                    new("range", Ra2AutomationTemplateParameterKind.Float, required: true, defaultValue: null),
                    new("projectileSpeed", Ra2AutomationTemplateParameterKind.Integer, required: true, defaultValue: null),
                    new("verses", Ra2AutomationTemplateParameterKind.String, required: true, defaultValue: null),
                    new("infDeath", Ra2AutomationTemplateParameterKind.Integer, required: true, defaultValue: null),
                    new("cellSpread", Ra2AutomationTemplateParameterKind.Float, required: true, defaultValue: null),
                    new("percentAtMax", Ra2AutomationTemplateParameterKind.Float, required: true, defaultValue: null),
                    new("antiAir", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
                    new("antiGround", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null)
                ]),
            new(
                DualArmamentTemplateId,
                DualArmamentTemplateVersion,
                "Techno 主副武器完整配置",
                "把现有 TechnoType 的 Primary 与 Secondary 同时绑定到两组字段完整、引用闭合的 direct-fire 武器链；不表达循环或交替开火。",
                Ra2AutomationTemplateOutputKind.CompleteObject,
                CreateDualArmamentParameterDescriptors()),
            new(
                ArcingProjectileTemplateId,
                ArcingProjectileTemplateVersion,
                "Arcing Projectile 完整配置",
                "把现有 Weapon 绑定到一个字段完整的原版曲射 Projectile；不混用 ROT、Vertical、Inviso 或 Phobos Trajectory。",
                Ra2AutomationTemplateOutputKind.CompleteObject,
                [
                    new("weaponId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
                    new("projectileId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
                    new("image", Ra2AutomationTemplateParameterKind.Reference, required: true, defaultValue: null),
                    new("antiAir", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
                    new("antiGround", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
                    new("subjectToWalls", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
                    new("subjectToElevation", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
                    new("subjectToCliffs", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null)
                ]),
            new(
                HomingProjectileTemplateId,
                HomingProjectileTemplateVersion,
                "Homing Projectile 完整配置",
                "把现有 Weapon 绑定到一个 ROT>0 的原版追踪 Projectile；不混用 Arcing、Vertical、Inviso 或 Phobos Trajectory。",
                Ra2AutomationTemplateOutputKind.CompleteObject,
                [
                    new("weaponId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
                    new("projectileId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
                    new("image", Ra2AutomationTemplateParameterKind.Reference, required: true, defaultValue: null),
                    new("rot", Ra2AutomationTemplateParameterKind.Integer, required: true, defaultValue: null),
                    new("antiAir", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
                    new("antiGround", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null)
                ]),
            new(
                YrCoreWarheadTemplateId,
                YrCoreWarheadTemplateVersion,
                "YR Core Warhead 完整配置",
                "把现有 Weapon 绑定到一个覆盖原版 11 护甲槽与核心伤害行为的 Warhead；不生成 Ares Versus.* override。",
                Ra2AutomationTemplateOutputKind.CompleteObject,
                CreateYrCoreWarheadParameterDescriptors())
        ]);

    private readonly Ra2AutomationDocumentQueryService _queryService;
    private readonly Ra2ContentTemplateCompiler _compiler;

    public Ra2AutomationTemplateService()
    {
        _queryService = new Ra2AutomationDocumentQueryService();
        _compiler = new Ra2ContentTemplateCompiler(_queryService);
    }

    public IReadOnlyList<Ra2AutomationTemplateDescriptor> GetTemplates() => Templates;

    public Ra2AutomationTemplateExpansionResult ExpandTemplate(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationTemplateExpansionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(request);

        if (request.TemplateId is not (
                WeaponChainTemplateId or
                CompleteWeaponChainTemplateId or
                DualArmamentTemplateId or
                ArcingProjectileTemplateId or
                HomingProjectileTemplateId or
                YrCoreWarheadTemplateId))
            return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.TemplateNotFound, "The requested template was not found.");
        int expectedVersion = request.TemplateId switch
        {
            WeaponChainTemplateId => WeaponChainTemplateVersion,
            CompleteWeaponChainTemplateId => CompleteWeaponChainTemplateVersion,
            DualArmamentTemplateId => DualArmamentTemplateVersion,
            ArcingProjectileTemplateId => ArcingProjectileTemplateVersion,
            HomingProjectileTemplateId => HomingProjectileTemplateVersion,
            _ => YrCoreWarheadTemplateVersion
        };
        if (request.TemplateVersion != expectedVersion)
            return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.TemplateVersionMismatch, "The requested template version is not available.");
        if (snapshot.Text.Length > Ra2AutomationDocumentQueryService.MaximumDocumentCharacters)
            return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.DocumentTooLarge, "The document exceeds the supported character limit.");

        Ra2ContentTemplateDefinition definition = WeaponChainDefinition;
        if (request.TemplateId == CompleteWeaponChainTemplateId)
        {
            Dictionary<string, string> arguments = request.Arguments
                .GroupBy(argument => argument.Name, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
            if (!arguments.TryGetValue("ownerWeaponSlot", out string? slot) ||
                slot is not ("Primary" or "Secondary"))
            {
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "ownerWeaponSlot must be Primary or Secondary.");
            }
            if (!arguments.TryGetValue("verses", out string? verses) || !IsValidVerses(verses))
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "verses must contain exactly 11 percentage tokens.");

            definition = CreateCompleteWeaponChainDefinition(slot);
        }
        else if (request.TemplateId == DualArmamentTemplateId)
        {
            Dictionary<string, string> arguments = request.Arguments
                .GroupBy(argument => argument.Name, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
            if (!HasValidVerses(arguments, "primaryVerses") || !HasValidVerses(arguments, "secondaryVerses"))
            {
                return Failure(
                    snapshot,
                    Ra2AutomationTemplateExpansionFailureKind.InvalidArguments,
                    "primaryVerses and secondaryVerses must each contain exactly 11 percentage tokens.");
            }

            definition = CreateDualArmamentDefinition();
        }
        else if (request.TemplateId == ArcingProjectileTemplateId)
        {
            definition = CreateArcingProjectileDefinition();
        }
        else if (request.TemplateId == HomingProjectileTemplateId)
        {
            IReadOnlyDictionary<string, string> arguments = ToArgumentMap(request);
            if (!TryGetInteger(arguments, "rot", out int rot) || rot <= 0)
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "rot must be an integer greater than zero.");

            definition = CreateHomingProjectileDefinition();
        }
        else if (request.TemplateId == YrCoreWarheadTemplateId)
        {
            IReadOnlyDictionary<string, string> arguments = ToArgumentMap(request);
            if (!HasValidVerses(arguments, "verses"))
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "verses must contain exactly 11 percentage tokens.");
            if (!TryGetInteger(arguments, "infDeath", out int infDeath) || infDeath is < 0 or > 10)
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "infDeath must be between 0 and 10.");
            if (!TryGetFiniteDouble(arguments, "cellSpread", out double cellSpread) || cellSpread is < 0 or > 11)
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "cellSpread must be between 0 and 11.");
            if (!TryGetFiniteDouble(arguments, "percentAtMax", out double percentAtMax) || percentAtMax < 0)
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "percentAtMax must be non-negative.");
            if (!TryGetFiniteDouble(arguments, "proneDamage", out double proneDamage) || proneDamage < 0)
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "proneDamage must be non-negative.");

            Ra2AutomationSectionQueryResult armorTypes = _queryService.GetSection(
                snapshot,
                new Ra2AutomationSectionQuery("ArmorTypes"),
                cancellationToken);
            if (armorTypes.Succeeded || armorTypes.FailureKind == Ra2AutomationSectionQueryFailureKind.AmbiguousSection)
            {
                return Failure(
                    snapshot,
                    Ra2AutomationTemplateExpansionFailureKind.InvalidArguments,
                    "The YR core Warhead profile does not support documents with an [ArmorTypes] section; use an Ares custom-armor profile.");
            }
            if (armorTypes.FailureKind == Ra2AutomationSectionQueryFailureKind.Canceled)
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.Canceled, "Template expansion was canceled.");
            if (armorTypes.FailureKind != Ra2AutomationSectionQueryFailureKind.NotFound)
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.ExpansionFailed, "The document could not be checked for custom ArmorTypes.");

            definition = CreateYrCoreWarheadDefinition();
        }

        Ra2ContentTemplateCompilationResult compilation = _compiler.Compile(
            definition,
            request.Arguments.Select(argument => new KeyValuePair<string, string>(argument.Name, argument.Value)),
            snapshot,
            cancellationToken);
        if (!compilation.Succeeded)
        {
            return Failure(
                snapshot,
                MapFailure(compilation.FailureKind),
                compilation.Message);
        }

        Ra2AutomationTemplateWarningFact[] warnings = compilation.Warnings
            .Select(warning => new Ra2AutomationTemplateWarningFact(
                Ra2AutomationTemplateWarningKind.FieldTrustCaution,
                warning.SectionName,
                warning.Key,
                warning.TrustLevel,
                warning.Message))
            .ToArray();
        return new Ra2AutomationTemplateExpansionResult(
            snapshot,
            Ra2AutomationTemplateExpansionFailureKind.None,
            "The template expansion succeeded.",
            compilation.Plan,
            warnings);
    }

    private static Ra2AutomationTemplateExpansionFailureKind MapFailure(Ra2ContentTemplateCompilationFailureKind failure)
        => failure switch
        {
            Ra2ContentTemplateCompilationFailureKind.MissingArgument => Ra2AutomationTemplateExpansionFailureKind.MissingRequiredArgument,
            Ra2ContentTemplateCompilationFailureKind.UnknownArgument => Ra2AutomationTemplateExpansionFailureKind.UnknownArgument,
            Ra2ContentTemplateCompilationFailureKind.DuplicateArgument => Ra2AutomationTemplateExpansionFailureKind.DuplicateArgument,
            Ra2ContentTemplateCompilationFailureKind.InvalidArgumentValue or
                Ra2ContentTemplateCompilationFailureKind.InvalidFieldValue => Ra2AutomationTemplateExpansionFailureKind.InvalidArguments,
            Ra2ContentTemplateCompilationFailureKind.FieldSchemaNotFound => Ra2AutomationTemplateExpansionFailureKind.FieldSchemaNotFound,
            Ra2ContentTemplateCompilationFailureKind.BlockedFieldTrust => Ra2AutomationTemplateExpansionFailureKind.BlockedFieldTrust,
            Ra2ContentTemplateCompilationFailureKind.RequiredSectionNotFound => Ra2AutomationTemplateExpansionFailureKind.RequiredSectionNotFound,
            Ra2ContentTemplateCompilationFailureKind.RequiredSectionKindMismatch => Ra2AutomationTemplateExpansionFailureKind.RequiredSectionKindMismatch,
            Ra2ContentTemplateCompilationFailureKind.OperationLimitExceeded => Ra2AutomationTemplateExpansionFailureKind.OperationLimitExceeded,
            Ra2ContentTemplateCompilationFailureKind.DocumentTooLarge => Ra2AutomationTemplateExpansionFailureKind.DocumentTooLarge,
            Ra2ContentTemplateCompilationFailureKind.Canceled => Ra2AutomationTemplateExpansionFailureKind.Canceled,
            _ => Ra2AutomationTemplateExpansionFailureKind.ExpansionFailed
        };

    private static Ra2AutomationTemplateExpansionResult Failure(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationTemplateExpansionFailureKind failureKind,
        string message)
        => new(snapshot, failureKind, message, null);

    private static Ra2ContentTemplateDefinition CreateCompleteWeaponChainDefinition(string ownerWeaponSlot)
        => new(
            CompleteWeaponChainTemplateId,
            CompleteWeaponChainTemplateVersion,
            "Complete direct-fire Weapon / Projectile / Warhead chain",
            [
                new Ra2ContentTemplateParameter("ownerSectionId", Ra2ContentTemplateParameterKind.Identifier, required: true),
                new Ra2ContentTemplateParameter("ownerWeaponSlot", Ra2ContentTemplateParameterKind.String, required: true),
                new Ra2ContentTemplateParameter("weaponId", Ra2ContentTemplateParameterKind.Identifier, required: true),
                new Ra2ContentTemplateParameter("projectileId", Ra2ContentTemplateParameterKind.Identifier, required: true),
                new Ra2ContentTemplateParameter("warheadId", Ra2ContentTemplateParameterKind.Identifier, required: true),
                new Ra2ContentTemplateParameter("damage", Ra2ContentTemplateParameterKind.Integer, required: true),
                new Ra2ContentTemplateParameter("rof", Ra2ContentTemplateParameterKind.Integer, required: true),
                new Ra2ContentTemplateParameter("range", Ra2ContentTemplateParameterKind.Float, required: true),
                new Ra2ContentTemplateParameter("projectileSpeed", Ra2ContentTemplateParameterKind.Integer, required: true),
                new Ra2ContentTemplateParameter("verses", Ra2ContentTemplateParameterKind.String, required: true),
                new Ra2ContentTemplateParameter("infDeath", Ra2ContentTemplateParameterKind.Integer, required: true),
                new Ra2ContentTemplateParameter("cellSpread", Ra2ContentTemplateParameterKind.Float, required: true),
                new Ra2ContentTemplateParameter("percentAtMax", Ra2ContentTemplateParameterKind.Float, required: true),
                new Ra2ContentTemplateParameter("antiAir", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new Ra2ContentTemplateParameter("antiGround", Ra2ContentTemplateParameterKind.Boolean, required: true)
            ],
            [
                new Ra2ContentTemplateSectionSpec(
                    Ra2ContentTemplateValueSource.Parameter("ownerSectionId"),
                    Ra2SectionKind.Techno,
                    [new Ra2ContentTemplateFieldSpec(ownerWeaponSlot, Ra2ContentTemplateValueSource.Parameter("weaponId"))],
                    Ra2ContentTemplateSectionTargetMode.RequireExisting),
                new Ra2ContentTemplateSectionSpec(
                    Ra2ContentTemplateValueSource.Parameter("weaponId"),
                    Ra2SectionKind.Weapon,
                    [
                        new Ra2ContentTemplateFieldSpec("Damage", Ra2ContentTemplateValueSource.Parameter("damage")),
                        new Ra2ContentTemplateFieldSpec("ROF", Ra2ContentTemplateValueSource.Parameter("rof")),
                        new Ra2ContentTemplateFieldSpec("Range", Ra2ContentTemplateValueSource.Parameter("range")),
                        new Ra2ContentTemplateFieldSpec("Projectile", Ra2ContentTemplateValueSource.Parameter("projectileId")),
                        new Ra2ContentTemplateFieldSpec("Speed", Ra2ContentTemplateValueSource.Parameter("projectileSpeed")),
                        new Ra2ContentTemplateFieldSpec("Warhead", Ra2ContentTemplateValueSource.Parameter("warheadId"))
                    ]),
                new Ra2ContentTemplateSectionSpec(
                    Ra2ContentTemplateValueSource.Parameter("projectileId"),
                    Ra2SectionKind.Projectile,
                    [
                        new Ra2ContentTemplateFieldSpec("Inviso", Ra2ContentTemplateValueSource.Literal("yes")),
                        new Ra2ContentTemplateFieldSpec("Image", Ra2ContentTemplateValueSource.Literal("none")),
                        new Ra2ContentTemplateFieldSpec("AA", Ra2ContentTemplateValueSource.Parameter("antiAir")),
                        new Ra2ContentTemplateFieldSpec("AG", Ra2ContentTemplateValueSource.Parameter("antiGround"))
                    ]),
                new Ra2ContentTemplateSectionSpec(
                    Ra2ContentTemplateValueSource.Parameter("warheadId"),
                    Ra2SectionKind.Warhead,
                    [
                        new Ra2ContentTemplateFieldSpec("Verses", Ra2ContentTemplateValueSource.Parameter("verses")),
                        new Ra2ContentTemplateFieldSpec("InfDeath", Ra2ContentTemplateValueSource.Parameter("infDeath")),
                        new Ra2ContentTemplateFieldSpec("CellSpread", Ra2ContentTemplateValueSource.Parameter("cellSpread")),
                        new Ra2ContentTemplateFieldSpec("PercentAtMax", Ra2ContentTemplateValueSource.Parameter("percentAtMax"))
                    ])
            ]);

    private static IReadOnlyList<Ra2AutomationTemplateParameterDescriptor> CreateDualArmamentParameterDescriptors()
    {
        List<Ra2AutomationTemplateParameterDescriptor> parameters =
        [
            new("ownerSectionId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null)
        ];
        AddChainParameterDescriptors(parameters, "primary");
        AddChainParameterDescriptors(parameters, "secondary");
        return parameters.AsReadOnly();
    }

    private static void AddChainParameterDescriptors(
        ICollection<Ra2AutomationTemplateParameterDescriptor> parameters,
        string prefix)
    {
        parameters.Add(new($"{prefix}WeaponId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}ProjectileId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}WarheadId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}Damage", Ra2AutomationTemplateParameterKind.Integer, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}Rof", Ra2AutomationTemplateParameterKind.Integer, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}Range", Ra2AutomationTemplateParameterKind.Float, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}ProjectileSpeed", Ra2AutomationTemplateParameterKind.Integer, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}Verses", Ra2AutomationTemplateParameterKind.String, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}InfDeath", Ra2AutomationTemplateParameterKind.Integer, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}CellSpread", Ra2AutomationTemplateParameterKind.Float, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}PercentAtMax", Ra2AutomationTemplateParameterKind.Float, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}AntiAir", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null));
        parameters.Add(new($"{prefix}AntiGround", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null));
    }

    private static Ra2ContentTemplateDefinition CreateDualArmamentDefinition()
    {
        List<Ra2ContentTemplateParameter> parameters =
        [
            new("ownerSectionId", Ra2ContentTemplateParameterKind.Identifier, required: true)
        ];
        AddChainParameters(parameters, "primary");
        AddChainParameters(parameters, "secondary");

        List<Ra2ContentTemplateSectionSpec> sections =
        [
            new(
                Ra2ContentTemplateValueSource.Parameter("ownerSectionId"),
                Ra2SectionKind.Techno,
                [
                    new("Primary", Ra2ContentTemplateValueSource.Parameter("primaryWeaponId")),
                    new("Secondary", Ra2ContentTemplateValueSource.Parameter("secondaryWeaponId"))
                ],
                Ra2ContentTemplateSectionTargetMode.RequireExisting)
        ];
        AddChainSections(sections, "primary");
        AddChainSections(sections, "secondary");

        return new(
            DualArmamentTemplateId,
            DualArmamentTemplateVersion,
            "Complete Techno Primary / Secondary direct-fire armament",
            parameters,
            sections);
    }

    private static void AddChainParameters(ICollection<Ra2ContentTemplateParameter> parameters, string prefix)
    {
        parameters.Add(new($"{prefix}WeaponId", Ra2ContentTemplateParameterKind.Identifier, required: true));
        parameters.Add(new($"{prefix}ProjectileId", Ra2ContentTemplateParameterKind.Identifier, required: true));
        parameters.Add(new($"{prefix}WarheadId", Ra2ContentTemplateParameterKind.Identifier, required: true));
        parameters.Add(new($"{prefix}Damage", Ra2ContentTemplateParameterKind.Integer, required: true));
        parameters.Add(new($"{prefix}Rof", Ra2ContentTemplateParameterKind.Integer, required: true));
        parameters.Add(new($"{prefix}Range", Ra2ContentTemplateParameterKind.Float, required: true));
        parameters.Add(new($"{prefix}ProjectileSpeed", Ra2ContentTemplateParameterKind.Integer, required: true));
        parameters.Add(new($"{prefix}Verses", Ra2ContentTemplateParameterKind.String, required: true));
        parameters.Add(new($"{prefix}InfDeath", Ra2ContentTemplateParameterKind.Integer, required: true));
        parameters.Add(new($"{prefix}CellSpread", Ra2ContentTemplateParameterKind.Float, required: true));
        parameters.Add(new($"{prefix}PercentAtMax", Ra2ContentTemplateParameterKind.Float, required: true));
        parameters.Add(new($"{prefix}AntiAir", Ra2ContentTemplateParameterKind.Boolean, required: true));
        parameters.Add(new($"{prefix}AntiGround", Ra2ContentTemplateParameterKind.Boolean, required: true));
    }

    private static void AddChainSections(ICollection<Ra2ContentTemplateSectionSpec> sections, string prefix)
    {
        sections.Add(new(
            Ra2ContentTemplateValueSource.Parameter($"{prefix}WeaponId"),
            Ra2SectionKind.Weapon,
            [
                new("Damage", Ra2ContentTemplateValueSource.Parameter($"{prefix}Damage")),
                new("ROF", Ra2ContentTemplateValueSource.Parameter($"{prefix}Rof")),
                new("Range", Ra2ContentTemplateValueSource.Parameter($"{prefix}Range")),
                new("Projectile", Ra2ContentTemplateValueSource.Parameter($"{prefix}ProjectileId")),
                new("Speed", Ra2ContentTemplateValueSource.Parameter($"{prefix}ProjectileSpeed")),
                new("Warhead", Ra2ContentTemplateValueSource.Parameter($"{prefix}WarheadId"))
            ]));
        sections.Add(new(
            Ra2ContentTemplateValueSource.Parameter($"{prefix}ProjectileId"),
            Ra2SectionKind.Projectile,
            [
                new("Inviso", Ra2ContentTemplateValueSource.Literal("yes")),
                new("Image", Ra2ContentTemplateValueSource.Literal("none")),
                new("AA", Ra2ContentTemplateValueSource.Parameter($"{prefix}AntiAir")),
                new("AG", Ra2ContentTemplateValueSource.Parameter($"{prefix}AntiGround"))
            ]));
        sections.Add(new(
            Ra2ContentTemplateValueSource.Parameter($"{prefix}WarheadId"),
            Ra2SectionKind.Warhead,
            [
                new("Verses", Ra2ContentTemplateValueSource.Parameter($"{prefix}Verses")),
                new("InfDeath", Ra2ContentTemplateValueSource.Parameter($"{prefix}InfDeath")),
                new("CellSpread", Ra2ContentTemplateValueSource.Parameter($"{prefix}CellSpread")),
                new("PercentAtMax", Ra2ContentTemplateValueSource.Parameter($"{prefix}PercentAtMax"))
            ]));
    }

    private static Ra2ContentTemplateDefinition CreateArcingProjectileDefinition()
        => new(
            ArcingProjectileTemplateId,
            ArcingProjectileTemplateVersion,
            "Complete original-game arcing Projectile",
            [
                new("weaponId", Ra2ContentTemplateParameterKind.Identifier, required: true),
                new("projectileId", Ra2ContentTemplateParameterKind.Identifier, required: true),
                new("image", Ra2ContentTemplateParameterKind.Reference, required: true),
                new("antiAir", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("antiGround", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("subjectToWalls", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("subjectToElevation", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("subjectToCliffs", Ra2ContentTemplateParameterKind.Boolean, required: true)
            ],
            [
                new(
                    Ra2ContentTemplateValueSource.Parameter("weaponId"),
                    Ra2SectionKind.Weapon,
                    [new("Projectile", Ra2ContentTemplateValueSource.Parameter("projectileId"))],
                    Ra2ContentTemplateSectionTargetMode.RequireExisting),
                new(
                    Ra2ContentTemplateValueSource.Parameter("projectileId"),
                    Ra2SectionKind.Projectile,
                    [
                        new("Image", Ra2ContentTemplateValueSource.Parameter("image")),
                        new("AA", Ra2ContentTemplateValueSource.Parameter("antiAir")),
                        new("AG", Ra2ContentTemplateValueSource.Parameter("antiGround")),
                        new("Arcing", Ra2ContentTemplateValueSource.Literal("yes")),
                        new("SubjectToWalls", Ra2ContentTemplateValueSource.Parameter("subjectToWalls")),
                        new("SubjectToElevation", Ra2ContentTemplateValueSource.Parameter("subjectToElevation")),
                        new("SubjectToCliffs", Ra2ContentTemplateValueSource.Parameter("subjectToCliffs"))
                    ])
            ]);

    private static Ra2ContentTemplateDefinition CreateHomingProjectileDefinition()
        => new(
            HomingProjectileTemplateId,
            HomingProjectileTemplateVersion,
            "Complete original-game homing Projectile",
            [
                new("weaponId", Ra2ContentTemplateParameterKind.Identifier, required: true),
                new("projectileId", Ra2ContentTemplateParameterKind.Identifier, required: true),
                new("image", Ra2ContentTemplateParameterKind.Reference, required: true),
                new("rot", Ra2ContentTemplateParameterKind.Integer, required: true),
                new("antiAir", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("antiGround", Ra2ContentTemplateParameterKind.Boolean, required: true)
            ],
            [
                new(
                    Ra2ContentTemplateValueSource.Parameter("weaponId"),
                    Ra2SectionKind.Weapon,
                    [new("Projectile", Ra2ContentTemplateValueSource.Parameter("projectileId"))],
                    Ra2ContentTemplateSectionTargetMode.RequireExisting),
                new(
                    Ra2ContentTemplateValueSource.Parameter("projectileId"),
                    Ra2SectionKind.Projectile,
                    [
                        new("Image", Ra2ContentTemplateValueSource.Parameter("image")),
                        new("ROT", Ra2ContentTemplateValueSource.Parameter("rot")),
                        new("AA", Ra2ContentTemplateValueSource.Parameter("antiAir")),
                        new("AG", Ra2ContentTemplateValueSource.Parameter("antiGround"))
                    ])
            ]);

    private static IReadOnlyList<Ra2AutomationTemplateParameterDescriptor> CreateYrCoreWarheadParameterDescriptors()
        => Array.AsReadOnly<Ra2AutomationTemplateParameterDescriptor>(
        [
            new("weaponId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
            new("warheadId", Ra2AutomationTemplateParameterKind.Identifier, required: true, defaultValue: null),
            new("verses", Ra2AutomationTemplateParameterKind.String, required: true, defaultValue: null),
            new("infDeath", Ra2AutomationTemplateParameterKind.Integer, required: true, defaultValue: null),
            new("cellSpread", Ra2AutomationTemplateParameterKind.Float, required: true, defaultValue: null),
            new("percentAtMax", Ra2AutomationTemplateParameterKind.Float, required: true, defaultValue: null),
            new("proneDamage", Ra2AutomationTemplateParameterKind.Float, required: true, defaultValue: null),
            new("conventional", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
            new("wall", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
            new("wood", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
            new("rocker", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
            new("sparky", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
            new("tiberium", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null),
            new("bright", Ra2AutomationTemplateParameterKind.Boolean, required: true, defaultValue: null)
        ]);

    private static Ra2ContentTemplateDefinition CreateYrCoreWarheadDefinition()
        => new(
            YrCoreWarheadTemplateId,
            YrCoreWarheadTemplateVersion,
            "Complete Yuri's Revenge core Warhead",
            [
                new("weaponId", Ra2ContentTemplateParameterKind.Identifier, required: true),
                new("warheadId", Ra2ContentTemplateParameterKind.Identifier, required: true),
                new("verses", Ra2ContentTemplateParameterKind.String, required: true),
                new("infDeath", Ra2ContentTemplateParameterKind.Integer, required: true),
                new("cellSpread", Ra2ContentTemplateParameterKind.Float, required: true),
                new("percentAtMax", Ra2ContentTemplateParameterKind.Float, required: true),
                new("proneDamage", Ra2ContentTemplateParameterKind.Float, required: true),
                new("conventional", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("wall", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("wood", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("rocker", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("sparky", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("tiberium", Ra2ContentTemplateParameterKind.Boolean, required: true),
                new("bright", Ra2ContentTemplateParameterKind.Boolean, required: true)
            ],
            [
                new(
                    Ra2ContentTemplateValueSource.Parameter("weaponId"),
                    Ra2SectionKind.Weapon,
                    [new("Warhead", Ra2ContentTemplateValueSource.Parameter("warheadId"))],
                    Ra2ContentTemplateSectionTargetMode.RequireExisting),
                new(
                    Ra2ContentTemplateValueSource.Parameter("warheadId"),
                    Ra2SectionKind.Warhead,
                    [
                        new("Verses", Ra2ContentTemplateValueSource.Parameter("verses")),
                        new("InfDeath", Ra2ContentTemplateValueSource.Parameter("infDeath")),
                        new("CellSpread", Ra2ContentTemplateValueSource.Parameter("cellSpread")),
                        new("PercentAtMax", Ra2ContentTemplateValueSource.Parameter("percentAtMax")),
                        new("ProneDamage", Ra2ContentTemplateValueSource.Parameter("proneDamage")),
                        new("Conventional", Ra2ContentTemplateValueSource.Parameter("conventional")),
                        new("Wall", Ra2ContentTemplateValueSource.Parameter("wall")),
                        new("Wood", Ra2ContentTemplateValueSource.Parameter("wood")),
                        new("Rocker", Ra2ContentTemplateValueSource.Parameter("rocker")),
                        new("Sparky", Ra2ContentTemplateValueSource.Parameter("sparky")),
                        new("Tiberium", Ra2ContentTemplateValueSource.Parameter("tiberium")),
                        new("Bright", Ra2ContentTemplateValueSource.Parameter("bright"))
                    ])
            ]);

    private static IReadOnlyDictionary<string, string> ToArgumentMap(Ra2AutomationTemplateExpansionRequest request)
        => request.Arguments
            .GroupBy(argument => argument.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);

    private static bool TryGetInteger(IReadOnlyDictionary<string, string> arguments, string name, out int value)
    {
        value = default;
        return arguments.TryGetValue(name, out string? text) &&
               int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryGetFiniteDouble(IReadOnlyDictionary<string, string> arguments, string name, out double value)
    {
        value = default;
        return arguments.TryGetValue(name, out string? text) &&
               double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
               double.IsFinite(value);
    }

    private static bool HasValidVerses(IReadOnlyDictionary<string, string> arguments, string name)
        => arguments.TryGetValue(name, out string? value) && IsValidVerses(value);

    private static bool IsValidVerses(string value)
    {
        string[] tokens = value.Split(',', StringSplitOptions.TrimEntries);
        if (tokens.Length != 11)
            return false;

        foreach (string token in tokens)
        {
            if (token.Length < 2 || token[^1] != '%' ||
                !double.TryParse(token[..^1], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double parsed) ||
                !double.IsFinite(parsed))
            {
                return false;
            }
        }

        return true;
    }
}
