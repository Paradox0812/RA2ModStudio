using System.Globalization;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelUnitClass
{
    Ground = 0,
    Air,
    LargeSurface,
    Unknown
}

internal enum Ra2VoxelUnitClassConfidenceBand
{
    High = 0,
    Medium,
    Low
}

internal enum Ra2VoxelUnitClassFactKind
{
    Geometry = 0,
    Semantic,
    Orientation
}

internal enum Ra2VoxelUnitClassConfirmationSource
{
    HumanConfirmedProposal = 0,
    HumanOverride,
    ManualWithoutAiAssessment
}

internal sealed record Ra2VoxelUnitClassFact(
    string FactId,
    Ra2VoxelUnitClassFactKind FactKind,
    string BoundedValue,
    string HostSource);

internal sealed class Ra2VoxelUnitClassEvidence
{
    private readonly Ra2VoxelUnitClassFact[] _facts;

    internal Ra2VoxelUnitClassEvidence(
        string modelIdentity,
        IEnumerable<Ra2VoxelUnitClassFact> facts)
    {
        ModelIdentity = Ra2VoxelColourContractIdentity.RequireSha256(modelIdentity, nameof(modelIdentity));
        _facts = (facts ?? throw new ArgumentNullException(nameof(facts)))
            .Select(fact => new Ra2VoxelUnitClassFact(
                Ra2VoxelColourContractIdentity.RequireIdentifier(fact.FactId, nameof(facts), 96),
                fact.FactKind,
                Ra2VoxelColourContractIdentity.RequireSingleLine(fact.BoundedValue, nameof(facts), 256),
                Ra2VoxelColourContractIdentity.RequireIdentifier(fact.HostSource, nameof(facts), 96)))
            .OrderBy(fact => fact.FactId, StringComparer.Ordinal)
            .ToArray();
        if (_facts.Length is < 1 or > 64 ||
            _facts.Select(fact => fact.FactId).Distinct(StringComparer.Ordinal).Count() != _facts.Length)
        {
            throw new ArgumentException("Unit-class evidence requires unique bounded facts.", nameof(facts));
        }
        foreach (Ra2VoxelUnitClassFact fact in _facts)
        {
            if (!Enum.IsDefined(fact.FactKind))
                throw new ArgumentException("Unit-class evidence contains an unknown fact kind.", nameof(facts));
        }
        if (!_facts.Any(fact => fact.FactKind == Ra2VoxelUnitClassFactKind.Geometry) ||
            !_facts.Any(fact => fact.FactKind == Ra2VoxelUnitClassFactKind.Semantic) ||
            !_facts.Any(fact => fact.FactKind == Ra2VoxelUnitClassFactKind.Orientation))
        {
            throw new ArgumentException("Unit-class evidence must contain geometry, semantic, and orientation facts.", nameof(facts));
        }

        GeometryFactsHash = ComputeFactsHash(Ra2VoxelUnitClassFactKind.Geometry);
        SemanticFactsHash = ComputeFactsHash(Ra2VoxelUnitClassFactKind.Semantic);
        OrientationFactsHash = ComputeFactsHash(Ra2VoxelUnitClassFactKind.Orientation);
        EvidenceHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-unit-class-evidence/1");
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, ModelIdentity);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, GeometryFactsHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, SemanticFactsHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, OrientationFactsHash);
        });
    }

    internal string ModelIdentity { get; }
    internal string GeometryFactsHash { get; }
    internal string SemanticFactsHash { get; }
    internal string OrientationFactsHash { get; }
    internal IReadOnlyList<Ra2VoxelUnitClassFact> Facts => Array.AsReadOnly(_facts);
    internal string EvidenceHash { get; }

    internal string ToPromptText()
    {
        System.Text.StringBuilder text = new();
        text.AppendLine("unit_class_evidence_schema: ra2-voxel-unit-class-evidence/1");
        text.AppendLine($"model_identity: {ModelIdentity}");
        text.AppendLine($"evidence_hash: {EvidenceHash}");
        text.AppendLine("authority: host-derived bounded facts; no raw coordinates, image pixels, palette theme, or write authority");
        foreach (Ra2VoxelUnitClassFact fact in _facts)
            text.Append("fact: ").Append(fact.FactId).Append(" kind=").Append(fact.FactKind)
                .Append(" value=").Append(fact.BoundedValue).Append(" source=").Append(fact.HostSource).AppendLine();
        return text.ToString();
    }

    private string ComputeFactsHash(Ra2VoxelUnitClassFactKind kind)
        => Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, $"ra2-voxel-unit-class-facts/{kind}/1");
            Ra2VoxelUnitClassFact[] selected = _facts.Where(fact => fact.FactKind == kind).ToArray();
            writer.Write(selected.Length);
            foreach (Ra2VoxelUnitClassFact fact in selected)
            {
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, fact.FactId);
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, fact.BoundedValue);
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, fact.HostSource);
            }
        });
}

internal static class Ra2VoxelUnitClassEvidenceBuilder
{
    internal static Ra2VoxelUnitClassEvidence Build(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelSemanticMaskComposition composition)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(composition);
        if (snapshot.OccupancyCount == 0)
            throw new ArgumentException("Unit-class evidence requires occupied geometry.", nameof(snapshot));
        if (!string.Equals(snapshot.CanonicalHash, composition.SourceSnapshotHash, StringComparison.Ordinal) ||
            snapshot.OccupancyCount != composition.CellCount)
        {
            throw new ArgumentException("Unit-class evidence inputs do not describe the same working snapshot.", nameof(composition));
        }
        if (composition.Assignments.Any(value =>
                !Enum.IsDefined(value.PartRole) || !Enum.IsDefined(value.MaterialRole) ||
                !Enum.IsDefined(value.RemapIntent) || !Enum.IsDefined(value.Source)))
        {
            throw new ArgumentException("Unit-class evidence contains an invalid semantic assignment.", nameof(composition));
        }

        string modelIdentity = ComputeGeometryIdentity(snapshot);
        double maximumCells = snapshot.Part.MaximumCellCount;
        double fillRatio = snapshot.OccupancyCount / maximumCells;
        HashSet<Ra2VoxelCoordinate> occupied = snapshot.Cells.Select(cell => cell.Coordinate).ToHashSet();
        int surfaceCount = snapshot.Cells.Count(cell => IsSurface(cell.Coordinate, occupied));
        string partRoles = JoinValues(composition.Assignments.Select(value => value.PartRole));
        string materialRoles = JoinValues(composition.Assignments.Select(value => value.MaterialRole));
        string assignmentSources = JoinValues(composition.Assignments.Select(value => value.Source));
        bool approvedRemap = composition.Assignments.Any(value => value.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved);

        Ra2VoxelUnitClassFact[] facts =
        [
            new("geometry.dimensions", Ra2VoxelUnitClassFactKind.Geometry,
                $"{snapshot.Part.XSize}x{snapshot.Part.YSize}x{snapshot.Part.ZSize}", "canonical-snapshot"),
            new("geometry.fill-ratio", Ra2VoxelUnitClassFactKind.Geometry,
                fillRatio.ToString("F4", CultureInfo.InvariantCulture), "canonical-snapshot"),
            new("geometry.surface-ratio", Ra2VoxelUnitClassFactKind.Geometry,
                (surfaceCount / (double)snapshot.OccupancyCount).ToString("F4", CultureInfo.InvariantCulture), "canonical-snapshot"),
            new("geometry.part-role", Ra2VoxelUnitClassFactKind.Geometry,
                snapshot.Part.Role.ToString(), "canonical-snapshot"),
            new("semantic.part-roles", Ra2VoxelUnitClassFactKind.Semantic, partRoles, "semantic-composition"),
            new("semantic.material-roles", Ra2VoxelUnitClassFactKind.Semantic, materialRoles, "semantic-composition"),
            new("semantic.assignment-sources", Ra2VoxelUnitClassFactKind.Semantic, assignmentSources, "semantic-composition"),
            new("semantic.approved-remap-present", Ra2VoxelUnitClassFactKind.Semantic,
                approvedRemap ? "true" : "false", "semantic-composition"),
            new("orientation.axes", Ra2VoxelUnitClassFactKind.Orientation,
                "X=left-right;Y=front-back-depth;Z=up", "canonical-coordinate-contract"),
            new("orientation.longest-axis", Ra2VoxelUnitClassFactKind.Orientation,
                LongestAxis(snapshot.Part), "canonical-snapshot"),
            new("orientation.vertical-axis", Ra2VoxelUnitClassFactKind.Orientation,
                "Z", "canonical-coordinate-contract")
        ];
        return new(modelIdentity, facts);
    }

    private static string ComputeGeometryIdentity(Ra2VoxelSceneSnapshot snapshot)
        => Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-geometry-identity/1");
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, snapshot.SceneId);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, snapshot.Part.PartId);
            writer.Write((int)snapshot.Part.Role);
            writer.Write(snapshot.Part.XSize);
            writer.Write(snapshot.Part.YSize);
            writer.Write(snapshot.Part.ZSize);
            writer.Write(snapshot.OccupancyCount);
            foreach (Ra2VoxelCell cell in snapshot.Cells.OrderBy(cell => cell.Coordinate.Z)
                         .ThenBy(cell => cell.Coordinate.Y).ThenBy(cell => cell.Coordinate.X))
            {
                writer.Write(cell.Coordinate.X);
                writer.Write(cell.Coordinate.Y);
                writer.Write(cell.Coordinate.Z);
            }
        });

    private static string JoinValues<T>(IEnumerable<T> values) where T : struct, Enum
        => string.Join(',', values.Distinct().OrderBy(value => Convert.ToInt32(value, CultureInfo.InvariantCulture)));

    private static string LongestAxis(Ra2VoxelPartDescriptor part)
    {
        (string Axis, int Size)[] values = [("X", part.XSize), ("Y", part.YSize), ("Z", part.ZSize)];
        return values.OrderByDescending(value => value.Size).ThenBy(value => value.Axis, StringComparer.Ordinal).First().Axis;
    }

    private static bool IsSurface(Ra2VoxelCoordinate coordinate, HashSet<Ra2VoxelCoordinate> occupied)
        => !occupied.Contains(new(coordinate.X - 1, coordinate.Y, coordinate.Z)) ||
           !occupied.Contains(new(coordinate.X + 1, coordinate.Y, coordinate.Z)) ||
           !occupied.Contains(new(coordinate.X, coordinate.Y - 1, coordinate.Z)) ||
           !occupied.Contains(new(coordinate.X, coordinate.Y + 1, coordinate.Z)) ||
           !occupied.Contains(new(coordinate.X, coordinate.Y, coordinate.Z - 1)) ||
           !occupied.Contains(new(coordinate.X, coordinate.Y, coordinate.Z + 1));
}

internal sealed record Ra2VoxelUnitClassProposalInput(
    Ra2VoxelUnitClass ProposedClass,
    Ra2VoxelUnitClassConfidenceBand ConfidenceBand,
    IReadOnlyList<string> EvidenceFactIds,
    string Reason,
    string ClassifierSkillId,
    string ClassifierSkillRevision,
    string ClassifierSkillContentHash,
    string EvidenceHash);

internal enum Ra2VoxelUnitClassProposalFailureKind
{
    None = 0,
    EvidenceMismatch,
    InvalidClass,
    InvalidConfidence,
    InvalidEvidenceReference,
    InvalidReason,
    ClassifierSkillMismatch,
    InvalidClassifierIdentity
}

internal sealed class Ra2VoxelUnitClassProposal
{
    internal const string RequiredClassifierSkillId = "ra2-voxel-unit-classification";

    private Ra2VoxelUnitClassProposal(Ra2VoxelUnitClassProposalInput input, IReadOnlyList<string> factIds, string proposalHash)
    {
        ProposedClass = input.ProposedClass;
        ConfidenceBand = input.ConfidenceBand;
        EvidenceFactIds = factIds;
        Reason = input.Reason;
        ClassifierSkillId = input.ClassifierSkillId;
        ClassifierSkillRevision = input.ClassifierSkillRevision;
        ClassifierSkillContentHash = input.ClassifierSkillContentHash;
        EvidenceHash = input.EvidenceHash;
        ProposalHash = proposalHash;
    }

    internal Ra2VoxelUnitClass ProposedClass { get; }
    internal Ra2VoxelUnitClassConfidenceBand ConfidenceBand { get; }
    internal IReadOnlyList<string> EvidenceFactIds { get; }
    internal string Reason { get; }
    internal string ClassifierSkillId { get; }
    internal string ClassifierSkillRevision { get; }
    internal string ClassifierSkillContentHash { get; }
    internal string EvidenceHash { get; }
    internal string ProposalHash { get; }

    internal static Ra2VoxelUnitClassProposalResult Validate(
        Ra2VoxelUnitClassEvidence evidence,
        Ra2VoxelUnitClassProposalInput input)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(input);
        if (!string.Equals(input.EvidenceHash, evidence.EvidenceHash, StringComparison.OrdinalIgnoreCase))
            return Failure(Ra2VoxelUnitClassProposalFailureKind.EvidenceMismatch, "The unit-class proposal targets stale evidence.");
        if (!Enum.IsDefined(input.ProposedClass))
            return Failure(Ra2VoxelUnitClassProposalFailureKind.InvalidClass, "The unit-class proposal contains an unknown class.");
        if (!Enum.IsDefined(input.ConfidenceBand))
            return Failure(Ra2VoxelUnitClassProposalFailureKind.InvalidConfidence, "The unit-class confidence band is invalid.");
        if (!string.Equals(input.ClassifierSkillId, RequiredClassifierSkillId, StringComparison.Ordinal))
            return Failure(Ra2VoxelUnitClassProposalFailureKind.ClassifierSkillMismatch, "The proposal was not produced with the required classifier Skill.");
        string revision;
        string skillHash;
        string reason;
        try
        {
            revision = Ra2VoxelColourContractIdentity.RequireIdentifier(input.ClassifierSkillRevision, nameof(input.ClassifierSkillRevision));
            skillHash = Ra2VoxelColourContractIdentity.RequireSha256(input.ClassifierSkillContentHash, nameof(input.ClassifierSkillContentHash));
            reason = Ra2VoxelColourContractIdentity.RequireSingleLine(input.Reason, nameof(input.Reason), 512);
        }
        catch (ArgumentException exception)
        {
            return Failure(
                exception.ParamName == nameof(input.Reason)
                    ? Ra2VoxelUnitClassProposalFailureKind.InvalidReason
                    : Ra2VoxelUnitClassProposalFailureKind.InvalidClassifierIdentity,
                "The unit-class proposal metadata is invalid.");
        }

        string[] factIds = (input.EvidenceFactIds ?? [])
            .Select(value => value?.Trim() ?? string.Empty)
            .ToArray();
        HashSet<string> available = evidence.Facts.Select(fact => fact.FactId).ToHashSet(StringComparer.Ordinal);
        if (factIds.Length is < 1 or > 32 || factIds.Any(id => !available.Contains(id)) ||
            factIds.Distinct(StringComparer.Ordinal).Count() != factIds.Length)
        {
            return Failure(Ra2VoxelUnitClassProposalFailureKind.InvalidEvidenceReference, "The proposal cites missing or duplicate evidence facts.");
        }
        factIds = factIds.OrderBy(id => id, StringComparer.Ordinal).ToArray();
        string normalizedEvidenceHash = evidence.EvidenceHash;
        Ra2VoxelUnitClassProposalInput normalized = input with
        {
            EvidenceFactIds = Array.AsReadOnly(factIds),
            Reason = reason,
            ClassifierSkillRevision = revision,
            ClassifierSkillContentHash = skillHash,
            EvidenceHash = normalizedEvidenceHash
        };
        string proposalHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-unit-class-proposal/1");
            writer.Write((int)normalized.ProposedClass);
            writer.Write((int)normalized.ConfidenceBand);
            writer.Write(factIds.Length);
            foreach (string id in factIds) Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, id);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, reason);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, RequiredClassifierSkillId);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, revision);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, skillHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, normalizedEvidenceHash);
        });
        return new(
            Ra2VoxelUnitClassProposalFailureKind.None,
            string.Empty,
            new Ra2VoxelUnitClassProposal(normalized, Array.AsReadOnly(factIds), proposalHash));
    }

    private static Ra2VoxelUnitClassProposalResult Failure(Ra2VoxelUnitClassProposalFailureKind kind, string message)
        => new(kind, message, null);
}

internal sealed record Ra2VoxelUnitClassProposalResult(
    Ra2VoxelUnitClassProposalFailureKind FailureKind,
    string Message,
    Ra2VoxelUnitClassProposal? Proposal)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelUnitClassProposalFailureKind.None && Proposal is not null;
}

internal enum Ra2VoxelUnitClassConfirmationFailureKind
{
    None = 0,
    EvidenceMismatch,
    ProposalRequired,
    ProposalNotAllowed,
    ClassDoesNotMatchSource,
    InvalidClass,
    InvalidSource
}

internal sealed class Ra2VoxelConfirmedUnitClass
{
    private Ra2VoxelConfirmedUnitClass(
        Ra2VoxelUnitClass unitClass,
        Ra2VoxelUnitClassConfirmationSource source,
        string? proposalHash,
        string evidenceHash,
        string confirmationHash)
    {
        UnitClass = unitClass;
        Source = source;
        ProposalHash = proposalHash;
        EvidenceHash = evidenceHash;
        ConfirmationHash = confirmationHash;
    }

    internal Ra2VoxelUnitClass UnitClass { get; }
    internal Ra2VoxelUnitClassConfirmationSource Source { get; }
    internal string? ProposalHash { get; }
    internal string EvidenceHash { get; }
    internal string ConfirmationHash { get; }

    internal static Ra2VoxelUnitClassConfirmationResult Create(
        Ra2VoxelUnitClassEvidence evidence,
        Ra2VoxelUnitClass selectedClass,
        Ra2VoxelUnitClassConfirmationSource source,
        Ra2VoxelUnitClassProposal? proposal)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        if (!Enum.IsDefined(selectedClass))
            return Failure(Ra2VoxelUnitClassConfirmationFailureKind.InvalidClass, "The confirmed unit class is invalid.");
        if (!Enum.IsDefined(source))
            return Failure(Ra2VoxelUnitClassConfirmationFailureKind.InvalidSource, "The confirmation source is invalid.");
        if (proposal is not null && !string.Equals(proposal.EvidenceHash, evidence.EvidenceHash, StringComparison.Ordinal))
            return Failure(Ra2VoxelUnitClassConfirmationFailureKind.EvidenceMismatch, "The unit-class proposal is stale.");
        if (source == Ra2VoxelUnitClassConfirmationSource.ManualWithoutAiAssessment && proposal is not null)
            return Failure(Ra2VoxelUnitClassConfirmationFailureKind.ProposalNotAllowed, "Manual fallback cannot claim an AI proposal.");
        if (source != Ra2VoxelUnitClassConfirmationSource.ManualWithoutAiAssessment && proposal is null)
            return Failure(Ra2VoxelUnitClassConfirmationFailureKind.ProposalRequired, "This confirmation source requires a validated proposal.");
        if (source == Ra2VoxelUnitClassConfirmationSource.HumanConfirmedProposal && selectedClass != proposal!.ProposedClass)
            return Failure(Ra2VoxelUnitClassConfirmationFailureKind.ClassDoesNotMatchSource, "A confirmed proposal must keep the proposed class.");
        if (source == Ra2VoxelUnitClassConfirmationSource.HumanOverride && selectedClass == proposal!.ProposedClass)
            return Failure(Ra2VoxelUnitClassConfirmationFailureKind.ClassDoesNotMatchSource, "A human override must select a different class.");

        string? proposalHash = proposal?.ProposalHash;
        string confirmationHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-unit-class-confirmation/1");
            writer.Write((int)selectedClass);
            writer.Write((int)source);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, proposalHash ?? string.Empty);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, evidence.EvidenceHash);
        });
        return new(
            Ra2VoxelUnitClassConfirmationFailureKind.None,
            string.Empty,
            new Ra2VoxelConfirmedUnitClass(selectedClass, source, proposalHash, evidence.EvidenceHash, confirmationHash));
    }

    private static Ra2VoxelUnitClassConfirmationResult Failure(Ra2VoxelUnitClassConfirmationFailureKind kind, string message)
        => new(kind, message, null);
}

internal sealed record Ra2VoxelUnitClassConfirmationResult(
    Ra2VoxelUnitClassConfirmationFailureKind FailureKind,
    string Message,
    Ra2VoxelConfirmedUnitClass? Confirmation)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelUnitClassConfirmationFailureKind.None && Confirmation is not null;
}
