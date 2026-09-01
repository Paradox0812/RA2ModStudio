namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelFixedReviewView
{
    PositiveX = 0,
    NegativeX,
    PositiveY,
    NegativeY,
    PositiveXPositiveY,
    NegativeXPositiveY,
    PositiveXNegativeY,
    NegativeXNegativeY
}

internal enum Ra2VoxelNormalContextState
{
    NotAvailable = 0,
    Available,
    Stale
}

internal enum Ra2VoxelVplCompatibilityState
{
    NotEvaluated = 0
}

internal sealed record Ra2VoxelFixedViewFacts(
    Ra2VoxelFixedReviewView View,
    int ProjectedPixelCount,
    int MacroPixelCount,
    int MesoPixelCount,
    int MicroPixelCount,
    int SubPixelRiskPixelCount);

internal sealed class Ra2VoxelGameScaleReviewFacts
{
    private readonly Ra2VoxelFixedViewFacts[] _views;

    internal Ra2VoxelGameScaleReviewFacts(
        string sourceSnapshotHash,
        string candidateSnapshotHash,
        string featureScaleProjectionHash,
        IEnumerable<Ra2VoxelFixedViewFacts> views)
    {
        SourceSnapshotHash = Ra2VoxelColourContractIdentity.RequireSha256(
            sourceSnapshotHash, nameof(sourceSnapshotHash));
        CandidateSnapshotHash = Ra2VoxelColourContractIdentity.RequireSha256(
            candidateSnapshotHash, nameof(candidateSnapshotHash));
        FeatureScaleProjectionHash = Ra2VoxelColourContractIdentity.RequireSha256(
            featureScaleProjectionHash, nameof(featureScaleProjectionHash));
        _views = (views ?? throw new ArgumentNullException(nameof(views)))
            .OrderBy(value => value.View)
            .ToArray();
        if (_views.Length != Enum.GetValues<Ra2VoxelFixedReviewView>().Length ||
            _views.Select(value => value.View).Distinct().Count() != _views.Length)
            throw new ArgumentException("Game-scale review requires all eight fixed views.", nameof(views));
        MinimumProjectedPixelCount = _views.Min(value => value.ProjectedPixelCount);
        MaximumProjectedPixelCount = _views.Max(value => value.ProjectedPixelCount);
        AverageMicroSurvivalRatio = _views.Average(value => value.ProjectedPixelCount == 0
            ? 0d
            : (value.MicroPixelCount + value.SubPixelRiskPixelCount) / (double)value.ProjectedPixelCount);
        FactsHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-game-scale-review/1");
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, SourceSnapshotHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, CandidateSnapshotHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, FeatureScaleProjectionHash);
            writer.Write(_views.Length);
            foreach (Ra2VoxelFixedViewFacts view in _views)
            {
                writer.Write((int)view.View);
                writer.Write(view.ProjectedPixelCount);
                writer.Write(view.MacroPixelCount);
                writer.Write(view.MesoPixelCount);
                writer.Write(view.MicroPixelCount);
                writer.Write(view.SubPixelRiskPixelCount);
            }
        });
    }

    internal string SourceSnapshotHash { get; }
    internal string CandidateSnapshotHash { get; }
    internal string FeatureScaleProjectionHash { get; }
    internal IReadOnlyList<Ra2VoxelFixedViewFacts> Views => Array.AsReadOnly(_views);
    internal int MinimumProjectedPixelCount { get; }
    internal int MaximumProjectedPixelCount { get; }
    internal double AverageMicroSurvivalRatio { get; }
    internal string FactsHash { get; }
}

internal static class Ra2VoxelGameScaleReviewProjector
{
    internal const string Revision = "fixed-eight-view-game-scale/1";

    internal static Ra2VoxelGameScaleReviewFacts Project(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelSceneSnapshot candidate,
        Ra2VoxelFeatureScaleProjection featureScale)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(featureScale);
        if (!string.Equals(source.CanonicalHash, featureScale.SourceSnapshotHash, StringComparison.Ordinal) ||
            source.OccupancyCount != candidate.OccupancyCount ||
            source.OccupancyCount != featureScale.CellCount ||
            !source.Cells.Select(value => value.Coordinate).SequenceEqual(candidate.Cells.Select(value => value.Coordinate)))
        {
            throw new ArgumentException("Game-scale review inputs do not share the same voxel occupancy.");
        }

        List<Ra2VoxelFixedViewFacts> views = [];
        foreach (Ra2VoxelFixedReviewView view in Enum.GetValues<Ra2VoxelFixedReviewView>())
        {
            Dictionary<(int U, int Z), Ra2VoxelFeatureScale> pixels = [];
            for (int index = 0; index < source.OccupancyCount; index++)
            {
                Ra2VoxelCoordinate c = source.Cells[index].Coordinate;
                int u = view switch
                {
                    Ra2VoxelFixedReviewView.PositiveX or Ra2VoxelFixedReviewView.NegativeX => c.Y,
                    Ra2VoxelFixedReviewView.PositiveY or Ra2VoxelFixedReviewView.NegativeY => c.X,
                    Ra2VoxelFixedReviewView.PositiveXPositiveY or Ra2VoxelFixedReviewView.NegativeXNegativeY => c.X - c.Y,
                    Ra2VoxelFixedReviewView.NegativeXPositiveY or Ra2VoxelFixedReviewView.PositiveXNegativeY => c.X + c.Y,
                    _ => throw new ArgumentOutOfRangeException()
                };
                (int U, int Z) key = (u, c.Z);
                Ra2VoxelFeatureScale scale = featureScale[index];
                if (!pixels.TryGetValue(key, out Ra2VoxelFeatureScale existing) || scale < existing)
                    pixels[key] = scale;
            }
            views.Add(new(view, pixels.Count,
                pixels.Values.Count(value => value == Ra2VoxelFeatureScale.Macro),
                pixels.Values.Count(value => value == Ra2VoxelFeatureScale.Meso),
                pixels.Values.Count(value => value == Ra2VoxelFeatureScale.Micro),
                pixels.Values.Count(value => value == Ra2VoxelFeatureScale.SubPixelRisk)));
        }
        return new(source.CanonicalHash, candidate.CanonicalHash, featureScale.ProjectionHash, views);
    }

    internal static Ra2VoxelNormalContextState NormalContext(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelNormalField? normalField)
    {
        if (normalField is null) return Ra2VoxelNormalContextState.NotAvailable;
        return string.Equals(normalField.SourceSnapshotHash, source.CanonicalHash, StringComparison.Ordinal)
            ? Ra2VoxelNormalContextState.Available
            : Ra2VoxelNormalContextState.Stale;
    }
}
