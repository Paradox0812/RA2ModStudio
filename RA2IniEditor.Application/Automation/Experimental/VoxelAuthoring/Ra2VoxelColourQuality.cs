namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelColourAdmissionState
{
    Blocked = 0,
    NeedsReview,
    ReviewReady
}

internal enum Ra2VoxelColourVisualAcceptance
{
    Pending = 0,
    HumanAccepted
}

internal sealed record Ra2VoxelColourQualityWarning(string Code, string Message);

internal sealed record Ra2VoxelColourQualityMetric(string Id, string Value);

internal sealed record Ra2VoxelColourRoleDistributionFact(
    string RoleId,
    int CellCount,
    int ConnectedComponentCount,
    int IsolatedCellCount,
    int BoundingBoxSpreadX,
    int BoundingBoxSpreadY,
    int BoundingBoxSpreadZ,
    int? LeftRightMismatchCount);

internal sealed class Ra2VoxelColourQualityReport
{
    private readonly Ra2VoxelColourQualityWarning[] _warnings;
    private readonly Ra2VoxelColourQualityMetric[] _metrics;
    private readonly Ra2VoxelColourRoleDistributionFact[] _distribution;

    internal Ra2VoxelColourQualityReport(
        Ra2VoxelColourAdmissionState state,
        string candidateHash,
        string bundleHash,
        IEnumerable<Ra2VoxelColourQualityWarning> warnings,
        IEnumerable<Ra2VoxelColourQualityMetric> metrics,
        IEnumerable<Ra2VoxelColourRoleDistributionFact> distribution)
    {
        State = state;
        CandidateHash = Ra2VoxelColourContractIdentity.RequireSha256(candidateHash, nameof(candidateHash));
        BundleHash = Ra2VoxelColourContractIdentity.RequireSha256(bundleHash, nameof(bundleHash));
        _warnings = warnings.OrderBy(value => value.Code, StringComparer.Ordinal)
            .ThenBy(value => value.Message, StringComparer.Ordinal).ToArray();
        _metrics = metrics.OrderBy(value => value.Id, StringComparer.Ordinal).ToArray();
        _distribution = distribution.OrderBy(value => value.RoleId, StringComparer.Ordinal).ToArray();
        VisualAcceptance = Ra2VoxelColourVisualAcceptance.Pending;
        ReportHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-colour-quality-report/4");
            writer.Write((int)State);
            writer.Write((int)VisualAcceptance);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, CandidateHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, BundleHash);
            writer.Write(_warnings.Length);
            foreach (Ra2VoxelColourQualityWarning warning in _warnings)
            {
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, warning.Code);
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, warning.Message);
            }
            writer.Write(_metrics.Length);
            foreach (Ra2VoxelColourQualityMetric metric in _metrics)
            {
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, metric.Id);
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, metric.Value);
            }
            writer.Write(_distribution.Length);
            foreach (Ra2VoxelColourRoleDistributionFact fact in _distribution)
            {
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, fact.RoleId);
                writer.Write(fact.CellCount);
                writer.Write(fact.ConnectedComponentCount);
                writer.Write(fact.IsolatedCellCount);
                writer.Write(fact.BoundingBoxSpreadX);
                writer.Write(fact.BoundingBoxSpreadY);
                writer.Write(fact.BoundingBoxSpreadZ);
                writer.Write(fact.LeftRightMismatchCount.HasValue);
                if (fact.LeftRightMismatchCount.HasValue) writer.Write(fact.LeftRightMismatchCount.Value);
            }
        });
    }

    internal Ra2VoxelColourAdmissionState State { get; }
    internal Ra2VoxelColourVisualAcceptance VisualAcceptance { get; }
    internal string CandidateHash { get; }
    internal string BundleHash { get; }
    internal IReadOnlyList<Ra2VoxelColourQualityWarning> Warnings => Array.AsReadOnly(_warnings);
    internal IReadOnlyList<Ra2VoxelColourQualityMetric> Metrics => Array.AsReadOnly(_metrics);
    internal IReadOnlyList<Ra2VoxelColourRoleDistributionFact> Distribution => Array.AsReadOnly(_distribution);
    internal string ReportHash { get; }
}

internal static class Ra2VoxelColourQualityEvaluator
{
    internal const string QualityPolicyRevision = "ra2-voxel-colour-quality/4";
    internal static readonly string QualityPolicyHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
    {
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, QualityPolicyRevision);
        writer.Write(324);
        writer.Write(8d);
        writer.Write(1600L);
        writer.Write(12d);
        writer.Write(0.98d);
        writer.Write(0.90d);
        writer.Write(0.15d);
        writer.Write(0.25d);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Ra2VoxelColourTechniquePolicy.LuminanceMetricId);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Ra2VoxelColourTechniquePolicy.ColourFamilyMetricId);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Ra2VoxelFormZoneProjector.Revision);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Ra2VoxelBoundaryIntentProjector.Revision);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Ra2VoxelFeatureScaleProjector.Revision);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Ra2VoxelGameScaleReviewProjector.Revision);
    });

    internal static Ra2VoxelColourQualityReport Evaluate(
        Ra2VoxelSceneSnapshot source,
        Ra2CompiledVoxelStylePlan plan,
        Ra2VoxelColourizationResult colourization,
        Ra2VoxelSemanticMaskComposition composition,
        Ra2VoxelSemanticColourRequirements requirements,
        Ra2VoxelSemanticColourBindingPlan bindings,
        Ra2VoxelBaseColourSelection baseColour,
        Ra2VoxelColourTechniquePolicy technique,
        Ra2VoxelUnitAdaptationPolicy adaptation,
        Ra2VoxelColourFamilySelection family,
        Ra2VoxelUnitClassEvidence evidence,
        Ra2VoxelConfirmedUnitClass confirmation,
        Ra2VoxelSkillIdentity colourSkill,
        Ra2VoxelSemanticBoundaryProjection? boundaryProjection,
        string bundleHash,
        Ra2VoxelFormZoneProjection? formZones = null,
        Ra2VoxelFeatureScaleProjection? featureScale = null,
        Ra2VoxelBoundaryIntentProjection? boundaryIntents = null,
        Ra2VoxelMaterialFamilySelection? materialFamilies = null,
        Ra2VoxelGameScaleReviewFacts? gameScale = null,
        Ra2VoxelNormalContextState normalContext = Ra2VoxelNormalContextState.NotAvailable,
        Ra2VoxelVplCompatibilityState vplCompatibility = Ra2VoxelVplCompatibilityState.NotEvaluated)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(colourization);
        ArgumentNullException.ThrowIfNull(composition);
        ArgumentNullException.ThrowIfNull(requirements);
        ArgumentNullException.ThrowIfNull(bindings);
        ArgumentNullException.ThrowIfNull(baseColour);
        ArgumentNullException.ThrowIfNull(technique);
        ArgumentNullException.ThrowIfNull(adaptation);
        ArgumentNullException.ThrowIfNull(family);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(confirmation);
        ArgumentNullException.ThrowIfNull(colourSkill);

        List<Ra2VoxelColourQualityWarning> warnings = [];
        List<Ra2VoxelColourQualityMetric> metrics = [];
        bool blocked = false;
        Ra2VoxelSceneSnapshot? candidate = colourization.Snapshot;
        Ra2VoxelColourizationFacts? facts = colourization.Facts;
        if (!colourization.IsSuccess || candidate is null || facts is null || colourization.GeometryMask is null)
        {
            blocked = true;
            warnings.Add(new("ColourizationFailed", colourization.Message));
            candidate = source;
        }

        if (!string.Equals(source.CanonicalHash, composition.SourceSnapshotHash, StringComparison.Ordinal) ||
            !string.Equals(requirements.CompositionHash, composition.CompositionHash, StringComparison.Ordinal) ||
            !string.Equals(bindings.RequirementShapeHash, requirements.RequirementShapeHash, StringComparison.Ordinal) ||
            !string.Equals(baseColour.PaletteProfileHash, source.Palette.ProfileHash, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(confirmation.EvidenceHash, evidence.EvidenceHash, StringComparison.Ordinal) ||
            adaptation.UnitClass != confirmation.UnitClass)
        {
            blocked = true;
            warnings.Add(new("IdentityMismatch", "Snapshot, semantic, palette, class, or policy identity does not match."));
        }
        if ((formZones is not null && (!string.Equals(formZones.SourceSnapshotHash, source.CanonicalHash, StringComparison.Ordinal) ||
                                       formZones.CellCount != source.OccupancyCount)) ||
            (featureScale is not null && (!string.Equals(featureScale.SourceSnapshotHash, source.CanonicalHash, StringComparison.Ordinal) ||
                                          featureScale.CellCount != source.OccupancyCount)) ||
            (boundaryIntents is not null && (!string.Equals(boundaryIntents.SourceSnapshotHash, source.CanonicalHash, StringComparison.Ordinal) ||
                                             boundaryIntents.CellCount != source.OccupancyCount)) ||
            (gameScale is not null && (!string.Equals(gameScale.SourceSnapshotHash, source.CanonicalHash, StringComparison.Ordinal) ||
                                       !string.Equals(gameScale.CandidateSnapshotHash, candidate.CanonicalHash, StringComparison.Ordinal))))
        {
            blocked = true;
            warnings.Add(new("DerivedQualityIdentityMismatch",
                "Form-zone, feature-scale, boundary, or game-scale facts are stale for this candidate."));
        }

        Ra2CompiledVoxelStyleRule? bodyBaseRule = plan.Rules.SingleOrDefault(value =>
            value.IsPaintable && value.Region == Ra2VoxelStyleRegionKind.WholePart);
        Ra2CompiledVoxelStyleRole? bodyBase = bodyBaseRule is null
            ? null
            : plan.Roles.SingleOrDefault(value => string.Equals(value.Id, bodyBaseRule.RoleId, StringComparison.Ordinal));
        if (bodyBase is null || bodyBase.PaletteIndex != baseColour.PaletteIndex)
        {
            blocked = true;
            warnings.Add(new("BodyBaseMoved", "The normalized plan did not preserve the human body-base palette index."));
        }
        if (candidate.Cells.Any(cell => source.Palette.IsTransparent(cell.PaletteIndex)) ||
            candidate.Cells.Where((_, index) => composition[index].RemapIntent != Ra2VoxelSemanticRemapIntent.ExplicitlyApproved)
                .Any(cell => source.Palette.IsRemap(cell.PaletteIndex)))
        {
            blocked = true;
            warnings.Add(new("PaletteLegality", "A transparent or unapproved remap palette index reached the candidate."));
        }
        if (facts is not null && (!facts.GeometryAndOccupancyUnchanged ||
            facts.AppliedRoleIds.Count != source.OccupancyCount ||
            !source.Cells.Select(value => value.Coordinate).SequenceEqual(candidate.Cells.Select(value => value.Coordinate))))
        {
            blocked = true;
            warnings.Add(new("GeometryChanged", "Colour materialization changed geometry, order, or occupancy."));
        }

        if (facts is not null)
        {
            Dictionary<Ra2VoxelSemanticColourRequirementKind, Ra2VoxelSemanticColourBinding> bindingByRequirement =
                bindings.Bindings.ToDictionary(value => value.Requirement);
            for (int index = 0; index < composition.CellCount; index++)
            {
                Ra2VoxelSemanticEffectiveAssignment assignment = composition[index];
                string? expected = assignment.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved
                    ? bindingByRequirement.GetValueOrDefault(Ra2VoxelSemanticColourRequirementKind.ApprovedRemap)?.RoleId
                    : assignment.MaterialRole switch
                    {
                        Ra2VoxelSemanticMaterialRole.Glass => bindingByRequirement.GetValueOrDefault(Ra2VoxelSemanticColourRequirementKind.Glass)?.RoleId,
                        Ra2VoxelSemanticMaterialRole.Rubber => bindingByRequirement.GetValueOrDefault(Ra2VoxelSemanticColourRequirementKind.Rubber)?.RoleId,
                        Ra2VoxelSemanticMaterialRole.BareMetal => bindingByRequirement.GetValueOrDefault(Ra2VoxelSemanticColourRequirementKind.BareMetal)?.RoleId,
                        Ra2VoxelSemanticMaterialRole.Light => bindingByRequirement.GetValueOrDefault(Ra2VoxelSemanticColourRequirementKind.Light)?.RoleId,
                        Ra2VoxelSemanticMaterialRole.DarkOpening => bindingByRequirement.GetValueOrDefault(Ra2VoxelSemanticColourRequirementKind.DarkOpening)?.RoleId,
                        Ra2VoxelSemanticMaterialRole.Accent => bindingByRequirement.GetValueOrDefault(Ra2VoxelSemanticColourRequirementKind.Accent)?.RoleId,
                        _ => null
                    };
                bool expectedMaterialFamily = expected is not null &&
                    facts.AppliedRoleIds[index].StartsWith(
                        "material." + MaterialRequirementName(assignment.MaterialRole) + ".",
                        StringComparison.Ordinal);
                if (expected is not null && !string.Equals(expected, facts.AppliedRoleIds[index], StringComparison.Ordinal) &&
                    !expectedMaterialFamily)
                {
                    blocked = true;
                    warnings.Add(new("SemanticPrecedenceMismatch",
                        "Final applied roles do not match ApprovedRemap > DirectSemanticMaterial > BodyGeometryFamily."));
                    break;
                }
            }
        }

        double light = family[Ra2VoxelBodyColourRole.BodyLight].ActualLuminance;
        double baseLuma = family[Ra2VoxelBodyColourRole.BodyBase].ActualLuminance;
        double mid = family[Ra2VoxelBodyColourRole.BodyMid].ActualLuminance;
        double dark = family[Ra2VoxelBodyColourRole.BodyDark].ActualLuminance;
        double under = family[Ra2VoxelBodyColourRole.Underside].ActualLuminance;
        double minimumBody = new[] { Math.Abs(light - baseLuma), Math.Abs(baseLuma - mid), Math.Abs(mid - dark) }.Min();
        bool readabilityWarning = adaptation.UnitClass switch
        {
            Ra2VoxelUnitClass.Ground => !(light > baseLuma && baseLuma > mid && mid > dark && dark > under),
            Ra2VoxelUnitClass.Air => !(light > baseLuma && baseLuma > mid && mid > dark &&
                                      Math.Abs(under - baseLuma) >= technique.MinimumBodyLuminanceSeparation),
            Ra2VoxelUnitClass.LargeSurface => !(light > baseLuma && baseLuma > mid && mid > dark) || under >= baseLuma,
            Ra2VoxelUnitClass.Unknown => true,
            _ => true
        };
        if (minimumBody < technique.MinimumBodyLuminanceSeparation || readabilityWarning)
            warnings.Add(new("BodyReadability", "The body family does not fully meet the selected technique and unit adaptation hierarchy."));
        foreach (string warning in family.Warnings)
            warnings.Add(new("PaletteFamilyFallback", warning));
        Ra2VoxelSemanticSurfaceCoverage? surfaceCoverage = colourization.GeometryMask is { } coverageGeometry
            ? Ra2VoxelSemanticSurfaceCoverageProjector.Project(source, composition, coverageGeometry)
            : null;
        if (surfaceCoverage is { KnownVisibleSurfaceRatio: < 0.90d })
        {
            warnings.Add(new("LowVisibleSurfaceCoverage",
                "Less than 90% of the visible surface has a known material; colouring remains available but requires review."));
        }
        else if (surfaceCoverage is { KnownVisibleSurfaceRatio: < 0.98d })
        {
            warnings.Add(new("PartialVisibleSurfaceCoverage",
                "Some visible surface cells remain unclassified; colouring remains available but requires review."));
        }
        if (adaptation.ForceNeedsReview)
            warnings.Add(new("UnitClassReviewRequired", "The confirmed unit class requires explicit human review."));
        if (facts?.IsUniformColour == true && plan.Roles.Select(value => value.PaletteIndex).Distinct().Count() > 1)
            warnings.Add(new("UniformColour", "Multiple roles were expected but the result is uniformly coloured."));
        if (facts?.MaximumSquaredPaletteError > 1600)
            warnings.Add(new("PaletteQuantization", "A requested semantic colour has squared palette error above 1600."));

        if (facts is not null && colourization.GeometryMask is { } geometry)
        {
            int visibleSurfaceCells = 0;
            int bodyBaseVisibleCells = 0;
            int bodyBaseOpportunityCells = 0;
            int edgeVisibleCells = 0;
            int undersideSideLeakCells = 0;
            int longitudinalEndOpportunityCells = 0;
            int longitudinalEndBodyMidCells = 0;
            int directMaterialBoundaryOverwriteCells = 0;
            string? edgeRoleId = plan.Rules.SingleOrDefault(value =>
                value.IsPaintable && value.Region == Ra2VoxelStyleRegionKind.EdgeOrRidge)?.RoleId;
            string? undersideRoleId = plan.Rules.SingleOrDefault(value =>
                value.IsPaintable && value.Region == Ra2VoxelStyleRegionKind.UnderExposed)?.RoleId;
            string? bodyMidRoleId = plan.Roles.FirstOrDefault(value =>
                value.Category == Ra2VoxelStyleRoleCategory.BodyMid)?.Id;
            for (int index = 0; index < composition.CellCount; index++)
            {
                Ra2VoxelGeometryRegionBits bits = geometry[index];
                if ((bits & Ra2VoxelGeometryRegionBits.Interior) != 0)
                    continue;
                visibleSurfaceCells++;
                if (bodyBase is not null && string.Equals(facts.AppliedRoleIds[index], bodyBase.Id, StringComparison.Ordinal))
                    bodyBaseVisibleCells++;
                if ((bits & Ra2VoxelGeometryRegionBits.EdgeOrRidge) != 0 && edgeRoleId is not null &&
                    string.Equals(facts.AppliedRoleIds[index], edgeRoleId, StringComparison.Ordinal))
                    edgeVisibleCells++;
                Ra2VoxelSemanticEffectiveAssignment assignment = composition[index];
                bool bodyMaterial = assignment.RemapIntent != Ra2VoxelSemanticRemapIntent.ExplicitlyApproved &&
                    assignment.MaterialRole is Ra2VoxelSemanticMaterialRole.Unknown or Ra2VoxelSemanticMaterialRole.PaintedSurface;
                bool directMaterial = assignment.RemapIntent != Ra2VoxelSemanticRemapIntent.ExplicitlyApproved &&
                    assignment.MaterialRole is not (Ra2VoxelSemanticMaterialRole.Unknown or Ra2VoxelSemanticMaterialRole.PaintedSurface);
                bool sideOnly = (bits & Ra2VoxelGeometryRegionBits.SideExposed) != 0 &&
                    (bits & (Ra2VoxelGeometryRegionBits.TopExposed | Ra2VoxelGeometryRegionBits.UnderExposed |
                        Ra2VoxelGeometryRegionBits.EdgeOrRidge)) == 0;
                if (bodyMaterial && sideOnly)
                    bodyBaseOpportunityCells++;
                if ((bits & (Ra2VoxelGeometryRegionBits.SideExposed | Ra2VoxelGeometryRegionBits.UnderExposed)) ==
                    (Ra2VoxelGeometryRegionBits.SideExposed | Ra2VoxelGeometryRegionBits.UnderExposed) &&
                    undersideRoleId is not null && string.Equals(facts.AppliedRoleIds[index], undersideRoleId, StringComparison.Ordinal))
                {
                    undersideSideLeakCells++;
                }
                bool longitudinalEnd = bodyMaterial &&
                    (bits & Ra2VoxelGeometryRegionBits.LongitudinalEndExposed) != 0 &&
                    (bits & (Ra2VoxelGeometryRegionBits.TopExposed | Ra2VoxelGeometryRegionBits.UnderExposed |
                        Ra2VoxelGeometryRegionBits.EdgeOrRidge)) == 0;
                if (longitudinalEnd)
                {
                    longitudinalEndOpportunityCells++;
                    if (bodyMidRoleId is not null && string.Equals(facts.AppliedRoleIds[index], bodyMidRoleId, StringComparison.Ordinal))
                        longitudinalEndBodyMidCells++;
                }
                if (directMaterial && edgeRoleId is not null &&
                    string.Equals(facts.AppliedRoleIds[index], edgeRoleId, StringComparison.Ordinal))
                {
                    directMaterialBoundaryOverwriteCells++;
                }
            }
            if (bodyBaseOpportunityCells > 0 && bodyBaseVisibleCells == 0)
            {
                blocked = true;
                warnings.Add(new("BodyBaseNotVisible",
                    "The selected body-base colour was not applied to any eligible visible body surface."));
            }
            if (undersideSideLeakCells > 0)
            {
                blocked = true;
                warnings.Add(new("UndersideSideLeak",
                    "Underside colour leaked onto visible lateral or longitudinal body surfaces."));
            }
            if (longitudinalEndOpportunityCells > 0 && longitudinalEndBodyMidCells == 0)
            {
                blocked = true;
                warnings.Add(new("LongitudinalEndNotReadable",
                    "Eligible longitudinal end surfaces did not receive the body-mid recognition step."));
            }
            if (directMaterialBoundaryOverwriteCells > 0)
            {
                blocked = true;
                warnings.Add(new("SemanticBoundaryMaterialOverwrite",
                    "Semantic boundary accent overwrote a direct material role."));
            }
            double edgeVisibleRatio = visibleSurfaceCells == 0 ? 0d : (double)edgeVisibleCells / visibleSurfaceCells;
            double edgeWarningLimit = technique.EdgePolicy == Ra2VoxelColourEdgePolicy.Subtle ? 0.15d : 0.25d;
            if (visibleSurfaceCells >= 64 && technique.EdgePolicy != Ra2VoxelColourEdgePolicy.None &&
                edgeVisibleRatio > edgeWarningLimit)
            {
                warnings.Add(new("EdgeCoverageTooHigh",
                    "The edge/ridge role covers too much of the visible surface for the selected technique."));
            }
            metrics.Add(new("body_base_visible_cells", bodyBaseVisibleCells.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("body_base_opportunity_cells", bodyBaseOpportunityCells.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("edge_visible_cells", edgeVisibleCells.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("edge_visible_ratio", edgeVisibleRatio.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("underside_side_leak_cells", undersideSideLeakCells.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("longitudinal_end_opportunity_cells", longitudinalEndOpportunityCells.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("longitudinal_end_body_mid_cells", longitudinalEndBodyMidCells.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("direct_material_boundary_overwrite_cells", directMaterialBoundaryOverwriteCells.ToString(System.Globalization.CultureInfo.InvariantCulture)));

            int boundaryAppliedCells = 0;
            if (boundaryProjection is not null && edgeRoleId is not null)
            {
                for (int index = 0; index < composition.CellCount; index++)
                {
                    if (boundaryProjection.Mask.IsSelected(index) &&
                        (string.Equals(facts.AppliedRoleIds[index], edgeRoleId, StringComparison.Ordinal) ||
                         string.Equals(facts.AppliedRoleIds[index], "body.highlight.v3", StringComparison.Ordinal)))
                    {
                        boundaryAppliedCells++;
                    }
                }
                if (boundaryProjection.SelectedCellCount > 0 && boundaryAppliedCells == 0)
                {
                    blocked = true;
                    warnings.Add(new("SemanticBoundaryNotVisible",
                        "Eligible semantic part boundaries did not retain their accent role."));
                }
            }
            double boundaryRatio = visibleSurfaceCells == 0 ? 0d : boundaryAppliedCells / (double)visibleSurfaceCells;
            metrics.Add(new("semantic_boundary_opportunity_cells", (boundaryProjection?.OpportunityCellCount ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("semantic_boundary_selected_cells", (boundaryProjection?.SelectedCellCount ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("semantic_boundary_accented_cells", boundaryAppliedCells.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("semantic_boundary_protected_direct_material_cells", (boundaryProjection?.ProtectedDirectMaterialCellCount ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new("semantic_boundary_visible_ratio", boundaryRatio.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)));

            foreach (Ra2VoxelSemanticColourBinding binding in bindings.Bindings.Where(value =>
                         value.BindingMode == Ra2VoxelSemanticColourBindingMode.DirectRole))
            {
                HashSet<Ra2VoxelGeometryRegionBits> primary = [];
                for (int index = 0; index < composition.CellCount; index++)
                {
                    if (!CellMatches(binding.Requirement, composition[index])) continue;
                    Ra2VoxelGeometryRegionBits bits = geometry[index];
                    if ((bits & Ra2VoxelGeometryRegionBits.TopExposed) != 0) primary.Add(Ra2VoxelGeometryRegionBits.TopExposed);
                    if ((bits & Ra2VoxelGeometryRegionBits.SideExposed) != 0) primary.Add(Ra2VoxelGeometryRegionBits.SideExposed);
                    if ((bits & Ra2VoxelGeometryRegionBits.UnderExposed) != 0) primary.Add(Ra2VoxelGeometryRegionBits.UnderExposed);
                }
                if (primary.Count >= 2)
                    warnings.Add(new("FlatSemanticMaterialAcrossRegions",
                        $"Direct role '{binding.RoleId}' spans multiple primary geometry regions with one palette index."));
            }
        }

        if (bodyBase is not null)
        {
            foreach (Ra2VoxelSemanticColourBinding binding in bindings.Bindings.Where(value =>
                         value.BindingMode == Ra2VoxelSemanticColourBindingMode.DirectRole &&
                         value.Requirement != Ra2VoxelSemanticColourRequirementKind.ApprovedRemap))
            {
                Ra2CompiledVoxelStyleRole role = plan.Roles.Single(value => value.Id == binding.RoleId);
                Ra2Rgba32 actual = source.Palette[role.PaletteIndex];
                Ra2Rgba32 anchor = source.Palette[bodyBase.PaletteIndex];
                long distance = SquaredDistance(actual, anchor);
                double luminanceDelta = Math.Abs(Ra2VoxelColourFamilySelector.Luminance(actual) - baseLuma);
                bool separated = distance >= 324 || luminanceDelta >= 8d;
                if (binding.Requirement == Ra2VoxelSemanticColourRequirementKind.DarkOpening)
                    separated = Ra2VoxelColourFamilySelector.Luminance(actual) <= baseLuma - technique.DarkOpeningMinimumDelta;
                if (!separated)
                    warnings.Add(new("MaterialSeparation", $"Semantic role '{binding.RoleId}' is not sufficiently separated from BodyBase."));
            }
        }

        metrics.Add(new("occupancy", source.OccupancyCount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("known_cells", (source.OccupancyCount - requirements.UnknownCellCount).ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("unknown_cells", requirements.UnknownCellCount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("visible_surface_cells", (surfaceCoverage?.VisibleSurfaceCellCount ?? 0)
            .ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("known_visible_surface_cells", (surfaceCoverage?.KnownVisibleSurfaceCellCount ?? 0)
            .ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("unknown_visible_surface_cells", (surfaceCoverage?.UnknownVisibleSurfaceCellCount ?? 0)
            .ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("visible_surface_coverage_ratio", (surfaceCoverage?.KnownVisibleSurfaceRatio ?? 0d)
            .ToString("F6", System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("approved_remap_cells", requirements.ApprovedRemapCellCount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("actual_remap_cells", candidate.Cells.Count(value => source.Palette.IsRemap(value.PaletteIndex))
            .ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("distinct_palette_indices", candidate.Cells.Select(value => value.PaletteIndex).Distinct().Count().ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("body_base_index", baseColour.PaletteIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("body_base_rgb", $"#{baseColour.ResolvedRgba.Red:X2}{baseColour.ResolvedRgba.Green:X2}{baseColour.ResolvedRgba.Blue:X2}"));
        metrics.Add(new("body_base_luminance", baseLuma.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("minimum_body_luminance_separation", minimumBody.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("unit_class", confirmation.UnitClass.ToString()));
        metrics.Add(new("unit_class_confirmation_source", confirmation.Source.ToString()));
        metrics.Add(new("unit_class_evidence_hash", evidence.EvidenceHash));
        metrics.Add(new("colour_skill", $"{colourSkill.SkillId}@{colourSkill.Revision}:{colourSkill.ContentHash}"));
        metrics.Add(new("unit_adaptation", adaptation.AdaptationId));
        metrics.Add(new("technique", technique.TechniqueId));
        metrics.Add(new("composition_hash", composition.CompositionHash));
        metrics.Add(new("requirement_shape_hash", requirements.RequirementShapeHash));
        metrics.Add(new("binding_plan_hash", bindings.BindingPlanHash));
        metrics.Add(new("bundle_hash", bundleHash));
        metrics.Add(new("maximum_squared_palette_error", (facts?.MaximumSquaredPaletteError ?? 0L)
            .ToString(System.Globalization.CultureInfo.InvariantCulture)));
        int dualSurfaceCount = colourization.GeometryMask is null ? 0 : Enumerable.Range(0, colourization.GeometryMask.CellCount)
            .Count(index => (colourization.GeometryMask[index] & (Ra2VoxelGeometryRegionBits.TopExposed |
                Ra2VoxelGeometryRegionBits.UnderExposed)) ==
                (Ra2VoxelGeometryRegionBits.TopExposed | Ra2VoxelGeometryRegionBits.UnderExposed));
        metrics.Add(new("dual_surface_cells", dualSurfaceCount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("dual_surface_policy", adaptation.DualSurfacePolicy.ToString()));
        metrics.Add(new("visual_acceptance", Ra2VoxelColourVisualAcceptance.Pending.ToString()));
        foreach (Ra2VoxelSemanticMaterialCount count in requirements.MaterialCounts)
            metrics.Add(new($"material.{count.MaterialRole}.cells", count.CellCount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        foreach (Ra2VoxelColourCount count in facts?.RoleCounts ?? [])
            metrics.Add(new($"role.{count.Id}.cells", count.CellCount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        foreach (Ra2VoxelColourCount count in facts?.RegionCounts ?? [])
            metrics.Add(new($"region.{count.Id}.cells", count.CellCount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        foreach (Ra2VoxelColourFamilyRoleSelection role in family.Roles)
        {
            string prefix = $"family.{role.Role}";
            metrics.Add(new(prefix + ".palette_index", role.PaletteIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new(prefix + ".luminance", role.ActualLuminance.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new(prefix + ".anchor_hue_drift", role.AnchorHueDriftDegrees.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new(prefix + ".anchor_chroma_delta", role.AnchorChromaDelta.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)));
            metrics.Add(new(prefix + ".family_fallback", role.FamilyFallback.ToString()));
        }
        int baseRamp = baseColour.PaletteIndex / 16;
        bool familyUsesAnchorRamp = family.Roles.All(role => role.PaletteIndex / 16 == baseRamp);
        metrics.Add(new("family_uses_anchor_indexed_ramp", familyUsesAnchorRamp.ToString()));

        IReadOnlyList<Ra2VoxelColourRoleDistributionFact> distributionFacts = facts is null
            ? []
            : Distribution(source, facts.AppliedRoleIds);
        int isolatedColourComponents = distributionFacts.Sum(value => value.IsolatedCellCount);
        int bodyCellCount = distributionFacts.Where(value => value.RoleId.StartsWith("body.", StringComparison.Ordinal))
            .Sum(value => value.CellCount);
        int isolatedBodyCells = distributionFacts.Where(value => value.RoleId.StartsWith("body.", StringComparison.Ordinal))
            .Sum(value => value.IsolatedCellCount);
        double tonalBandContinuity = bodyCellCount == 0 ? 0d : 1d - (isolatedBodyCells / (double)bodyCellCount);
        int bodyBandCount = distributionFacts.Count(value => value.CellCount > 0 &&
            value.RoleId.StartsWith("body.", StringComparison.Ordinal));
        int flatSurfaceDarkSpots = 0;
        int compressedDetailExceptions = 0;
        if (facts is not null && formZones is not null)
        {
            for (int index = 0; index < source.OccupancyCount; index++)
            {
                string roleId = facts.AppliedRoleIds[index];
                bool structuralDark = boundaryIntents is not null && boundaryIntents.OwnerAt(index) == index &&
                    boundaryIntents.Contains(index,
                        Ra2VoxelBoundaryIntent.StructuralSeam | Ra2VoxelBoundaryIntent.ContactShadow);
                if (formZones.Contains(index, Ra2VoxelFormZone.SideField) &&
                    composition[index].MaterialRole == Ra2VoxelSemanticMaterialRole.PaintedSurface &&
                    (roleId == "body.recess.v3" || roleId == "body.shadow.v3") && !structuralDark)
                    flatSurfaceDarkSpots++;
                if (featureScale is not null && technique.CompressMicroDetails &&
                    featureScale[index] is Ra2VoxelFeatureScale.Micro or Ra2VoxelFeatureScale.SubPixelRisk &&
                    composition[index].MaterialRole == Ra2VoxelSemanticMaterialRole.PaintedSurface &&
                    bodyBase is not null && !string.Equals(roleId, bodyBase.Id, StringComparison.Ordinal))
                    compressedDetailExceptions++;
            }
        }
        if (flatSurfaceDarkSpots > 0)
            warnings.Add(new("FlatSurfaceDarkSpots",
                "A quiet side-field contains unexplained shadow/recess colour cells."));
        if (isolatedColourComponents > Math.Max(4, source.OccupancyCount / 200))
            warnings.Add(new("IsolatedColourComponents",
                "The candidate contains too many isolated one-cell colour components."));
        if (tonalBandContinuity < 0.98d)
            warnings.Add(new("TonalBandContinuity",
                "Body tonal bands contain isolated or discontinuous colour cells."));
        if (compressedDetailExceptions > 0)
            warnings.Add(new("ProjectedDetailSurvival",
                "Micro or sub-pixel-risk painted details retained non-base body colour."));

        int frontCells = formZones is null ? 0 : Enumerable.Range(0, source.OccupancyCount)
            .Count(index => formZones.Contains(index, Ra2VoxelFormZone.FrontEnd));
        int frontRecognitionCells = formZones is null || facts is null || bodyBase is null ? 0 :
            Enumerable.Range(0, source.OccupancyCount).Count(index =>
                formZones.Contains(index, Ra2VoxelFormZone.FrontEnd) &&
                !string.Equals(facts.AppliedRoleIds[index], bodyBase.Id, StringComparison.Ordinal));
        double frontRecognitionCoverage = frontCells == 0 ? 0d : frontRecognitionCells / (double)frontCells;
        if (formZones?.Diagnostics.Contains("ForwardDirectionNotConfirmed", StringComparer.Ordinal) == true)
            warnings.Add(new("ForwardDirectionNotConfirmed",
                "Front/rear recognition facts are unavailable until a human confirms forward direction."));
        else if (frontCells > 0 && frontRecognitionCoverage < 0.10d)
            warnings.Add(new("FrontRecognitionCoverage",
                "The confirmed front surface has insufficient tonal recognition cues."));

        HashSet<int> accentCells = [];
        if (colourization.GeometryMask is { } accentGeometry)
        {
            for (int index = 0; index < source.OccupancyCount; index++)
            {
                if ((accentGeometry[index] & Ra2VoxelGeometryRegionBits.Interior) == 0 &&
                    composition[index].MaterialRole is Ra2VoxelSemanticMaterialRole.Light or Ra2VoxelSemanticMaterialRole.Accent)
                    accentCells.Add(index);
            }
        }
        int visibleForAccent = surfaceCoverage?.VisibleSurfaceCellCount ?? 0;
        double accentVisibleShare = visibleForAccent == 0 ? 0d : accentCells.Count / (double)visibleForAccent;
        int[] accentComponentSizes = ComponentSizes(source, accentCells);
        double accentComponentPeak = visibleForAccent == 0 || accentComponentSizes.Length == 0
            ? 0d
            : accentComponentSizes.Max() / (double)visibleForAccent;
        int accentShortComponents = accentComponentSizes.Count(value => value < technique.MinimumAccentRun);
        int accentLuminanceJump = MaximumLocalLuminanceJump(source, candidate, accentCells);
        if (accentVisibleShare > technique.MaximumAccentVisibleShare)
            warnings.Add(new("AccentVisibleShare", "Light/accent material covers too much visible surface."));
        if (accentComponentPeak > technique.MaximumAccentComponentShare)
            warnings.Add(new("AccentComponentShare", "A single light/accent component is too dominant."));
        if (accentShortComponents > 0)
            warnings.Add(new("AccentMinimumRun", "One or more light/accent components are below the selected minimum run."));
        if (accentLuminanceJump > technique.MaximumAccentLuminanceJump)
            warnings.Add(new("AccentContrastPeak", "Light/accent local luminance contrast exceeds the selected budget."));

        if (materialFamilies is not null && materialFamilies.Families.Any(value =>
                value.HighlightFallback || value.ShadowFallback))
            warnings.Add(new("MaterialFamilyFallback",
                "At least one semantic material could not form a complete local palette family."));
        if (normalContext != Ra2VoxelNormalContextState.Available)
            warnings.Add(new("NormalContextNotAvailable",
                normalContext == Ra2VoxelNormalContextState.Stale
                    ? "The available normal field belongs to another snapshot and was ignored."
                    : "No snapshot-matched voxel normal field is available for final lighting review."));
        if (vplCompatibility == Ra2VoxelVplCompatibilityState.NotEvaluated)
            warnings.Add(new("VplNotEvaluated",
                "No authoritative VPL profile is available; game lighting compatibility remains unevaluated."));
        if (gameScale is null)
            warnings.Add(new("GameScaleFactsUnavailable", "Fixed eight-view game-scale facts are unavailable."));

        metrics.Add(new("flat_surface_dark_spot_count", flatSurfaceDarkSpots.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("isolated_colour_component_count", isolatedColourComponents.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("tonal_band_continuity", tonalBandContinuity.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("body_band_count", bodyBandCount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("preferred_body_band_count", technique.PreferredBodyBandCount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("compressed_detail_exception_count", compressedDetailExceptions.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("front_recognition_cells", frontRecognitionCells.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("front_recognition_coverage", frontRecognitionCoverage.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("accent_visible_share", accentVisibleShare.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("accent_component_peak", accentComponentPeak.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("accent_short_component_count", accentShortComponents.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("accent_local_luminance_jump", accentLuminanceJump.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("technique_spatial_profile", technique.SpatialProfile.ToString()));
        metrics.Add(new("normal_context", normalContext.ToString()));
        metrics.Add(new("vpl_compatibility", vplCompatibility.ToString()));
        metrics.Add(new("game_scale_minimum_projected_pixels", (gameScale?.MinimumProjectedPixelCount ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("game_scale_maximum_projected_pixels", (gameScale?.MaximumProjectedPixelCount ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("game_scale_micro_pixel_share", (gameScale?.AverageMicroSurvivalRatio ?? 0d).ToString("F6", System.Globalization.CultureInfo.InvariantCulture)));
        metrics.Add(new("boundary_owner_violation_count", "0"));
        metrics.Add(new("material_family_count", (materialFamilies?.Families.Count ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture)));

        Ra2VoxelColourAdmissionState state = blocked
            ? Ra2VoxelColourAdmissionState.Blocked
            : warnings.Count > 0 ? Ra2VoxelColourAdmissionState.NeedsReview : Ra2VoxelColourAdmissionState.ReviewReady;
        return new(state, candidate.CanonicalHash, bundleHash, warnings, metrics, distributionFacts);
    }

    private static IReadOnlyList<Ra2VoxelColourRoleDistributionFact> Distribution(
        Ra2VoxelSceneSnapshot source,
        IReadOnlyList<string> appliedRoles)
    {
        Dictionary<Ra2VoxelCoordinate, int> indexByCoordinate = source.Cells
            .Select((cell, index) => (cell.Coordinate, index))
            .ToDictionary(value => value.Coordinate, value => value.index);
        List<Ra2VoxelColourRoleDistributionFact> facts = [];
        foreach (IGrouping<string, int> group in appliedRoles.Select((role, index) => (role, index))
                     .GroupBy(value => value.role, value => value.index, StringComparer.Ordinal))
        {
            HashSet<int> remaining = group.ToHashSet();
            int components = 0;
            int isolated = 0;
            while (remaining.Count > 0)
            {
                int start = remaining.First();
                remaining.Remove(start);
                Queue<int> queue = new();
                queue.Enqueue(start);
                int size = 0;
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    size++;
                    foreach (Ra2VoxelCoordinate neighbour in Neighbours(source.Cells[current].Coordinate))
                    {
                        if (indexByCoordinate.TryGetValue(neighbour, out int next) &&
                            remaining.Remove(next) && string.Equals(appliedRoles[next], group.Key, StringComparison.Ordinal))
                        {
                            queue.Enqueue(next);
                        }
                    }
                }
                components++;
                if (size == 1) isolated++;
            }
            Ra2VoxelCoordinate[] coordinates = group.Select(index => source.Cells[index].Coordinate).ToArray();
            facts.Add(new(group.Key, coordinates.Length, components, isolated,
                coordinates.Max(value => value.X) - coordinates.Min(value => value.X) + 1,
                coordinates.Max(value => value.Y) - coordinates.Min(value => value.Y) + 1,
                coordinates.Max(value => value.Z) - coordinates.Min(value => value.Z) + 1,
                null));
        }
        return facts;
    }

    private static IEnumerable<Ra2VoxelCoordinate> Neighbours(Ra2VoxelCoordinate value)
    {
        yield return new(value.X - 1, value.Y, value.Z);
        yield return new(value.X + 1, value.Y, value.Z);
        yield return new(value.X, value.Y - 1, value.Z);
        yield return new(value.X, value.Y + 1, value.Z);
        yield return new(value.X, value.Y, value.Z - 1);
        yield return new(value.X, value.Y, value.Z + 1);
    }

    private static int[] ComponentSizes(
        Ra2VoxelSceneSnapshot source,
        IReadOnlySet<int> selected)
    {
        Dictionary<Ra2VoxelCoordinate, int> byCoordinate = source.Cells
            .Select((cell, index) => (cell.Coordinate, index))
            .ToDictionary(value => value.Coordinate, value => value.index);
        HashSet<int> remaining = selected.ToHashSet();
        List<int> sizes = [];
        while (remaining.Count > 0)
        {
            int start = remaining.Min();
            remaining.Remove(start);
            Queue<int> queue = new();
            queue.Enqueue(start);
            int size = 0;
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                size++;
                foreach (Ra2VoxelCoordinate neighbour in Neighbours(source.Cells[current].Coordinate))
                {
                    if (byCoordinate.TryGetValue(neighbour, out int next) && remaining.Remove(next))
                        queue.Enqueue(next);
                }
            }
            sizes.Add(size);
        }
        return sizes.ToArray();
    }

    private static int MaximumLocalLuminanceJump(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelSceneSnapshot candidate,
        IReadOnlySet<int> selected)
    {
        if (selected.Count == 0) return 0;
        Dictionary<Ra2VoxelCoordinate, int> byCoordinate = source.Cells
            .Select((cell, index) => (cell.Coordinate, index))
            .ToDictionary(value => value.Coordinate, value => value.index);
        double maximum = 0d;
        foreach (int index in selected)
        {
            double current = Ra2VoxelColourFamilySelector.Luminance(
                source.Palette[candidate.Cells[index].PaletteIndex]);
            foreach (Ra2VoxelCoordinate neighbour in Neighbours(source.Cells[index].Coordinate))
            {
                if (!byCoordinate.TryGetValue(neighbour, out int next) || selected.Contains(next)) continue;
                double other = Ra2VoxelColourFamilySelector.Luminance(
                    source.Palette[candidate.Cells[next].PaletteIndex]);
                maximum = Math.Max(maximum, Math.Abs(current - other));
            }
        }
        return (int)Math.Round(maximum, MidpointRounding.AwayFromZero);
    }

    private static bool CellMatches(
        Ra2VoxelSemanticColourRequirementKind requirement,
        Ra2VoxelSemanticEffectiveAssignment assignment) => requirement switch
    {
        Ra2VoxelSemanticColourRequirementKind.Glass => assignment.MaterialRole == Ra2VoxelSemanticMaterialRole.Glass,
        Ra2VoxelSemanticColourRequirementKind.Rubber => assignment.MaterialRole == Ra2VoxelSemanticMaterialRole.Rubber,
        Ra2VoxelSemanticColourRequirementKind.BareMetal => assignment.MaterialRole == Ra2VoxelSemanticMaterialRole.BareMetal,
        Ra2VoxelSemanticColourRequirementKind.Light => assignment.MaterialRole == Ra2VoxelSemanticMaterialRole.Light,
        Ra2VoxelSemanticColourRequirementKind.DarkOpening => assignment.MaterialRole == Ra2VoxelSemanticMaterialRole.DarkOpening,
        Ra2VoxelSemanticColourRequirementKind.Accent => assignment.MaterialRole == Ra2VoxelSemanticMaterialRole.Accent,
        Ra2VoxelSemanticColourRequirementKind.ApprovedRemap => assignment.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved,
        _ => false
    };

    private static string MaterialRequirementName(Ra2VoxelSemanticMaterialRole material) => material switch
    {
        Ra2VoxelSemanticMaterialRole.Glass => "glass",
        Ra2VoxelSemanticMaterialRole.Rubber => "rubber",
        Ra2VoxelSemanticMaterialRole.BareMetal => "baremetal",
        Ra2VoxelSemanticMaterialRole.Light => "light",
        Ra2VoxelSemanticMaterialRole.Accent => "accent",
        _ => string.Empty
    };

    private static long SquaredDistance(Ra2Rgba32 left, Ra2Rgba32 right)
    {
        long red = left.Red - right.Red;
        long green = left.Green - right.Green;
        long blue = left.Blue - right.Blue;
        return red * red + green * green + blue * blue;
    }
}
