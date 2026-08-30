namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelSemanticColourRequirementKind
{
    PaintedSurface = 0,
    Glass,
    Rubber,
    BareMetal,
    Light,
    DarkOpening,
    Accent,
    ApprovedRemap
}

internal sealed record Ra2VoxelSemanticMaterialCount(
    Ra2VoxelSemanticMaterialRole MaterialRole,
    int CellCount);

internal sealed record Ra2VoxelSemanticColourRequirement(
    Ra2VoxelSemanticColourRequirementKind Kind,
    int CellCount);

internal sealed class Ra2VoxelSemanticColourRequirements
{
    private readonly Ra2VoxelSemanticMaterialCount[] _materialCounts;
    private readonly Ra2VoxelSemanticColourRequirement[] _required;

    internal Ra2VoxelSemanticColourRequirements(
        string sourceSnapshotHash,
        string compositionHash,
        IEnumerable<Ra2VoxelSemanticMaterialCount> materialCounts,
        int approvedRemapCellCount)
    {
        SourceSnapshotHash = Ra2VoxelColourContractIdentity.RequireSha256(sourceSnapshotHash, nameof(sourceSnapshotHash));
        CompositionHash = Ra2VoxelColourContractIdentity.RequireSha256(compositionHash, nameof(compositionHash));
        _materialCounts = (materialCounts ?? throw new ArgumentNullException(nameof(materialCounts)))
            .OrderBy(value => value.MaterialRole)
            .ToArray();
        if (_materialCounts.Length != Enum.GetValues<Ra2VoxelSemanticMaterialRole>().Length ||
            _materialCounts.Select(value => value.MaterialRole).Distinct().Count() != _materialCounts.Length ||
            _materialCounts.Any(value => !Enum.IsDefined(value.MaterialRole) || value.CellCount < 0))
        {
            throw new ArgumentException("Semantic material counts must contain every material exactly once.", nameof(materialCounts));
        }
        if (approvedRemapCellCount < 0)
            throw new ArgumentOutOfRangeException(nameof(approvedRemapCellCount));

        ApprovedRemapCellCount = approvedRemapCellCount;
        UnknownCellCount = _materialCounts.Single(value => value.MaterialRole == Ra2VoxelSemanticMaterialRole.Unknown).CellCount;
        _required = _materialCounts
            .Where(value => value.MaterialRole != Ra2VoxelSemanticMaterialRole.Unknown && value.CellCount > 0)
            .Select(value => new Ra2VoxelSemanticColourRequirement(Map(value.MaterialRole), value.CellCount))
            .Concat(approvedRemapCellCount > 0
                ? [new Ra2VoxelSemanticColourRequirement(Ra2VoxelSemanticColourRequirementKind.ApprovedRemap, approvedRemapCellCount)]
                : [])
            .OrderBy(value => value.Kind)
            .ToArray();
        RequirementShapeHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-semantic-colour-requirement-shape/1");
            writer.Write(_required.Length);
            foreach (Ra2VoxelSemanticColourRequirement requirement in _required)
                writer.Write((int)requirement.Kind);
        });
    }

    internal string SourceSnapshotHash { get; }
    internal string CompositionHash { get; }
    internal string RequirementShapeHash { get; }
    internal IReadOnlyList<Ra2VoxelSemanticMaterialCount> MaterialCounts => Array.AsReadOnly(_materialCounts);
    internal IReadOnlyList<Ra2VoxelSemanticColourRequirement> Required => Array.AsReadOnly(_required);
    internal int UnknownCellCount { get; }
    internal int ApprovedRemapCellCount { get; }
    internal int CellCount => _materialCounts.Sum(value => value.CellCount);

    private static Ra2VoxelSemanticColourRequirementKind Map(Ra2VoxelSemanticMaterialRole role) => role switch
    {
        Ra2VoxelSemanticMaterialRole.PaintedSurface => Ra2VoxelSemanticColourRequirementKind.PaintedSurface,
        Ra2VoxelSemanticMaterialRole.Glass => Ra2VoxelSemanticColourRequirementKind.Glass,
        Ra2VoxelSemanticMaterialRole.Rubber => Ra2VoxelSemanticColourRequirementKind.Rubber,
        Ra2VoxelSemanticMaterialRole.BareMetal => Ra2VoxelSemanticColourRequirementKind.BareMetal,
        Ra2VoxelSemanticMaterialRole.Light => Ra2VoxelSemanticColourRequirementKind.Light,
        Ra2VoxelSemanticMaterialRole.DarkOpening => Ra2VoxelSemanticColourRequirementKind.DarkOpening,
        Ra2VoxelSemanticMaterialRole.Accent => Ra2VoxelSemanticColourRequirementKind.Accent,
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };
}

internal static class Ra2VoxelSemanticColourRequirementsProjector
{
    internal static Ra2VoxelSemanticColourRequirements Project(Ra2VoxelSemanticMaskComposition composition)
    {
        ArgumentNullException.ThrowIfNull(composition);
        Ra2VoxelSemanticMaterialCount[] counts = Enum.GetValues<Ra2VoxelSemanticMaterialRole>()
            .Select(role => new Ra2VoxelSemanticMaterialCount(
                role,
                composition.Assignments.Count(value => value.MaterialRole == role)))
            .ToArray();
        int remap = composition.Assignments.Count(value =>
            value.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved);
        Ra2VoxelSemanticColourRequirements result = new(
            composition.SourceSnapshotHash,
            composition.CompositionHash,
            counts,
            remap);
        if (result.CellCount != composition.CellCount || result.ApprovedRemapCellCount > composition.CellCount)
            throw new InvalidOperationException("Semantic colour requirements failed their count invariant.");
        return result;
    }
}

internal enum Ra2VoxelSemanticColourBindingMode
{
    BodyGeometryFamily = 0,
    DirectRole
}

internal sealed record Ra2VoxelSemanticColourBinding(
    Ra2VoxelSemanticColourRequirementKind Requirement,
    Ra2VoxelSemanticColourBindingMode BindingMode,
    string RoleId);

internal enum Ra2VoxelSemanticColourBindingFailureKind
{
    None = 0,
    RequirementShapeMismatch,
    MissingBinding,
    DuplicateBinding,
    ExtraBinding,
    UnknownRole,
    IncompatibleBinding,
    LightAccentRoleConflict
}

internal sealed class Ra2VoxelSemanticColourBindingPlan
{
    private readonly Ra2VoxelSemanticColourBinding[] _bindings;

    private Ra2VoxelSemanticColourBindingPlan(
        string requirementShapeHash,
        string compiledPlanHash,
        IEnumerable<Ra2VoxelSemanticColourBinding> bindings,
        string bindingPlanHash)
    {
        RequirementShapeHash = requirementShapeHash;
        CompiledPlanHash = compiledPlanHash;
        _bindings = bindings.OrderBy(value => value.Requirement).ToArray();
        BindingPlanHash = bindingPlanHash;
    }

    internal string RequirementShapeHash { get; }
    internal string CompiledPlanHash { get; }
    internal IReadOnlyList<Ra2VoxelSemanticColourBinding> Bindings => Array.AsReadOnly(_bindings);
    internal string BindingPlanHash { get; }

    internal static Ra2VoxelSemanticColourBindingResult Validate(
        Ra2VoxelSemanticColourRequirements requirements,
        Ra2CompiledVoxelStylePlan compiledPlan,
        IEnumerable<Ra2VoxelSemanticColourBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(requirements);
        ArgumentNullException.ThrowIfNull(compiledPlan);
        Ra2VoxelSemanticColourBinding[] array = (bindings ?? throw new ArgumentNullException(nameof(bindings))).ToArray();
        if (array.Any(binding => !Enum.IsDefined(binding.Requirement) || !Enum.IsDefined(binding.BindingMode)))
            return Failure(Ra2VoxelSemanticColourBindingFailureKind.IncompatibleBinding, "A semantic colour binding contains an unknown enum value.");
        if (array.GroupBy(binding => binding.Requirement).Any(group => group.Count() > 1))
            return Failure(Ra2VoxelSemanticColourBindingFailureKind.DuplicateBinding, "A semantic colour requirement is bound more than once.");

        HashSet<Ra2VoxelSemanticColourRequirementKind> expected = requirements.Required
            .Select(value => value.Kind)
            .ToHashSet();
        HashSet<Ra2VoxelSemanticColourRequirementKind> actual = array
            .Select(value => value.Requirement)
            .ToHashSet();
        if (expected.Except(actual).Any())
            return Failure(Ra2VoxelSemanticColourBindingFailureKind.MissingBinding, "A semantic colour requirement has no binding.");
        if (actual.Except(expected).Any())
            return Failure(Ra2VoxelSemanticColourBindingFailureKind.ExtraBinding, "A binding exists for a material that is not required.");

        Dictionary<string, Ra2CompiledVoxelStyleRole> roles = compiledPlan.Roles
            .ToDictionary(role => role.Id, StringComparer.Ordinal);
        if (expected.Contains(Ra2VoxelSemanticColourRequirementKind.PaintedSurface))
        {
            Ra2VoxelStyleRoleCategory[] requiredBodyCategories =
            [
                Ra2VoxelStyleRoleCategory.BodyBase,
                Ra2VoxelStyleRoleCategory.BodyLight,
                Ra2VoxelStyleRoleCategory.BodyMid,
                Ra2VoxelStyleRoleCategory.BodyDark,
                Ra2VoxelStyleRoleCategory.Underside
            ];
            if (requiredBodyCategories.Any(category => compiledPlan.Roles.All(role => role.Category != category)))
            {
                return Failure(
                    Ra2VoxelSemanticColourBindingFailureKind.IncompatibleBinding,
                    "PaintedSurface requires a complete body geometry role family.");
            }
        }
        foreach (Ra2VoxelSemanticColourBinding binding in array)
        {
            if (string.IsNullOrWhiteSpace(binding.RoleId) || binding.RoleId.Length > 64)
                return Failure(Ra2VoxelSemanticColourBindingFailureKind.UnknownRole, "A semantic colour binding role id is invalid.");
            if (!roles.TryGetValue(binding.RoleId, out Ra2CompiledVoxelStyleRole? role))
                return Failure(Ra2VoxelSemanticColourBindingFailureKind.UnknownRole, "A semantic colour binding references an unknown style role.");
            if (!IsCompatible(binding, role.Category))
                return Failure(Ra2VoxelSemanticColourBindingFailureKind.IncompatibleBinding, "A semantic colour binding has an incompatible mode or role category.");
        }

        Ra2VoxelSemanticColourBinding? light = array.SingleOrDefault(value => value.Requirement == Ra2VoxelSemanticColourRequirementKind.Light);
        Ra2VoxelSemanticColourBinding? accent = array.SingleOrDefault(value => value.Requirement == Ra2VoxelSemanticColourRequirementKind.Accent);
        if (light is not null && accent is not null && string.Equals(light.RoleId, accent.RoleId, StringComparison.Ordinal))
            return Failure(Ra2VoxelSemanticColourBindingFailureKind.LightAccentRoleConflict, "Light and Accent must use different style role ids.");

        string hash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-semantic-colour-binding/1");
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, requirements.RequirementShapeHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, compiledPlan.PlanHash);
            writer.Write(array.Length);
            foreach (Ra2VoxelSemanticColourBinding binding in array.OrderBy(value => value.Requirement))
            {
                writer.Write((int)binding.Requirement);
                writer.Write((int)binding.BindingMode);
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, binding.RoleId);
            }
        });
        return new(
            Ra2VoxelSemanticColourBindingFailureKind.None,
            string.Empty,
            new Ra2VoxelSemanticColourBindingPlan(
                requirements.RequirementShapeHash,
                compiledPlan.PlanHash,
                array,
                hash));
    }

    private static bool IsCompatible(
        Ra2VoxelSemanticColourBinding binding,
        Ra2VoxelStyleRoleCategory category)
        => binding.Requirement switch
        {
            Ra2VoxelSemanticColourRequirementKind.PaintedSurface =>
                binding.BindingMode == Ra2VoxelSemanticColourBindingMode.BodyGeometryFamily &&
                category == Ra2VoxelStyleRoleCategory.BodyBase,
            Ra2VoxelSemanticColourRequirementKind.Glass => Direct(binding, category, Ra2VoxelStyleRoleCategory.Glass),
            Ra2VoxelSemanticColourRequirementKind.Rubber => Direct(binding, category, Ra2VoxelStyleRoleCategory.Rubber),
            Ra2VoxelSemanticColourRequirementKind.BareMetal => Direct(binding, category, Ra2VoxelStyleRoleCategory.BareMetal),
            Ra2VoxelSemanticColourRequirementKind.Light or Ra2VoxelSemanticColourRequirementKind.Accent =>
                Direct(binding, category, Ra2VoxelStyleRoleCategory.Accent),
            Ra2VoxelSemanticColourRequirementKind.DarkOpening => Direct(binding, category, Ra2VoxelStyleRoleCategory.BodyDark),
            Ra2VoxelSemanticColourRequirementKind.ApprovedRemap => Direct(binding, category, Ra2VoxelStyleRoleCategory.Remap),
            _ => false
        };

    private static bool Direct(
        Ra2VoxelSemanticColourBinding binding,
        Ra2VoxelStyleRoleCategory actual,
        Ra2VoxelStyleRoleCategory expected)
        => binding.BindingMode == Ra2VoxelSemanticColourBindingMode.DirectRole && actual == expected;

    private static Ra2VoxelSemanticColourBindingResult Failure(
        Ra2VoxelSemanticColourBindingFailureKind kind,
        string message)
        => new(kind, message, null);
}

internal sealed record Ra2VoxelSemanticColourBindingResult(
    Ra2VoxelSemanticColourBindingFailureKind FailureKind,
    string Message,
    Ra2VoxelSemanticColourBindingPlan? Plan)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelSemanticColourBindingFailureKind.None && Plan is not null;
}
