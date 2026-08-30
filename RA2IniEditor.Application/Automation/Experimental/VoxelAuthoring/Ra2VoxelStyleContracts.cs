using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelStyleRoleCategory
{
    BodyBase = 0,
    BodyLight,
    BodyMid,
    BodyDark,
    Underside,
    Glass,
    Rubber,
    BareMetal,
    Accent,
    Remap
}

internal enum Ra2VoxelStyleRegionKind
{
    WholePart = 0,
    TopExposed,
    SideExposed,
    UnderExposed,
    EdgeOrRidge,
    Interior,
    ExplicitMask,
    DonorMask,
    SourceMaterialMask
}

internal enum Ra2VoxelStyleEvidenceKind
{
    DeterministicGeometry = 0,
    ExplicitUserMask,
    DonorProjection,
    SourceMaterial,
    InferredTextOnly
}

internal enum Ra2VoxelStyleRemapPolicy
{
    None = 0,
    ExplicitMask
}

internal enum Ra2VoxelStylePlanFailureKind
{
    None = 0,
    MalformedProposal,
    SourceScopeMismatch,
    UnknownColourRole,
    PaletteMismatch,
    PaletteResolutionFailed,
    TransparentIndexSelected,
    RemapPolicyViolation,
    RuleConflict,
    CoverageViolation,
    ResourceLimitExceeded,
    AnalysisFailed
}

internal sealed record Ra2VoxelStyleRoleDefinition(
    string Id,
    Ra2VoxelStyleRoleCategory Category,
    byte? ExactPaletteIndex,
    Ra2Rgba32? TargetColour,
    IReadOnlyList<string> SourceScopeIds);

internal sealed record Ra2VoxelStyleRuleDefinition(
    Ra2VoxelStyleRegionKind Region,
    string RoleId,
    Ra2VoxelStyleEvidenceKind Evidence,
    string? MaskId,
    IReadOnlyList<string> SourceScopeIds);

internal sealed class Ra2VoxelStylePlanDefinition
{
    internal Ra2VoxelStylePlanDefinition(
        string title,
        string summary,
        string sourcePackHash,
        string paletteHash,
        string compilerRevision,
        string modelIdentity,
        Ra2VoxelStyleRemapPolicy remapPolicy,
        string interiorRoleId,
        IEnumerable<Ra2VoxelStyleRoleDefinition> roles,
        IEnumerable<Ra2VoxelStyleRuleDefinition> rules,
        IEnumerable<string>? unresolvedAssumptions = null)
    {
        Title = title ?? string.Empty;
        Summary = summary ?? string.Empty;
        SourcePackHash = sourcePackHash ?? string.Empty;
        PaletteHash = paletteHash ?? string.Empty;
        CompilerRevision = compilerRevision ?? string.Empty;
        ModelIdentity = modelIdentity ?? string.Empty;
        RemapPolicy = remapPolicy;
        InteriorRoleId = interiorRoleId ?? string.Empty;
        Roles = Array.AsReadOnly((roles ?? throw new ArgumentNullException(nameof(roles))).ToArray());
        Rules = Array.AsReadOnly((rules ?? throw new ArgumentNullException(nameof(rules))).ToArray());
        UnresolvedAssumptions = Array.AsReadOnly((unresolvedAssumptions ?? []).ToArray());
    }

    internal string Title { get; }
    internal string Summary { get; }
    internal string SourcePackHash { get; }
    internal string PaletteHash { get; }
    internal string CompilerRevision { get; }
    internal string ModelIdentity { get; }
    internal Ra2VoxelStyleRemapPolicy RemapPolicy { get; }
    internal string InteriorRoleId { get; }
    internal IReadOnlyList<Ra2VoxelStyleRoleDefinition> Roles { get; }
    internal IReadOnlyList<Ra2VoxelStyleRuleDefinition> Rules { get; }
    internal IReadOnlyList<string> UnresolvedAssumptions { get; }
}

internal sealed record Ra2CompiledVoxelStyleRole(
    string Id,
    Ra2VoxelStyleRoleCategory Category,
    byte PaletteIndex,
    byte? RequestedExactPaletteIndex,
    Ra2Rgba32? RequestedColour,
    IReadOnlyList<string> SourceScopeIds);

internal sealed record Ra2CompiledVoxelStyleRule(
    Ra2VoxelStyleRegionKind Region,
    string RoleId,
    Ra2VoxelStyleEvidenceKind Evidence,
    string? MaskId,
    bool IsPaintable,
    IReadOnlyList<string> SourceScopeIds);

internal sealed class Ra2CompiledVoxelStylePlan
{
    internal const int CurrentSchemaVersion = 1;
    private readonly Ra2CompiledVoxelStyleRole[] _roles;
    private readonly Ra2CompiledVoxelStyleRule[] _rules;
    private readonly string[] _unresolvedAssumptions;

    internal Ra2CompiledVoxelStylePlan(
        string title,
        string summary,
        string sourcePackHash,
        string paletteHash,
        string compilerRevision,
        string modelIdentity,
        Ra2VoxelStyleRemapPolicy remapPolicy,
        string interiorRoleId,
        IEnumerable<Ra2CompiledVoxelStyleRole> roles,
        IEnumerable<Ra2CompiledVoxelStyleRule> rules,
        IEnumerable<string> unresolvedAssumptions)
    {
        Title = title;
        Summary = summary;
        SourcePackHash = sourcePackHash;
        PaletteHash = paletteHash;
        CompilerRevision = compilerRevision;
        ModelIdentity = modelIdentity;
        RemapPolicy = remapPolicy;
        InteriorRoleId = interiorRoleId;
        _roles = roles.ToArray();
        _rules = rules.ToArray();
        _unresolvedAssumptions = unresolvedAssumptions.ToArray();
        PlanHash = ComputeHash();
    }

    internal string Title { get; }
    internal string Summary { get; }
    internal string SourcePackHash { get; }
    internal string PaletteHash { get; }
    internal string CompilerRevision { get; }
    internal string ModelIdentity { get; }
    internal Ra2VoxelStyleRemapPolicy RemapPolicy { get; }
    internal string InteriorRoleId { get; }
    internal IReadOnlyList<Ra2CompiledVoxelStyleRole> Roles => Array.AsReadOnly(_roles);
    internal IReadOnlyList<Ra2CompiledVoxelStyleRule> Rules => Array.AsReadOnly(_rules);
    internal IReadOnlyList<string> UnresolvedAssumptions => Array.AsReadOnly(_unresolvedAssumptions);
    internal string PlanHash { get; }

    private string ComputeHash()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(CurrentSchemaVersion);
        WriteString(writer, Title);
        WriteString(writer, Summary);
        WriteString(writer, SourcePackHash);
        WriteString(writer, PaletteHash);
        WriteString(writer, CompilerRevision);
        WriteString(writer, ModelIdentity);
        writer.Write((int)RemapPolicy);
        WriteString(writer, InteriorRoleId);
        writer.Write(_roles.Length);
        foreach (Ra2CompiledVoxelStyleRole role in _roles)
        {
            WriteString(writer, role.Id);
            writer.Write((int)role.Category);
            writer.Write(role.PaletteIndex);
            writer.Write(role.RequestedExactPaletteIndex.HasValue);
            if (role.RequestedExactPaletteIndex is byte exact)
                writer.Write(exact);
            writer.Write(role.RequestedColour.HasValue);
            if (role.RequestedColour is Ra2Rgba32 requested)
            {
                writer.Write(requested.Red);
                writer.Write(requested.Green);
                writer.Write(requested.Blue);
                writer.Write(requested.Alpha);
            }
            WriteStrings(writer, role.SourceScopeIds);
        }
        writer.Write(_rules.Length);
        foreach (Ra2CompiledVoxelStyleRule rule in _rules)
        {
            writer.Write((int)rule.Region);
            WriteString(writer, rule.RoleId);
            writer.Write((int)rule.Evidence);
            WriteString(writer, rule.MaskId ?? string.Empty);
            writer.Write(rule.IsPaintable);
            WriteStrings(writer, rule.SourceScopeIds);
        }
        WriteStrings(writer, _unresolvedAssumptions);
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    private static void WriteStrings(BinaryWriter writer, IEnumerable<string> values)
    {
        string[] array = values.ToArray();
        writer.Write(array.Length);
        foreach (string value in array)
            WriteString(writer, value);
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }
}

internal sealed record Ra2VoxelStylePlanCompilationResult(
    Ra2VoxelStylePlanFailureKind FailureKind,
    string Message,
    Ra2CompiledVoxelStylePlan? Plan)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelStylePlanFailureKind.None && Plan is not null;
}

internal static class Ra2VoxelStylePlanCompiler
{
    internal const int MaximumRoleCount = 32;
    internal const int MaximumRuleCount = 64;
    internal const int MaximumAssumptionCount = 32;
    internal const int MaximumTextLength = 512;

    internal static Ra2VoxelStylePlanCompilationResult Compile(
        Ra2VoxelStylePlanDefinition definition,
        Ra2VoxelPaletteProfile palette,
        IEnumerable<string> allowedSourceScopeIds)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(allowedSourceScopeIds);
        try
        {
            HashSet<string> scopes = allowedSourceScopeIds.ToHashSet(StringComparer.Ordinal);
            if (scopes.Count == 0 ||
                !IsBoundedText(definition.Title, MaximumTextLength) ||
                !IsBoundedText(definition.Summary, MaximumTextLength) ||
                !IsSha256(definition.SourcePackHash) ||
                !IsBoundedText(definition.CompilerRevision, 128) ||
                !IsBoundedText(definition.ModelIdentity, 256) ||
                !Enum.IsDefined(definition.RemapPolicy))
            {
                return Failure(Ra2VoxelStylePlanFailureKind.MalformedProposal, "The style plan metadata is invalid.");
            }
            if (!string.Equals(definition.PaletteHash, palette.ProfileHash, StringComparison.Ordinal))
                return Failure(Ra2VoxelStylePlanFailureKind.PaletteMismatch, "The style plan palette does not match the active palette.");
            if (definition.Roles.Count is < 1 or > MaximumRoleCount ||
                definition.Rules.Count is < 1 or > MaximumRuleCount ||
                definition.UnresolvedAssumptions.Count > MaximumAssumptionCount)
            {
                return Failure(Ra2VoxelStylePlanFailureKind.ResourceLimitExceeded, "The style plan exceeds its collection limits.");
            }
            if (definition.UnresolvedAssumptions.Any(value => !IsBoundedText(value, MaximumTextLength)))
                return Failure(Ra2VoxelStylePlanFailureKind.MalformedProposal, "A style plan assumption is invalid.");

            List<Ra2CompiledVoxelStyleRole> compiledRoles = [];
            HashSet<string> roleIds = new(StringComparer.Ordinal);
            foreach (Ra2VoxelStyleRoleDefinition role in definition.Roles)
            {
                if (!IsIdentifier(role.Id))
                    return Failure(Ra2VoxelStylePlanFailureKind.MalformedProposal, "A style colour role id is invalid.");
                if (!Enum.IsDefined(role.Category))
                    return Failure(Ra2VoxelStylePlanFailureKind.MalformedProposal, "A style colour role category is invalid.");
                if (!roleIds.Add(role.Id))
                    return Failure(Ra2VoxelStylePlanFailureKind.MalformedProposal, $"The style colour role id '{role.Id}' is duplicated.");
                if (!role.ExactPaletteIndex.HasValue && !role.TargetColour.HasValue)
                    return Failure(Ra2VoxelStylePlanFailureKind.MalformedProposal, $"The style colour role '{role.Id}' does not define a colour source.");
                if (role.ExactPaletteIndex.HasValue && role.TargetColour.HasValue)
                    return Failure(Ra2VoxelStylePlanFailureKind.MalformedProposal, $"The style colour role '{role.Id}' defines conflicting palette-index and RGB colour sources.");
                if (!ValidateScopes(role.SourceScopeIds, scopes))
                    return Failure(Ra2VoxelStylePlanFailureKind.SourceScopeMismatch, "A style colour role references an unknown source scope.");

                byte index;
                try
                {
                    index = ResolvePaletteIndex(role, palette);
                }
                catch (InvalidOperationException)
                {
                    return Failure(Ra2VoxelStylePlanFailureKind.PaletteResolutionFailed, "A style colour role cannot be resolved in the active palette.");
                }
                if (palette.IsTransparent(index))
                    return Failure(Ra2VoxelStylePlanFailureKind.TransparentIndexSelected, "A style colour role selected a transparent palette index.");
                bool isRemapRole = role.Category == Ra2VoxelStyleRoleCategory.Remap;
                if (isRemapRole != palette.IsRemap(index))
                    return Failure(Ra2VoxelStylePlanFailureKind.RemapPolicyViolation, "A style colour role violates the active remap palette policy.");
                compiledRoles.Add(new(
                    role.Id,
                    role.Category,
                    index,
                    role.ExactPaletteIndex,
                    role.TargetColour,
                    Copy(role.SourceScopeIds)));
            }

            if (!roleIds.Contains(definition.InteriorRoleId))
                return Failure(Ra2VoxelStylePlanFailureKind.UnknownColourRole, "The style plan interior role does not exist.");
            Ra2CompiledVoxelStyleRole interior = compiledRoles.First(role => role.Id == definition.InteriorRoleId);
            if (IsSemanticCategory(interior.Category))
                return Failure(Ra2VoxelStylePlanFailureKind.CoverageViolation, "The interior role cannot be a semantic material or remap role.");

            List<Ra2CompiledVoxelStyleRule> compiledRules = [];
            HashSet<(Ra2VoxelStyleRegionKind, string)> uniqueRules = [];
            foreach (Ra2VoxelStyleRuleDefinition rule in definition.Rules)
            {
                if (!Enum.IsDefined(rule.Region) || !Enum.IsDefined(rule.Evidence) ||
                    !roleIds.Contains(rule.RoleId) ||
                    !uniqueRules.Add((rule.Region, rule.MaskId ?? string.Empty)))
                {
                    return Failure(Ra2VoxelStylePlanFailureKind.RuleConflict, "A style region rule is invalid or conflicts with another rule.");
                }
                if (!ValidateScopes(rule.SourceScopeIds, scopes))
                    return Failure(Ra2VoxelStylePlanFailureKind.SourceScopeMismatch, "A style region rule references an unknown source scope.");
                Ra2CompiledVoxelStyleRole role = compiledRoles.First(candidate => candidate.Id == rule.RoleId);
                bool paintable = IsPaintable(rule, role.Category, definition.RemapPolicy);
                compiledRules.Add(new(
                    rule.Region,
                    rule.RoleId,
                    rule.Evidence,
                    NormalizeMaskId(rule.MaskId),
                    paintable,
                    Copy(rule.SourceScopeIds)));
            }

            if (!compiledRules.Any(rule => rule.Region == Ra2VoxelStyleRegionKind.WholePart && rule.IsPaintable))
                return Failure(Ra2VoxelStylePlanFailureKind.CoverageViolation, "The style plan requires one paintable WholePart base rule.");

            Ra2CompiledVoxelStylePlan plan = new(
                definition.Title.Trim(),
                definition.Summary.Trim(),
                definition.SourcePackHash,
                definition.PaletteHash,
                definition.CompilerRevision.Trim(),
                definition.ModelIdentity.Trim(),
                definition.RemapPolicy,
                definition.InteriorRoleId,
                compiledRoles,
                compiledRules,
                definition.UnresolvedAssumptions.Select(value => value.Trim()).Distinct(StringComparer.Ordinal));
            return new(Ra2VoxelStylePlanFailureKind.None, string.Empty, plan);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return Failure(Ra2VoxelStylePlanFailureKind.AnalysisFailed, "The style plan could not be compiled safely.");
        }
    }

    private static byte ResolvePaletteIndex(Ra2VoxelStyleRoleDefinition role, Ra2VoxelPaletteProfile palette)
    {
        if (role.ExactPaletteIndex is byte exact)
            return exact;
        return role.Category == Ra2VoxelStyleRoleCategory.Remap
            ? palette.FindNearestRemapIndex(role.TargetColour!.Value)
            : palette.FindNearestOpaqueNonRemapIndex(role.TargetColour!.Value);
    }

    private static bool IsPaintable(
        Ra2VoxelStyleRuleDefinition rule,
        Ra2VoxelStyleRoleCategory category,
        Ra2VoxelStyleRemapPolicy remapPolicy)
    {
        bool geometryRegion = rule.Region is Ra2VoxelStyleRegionKind.WholePart or
            Ra2VoxelStyleRegionKind.TopExposed or Ra2VoxelStyleRegionKind.SideExposed or
            Ra2VoxelStyleRegionKind.UnderExposed or Ra2VoxelStyleRegionKind.EdgeOrRidge or
            Ra2VoxelStyleRegionKind.Interior;
        if (geometryRegion)
            return rule.Evidence == Ra2VoxelStyleEvidenceKind.DeterministicGeometry && !IsSemanticCategory(category);

        bool evidenceMatches = rule.Region == Ra2VoxelStyleRegionKind.ExplicitMask &&
                               rule.Evidence == Ra2VoxelStyleEvidenceKind.ExplicitUserMask;
        if (!evidenceMatches || string.IsNullOrWhiteSpace(rule.MaskId))
            return false;
        if (category == Ra2VoxelStyleRoleCategory.Remap)
            return remapPolicy == Ra2VoxelStyleRemapPolicy.ExplicitMask &&
                   rule.Region == Ra2VoxelStyleRegionKind.ExplicitMask &&
                   rule.Evidence == Ra2VoxelStyleEvidenceKind.ExplicitUserMask;
        return true;
    }

    private static bool IsSemanticCategory(Ra2VoxelStyleRoleCategory category)
        => category is Ra2VoxelStyleRoleCategory.Glass or Ra2VoxelStyleRoleCategory.Rubber or
            Ra2VoxelStyleRoleCategory.BareMetal or Ra2VoxelStyleRoleCategory.Accent or
            Ra2VoxelStyleRoleCategory.Remap;

    private static bool ValidateScopes(IEnumerable<string> values, HashSet<string> allowed)
    {
        string[] array = values?.ToArray() ?? [];
        return array.Length is >= 1 and <= 8 &&
               array.All(value => IsBoundedText(value, 128) && allowed.Contains(value)) &&
               array.Distinct(StringComparer.Ordinal).Count() == array.Length;
    }

    private static IReadOnlyList<string> Copy(IEnumerable<string> values)
        => Array.AsReadOnly(values.ToArray());

    private static string? NormalizeMaskId(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsIdentifier(string value)
        => IsBoundedText(value, 64) && char.IsAsciiLetter(value[0]) &&
           value.All(character => char.IsAsciiLetterOrDigit(character) || character is '.' or '-' or '_');

    private static bool IsBoundedText(string value, int maximum)
        => !string.IsNullOrWhiteSpace(value) && value.Length <= maximum && value.IndexOfAny(['\r', '\n', '\0']) < 0;

    private static bool IsSha256(string value)
        => value.Length == 64 && value.All(character => char.IsAsciiHexDigit(character));

    private static Ra2VoxelStylePlanCompilationResult Failure(Ra2VoxelStylePlanFailureKind kind, string message)
        => new(kind, message, null);
}
