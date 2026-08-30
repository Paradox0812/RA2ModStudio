extern alias Ra2Application;

using System.IO;
using Ra2VoxelSceneSnapshot = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSceneSnapshot;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelAcceptedCandidateKind
{
    Original = 0,
    Direct,
    Refined,
    Symmetry,
    Styled,
    ContrastStyled
}

/// <summary>
/// Immutable session authority captured by the user's explicit final-candidate action.
/// It is never serialized and is the only snapshot eligible for asset export.
/// </summary>
internal sealed record Ra2VoxelAcceptedCandidate
{
    internal Ra2VoxelAcceptedCandidate(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelAcceptedCandidateKind kind,
        string displayName,
        string suggestedFileName,
        long sessionGeneration)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("A final candidate requires a display name.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(suggestedFileName) ||
            !string.Equals(Path.GetExtension(suggestedFileName), ".vox", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(Path.GetFileName(suggestedFileName), suggestedFileName, StringComparison.Ordinal))
        {
            throw new ArgumentException("A final candidate requires a safe VOX file name.", nameof(suggestedFileName));
        }

        Kind = kind;
        DisplayName = displayName.Trim();
        SuggestedFileName = suggestedFileName;
        SessionGeneration = sessionGeneration;
    }

    internal Ra2VoxelSceneSnapshot Snapshot { get; }
    internal Ra2VoxelAcceptedCandidateKind Kind { get; }
    internal string DisplayName { get; }
    internal string SuggestedFileName { get; }
    internal long SessionGeneration { get; }
    internal string CanonicalHash => Snapshot.CanonicalHash;
}
