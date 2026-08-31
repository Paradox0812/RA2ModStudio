using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelUnitClassContractTests
{
    [Fact]
    public void Evidence_IsDeterministicBoundedAndIndependentOfPaletteColours()
    {
        Ra2VoxelSceneSnapshot firstSnapshot = CreateSnapshot(60, colourShift: 0);
        Ra2VoxelSceneSnapshot secondSnapshot = CreateSnapshot(120, colourShift: 17);
        Ra2VoxelUnitClassEvidence first = Ra2VoxelUnitClassEvidenceBuilder.Build(
            firstSnapshot,
            Composition(firstSnapshot, Ra2VoxelSemanticPartRole.Wheel, Ra2VoxelSemanticMaterialRole.Rubber));
        Ra2VoxelUnitClassEvidence second = Ra2VoxelUnitClassEvidenceBuilder.Build(
            secondSnapshot,
            Composition(secondSnapshot, Ra2VoxelSemanticPartRole.Wheel, Ra2VoxelSemanticMaterialRole.Rubber));

        Assert.Equal(first.ModelIdentity, second.ModelIdentity);
        Assert.Equal(first.GeometryFactsHash, second.GeometryFactsHash);
        Assert.Equal(first.SemanticFactsHash, second.SemanticFactsHash);
        Assert.Equal(first.EvidenceHash, second.EvidenceHash);
        Assert.DoesNotContain("palette_index", first.ToPromptText(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("coordinate:", first.ToPromptText(), StringComparison.OrdinalIgnoreCase);
        Assert.All(first.Facts, fact => Assert.InRange(fact.BoundedValue.Length, 1, 256));
    }

    [Fact]
    public void Evidence_SemanticPresenceChangesHashButCountsDoNot()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(60, 0);
        Ra2VoxelSemanticEffectiveAssignment painted = Assignment(
            Ra2VoxelSemanticPartRole.BodyShell,
            Ra2VoxelSemanticMaterialRole.PaintedSurface);
        Ra2VoxelSemanticEffectiveAssignment glass = Assignment(
            Ra2VoxelSemanticPartRole.BodyShell,
            Ra2VoxelSemanticMaterialRole.Glass);
        Ra2VoxelUnitClassEvidence oneGlass = Ra2VoxelUnitClassEvidenceBuilder.Build(
            snapshot,
            new(snapshot.CanonicalHash, Enumerable.Repeat(painted, snapshot.OccupancyCount - 1).Append(glass), new string('A', 64)));
        Ra2VoxelUnitClassEvidence twoGlass = Ra2VoxelUnitClassEvidenceBuilder.Build(
            snapshot,
            new(snapshot.CanonicalHash, Enumerable.Repeat(painted, snapshot.OccupancyCount - 2).Concat([glass, glass]), new string('B', 64)));
        Ra2VoxelUnitClassEvidence noGlass = Ra2VoxelUnitClassEvidenceBuilder.Build(
            snapshot,
            new(snapshot.CanonicalHash, Enumerable.Repeat(painted, snapshot.OccupancyCount), new string('C', 64)));

        Assert.Equal(oneGlass.SemanticFactsHash, twoGlass.SemanticFactsHash);
        Assert.Equal(oneGlass.EvidenceHash, twoGlass.EvidenceHash);
        Assert.NotEqual(oneGlass.SemanticFactsHash, noGlass.SemanticFactsHash);
    }

    [Fact]
    public void Proposal_RequiresCurrentEvidenceCitationsAndClassifierIdentity()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(60, 0);
        Ra2VoxelUnitClassEvidence evidence = Ra2VoxelUnitClassEvidenceBuilder.Build(
            snapshot,
            Composition(snapshot, Ra2VoxelSemanticPartRole.Wheel, Ra2VoxelSemanticMaterialRole.Rubber));
        string geometryFact = evidence.Facts.First(fact => fact.FactKind == Ra2VoxelUnitClassFactKind.Geometry).FactId;
        string semanticFact = evidence.Facts.First(fact => fact.FactKind == Ra2VoxelUnitClassFactKind.Semantic).FactId;
        Ra2VoxelUnitClassProposalInput input = new(
            Ra2VoxelUnitClass.Ground,
            Ra2VoxelUnitClassConfidenceBand.High,
            [semanticFact, geometryFact],
            "轮组语义与紧凑车体几何共同支持地面载具。",
            Ra2VoxelUnitClassProposal.RequiredClassifierSkillId,
            "1",
            new string('D', 64),
            evidence.EvidenceHash);

        Ra2VoxelUnitClassProposalResult first = Ra2VoxelUnitClassProposal.Validate(evidence, input);
        Ra2VoxelUnitClassProposalResult reordered = Ra2VoxelUnitClassProposal.Validate(
            evidence,
            input with { EvidenceFactIds = [geometryFact, semanticFact] });
        Ra2VoxelUnitClassProposalResult foreign = Ra2VoxelUnitClassProposal.Validate(
            evidence,
            input with { EvidenceFactIds = ["geometry.not-present"] });
        Ra2VoxelUnitClassProposalResult wrongSkill = Ra2VoxelUnitClassProposal.Validate(
            evidence,
            input with { ClassifierSkillId = "ra2-air-voxel-colour-techniques" });
        Ra2VoxelUnitClassProposalResult stale = Ra2VoxelUnitClassProposal.Validate(
            evidence,
            input with { EvidenceHash = new string('E', 64) });

        Assert.True(first.IsSuccess, first.Message);
        Assert.True(reordered.IsSuccess, reordered.Message);
        Assert.Equal(first.Proposal!.ProposalHash, reordered.Proposal!.ProposalHash);
        Assert.Equal(Ra2VoxelUnitClassProposalFailureKind.InvalidEvidenceReference, foreign.FailureKind);
        Assert.Equal(Ra2VoxelUnitClassProposalFailureKind.ClassifierSkillMismatch, wrongSkill.FailureKind);
        Assert.Equal(Ra2VoxelUnitClassProposalFailureKind.EvidenceMismatch, stale.FailureKind);
    }

    [Fact]
    public void Confirmation_SeparatesConfirmedOverrideAndManualFallback()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(60, 0);
        Ra2VoxelUnitClassEvidence evidence = Ra2VoxelUnitClassEvidenceBuilder.Build(
            snapshot,
            Composition(snapshot, Ra2VoxelSemanticPartRole.Wheel, Ra2VoxelSemanticMaterialRole.Rubber));
        Ra2VoxelUnitClassProposal proposal = ValidProposal(evidence);

        Ra2VoxelUnitClassConfirmationResult confirmed = Ra2VoxelConfirmedUnitClass.Create(
            evidence,
            Ra2VoxelUnitClass.Ground,
            Ra2VoxelUnitClassConfirmationSource.HumanConfirmedProposal,
            proposal);
        Ra2VoxelUnitClassConfirmationResult corrected = Ra2VoxelConfirmedUnitClass.Create(
            evidence,
            Ra2VoxelUnitClass.Air,
            Ra2VoxelUnitClassConfirmationSource.HumanOverride,
            proposal);
        Ra2VoxelUnitClassConfirmationResult manual = Ra2VoxelConfirmedUnitClass.Create(
            evidence,
            Ra2VoxelUnitClass.Unknown,
            Ra2VoxelUnitClassConfirmationSource.ManualWithoutAiAssessment,
            null);
        Ra2VoxelUnitClassConfirmationResult manualSelection = Ra2VoxelConfirmedUnitClass.Create(
            evidence,
            Ra2VoxelUnitClass.Air,
            Ra2VoxelUnitClassConfirmationSource.HumanManualSelection,
            null);
        Ra2VoxelUnitClassConfirmationResult manualSelectionWithProposal = Ra2VoxelConfirmedUnitClass.Create(
            evidence,
            Ra2VoxelUnitClass.Air,
            Ra2VoxelUnitClassConfirmationSource.HumanManualSelection,
            proposal);
        Ra2VoxelUnitClassConfirmationResult falseConfirmation = Ra2VoxelConfirmedUnitClass.Create(
            evidence,
            Ra2VoxelUnitClass.Air,
            Ra2VoxelUnitClassConfirmationSource.HumanConfirmedProposal,
            proposal);
        Ra2VoxelUnitClassConfirmationResult falseOverride = Ra2VoxelConfirmedUnitClass.Create(
            evidence,
            Ra2VoxelUnitClass.Ground,
            Ra2VoxelUnitClassConfirmationSource.HumanOverride,
            proposal);

        Assert.True(confirmed.IsSuccess, confirmed.Message);
        Assert.True(corrected.IsSuccess, corrected.Message);
        Assert.True(manual.IsSuccess, manual.Message);
        Assert.True(manualSelection.IsSuccess, manualSelection.Message);
        Assert.Null(manualSelection.Confirmation!.ProposalHash);
        Assert.Equal(Ra2VoxelUnitClassConfirmationFailureKind.ProposalNotAllowed, manualSelectionWithProposal.FailureKind);
        Assert.NotEqual(confirmed.Confirmation!.ConfirmationHash, corrected.Confirmation!.ConfirmationHash);
        Assert.Null(manual.Confirmation!.ProposalHash);
        Assert.Equal(Ra2VoxelUnitClassConfirmationFailureKind.ClassDoesNotMatchSource, falseConfirmation.FailureKind);
        Assert.Equal(Ra2VoxelUnitClassConfirmationFailureKind.ClassDoesNotMatchSource, falseOverride.FailureKind);
    }

    private static Ra2VoxelUnitClassProposal ValidProposal(Ra2VoxelUnitClassEvidence evidence)
    {
        Ra2VoxelUnitClassProposalResult result = Ra2VoxelUnitClassProposal.Validate(
            evidence,
            new(
                Ra2VoxelUnitClass.Ground,
                Ra2VoxelUnitClassConfidenceBand.High,
                [evidence.Facts[0].FactId],
                "地面载具证据成立。",
                Ra2VoxelUnitClassProposal.RequiredClassifierSkillId,
                "1",
                new string('D', 64),
                evidence.EvidenceHash));
        Assert.True(result.IsSuccess, result.Message);
        return result.Proposal!;
    }

    private static Ra2VoxelSceneSnapshot CreateSnapshot(byte paletteIndex, int colourShift)
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(value => new Ra2Rgba32(
                (byte)((value + colourShift) % 256),
                (byte)((value + colourShift) % 256),
                (byte)((value + colourShift) % 256)))
            .ToArray();
        colours[0] = new(0, 0, 0, 0);
        Ra2VoxelPaletteProfile palette = new($"unit-class-{colourShift}", colours, [0], Enumerable.Range(16, 16).Select(value => (byte)value));
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "unit", 4, 8, 3);
        IEnumerable<Ra2VoxelCell> cells = from x in Enumerable.Range(0, 4)
                                         from y in Enumerable.Range(0, 8)
                                         from z in Enumerable.Range(0, 2)
                                         select new Ra2VoxelCell(new(x, y, z), paletteIndex);
        return new("unit", part, palette, cells);
    }

    private static Ra2VoxelSemanticMaskComposition Composition(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelSemanticPartRole partRole,
        Ra2VoxelSemanticMaterialRole materialRole)
        => new(
            snapshot.CanonicalHash,
            Enumerable.Repeat(Assignment(partRole, materialRole), snapshot.OccupancyCount),
            new string('A', 64));

    private static Ra2VoxelSemanticEffectiveAssignment Assignment(
        Ra2VoxelSemanticPartRole partRole,
        Ra2VoxelSemanticMaterialRole materialRole)
        => new(
            "region",
            partRole,
            materialRole,
            Ra2VoxelSemanticRemapIntent.None,
            Ra2VoxelSemanticAssignmentSource.HumanOverride,
            1d,
            "test");
}
