namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal sealed class Ra2VoxelSkillIdentity
{
    internal Ra2VoxelSkillIdentity(string skillId, string revision, string contentHash)
    {
        SkillId = Ra2VoxelColourContractIdentity.RequireIdentifier(skillId, nameof(skillId));
        Revision = Ra2VoxelColourContractIdentity.RequireIdentifier(revision, nameof(revision));
        ContentHash = Ra2VoxelColourContractIdentity.RequireSha256(contentHash, nameof(contentHash));
    }

    internal string SkillId { get; }
    internal string Revision { get; }
    internal string ContentHash { get; }
}

internal sealed record Ra2VoxelColourMaterializationContext(
    Ra2VoxelSceneSnapshot Source,
    Ra2CompiledVoxelStylePlan RawPlan,
    Ra2VoxelSemanticMaskComposition Composition,
    Ra2VoxelSemanticColourRequirements Requirements,
    Ra2VoxelSemanticColourBindingPlan BindingPlan,
    Ra2VoxelUnitClassEvidence Evidence,
    Ra2VoxelConfirmedUnitClass Confirmation,
    Ra2VoxelSkillIdentity ColourSkill,
    Ra2VoxelBaseColourSelection BaseColour,
    Ra2VoxelColourTechniquePolicy Technique,
    Ra2VoxelUnitAdaptationPolicy Adaptation,
    string BindingSchemaRevision = "ra2-voxel-semantic-colour-binding/1");

internal enum Ra2VoxelColourMaterializationFailureKind
{
    None = 0,
    IdentityMismatch,
    BaseColourInvalid,
    PaletteFamilyUnavailable,
    NormalizedPlanInvalid,
    SemanticBindingInvalid,
    ColourizationFailed,
    QualityBlocked,
    Cancelled,
    AnalysisFailed
}

internal sealed record Ra2VoxelColourMaterializationCandidate(
    Ra2CompiledVoxelStylePlan Plan,
    Ra2VoxelColourizationResult Colourization,
    Ra2VoxelColourQualityReport Quality,
    string BundleHash,
    bool IsContrast,
    Ra2VoxelPaletteContrastFacts? ContrastFacts);

internal sealed record Ra2VoxelColourMaterializationResult(
    Ra2VoxelColourMaterializationFailureKind FailureKind,
    string Message,
    Ra2CompiledVoxelStylePlan? NormalizedPlan,
    Ra2VoxelColourFamilySelection? FamilySelection,
    Ra2VoxelSemanticStyleIntegrationResult? SemanticIntegration,
    Ra2VoxelColourMaterializationCandidate? Ordinary,
    Ra2VoxelColourMaterializationCandidate? Contrast)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelColourMaterializationFailureKind.None &&
                               Ordinary is { Quality.State: not Ra2VoxelColourAdmissionState.Blocked };
}

internal static class Ra2VoxelSemanticColourMaterializer
{
    internal const string NormalizerRevision = "ra2-voxel-style-normalizer/1";

    internal static Ra2VoxelColourMaterializationResult Materialize(
        Ra2VoxelColourMaterializationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? identityFailure = ValidateIdentities(context);
            if (identityFailure is not null)
                return Failure(Ra2VoxelColourMaterializationFailureKind.IdentityMismatch, identityFailure);

            Ra2VoxelColourFamilyResult familyResult = Ra2VoxelColourFamilySelector.Select(
                context.Source.Palette,
                context.BaseColour,
                context.Technique,
                context.Adaptation,
                context.RawPlan);
            if (!familyResult.IsSuccess || familyResult.Selection is null)
                return Failure(Ra2VoxelColourMaterializationFailureKind.PaletteFamilyUnavailable, familyResult.Message);

            Ra2VoxelStylePlanCompilationResult normalized = Normalize(context, familyResult.Selection);
            if (!normalized.IsSuccess || normalized.Plan is null)
                return Failure(Ra2VoxelColourMaterializationFailureKind.NormalizedPlanInvalid, normalized.Message);

            Ra2VoxelSemanticStyleIntegrationResult integration = Ra2VoxelSemanticStyleIntegrator.Integrate(
                normalized.Plan,
                context.Composition,
                context.Requirements,
                context.BindingPlan,
                context.RawPlan.PlanHash);
            string ordinaryBundleHash = ComputeBundleHash(context, integration.Plan.PlanHash, contrast: false);
            Ra2VoxelColourizationResult ordinaryColourization = Ra2VoxelColourizer.Colourize(
                context.Source,
                integration.Plan,
                integration.Masks,
                context.Adaptation.DualSurfacePolicy,
                cancellationToken);
            if (!ordinaryColourization.IsSuccess)
                return Failure(Ra2VoxelColourMaterializationFailureKind.ColourizationFailed, ordinaryColourization.Message,
                    normalized.Plan, familyResult.Selection, integration);

            Ra2VoxelColourQualityReport ordinaryQuality = Ra2VoxelColourQualityEvaluator.Evaluate(
                context.Source,
                integration.Plan,
                ordinaryColourization,
                context.Composition,
                context.Requirements,
                context.BindingPlan,
                context.BaseColour,
                context.Technique,
                context.Adaptation,
                familyResult.Selection,
                context.Evidence,
                context.Confirmation,
                context.ColourSkill,
                ordinaryBundleHash);
            Ra2VoxelColourMaterializationCandidate ordinary = new(
                integration.Plan,
                ordinaryColourization,
                ordinaryQuality,
                ordinaryBundleHash,
                IsContrast: false,
                ContrastFacts: null);
            if (ordinaryQuality.State == Ra2VoxelColourAdmissionState.Blocked)
                return Failure(Ra2VoxelColourMaterializationFailureKind.QualityBlocked,
                    "The ordinary colour candidate failed a hard quality gate.", normalized.Plan,
                    familyResult.Selection, integration, ordinary);

            Ra2VoxelColourMaterializationCandidate? contrastCandidate = null;
            try
            {
                HashSet<string> protectedRoles = context.BindingPlan.Bindings
                    .Where(value => value.BindingMode == Ra2VoxelSemanticColourBindingMode.DirectRole)
                    .Select(value => value.RoleId)
                    .ToHashSet(StringComparer.Ordinal);
                Ra2VoxelPaletteContrastResult contrast = Ra2VoxelPaletteContrastOptimizer.Optimize(
                    integration.Plan,
                    context.Source.Palette,
                    context.BaseColour,
                    context.Technique,
                    context.Adaptation,
                    context.RawPlan,
                    protectedRoles);
                if (contrast.Facts.ChangedRoleCount > 0)
                {
                    Ra2VoxelColourFamilyResult contrastFamily = Ra2VoxelColourFamilySelector.Select(
                        context.Source.Palette,
                        context.BaseColour,
                        context.Technique,
                        context.Adaptation,
                        context.RawPlan,
                        contrast: true);
                    if (contrastFamily.IsSuccess && contrastFamily.Selection is not null)
                    {
                        string contrastBundleHash = ComputeBundleHash(context, contrast.Plan.PlanHash, contrast: true);
                        Ra2VoxelColourizationResult contrastColourization = Ra2VoxelColourizer.Colourize(
                            context.Source,
                            contrast.Plan,
                            integration.Masks,
                            context.Adaptation.DualSurfacePolicy,
                            cancellationToken);
                        if (contrastColourization.IsSuccess)
                        {
                            Ra2VoxelColourQualityReport contrastQuality = Ra2VoxelColourQualityEvaluator.Evaluate(
                                context.Source,
                                contrast.Plan,
                                contrastColourization,
                                context.Composition,
                                context.Requirements,
                                context.BindingPlan,
                                context.BaseColour,
                                context.Technique,
                                context.Adaptation,
                                contrastFamily.Selection,
                                context.Evidence,
                                context.Confirmation,
                                context.ColourSkill,
                                contrastBundleHash);
                            contrastCandidate = new(
                                contrast.Plan,
                                contrastColourization,
                                contrastQuality,
                                contrastBundleHash,
                                IsContrast: true,
                                contrast.Facts);
                        }
                    }
                }
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
            {
                // Contrast is optional; ordinary remains the authoritative candidate.
            }

            return new(
                Ra2VoxelColourMaterializationFailureKind.None,
                string.Empty,
                normalized.Plan,
                familyResult.Selection,
                integration,
                ordinary,
                contrastCandidate);
        }
        catch (OperationCanceledException)
        {
            return Failure(Ra2VoxelColourMaterializationFailureKind.Cancelled, "Colour materialization was cancelled.");
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            return Failure(Ra2VoxelColourMaterializationFailureKind.AnalysisFailed,
                "Colour materialization inputs could not be processed safely.");
        }
    }

    private static string? ValidateIdentities(Ra2VoxelColourMaterializationContext context)
    {
        if (!string.Equals(context.Source.CanonicalHash, context.Composition.SourceSnapshotHash, StringComparison.Ordinal) ||
            context.Source.OccupancyCount != context.Composition.CellCount ||
            !string.Equals(context.Requirements.SourceSnapshotHash, context.Source.CanonicalHash, StringComparison.Ordinal) ||
            !string.Equals(context.Requirements.CompositionHash, context.Composition.CompositionHash, StringComparison.Ordinal) ||
            !string.Equals(context.BindingPlan.RequirementShapeHash, context.Requirements.RequirementShapeHash, StringComparison.Ordinal) ||
            !string.Equals(context.BindingPlan.CompiledPlanHash, context.RawPlan.PlanHash, StringComparison.Ordinal) ||
            !string.Equals(context.RawPlan.PaletteHash, context.Source.Palette.ProfileHash, StringComparison.Ordinal))
        {
            return "Snapshot, composition, requirements, binding, plan, or palette identity is stale.";
        }
        if (!string.Equals(context.Evidence.EvidenceHash, context.Confirmation.EvidenceHash, StringComparison.Ordinal) ||
            context.Adaptation.UnitClass != context.Confirmation.UnitClass ||
            !string.Equals(context.Adaptation.ColouringSkillId, context.ColourSkill.SkillId, StringComparison.Ordinal) ||
            context.Confirmation.Source != Ra2VoxelUnitClassConfirmationSource.HumanManualSelection)
        {
            return "Unit-class confirmation, adaptation, or exact Skill route identity is stale.";
        }
        if (!string.Equals(context.BaseColour.PaletteProfileHash, context.Source.Palette.ProfileHash, StringComparison.OrdinalIgnoreCase) ||
            context.Source.Palette.IsTransparent(context.BaseColour.PaletteIndex) ||
            context.Source.Palette.IsRemap(context.BaseColour.PaletteIndex) ||
            context.Source.Palette[context.BaseColour.PaletteIndex] != context.BaseColour.ResolvedRgba)
        {
            return "The human base-colour selection is invalid for the active palette.";
        }
        if (!string.Equals(context.Technique.PolicyHash,
                Ra2VoxelColourTechniqueCatalog.Find(context.Technique.TechniqueId)?.PolicyHash, StringComparison.Ordinal) ||
            !string.Equals(context.Adaptation.PolicyHash,
                Ra2VoxelUnitAdaptationCatalog.For(context.Confirmation.UnitClass).PolicyHash, StringComparison.Ordinal) ||
            !string.Equals(context.BindingSchemaRevision, "ra2-voxel-semantic-colour-binding/1", StringComparison.Ordinal))
        {
            return "Technique, adaptation, metric, quality, or binding-schema policy identity is invalid.";
        }
        return null;
    }

    private static Ra2VoxelStylePlanCompilationResult Normalize(
        Ra2VoxelColourMaterializationContext context,
        Ra2VoxelColourFamilySelection family)
    {
        Ra2CompiledVoxelStylePlan raw = context.RawPlan;
        Ra2CompiledVoxelStyleRole bodyBase = FindGeometryRole(
            Ra2VoxelStyleRegionKind.WholePart, Ra2VoxelStyleRoleCategory.BodyBase);
        Ra2CompiledVoxelStyleRole bodyLight = FindGeometryRole(
            Ra2VoxelStyleRegionKind.TopExposed, Ra2VoxelStyleRoleCategory.BodyLight);
        Ra2CompiledVoxelStyleRole bodyMid = FindGeometryRole(
            Ra2VoxelStyleRegionKind.SideExposed, Ra2VoxelStyleRoleCategory.BodyMid);
        Ra2CompiledVoxelStyleRole bodyDark = FindGeometryRole(
            Ra2VoxelStyleRegionKind.Interior, Ra2VoxelStyleRoleCategory.BodyDark);
        Ra2CompiledVoxelStyleRole underside = FindGeometryRole(
            Ra2VoxelStyleRegionKind.UnderExposed, Ra2VoxelStyleRoleCategory.Underside);
        Ra2CompiledVoxelStyleRole edge = raw.Rules
            .Where(value => value.IsPaintable && value.Region == Ra2VoxelStyleRegionKind.EdgeOrRidge)
            .Select(value => raw.Roles.Single(role => role.Id == value.RoleId))
            .FirstOrDefault() ?? bodyLight;

        HashSet<string> topIds = raw.Rules.Where(value => value.Region == Ra2VoxelStyleRegionKind.TopExposed)
            .Select(value => value.RoleId).ToHashSet(StringComparer.Ordinal);
        HashSet<string> edgeIds = raw.Rules.Where(value => value.Region == Ra2VoxelStyleRegionKind.EdgeOrRidge)
            .Select(value => value.RoleId).ToHashSet(StringComparer.Ordinal);
        List<string> assumptions = raw.UnresolvedAssumptions.ToList();
        if (raw.Roles.Any(value => value.Category == Ra2VoxelStyleRoleCategory.BodyBase &&
                                   value.PaletteIndex != context.BaseColour.PaletteIndex))
        {
            assumptions.Add("Provider body-base colour intent was overridden by the human palette anchor.");
        }
        assumptions.AddRange(family.Warnings);

        List<Ra2VoxelStyleRoleDefinition> roles = [];
        foreach (Ra2CompiledVoxelStyleRole role in raw.Roles)
        {
            Ra2VoxelBodyColourRole? familyRole = role.Category switch
            {
                Ra2VoxelStyleRoleCategory.BodyBase => Ra2VoxelBodyColourRole.BodyBase,
                Ra2VoxelStyleRoleCategory.BodyLight when edgeIds.Contains(role.Id) && !topIds.Contains(role.Id) => Ra2VoxelBodyColourRole.EdgeOrRidge,
                Ra2VoxelStyleRoleCategory.BodyLight => Ra2VoxelBodyColourRole.BodyLight,
                Ra2VoxelStyleRoleCategory.BodyMid => Ra2VoxelBodyColourRole.BodyMid,
                Ra2VoxelStyleRoleCategory.BodyDark => Ra2VoxelBodyColourRole.BodyDark,
                Ra2VoxelStyleRoleCategory.Underside => Ra2VoxelBodyColourRole.Underside,
                _ => null
            };
            if (familyRole == Ra2VoxelBodyColourRole.BodyBase)
            {
                roles.Add(new(role.Id, role.Category, context.BaseColour.PaletteIndex, null, role.SourceScopeIds));
            }
            else if (familyRole.HasValue)
            {
                byte selected = family[familyRole.Value].PaletteIndex;
                roles.Add(new(role.Id, role.Category, null, context.Source.Palette[selected], role.SourceScopeIds));
            }
            else
            {
                roles.Add(new(role.Id, role.Category, role.RequestedExactPaletteIndex, role.RequestedColour, role.SourceScopeIds));
            }
        }

        List<Ra2VoxelStyleRuleDefinition> rules =
        [
            Geometry(Ra2VoxelStyleRegionKind.WholePart, bodyBase),
            Geometry(Ra2VoxelStyleRegionKind.Interior, bodyDark),
            Geometry(Ra2VoxelStyleRegionKind.SideExposed, bodyMid),
            Geometry(Ra2VoxelStyleRegionKind.TopExposed, bodyLight),
            Geometry(Ra2VoxelStyleRegionKind.UnderExposed, underside)
        ];
        if (context.Technique.EdgePolicy != Ra2VoxelColourEdgePolicy.None)
            rules.Add(Geometry(Ra2VoxelStyleRegionKind.EdgeOrRidge, edge));

        string[] scopes = raw.Roles.SelectMany(value => value.SourceScopeIds)
            .Concat(raw.Rules.SelectMany(value => value.SourceScopeIds))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return Ra2VoxelStylePlanCompiler.Compile(
            new(
                raw.Title,
                raw.Summary,
                raw.SourcePackHash,
                raw.PaletteHash,
                raw.CompilerRevision + "+" + NormalizerRevision,
                raw.ModelIdentity,
                context.Requirements.ApprovedRemapCellCount > 0
                    ? Ra2VoxelStyleRemapPolicy.ExplicitMask
                    : Ra2VoxelStyleRemapPolicy.None,
                bodyDark.Id,
                roles,
                rules,
                assumptions.Distinct(StringComparer.Ordinal)),
            context.Source.Palette,
            scopes);

        Ra2CompiledVoxelStyleRole FindGeometryRole(
            Ra2VoxelStyleRegionKind region,
            Ra2VoxelStyleRoleCategory expectedCategory)
        {
            Ra2CompiledVoxelStyleRole? selected = raw.Rules
                .Where(value => value.IsPaintable && value.Region == region)
                .Select(value => raw.Roles.Single(role => role.Id == value.RoleId))
                .SingleOrDefault();
            selected ??= raw.Roles.FirstOrDefault(value => value.Category == expectedCategory);
            if (selected is null || selected.Category != expectedCategory)
                throw new InvalidOperationException($"The raw style plan is missing the required {region}/{expectedCategory} geometry role.");
            return selected;
        }

        static Ra2VoxelStyleRuleDefinition Geometry(
            Ra2VoxelStyleRegionKind region,
            Ra2CompiledVoxelStyleRole role) => new(
                region,
                role.Id,
                Ra2VoxelStyleEvidenceKind.DeterministicGeometry,
                null,
                role.SourceScopeIds);
    }

    private static string ComputeBundleHash(
        Ra2VoxelColourMaterializationContext context,
        string materializedPlanHash,
        bool contrast) => Ra2VoxelColourContractIdentity.ComputeHash(writer =>
    {
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-colour-materialization-bundle/2");
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.RawPlan.PlanHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.BindingPlan.BindingPlanHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Source.Palette.ProfileHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.BaseColour.SelectionHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Evidence.EvidenceHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Confirmation.ConfirmationHash);
        WriteSkill(writer, context.ColourSkill);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Technique.TechniqueId);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Technique.Revision);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Technique.PolicyHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Adaptation.AdaptationId);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Adaptation.Revision);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Adaptation.PolicyHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Requirements.RequirementShapeHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.Composition.CompositionHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, context.BindingSchemaRevision);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Ra2VoxelColourTechniquePolicy.LuminanceMetricId);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Ra2VoxelColourTechniquePolicy.ColourFamilyMetricId);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Ra2VoxelColourQualityEvaluator.QualityPolicyHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, materializedPlanHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer,
            contrast ? Ra2VoxelPaletteContrastOptimizer.PolicyAwareRevision : string.Empty);
    });

    private static void WriteSkill(BinaryWriter writer, Ra2VoxelSkillIdentity skill)
    {
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, skill.SkillId);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, skill.Revision);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, skill.ContentHash);
    }

    private static Ra2VoxelColourMaterializationResult Failure(
        Ra2VoxelColourMaterializationFailureKind kind,
        string message,
        Ra2CompiledVoxelStylePlan? normalized = null,
        Ra2VoxelColourFamilySelection? family = null,
        Ra2VoxelSemanticStyleIntegrationResult? integration = null,
        Ra2VoxelColourMaterializationCandidate? ordinary = null)
        => new(kind, message, normalized, family, integration, ordinary, null);
}
